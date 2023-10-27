using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.GdxCore3;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 即時連續取像
    /// </summary>
    public class RcpLiveImageProcess : MirrorAbsImageProcess
    {
        const string PHASE_1 = "即時取像";
        //const string PHASE_2 = "定位點檢";
        //const string PHASE_3 = "連續取像測試";

        #region PRIVATE_DATA
        string m_taskName;
        ICam m_cam = null;
        int m_camID = -1;
        #endregion

        #region SINGLETONS
        static RcpLiveImageProcess _primeLiveProcess = null;
        static RcpLiveImageProcess _rcpLiveProcess = null;
        RcpLiveImageProcess(string name)
        {
            m_taskName = name;
        }
        void restoreDefaultName()
        {
            if (this == _primeLiveProcess)
                m_taskName = "Prime.LiveImaging";
            else if (this == _rcpLiveProcess)
                m_taskName = "Rcp.LiveImaging";
            else
                m_taskName = "LiveImaging";
        }
        #endregion

        public override string Name
        {
            get { return m_taskName; }
        }
        public static RcpLiveImageProcess Instance
        {
            get
            {
                return Singleton("Prime");
            }
        }
        public static RcpLiveImageProcess Singleton(string name)
        {
            if (name == "Prime")
            {
                if (_primeLiveProcess == null)
                {
                    _primeLiveProcess = new RcpLiveImageProcess(name);
                    _primeLiveProcess.restoreDefaultName();
                }
                return _primeLiveProcess;
            }
            else
            {
                if (_rcpLiveProcess == null)
                {
                    _rcpLiveProcess = new RcpLiveImageProcess(name);
                    _rcpLiveProcess.restoreDefaultName();
                }
                return _rcpLiveProcess;
            }
        }

        public override void Stop()
        {
            if (is_testing())
                end_test();
            
            if (m_phase1 != null)
                m_phase1.IsUserStop = true;

            base.Stop();

            //if (is_directly_threading_mode())
            //{
            //    _LOG("線程 Stop", $"CamID={m_camID}");
            //}
        }

        /// <summary>
        /// 第一個參數 args[0] 可為:                  <br/>
        /// (1) camID (int) : 進行 一般即時連續取像   <br/>
        /// (2) "CheckMarks" : 進行 定位點檢          <br/>
        /// (3) "StressTest" : 進行 連續取像測試      <br/>
        /// </summary>
        public override void Start(params object[] args)
        {
            if (args.Length > 0)
            {
                if (int.TryParse(args[0].ToString(), out int camID))
                {
                    if (args.Length > 1)
                        m_taskName = args[1].ToString();
                    start_task(camID, m_taskName);                    
                }
                else if(args[0].ToString() == "CheckMarks")
                {
                    start_task(1, args[0].ToString());
                }
                else if (args[0].ToString() == "StressTest")
                {
                    start_task(0, args[0].ToString());
                }
                else
                {
                    _LOG("start: args 有誤", Color.Red);
                }
            }
        }

        #region PRIVATE_START_FUNCTION
        /// <summary>
        /// NextState ID will be 5 (for Tick())
        /// </summary>
        void start_task(int camID, string taskName)
        {
            if (taskName == "StressTest")
            {
                m_taskName = taskName;
                if (m_camID < 0)
                {
                    m_camID = 0;
                    m_cam = GetCamera(0);
                }
                SetNextState(5, 300);
                base.Start();
                return;
            }

            if (camID < 0 || camID >= 2)
            {
                _LOG($"start: camID={camID} 錯誤!");
                return;
            }
            else if (m_camID != camID || m_taskName != taskName)
            {
                bool isEmpty = (m_camID < 0 || m_cam == null);

                m_taskName = taskName;
                m_camID = camID;
                m_cam = GetCamera(camID);

                int delayTime = isEmpty ? 10 :
                    (IsOn || is_thread_running()) ? 300 : 10;

                SetNextState(5, delayTime);
                base.Start();
            }
            else
            {
                _LOG("start: 舊的 args, 持續現況不變.");

                if (m_cam != null && m_camID >= 0)
                {
                    int delayTime = (IsOn || is_thread_running()) ? 300 : 10;
                    SetNextState(5, delayTime);
                    base.Start();
                }
            }
        }
        void tick_rcpLiveProcess()
        {
            // Tick() 的哀愁 ~ 無法獨立運作.
            // 借用 prime Live 的 Tick()
            if (_rcpLiveProcess != null &&
                _rcpLiveProcess != this &&
                _rcpLiveProcess != _primeLiveProcess)
            {
                _rcpLiveProcess.Tick();
            }
        }
        #endregion


        public override void Tick()
        {
            tick_rcpLiveProcess();

            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:
                        #region INIT
                        if (true)
                        {
                            if (is_thread_running())
                            {
                                _LOG($"camID={m_camID}", "停止舊線程 ...");
                                int nextState = 6;
                                m_phase1.ExitCode = nextState;
                                stop_scan_thread();
                                SetNextState(nextState, 300);
                                // 強制 Process.IsOn = true
                                base.IsOn = true;
                                return;
                            }
                            else
                            {
                                _LOG($"camID={m_camID}", "準備啟動");
                                SetNextState(6, 10);
                            }
                        }
                        #endregion
                        break;

                    case 6:
                        #region WAIT_FOR_THREAD_STOP_THEN_START
                        if (Process.IsTimeup)
                        {
                            if (is_thread_running())
                            {
                                stop_scan_thread();
                                SetNextState(6, 300);
                                return;
                            }
                            if (m_taskName == "CheckMarks")
                            {
                                SetNextState(300, 300);
                            }
                            else if (m_taskName == "StressTest")
                            {
                                begin_test();
                                SetNextState(10, 300);
                            }
                            else
                            {
                                SetNextState(10, 300);
                            }
                        }
                        #endregion
                        break;


                    #region PHASE_NORMAL_LIVE_AND_STRESS_TEST

                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (is_testing())
                            {
                                //int expo = m_camID==0 ? 140 : 600;    // just for speed testing.
                                //float gain = m_camID==0 ? 0f : 19f;
                                //_LOG($"camID={m_camID}", $"設定曝光時間={expo}, 增益={gain:0.0}");
                                //m_cam.SetExposure(expo);
                                //m_cam.SetGain(gain);
                            }
                            else
                            {
                                // 由 Process 使用者, 負責設定 exposure time.
                            }
                            SetNextState(100);
                        }
                        break;

                    case 100:
                        if (Process.IsTimeup)
                        {
                            _LOG($"camID={m_camID}", "線程 Start");
                            if (is_testing())
                                phase1_init(1000, run_one_step_stress_test);
                            else
                                phase1_init(1000, run_one_step_live);
                            SetNextState(199, 500);//放置线程前面，防止卡在线程中无法终止
                            start_scan_thread(m_phase1);
                            FireMessage(m_taskName + ".ThreadStarted");
                        }
                        break;

                    case 199:
                        // Threading
                        break;

                    case 1000:
                        if (Process.IsTimeup)
                        {
                            _LOG($"camID={m_camID}", "線程 Stop");
                            Stop();
                            FireMessage(m_taskName + ".ThreadStopped");
                            FireCompleted();
                        }
                        break;

                    #endregion


                    #region PHASE_CORETRONICS_MARKS_CHECKING
                    case 30:
                        if (Process.IsTimeup)
                        {
                            //bool ok = run_core_marks_checking();
                            //// Stop();
                            //FireCompleted(new ProcessEventArgs(ok ? "OK" : "NG"));
                            SetNextState(300);
                        }
                        break;
                    case 300:
                        if (Process.IsTimeup)
                        {
                            _LOG($"camID={m_camID}", "線程 Start");
                            phase1_init(3000, run_core_marks_checking);
                            SetNextState(399, 500);//放置线程前面，防止卡在线程中无法终止
                            start_scan_thread(m_phase1);
                            FireMessage(m_taskName + ".ThreadStarted");
                        }
                        break;

                    case 399:
                        // Threading
                        break;

                    case 3000:
                        if (Process.IsTimeup)
                        {
                            Stop();
                            _LOG($"camID={m_camID}", "線程 Stop");
                            // FireMessage(m_taskName + ".ThreadStopped");

                            //>>> 自動重新 Live
                            //>>> m_taskName = "Rcp.LiveImaging";
                            restoreDefaultName();
                            SetNextState(5, 300);
                            base.IsOn = true;
                        }
                        break;
                    #endregion


                    case 9999:
                        #region EXCEPTIONS
                        if (Process.IsTimeup)
                        {
                            _LOG($"camID={m_camID}", "中止!", Color.Purple);
                            Terminate();
                            FireMessage(m_taskName + ".Aborted");
                            break;
                        }
                        #endregion
                        break;
                }
            }
        }

        public void DumpImage(string tag)
        {
            // RESERVED ++
            return;
            // RESERVED --
            m_dumpTag = tag;
        }


        #region LIVE_IMAGE_LOOP
        private volatile string m_dumpTag = null;
        protected override QVector CompensationInitPos
        {
            get { return m_phase1.InitMotorPos; }
        }
        XRunContext m_phase1 = new XRunContext(PHASE_1, 1000);
        XRunContext phase1_init(int exitCode, Action<XRunContext> func)
        {
            //(0) Read Motors Current Position as InitPos 
            //    (純像測不需要)
            m_phase1.InitMotorPos = ax_read_current_pos();
            m_phase1.ExitCode = exitCode;

            //(1) Phase One Step Func
            //////if (is_testing())
            //////    m_phase1.StepFunc = phase1_run_one_step_stress_test;
            //////else
            //////    m_phase1.StepFunc = phase1_run_one_step_capture;
            m_phase1.StepFunc = func;

            // Reset                
            m_phase1.Reset();
            m_timeFps = DateTime.Now;
            return m_phase1;
        }
        void run_one_step_live(XRunContext runCtrl)
        {
            // var runCtrl = m_run2;
            runCtrl.IsCompleted = false;

            //(1) 檢查馬達狀態
            if (!runCtrl.Go)
                return;

            //(2) 拍照
            bool dump = m_dumpTag != null;
            using (Bitmap bmp = snapshot_image(m_cam, m_dumpTag, dump))
            {
                m_dumpTag = null;
                //(2.1) 通知 GUI 更新 Image
                if (runCtrl.Go)
                {
                    FireLiveImaging(bmp);
                }
            }
            
        }
        #endregion


        #region STRESS_TESTING_MEMBERS
        class XTestContext
        {
            public int TestCount = 0;
            public int OldRotate = -1;
            public int Rotate = 0;
            public bool UsingDLL = false;
            public int MirrorIndex = 0;
        }
        XTestContext m_test = null;
        DateTime m_timeFps = DateTime.Now;
        bool is_testing()
        {
            return m_test != null && m_taskName == "StressTest";
        }
        void begin_test()
        {
            if (m_test == null)
                m_test = new XTestContext();

            int nextCamID = m_test.TestCount % 2;
            var nextCam = GetCamera(nextCamID);

            if (m_cam != nextCam || m_test.OldRotate < 0)
            {
                // 紀錄原有的旋轉角度
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
            _LOG("準備測試", $"camID={m_camID}", $"Rotate={m_test.Rotate}", tag, color);

            int expo = 100; // m_camID == 0 ? 140 : 600;    // just for speed testing.
            float gain = m_camID == 0 ? 0f : 19f;
            _LOG($"camID={m_camID}", $"設定曝光時間={expo}, 增益={gain:0.0}");
            m_cam.SetExposure(expo);
            m_cam.SetGain(gain);

            FireCompensatedInfo(null, 0, null, null);   //把圖示清掉
        }
        void end_test()
        {
            if (m_test != null && m_cam != null)
            {
                m_cam.RotateAngle = m_test.OldRotate;
                _LOG($"camID={m_camID}", "恢復原有的旋轉角度", m_cam.RotateAngle);
                restoreDefaultName();
            }
        }
        void run_one_step_stress_test(XRunContext runCtrl)
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
            //bool dump = false;
            //int tag = m_test.TestCount % 6;
            using (Bitmap bmp = snapshot_image(m_cam, null, false))
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
                int NFrames = 10;
                if (runCtrl.RunCount > 0 && (runCtrl.RunCount % NFrames) == 0)
                {
                    var tm1 = DateTime.Now;
                    var ts = tm1 - m_timeFps;
                    var fps = NFrames / ts.TotalSeconds;
                    m_timeFps = tm1;
                    _LOG($"camID={m_camID}", $"rotate={m_test.Rotate}", $"fps={fps:0.0}");

                    if (runCtrl.RunCount / NFrames >= 5)
                    {
                        //自動切換 StressTest
                        runCtrl.RunCount = 0;
                        begin_test();
                    }
                }
            }
        }
        #endregion


        #region CORETROINICS_MARKS_CHECKING
        void run_core_marks_checking(XRunContext runCtrl)
        {
            bool ok = true;
            int maxPixelsDiff = RecipeCHClass.Instance.JudgeCamPixelsDiff;
            using (var bmp = snapshot_image(m_cam, "proj_自動定位點檢", true))
            {
                GdxCore.CheckMarkPoints(bmp);
                var goldPts = GdxCore.getGoldenMarkPoints();
                var algoPts = GdxCore.getAlgoMarkPoints();
                _LOG("定位檢點 goldPts", GdxCore.toString(goldPts));
                _LOG("定位檢點 algoPts", GdxCore.toString(algoPts));
                int N = Math.Min(goldPts.Length, algoPts.Length);
                for (int i = 0; i < N; i++)
                {
                    int dx = goldPts[i].X - algoPts[i].X;
                    int dy = goldPts[i].Y - algoPts[i].Y;
                    if (Math.Abs(dx) > maxPixelsDiff || Math.Abs(dy) > maxPixelsDiff)
                    {
                        ok = false;
                        break;
                    }
                }
                FireMarkPointInfo(Name, bmp, goldPts, algoPts);
            }
            runCtrl.RunCount++;
            runCtrl.IsCompleted = true;

            string result = ok ? "OK" : $"NG\n\r差異超過{maxPixelsDiff}Pixels";
            _LOG("定位檢點", "結果=" + result);
            FireCompleted(new ProcessEventArgs(result));
        }
        #endregion
    }
}
