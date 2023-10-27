using Eazy_Project_III.OPSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System;
using System.Drawing;


namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 投影補償 (Projection Compensate) 流程 <br/>
    /// --------------------------------------------
    /// @LETIAN: 20220623 加入Phase1, Phase2, Phase3
    /// @LETIAN: 20220619 開始實作
    /// </summary>
    public class MirrorBlackboxProcess : MirrorAbsImageProcess
    {
        static bool OPT_BYPASS_BALL_CENTER_COMP = false;
        const string PHASE_1 = "03-1 光斑投影補償";
        const string PHASE_2 = "03-2 轉軸球心補償";

        #region PROTECTED_DATA
        protected int m_mirrorIndex = 0;
        #endregion

        #region SINGLETON
        static MirrorBlackboxProcess _singleton = null;
        private MirrorBlackboxProcess()
        {
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
                int.TryParse(args[0].ToString(), out m_mirrorIndex);
            }
            if (args.Length > 1)
            {
                if (bool.TryParse(args[1].ToString(), out bool is_step_debug))
                {
                    m_phase1.IsDebugMode = is_step_debug;
                    m_phase2.IsDebugMode = is_step_debug;
                }
            }
            base.Start();
        }
        public override void Stop()
        {
            if (m_phase1 != null)
                m_phase1.IsUserStop = true;
            if (m_phase2 != null)
                m_phase2.IsUserStop = true;
            base.Stop();
        }

        public override void Tick()
        {
            if (!IsValidPlcScanned())
                return;

            var Process = this;

            if (Process.IsOn)
            {
                if (!IsValidPlcScanned())
                    return;

                switch (Process.ID)
                {
                    #region INIT

                    case 5:
                        if (true)
                        {
                            if (is_thread_running())
                            {
                                _LOG("Thread 未清除, 中止流程", Color.Red);
                                Terminate();
                                return;
                            }
                            else if (!(0 <= m_mirrorIndex && m_mirrorIndex < 2))
                            {
                                _LOG("MirrorIndex 錯誤, 中止流程", Color.Red);
                                Terminate();
                            }
                            else
                            {
                                _LOG("Start", "Mirror", m_mirrorIndex);
                                set_projector_light(true);
                                SetNextState(10);
                            }
                        }
                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            _LOG("設定相機非連續模式");
                            GetCamera(0).StopCapture();
                            GetCamera(1).StopCapture();

                            var cam = GetCamera(1);
                            Recipe.GetCameraExpoAndGain(1, m_mirrorIndex, out int expo, out float gain);
                            _LOG("提前設定曝光時間與增益", expo, gain.ToString("0.0"));
                            cam.SetExposure(expo);
                            cam.SetGain(gain);

                            // Light On Delay
                            SetNextState(100, RecipeCHClass.Instance.LightOnDelay2);
                        }
                        break;

                    #endregion

                    #region PHASE_I
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
                            var phase = m_phase1;
                            bool isReady = check_motor_ready(phase, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady)
                            {
                                _LOG(phase.Name, "開始線程");
                                SetNextState(199, 500);//放置线程前面，防止卡在线程中无法终止
                                start_scan_thread(phase);
                            }
                        }
                        break;
                    case 199:
                        // PHASE_1 (THREADING)
                        break;
                    case 1000:
                        if (Process.IsTimeup)
                        {
                            var phase = m_phase1;
                            bool isReady = check_motor_ready(phase, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady && check_completed(phase))
                            {
                                _LOG(phase.Name, "完成");
                                SetNextState(200);
                            }
                        }
                        break;

                    #endregion

                    #region PHASE_II
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
                            var phase = m_phase2;
                            bool isReady = check_motor_ready(phase, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady)
                            {
                                _LOG(phase.Name, "開始線程");
                                SetNextState(299, 500);//放置线程前面，防止卡在线程中无法终止
                                start_scan_thread(phase);
                            }
                        }
                        break;
                    case 299:
                        // PHASE_2 (THREADING)
                        break;
                    case 2000:
                        if (Process.IsTimeup)
                        {
                            var phase = m_phase2;
                            bool isReady = check_motor_ready(phase, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady && check_completed(phase))
                            {
                                _LOG(phase.Name, "完成");
                                set_projector_light(false);
                                Stop();
                                FireCompleted();
                            }
                        }
                        break;
                    #endregion

                    case 9999:
                        #region EXCEPTIONS
                        if (Process.IsTimeup)
                        {
                            _LOG("補償中止!", Color.Purple);
                            set_projector_light(false);
                            Terminate();
                            break;
                        }
                        #endregion
                        break;
                }
            }
        }


        #region COMPENSATION_MODULES

        /// <summary>
        /// θy, θz 補償方向
        /// </summary>
        static double[] THETA_DIR = new double[] { -1, 1 };         // θy, θz
        QVector m_thetaCompGain;
        QVector m_thetaCompGain2;
        int[] m_motorParams = new int[2];                           // from coretronics dll
        bool[] m_coreCompDone = new bool[2];
        int m_compMaxRunCount = int.MaxValue;
        int m_compSepPixels = 15;
        GdxMotorCoordsTransform m_trf;

        protected override QVector CompensationInitPos
        {
            get { return m_phase1.InitMotorPos; }
        }

        QVector m_initMotorPos = new QVector(N_MOTORS);
        QVector m_currMotorPos = new QVector(N_MOTORS);
        QVector m_nextMotorPos = new QVector(N_MOTORS);
        QVector m_targetPos = new QVector(N_MOTORS);
        QVector m_lastIncr = new QVector(N_MOTORS);
        QVector m_incr = new QVector(N_MOTORS);

        XRunContext m_phase1 = new XRunContext(PHASE_1, 1000);
        XRunContext m_phase2 = new XRunContext(PHASE_2, 2000);

        XRunContext phase1_init()
        {
            //(0) Read Motors Current Position as InitPos
            m_initMotorPos = ax_read_current_pos();
            m_showIDs = new int[] { 3, 4, 5 };
            ax_set_motor_speed(SpeedTypeEnum.GO);
            InitCompensationSteps();

            //(1) Phase Run Context
            m_phase1.StepFunc = phase1_run_one_step;
            m_phase1.InitMotorPos = new QVector(m_initMotorPos);
            m_phase1.Reset();

            //(2) steps and LOG
            //////double percA = AxisUnitConvert.PERCISIONS[5];
            //////double stepMaxA = Math.Round(RecipeCHClass.Instance.CompStepAngleMax * percA, 4);
            //////double stepMaxU = Math.Round(RecipeCHClass.Instance.CompStepSize * 0.001, 4);
            //////COMP_STEP = new QVector(stepMaxU, stepMaxU, stepMaxU, stepMaxU, stepMaxA, stepMaxA);
            //////_LOG("單步最大補償量", COMP_STEP);
            //////InitCompensationSteps();

            //(3) step gains
            double percA = AxisUnitConvert.PERCISIONS[5];
            double gain = percA * (double)RecipeCHClass.Instance.CompStepAngleGain;
            double gain2 = percA * (double)RecipeCHClass.Instance.CompStepAngleGain2;
            m_thetaCompGain = new QVector(THETA_DIR) * gain;
            m_thetaCompGain2 = new QVector(THETA_DIR) * gain2;

            //(4) max compensation times (phase 1 光斑)
            m_compMaxRunCount = (int)RecipeCHClass.Instance.CompMaxTimes;
            m_compSepPixels = (int)RecipeCHClass.Instance.CompSepPixels;
            _LOG(PHASE_1, "增益分水嶺", m_compSepPixels);

            //(5) individual completion flags
            for (int i = 0; i < m_coreCompDone.Length; i++)
                m_coreCompDone[i] = false;

            return m_phase1;
        }
        void phase1_run_one_step(XRunContext runCtrl)
        {
            // var runCtrl = m_run2;
            runCtrl.IsCompleted = false;
            
            //(1) 檢查馬達狀態
            if (!check_motor_ready(runCtrl, out bool isError))
                return;
            if (!runCtrl.Go || isError)
                return;

            //(2) 拍照
            _LOG(runCtrl.Name, "擷取影像");
            var cam = GetCamera(1);
            
            //(2.0) 紅斑(mirror==0) or 綠斑(mirror==1)
            int compType = GdxCore.getProjCompType(m_mirrorIndex, out Color color);
            string mirrorTag = color == Color.DarkGreen ? "G" : "R";

            using (Bitmap bmp = snapshot_image(cam, $"{mirrorTag}_projection", true))
            {
                //(2.1) Calls to DLL
                var tm0 = DateTime.Now;
                bool isCompOK = GdxCore.CalcProjCompensation(bmp, m_motorParams, compType);
                var ts = DateTime.Now - tm0;
                _LOG(runCtrl.Name, "Coretronics", "ProjComp", isCompOK ? "OK" : "NG", m_motorParams[0], m_motorParams[1],
                    "ms=" + (int)ts.TotalMilliseconds, color);

                //(2.2) 補償後圖示結果 (Compensation Info)
                var info = GdxCore.GetProjCompensationInfo(compType);

                //(2.3) 通知 GUI 更新 Image
                //> FireLiveImaging(bmp);
                FireCompensatedInfo(runCtrl.Name, m_mirrorIndex, bmp, info);

                //(2.4) NG
                if (!isCompOK)
                {
                    string errCompMsg = PHASE_1 + ", 判定 NG!";
                    // Log
                    _LOG(errCompMsg, Color.Red);
                    // Events
                    FireNG(errCompMsg);     //<<< 光斑補償 NG
                    // soft-terminate
                    SetNextState(9999);
                    // run flag
                    runCtrl.Go = false;
                    return;
                }

                //(2.5) m_motorParams == (0,0) => 完成
                runCtrl.IsCompleted = _is_completed(m_coreCompDone, m_motorParams);

                if (runCtrl.IsCompleted)
                {
                    //(2.5.1) 調試模式
                    m_currMotorPos = ax_read_current_pos();
                    if (!check_debug_mode(runCtrl, null, m_currMotorPos, m_incr))
                        return;

                    //(2.5.2) Completed
                    if (runCtrl.IsCompleted)
                        return;
                }

                //(3) Current Pos
                m_currMotorPos = ax_read_current_pos();

                //(4) 計算 Delta for *** PHASE 1 ***
                m_incr = phase1_calc_next_incr(m_motorParams, m_lastIncr);
                m_nextMotorPos = m_currMotorPos + m_incr;

                //(5) Safe-Box
                m_lastIncr = m_incr;
                if (_clip_into_safe_box(m_nextMotorPos))
                    m_incr = m_nextMotorPos - m_currMotorPos;

                //(6) delta 小於馬達解析度, 當作完成.
                //if (false)
                //{
                //    runCtrl.IsCompleted = _is_almost_zero(m_incr);
                //}

                //(6.1) 調試模式
                if (!check_debug_mode(runCtrl, null, m_currMotorPos, m_incr))
                    return;

                //(6.2) IsCompleted ?
                if (runCtrl.IsCompleted)
                    return;

                //(8) Max Run Count
                if (!check_max_run_count(runCtrl, m_compMaxRunCount))
                    return;
                
                //(9) 下馬達指令
                log_motor_command(runCtrl, m_nextMotorPos, m_incr);
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
        QVector phase1_calc_next_incr(int[] motorParams, QVector lastIncr)
        {
            var incr = new QVector(N_MOTORS);

            // 2022/06/21 only compensate theta_y, theta_z
            for (int i = 0; i < 4; i++)
                incr[i] = 0;

#if (false)
            // 2022/06/21 only compensate theta_y, theta_z
            for (int i = 4, k = 0; i < N_MOTORS; i++, k++)
            {
                if (motorParams[k] > 0)
                    incr[i] = (COMP_STEP[i] * THETA_DIR[k]);
                else if (motorParams[k] < 0)
                    incr[i] = -(COMP_STEP[i] * THETA_DIR[k]);
                else
                    incr[i] = 0;
            }
#else

            // k= 0,1
            int lenK = motorParams.Length;
            for (int i = 4, k = 0; k < lenK; i++, k++)
            {
                // LOG AGAIN
                int pixelsDiff = motorParams[k];

                var gains = Math.Abs(pixelsDiff) <= m_compSepPixels ? 
                    m_thetaCompGain : 
                    m_thetaCompGain2;

                // Log10 gain
                //  int sign = pixelsDiff >= 0 ? 1 : -1;
                //  double delta = (pixelsDiff) == 0 ? 0 :
                //    sign * gains[k] * Math.Log10(Math.Abs(pixelsDiff));

                // Linear Gain
                double delta = (pixelsDiff) == 0 ? 0 : gains[k] * pixelsDiff;
                double deltaA = Math.Abs(delta);

                // Force to zero if the local completion flag is ON.
                if (m_coreCompDone[k])
                {
                    // 2022-10-20 YoMin 要求:
                    // 如果補償 < 0.0167, 就用 0.0167 走完, 走完直接結束.
                    delta = 0.0;
                }
                else if (deltaA == 0)
                {
                    // 2022-10-20 YoMin 要求:
                    // 如果補償 < 0.0167, 就用 0.0167 走完, 走完直接結束.
                    m_coreCompDone[k] = true;
                }
                // Min
                else if (deltaA < MIN_DELTA[i])
                {
                    // 2022-10-20 YoMin 要求:
                    // 如果補償 < 0.0167, 就用 0.0167 走完, 走完直接結束.
                    delta = delta > 0 ? MIN_DELTA[i] : -MIN_DELTA[i];
                    m_coreCompDone[k] = true;
                    _LOG(PHASE_1, $"θ[{k}]達成強制停止條件", Color.DarkMagenta);
                }
                // Max
                else
                {
                    double deltaMax = COMP_STEP[i];
                    if (Math.Abs(delta) > deltaMax)
                        delta = delta > 0 ? deltaMax : -deltaMax;
                }

                incr[i] = delta;
            }
#endif

            //AxisUnitConvert.ToAxis(incr, true);
            //AxisUnitConvert.ToWorld(incr, true);

            return incr;
        }

        XRunContext phase2_init()
        {
            //(0) Read Motors Current Position as InitPos
            m_initMotorPos = ax_read_current_pos();
            m_showIDs = new int[] { 0, 1, 2, 3 };
            _LOG("中光電補償後最終角度", "(θy,θz)", m_initMotorPos.Slice(4, 2));

            //(1) Phase Run Context
            m_phase2.StepFunc = phase2_run_one_step;
            m_phase2.InitMotorPos = new QVector(m_initMotorPos);
            m_phase2.Reset();

            //(2) 球心偏移 U 值
            double sphereCenterOffsetU = 0;
            if (!OPT_BYPASS_BALL_CENTER_COMP)
            {
                var trf = GdxGlobal.Facade.LaserCoordsTransform;
                var lastCenterComp = trf.GetLastCompensation(m_mirrorIndex);
                if (lastCenterComp != null)
                {
                    sphereCenterOffsetU = lastCenterComp[3];
                    _LOG(m_phase2.Name, "球心偏移 ΔU", sphereCenterOffsetU);
                }
                else
                {
                    _LOG("缺少 02中心補償之結果", "球心偏移 ΔU 視為 0");
                }                
            }

            //(3) Transform 
            QVector mv0 = this.CompensationInitPos;
            m_trf = GdxGlobal.Facade.MotorCoordsTransform;
            m_trf.Init(mv0, sphereCenterOffsetU);

            // 計算 & 設定 目標值
            m_targetPos = phase2_calc_target(sphereCenterOffsetU);
            _clip_into_safe_box(m_targetPos);

            // Compensation Steps
            COMP_STEP[2] = AxisUnitConvert.PERCISIONS[3] * 25;
            COMP_STEP[3] = AxisUnitConvert.PERCISIONS[3] * 25;

            return m_phase2;
        }
        QVector phase2_calc_target(double sphereCenterOffsetU)
        {
            //取出 中光電貢獻的補償量
            m_currMotorPos = ax_read_current_pos();
            var initPos_before_core = this.CompensationInitPos;
            var finalPos_of_core = m_currMotorPos;
            var cxDelta = finalPos_of_core - initPos_before_core;
            _LOG("中光電補償後角度變化", "Δ(θy,θz)", cxDelta.Slice(4, 2));

            //JEZ 球心偏移 補償 目標值
            //var delta = m_trf.CalcSphereCenterCompensation(initPos, cxDelta);
            var delta = m_trf.SimpleSphereCenterCompensation(
                            sphereCenterOffsetU, 
                            initPos_before_core, 
                            finalPos_of_core);

            _LOG("球心計算之補償量", "Δ(X,Y,U,Z)", cxDelta.Slice(0, 4));
            var targetPos = m_currMotorPos + delta;

            return targetPos;
        }
        void phase2_run_one_step(XRunContext runCtrl)
        {
            runCtrl.IsCompleted = false;

            // 檢查馬達狀態
            if (!check_motor_ready(runCtrl, out bool isError))
                return;
            if (!runCtrl.Go || isError)
                return;

            // 計算馬達移動
            //(1) Current Pos
            m_currMotorPos = ax_read_current_pos();

            //(2) Delta for *** PHASE II ***
            m_incr = phase2_calc_next_incr(runCtrl, m_currMotorPos, m_targetPos);

            //(3) Next Pos
            m_nextMotorPos = m_currMotorPos + m_incr;

            //(4) Safe Box
            if (_clip_into_safe_box(m_nextMotorPos))
                m_incr = m_nextMotorPos - m_currMotorPos;

            //(5) delta 小於馬達解析度, 當作完成.
            runCtrl.IsCompleted = AxisUnitConvert.IsSmallVector(m_incr);

            //(5.1) 調試模式 (X,Y,Z,U)
            if (!check_debug_mode(runCtrl, m_targetPos, m_currMotorPos, m_incr))
            {
                m_showIDs = null;
                return;
            }
            m_showIDs = null;

            //(5.2) Completed
            if (runCtrl.IsCompleted)
                return;

            //(7) Max Run Count
            if (!check_max_run_count(runCtrl))
                return;

            //(8) 下指令
            log_motor_command(runCtrl, m_nextMotorPos, m_incr);
            ax_start_move(m_nextMotorPos);
            //System.Threading.Thread.Sleep(MOTOR_CMD_DELAY);
        }
        QVector phase2_calc_next_incr(XRunContext runCtrl, QVector cur, QVector target)
        {
            var incr = target - cur;
            for (int i = 0; i < N_MOTORS; i++)
            {
                if (Math.Abs(incr[i]) > COMP_STEP[i])
                {
                    incr[i] = incr[i] < 0 ? -COMP_STEP[i] : COMP_STEP[i];
                }
            }
            return AxisUnitConvert.Round(incr, true);
        }

#endregion


        void set_projector_light(bool on)
        {
            var projector = base.ProjectorActuactor;
            if (on)
            {
                projector.SetGain(Recipe.OutputGain);//打開光時 設定Gain值
            }
            if (m_mirrorIndex == 0)
            {
                projector.SetColor(Eazy_Project_Interface.ProjectColor.LightRed, on);
                _LOG(on ? "打紅光" : "關紅光");
            }
            else
            {
                projector.SetColor(Eazy_Project_Interface.ProjectColor.LightGreen, on);
                //projector.SetColor(Eazy_Project_Interface.ProjectColor.LightBlue, on);
                _LOG(on ? "打藍綠光" : "關藍綠光");
            }
            //MACHINE.PLCIO.ADR_POGO_PIN = on;//調試先不用
        }
    }
}
