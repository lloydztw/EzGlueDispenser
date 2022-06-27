using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
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
    public class TestLiveImageProcess : MirrorAbsImageProcess
    {
        const string PHASE_1 = "連續取像測試";

        #region PRIVATE_DATA
        ICam m_cam;
        int m_camID = 0;
        DateTime m_tm;
        #endregion

        #region SINGLETON
        static TestLiveImageProcess _singleton = null;
        private TestLiveImageProcess()
        {
        }
        #endregion

        public static TestLiveImageProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new TestLiveImageProcess();
                return _singleton;
            }
        }
        public override string Name
        {
            get { return "LiveImageTest"; }
        }

        /// <summary>
        /// 第一個參數 args[0] 為 mirror_index
        /// </summary>
        /// <param name="args"></param>
        public override void Start(params object[] args)
        {
            if (args.Length > 0)
            {
                int.TryParse(args[0].ToString(), out m_camID);
                m_cam = (m_camID == 0) ? ICamForCali : ICamForBlackBox;
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
                            else
                            {
                                Process.ID = 10;
                                MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                                _LOG("準備", "CamID", m_camID);
                            }
                        }
                        break;
                    #endregion

                    #region PHASE_I

                    case 10:
                        if (Process.IsTimeup)
                        {
                            int expo = m_camID == 0 ?
                                RecipeCHClass.Instance.CaliCamExpo :
                                RecipeCHClass.Instance.JudgeCamExpo;
                            _LOG("設定曝光時間", expo);
                            ICamForCali.SetExposure(expo);
                            SetNextState(100);
                        }
                        break;

                    case 100:
                        if (Process.IsTimeup)
                        {
                            _LOG("啟動線程");
                            phase1_init();
                            start_scan_thread(m_phase1);
                            SetNextState(199, 500);
                        }
                        break;

                    case 199:
                        // Threading
                        break;

                    case 1000:
                        if(Process.IsTimeup)
                        {
                            _LOG("結束線程");
                            Stop();
                            FireCompleted();
                        }
                        break;

                    #endregion

                    case 9999:
                        #region EXCEPTIONS
                        if (Process.IsTimeup)
                        {
                            _LOG("補償中止!", Color.Purple);
                            Terminate();
                            break;
                        }
                        #endregion
                        break;
                }
            }
        }

        #region LIVE_IMAGE_LOOP
        protected override QVector CompensationInitPos
        {
            get { return m_phase1.InitMotorPos; }
        }
        XRunContext m_phase1 = new XRunContext(PHASE_1, 1000);
        XRunContext phase1_init()
        {
            //(0) Read Motors Current Position as InitPos
            m_phase1.InitMotorPos = ax_read_current_pos();

            //(1) Phase Run Context
            m_phase1.StepFunc = phase1_run_one_step;
            m_phase1.Reset();

            m_tm = DateTime.Now;
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
            using (Bitmap bmp = snapshot_image(m_cam, runCtrl.RunCount % 5))
            {
                //(2.1) 通知 GUI 更新 Image
                FireLiveImaging(bmp);
                
                if(runCtrl.RunCount > 20)
                {
                    var tm1 = DateTime.Now;
                    var ts = tm1 - m_tm;
                    var fps = runCtrl.RunCount / ts.TotalSeconds;
                    m_tm = tm1;
                    _LOG("fps", fps.ToString("0.0"));
                    runCtrl.RunCount = 1;
                }

                if (!check_max_run_count(runCtrl))
                    return;
            }
        }
        #endregion
    }
}
