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
    /// <summary>
    /// 投影補償 (Projection Compensate) 流程 <br/>
    /// --------------------------------------------
    /// @LETIAN: 20220619 開始實作
    /// </summary>
    public class MirrorBlackboxProcess : BaseProcess
    {
        const string IMAGE_SAVE_PATH = @"D:\EVENTLOG\Nlogs\images";
        const int MAX_RUN_COUNT = 10;

        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion

        #region PRIVATE_DATA
        int _mirrorIndex = 0;
        #endregion

        #region SINGLETON
        static MirrorBlackboxProcess _singleton = null;
        private MirrorBlackboxProcess()
        {
            init_dirs();
        }
        #endregion

        public static MirrorBlackboxProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MirrorBlackboxProcess();
                return _singleton;
            }
        }
        public event EventHandler<ProcessEventArgs> OnLiveImage;
        public event EventHandler<ProcessEventArgs> OnLiveCompensating;
        public override string Name
        {
            get { return "BlackBox"; }
        }

        /// <summary>
        /// 啟動 <br/>
        /// args[0]: mirrorIndex
        /// </summary>
        /// <param name="args">args[0]: mirrorIndex</param>
        public override void Start(params object[] args)
        {
            // (1) 可以直接嘗試將 args[0] 轉型
            //      try { Mirror_CalibrateProcessIndex = (int)args[0]; }
            //      catch { }
            // (2) 目前為了相容 Tick() 舊碼 ,
            //      暫時透過 base (ProcessClass.RelateString) 傳遞
            //      (a little awkwardly)
            base.Start(args[0]);
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
        }

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:
                        if (is_thread_running())
                        {
                            _LOG("Thread 未清除啟動", Color.Red);
                            Terminate();
                            return;
                        }
                        else if (!int.TryParse(Process.RelateString, out _mirrorIndex) || _mirrorIndex >= 2)
                        {
                            _LOG("未定义Mirror的值停止流程", Color.Red);
                            Terminate();
                        }
                        else
                        {
                            _LOG("Start", "Mirror", _mirrorIndex);
                            set_light(true);
                            SetNextState(10);
                        }
                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            int expo = RecipeCHClass.Instance.JudgeCamExpo;
                            _LOG("設定曝光時間", expo);
                            var cam = ICamForBlackBox;
                            cam.SetExposure(expo);
                            SetNextState(20);
                        }
                        break;

                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!ax_is_ready())
                            {
                                _LOG("馬達沒有Ready", Color.OrangeRed);
                                //Terminate();
                            }
                            else
                            {
                                _LOG("開始連續取像補償");
                                cx_init_compensation();
                                start_scan_thread();
                                SetNextState(40, 500);
                            }
                        }
                        break;

                    case 40:
                        if (Process.IsTimeup)
                        {
                            // THREADING
                        }
                        break;

                    case 4001:
                        if(Process.IsTimeup)
                        {
                            if (cx_is_last_step_completed())
                            {
                                _LOG("補償完成");
                                Stop();
                                FireCompleted();
                            }
                            if (ax_is_error())
                            {
                                _LOG("碼達異常!", Color.Red);
                            }
                        }
                        break;

                    case 9999:
                        if (Process.IsTimeup)
                        {
                            _LOG("補償中止!", Color.Purple);
                            set_light(false);
                            Terminate();
                            break;
                        }
                        break;
                }
            }
        }


        #region PRIVATE_THREAD_FUNCTIONS
        Thread _thread = null;
        bool _runFlag = false;
        bool is_thread_running()
        {
            return _runFlag || _thread != null;
        }
        void start_scan_thread()
        {
            if (!is_thread_running())
            {
                _runFlag = true;
                _thread = new Thread(thread_func);
                _thread.Start();
            }
            else
            {
                GdxGlobal.LOG.Warn("有 Thread 尚未結束");
            }
        }
        void stop_scan_thread(int timeout = 3000)
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
        void thread_func()
        {
            int runCount = 0;

            while (_runFlag)
            {
                try
                {
                    Thread.Sleep(1);

                    bool isCompleted;

                    bool go = cx_run_one_step_compensation(ref runCount, out isCompleted);

                    if (!go || isCompleted)
                        break;
                }
                catch (Exception ex)
                {
                    if (_runFlag)
                    {
                        _LOG(ex, "live compensating 異常!");
                        SetNextState(9999);
                    }
                }
            }

            _runFlag = false;
            _thread = null;
        }
        #endregion


        void init_dirs()
        {
            if (!System.IO.Directory.Exists(IMAGE_SAVE_PATH))
            {
                System.IO.Directory.CreateDirectory(IMAGE_SAVE_PATH);
            }
        }
        void FireLiveImaging(Bitmap bmp)
        {
            var e = new ProcessEventArgs("live.image", bmp);
            OnLiveImage?.Invoke(this, e);
        }
        bool FireCompensating(string actor, QVector dst, QVector delta)
        {
            var arg = new object[] { actor, dst, delta };
            var e = new ProcessEventArgs("live.Compensating", arg);
            OnLiveCompensating?.Invoke(this, e);
            if (e.GoControlByClient != null)
                e.GoControlByClient.WaitOne();
            bool go = !e.Cancel;
            return go;
        }
        bool FireCompensatingAndCheckMotorPos(string actor, QVector dst, QVector delta, out bool isMotorPosChangedByClient)
        {
            bool go = true;
            isMotorPosChangedByClient = false;
            if (OnLiveCompensating != null)
            {
                _LOG("調適: 單步補償");
                go = FireCompensating(actor, dst, delta);
            }
            return go;
        }

        const int N_MOTORS = 6;
        const double MAX_DELTA_XYZ = 0.25;
        const double MAX_DELTA_A = 0.25;
        const double MIN_DELTA_XYZ = MAX_DELTA_XYZ / 100;
        const double MIN_DELTA_A = 0.0167;
        const double INCR_XYZ = MAX_DELTA_XYZ / 10;
        const double INCR_A = 0.0167;
        double _decay_rate = 0.9;


        IAxis[] _blackboxMotors = null;
        IAxis[] BlackBoxMotors
        {
            get
            {
                if (_blackboxMotors == null)
                {
                    var indexes = new int[] { 0, 1, 2, 6, 7, 8 };
                    _blackboxMotors = new IAxis[indexes.Length];
                    int i = 0;
                    foreach (int idx in indexes)
                        _blackboxMotors[i++] = GetAxis(idx);
                }
                return _blackboxMotors;
            }
        }
        int[] m_motorParams = new int[2];
        QVector m_initMotorPos = new QVector(N_MOTORS);
        QVector m_currMotorPos = new QVector(N_MOTORS);
        QVector m_nextMotorPos = new QVector(N_MOTORS);
        QVector m_lastIncr = new QVector(N_MOTORS);
        QVector m_incr = new QVector(N_MOTORS);
        GdxMotorCoordsTransform m_trf;

        void cx_init_compensation()
        {
            //(0) Read Motors Current Position as InitPos
            System.Diagnostics.Trace.Assert(ax_is_ready());
            m_initMotorPos = ax_read_current_pos();

            //(1) Configuration
            var Ini = INI.Instance;
            var MotorCfg = MotorConfig.Instance;
            double sphereCenterOffsetU = (_mirrorIndex==0) ?
                        Ini.Mirror1_Offset_Adj : 
                        Ini.Mirror2_Offset_Adj;
            double u0 = MotorCfg.VirtureZero;
            double theta_z0 = MotorCfg.TheaZVirtureZero;
            double theta_y0 = MotorCfg.TheaYVirtureZero;
            QVector mv0 = m_initMotorPos;
            m_trf = new GdxMotorCoordsTransform(mv0, sphereCenterOffsetU);
            
            //(2) Set Motors Speed         
            for (int i = 0; i < N_MOTORS; i++)
            {
                var pmotor = (PLCMotionClass)BlackBoxMotors[i];
                pmotor.SetSpeed(SpeedTypeEnum.GOSLOW);
            }            
        }
        bool cx_run_one_step_compensation(ref int runCount, out bool isCompleted)
        {
            bool go = cx_run_one_step_coretron(ref runCount, out isCompleted);

            if (go && isCompleted)
            {
                set_light(false);
                go = cx_run_one_step_jez();
                if (go)
                {
                    SetNextState(4001);
                }
            }

            return go;
        }
        bool cx_run_one_step_coretron(ref int runCount, out bool isCompleted)
        {
            isCompleted = false;

            if (ax_is_error())
            {
                _LOG("Motor Error", Color.Red);
                return false;
            }

            if (!ax_is_ready())
            {
                return true;
            }

            using (Bitmap bmp = snapshot_image(runCount))
            {
                // 中光電
                FireLiveImaging(bmp);

                Color color = _mirrorIndex == 0 ? Color.DarkOrange : Color.Blue;
                int compType = _mirrorIndex == 0 ? 1 : 0;
                GdxCore.CalcProjCompensation(bmp, m_motorParams, compType);
                
                _LOG("Coretronics", "ProjComp", m_motorParams[0], m_motorParams[1], color);

                isCompleted = _is_zero(m_motorParams);
                if (isCompleted)
                    return true;

                // 計算馬達移動
                m_currMotorPos = ax_read_current_pos();
                m_incr = _calc_next_incr(m_motorParams, m_lastIncr);
                m_nextMotorPos += m_currMotorPos + m_incr;
                m_lastIncr = m_incr;
                if (_clip_into_safe_box(m_nextMotorPos))
                    m_incr = m_nextMotorPos - m_currMotorPos;

                // 是否可以完成
                isCompleted = _is_almost_zero(m_incr);
                if (isCompleted)
                    return true;

                // STEP TRACER
                bool isDirty;
                bool go = FireCompensatingAndCheckMotorPos("Coretronics", m_nextMotorPos, m_incr, out isDirty);
                if (!go)
                {
                    _LOG("調適: 中止補償!", Color.Red);
                    SetNextState(9999);
                    return false;
                }
                if (isDirty)
                {
                    //// Repeat next run by thread_func
                    //// _LOG("馬達位置已被改動, 重新計算...");
                    //// return true;
                    _LOG("馬達位置已被改動, 為安全起見, 強制中止補償!", Color.Red);
                    return false;
                }
                if (++runCount > MAX_RUN_COUNT)
                {
                    _LOG("補償次數超過上限", MAX_RUN_COUNT, Color.Red);
                    SetNextState(9999);
                    return false;
                }

                //> _LOG("move motors", m_nextMotorPos);
                ax_start_move(m_nextMotorPos);

                #region SIMULATION
                if (!isCompleted && GdxGlobal.Facade.IsSimCamera(1))
                {
                    isCompleted = (runCount >= 3);
                    if (isCompleted)
                        return true;
                }
                #endregion

                return true;
            }
        }
        bool cx_run_one_step_jez()
        {
            _LOG("JEZ 微調補償");

            while (true)
            {
                //取出 中光電貢獻的補償量
                m_currMotorPos = ax_read_current_pos();
                var cxDelta = m_currMotorPos - m_initMotorPos;

                //JEZ 補償
                var ezDelta = m_trf.CalcSphereCenterCompensation(m_initMotorPos, cxDelta);
                m_nextMotorPos = m_currMotorPos + ezDelta;
                if (_clip_into_safe_box(m_nextMotorPos))
                    ezDelta = m_nextMotorPos - m_currMotorPos;

                // STEP TRACER
                bool isDirty;
                bool go = FireCompensatingAndCheckMotorPos("JEZ", m_nextMotorPos, ezDelta, out isDirty);
                if (!go)
                {
                    _LOG("調適: 中止補償!", Color.Red);
                    SetNextState(9999);
                    return false;
                }
                if (isDirty)
                {
                    //// Repeat Polling Motor Pos
                    //// _LOG("馬達位置已被改動, 重新計算...");
                    //// continue;
                    _LOG("馬達位置已被改動, 為安全起見, 強制中止補償!", Color.Red);
                    return false;
                }

                // 下馬達指令
                ax_start_move(m_nextMotorPos);
                return go;
            }
        }
        bool cx_is_last_step_completed()
        {
            return ax_is_ready();
        }

        QVector ax_read_current_pos()
        {
            //var motors = BlackboxMotors();
            var pos = new QVector(N_MOTORS);
            for (int i = 0; i < N_MOTORS; i++)
                pos[i] = BlackBoxMotors[i].GetPos();
            return ax_convert_to_physical_unit(pos);
        }
        void ax_start_move(QVector pos)
        {
            //var motors = BlackboxMotors();
            var steps = ax_convert_to_plc_unit(pos);
            for (int i = 0; i < 4; i++)
            {
                BlackBoxMotors[i].Go(steps[i], 0);
            }
            for (int i = 4; i < N_MOTORS; i++)
            {
                BlackBoxMotors[i].Go(steps[i], 0);
            }
        }
        bool ax_is_ready()
        {
            //var motors = BlackboxMotors();
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (!BlackBoxMotors[i].IsOK)
                    return false;
            }
            return true;
        }
        bool ax_is_error()
        {
            //var motors = BlackboxMotors();
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (BlackBoxMotors[i].IsError)
                    return true;
            }
            return false;
        }
        QVector ax_convert_to_plc_unit(QVector pos)
        {
            var v = new QVector(pos);
            v[4] = Math.Round(v[4] / 0.0167);
            v[5] = Math.Round(v[5] / 0.0167);
            return v;
        }
        QVector ax_convert_to_physical_unit(QVector pos)
        {
            var v = new QVector(pos);
            v[4] = Math.Round(v[4] * 0.0167);
            v[5] = Math.Round(v[5] * 0.0167);
            return v;
        }

        QVector _calc_next_incr(int[] motorParams, QVector lastIncr)
        {
            var incr = new QVector(N_MOTORS);

            //for (int i = 0; i < 4; i++)
            //{
            //    if (motorParams[i] > 0)
            //        incr[i] = STEP_XYZ;
            //    else if (motorParams[i] < 0)
            //        incr[i] = -STEP_XYZ;
            //    else
            //        incr[i] = 0;
            //}

            //for (int i = 4; i < N_MOTORS; i++)
            //{
            //    if (motorParams[i] > 0)
            //        incr[i] = STEP_A;
            //    else if (motorParams[i] < 0)
            //        incr[i] = -STEP_A;
            //    else
            //        incr[i] = 0;
            //}

            // Y軸 封住不動
            //incr[1] = 0;

            // 2022/06/21 only compensate theta_y, theta_z
            for (int i = 0; i < 4; i++)
                incr[i] = 0;

            for (int i = 4, k=0; i < N_MOTORS; i++, k++)
            {
                if (motorParams[k] > 0)
                    incr[i] = INCR_A;
                else if (motorParams[k] < 0)
                    incr[i] = -INCR_A;
                else
                    incr[i] = 0;
            }

            return incr;
        }
        bool _clip_into_safe_box(QVector pos)
        {
            bool clip = false;
            for (int i = 0; i < 4; i++)
            {
                double min = m_initMotorPos[i] - MAX_DELTA_XYZ;
                double max = m_initMotorPos[i] + MAX_DELTA_XYZ;
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
            for (int i = 4; i < N_MOTORS; i++)
            {
                double min = m_initMotorPos[i] - MAX_DELTA_A;
                double max = m_initMotorPos[i] + MAX_DELTA_A;
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
        bool _is_almost_zero(QVector delta)
        {
            bool yes = true;
            yes &= Math.Abs(delta[0]) < MIN_DELTA_XYZ;
            yes &= Math.Abs(delta[1]) < MIN_DELTA_XYZ;
            yes &= Math.Abs(delta[2]) < MIN_DELTA_XYZ;
            yes &= Math.Abs(delta[3]) < MIN_DELTA_XYZ;
            yes &= Math.Abs(delta[4]) < MIN_DELTA_A;
            yes &= Math.Abs(delta[5]) < MIN_DELTA_A;
            return yes;
        }
        bool _is_zero(int[] motorParams)
        {
            bool yes = true;
            for (int i = 0; i < motorParams.Length; i++)
                yes &= (motorParams[i] == 0);
            return yes;
        }

        Bitmap snapshot_image(int runCount)
        {
            // Capture Image
            var cam = ICamForBlackBox;
            cam.Snap();
            Bitmap bmp = new Bitmap(cam.GetSnap());

            #region ASYNC_DUMP_IMAGE
            var dump_func = new Action<Bitmap, int>((b, i) => {
                string fileName = string.Format("image_proj_{0}.bmp", i);
                fileName = System.IO.Path.Combine(IMAGE_SAVE_PATH, fileName);
                b.Save(fileName, ImageFormat.Bmp);
            });
            dump_func.BeginInvoke((Bitmap)bmp.Clone(), runCount, null, null);
            #endregion

            return bmp;
        }
        void set_light(bool on)
        {
            _LOG(on ? "打光" : "關光");
            MACHINE.PLCIO.ADR_POGO_PIN = on;
        }
    }
}
