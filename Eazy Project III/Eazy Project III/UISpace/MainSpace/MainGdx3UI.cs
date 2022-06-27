using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.FormSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_III.ProcessSpace;
using Eazy_Project_III.UISpace.IOSpace;
using Eazy_Project_Interface;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
using JetEazy.DBSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using JzDisplay;
using JzDisplay.UISpace;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.IOSpace;



namespace Eazy_Project_III.UISpace.MainSpace
{
    public partial class MainGdx3UI : UserControl
    {
        VersionEnum VERSION
        {
            get
            {
                return Universal.VERSION;
            }
        }
        OptionEnum OPTION
        {
            get
            {
                return Universal.OPTION;
            }
        }


        #region WINDOW_GUI_CONTROLS

        Button btnStart;
        Button btnStop;
        Button btnReset;

        Button btnClearAlarm;
        Button btnMute;
        Button btnManual_Auto;

        Button btnPickProcess;
        Button btnCalibrateProcess;
        Button btnPutProcess;
        Button btnDispensingProcess;

        Label lblState;
        Label lblAlarm;
        #endregion


        #region JEZ_COMPONENTS
        AccDBClass ACCDB
        {
            get
            {
                return Universal.ACCDB;
            }
        }
        DispUI m_DispUI;
        //IO_INPUTUI _INPUTUI;
        X3INPUTUI _X3INPUTUI;
        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        DispensingMachineClass MACHINE
        {
            get { return (DispensingMachineClass)Universal.MACHINECollection.MACHINE; }
        }
        #endregion


        public MainGdx3UI()
        {
            InitializeComponent();
            SizeChanged += MainX3UI_SizeChanged;
        }
        public void Init()
        {
            init_Display();
            update_Display();

            CommonLogClass.Instance.SetRichTextBox(richTextBox1);
            //CommonLogClass.Instance.LogMessage("系统初始化......", Color.Black);

            //_INPUTUI = iO_INPUTUI1;
            _X3INPUTUI = x3INPUTUI1;

            lblState = label11;
            lblAlarm = label5;

            btnStart = button6;
            btnStop = button4;
            btnReset = button7;

            btnClearAlarm = button8;
            btnMute = button9;
            btnPickProcess = button3;
            btnCalibrateProcess = button5;
            btnPutProcess = button10;
            btnDispensingProcess = button2;

            btnManual_Auto = button1;
            btnManual_Auto.Click += BtnManual_Auto_Click;

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnClearAlarm.Click += BtnClearAlarm_Click;
            btnMute.Click += BtnMute_Click;

            btnReset.Click += BtnReset_Click;
            btnPickProcess.Click += BtnPickProcess_Click;
            btnCalibrateProcess.Click += BtnCalibrateProcess_Click;
            btnPutProcess.Click += BtnPutProcess_Click;
            btnDispensingProcess.Click += BtnDispensingProcess_Click;

#if OPT_LIVE_IMAGE_STRESS_TEST
            btnTestLiveImaging.Click += btnTestLiveImaging_Click;
            btnTestLiveImaging.Visible = true;
#else
            btnTestLiveImaging.Visible = false;
#endif

            //MACHINE.EVENT.Initial(lsbEvent);
            MACHINE.EVENT.Initial(lblAlarm);

            MACHINE.TriggerAction += MACHINE_TriggerAction;
            MACHINE.EVENT.TriggerAlarm += EVENT_TriggerAlarm;

            //_INPUTUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
            _X3INPUTUI.Initial(VERSION, OPTION, MACHINE);

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            //////m_LE_Running = true;
                            //////m_thread_LE = new System.Threading.Thread(new System.Threading.ThreadStart(GetLE));
                            //////m_thread_LE.Start();
                            GdxCore.Init();
                            var laser = GdxCore.GetLaser();
                            laser.OnScanned += Laser_OnScanned;
                            laser.StartAutoScan();
                            break;
                    }

                    break;
            }

            SetNormalLight();

            InitAllProcesses();
            StopAllProcesses("INIT");
        }
        public void Close()
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            //////m_LE_Running = false;
                            //////if (m_thread_LE != null)
                            //////{
                            //////    m_LE_Running = false;
                            //////    m_thread_LE.Abort();
                            //////}
                            GdxCore.GetLaser().StopAutoScan();
                            break;
                    }

                    break;
            }

            SetNormalLight();
        }
        public void Tick()
        {
            //_INPUTUI.Tick();
            _X3INPUTUI.Tick();

            ////ResetTick();
            ////PickTick();
            ////CalibrateTick();
            ////BlackboxTick();
            ////DispensingTick();
            ////MainProcessTick();

            ////m_resetprocess.Tick();
            ////m_pickprocess.Tick();
            ////m_calibrateprocess.Tick();
            ////m_blackboxprocess.Tick();
            ////m_dispensingprocess.Tick();
            ////m_mainprocess.Tick();
            //////> m_BuzzerProcess.Tick();
            
            TickAllProcesses();


            btnManual_Auto.BackColor = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? Color.Red : Color.Lime);
            btnManual_Auto.Text = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? "自动模式" : "手动模式");
            btnManual_Auto.Text = LanguageExClass.Instance.ToTraditionalChinese(btnManual_Auto.Text);

            btnStart.BackColor = (m_mainprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnReset.BackColor = (m_resetprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnPickProcess.BackColor = (m_pickprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnCalibrateProcess.BackColor = (m_calibrateprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnPutProcess.BackColor = (m_blackboxprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnDispensingProcess.BackColor = (m_dispensingprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));

            btnTestLiveImaging.BackColor = (m_testLiveImageProcess.IsOn ? Color.Red : Color.FromArgb(255, 224, 192));

            AlarmUITick();
        }


        #region WINDOW_EVENT_HANDLERS

        private void MainX3UI_SizeChanged(object sender, EventArgs e)
        {
            _auto_layout();
        }
        private void BtnMute_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.ADR_BUZZER = false;
        }
        private void BtnClearAlarm_Click(object sender, EventArgs e)
        {
            string onStrMsg = "請檢查警報是否清除？";
            string offStrMsg = "請檢查警報是否清除？";
            string msg = (true ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                MACHINE.ClearAlarm = true;
                MACHINE.EVENT.RemoveAlarm();
            }
        }
        private void BtnStop_Click(object sender, EventArgs e)
        {
            string onStrMsg = "是否停止？";
            string offStrMsg = "是否停止？";
            string msg = (true ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                StopAllProcesses("USERSTOP");
            }

        }

        frmUserSelect myUserSelectForm = null;

        private void BtnStart_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }

            string onStrMsg = "是否要启动？";
            string offStrMsg = "是否要停止？";
            string msg = (m_mainprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_mainprocess.IsOn)
                {
                    myUserSelectForm = new frmUserSelect();
                    if (myUserSelectForm.ShowDialog() == DialogResult.OK)
                    {
                        MainGroupIndex = myUserSelectForm.GetIndex;
                        MainMirrorIndex = myUserSelectForm.PutIndex;
                        MainAloneToMirror = myUserSelectForm.IsAloneToMirror;

                        m_mainprocess.Start();
                    }
                }
                else
                    StopAllProcesses("USERSTOP");
            }
        }
        private void BtnPickProcess_Click(object sender, EventArgs e)
        {
            ////MACHINE.PLCIO.SetIO(IOConstClass.QB1625, true);
            //MACHINE.PLCIO.SetIO(IOConstClass.QB1665, true);
            //return;

            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法吸料，请检查。");
                return;
            }

            string onStrMsg = "是否要进行吸料测试？";
            string offStrMsg = "是否要停止吸料测试流程？";
            string msg = (m_pickprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_pickprocess.IsOn)
                {

                    myUserSelectForm = new frmUserSelect();
                    if (myUserSelectForm.ShowDialog() == DialogResult.OK)
                    {
                        MainGroupIndex = myUserSelectForm.GetIndex;
                        MainMirrorIndex = myUserSelectForm.PutIndex;

                        m_pickprocess.Start(MainMirrorIndex.ToString());
                    }
                }
                else
                    m_pickprocess.Stop();
            }
        }
        private void BtnCalibrateProcess_Click(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.SetIO(IOConstClass.QB1625, true);
            ////MACHINE.PLCIO.SetIO(IOConstClass.QB1665, true);
            //return;

            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法校正，请检查。");
                return;
            }

            string onStrMsg = "是否要进行校正测试？";
            string offStrMsg = "是否要停止校正测试流程？";
            string msg = (m_calibrateprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_calibrateprocess.IsOn)
                    m_calibrateprocess.Start(MainMirrorIndex, true);
                else
                    m_calibrateprocess.Stop();
            }
        }
        private void BtnPutProcess_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法放料，请检查。");
                return;
            }

            string onStrMsg = "是否要进行放料测试？";
            string offStrMsg = "是否要停止放料测试流程？";
            string msg = (m_blackboxprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_blackboxprocess.IsOn)
                    m_blackboxprocess.Start(MainMirrorIndex, true);
                else
                    m_blackboxprocess.Stop();
            }
        }
        private void BtnDispensingProcess_Click(object sender, EventArgs e)
        {

            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法点胶，请检查。");
                return;
            }

            string onStrMsg = "是否要进行点胶测试？";
            string offStrMsg = "是否要停止点胶测试流程？";
            string msg = (m_dispensingprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_dispensingprocess.IsOn)
                    m_dispensingprocess.Start(MainMirrorIndex.ToString());
                else
                    m_dispensingprocess.Stop();
            }
        }
        private void BtnManual_Auto_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1090, MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? 0 : 1);
        }
        private void BtnReset_Click(object sender, EventArgs e)
        {

            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1)
            {
                VsMSG.Instance.Warning("自动模式下，无法复位，请检查。");
                return;
            }

            //判断吸嘴是否有料
            string IX0_6 = "0:IX0.6";//輸入點位6
            //if (MACHINE.PLCIO.GetIO(IX0_6))
            if (MACHINE.PLCIO.GetInputIndex(6))
            {
                VsMSG.Instance.Warning("請先取走鏡片，再復位。");
                return;
            }

            string onStrMsg = "是否要进行复位？";
            string offStrMsg = "是否要停止复位流程？";
            string msg = (m_resetprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_resetprocess.IsOn)
                    m_resetprocess.Start();
                else
                    m_resetprocess.Stop();
            }
        }
        private void btnTestLiveImaging_Click(object sender, EventArgs e)
        {
            var ts = m_testLiveImageProcess;
            if (ts.IsOn)
            {
                ts.Stop();
                System.Threading.Thread.Sleep(100);
            }
            else
            {
                int camID = 0;
                var tag = btnTestLiveImaging.Tag;
                if (tag != null)
                    int.TryParse(tag.ToString(), out camID);
                camID = (camID + 1) % 2;
                btnTestLiveImaging.Tag = camID;
                ts.Start(camID);
                System.Threading.Thread.Sleep(100);
            }
        }

        #endregion


        #region HARDWARE_EVENT_HANDLERS
        private void Laser_OnScanned(object sender, double e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    Laser_OnScanned(sender, e);
                }));
            }
            else
            {
                lblLEText.Text = "LE : " + e.ToString("0.000") + " mm";
            }
        }
        #endregion


        #region ALARM & UI 

        private void EVENT_TriggerAlarm(bool IsBuzzer)
        {
            MACHINE.PLCIO.ADR_BUZZER = IsBuzzer;
            if (!IsBuzzer)
            {
                SetNormalLight();
            }
        }

        bool IsEMCTriggered = false;

        private void MACHINE_TriggerAction(MachineEventEnum machineevent)
        {
            switch (machineevent)
            {
                case MachineEventEnum.ALARM_SERIOUS:
                    IsAlarmsSeriousX = true;
                    //SetAbnormalLight();
                    break;
                case MachineEventEnum.ALARM_COMMON:
                    IsAlarmsCommonX = true;
                    //SetAbnormalLight();
                    break;
                case MachineEventEnum.EMC:
                    IsEMCTriggered = true;
                    break;
            }
        }

        public void AlarmUITick()
        {

            #region ALARM

            if (IsAlarmsSeriousX)
            {
                SetAbnormalLight();

                IsAlarmsSeriousX = false;
                StopAllProcesses();
                //SetSeriousAlarms0();
                SetSeriousAlarms1();

                //StopAllProcess();
            }

            if (IsAlarmsCommonX)
            {
                SetAbnormalLight();

                IsAlarmsCommonX = false;
                StopAllProcesses();
                SetCommonAlarms();

            }

            if (!Universal.IsNoUseIO)
            {
                if (IsEMCTriggered)
                {
                    SetAbnormalLight();

                    IsEMCTriggered = false;
                    StopAllProcesses();
                    //OnTrigger(ActionEnum.ACT_ISEMC, "");
                }
            }

            #endregion

            UpdateStateUI();
            m_BuzzerProcess.Tick();
        }

        #region ALARMS

        bool IsAlarmsSeriousX = false;
        bool IsAlarmsCommonX = false;

        //void SetSeriousAlarms0()
        //{
        //    foreach (PLCAlarmsItemDescriptionClass item in MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS0].PLCALARMSDESCLIST)
        //    {
        //        if (MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
        //        {
        //            MACHINE.EVENT.GenEvent("A0001", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.DataNow);
        //        }
        //    }
        //}
        void SetSeriousAlarms1()
        {
            foreach (PLCAlarmsItemDescriptionClass item in MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCALARMSDESCLIST)
            {
                if (MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
                {
                    MACHINE.EVENT.GenEvent("A0001", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.AccNow);
                }
            }
        }
        void SetCommonAlarms()
        {
            foreach (PLCAlarmsItemDescriptionClass item in MACHINE.PLCIO.PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCALARMSDESCLIST)
            {
                //if(item.ADR_Address =="MX4.0")
                //{
                //    Console.WriteLine("DEBUG");
                //}

                if (MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
                {
                    MACHINE.EVENT.GenEvent("A0002", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.AccNow);
                }
            }
        }

        void SetNormalLight()
        {
            MACHINE.PLCIO.ADR_RED = false;
            MACHINE.PLCIO.ADR_YELLOW = true;
            MACHINE.PLCIO.ADR_GREEN = false;
        }
        void SetAbnormalLight()
        {
            MACHINE.PLCIO.ADR_RED = true;
            MACHINE.PLCIO.ADR_YELLOW = false;
            MACHINE.PLCIO.ADR_GREEN = false;
        }
        void SetRunningLight()
        {
            MACHINE.PLCIO.ADR_RED = false;
            MACHINE.PLCIO.ADR_YELLOW = false;
            MACHINE.PLCIO.ADR_GREEN = true;
        }
        //void SetBuzzer(bool IsON)
        //{
        //    MACHINE.PLCIO.ADR_BUZZER = IsON;
        //}

        #endregion


        private void UpdateStateUI()
        {
            //if (m_TestProcess.IsOn)
            //    lblState.Text = "执行-测试区取像中 " + m_TestProcess.ID.ToString();
            if (m_dispensingprocess.IsOn)
                lblState.Text = "执行-点胶中 " + m_dispensingprocess.ID.ToString();
            else if (m_blackboxprocess.IsOn)
                lblState.Text = "执行-放取校正中  " + m_blackboxprocess.ID.ToString();
            else if (m_calibrateprocess.IsOn)
                lblState.Text = "执行-校正中 " + m_calibrateprocess.ID.ToString();
            else if (m_pickprocess.IsOn)
                lblState.Text = "执行-拾取中 " + m_pickprocess.ID.ToString();
            else if (m_mainprocess.IsOn)
                lblState.Text = "跑线中 " + m_mainprocess.ID.ToString();
            //else if (resetpartialprocess.IsOn)
            //    lblState.Text = "小复位中 " + resetpartialprocess.ID.ToString();
            else if (m_resetprocess.IsOn)
                lblState.Text = "复位中 " + m_resetprocess.ID.ToString();
            else
                lblState.Text = "待机";

            if (MACHINE.PLCIO.ADR_ISEMC)
            {
                lblState.Text = "急停中";
                lblState.BackColor = Color.Red;
            }
            else if (MACHINE.PLCIO.ADR_ISSCREEN)
            {
                lblState.Text = "光幕遮挡";
                lblState.BackColor = Color.Red;
            }
            else
            {
                lblState.BackColor = Color.Black;
            }

            lblState.Text = LanguageExClass.Instance.ToTraditionalChinese(lblState.Text);
        }

        #endregion


        #region RESERVED_CODE
        //private void M_DispUI_AdjustAction(PointF ptfoffset)
        //{
        //    CCDCollection.RestoreList();
        //    //lblDebug.Text = ptfoffset.ToString() + Environment.NewLine + CCDCollection.GetRectRelateData() + Environment.NewLine;

        //    CCDCollection.MoveCCDRectRelateIndex(new Point((int)ptfoffset.X, (int)ptfoffset.Y), MoveString);
        //    m_DispUI.ReplaceDisplayImage(CCDCollection.bmpAll);

        //    //lblDebug.Text += CCDCollection.GetRectRelateData();
        //    //lblDebug.Invalidate();
        //}

        //private void M_DispUI_MoverAction(MoverOpEnum moverop, string opstring)
        //{
        //    switch (moverop)
        //    {
        //        case MoverOpEnum.READYTOMOVE:

        //            CCDCollection.BackupList();

        //            //lblDebug2.Text = CCDCollection.GetRectRelateData();

        //            break;
        //    }
        //}
        #endregion

        void init_Display()
        {
            m_DispUI = dispUI1;
            m_DispUI.Initial(100, 0.01f);
            m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);

            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;
        }
        void update_Display()
        {
            m_DispUI.Refresh();
            m_DispUI.DefaultView();
        }


        #region PROCESSES_For_STATION_3
        /// <summary>
        /// 记录操作 Mirror0 还是 Mirror1 即左边还是右边
        /// </summary>
        int MainMirrorIndex
        {
            get { return ((MainProcess)m_mainprocess).MainMirrorIndex; }
            set { ((MainProcess)m_mainprocess).MainMirrorIndex = value; }
        }
        /// <summary>
        /// 拾取第几组 总共4组  从0开始
        /// </summary>
        int MainGroupIndex
        {
            get { return ((MainProcess)m_mainprocess).MainGroupIndex; }
            set { ((MainProcess)m_mainprocess).MainGroupIndex = value; }
        }
        /// <summary>
        /// 单独制作一个Mirror
        /// </summary>
        bool MainAloneToMirror
        {
            get { return ((MainProcess)m_mainprocess).MainAloneToMirror; }
            set { ((MainProcess)m_mainprocess).MainAloneToMirror = value; }
        }
        MainProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        BaseProcess m_resetprocess
        {
            get { return ResetProcess.Instance; }
        }
        BaseProcess m_pickprocess
        {
            get
            {
                return MirrorPickProcess.Instance;
            }
        }
        BaseProcess m_calibrateprocess
        {
            get
            {
                return MirrorCalibProcess.Instance;
            }
        }
        BaseProcess m_blackboxprocess
        {
            get
            {
                return MirrorBlackboxProcess.Instance;
            }
        }
        BaseProcess m_dispensingprocess
        {
            get
            {
                return MirrorDispenseProcess.Instance;
            }
        }
        BaseProcess m_testLiveImageProcess
        {
            get { return TestLiveImageProcess.Instance; }
        }
        #endregion


        void StopAllProcesses(string reason = "")
        {
            m_mainprocess.Stop();
            m_pickprocess.Stop();
            m_calibrateprocess.Stop();
            m_blackboxprocess.Stop();
            m_dispensingprocess.Stop();
            m_resetprocess.Stop();
            m_BuzzerProcess.Stop();
            m_testLiveImageProcess.Stop();

            switch (reason)
            {
                case "INIT":
                    MACHINE.PLCIO.CLEARALARMS = true;
                    break;
                case "USERSTOP":
                    SetNormalLight();
                    break;
            }

        }
        void InitAllProcesses()
        {
            m_mainprocess.OnCompleted += process_OnCompleted;
            // Buzzer 的 結束 似乎不需要特別關注.
            // m_BuzzerProcess.OnCompleted += process_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            m_pickprocess.OnCompleted += process_OnCompleted;
            m_calibrateprocess.OnCompleted += process_OnCompleted;
            m_blackboxprocess.OnCompleted += process_OnCompleted;
            m_dispensingprocess.OnCompleted += process_OnCompleted;
            // Calib and BlackBox
            m_calibrateprocess.OnMessage += calibrateProcess_OnMessage;
            m_blackboxprocess.OnMessage += blackboxProcess_OnMessage;
            ((MirrorCalibProcess)m_calibrateprocess).OnLiveImage += process_OnLiveImage;
            ((MirrorBlackboxProcess)m_blackboxprocess).OnLiveImage += process_OnLiveImage;
            ((MirrorCalibProcess)m_calibrateprocess).OnLiveCompensating += process_OnLiveCompensating;
            ((MirrorBlackboxProcess)m_blackboxprocess).OnLiveCompensating += process_OnLiveCompensating;

            //TEST
#if OPT_LIVE_IMAGE_STRESS_TEST
            ((TestLiveImageProcess)m_testLiveImageProcess).OnLiveImage += process_OnLiveImage;
#endif
        }
        void TickAllProcesses()
        {
            m_resetprocess.Tick();
            m_pickprocess.Tick();
            m_calibrateprocess.Tick();
            m_blackboxprocess.Tick();
            m_dispensingprocess.Tick();
            m_mainprocess.Tick();
            //> m_BuzzerProcess.Tick();
            m_testLiveImageProcess.Tick();
        }


        #region PROCESS_EVENT_HANSLERS
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            check_coretronic_version();
        }
        private void process_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (sender == m_resetprocess)
            {
                if (!check_coretronic_version())
                {
                    // 此處異常檢查
                    // 可考慮放入 ResetProcess 內部處理
                    // 或設計一個 AlarmManager 集中管理.
                    // Buzzer and m_mainprocess.Stop()
                    // 就不用 流出到 GUI 這一層了
                    m_BuzzerProcess.Start(1);   // 叫一聲
                    m_mainprocess.Stop();       // 中斷主流程
                    return;
                }
            }

            try
            {
                // Do whatever message you want to show to the operators.
                string msg = "程序 " + ((BaseProcess)sender).Name + "\n已完成!\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void process_OnLiveImage(object sender, ProcessEventArgs e)
        {
            if (e.Tag != null && e.Tag is Bitmap)
            {
                try
                {
                    //@LETIAN: 2022/06/20 (for backgroud thread)
                    // bmp 由 sender maintains life cycle.
                    // 由於 DispUI 內部會另行複製 bmp
                    // 所以 在此 Handler Function 內不用 Dispose()
                    if (InvokeRequired)
                    {
                        EventHandler<ProcessEventArgs> h = process_OnLiveImage;
                        this.Invoke(h, sender, e);
                    }
                    else
                    {
                        var bmp = (Bitmap)e.Tag;
                        var bmpShow = new Bitmap(bmp);
                        m_DispUI.SetDisplayImage(bmpShow);
                        bmp.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    GdxGlobal.LOG.Error(ex, "OnLiveImage");
                }
            }
        }
        private void calibrateProcess_OnMessage(object sender, ProcessEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message))
                return;

            if(e.Message.StartsWith("NG"))
            {
                // 中心點偏移量過大
                string err = e.Message;
                CommonLogClass.Instance.LogMessage(err, Color.Red);
                m_BuzzerProcess.Start(1);   // 叫一聲
            }
            else
            {

            }
        }
        private void blackboxProcess_OnMessage(object sender, ProcessEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message))
                return;

            if (e.Message.StartsWith("NG"))
            {
                // 投影補償失敗
                string err = e.Message;
                CommonLogClass.Instance.LogMessage(err, Color.Red);
                m_BuzzerProcess.Start(1);   // 叫一聲
            }
            else
            {

            }
        }
        private void process_OnLiveCompensating(object sender, CompensatingEventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    EventHandler<CompensatingEventArgs> h = process_OnLiveCompensating;
                    this.Invoke(h, sender, e);
                }
                else
                {
                    // @LETIAN: 20220623 Async GUI interaction (new)
                    // e.GoControlByClient = new ManualResetEvent(false);
                    var frm = new FormCompensationStepTracer(e)
                    {
                        //Tag = e,
                        TopMost = true
                    };
                    frm.FormClosed += new FormClosedEventHandler((s2, e2) =>
                    {
                        //>>> DialogResult ret = ((Form)s2).DialogResult;
                        //>>> var blackBoxEventArgs = (ProcessEventArgs)((Form)s2).Tag;
                        //>>> blackBoxEventArgs.Cancel = (ret != DialogResult.OK);
                        //>>> blackBoxEventArgs.GoControlByClient.Set();
                        frm.Dispose();
                    });
                    frm.Show(this);
                    frm.Location = new Point(100, 100);
                }
            }
            catch(Exception ex)
            {
                e.Cancel = true;
            }
        }
        private bool check_coretronic_version()
        {
            // 檢查中光電 Version and UpdateParams
            // 將來看, 此檢查 是否歸屬於 ResetProcess
            // 再將此段程序 移入.
            // 用 event 通知 GUI 顯示異常.
            string version = GdxCore.GetDllVersion();
            CommonLogClass.Instance.LogMessage("中光電 DLL Version = " + version, Color.Blue);

            bool ok = GdxCore.UpdateParams();
            string msg = "中光電 DLL UpdateParams() = " + ok;
            CommonLogClass.Instance.LogMessage(msg, ok ? Color.Green : Color.Red);

            if (!ok)
            {
                VsMSG.Instance.Warning(msg.Replace("=", "\n\r"));
            }
            return ok;
        }
        #endregion


        #region AUTO_LAYOUT_FUNCTIONS
        void _auto_layout()
        {
#if !OPT_LETIAN_DEBUG
            return;
#endif
            //@LETIAN: auto layout gui component's location and size
            //  暫時可以塞到我的小螢幕.
            //  以後再細改.
            if (FindForm().WindowState == FormWindowState.Minimized)
                return;

            _auto_adjust_bottom_panels();
            _auto_adjust_disp_ui();

            lblLEText.Top = dispUI1.Top;
            lblLEText.Left = dispUI1.Right - lblLEText.Width;
        }
        void _auto_adjust_bottom_panels()
        {
            var rcc = ClientRectangle;
            int h = tabControl1.Height;
            tabControl1.Left = rcc.Width - tabControl1.Width - 5;
            tabControl1.Top = rcc.Height - h;
            groupBox1.Top = rcc.Height - h;
            groupBox1.Height = h;
            groupBox1.Width = rcc.Width - tabControl1.Width - 5;
        }
        void _auto_adjust_disp_ui()
        {
            var rcc = ClientRectangle;
            var bottomPanel = tabControl1;
            dispUI1.Height = rcc.Height - bottomPanel.Height - 5;
            dispUI1.Width = rcc.Width;
            foreach (Control c in dispUI1.Controls)
            {
                c.Width = rcc.Width;
                if(c is PictureBox)
                {
                    c.Height = dispUI1.Bottom - c.Top;
                }
            }
        }
        #endregion

    }
}
