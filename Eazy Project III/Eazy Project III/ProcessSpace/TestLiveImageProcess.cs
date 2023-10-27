using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.GdxCore3;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Diagnostics;
using System.Drawing;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 即時連續取像
    /// </summary>
    public class TestLiveImageProcess : MirrorAbsImageProcess
    {
        const string PHASE_1 = "連續取像";

        #region PRIVATE_DATA
        ICam m_cam;
        int m_camID = 0;
        string m_name;
        #endregion

        #region SINGLETON
        static TestLiveImageProcess _singletonOfStressTest = null;
        TestLiveImageProcess(string name)
        {
            m_name = name;
        }
        #endregion

        public override string Name
        {
            get { return m_name; }
        }
        public static TestLiveImageProcess Instance
        {
            get
            {
                if (_singletonOfStressTest == null)
                {
                    _singletonOfStressTest = new TestLiveImageProcess("Test.LiveImaging");
                }
                return _singletonOfStressTest;
            }
        }
        public static TestLiveImageProcess CreateLiveProcess()
        {
            var process = new TestLiveImageProcess("Normal.LiveImaging");
            return process;
        }

        /// <summary>
        /// 第一個參數 args[0] 為 camID
        /// </summary>
        /// <param name="args"></param>
        public override void Start(params object[] args)
        {
            if (base.IsOn)
                return;

            if (m_name.StartsWith("Normal"))
            {
                if (args.Length > 0)
                {
                    int.TryParse(args[0].ToString(), out m_camID);
                    m_cam = (m_camID == 0) ? ICamForCali : ICamForBlackBox;
                }

                // 直接啟動線程, 不依賴 Tick() !!!
                m_test = null;
                if (!is_thread_running() && !base.IsOn)
                {
                    base.IsOn = true;
                    SetNextState(100, 0);
                    Tick();
                    return;
                }
            }
            else
            {
                init_test();
                base.Start();
            }
        }
        public override void Stop()
        {
            end_test();
            base.Stop();

            if (is_directly_threading_mode())
            {
                //for (int i = 0; i < 10; i++)
                //{
                //    if (is_thread_running())
                //        System.Threading.Thread.Sleep(200);
                //    else
                //        break;
                //}
                _LOG("線程 Stop", $"CamID={m_camID}");
            }
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
                            int expo = (m_camID == 0) ?
                                RecipeCHClass.Instance.CaliCamExpo :
                                RecipeCHClass.Instance.JudgeCamExpo;
                            //>>> expo = 1000;    // just for speed testing.
                            _LOG("設定曝光時間", expo);
                            m_cam.SetExposure(expo);
                            SetNextState(100);
                        }
                        break;

                    case 100:
                        if (Process.IsTimeup || is_directly_threading_mode())
                        {
                            _LOG("線程 Start", $"CamID={m_camID}");
                            phase1_init();
                            start_scan_thread(m_phase1);
                            SetNextState(199, 500);
                        }
                        break;

                    case 199:
                        // Threading
                        break;

                    case 1000:
                        if (Process.IsTimeup)
                        {
                            Stop();
                            FireCompleted();
                        }
                        break;

                    #endregion

                    case 9999:
                        #region EXCEPTIONS
                        if (Process.IsTimeup)
                        {
                            _LOG("中止!", Color.Purple);
                            Terminate();
                            break;
                        }
                        #endregion
                        break;
                }
            }
        }
        public Bitmap Snapshot()
        {
            Stop();
            for (int i = 0; i < 10; i++)
            {
                if (is_thread_running())
                    System.Threading.Thread.Sleep(200);
                else
                    break;
            }
            var bmp = snapshot_image(m_cam, 0, true);
            return bmp;
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
            //    (純像測不需要)
            m_phase1.InitMotorPos = ax_read_current_pos();

            //(1) Phase One Step Func
            if (m_test == null)
                m_phase1.StepFunc = phase1_run_one_step_capture;
            else
                m_phase1.StepFunc = phase1_run_one_step_stress_test;

            // Reset                
            m_phase1.Reset();
            m_timeFps = DateTime.Now;
            return m_phase1;
        }
        void phase1_run_one_step_capture(XRunContext runCtrl)
        {
            // var runCtrl = m_run2;
            runCtrl.IsCompleted = false;

            //(1) 檢查馬達狀態
            if (!runCtrl.Go)
                return;

            //(2) 拍照
            bool dump = false;
            using (Bitmap bmp = snapshot_image(m_cam, 0, dump))
            {
                //(2.1) 通知 GUI 更新 Image
                if (runCtrl.Go)
                {
                    FireLiveImaging(bmp);
                }
            }
        }
        void phase1_run_one_step_stress_test(XRunContext runCtrl)
        {
            runCtrl.IsCompleted = false;

            //(1) 檢查馬達狀態
            if (false)
            {
                if (!check_motor_ready(runCtrl, out bool isError))
                    return;

                if (!runCtrl.Go || isError)
                    return;
            }
            else
            {
                if (!runCtrl.Go)
                    return;
            }


            //(2) 拍照
            bool dump = false;
            int runCount = m_test.TestCount % 6;
            using (Bitmap bmp = snapshot_image(m_cam, runCount, dump))
            {
                if (runCtrl.Go)
                {
                    // (2.1) 測試 coretronics DLL
                    if (m_test.UsingDLL)
                    {
                        var sw = new Stopwatch();
                        int mirrorIndex = m_test.MirrorIndex;
                        int compType = GdxCore.getProjCompType(mirrorIndex, out Color color);
                        string name;
                        if (m_camID == 0)
                        {
                            sw.Start();
                            GdxCore.CheckCenterCompensation(compType, bmp);
                            sw.Stop();
                            var info = GdxCore.GetCenterCompensationInfo(compType);
                            FireCompensatedInfo(name = "Go/NoGo判定", mirrorIndex, bmp, info);
                        }
                        else
                        {
                            sw.Start();
                            GdxCore.CalcProjCompensation(bmp, new int[2], compType);
                            sw.Stop();
                            var info = GdxCore.GetProjCompensationInfo(compType);
                            FireCompensatedInfo(name = "光斑", mirrorIndex, bmp, info);
                        }
                        _LOG($"camID={m_camID}", $"調用中光電 {name} DLL = {(int)sw.ElapsedMilliseconds} ms", color);
                    }
                    // (2.2) 測試單純 LiveImaging
                    else
                    {
                        FireLiveImaging(bmp);
                    }
                }

                if (!check_max_run_count(runCtrl))
                    return;

                // FPS 統計
                if (runCtrl.RunCount >= 10)
                {
                    var tm1 = DateTime.Now;
                    var ts = tm1 - m_timeFps;
                    var fps = runCtrl.RunCount / ts.TotalSeconds;
                    m_timeFps = tm1;
                    _LOG($"camID={m_camID}", $"rotate={m_test.Rotate}", $"fps={fps:0.0}");
                    runCtrl.RunCount = 0;
                }
            }
        }
        bool is_directly_threading_mode()
        {
            return m_test == null;
        }
        #endregion

        #region STRESS_TESTING_MEMBERS
        class XTestContext
        {
            public int TestCount = 0;
            public int OldRotate = 0;
            public int Rotate = 0;
            public bool UsingDLL = false;
            public int MirrorIndex = 0;
        }
        XTestContext m_test = null;
        DateTime m_timeFps = DateTime.Now;
        #endregion

        #region PRIVAE_FUNCTIONS
        void init_test()
        {
            if (m_test == null)
                m_test = new XTestContext();

            int nextCamID = m_test.TestCount % 2;
            var nextCam = nextCamID == 0 ? ICamForCali : ICamForBlackBox;

            if (m_cam != nextCam)
            {
                //end_test();     // 恢復原有的旋轉角度
                m_test.OldRotate = nextCam.RotateAngle;
            }

            m_cam = nextCam;
            m_camID = nextCamID;


            int dualRunCount = m_test.TestCount / 2;
            switch (dualRunCount % 4)
            {
                case 0:
                    m_test.Rotate = 0;
                    m_test.UsingDLL = false;
                    break;
                case 1:
                    m_test.Rotate = 270;
                    m_test.UsingDLL = false;
                    break;
                case 2:
                    m_test.Rotate = 270;
                    m_test.UsingDLL = true;
                    m_test.MirrorIndex = 0;     //紅
                    break;
                case 3:
                    m_test.Rotate = 270;
                    m_test.UsingDLL = true;
                    m_test.MirrorIndex = 1;     //綠
                    break;
            }

            m_cam.RotateAngle = m_test.Rotate;
            m_test.TestCount++;
            m_timeFps = DateTime.Now;

            string tag = "無 DLL";
            Color color = Color.Purple;
            if (m_test.UsingDLL)
            {
                int compType = GdxCore.getProjCompType(m_test.MirrorIndex, out color);
                tag = compType == 0 ? "使用 DLL (綠)" : "使用 DLL (紅)";
            }
            _LOG("準備測試", $"CamID={m_camID}", $"Rotate={m_test.Rotate}", tag, color);
        }
        void end_test()
        {
            if (m_test != null && m_cam != null)
            {
                m_cam.RotateAngle = m_test.OldRotate;
                _LOG($"camID={m_camID}", "恢復原有的旋轉角度", m_cam.RotateAngle);
            }
        }
        #endregion
    }
}
