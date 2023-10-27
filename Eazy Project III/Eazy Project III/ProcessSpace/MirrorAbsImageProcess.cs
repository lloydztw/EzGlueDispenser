using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;



namespace Eazy_Project_III.ProcessSpace
{
    #region S3_PROCESS_EVENT_ARGS

    /// <summary>
    /// 補償執行中的事件參數 (for 單步調試 使用)
    /// </summary>
    public class CompensatingEventArgs : ProcessEventArgs
    {
        public string PhaseName;
        public QVector FinalTarget;
        public QVector InitPos;
        public QVector CurrentPos;
        public QVector Delta;
        public QVector MaxDelta;
        public bool ContinueToDebug;
        public int[] ShowIDs = null;
    }

    /// <summary>
    /// 補償運算後事件參數 
    /// </summary>
    public class CompensatedInfoEventArgs : ProcessEventArgs
    {
        /// <summary>
        /// CenterComp or ProjectionComp <br/>
        /// (02中心補償 或 03光斑補償)
        /// </summary>
        public string Name;
        /// <summary>
        /// MirrorIndex
        /// </summary>
        public int MirrorIndex;
        /// <summary>
        /// Sender 負責 life cycle
        /// </summary>
        public Bitmap Image
        {
            get { return (Bitmap)Tag; }
            set { Tag = value; }
        }
        /// <summary>
        /// 中光電補償運算之總合結果 (含有許多圖標)
        /// </summary>
        public CoreCompInfo Info;

        public CompensatedInfoEventArgs(string name, int mirrorIndex, Bitmap bmp, CoreCompInfo info)
        {
            this.Name = name;
            this.MirrorIndex = mirrorIndex;
            this.Image = bmp;
            //this.Rects = new GdxCoreRect[rects.Length];
            //Array.Copy(rects, this.Rects, rects.Length);
            this.Info = info;
        }
    }

    /// <summary>
    /// 定位點設定 之 事件參數
    /// </summary>
    public class CoreMarkPointEventArgs : ProcessEventArgs
    {
        public string Name;
        /// <summary>
        /// Sender 負責 life cycle
        /// </summary>
        public Bitmap Image
        {
            get { return (Bitmap)Tag; }
            set { Tag = value; }
        }
        public Point[] GoldenPts;
        public Point[] AlgoPts;

        public CoreMarkPointEventArgs(string name, Bitmap bmp, Point[] goldenPts, Point[] algoPts)
        {
            Name = name;
            Image = bmp;
            GoldenPts = new Point[goldenPts.Length];
            Array.Copy(goldenPts, GoldenPts, GoldenPts.Length);
            AlgoPts = new Point[algoPts.Length];
            Array.Copy(algoPts, AlgoPts, AlgoPts.Length);
        }
    }

    #endregion



    /// <summary>
    /// 像測補償抽象類別<br/>
    /// @LETIAN: 20220624 refactor
    /// </summary>
    public abstract class MirrorAbsImageProcess : BaseProcess
    {
        #region CONFIGURATION_組態設定
        protected static string IMAGE_SAVE_PATH { get { return Universal.LOG_IMG_PATH; } }
        protected static int MOTOR_TIMEOUT_WAIT_COUNT = 50000;
        protected static int MOTOR_CMD_DELAY = 300;
        #endregion


        #region ACCESS_TO_OTHER_PROCESSES
        protected BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion


        public event EventHandler<ProcessEventArgs> OnLiveImage;
        public event EventHandler<CompensatingEventArgs> OnLiveCompensating;
        public event EventHandler<CompensatedInfoEventArgs> OnCompensatedInfo;
        public event EventHandler<CoreMarkPointEventArgs> OnMarkPointInfo;


        protected MirrorAbsImageProcess()
        {
            init_dirs();
        }
        public override void Start(params object[] args)
        {
            //>>> _initDelayAndTimeout();
            base.Start(args);
            _initDelayAndTimeout();
        }
        public override void Stop()
        {
            base.Stop();
            stop_scan_thread();
        }
        public void Terminate()
        {
            Set_Cooling_Module(false);
            m_mainprocess.Stop();
            this.Stop();
            ax_set_motor_speed(SpeedTypeEnum.GO);

            // 利用 OnNG event 讓 GUI 顯示 報警視窗.
            if (LastNG != null)
                FireNG(LastNG);
        }


        #region PRIVATE_THREAD_FUNCTIONS
        private Thread _thread = null;
        private bool _runFlag = false;
        private bool _isThreadStopping = false;

        protected bool is_thread_running()
        {
            return _runFlag || _thread != null;
        }
        protected void start_scan_thread(XRunContext phase)
        {
            if (!is_thread_running())
            {
                _runFlag = true;
                _thread = new Thread(thread_func);
                _thread.Name = this.Name;
                _thread.Start(phase);
            }
            else
            {
                GdxGlobal.LOG.Warn("有 Thread 尚未結束");
            }
        }
        protected void stop_scan_thread(int timeout = 3000)
        {
            if (is_thread_running())
            {
                _runFlag = false;
                var stopFunc = new Action<int>((tmout) =>
                {
                    if (!_isThreadStopping)
                    {
                        _isThreadStopping = true;
                        try
                        {
                            var t = _thread;
                            if (t != null)
                            {
                                if (!t.Join(tmout))
                                    t.Abort();
                                _thread = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            GdxGlobal.LOG.Warn(ex, "Thread 終止異常!");
                        }
                        _isThreadStopping = false;
                    }
                });
                stopFunc.BeginInvoke(timeout, null, null);
            }
        }
        private void thread_func(object arg)
        {
            var phase = (XRunContext)arg;

            while (_runFlag)
            {
                try
                {
                    //>>> 確保 PLC 有效 scanned 出現 2次 以上
                    if (!IsValidPlcScanned(2))
                    {
                        Thread.Sleep(2);
                        continue;
                    }

                    phase.StepFunc(phase);

                    if (!phase.Go)
                        break;

                    if (phase.IsCompleted)
                    {
                        if (phase.RunCount == 0)
                            _LOG(phase.Name, "補償 = 0");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (_runFlag)
                    {
                        try
                        {
                            _LOG(ex, "live compensating 異常!");
                            SetNextState(9999);
                        }
                        catch
                        {
                        }
                    }
                    break;
                }
            }

            _runFlag = false;
            _thread = null;

            //// if (phase == m_phase1)
            //// {
            ////    SetNextState(1000);
            //// }
            //// if (phase == m_phase2)
            //// {
            ////    SetNextState(2000);
            //// }
            //// if (phase == m_phase3)
            //// {
            ////    SetNextState(3000);
            //// }

            int nextState = phase.ExitCode;
            SetNextState(nextState);
            base.IsOn = true;
        }
        #endregion


        #region PROTECTED_DATA
        protected int[] m_showIDs = new int[] { 3, 4, 5 };  //<<< RESERVED
        string m_imageDumpStemName = "image";
        #endregion


        #region PROTECTED_EVENT_LAUNCHERS
        void init_dirs()
        {
            if (!System.IO.Directory.Exists(IMAGE_SAVE_PATH))
            {
                System.IO.Directory.CreateDirectory(IMAGE_SAVE_PATH);
                //string subFolder = System.IO.Path.Combine(IMAGE_SAVE_PATH, Name);
                //if (!System.IO.Directory.Exists(subFolder))
                //    System.IO.Directory.CreateDirectory(subFolder);
            }
        }
        protected void FireLiveImaging(Bitmap bmp)
        {
            try
            {
                OnLiveImage?.Invoke(this, new ProcessEventArgs("live.image", bmp));
            }
            catch (Exception ex)
            {
                _LOG(ex, "FireLiveImaging 異常!");
            }
        }
        protected void FireCompensationStart()
        {
            FireMessage("Comp.Start");
        }
        protected bool FireCompensating(XRunContext phase, QVector target, QVector cur, QVector delta, out bool isMotorPosChangedByClient)
        {
            ////// isMotorPosChangedByClient = false;
            ////// return phase.Go;

            var e = new CompensatingEventArgs()
            {
                PhaseName = phase.Name,
                InitPos = new QVector(CompensationInitPos),
                FinalTarget = target != null ? new QVector(target) : null,
                CurrentPos = new QVector(cur),
                Delta = new QVector(delta),
                MaxDelta = new QVector(MAX_DELTA),
                ContinueToDebug = true,
                ShowIDs = m_showIDs
            };

            OnLiveCompensating?.Invoke(this, e);
            if (e.GoControlByClient != null)
                e.GoControlByClient.WaitOne();

            if (e.Cancel)
            {
                phase.Go = false;
            }

            if (!e.ContinueToDebug)
            {
                phase.IsDebugMode = false;
            }

            // 檢查馬達位置是否被 user 移動
            var cur2 = ax_read_current_pos();
            isMotorPosChangedByClient = !QVector.AreEqual(cur, cur2, 4);
            if (isMotorPosChangedByClient)
            {
                _LOG("dCur", cur2 - cur, Color.Red);
                isMotorPosChangedByClient = false;
            }

            return phase.Go;
        }
        protected void FireCompensatedInfo(string name, int mirrorIdx, Bitmap bmp, CoreCompInfo info)
        {
            try
            {
                if (OnCompensatedInfo != null)
                {
                    var e = (name == null) ? null : new CompensatedInfoEventArgs(name, mirrorIdx, bmp, info);
                    OnCompensatedInfo(this, e);
                }
                else
                {
                    FireLiveImaging(bmp);
                }
            }
            catch (Exception ex)
            {
                _LOG(ex, "FireCompensatedInfo 異常!");
            }
        }
        protected void FireMarkPointInfo(string name, Bitmap bmp, Point[] goldenPts, Point[] algoPts)
        {
            try
            {
                if (OnMarkPointInfo != null)
                {
                    var e = (name == null) ? null : new CoreMarkPointEventArgs(name, bmp, goldenPts, algoPts);
                    OnMarkPointInfo(this, e);
                }
                else
                {
                    FireLiveImaging(bmp);
                }
            }
            catch (Exception ex)
            {
                _LOG(ex, "FireCompensatedInfo 異常!");
            }
        }
        #endregion


        #region PROTECTED_補償設定_與_馬達控制

        /// <summary>
        /// 補償控制, 所有可能用到的馬達軸編號 <br/>
        /// X, Y, Z, U, θy, θz
        /// </summary>
        protected readonly static int[] COMP_MOTORS_IDS = new int[] { 0, 1, 2, 6, 7, 8 };
        protected readonly static int N_MOTORS = COMP_MOTORS_IDS.Length;

        /// <summary>
        /// Axis 單位尺度轉換 for X, Y, Z, U, θy, θz <br/>
        /// 物理世界數學運算的 單位尺度 必須是 ( 1mm, 1mm, 1mm, 1mm, 1deg, 1deg ) <br/>
        /// Gaara IAxis 單位尺度 是 ( 1mm, 1mm, 1mm, 0.001 mm, 0.0167 deg, 0.0167 deg ) <br/>
        /// </summary>
        public class AxisUnitConvert
        {
            public readonly static double[] AXIS_TO_WORLD = new double[]
            {
                1, 1, 1, 0.001, 0.0167, 0.0167
            };
            public readonly static double[] PERCISIONS = new double[]
            {
                0.01, 0.01, 0.01, 0.001, 0.0167, 0.0167
            };
            public static QVector Round(QVector world, bool inplace = false)
            {
                var u = inplace ? world : new QVector(world);
                for (int i = 0; i < N_MOTORS; i++)
                {
                    // 搭配 PLCMotionClass 實作
                    // 中轉 float 可能會有誤差
                    int N = (int)Math.Round(world[i] / PERCISIONS[i]);
                    u[i] = (double)Math.Round((double)PERCISIONS[i] * N, 4);
                }
                return u;
            }
            public static QVector ToAxis(QVector world, bool inplace = false)
            {
                var u = inplace ? world : new QVector(world);
                for (int i = 0; i < N_MOTORS; i++)
                {
                    double value = world[i] / AXIS_TO_WORLD[i];
                    value = Math.Round(value, 4);
                    u[i] = value;
                }
                return u;
            }
            public static QVector ToWorld(QVector v, bool inplace = false)
            {
                var u = inplace ? v : new QVector(v);
                for (int i = 0; i < N_MOTORS; i++)
                {
                    //int N = STEPS_PER_PHYSIC_UNIT[i];
                    //float value = ((float)((int)v[i]) / N);
                    //u[i] = value;
                    double value = v[i] * AXIS_TO_WORLD[i];
                    value = Math.Round(value, 4);
                    u[i] = value;
                }
                return u;
            }
            public static bool IsSmallVector(QVector incr, int steps = 2)
            {
                bool yes = true;
                for (int i = 0; i < N_MOTORS; i++)
                {
                    //int N = STEPS_PER_PHYSIC_UNIT[i];
                    //int N = (int)(1 / (float)PHYSIC_PER_STEP[i]);
                    //int p = (int)((float)incr[i] * N);
                    //yes &= (Math.Abs(p) < steps);
                    if (Math.Abs(incr[i]) >= PERCISIONS[i] * steps)
                        return false;
                }
                return yes;
            }
        }
        protected QVector MAX_DELTA = new QVector(0.5, 0.5, 0.5, 0.5, 2.0, 2.0);
        protected QVector MIN_DELTA = new QVector(AxisUnitConvert.PERCISIONS);
        protected QVector COMP_STEP = new QVector(AxisUnitConvert.PERCISIONS) * 5;

        protected void InitCompensationSteps()
        {
            //>>> _initDelayAndTimeout();

            double totalMaxA = Math.Round((double)RecipeCHClass.Instance.CompStepAngleTotalMax, 4);
            double totalMaxU = Math.Round(RecipeCHClass.Instance.CompStepSizeTotalMax * 0.001, 4);
            MAX_DELTA = new QVector(totalMaxU, totalMaxU, totalMaxU, totalMaxU, totalMaxA, totalMaxA);
            _LOG("累積最大補償量", MAX_DELTA);

            double percA = AxisUnitConvert.PERCISIONS[5];
            double stepMaxA = Math.Round(RecipeCHClass.Instance.CompStepAngleMax * percA, 4);
            double stepMaxU = Math.Round(RecipeCHClass.Instance.CompStepSize * 0.001, 4);
            COMP_STEP = new QVector(stepMaxU, stepMaxU, stepMaxU, stepMaxU, stepMaxA, stepMaxA);
            _LOG("單步最大補償量", COMP_STEP);
        }
        private void _initDelayAndTimeout()
        {
            //@LETIAN: 2022/10/22 啟用
            //base.InitDefaultDelay();

            MOTOR_CMD_DELAY = RecipeCHClass.Instance.CompMotorDelay;
            int moTimeout = RecipeCHClass.Instance.CompMotorTimeout * 1000;
            _LOG("補償馬達 Delay(ms)", MOTOR_CMD_DELAY, "Timeout(ms)", moTimeout);

            int i = 0;
            foreach (var motor in BlackBoxMotors)
            {
                motor.Timeout = moTimeout;
                motor.InPosPercision = AxisUnitConvert.PERCISIONS[i] / 2;
                i++;
            }

#if (OPT_SIM && false)
            // 加快模擬速度
            MOTOR_CMD_DELAY = 10;
#endif
        }

        #region PRIVATE_MOTOR_DATA
        volatile int m_motorWaitCount = 0;
        private EzAxis[] _blackboxMotors = null;
        #endregion

        internal EzAxis[] BlackBoxMotors
        {
            get
            {
                if (_blackboxMotors == null)
                {
                    // X, Y, Z, U, theta_y, theta_z
                    var indexes = COMP_MOTORS_IDS;
                    _blackboxMotors = new EzAxis[indexes.Length];
                    int i = 0;
                    foreach (int idx in indexes)
                    {
                        _blackboxMotors[i++] = new EzAxis(GetAxis(idx), idx);
                    }
                }
                return _blackboxMotors;
            }
        }

        /// <summary>
        /// X, Y, Z, U, θy, θz
        /// </summary>
        protected QVector ax_read_current_pos()
        {
            var pos = new QVector(N_MOTORS);
            for (int i = 0; i < N_MOTORS; i++)
                pos[i] = BlackBoxMotors[i].GetPos();
            //> return ax_convert_to_physical_unit(pos);
            return AxisUnitConvert.ToWorld(pos, true);
        }
        protected void ax_start_move(QVector pos)
        {
            //var plcPos = ax_convert_to_plc_unit(pos);
            var ax_pos = AxisUnitConvert.ToAxis(pos);

            if (GdxGlobal.Facade.IsSimMotor())
            {
                for (int i = 0; i < N_MOTORS; i++)
                {
                    //>>> BlackBoxMotors[i].Go(plcPos[i], 0);
                    int axisID = COMP_MOTORS_IDS[i];
                    GdxGlobal.IO.sim_axis_to_pos(axisID, ax_pos[i]);
                }
            }
            else
            {
                for (int i = 0; i < N_MOTORS; i++)
                {
                    BlackBoxMotors[i].Go(ax_pos[i], 0);
                }
            }

            InvalidatePlcScanned();
            Thread.Sleep(MOTOR_CMD_DELAY);
        }
        protected void ax_set_motor_speed(SpeedTypeEnum mode)
        {
            for (int i = 0; i < N_MOTORS; i++)
            {
                var pmotor = (PLCMotionClass)BlackBoxMotors[i];
                pmotor.SetSpeed(mode);
            }
        }
        protected bool ax_is_wait_ready_timeout()
        {
#if (OPT_OLD_MOTOR_READY)
            return m_motorWaitCount > MOTOR_TIMEOUT_WAIT_COUNT;
#else
            foreach (var motor in BlackBoxMotors)
            {
                if (motor.IsTimeout())
                {
                    _LOG($"馬達 [{motor.ID} 軸] Timeout!", Color.Red);
                    return true;
                }
            }
            return false;
#endif
        }
        protected bool ax_is_ready(out int errAxisID)
        {
#if (OPT_OLD_MOTOR_READY)
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (!BlackBoxMotors[i].IsOK)
                {
                    Interlocked.Increment(ref m_motorWaitCount);
                    return false;
                }
            }
            Interlocked.Exchange(ref m_motorWaitCount, 0);
            return true;
#else
            foreach (var motor in BlackBoxMotors)
            {
                if (!motor.SmartCheckReady())
                {
                    errAxisID = motor.ID;
                    return false;
                }
            }

            errAxisID = -1;
            return true;
#endif
        }
        protected bool ax_is_error(out int errAxisID)
        {
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (BlackBoxMotors[i].IsError)
                {
                    errAxisID = BlackBoxMotors[i].ID;
                    return true;
                }
            }
            errAxisID = -1;
            return false;
        }

        /// <summary>
        /// 初始位置 (開始補償前一刻的馬達初始位置.)
        /// </summary>
        protected abstract QVector CompensationInitPos { get; }

        protected class XRunContext
        {
            public XRunContext(string name, int exitCode)
            {
                Name = name;
                ExitCode = exitCode;
                Reset();
            }
            public string Name;
            public int RunCount;
            public bool Go;
            public bool IsCompleted;
            public bool IsDebugMode;
            public bool IsUserStop;
            public int ExitCode;
            public QVector InitMotorPos;
            public Action<XRunContext> StepFunc;
            public void Reset()
            {
                IsUserStop = false;
                IsCompleted = false;
                RunCount = 0;
                Go = true;
            }
        }

        /// <summary>
        /// 檢查 motor 狀態是否 ready, 
        /// 有異常時會將 runCtrl.Go = false
        /// </summary>
        /// <param name="runCtrl"></param>
        /// <returns>go/no go</returns>
        protected bool check_motor_ready(XRunContext runCtrl, out bool isError)
        {
            int errAxisID;
            if (ax_is_error(out errAxisID))
            {
                if (!runCtrl.IsUserStop)
                {
                    _LOG(runCtrl.Name, LastNG = $"馬達 [index={errAxisID}軸] 異常", Color.Red);
                    isError = true;
                }
                else
                {
                    isError = false;
                }
                return (runCtrl.Go = false);
            }

            bool isReady = ax_is_ready(out errAxisID);

            if (!isReady)
            {
                if (ax_is_wait_ready_timeout())
                {
                    _LOG(runCtrl.Name, LastNG = $"馬達 [{errAxisID}軸] 等不到 Ready!", Color.Red);
                    isError = true;
                    return (runCtrl.Go = false);
                }
            }

            isError = false;
            return isReady;
        }
        protected bool check_max_run_count(XRunContext runCtrl, int maxRunCount = 0)
        {
            if (maxRunCount <= 0)
                maxRunCount = int.MaxValue;

            int count = Interlocked.Increment(ref runCtrl.RunCount);

            if (count > maxRunCount)
            {
                string errMaxRunCountMsg = $"{runCtrl.Name}, 補償次數超過上限 {maxRunCount} !";
                _LOG(errMaxRunCountMsg, Color.Red);

                // Events
                FireNG(errMaxRunCountMsg);  //<<< 補償次數超過上限

                // Soft-Terminate
                SetNextState(9999);

                // go flags 
                runCtrl.Go = false;
                return false;
            }

            return runCtrl.Go;
        }
        protected bool check_debug_mode(XRunContext runCtrl, QVector targetMotorPos, QVector curMotorPos, QVector incr)
        {
            if (!runCtrl.IsDebugMode)
                return runCtrl.Go;

            bool go = FireCompensating(runCtrl, targetMotorPos, curMotorPos, incr, out bool isDirty);

            if (!go)
            {
                _LOG(runCtrl.Name, "調試", "中止補償!", Color.Red);
                SetNextState(9999);
                runCtrl.IsCompleted = false;
                return (runCtrl.Go = false);
            }

            if (isDirty)
            {
                //// Repeat next run by thread_func
                //// _LOG("馬達位置已被改動, 重新計算...");
                //// return true;                
                _LOG(runCtrl.Name, "調試", "馬達位置已被改動, 為安全起見, 強制中止補償!", Color.Red);
                runCtrl.IsCompleted = false;
                return (runCtrl.Go = false);
            }

            return runCtrl.Go;
        }
        protected bool check_completed(XRunContext runCtrl)
        {
            if (runCtrl.IsCompleted)
            {
                return ax_is_ready(out int errAxisID);
            }
            return false;
        }
        protected void log_motor_command(XRunContext runCtrl, QVector dstMotorPos, QVector incr)
        {
            string tag = string.Format("[{0}]", runCtrl.RunCount);
            //_LOG(runCtrl.Name, tag, "啟自", dstMotorPos - incr);
            //_LOG(runCtrl.Name, tag, "增減", incr);
            _LOG(runCtrl.Name, tag, "移到", dstMotorPos);
        }

        protected bool _clip_into_safe_box(QVector pos)
        {
            var initPos = CompensationInitPos;

            bool clip = false;
            for (int i = 0; i < N_MOTORS; i++)
            {
                double min = initPos[i] - MAX_DELTA[i];
                double max = initPos[i] + MAX_DELTA[i];
                if (pos[i] < min)
                {
                    pos[i] = min;
                    clip = true;
                }
                if (pos[i] > max)
                {
                    pos[i] = max;
                    clip = true;
                }
            }
            return clip;
        }
        protected bool _is_almost_zero(QVector delta)
        {
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (Math.Abs(delta[i]) >= MIN_DELTA[i])
                    return false;
            }
            return true;
        }
        protected bool _is_completed(bool[] coreCompFlags, int[] motorParams)
        {
            bool yes = true;
            for (int i = 0; i < coreCompFlags.Length; i++)
            {
                bool completed = coreCompFlags[i] || (motorParams[i] == 0);
                yes &= completed;
            }
            return yes;
        }

        #endregion


        #region 影像擷取與存檔

        public string ImageDumpStemName
        {
            get
            {
                return m_imageDumpStemName;
            }
            set
            {
                m_imageDumpStemName = value;
            }
        }

        /// <summary>
        /// The caller must maintain the life-cycle of the return Bitmap.
        /// </summary>
        protected Bitmap snapshot_image(ICam cam, string tag = null, bool dump = true)
        {
            try
            {
                cam.Snap();
                Bitmap bmp = cam.GetSnap();

                // snap 兩次 (temporally trial)
                if (false)
                {
                    bmp.Dispose();
                    Thread.Sleep(100);
                    cam.Snap();
                    bmp = cam.GetSnap();
                }

                if (dump)
                {
                    #region ASYNC_DUMP_IMAGE
                    var dump_func = new Action<Bitmap, string>((bmp2, t) =>
                    {
                        //>>> string path = System.IO.Path.Combine(IMAGE_SAVE_PATH, DateTime.Now.ToString("yyyyMMdd"));
                        string path = System.IO.Path.Combine(IMAGE_SAVE_PATH, "_tmp");

                        if (!System.IO.Directory.Exists(path))
                            System.IO.Directory.CreateDirectory(path);

                        string fileName = tag != null ?
                            string.Format("{0}_{1}_{2:yyyyMMdd_HHmmss}.png", m_imageDumpStemName, tag, DateTime.Now) :
                            string.Format("{0}_{1:yyyyMMdd_HHmmss}.png", m_imageDumpStemName, DateTime.Now);

                        fileName = System.IO.Path.Combine(path, fileName);

                        bmp2.Save(fileName, ImageFormat.Png);
                        bmp2.Dispose();
                    });
                    dump_func.BeginInvoke((Bitmap)bmp.Clone(), tag, null, null);
                    #endregion
                }

                return bmp;
            }
            catch (Exception ex)
            {
                _LOG(ex, "相機異常!");
                return new Bitmap(1, 1);
            }
        }

        #endregion
    }
}
