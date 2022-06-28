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

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
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
                            ICamForCali.StopCapture();
                            ICamForBlackBox.StopCapture();

                            int expo = RecipeCHClass.Instance.JudgeCamExpo;
                            _LOG("提前設定曝光時間", expo);
                            ICamForBlackBox.SetExposure(expo);
                            SetNextState(100, 1000);
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
                                _LOG(phase.Name, "開始");
                                start_scan_thread(phase);
                                SetNextState(199, 500);
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
                                _LOG(phase.Name, "開始");
                                start_scan_thread(phase);
                                SetNextState(299, 500);
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
        int[] m_motorParams = new int[2];                           // from coretronics dll
        //double _decay_rate = 0.9;                                 // reserved for smart compensation
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
            ax_set_motor_speed(SpeedTypeEnum.GOSLOW);

            //(1) Phase Run Context
            m_phase1.StepFunc = phase1_run_one_step;
            m_phase1.InitMotorPos = new QVector(m_initMotorPos);
            m_phase1.Reset();

            //(2) LOG
            _LOG("單步最大補償量", COMP_STEP);

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
            var cam = ICamForBlackBox;
            using (Bitmap bmp = snapshot_image(cam, runCtrl.RunCount))
            {
                //(2.1) 通知 GUI 更新 Image
                // FireLiveImaging(bmp);

                //(2.2) 紅or綠光斑
                Color color = m_mirrorIndex == 0 ? Color.DarkOrange : Color.Blue;
                int compType = m_mirrorIndex == 0 ? 1 : 0;
                GdxCore.CalcProjCompensation(bmp, m_motorParams, compType);
                _LOG(runCtrl.Name, "Coretronics", "ProjComp", m_motorParams[0], m_motorParams[1], color);

                //(2.2) 通知 GUI 更新 Image
                FireLiveImaging(bmp);

                //(2.3) m_motorParams == (0,0) => 完成
                runCtrl.IsCompleted = _is_zero(m_motorParams);
                if (runCtrl.IsCompleted)
                {
                    //(2.4) 調試模式
                    m_currMotorPos = ax_read_current_pos();
                    if (!check_debug_mode(runCtrl, null, m_currMotorPos, m_incr))
                        return;
                    //(2.5) Completed
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
                if (false)
                {
                    runCtrl.IsCompleted = _is_almost_zero(m_incr);
                }

                //(6.1) 調試模式
                if (!check_debug_mode(runCtrl, null, m_currMotorPos, m_incr))
                    return;

                //(6.2) IsCompleted ?
                if (runCtrl.IsCompleted)
                    return;

                //(8) Max Run Count
                if (!check_max_run_count(runCtrl))
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
            return m_phase2;
        }
        QVector phase2_calc_target(double sphereCenterOffsetU)
        {
            double cosFactor = 0.89;

            //取出 中光電貢獻的補償量
            var initPos = this.CompensationInitPos;
            m_currMotorPos = ax_read_current_pos();
            var cxDelta = m_currMotorPos - initPos;
            _LOG("中光電補償後角度變化", "(Δθy, Δθz)", cxDelta.Slice(4, 2));

            //JEZ 球心偏移 補償 目標值
            //var delta = m_trf.CalcSphereCenterCompensation(initPos, cxDelta);
            var delta = m_trf.SimpleSphereCenterCompensation(sphereCenterOffsetU, initPos, m_currMotorPos);
            var targetPos = m_currMotorPos + delta * cosFactor;

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
            System.Threading.Thread.Sleep(MOTOR_CMD_DELAY);
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
