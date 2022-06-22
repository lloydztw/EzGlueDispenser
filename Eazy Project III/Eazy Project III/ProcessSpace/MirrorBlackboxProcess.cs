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
    public class CompensatingEventArgs : ProcessEventArgs
    {
        public string PhaseName;
        public QVector MaxDelta;
        public QVector InitPos;
        public QVector CurrentPos;
        public QVector Delta;
        public bool ContinueToDebug;
    }



    /// <summary>
    /// 投影補償 (Projection Compensate) 流程 <br/>
    /// --------------------------------------------
    /// @LETIAN: 20220619 開始實作
    /// </summary>
    public class MirrorBlackboxProcess : BaseProcess
    {
        const string IMAGE_SAVE_PATH = @"D:\EVENTLOG\Nlogs\images";
        const string PHASE_1 = "03-1 平面點位補償";
        const string PHASE_2 = "03-2 光測投影補償";
        const string PHASE_3 = "03-3 轉軸球心補償";
        static int MOTOR_TIMEOUT_WAIT_COUNT = 1000;
        static int MAX_RUN_COUNT = int.MaxValue;

        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion

        #region PRIVATE_DATA
        int _mirrorIndex = 0;
        bool _is_step_debug1 = false;
        bool _is_step_debug2 = false;
        bool _is_step_debug3 = false;
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
        public event EventHandler<CompensatingEventArgs> OnLiveCompensating;
        public override string Name
        {
            get { return "BlackBox"; }
        }

        /// <summary>
        /// 啟動 <br/>
        /// args[0]: mirrorIndex <br/>
        /// args[1]: is_step_debug <br/>
        /// </summary>
        /// <param name="args">mirrorIndex, is_step_debug</param>
        public override void Start(params object[] args)
        {
            // 先轉型成 string, (以便與舊程式相容.)
            if (args.Length > 0)
            {
                int.TryParse(args[0].ToString(), out _mirrorIndex);
            }
            if (args.Length > 1)
            {
                if (bool.TryParse(args[1].ToString(), out bool is_step_debug))
                {
                    _is_step_debug1 = is_step_debug;
                    _is_step_debug2 = is_step_debug;
                    _is_step_debug3 = is_step_debug;
                }
            }
            base.Start();
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
                        if (true)
                        {
                            if (is_thread_running())
                            {
                                _LOG("Thread 未清除, 中止流程", Color.Red);
                                Terminate();
                                return;
                            }
                            else if (!(0 <= _mirrorIndex && _mirrorIndex < 2))
                            {
                                _LOG("MirrorIndex 錯誤, 中止流程", Color.Red);
                                Terminate();
                            }
                            else
                            {
                                _LOG("Start", "Mirror", _mirrorIndex);
                                set_light(true);
                                SetNextState(10);
                            }
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            int expo = RecipeCHClass.Instance.JudgeCamExpo;
                            _LOG("提前設定曝光時間", expo);
                            ICamForBlackBox.SetExposure(expo);
                            SetNextState(100);
                        }
                        break;

                    case 100:
                        if (Process.IsTimeup)
                        {
                            _LOG(PHASE_1, "預備");
                            phase1_init();
                            SetNextState(110);
                        }
                        break;
                    case 110:
                        if (Process.IsTimeup)
                        {                            
                            bool isReady = check_motor_ready(m_phase1, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady)
                            {
                                _LOG(PHASE_1, "開始");
                                start_scan_thread(m_phase1, phase1_run_one_step);
                                SetNextState(199, 500);
                            }
                        }
                        break;
                    case 199:
                        // PHASE_1 THREADING
                        break;
                    case 1000:
                        if (Process.IsTimeup)
                        {
                            bool isReady = check_motor_ready(m_phase1, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady && check_completed(m_phase1))
                            {
                                _LOG(PHASE_1, "完成");
                                SetNextState(200);
                            }
                        }
                        break;

                    case 200:
                        if (Process.IsTimeup)
                        {
                            _LOG(PHASE_2, "預備");
                            phase2_init();
                            SetNextState(210);
                        }
                        break;
                    case 210:
                        if (Process.IsTimeup)
                        {
                            bool isReady = check_motor_ready(m_phase2, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if  (isReady)
                            {
                                _LOG(PHASE_2, "開始連續攝相");
                                start_scan_thread(m_phase2, phase2_run_one_step);
                                SetNextState(299, 500);
                            }
                        }
                        break;
                    case 299:
                        // PHASE_2 THREADING
                        break;
                    case 2000:
                        if (Process.IsTimeup)
                        {
                            bool isReady = check_motor_ready(m_phase2, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady && check_completed(m_phase2))
                            {
                                _LOG(PHASE_2, "完成");
                                SetNextState(300);
                            }
                        }
                        break;


                    case 300:
                        if (Process.IsTimeup)
                        {
                            _LOG(PHASE_3, "預備");
                            phase3_init();
                            SetNextState(310);
                        }
                        break;
                    case 310:
                        if (Process.IsTimeup)
                        {
                            bool isReady = check_motor_ready(m_phase3, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if  (isReady)
                            {
                                _LOG(PHASE_3, "開始");
                                start_scan_thread(m_phase3, phase3_run_one_step);
                                SetNextState(399);
                            }
                        }
                        break;
                    case 399:
                        // PHASE_3 THREADING
                        break;
                    case 3000:
                        if (Process.IsTimeup)
                        {
                            bool isReady = check_motor_ready(m_phase3, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                                break;
                            }
                            if (isReady && check_completed(m_phase3))
                            {
                                _LOG(PHASE_3, "完成");
                                Stop();
                                FireCompleted();
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
        void start_scan_thread(XRunContext run, Action<XRunContext> one_step_func)
        {
            if (!is_thread_running())
            {
                _runFlag = true;
                _thread = new Thread(thread_func);
                _thread.Start(new object[] { run, one_step_func });
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
        void thread_func(object args)
        {
            object[] objs = (object[])args;
            var runPhase = (XRunContext)objs[0];
            var runFunc = (Action<XRunContext>)phase1_run_one_step;

            while (_runFlag)
            {
                try
                {
                    Thread.Sleep(1);

                    runFunc(runPhase);

                    if (!runPhase.Go)
                        break;

                    if (runPhase.IsCompleted)
                        break;
                }
                catch (Exception ex)
                {
                    if (_runFlag)
                    {
                        _LOG(ex, "live compensating 異常!");
                        SetNextState(9999);
                    }
                    break;
                }
            }

            _runFlag = false;
            _thread = null;

            if (runPhase == m_phase1)
            {
                SetNextState(1000);
            }
            if (runPhase == m_phase2)
            {
                SetNextState(2000);
            }
            if (runPhase == m_phase3)
            {
                SetNextState(3000);
            }
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
        bool FireCompensating(XRunContext phase, QVector cur, QVector delta, out bool isMotorPosChangedByClient)
        {
            var e = new CompensatingEventArgs()
            {
                PhaseName = phase.Name,
                InitPos = new QVector(m_phase1.InitMotorPos),  // 固定傳回 phase1 init pos 
                MaxDelta = new QVector(MAX_DELTA),
                CurrentPos = new QVector(cur),
                Delta = new QVector(delta),
                ContinueToDebug = true
            };

            OnLiveCompensating?.Invoke(this, e);
            if (e.GoControlByClient != null)
                e.GoControlByClient.WaitOne();

            if (e.Cancel)
            {
                phase.Go = false;
            }

            if(!e.ContinueToDebug)
            {
                phase.IsDebugMode = false;
            }

            // 檢查馬達位置是否被 user 移動
            var cur2 = ax_read_current_pos();
            isMotorPosChangedByClient = !QVector.AreEqual(cur, cur2);

            return phase.Go;
        }


        const int N_MOTORS = 6;
        static QVector MAX_DELTA = new QVector(0.25, 0.25, 0.25, 0.25, 2, 2);
        static QVector MIN_DELTA = new QVector(0.0025, 0.0025, 0.0025, 0.0025, 0.0167, 0.0167);
        //static double MAX_DELTA_XYZ = 0.25;
        //static double MAX_DELTA_A = 2;
        //static double MIN_DELTA_XYZ = MAX_DELTA_XYZ / 100;
        //static double MIN_DELTA_A = 0.0167;
        static double STEP_XYZU = MIN_DELTA[0] * 2;
        static double STEP_A = MIN_DELTA[5] * 5;
        static double[] THETA_DIR = new double[] { -1, 1 };     // theta_y, theta_z
        int[] m_motorParams = new int[2];                       // from coretronics dll
        double _decay_rate = 0.9;                               // reserved for smart compensation
        GdxMotorCoordsTransform m_trf;


        IAxis[] _blackboxMotors = null;
        IAxis[] BlackBoxMotors
        {
            get
            {
                if (_blackboxMotors == null)
                {
                    // X, Y, Z, U, theta_y, theta_z
                    var indexes = new int[] { 0, 1, 2, 6, 7, 8 };
                    _blackboxMotors = new IAxis[indexes.Length];
                    int i = 0;
                    foreach (int idx in indexes)
                        _blackboxMotors[i++] = GetAxis(idx);
                }
                return _blackboxMotors;
            }
        }
        QVector m_initMotorPos = new QVector(N_MOTORS);
        QVector m_currMotorPos = new QVector(N_MOTORS);
        QVector m_nextMotorPos = new QVector(N_MOTORS);
        QVector m_lastIncr = new QVector(N_MOTORS);
        QVector m_incr = new QVector(N_MOTORS);
        volatile int m_motorWaitCount = 0;

#if(OLD)
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
                m_incr = phase2_calc_next_incr(m_motorParams, m_lastIncr);
                m_nextMotorPos = m_currMotorPos + m_incr;

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
                //////double u0 = 0;
                //////var mv0 = m_initMotorPos + new QVector(0, 0, 0, u0, 0, 0);
                //////var trf = new GdxMotorCoordsTransform(mv0);
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
#endif

        QVector ax_convert_to_physical_unit(QVector pos)
        {
            var v = new QVector(pos);
            v[4] = (v[4] * 0.0167);
            v[5] = (v[5] * 0.0167);
            return v;
        }
        QVector ax_convert_to_plc_unit(QVector pos)
        {
            var v = new QVector(pos);
            v[4] = Math.Round(v[4] / 0.0167);
            v[5] = Math.Round(v[5] / 0.0167);
            return v;
        }
        QVector ax_read_current_pos()
        {
            var pos = new QVector(N_MOTORS);
            for (int i = 0; i < N_MOTORS; i++)
                pos[i] = BlackBoxMotors[i].GetPos();
            return ax_convert_to_physical_unit(pos);
        }
        void ax_start_move(QVector pos)
        {
            var plcPos = ax_convert_to_plc_unit(pos);
            for (int i = 0; i < 4; i++)
            {
                BlackBoxMotors[i].Go(plcPos[i], 0);
            }

            for (int i = 4; i < N_MOTORS; i++)
            {
                BlackBoxMotors[i].Go(plcPos[i], 0);
            }
        }
        void ax_set_motor_speed(SpeedTypeEnum mode)
        {
            for (int i = 0; i < N_MOTORS; i++)
            {
                var pmotor = (PLCMotionClass)BlackBoxMotors[i];
                pmotor.SetSpeed(mode);
            }
        }
        bool ax_is_wait_ready_timeout()
        {
            return m_motorWaitCount > MOTOR_TIMEOUT_WAIT_COUNT;
        }
        bool ax_is_ready()
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
        bool ax_is_error()
        {
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (BlackBoxMotors[i].IsError)
                    return true;
            }
            return false;
        }

        class XRunContext
        {
            public string Name;
            public int RunCount = 0;
            public bool IsCompleted = false;
            public bool Go = true;
            public bool IsDebugMode = false;
            public QVector InitMotorPos = null;
            public XRunContext(string name)
            {
                Name = name;
            }
        }
        XRunContext m_phase1 = null;
        XRunContext m_phase2 = null;
        XRunContext m_phase3 = null;

        XRunContext phase1_init()
        {
            //(0) Read Motors Current Position as InitPos
            ax_set_motor_speed(SpeedTypeEnum.GOSLOW);
            m_initMotorPos = ax_read_current_pos();

            //(1) Phase Run Context
            m_phase1 = new XRunContext(PHASE_1);
            m_phase1.InitMotorPos = new QVector(m_initMotorPos);
            m_phase1.IsDebugMode = _is_step_debug1;

            //(2) Configuration
            //var Ini = INI.Instance;
            //var MotorCfg = MotorConfig.Instance;
            //double sphereCenterOffsetU = (_mirrorIndex == 0) ?
            //            Ini.Mirror1_Offset_Adj :
            //            Ini.Mirror2_Offset_Adj;

            //double u0 = MotorCfg.VirtureZero;
            //double theta_z0 = MotorCfg.TheaZVirtureZero;
            //double theta_y0 = MotorCfg.TheaYVirtureZero;

            //QVector mv0 = m_initMotorPos;
            //m_trf = new GdxMotorCoordsTransform(mv0, sphereCenterOffsetU);
            return m_phase1;
        }
        void phase1_run_one_step(XRunContext runCtrl)
        {
            runCtrl.IsCompleted = false;

            // 檢查馬達狀態
            if (!check_motor_ready(runCtrl, out bool isError))
                return;
            if (!runCtrl.Go || isError)
                return;

            // 計算馬達移動
            {
                //(1) Current Pos
                m_currMotorPos = ax_read_current_pos();

                //(2) Delta for *** PHASE 1 ***
                m_incr = phase1_calc_next_incr(runCtrl);

                //(3) Next Pos
                m_nextMotorPos = m_currMotorPos + m_incr;

                //(4) Safe Box
                m_lastIncr = m_incr;
                if (_clip_into_safe_box(m_nextMotorPos))
                    m_incr = m_nextMotorPos - m_currMotorPos;

                //(5) delta 小於馬達解析度, 當作完成.
                runCtrl.IsCompleted = _is_almost_zero(m_incr);
                if (runCtrl.IsCompleted)
                    return;

                //(6) 調試模式
                if (runCtrl.IsDebugMode)
                {
                    check_debug_mode(runCtrl, m_currMotorPos, m_incr);
                }

                //(7) Max Run Count
                if (!check_max_run_count(runCtrl))
                    return;

                //(8) 下指令
                _LOG(runCtrl.Name, "馬達 Delta", m_incr);
                _LOG(runCtrl.Name, "馬達 GoTo", m_nextMotorPos);
                ax_start_move(m_nextMotorPos);
            }
        }
        QVector phase1_calc_next_incr(XRunContext runCtrl)
        {
            var incr = new QVector(N_MOTORS);

            #region SIMULATION
            if (GdxGlobal.Facade.IsSimPLC())
            {
                if (runCtrl.RunCount > 5)
                {
                    runCtrl.IsCompleted = true;
                    return incr;
                }
                else
                {
                    incr.X = new Random().NextDouble() * STEP_XYZU;
                }
            }
            #endregion

            return incr;
        }

        XRunContext phase2_init()
        {
            //(0) Read Motors Current Position as InitPos
            m_initMotorPos = ax_read_current_pos();

            //(1) Phase Run Context
            m_phase2 = new XRunContext(PHASE_2);
            m_phase2.InitMotorPos = new QVector(m_initMotorPos);
            m_phase2.IsDebugMode = _is_step_debug2;

            //(2) Configuration
            var Ini = INI.Instance;
            var MotorCfg = MotorConfig.Instance;
            double sphereCenterOffsetU = (_mirrorIndex == 0) ?
                        Ini.Mirror1_Offset_Adj :
                        Ini.Mirror2_Offset_Adj;

            double u0 = MotorCfg.VirtureZero;
            double theta_z0 = MotorCfg.TheaZVirtureZero;
            double theta_y0 = MotorCfg.TheaYVirtureZero;

            QVector mv0 = m_initMotorPos;
            m_trf = new GdxMotorCoordsTransform(mv0, sphereCenterOffsetU);

            return m_phase2;
        }
        void phase2_run_one_step(XRunContext runCtrl)
        {
            // var runCtrl = m_run2;
            runCtrl.IsCompleted = false;
            
            //(1) 檢查馬達狀態
            if (!check_motor_ready(runCtrl, out bool isError))
                return;
            if (!runCtrl.Go || isError)
                return;

            //(2) 拍照
            using (Bitmap bmp = snapshot_image(runCtrl.RunCount))
            {
                //(2.1) 通知 GUI 更新 Image
                FireLiveImaging(bmp);

                //(2.2) 紅or綠光斑
                Color color = _mirrorIndex == 0 ? Color.DarkRed : Color.DarkGreen;
                int compType = _mirrorIndex == 0 ? 1 : 0;
                GdxCore.CalcProjCompensation(bmp, m_motorParams, compType);
                _LOG(runCtrl.Name, "Coretronics", "ProjComp", m_motorParams[0], m_motorParams[1], color);

                //(2.3) m_motorParams == (0,0) => 完成
                runCtrl.IsCompleted = _is_zero(m_motorParams);
                if (runCtrl.IsCompleted)
                    return;

                //(3) Current Pos
                m_currMotorPos = ax_read_current_pos();

                //(4) 計算 Delta for *** PHASE 2 ***
                m_incr = phase2_calc_next_incr(m_motorParams, m_lastIncr);
                m_nextMotorPos = m_currMotorPos + m_incr;

                //(5) Safe-Box
                m_lastIncr = m_incr;
                if (_clip_into_safe_box(m_nextMotorPos))
                    m_incr = m_nextMotorPos - m_currMotorPos;

                //(6) delta 小於馬達解析度, 當作完成.
                runCtrl.IsCompleted = _is_almost_zero(m_incr);
                if (runCtrl.IsCompleted)
                    return;

                //(7) 調試模式
                if (runCtrl.IsDebugMode)
                {
                    check_debug_mode(runCtrl, m_currMotorPos, m_incr);
                }

                //(8) Max Run Count
                if (!check_max_run_count(runCtrl))
                    return;

                //(9) 下馬達指令
                _LOG(runCtrl.Name, "馬達 Delta", m_incr);
                _LOG(runCtrl.Name, "馬達 GoTo", m_nextMotorPos);
                ax_start_move(m_nextMotorPos);

                #region SIMULATION
                if (!runCtrl.IsCompleted && GdxGlobal.Facade.IsSimCamera(1))
                {
                    if (runCtrl.RunCount >= 3)
                    {
                        runCtrl.IsCompleted = true;
                        return;
                    }
                }
                #endregion
            }
        }
        QVector phase2_calc_next_incr(int[] motorParams, QVector lastIncr)
        {
            var incr = new QVector(N_MOTORS);

            // 2022/06/21 only compensate theta_y, theta_z
            for (int i = 0; i < 4; i++)
                incr[i] = 0;

            // 2022/06/21 only compensate theta_y, theta_z
            for (int i = 4, k = 0; i < N_MOTORS; i++, k++)
            {
                if (motorParams[k] > 0)
                    incr[i] = STEP_A * THETA_DIR[k];
                else if (motorParams[k] < 0)
                    incr[i] = -STEP_A * THETA_DIR[k];
                else
                    incr[i] = 0;
            }

            return incr;
        }

        XRunContext phase3_init()
        {
            //(0) Read Motors Current Position as InitPos
            m_initMotorPos = ax_read_current_pos();

            //(0.1) Phase Run Context
            m_phase3 = new XRunContext(PHASE_3);
            m_phase3.InitMotorPos = new QVector(m_initMotorPos);
            m_phase3.IsDebugMode = _is_step_debug3;

            //(1) Configuration
            //var Ini = INI.Instance;
            //var MotorCfg = MotorConfig.Instance;
            //double sphereCenterOffsetU = (_mirrorIndex == 0) ?
            //            Ini.Mirror1_Offset_Adj :
            //            Ini.Mirror2_Offset_Adj;

            //double u0 = MotorCfg.VirtureZero;
            //double theta_z0 = MotorCfg.TheaZVirtureZero;
            //double theta_y0 = MotorCfg.TheaYVirtureZero;

            //QVector mv0 = m_initMotorPos;
            //m_trf = new GdxMotorCoordsTransform(mv0, sphereCenterOffsetU);

            //(2) Set Motors Speed (SLOW-MODE)        
            //for (int i = 0; i < N_MOTORS; i++)
            //{
            //    var pmotor = (PLCMotionClass)BlackBoxMotors[i];
            //    pmotor.SetSpeed(SpeedTypeEnum.GOSLOW);
            //}

            return m_phase3;
        }
        void phase3_run_one_step(XRunContext runCtrl)
        {
            runCtrl.IsCompleted = false;

            // 檢查馬達狀態
            if (!check_motor_ready(runCtrl, out bool isError))
                return;
            if (!runCtrl.Go || isError)
                return;


            // 計算馬達移動
            {
                //(1) Current Pos
                m_currMotorPos = ax_read_current_pos();

                //(2) Delta for *** PHASE 3 ***
                m_incr = phase3_calc_next_incr(runCtrl);

                //(3) Next Pos
                m_nextMotorPos = m_currMotorPos + m_incr;

                //(4) Safe Box
                m_lastIncr = m_incr;
                if (_clip_into_safe_box(m_nextMotorPos))
                    m_incr = m_nextMotorPos - m_currMotorPos;

                //(5) delta 小於馬達解析度, 當作完成.
                runCtrl.IsCompleted = _is_almost_zero(m_incr);
                if (runCtrl.IsCompleted)
                    return;

                //(6) 調試模式
                if (runCtrl.IsDebugMode)
                {
                    check_debug_mode(runCtrl, m_currMotorPos, m_incr);
                }

                //(7) Max Run Count
                if (!check_max_run_count(runCtrl))
                    return;

                //(8) 下指令
                _LOG(runCtrl.Name, "馬達 Delta", m_incr);
                _LOG(runCtrl.Name, "馬達 GoTo", m_nextMotorPos);
                ax_start_move(m_nextMotorPos);
            }
        }
        QVector phase3_calc_next_incr(XRunContext runCtrl)
        {
            var incr = new QVector(N_MOTORS);

            #region SIMULATION
            if (GdxGlobal.Facade.IsSimPLC())
            {
                if (runCtrl.RunCount > 5)
                {
                    runCtrl.IsCompleted = true;
                    return incr;
                }
                else
                {
                    incr.Z = new Random().NextDouble() * STEP_XYZU;
                }
            }
            #endregion
            
            return incr;
        }


        /// <summary>
        /// 檢查 motor 狀態是否 ready, 
        /// 有異常時會將 runCtrl.Go = false
        /// </summary>
        /// <param name="runCtrl"></param>
        /// <returns>go/no go</returns>
        bool check_motor_ready(XRunContext runCtrl, out bool isError)
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
        bool check_max_run_count(XRunContext runCtrl)
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
        bool check_debug_mode(XRunContext runCtrl, QVector curMotorPos, QVector incr)
        {
            bool go = FireCompensating(runCtrl, curMotorPos, incr, out bool isDirty);

            if (!go)
            {
                _LOG(runCtrl.Name, "調試", "中止補償!", Color.Red);
                SetNextState(9999);
                return (runCtrl.Go = false);
            }

            if (isDirty)
            {
                //// Repeat next run by thread_func
                //// _LOG("馬達位置已被改動, 重新計算...");
                //// return true;
                
                _LOG(runCtrl.Name, "調試", "馬達位置已被改動, 為安全起見, 強制中止補償!", Color.Red);
                return (runCtrl.Go = false);
            }

            return runCtrl.Go;
        }
        bool check_completed(XRunContext runCtrl)
        {
            if (runCtrl.IsCompleted)
            {
                return ax_is_ready();
            }
            return false;
        }


        bool _clip_into_safe_box(QVector pos)
        {
            bool clip = false;
            //for (int i = 0; i < 4; i++)
            //{
            //    double min = m_initMotorPos[i] - MAX_DELTA_XYZ;
            //    double max = m_initMotorPos[i] + MAX_DELTA_XYZ;
            //    if (pos[i] < min)
            //    {
            //        pos[i] = min;
            //        clip = true;
            //    }
            //    if (pos[i] > max)
            //    {
            //        pos[i] = max;
            //        clip = true;
            //    }
            //}
            //for (int i = 4; i < N_MOTORS; i++)
            //{
            //    double min = m_initMotorPos[i] - MAX_DELTA_A;
            //    double max = m_initMotorPos[i] + MAX_DELTA_A;
            //    if (pos[i] < min)
            //    {
            //        pos[i] = min;
            //        clip = true;
            //    }
            //    if (pos[i] > max)
            //    {
            //        pos[i] = max;
            //        clip = true;
            //    }
            //}

            var initPos = m_phase1.InitMotorPos;
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
        bool _is_almost_zero(QVector delta)
        {
            bool yes = true;
            //yes &= Math.Abs(delta[0]) < MIN_DELTA_XYZ;
            //yes &= Math.Abs(delta[1]) < MIN_DELTA_XYZ;
            //yes &= Math.Abs(delta[2]) < MIN_DELTA_XYZ;
            //yes &= Math.Abs(delta[3]) < MIN_DELTA_XYZ;
            //yes &= Math.Abs(delta[4]) < MIN_DELTA_A;
            //yes &= Math.Abs(delta[5]) < MIN_DELTA_A;
            for (int i = 0; i < N_MOTORS; i++)
                yes &= Math.Abs(delta[i]) < MIN_DELTA[i];
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
            //MACHINE.PLCIO.ADR_POGO_PIN = on;//調試先不用
        }
    }







    
}
