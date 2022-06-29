using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.OPSpace;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Drawing;
using System.Linq;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 中心點偏移補償流程
    /// @LETIAN: 20220624 加入Phase3 線程 支援 互動式調試模式.
    /// @LETIAN: 20220619 重新包裝
    /// </summary>
    public class MirrorCalibProcess : MirrorAbsImageProcess
    {
        const string PHASE_1 = "02-1 中光電 Center Comp";
        const string PHASE_2 = "02-2 移動吸嘴模组";
        const string PHASE_3 = "02-3 中心偏移補償";

        #region PRIVATE_DATA
        /// <summary>
        /// 判断校正组 跑哪一个Mirror 左边还是右边
        /// </summary>
        int m_mirrorIndex = 0;
        #endregion

        #region SINGLETON
        static MirrorCalibProcess _singleton = null;
        private MirrorCalibProcess()
        {
        }
        #endregion

        public static MirrorCalibProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MirrorCalibProcess();
                return _singleton;
            }
        }
        public override string Name
        {
            get { return "Mirror校正"; }
        }


        /// <summary>
        /// 第一個參數 args[0] 為 Mirror_CalibrateProcessIndex
        /// </summary>
        /// <param name="args"></param>
        public override void Start(params object[] args)
        {
            //@ LETIAN: 改用直接使用 args[0], args[1]
            // 先轉型成 string, (以便與舊程式相容.)
            if (args.Length > 0)
            {
                int.TryParse(args[0].ToString(), out m_mirrorIndex);
            }
            if (args.Length > 1)
            {
                if (bool.TryParse(args[1].ToString(), out bool is_step_debug))
                {
                    m_phase3.IsDebugMode = is_step_debug;
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
                            Process.NextDuriation = NextDurtimeTmp;
                            //switch (Process.RelateString)
                            //{
                            //    case "0":
                            //        Mirror_CalibrateProcessIndex = 0;
                            //        Process.ID = 10;
                            //        MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                            //        CommonLogClass.Instance.LogMessage("校正启动Mirror0", Color.Black);
                            //        GdxCore.Trace("MirrorCalibration.Start", Process, 0);
                            //        break;
                            //    case "1":
                            //        Mirror_CalibrateProcessIndex = 1;
                            //        Process.ID = 10;
                            //        MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                            //        CommonLogClass.Instance.LogMessage("校正启动Mirror1", Color.Black);
                            //        GdxCore.Trace("MirrorCalibration.Start", Process, 1);
                            //        break;
                            //    default:
                            //        m_mainprocess.Stop();
                            //        Process.Stop();
                            //        CommonLogClass.Instance.LogMessage("校正 未定义Mirror的值停止流程", Color.Red);
                            //        break;
                            //}

                            if (is_thread_running())
                            {
                                _LOG("Thread 未清除, 中止流程", Color.Red);
                                Terminate();
                                return;
                            }
                            else if (0 <= m_mirrorIndex && m_mirrorIndex < 2)
                            {
                                Process.ID = 10;
                                MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                                _LOG("啟動", "Mirror", m_mirrorIndex);
                                GdxCore.Trace("MirrorCalibration.Start", Process, m_mirrorIndex);
                            }
                            else
                            {
                                // CommonLogClass.Instance.LogMessage("校正 未定义Mirror的值停止流程", Color.Red);
                                _LOG("未定義 Mirror", m_mirrorIndex, "停止流程", Color.Red);
                                Terminate();
                            }
                        }
                        break;
                    #endregion

                    #region PHASE_I

                    case 10:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //到达位置 打开灯光 设定曝光
                                //ICamForCali.SetExposure(RecipeCHClass.Instance.CaliCamExpo);
                                //Process.NextDuriation = NextDurtimeTmp;
                                //Process.ID = 20;
                                int expo = RecipeCHClass.Instance.CaliCamExpo;
                                _LOG("設定曝光時間", expo);
                                ICamForCali.SetExposure(expo);
                                SetNextState(15);
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            _LOG(PHASE_1, "擷取影像");
                            //(15.0) 拍照
                            var cam = ICamForCali;
                            using (Bitmap bmp = snapshot_image(cam, 0))
                            {
                                //(15.1) 通知 GUI 更新 Image
                                // FireLiveImaging(bmp);
                                //(15.2) 中光電 Go-NoGo
                                bool go = GdxCore.CheckCompensate(bmp);
                                FireLiveImaging(bmp);
                                //(15.3) NO-GO
                                if (!go)
                                {
                                    // 就地停止 Process
                                    Terminate();
                                    // Log
                                    _LOG(PHASE_1, "回報 No GO!");
                                    FireMessage("NG. " + PHASE_1 + ", 回報 No GO!");
                                    return;
                                }
                            }
                            //(15.4) 關燈
                            MACHINE.PLCIO.ADR_SMALL_LIGHT = false;
                            //(15.5) Move Motors (goto 20)
                            SetNextState(20);
                        }
                        break;

                    #endregion

                    #region PHASE_II
                    case 20:
                        if (Process.IsTimeup)
                        {
                            _LOG(PHASE_2);

#if (false)
                            //> =====================================================
                            //@LETIAN: 現在 中光電已經改成 Go/NoGO 是否還要使用此步驟?
                            //> =====================================================
                            // 计算偏移值 
                            // 参数中算的解析度或是手动输入
                            // 参数中先记录Mirror的中心位置
                            // 测试中算的Mirror的中心位置
                            // 计算两个中心位置之差
                            // 补偿的是吸嘴模组的y和z轴 相当于画面中的 x和y 
                            // 画面中向左为正 向下为正
                            //> =====================================================
                            PointF ptfOffset = new PointF(0, 0);
                            ptfOffset.X -= RecipeCHClass.Instance.CaliPicCenter.X;
                            ptfOffset.Y -= RecipeCHClass.Instance.CaliPicCenter.Y;

                            //补偿放入的位置
                            string mirrorPutPos = m_mirrorIndex == 0 ?
                                    INI.Instance.Mirror1PutPos :
                                    INI.Instance.Mirror2PutPos;
                            string posPutAdjust = ToolAdjustData(mirrorPutPos, ptfOffset.X, ptfOffset.Y);
                            GdxCore.Trace("MirrorCalibration.MoveToPut", Process, "putPos", mirrorPutPos);
                            GdxCore.Trace("MirrorCalibration.MoveToPut", Process, "ofset", ptfOffset);
                            GdxCore.Trace("MirrorCalibration.MoveToPut", Process, "adjPos", posPutAdjust);
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 3, posPutAdjust);
#else
                            string mirrorPutPos = m_mirrorIndex == 0 ?
                                        INI.Instance.Mirror1PutPos :
                                        INI.Instance.Mirror2PutPos;
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 3, mirrorPutPos);
#endif

                            // MACHINE.PLCIO.ADR_SMALL_LIGHT = false; //<<< 已經於 state (15.4) 關掉
                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 301;
                        }
                        break;

                    case 301:
                        if (Process.IsTimeup)
                        {
                            MACHINE.PLCIO.SetIO(IOConstClass.QB1543, true);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 30;
                        }
                        break;

                    case 30:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorCalibration.IO.Wait", Process, "QB1543", false);

                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1543))
                            {
                                //微調模組到達 0的位置 方便下面 微調
                                //CommonLogClass.Instance.LogMessage("吸嘴模组到达位置", Color.Black);
                                _LOG("吸嘴模组到达位置");

                                //微調 Z=0 thetaY=ready thetaZ=ready

                                string posStr = "0,";
                                posStr += MACHINECollection.GetSingleAXISPositionForReady(7) + ",";
                                posStr += MACHINECollection.GetSingleAXISPositionForReady(8);

                                GdxCore.Trace("MirrorCalibration.Adjust", Process, "Pos", posStr);
                                MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, posStr);

                                //switch (Mirror_CalibrateProcessIndex)
                                //{
                                //    case 0:
                                //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, "0,0,0");
                                //        break;
                                //    case 1:
                                //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, "0,0,0");
                                //        break;
                                //}

                                //switch (Mirror_CalibrateProcessIndex)
                                //{
                                //    case 0:
                                //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, INI.Instance.sMirrorPutAdjDeep1Length + ",0,0");
                                //        break;
                                //    case 1:
                                //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, INI.Instance.sMirrorPutAdjDeep2Length + ",0,0");
                                //        break;
                                //}

                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, INI.Instance.sMirrorPutAdjDeep1Length + ",0,0");
                                MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_ADJUST, 1);

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 40;
                            }
                        }
                        break;

                    case 40:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_ADJUST, 1))
                            {
                                _LOG("微调模组到达位置");
                                ////// CommonLogClass.Instance.LogMessage("微调模组到达位置", Color.Black);
                                ////// Process.Stop();
                                ////// CommonLogClass.Instance.LogMessage("校正完成", Color.Black);
                                ////// GdxCore.Trace("MirrorCalibration.Completed", Process);
                                ////// FireCompleted();
                                //---------------------
                                // 進入 PHASE_III
                                SetNextState(300);
                            }
                        }
                        break;
                    #endregion

                    #region PHASE_III_CETER_COMPENSATION

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
                            var phase = m_phase3;
                            bool isReady = check_motor_ready(phase, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady)
                            {
                                _LOG(phase.Name, "開始");
                                start_scan_thread(phase);
                                SetNextState(399, 500);
                            }
                        }
                        break;
                    case 399:
                        // PHASE_1 (THREADING)
                        break;
                    case 3000:
                        if (Process.IsTimeup)
                        {
                            var phase = m_phase3;
                            bool isReady = check_motor_ready(phase, out bool isError);
                            if (isError)
                            {
                                SetNextState(9999);
                            }
                            else if (isReady && check_completed(phase))
                            {
                                _LOG(phase.Name, "完成");
                                double L = GdxCore.GetQCMotorPos(m_mirrorIndex, out double X, out double Y, out double Z);                                
                                _LOG("mirror", m_mirrorIndex, "QC 馬達座標", new QVector(X, Y, Z), "Laser 期望值", L.ToString("0.000"), Color.Purple);
                                Stop();
                                FireCompleted();
                            }
                        }
                        break;

                    #endregion

                    #region EXCEPTIONS
                    case 9999:
                        if (Process.IsTimeup)
                        {
                            _LOG("補償中止!", Color.Purple);
                            Terminate();
                            break;
                        }
                        break;
                        #endregion
                }
            }
        }


        #region NONE_USED_FUNCTIONS
        /// <summary>
        /// 工具 对位置进行补偿
        /// </summary>
        /// <param name="eOrg">原始位置 格式x,y,z</param>
        /// <param name="eOffsetX">x补偿</param>
        /// <param name="eOffsetY">y补偿</param>
        /// <returns>补偿后的位置 格式x,y,z</returns>
        private string ToolAdjustData(string eOrg, float eOffsetX, float eOffsetY)
        {
            string[] orgs = eOrg.Split(',').ToArray();
            float x = float.Parse(orgs[1]) + eOffsetX;
            float y = float.Parse(orgs[2]) + eOffsetY;
            string res = orgs[0] + "," + x.ToString() + "," + y.ToString();
            return res;
        }
        #endregion


        #region CENTER_COMPENSATION_MODULE

        protected override QVector CompensationInitPos
        {
            get { return m_phase3.InitMotorPos; }
        }

        QVector m_initMotorPos = new QVector(N_MOTORS);
        QVector m_currMotorPos = new QVector(N_MOTORS);
        QVector m_nextMotorPos = new QVector(N_MOTORS);
        QVector m_targetPos = new QVector(N_MOTORS);
        QVector m_incr = new QVector(N_MOTORS);

        XRunContext m_phase3 = new XRunContext(PHASE_3, 3000);
        XRunContext phase3_init()
        {
            //(0) Read Motors Current Position as InitPos
            m_showIDs = new int[] { 3, 4, 5 };
            m_initMotorPos = ax_read_current_pos();
            ax_set_motor_speed(SpeedTypeEnum.GOSLOW);

            //(1) Phase Run Context
            m_phase3.StepFunc = phase3_run_one_step;
            m_phase3.InitMotorPos = new QVector(m_initMotorPos);
            m_phase3.Reset();

            //(2) Configuration and Calculation
            var comp = GdxGlobal.Facade.LaserCoordsTransform;
            if (comp.CanCompensate(m_mirrorIndex))
            {
                var delta = comp.CalcCompensation(m_mirrorIndex, m_initMotorPos);
                _LOG("中心偏移補償", "預計補償量", delta);
                m_targetPos = m_initMotorPos + delta;
                _clip_into_safe_box(m_targetPos);
            }
            else
            {
                _LOG("中心偏移補償", "沒有建好雷射量測點", "忽略中...", Color.OrangeRed);
                m_targetPos = m_initMotorPos;
                m_phase3.IsDebugMode = false;
                return m_phase3;
            }

            //(*) Simulation
            #region SIMULATION
            //////if (false && (GdxGlobal.Facade.IsSimMotor() || GdxGlobal.Facade.IsSimPLC()))
            //////{
            //////    var rnd = new Random();
            //////    m_targetPos = new QVector(m_initMotorPos);
            //////    for (int i = 0; i < 4; i++)
            //////    {
            //////        m_targetPos[i] += MAX_DELTA[i] * (rnd.NextDouble() * 0.5 - 1);
            //////    }
            //////    _clip_into_safe_box(m_targetPos);
            //////    AxisUnitConvert.Round(m_targetPos, true);
            //////}
            #endregion

            // U compensation step 增大
            COMP_STEP[3] = AxisUnitConvert.PERCISIONS[3] * 25;

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
            //(1) Current Pos
            m_currMotorPos = ax_read_current_pos();

            //(2) Delta for *** PHASE III ***
            m_incr = phase3_calc_next_incr(runCtrl, m_currMotorPos, m_targetPos);

            //(3) Next Pos
            m_nextMotorPos = m_currMotorPos + m_incr;

            //(4) Safe Box
            if (_clip_into_safe_box(m_nextMotorPos))
                m_incr = m_nextMotorPos - m_currMotorPos;

            //(5) delta 小於馬達解析度, 當作完成.
            runCtrl.IsCompleted = AxisUnitConvert.IsSmallVector(m_incr);

            //(5.1) 調試模式
            if (!check_debug_mode(runCtrl, m_targetPos, m_currMotorPos, m_incr))
                return;

            //(5.2) IsCompleted ?
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
        QVector phase3_calc_next_incr(XRunContext runCtrl, QVector cur, QVector target)
        {
            // DEBUG
            // return new QVector(N_MOTORS);

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
    }
}
