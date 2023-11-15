using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.FormSpace;
using Eazy_Project_III.ProcessSpace;
using Eazy_Project_III.UISpace.IOSpace;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using System;
using System.Drawing;
using System.IO;
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
        Control runUI = null;
        Control txtBarcode = null;
        Control lblPassSign = null;
        Control lblProductionRunTime = null;

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
        Button btnDispeningManual;
        #endregion


        #region JEZ_COMPONENTS
        AccDBClass ACCDB
        {
            get
            {
                return Universal.ACCDB;
            }
        }
        //DispUI m_DispUI;
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

        bool[] m_plcCommError;
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
            btnDispeningManual = button11;

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
            btnDispeningManual.Click += BtnDispeningManual_Click;

#if (OPT_LIVE_IMAGE_STRESS_TEST)
            btnTestLiveImaging.Click += btnTestLiveImaging_Click;
            btnTestLiveImaging.Visible = true;
#else
            btnTestLiveImaging.Visible = false;
#endif

            //MACHINE.EVENT.Initial(lsbEvent);
            MACHINE.EVENT.Initial(lblAlarm);

            MACHINE.TriggerAction += MACHINE_TriggerAction;
            MACHINE.EVENT.TriggerAlarm += EVENT_TriggerAlarm;
            MACHINE.MachineCommErrorStringAction += MACHINE_MachineCommErrorStringAction;
            //_INPUTUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
            _X3INPUTUI.Initial(VERSION, OPTION, MACHINE);

            m_plcCommError = new bool[MACHINE.PLCCollection.Length];
            int i = 0;
            while (i < MACHINE.PLCCollection.Length)
            {
                m_plcCommError[i] = false;
                i++;
            }

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
            btnTestLiveImaging.BackColor = (m_liveImageProcess.IsOn ? Color.Red : Color.FromArgb(255, 224, 192));
            if (runUI != null)
                runUI.Enabled = !m_mainprocess.IsOn;

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

            //判斷是否復位完成
            if (!MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE))
            {
                VsMSG.Instance.Warning("設備未復位，無法啓動，請復位。");
                return;
            }

            string onStrMsg = "是否要启动？";
            string offStrMsg = "是否要停止？";
            string msg = (m_mainprocess.IsOn ? offStrMsg : onStrMsg);
            if (m_mainprocess.IsOn)
                if (VsMSG.Instance.Question(msg) != DialogResult.OK)
                    return;

            if (true)
            {
                if (!m_mainprocess.IsOn)
                {
                    var barcode = txtBarcode.Text.Trim();
                    if (string.IsNullOrEmpty(barcode))
                    {
                        VsMSG.Instance.Warning("請輸入條碼!");
                        _sim_auto_barcode();
                        return;
                    }

                    myUserSelectForm = new frmUserSelect();
                    if (myUserSelectForm.ShowDialog() == DialogResult.OK)
                    {
                        MainGroupIndex = myUserSelectForm.GetIndex;
                        MainMirrorIndex = myUserSelectForm.PutIndex;
                        MainAloneToMirror = myUserSelectForm.IsAloneToMirror;
                        m_mainprocess.Barcode = txtBarcode.Text.Trim();
                        m_mainprocess.Start();
                    }
                }
                else
                {
                    StopAllProcesses("USERSTOP");
                }
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

            //判斷是否復位完成
            if (!MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE))
            {
                VsMSG.Instance.Warning("設備未復位，無法啓動，請復位。");
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

            //判斷是否復位完成
            if (!MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE))
            {
                VsMSG.Instance.Warning("設備未復位，無法啓動，請復位。");
                return;
            }

            string onStrMsg = "是否要进行校正测试？";
            string offStrMsg = "是否要停止校正测试流程？";
            string msg = (m_calibrateprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_calibrateprocess.IsOn)
                {
                    m_mainprocess.Barcode = "DEBUG";
                    m_calibrateprocess.Start(MainMirrorIndex, true);
                }
                else
                {
                    m_calibrateprocess.Stop();
                }
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
            //判斷是否復位完成
            if (!MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE))
            {
                VsMSG.Instance.Warning("設備未復位，無法啓動，請復位。");
                return;
            }

            string onStrMsg = "是否要进行放料测试？";
            string offStrMsg = "是否要停止放料测试流程？";
            string msg = (m_blackboxprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_blackboxprocess.IsOn)
                {
                    m_mainprocess.Barcode = "DEBUG";
                    m_blackboxprocess.Start(MainMirrorIndex, true);
                }
                else
                {
                    m_blackboxprocess.Stop();
                }
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
            //判斷是否復位完成
            if (!MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE))
            {
                VsMSG.Instance.Warning("設備未復位，無法啓動，請復位。");
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
            int action = MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? 0 : 1;
            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1090, action);
        }
        private void BtnReset_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1)
            {
                VsMSG.Instance.Warning("自动模式下，无法复位，请检查。");
                return;
            }

            //>>>no check 20220719
            ////判断吸嘴是否有料
            //string IX0_6 = "0:IX0.6";//輸入點位6
            ////if (MACHINE.PLCIO.GetIO(IX0_6))
            //if (MACHINE.PLCIO.GetInputIndex(6))
            //{
            //    VsMSG.Instance.Warning("請先取走鏡片，再復位。");
            //    return;
            //}

            //if(check_have_mirror())
            //{
            //    return;
            //}

            string onStrMsg = "是否要进行复位？";
            string offStrMsg = "是否要停止复位流程？";
            string msg = (m_resetprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_resetprocess.IsOn)
                {
                    if (m_mainprocess.LastNG != null)
                        StopAllProcesses("RESET");
                    m_resetprocess.Start();
                }
                else
                {
                    m_resetprocess.Stop();
                }
            }
        }
        private void btnTestLiveImaging_Click(object sender, EventArgs e)
        {
            var ps = m_liveImageProcess;
            if (ps.IsOn)
            {
                ps.Stop();
                System.Threading.Thread.Sleep(100);
            }
            else
            {
                //int camID = 0;
                //var tag = btnTestLiveImaging.Tag;
                //if (tag != null)
                //    int.TryParse(tag.ToString(), out camID);
                //camID = (camID + 1) % 2;
                //btnTestLiveImaging.Tag = camID;
                //ts.Start(camID);
                //System.Threading.Thread.Sleep(100);
                ps.Start("StressTest");
            }
        }
        private void BtnDispeningManual_Click(object sender, EventArgs e)
        {
            int delaytime = 1;
            bool bOK = int.TryParse(comboBox1.Text, out delaytime);
            if (bOK)
            {
                string msg = "手動出膠 時間 " + delaytime.ToString() + " 毫秒";
                if (VsMSG.Instance.Question(msg) == DialogResult.OK)
                {
                    DispensingMs(delaytime);
                }
            }
        }
        private void DispensingMs(int itime)
        {
            if (!MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_SWITCH_DISPENSING))
            {
                System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() =>
                {
                    MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_SWITCH_DISPENSING, true);
                    System.Threading.Thread.Sleep(itime);
                    MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_SWITCH_DISPENSING, false);
                });
                task.Start();
            }
        }

        #endregion


        #region HARDWARE_EVENT_HANDLERS
        private void MACHINE_MachineCommErrorStringAction(string str)
        {
            //輸出那個plc掉綫
            int index = 0;
            string _plcIndex = str.Replace("PLC", "");
            bool bOK = int.TryParse(_plcIndex, out index);
            string _errorStr = "plc通訊中斷!!!\r\n(編號Index=" + index.ToString() + ")\r\n是否重連?";
            //先停掉流程
            StopAllProcesses("PlcCommError");
            if (!m_plcCommError[index])
            {
                m_plcCommError[index] = true;
                if (VsMSG.Instance.Question(_errorStr) == DialogResult.OK)
                {
                    //重連
                    MACHINE.PLCCollection[index].RetryConn();
                    m_plcCommError[index] = false;
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }
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
            if (Universal.IsSilentMode)
                MACHINE.PLCIO.ADR_BUZZER = false;
            else
                MACHINE.PLCIO.ADR_BUZZER = IsBuzzer;

            if (!IsBuzzer)
            {
                SetNormalLight();
            }
        }

        bool IsEMCTriggered = false;
        bool IsSCREENTriggered = false;

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
                case MachineEventEnum.CURTAIN:
                    IsSCREENTriggered = true;
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
                StopAllProcesses("ALM.S");
                //SetSeriousAlarms0();
                //SetSeriousAlarms1();

                //StopAllProcess();
            }

            if (IsAlarmsCommonX)
            {
                SetAbnormalLight();

                IsAlarmsCommonX = false;
                StopAllProcesses("ALM.C");
                //SetCommonAlarms();

            }

            if (!Universal.IsNoUseIO)
            {
                if (IsEMCTriggered)
                {
                    SetAbnormalLight();

                    IsEMCTriggered = false;
                    StopAllProcesses("EMC");
                    //OnTrigger(ActionEnum.ACT_ISEMC, "");
                }
                if (IsSCREENTriggered)
                {
                    //SetAbnormalLight();

                    IsSCREENTriggered = false;
                    if (!MACHINE.PLCIO.ADR_BYPASS_SCREEN)
                        StopAllProcesses("SCREEN");
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

                switch (item.ADR_Address)
                {
                    case "IX0.0"://急停被按下
                    case "IX1.1"://光幕被遮擋

                        if (!MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
                        {
                            MACHINE.EVENT.GenEvent("A0002", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.AccNow);
                        }

                        break;
                    default:
                        if (MACHINE.PLCIO.GetAlarmsAddress(item.BitNo, item.ADR_Address))
                        {
                            MACHINE.EVENT.GenEvent("A0002", EventActionTypeEnum.AUTOMATIC, item.ADR_Chinese, ACCDB.AccNow);
                        }
                        break;
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
            string ngMsg = null;

            //if (m_TestProcess.IsOn)
            //    lblState.Text = "执行-测试区取像中 " + m_TestProcess.ID.ToString();
            if (MACHINE.PLCIO.IntAlarmsCommon != 0 || MACHINE.PLCIO.IntAlarmsSerious != 0)
            {
                lblState.Text = $"报警中 [{MACHINE.PLCIO.IntAlarmsCommon}]" +
                    $" [{MACHINE.PLCIO.IntAlarmsSerious}]";
                if (MACHINE.IsClearAlarmCache)
                    return;
                if (MACHINE.PLCIO.IntAlarmsCommon != 0)
                {
                    SetCommonAlarms();
                }
                if (MACHINE.PLCIO.IntAlarmsSerious != 0)
                {
                    //lblState.Text = "严重报警中";
                    SetSeriousAlarms1();
                }
            }
            else if (m_dispensingprocess.IsOn)
                lblState.Text = "执行-点胶中 " + m_dispensingprocess.ID.ToString();
            else if (m_blackboxprocess.IsOn)
            {
                ngMsg = m_mainprocess.LastNG;
                lblState.Text = "执行-放取校正中  " + m_blackboxprocess.ID.ToString();
            }
            else if (m_calibrateprocess.IsOn)
            {
                ngMsg = m_mainprocess.LastNG;
                lblState.Text = "执行-校正中 " + m_calibrateprocess.ID.ToString();
            }
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
                if (!MACHINE.PLCIO.ADR_BYPASS_SCREEN)
                {
                    lblState.Text = "光幕遮挡";
                    lblState.BackColor = Color.Red;
                }
            }
            else
            {
                if (ngMsg != null)
                {
                    lblState.Text = ngMsg; // "NG: " + lblState.Text;
                    lblState.BackColor = Color.Red;
                }
                else
                {
                    lblState.BackColor = Color.Black;
                }
            }

            lblState.Text = LanguageExClass.Instance.ToTraditionalChinese(lblState.Text);

            _updateProductionRunTime();
            _updateButtonsStatus();
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


        #region RESERVED_CODE
        void init_Display()
        {
            //m_DispUI = dispUI1;
            //m_DispUI.Initial(100, 0.01f);
            //m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);
            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;
        }
        void update_Display()
        {
            //m_DispUI.Refresh();
            //m_DispUI.DefaultView();
        }
        #endregion


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
        BaseProcess m_liveImageProcess
        {
            get { return RcpLiveImageProcess.Instance; }
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
            //m_BuzzerProcess.Stop();

            switch (reason)
            {
                case "INIT":
                    MACHINE.PLCIO.CLEARALARMS = true;
                    break;
                case "USERSTOP":
                    m_liveImageProcess.Stop();
                    SetNormalLight();

                    m_BuzzerProcess.Stop();

                    MACHINE.PLCIO.ADR_STOP_PLC_SIGN = true;
                    break;
                default:
                    break;
            }

            set_cooling_module(false);
            close_projector_light();

            CommonLogClass.Instance.LogMessage($"Mode:{reason}", Color.Black);
        }
        void InitAllProcesses()
        {
            //----------------------------------------------------------------
            // (1) 大部的 Processes 應該可以當成 MainProcess 的 Child Process,
            //      可以集中由 MainProcess 管理, 形成一體 Model.
            // (2) 以下對 Process Event Handler 的掛載.
            //      在 Model-View-Control 的架構規範下, 屬於 Control.
            //      ~ 以後再從 GUI(MainGdx3UI) 抽離出來.
            //----------------------------------------------------------------
            m_mainprocess.OnCompleted += process_OnCompleted;
            // Buzzer 的結束 用來檢視是否有 NG 發生.
            m_BuzzerProcess.OnCompleted += buzzer_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            m_pickprocess.OnCompleted += process_OnCompleted;
            m_calibrateprocess.OnCompleted += process_OnCompleted;
            m_blackboxprocess.OnCompleted += process_OnCompleted;
            m_dispensingprocess.OnCompleted += process_OnCompleted;

            //----------------------------------------------------------------
            m_pickprocess.OnMessage += pickprocess_OnMessage;
            m_dispensingprocess.OnMessage += dispensingprocess_OnMessage;

            //----------------------------------------------------------------
            // Calib, BlackBox, LiveImage Process 都是與影像有關的,
            // 行為相似, 共同繼承自 MirrorAbsImageProcess,
            // 可以一起處理 !
            //----------------------------------------------------------------
            var imgProcesses = new MirrorAbsImageProcess[]
            {
                (MirrorAbsImageProcess)m_calibrateprocess,
                (MirrorAbsImageProcess)m_blackboxprocess,
                (MirrorAbsImageProcess)m_liveImageProcess,  // liveImageProcess 納入正式成員
            };
            foreach (var ps in imgProcesses)
            {
                // 通用型事件
                ps.OnNG += process_OnNG;
                ps.OnMessage += process_OnMessage;
                // 即時影像 Event
                ps.OnLiveImage += process_OnLiveImage;
                // 中光電補償運算中, 單步調試的互動式 Event
                ps.OnLiveCompensating += process_OnLiveCompensating;
                // 中光電補償運算後, 圖點結果 Event.
                // 掛上此 Handler 後, OnLiveImage 的功能會被替代.
                // 直接在此 Handler 同時處理 LiveImaging
                ps.OnCompensatedInfo += process_OnCompensatedInfo;
            }
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
            m_liveImageProcess.Tick();
        }


        #region PROCESS_EVENT_HANSLERS
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            check_coretronic_version();
            _intercept_UI();
        }
        private void process_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (sender == m_resetprocess)
            {
                if(m_resetprocess.RelateString == "CloseWindows")
                {
                    //執行的關閉流程 這裏則跳出
                    return;
                }

                if (m_mainprocess.LastNG != null)
                {
                    clear_NG();
                }

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
                if (check_have_mirror())
                {
                    return;
                }
            }

            try
            {
                if (sender == m_mainprocess)
                {
                    handle_main_process_completed(sender, e);
                    if (e.Message == "PartialCompleted")
                        return;
                }

                // Do whatever message you want to show to the operators.
                string msg = $"程序 {((BaseProcess)sender).Name}, 已完成!\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void process_OnMessage(object sender, ProcessEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message))
                return;

            if (e.Message.StartsWith("NG"))
            {
                // For backward compatibility
                process_OnNG(sender, e);
            }
            else
            {
                // RESERVED
            }
        }
        private void process_OnNG(object sender, ProcessEventArgs e)
        {
            // 異常
            //>>> string ngMsg = "異常: " + e.Message;
            //>>> CommonLogClass.Instance.LogMessage(ngMsg, Color.Red);

            // 標記 NG 到 main process
            m_mainprocess.SetNG(e.Message);
            m_BuzzerProcess.Start(1);           // Buzzer 叫一聲
            _updateButtonsStatus();
        }
        private void buzzer_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = buzzer_OnCompleted;
                BeginInvoke(h, sender, e);
            }
            else
            {
                if (m_mainprocess.LastNG != null)
                {
                    //>>> MessageBox.Show(m_mainprocess.LastNG);
                    var errMsg = m_mainprocess.LastNG + "\n\r\n\r(後續可按 復位 排除NG態)";
                    VsMSG.Instance.Warning(errMsg);
                }
            }
        }
        private void process_OnLiveImage(object sender, ProcessEventArgs e)
        {
            if (e.Tag != null && e.Tag is Bitmap)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        EventHandler<ProcessEventArgs> h = process_OnLiveImage;
                        this.Invoke(h, sender, e);
                    }
                    else
                    {
                        //@LETIAN: 2022/07/01 改用 GdxDispUI 增加一些 fps
                        // bmp 由 Sender maintains life cycle.
                        // 在此不用 Dispose
                        Bitmap bmp = (Bitmap)e.Tag;
                        dispUI1.UpdateLiveImage(bmp);
                    }
                }
                catch (Exception ex)
                {
                    //>>> 此一層的 try - catch 以後可以省略.
                    //>>> 會由 Event Sender 處理 exception
                    throw ex;
                }
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
                    var frm = new FormCompensationStepTracer(e);
                    frm.FormClosed += new FormClosedEventHandler((s2, e2) =>
                    {
                        frm.Dispose();  // self-destroy ...
                    });
                    frm.TopMost = true;
                    frm.Show(this);
                    frm.Location = new Point(100, 100);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
            }
        }
        private void process_OnCompensatedInfo(object sender, CompensatedInfoEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<CompensatedInfoEventArgs> h = process_OnCompensatedInfo;
                this.Invoke(h, sender, e);
            }
            else
            {
                dispUI1.UpdateCompensatedInfo(e);
            }
        }
        private void pickprocess_OnMessage(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = pickprocess_OnMessage;
                this.Invoke(h, sender, e);
            }
            else
            {
                if (e.Message == "Picker.Start")
                {
                    // 關掉上一回合的 中光電補償圖示
                    dispUI1.UpdateCompensatedInfo(null);
                    dispUI1.Refresh();
                }
            }
        }
        private void dispensingprocess_OnMessage(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = dispensingprocess_OnMessage;
                this.Invoke(h, sender, e);
            }
            else
            {
                // UV 照射 計時顯示 on/off
                if (e.Message.StartsWith("UV"))
                {
                    bool isOn = !e.Message.Contains("Off");
                    lblUvTiming.Visible = isOn;
                    if (isOn)
                    {
                        lblUvTiming.Text = e.Message;
                        lblUvTiming.Refresh();
                    }
                }
            }
        }
        private void handle_main_process_completed(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = handle_main_process_completed;
                BeginInvoke(h, sender, e);
            }
            else
            {
                if (e.Message == "PartialCompleted")
                {
                    var barcode = m_mainprocess.Barcode;
                    var ngMsg = m_mainprocess.LastNG;
                    int mirrorIdx = m_mainprocess.MainMirrorIndex;

                    try
                    {
                        var args = (object[])e.Tag;
                        mirrorIdx = (int)args[0];
                        ngMsg = (string)args[1];
                    }
                    catch (Exception ex)
                    {
                        GdxGlobal.LOG.Warn(ex, "PartialCompleted Event 格式有誤!");
                        return;
                    }

                    var gen = new ZxReportGenerator();
                    gen.GenerateReports(barcode, ngMsg, mirrorIdx);

                    if (lblPassSign != null)
                    {
                        lblPassSign.Text = (ngMsg == null) ? "PASS" : "NG";
                        lblPassSign.ForeColor = (ngMsg == null) ? Color.Green : Color.Red;
                    }
                }
                else
                {
                    // Final Completed 
                    if (txtBarcode != null)
                        txtBarcode.Text = "";
                    _sim_auto_barcode();
                }
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
        private bool check_have_mirror()
        {
            // 檢查復位完成是否有鏡片 點擊確定按鈕 清除警報
            // 將來看, 此檢查 是否歸屬於 ResetProcess
            // 再將此段程序 移入.
            // 用 event 通知 GUI 顯示異常.
            bool ok = MACHINE.PLCIO.ADR_ISRESETCOOMPLETE_HAVE_MIRROR;
            if (ok)
            {
                string msg = "吸嘴有鏡片，請取出后，點擊確定";
                //CommonLogClass.Instance.LogMessage(msg, Color.Red);
                VsMSG.Instance.Warning(msg);
                MACHINE.PLCIO.CLEARALARMS = true;
                //DialogResult dialogResult = VsMSG.Instance.Question(msg);
                //if(dialogResult == DialogResult.OK)
                //{
                //    MACHINE.PLCIO.CLEARALARMS = true;
                //}
            }
            return ok;
        }
        private void set_cooling_module(bool on)
        {
            MACHINE.PLCIO.SetOutputIndex((int)ControlSpace.IOSpace.DispensingAddressEnum.ADR_FINTOP, on);
            MACHINE.PLCIO.SetOutputIndex((int)ControlSpace.IOSpace.DispensingAddressEnum.ADR_FINBOTTOM, !on);
            MACHINE.PLCIO.SetOutputIndex((int)ControlSpace.IOSpace.DispensingAddressEnum.ADR_FAN, on);

            //_LOG(on ? "散熱模組打開" : "散熱模組關閉");

        }
        private void close_projector_light()
        {
            var projector = OPSpace.Projector.Instance;
            projector.SetColor(Eazy_Project_Interface.ProjectColor.LightRed, false);
            projector.SetColor(Eazy_Project_Interface.ProjectColor.LightGreen, false);
        }
        private void clear_NG()
        {
            if (m_mainprocess.LastNG != null)
            {
                m_mainprocess.SetNG(null);
                StopAllProcesses("USERSTOP");

                if (lblPassSign != null)
                    lblPassSign.Text = "";

                VsMSG.Instance.Info("請取下鏡片!");
            }
            _updateButtonsStatus();
        }

        private void _updateButtonsStatus()
        {
            bool isNG = (m_mainprocess.LastNG != null);
            btnStart.Enabled = !isNG;
            btnStop.Enabled = !isNG;
            //>>> btnManual_Auto.Enabled = !isNG;
        }
        private void _updateProductionRunTime()
        {
            if (lblProductionRunTime != null)
            {
                if (m_mainprocess.IsOn)
                {
                    var ts = DateTime.Now - m_mainprocess.StartTime;
                    if (lblProductionRunTime.Tag != null)
                    {
                        if (int.TryParse(lblProductionRunTime.Tag.ToString(), out int secs))
                        {
                            if (secs == (int)ts.TotalSeconds)
                                return;
                        }
                    }
                    lblProductionRunTime.Text = ts.ToString().Split('.')[0];
                    lblProductionRunTime.Tag = ((int)ts.TotalSeconds).ToString();
                }
            }
        }
        private void _sim_auto_barcode()
        {
#if (OPT_SIM)
            if (txtBarcode == null)
                return;

            char sep = '#';
            int count = 1;
            if (txtBarcode.Tag != null)
            {
                var tag = txtBarcode.Tag.ToString();
                var strs = tag.Split(sep);
                if (strs.Length > 1 && int.TryParse(strs[1], out count))
                    count++;
            }
            var barcode = string.Format("SIM{0}{1:000}", sep, count);
            txtBarcode.Text = barcode;
            txtBarcode.Tag = barcode;
#endif
        }
        #endregion


        #region AUTO_LAYOUT_FUNCTIONS
        void _intercept_UI(Control panel = null)
        {
            // 在不影響其他站的情況下
            // 把 EssUI 的顯示 PlcRx label
            // 移轉過來 layout

            if (panel == null)
                panel = FindForm();

            bool isDoneWithEssUI = false;
            bool isDoneWithRunUI = false;

            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl is UserControl)
                {
                    if (isDoneWithEssUI && isDoneWithRunUI)
                    {
                        return;
                    }
                    else if (!isDoneWithEssUI && ctrl.Name == "essUI1")
                    {
                        isDoneWithEssUI = true;
                        _intercept_EssUI_PlcTimeLabel(ctrl);
                    }
                    else if (!isDoneWithRunUI && ctrl.Name == "runUI1")
                    {
                        isDoneWithRunUI = true;
                        _intercept_RunUI_BarcodeTextBox(ctrl);
                        runUI = ctrl;
                    }
                    else
                    {
                        _intercept_UI(ctrl);
                        continue;
                    }
                }
            }
        }
        void _intercept_EssUI_PlcTimeLabel(Control panel)
        {
            foreach (Control c in panel.Controls)
            {
                if (c.Name == "label10" && c is Label)
                {
                    // 在不影響其他站的情況下
                    // 把 EssUI 的顯示 PlcRx label
                    // 移轉過來 layout
                    var oldParent = c.Parent;
                    var newParent = lblScanningTime.Parent;
                    oldParent.Controls.Remove(c);
                    newParent.Controls.Add(c);
                    c.Parent = newParent;
                    ((Label)c).BorderStyle = lblScanningTime.BorderStyle;
                    ((Label)c).TextAlign = lblScanningTime.TextAlign;
                    ((Label)c).AutoSize = false;
                    c.Font = lblScanningTime.Font;
                    c.BackColor = lblScanningTime.BackColor;
                    c.ForeColor = lblScanningTime.ForeColor;
                    c.Size = lblScanningTime.Size;
                    c.Location = lblScanningTime.Location;
                    var swap = lblScanningTime;
                    c.Tag = swap;
                    lblScanningTime = (Label)c;
                    lblScanningTime.Visible = true;
                    swap.Visible = false;
                    dispUI1.AlignToTitleBarDockArea(lblScanningTime, lblLEText);
                    return;
                }
                else
                {
                    _intercept_EssUI_PlcTimeLabel(c);
                }
            }
        }
        void _intercept_RunUI_BarcodeTextBox(Control panel)
        {
            foreach (Control c in panel.Controls)
            {
                if (c.Name == "label2" && lblProductionRunTime == null)
                {
                    lblProductionRunTime = c;
                }
                if (c.Name == "label4" && lblPassSign == null)
                {
                    lblPassSign = c;
                }
                if (c.Name == "textBox3" && c is TextBox && txtBarcode == null)
                {
                    txtBarcode = c;
                }

                if (txtBarcode != null && lblPassSign != null && lblProductionRunTime != null)
                {
                    return;
                }
                else
                {
                    _intercept_RunUI_BarcodeTextBox(c);
                }
            }
        }
        void _auto_layout()
        {
            if (FindForm().WindowState == FormWindowState.Minimized)
                return;

            _auto_adjust_bottom_panels();
            _auto_adjust_disp_ui();

            //@LETIAN:20220701:
            //  lblLEText 已經交給 GdxDispUI 託管, 會自動 layout
        }
        void _auto_adjust_bottom_panels()
        {
            var rcc = ClientRectangle;
            int h = tabControl1.Height;
            tabControl1.Left = rcc.Width - tabControl1.Width - 5;
            tabControl1.Top = rcc.Height - h - 5;
            groupBox1.Top = rcc.Height - h;
            groupBox1.Height = h;
            groupBox1.Width = rcc.Width - tabControl1.Width - 5;
            //tabControl1.Height = rcc.Bottom - 5 - tabControl1.Top;
        }
        void _auto_adjust_disp_ui()
        {
            var rcc = ClientRectangle;
            var bottomPanel = tabControl1;
            //dispUI1.Height = rcc.Height - bottomPanel.Height - 5;
            dispUI1.Height = bottomPanel.Top - 3 - rcc.Top;
            // GdxDispUI 子項, 會自動 layout
#if (false)
            dispUI1.Width = rcc.Width;
            foreach (Control c in dispUI1.Controls)
            {
                c.Width = rcc.Width;
                if(c is PictureBox)
                {
                    c.Height = dispUI1.Bottom - c.Top;
                }
            }
#endif
            //@LETIAN:20220701:
            //  lblLEText 交給 GdxDispUI 託管, 會自動 layout
            //lblLEText.Left = dispUI1.Right - lblLEText.Width;
            //lblLEText.Top = dispUI1.Top;
            dispUI1.AlignToTitleBarDockArea(lblScanningTime, lblLEText);
            //UV Timing
            lblUvTiming.Top = (dispUI1.Bottom - lblUvTiming.Height);
            lblUvTiming.Top = lblLEText.Bottom + 5;
            lblUvTiming.Left = dispUI1.Left;
            lblUvTiming.Width = dispUI1.Width;
            lblUvTiming.BringToFront();
        }
        #endregion
    }
}
