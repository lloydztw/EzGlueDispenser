using Eazy_Project_Interface;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;



namespace Eazy_Project_III.ProcessSpace
{
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
    /// 像測補償抽象類別<br/>
    /// --------------------------------------------
    /// @LETIAN: 20220624 refactor
    /// </summary>
    public abstract class MirrorAbsImageProcess : BaseProcess
    {
        protected const string IMAGE_SAVE_PATH = @"D:\EVENTLOG\Nlogs\images";
        protected static int MOTOR_TIMEOUT_WAIT_COUNT = 5000;
        protected static int MOTOR_CMD_DELAY = 10;
        protected static int MAX_RUN_COUNT = int.MaxValue;

        #region ACCESS_TO_OTHER_PROCESSES
        protected BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion

        public event EventHandler<ProcessEventArgs> OnLiveImage;
        public event EventHandler<CompensatingEventArgs> OnLiveCompensating;

        protected MirrorAbsImageProcess()
        {
            init_dirs();
        }
        public override void Stop()
        {
            base.Stop();
            stop_scan_thread();
        }
        public void Terminate()
        {
            m_mainprocess.Stop();
            this.Stop();
            ax_set_motor_speed(SpeedTypeEnum.GO);
        }


        #region PRIVATE_THREAD_FUNCTIONS
        private Thread _thread = null;
        private bool _runFlag = false;
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
                try
                {
                    if (!_thread.Join(timeout))
                        _thread.Abort();
                    _thread = null;
                }
                catch (Exception ex)
                {
                    GdxGlobal.LOG.Warn(ex, "無法終止 Thread!");
                }
            }
        }
        private void thread_func(object arg)
        {
            var phase = (XRunContext)arg;

            while (_runFlag)
            {
                try
                {
                    Thread.Sleep(1);

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
        }
        #endregion

        #region PROTECTED_DATA
        protected int[] m_showIDs = new int[] { 3, 4, 5 };
        #endregion

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
            var e = new ProcessEventArgs("live.image", bmp);
            OnLiveImage?.Invoke(this, e);
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
        protected static QVector MAX_DELTA = new QVector(0.25, 0.25, 0.25, 0.25, 2.0, 2.0);
        protected static QVector MIN_DELTA = new QVector(AxisUnitConvert.PERCISIONS);
        protected static QVector COMP_STEP = new QVector(AxisUnitConvert.PERCISIONS) * 5;
        //protected static double STEP_XYZU = MAX_DELTA[0] * 0.2;
        //protected static double STEP_A = Math.Round(AxisUnitConvert.PERCISIONS[5] * 5, 4);

        volatile int m_motorWaitCount = 0;
        IAxis[] _blackboxMotors = null;
        protected IAxis[] BlackBoxMotors
        {
            get
            {
                if (_blackboxMotors == null)
                {
                    // X, Y, Z, U, theta_y, theta_z
                    var indexes = COMP_MOTORS_IDS;
                    _blackboxMotors = new IAxis[indexes.Length];
                    int i = 0;
                    foreach (int idx in indexes)
                        _blackboxMotors[i++] = GetAxis(idx);
                }
                return _blackboxMotors;
            }
        }

#if(false)
        protected QVector ax_convert_to_physical_unit(QVector pos)
        {
            pos[4] = pos[4];
            return pos;
        }
        protected QVector ax_convert_to_plc_unit(QVector pos)
        {
            var u = AxisUnitConvert.ToAxis(pos);
            return u;
        }
#endif

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
                    //BlackBoxMotors[i].Go(plcPos[i], 0);
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
            return m_motorWaitCount > MOTOR_TIMEOUT_WAIT_COUNT;
        }
        protected bool ax_is_ready()
        {
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
        }
        protected bool ax_is_error()
        {
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (BlackBoxMotors[i].IsError)
                    return true;
            }
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
            public int ExitCode;
            public QVector InitMotorPos;
            public Action<XRunContext> StepFunc;
            public void Reset()
            {
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
            if (ax_is_error())
            {
                _LOG(runCtrl.Name, "馬達異常", Color.Red);
                isError = true;
                return (runCtrl.Go = false);
            }

            bool isReady = ax_is_ready();

            if (!isReady)
            {
                if (ax_is_wait_ready_timeout())
                {
                    _LOG(runCtrl.Name, "馬達等不到 Ready", Color.Red);
                    isError = true;
                    return (runCtrl.Go = false);
                }
            }

            isError = false;
            return isReady;
        }
        protected bool check_max_run_count(XRunContext runCtrl)
        {
            int count = Interlocked.Increment(ref runCtrl.RunCount);
            if (count > MAX_RUN_COUNT)
            {
                _LOG(runCtrl.Name, "補償次數超過上限", MAX_RUN_COUNT, Color.Red);
                SetNextState(9999);
                return runCtrl.Go = false;
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
                return ax_is_ready();
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
        protected bool _is_zero(int[] motorParams)
        {
            bool yes = true;
            for (int i = 0; i < motorParams.Length; i++)
                yes &= (motorParams[i] == 0);
            return yes;
        }


        /// <summary>
        /// The caller must maintain the life-cycle of the return Bitmap.
        /// </summary>
        protected Bitmap snapshot_image(ICam cam, int runCount)
        {
            try
            {
                cam.Snap();
                using (Bitmap bmp0 = cam.GetSnap())
                {
                    Bitmap bmp = new Bitmap(bmp0);
                    #region ASYNC_DUMP_IMAGE
                    var dump_func = new Action<Bitmap, int>((bmp2, i) =>
                    {
                        string fileName = string.Format("image_{0}_{1:yyyyMMdd_HHmmss}.jpg", Name, DateTime.Now);
                        fileName = System.IO.Path.Combine(IMAGE_SAVE_PATH, fileName);
                        bmp2.Save(fileName, ImageFormat.Bmp);
                        bmp2.Dispose();
                    });
                    dump_func.BeginInvoke((Bitmap)bmp.Clone(), runCount, null, null);
                    #endregion
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                _LOG(ex, "相機異常!");
                return new Bitmap(1, 1);
            }
        }
    }
}
