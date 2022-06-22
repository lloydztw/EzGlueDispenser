using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Eazy_Project_Interface;
using Eazy_Project_III.ControlSpace.MachineSpace;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.UISpace;
using JzDisplay;
using JzDisplay.UISpace;
using PhotoMachine.UISpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VsCommon.ControlSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_III.UISpace;
using Eazy_Project_III.ControlSpace.IOSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.ControlSpace;
using VsCommon.ControlSpace.IOSpace;
using Eazy_Project_Measure;
using Eazy_Project_III.FormSpace;
using Eazy_Project_III.UISpace.IOSpace;
using JetEazy.GdxCore3;



namespace Eazy_Project_III.UISpace.MainSpace
{
    public partial class MainX3UI : UserControl
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

        //相机

        /// <summary>
        /// 检查校正的相机
        /// </summary>
        ICam ICamForCali
        {
            get { return Universal.CAMERAS[0]; }
        }
        /// <summary>
        /// 投影的校正相机
        /// </summary>
        ICam ICamForBlackBox
        {
            get { return Universal.CAMERAS[1]; }
        }

        IUV m_UV
        {
            get { return UV.Instance; }
        }

        IAxis IAXIS_0
        {
            get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[0]; }
        }

        AccDBClass ACCDB
        {
            get
            {
                return Universal.ACCDB;
            }
        }

        #region 操作界面

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

#if OPT_OLD_LASER_THREAD
        System.Threading.Thread m_thread_LE = null;
        bool m_LE_Running = false;
#endif

        public MainX3UI()
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

            StopAllProcess("INIT");
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

            ResetTick();
            PickTick();
            CalibrateTick();
            BlackboxTick();
            DispensingTick();

            MainProcessTick();

            btnManual_Auto.BackColor = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? Color.Red : Color.Lime);
            btnManual_Auto.Text = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? "自动模式" : "手动模式");
            btnManual_Auto.Text = LanguageExClass.Instance.ToTraditionalChinese(btnManual_Auto.Text);

            btnStart.BackColor = (m_mainprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnReset.BackColor = (m_resetprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnPickProcess.BackColor = (m_pickprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnCalibrateProcess.BackColor = (m_calibrateprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnPutProcess.BackColor = (m_blackboxprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnDispensingProcess.BackColor = (m_dispensingprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));

            AlarmUITick();
        }

#if OPT_OLD_LASER_THREAD
        private void GetLE()
        {
            while (m_LE_Running)
            {

                System.Threading.Thread.Sleep(10);
                try
                {
                    //读数据 
                    double d_LE_Data = LEClass.Instance.Snap();
                    this.Invoke(new Action(() =>
                    {
                        lblLEText.Text = "LE : " + d_LE_Data.ToString("0.000") + " mm";
                    }));
                }
                catch
                {

                }
            }
        }
#endif

        private void MainX3UI_SizeChanged(object sender, EventArgs e)
        {
            _auto_layout();
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
                StopAllProcess("USERSTOP");
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
                    StopAllProcess("USERSTOP");
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
                    m_calibrateprocess.Start(MainMirrorIndex.ToString());
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
                    m_blackboxprocess.Start(MainMirrorIndex.ToString());
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

        //private void Button1_MouseUp(object sender, MouseEventArgs e)
        //{

        //}

        //private void Button1_MouseDown(object sender, MouseEventArgs e)
        //{

        //}


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
                StopAllProcess();
                //SetSeriousAlarms0();
                SetSeriousAlarms1();

                //StopAllProcess();
            }

            if (IsAlarmsCommonX)
            {
                SetAbnormalLight();

                IsAlarmsCommonX = false;
                StopAllProcess();
                SetCommonAlarms();

            }

            if (!Universal.IsNoUseIO)
            {
                if (IsEMCTriggered)
                {
                    SetAbnormalLight();

                    IsEMCTriggered = false;
                    StopAllProcess();
                    //OnTrigger(ActionEnum.ACT_ISEMC, "");
                }
            }

            #endregion


            UpdateStateUI();
            BuzzerTick();
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

        public void StopAllProcess(string eStrMode = "")
        {
            m_mainprocess.Stop();
            m_pickprocess.Stop();
            m_calibrateprocess.Stop();
            m_blackboxprocess.Stop();
            m_dispensingprocess.Stop();
            m_resetprocess.Stop();

            m_BuzzerProcess.Stop();

            switch (eStrMode)
            {
                case "INIT":
                    MACHINE.PLCIO.CLEARALARMS = true;
                    break;
                case "USERSTOP":
                    SetNormalLight();
                    break;
            }

        }

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

        /// <summary>
        /// 叫的第几次
        /// </summary>
        int m_BuzzerIndex = 0;
        /// <summary>
        /// 叫几声
        /// </summary>
        int m_BuzzerCount = 3;

        /// <summary>
        /// 蜂鸣器叫几声流程
        /// </summary>
        ProcessClass m_BuzzerProcess = new ProcessClass();
        private void BuzzerTick()
        {
            ProcessClass Process = m_BuzzerProcess;
            //iNextDurtime[3] = 1000;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 100;
                        Process.ID = 10;

                        switch (Process.RelateString)
                        {
                            default:
                                m_BuzzerIndex = 0;
                                //m_BuzzerCount = 3;
                                break;
                        }

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (m_BuzzerIndex < m_BuzzerCount)
                            {
                                MACHINE.PLCIO.ADR_BUZZER = true;

                                Process.NextDuriation = 500;
                                Process.ID = 15;

                                m_BuzzerIndex++;
                            }
                            else
                            {
                                MACHINE.PLCIO.ADR_BUZZER = false;
                                Process.Stop();
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            MACHINE.PLCIO.ADR_BUZZER = false;

                            Process.NextDuriation = 500;
                            Process.ID = 10;
                        }
                        break;
                }
            }
        }

        #endregion


        #region 操作界面

        int NextDurtimeTmp = 300;
        /// <summary>
        /// 记录操作 Mirror0 还是 Mirror1 即左边还是右边
        /// </summary>
        int MainMirrorIndex = 0;
        /// <summary>
        /// 拾取第几组 总共4组  从0开始
        /// </summary>
        int MainGroupIndex = 0;
        /// <summary>
        /// 单独制作一个Mirror
        /// </summary>
        bool MainAloneToMirror = false;


        ProcessClass m_mainprocess = new ProcessClass();
        void MainProcessTick()
        {
            ProcessClass Process = m_mainprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        SetRunningLight();

                        Process.NextDuriation = NextDurtimeTmp;
                        Process.ID = 510;

                        CommonLogClass.Instance.LogMessage("主流程开始", Color.Black);

                        //m_PickIndex = MainGroupIndex;
                        //MainMirrorIndex = 0;

                        break;
                    case 510:
                        if (Process.IsTimeup)
                        {

                            Process.NextDuriation = 500;
                            Process.ID = 10;

                            CommonLogClass.Instance.LogMessage("吸料开始", Color.Black);
                            CommonLogClass.Instance.LogMessage("拾取 组 " + MainGroupIndex.ToString(), Color.Black);

                            m_pickprocess.Start(MainMirrorIndex.ToString());
                            //CommonLogClass.Instance.LogMessage("拾取 Mirror " + MainMirrorIndex.ToString(), Color.Black);

                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (!m_pickprocess.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 20;

                                CommonLogClass.Instance.LogMessage("吸料结束", Color.Black);
                                CommonLogClass.Instance.LogMessage("校正开始", Color.Black);
                                m_calibrateprocess.Start(MainMirrorIndex.ToString());
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!m_calibrateprocess.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 30;

                                CommonLogClass.Instance.LogMessage("校正结束", Color.Black);
                                CommonLogClass.Instance.LogMessage("blackBox开始", Color.Black);
                                m_blackboxprocess.Start(MainMirrorIndex.ToString());
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (!m_blackboxprocess.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 40;

                                CommonLogClass.Instance.LogMessage("blackBox结束", Color.Black);
                                CommonLogClass.Instance.LogMessage("点胶开始", Color.Black);
                                m_dispensingprocess.Start(MainMirrorIndex.ToString());
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            if (!m_dispensingprocess.IsOn)
                            {
                                CommonLogClass.Instance.LogMessage("点胶结束", Color.Black);

                                if (MainMirrorIndex < 1 && !MainAloneToMirror)
                                {
                                    MainMirrorIndex++;//左边搞完 搞右边 先Mirror0 然后 Mirror1
                                    Process.NextDuriation = 500;
                                    Process.ID = 510;
                                }
                                else
                                {
                                    Process.NextDuriation = 500;
                                    Process.ID = 50;

                                    m_BuzzerIndex = 0;
                                    m_BuzzerCount = 3;//测试完成叫三声

                                    m_BuzzerProcess.Start();
                                }
                            }
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            if (!m_BuzzerProcess.IsOn)
                            {
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("主流程结束", Color.Black);

                                SetNormalLight();
                            }
                        }
                        break;
                }
            }
        }

        ProcessClass m_resetprocess = new ProcessClass();
        /// <summary>
        /// INIT流程 即初始化流程 所有轴在手动模式下归位 归位完成后 运动至各轴初始化位置
        /// </summary>
        void ResetTick()
        {
            ProcessClass Process = m_resetprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        SetRunningLight();

                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));

                        CommonLogClass.Instance.LogMessage("模组初始化位置设定", Color.Black);

                        Process.NextDuriation = 2000;
                        Process.ID = 10;

                        MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_RESET_START, true);
                        CommonLogClass.Instance.LogMessage("所有轴复位中", Color.Black);

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                            {
                                //CommonLogClass.Instance.LogMessage("所有轴复位完成", Color.Lime);
                                //Process.Stop();

                                //SetNormalLight();

                                m_BuzzerIndex = 0;
                                m_BuzzerCount = 1;//复位完成叫一声

                                m_BuzzerProcess.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 20;

                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 1, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));

                                //CommonLogClass.Instance.LogMessage("模组初始化位置设定", Color.Black);
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!m_BuzzerProcess.IsOn)
                            {
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("所有轴复位完成", Color.Black);
                                SetNormalLight();

                                bool ok = check_coretronic_version();
                                if (!ok)
                                {
                                    // Do what ever you wants ...
                                }
                            }
                        }
                        break;
                        //case 20:
                        //    if (Process.IsTimeup)
                        //    {
                        //        //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                        //        {

                        //            Process.NextDuriation = 500;
                        //            Process.ID = 30;

                        //            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                        //            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_DISPENSING, 1);
                        //            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_ADJUST, 1);

                        //            CommonLogClass.Instance.LogMessage("模组开始启动", Color.Red);
                        //        }
                        //    }
                        //    break;
                        //case 30:
                        //    if (Process.IsTimeup)
                        //    {
                        //        if ((MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1)
                        //            && MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_DISPENSING, 1)
                        //            //&& MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_ADJUST, 1)  //目前没有微调模组 不判断
                        //            )
                        //            || Universal.IsNoUseIO)
                        //        {
                        //            CommonLogClass.Instance.LogMessage("模组定位完成", Color.Lime);
                        //            Process.Stop();
                        //        }
                        //    }
                        //    break;
                }
            }
        }


        int m_PlaneIndex = 0;
        /// <summary>
        /// 缓存平面度需要到达的位置 <br/>
        /// (will be load from INI)
        /// </summary>
        List<string> m_PlaneRunList = new List<string>();
        /// <summary>
        /// 缓存平面度量测的高度 <br/>
        /// (z coordinate will be measured by laser)
        /// </summary>
        List<string> m_PlaneRunDataList = new List<string>();

        /// <summary>
        /// 记录拾取第几组
        /// </summary>
        int m_PickIndex = 0;
        /// <summary>
        /// 记录放入哪一个Mirror
        /// </summary>
        int m_PickMirrorIndex = 0;

        ProcessClass m_pickprocess = new ProcessClass();
        void PickTick()
        {
            ProcessClass Process = m_pickprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = NextDurtimeTmp;
                        m_PlaneRunDataList.Clear();
                        m_PlaneIndex = 0;
                        m_PlaneRunList.Clear();

                        bool bInitOK = true;

                        // 指定 m_PlaneRunList 來自 INI.Instance.Mirror<i+1>PlanePosList
                        // 檢查 MainGroupIndex 是否在 INI.Instance.Mirror<i+1>PosList.Count 範圍內.
                        switch (Process.RelateString)
                        {
                            case "0":
                                m_PickMirrorIndex = 0;

                                foreach (string str in INI.Instance.Mirror1PlanePosList)
                                    m_PlaneRunList.Add(str);

                                //CommonLogClass.Instance.LogMessage("校正启动Mirror0", Color.Lime);

                                if (MainGroupIndex >= INI.Instance.Mirror1PosList.Count)
                                {
                                    bInitOK = false;
                                }

                                break;
                            case "1":
                                m_PickMirrorIndex = 1;

                                foreach (string str in INI.Instance.Mirror2PlanePosList)
                                    m_PlaneRunList.Add(str);

                                //CommonLogClass.Instance.LogMessage("校正启动Mirror1", Color.Lime);

                                if (MainGroupIndex >= INI.Instance.Mirror2PosList.Count)
                                {
                                    bInitOK = false;
                                }

                                break;
                            default:

                                bInitOK = false;

                                break;
                        }

                        if (bInitOK)
                        {
                            Process.ID = 10;
                            CommonLogClass.Instance.LogMessage("拾取启动 index=" + MainGroupIndex.ToString(), Color.Black);
                        }
                        else
                        {
                            m_mainprocess.Stop();
                            Process.Stop();
                            CommonLogClass.Instance.LogMessage("拾取 未定义Mirror的值停止流程", Color.Red);
                        }

                        break;

                    #region 测试平面度

                    case 10:
                        if (Process.IsTimeup)
                        {
                            //开始循环设定 产品 平面度位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, m_PlaneRunList[m_PlaneIndex]);

                            CommonLogClass.Instance.LogMessage("产品平面度位置设定 Index=" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //GO
                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                            CommonLogClass.Instance.LogMessage("启动 Index" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 30;
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //读数据 
                                double z = LEClass.Instance.Snap();
                                //读数据的xyz位置 提取yz 作为平面度的xy
                                string[] plane_xyz = m_PlaneRunList[m_PlaneIndex].Split(',').ToArray();

                                //组合新位置 用于计算平面度
                                string planeNew_xyz = plane_xyz[1] + "," + plane_xyz[2] + "," + z.ToString();
                                m_PlaneRunDataList.Add(planeNew_xyz);

                                CommonLogClass.Instance.LogMessage("Index=" + m_PlaneIndex.ToString() + ":" + planeNew_xyz, Color.Black);

                                m_PlaneIndex++;

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 40;
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            //读取数据完成
                            if (m_PlaneIndex < m_PlaneRunList.Count)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 10;
                            }
                            else
                            {
                                //计算平面度

                                bool bOK = true;

                                //首先判断块规资料是否超过3个 不超过则NG

                                if (INI.Instance.Mirror0PlaneHeightPosList.Count >= 3)
                                {
                                    QPoint3D[] _planeheight = new QPoint3D[INI.Instance.Mirror0PlaneHeightPosList.Count];
                                    int i = 0;
                                    while (i < INI.Instance.Mirror0PlaneHeightPosList.Count)
                                    {
                                        string _strplane = INI.Instance.Mirror0PlaneHeightPosList[i];
                                        string[] _strxyz = _strplane.Split(',').ToArray();
                                        _planeheight[i] = new QPoint3D(double.Parse(_strxyz[0]), double.Parse(_strxyz[1]), double.Parse(_strxyz[2]));
                                        i++;
                                    }

                                    QPlane myPlane = new QPlane();
                                    myPlane.LeastSquareFit(_planeheight);

                                    string myHeightStr = string.Empty;

                                    foreach (string str in m_PlaneRunDataList)
                                    {
                                        string[] runStrPlane = str.Split(',').ToArray();
                                        QPoint3D run = new QPoint3D(double.Parse(runStrPlane[0]), double.Parse(runStrPlane[1]), double.Parse(runStrPlane[2]));
                                        double runheight = myPlane.GetZLocation(run);

                                        //CommonLogClass.Instance.LogMessage("平面度测试正常", Color.Lime);
                                        myHeightStr += runheight.ToString() + ",";
                                    }

                                    CommonLogClass.Instance.LogMessage(myHeightStr, Color.Black);
                                    bOK = true;

                                }
                                else
                                {
                                    bOK = false;
                                }

                                if (bOK)
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 50;

                                    CommonLogClass.Instance.LogMessage("平面度测试正常", Color.Black);
                                }
                                else
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 4010;

                                    CommonLogClass.Instance.LogMessage("平面度超标", Color.Red);
                                }

                            }
                        }
                        break;

                    #endregion

                    case 50:
                        if (Process.IsTimeup)
                        {

                            //开始吸料流程 及 到达测试偏移位置

                            switch (m_PickMirrorIndex)
                            {
                                case 0:
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 5, INI.Instance.Mirror1PosList[MainGroupIndex]);
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 5, INI.Instance.sMirrorAdjDeep1Length.ToString() + ",0,0");
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 4, INI.Instance.Mirror1CaliPos);
                                    break;
                                case 1:
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 5, INI.Instance.Mirror2PosList[MainGroupIndex]);
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 5, INI.Instance.sMirrorAdjDeep2Length.ToString() + ",0,0");
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 4, INI.Instance.Mirror2CaliPos);
                                    break;
                            }


                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 4, INI.Instance.sMirrorAdjBackLength.ToString() + ",0,0");

                            CommonLogClass.Instance.LogMessage("吸嘴 测试偏移 位置写入", Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 60;
                        }
                        break;
                    case 60:
                        if (Process.IsTimeup)
                        {
                            //开始启动

                            MACHINE.PLCIO.SetIO(IOConstClass.QB1542, true);

                            CommonLogClass.Instance.LogMessage("拾取及到达测试位置 启动", Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 70;

                        }
                        break;
                    case 70:
                        if (Process.IsTimeup)
                        {
                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1542))
                            {
                                CommonLogClass.Instance.LogMessage("拾取及到达测试位置 完成", Color.Black);

                                Process.Stop();
                            }
                        }
                        break;

                    #region INITIAL POS

                    case 4010:
                        if (Process.IsTimeup)
                        {
                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 4020;

                            MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                            CommonLogClass.Instance.LogMessage("吸嘴模组回待命启动", Color.Black);
                        }
                        break;
                    case 4020:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("吸嘴模组待命完成", Color.Black);

                                //平面度超标 模组1 归位  停止主流程
                                //if (m_mainprocess.IsOn)
                                m_mainprocess.Stop();

                                Process.Stop();

                            }
                        }
                        break;

                        #endregion

                }
            }
        }

        /// <summary>
        /// 判断校正组 跑哪一个Mirror 左边还是右边
        /// </summary>
        int Mirror_CalibrateProcessIndex = 0;

        ProcessClass m_calibrateprocess = new ProcessClass();
        /// <summary>
        /// 校正流程
        /// 1.判断 Mirror_CalibrateProcessIndex 的值 执行Mirror0 还是 Mirror1 
        /// 2.抓图计算偏移
        /// 3.开始放入料 并且把offset 值加进去
        /// </summary>
        void CalibrateTick()
        {
            ProcessClass Process = m_calibrateprocess;

            if (Process.IsOn)
            {
                //> GdxCore.Trace("MirrorCalibration.Tick", Process, 0);

                switch (Process.ID)
                {
                    case 5:
                        Process.NextDuriation = NextDurtimeTmp;
                        switch (Process.RelateString)
                        {
                            case "0":
                                Mirror_CalibrateProcessIndex = 0;

                                Process.ID = 10;
                                MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                                CommonLogClass.Instance.LogMessage("校正启动Mirror0", Color.Black);
                                GdxCore.Trace("MirrorCalibration.Start", Process, 0);
                                break;
                            case "1":
                                Mirror_CalibrateProcessIndex = 1;

                                Process.ID = 10;
                                MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                                CommonLogClass.Instance.LogMessage("校正启动Mirror1", Color.Black);
                                GdxCore.Trace("MirrorCalibration.Start", Process, 1);
                                break;
                            default:
                                m_mainprocess.Stop();
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("校正 未定义Mirror的值停止流程", Color.Red);
                                break;
                        }
                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //到达位置 打开灯光 设定曝光

                                ICamForCali.SetExposure(RecipeCHClass.Instance.CaliCamExpo);

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 20;

                            }
                        }
                        break;

                    case 20:
                        if (Process.IsTimeup)
                        {
                            CommonLogClass.Instance.LogMessage("擷取影像", Color.Black);

                            ICamForCali.Snap();
                            Bitmap bmp = new Bitmap(ICamForCali.GetSnap());
                            bmp.Save("image0.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            m_DispUI.SetDisplayImage(bmp);


                            //计算偏移值
                            //参数中算的解析度或是手动输入
                            //参数中先记录Mirror的中心位置
                            //测试中算的Mirror的中心位置
                            //计算两个中心位置之差
                            //补偿的是吸嘴模组的y和z轴 相当于画面中的 x和y 
                            //画面中向左为正 向下为正
                            PointF ptfOffset = new PointF(0, 0);
                            ptfOffset.X -= RecipeCHClass.Instance.CaliPicCenter.X;
                            ptfOffset.Y -= RecipeCHClass.Instance.CaliPicCenter.Y;

                            //补偿放入的位置
#if (false)
                            string posPutAdjust = string.Empty;
                            string mirrorPutPos = string.Empty;
                            switch (Mirror_CalibrateProcessIndex)
                            {
                                case 0:
                                    mirrorPutPos = INI.Instance.Mirror1PutPos;
                                    posPutAdjust = ToolAdjustData(INI.Instance.Mirror1PutPos, ptfOffset.X, ptfOffset.Y);
                                    break;
                                case 1:
                                    mirrorPutPos = INI.Instance.Mirror2PutPos;
                                    posPutAdjust = ToolAdjustData(INI.Instance.Mirror2PutPos, ptfOffset.X, ptfOffset.Y);
                                    break;
                            }
#else
                            string mirrorPutPos = string.Empty;
                            switch (Mirror_CalibrateProcessIndex)
                            {
                                case 0:
                                    mirrorPutPos = INI.Instance.Mirror1PutPos;
                                    break;
                                case 1:
                                    mirrorPutPos = INI.Instance.Mirror2PutPos;
                                    break;
                            }
                            string posPutAdjust = ToolAdjustData(mirrorPutPos, ptfOffset.X, ptfOffset.Y);
#endif

                            GdxCore.Trace("MirrorCalibration.Compensate", Process, bmp, mirrorPutPos, ptfOffset);
                            bool go = GdxCore.CheckCompensate(bmp);
                            if (!go)
                            {

                            }


                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 3, posPutAdjust);

                            MACHINE.PLCIO.ADR_SMALL_LIGHT = false;

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 3010;

                        }
                        break;

                    case 3010:
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
                                CommonLogClass.Instance.LogMessage("吸嘴模组到达位置", Color.Black);

                                switch (Mirror_CalibrateProcessIndex)
                                {
                                    case 0:
                                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, "0,0,0");
                                        break;
                                    case 1:
                                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, "0,0,0");
                                        break;
                                }

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
                                CommonLogClass.Instance.LogMessage("微调模组到达位置", Color.Black);
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("校正完成", Color.Black);
                                GdxCore.Trace("MirrorCalibration.Completed", Process);
                            }
                        }
                        break;
                }
            }
        }

        ProcessClass m_blackboxprocess = new ProcessClass();
        void BlackboxTick()
        {
            ProcessClass Process = m_blackboxprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 1000;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            Process.Stop();
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// 判断点胶组 跑哪一个Mirror 左边还是右边
        /// </summary>
        int Mirror_DispensingProcessIndex = 0;

        int m_DispensingIndex = 0;
        List<string> m_DispensingRunList = new List<string>();

        ProcessClass m_dispensingprocess = new ProcessClass();
        /// <summary>
        /// 点胶流程
        /// 1.判断 Mirror_DispensingProcessIndex 值   到达Mirror0 还是 Mirror1  
        /// 如果 Mirror_DispensingProcessIndex 的值 不是0和1 将结束此流程 并停止主流
        /// 否则 继续下面的流程
        /// 2.将位置转存到缓存的list中 m_DispensingRunList
        /// 3.通过m_DispensingIndex 记录 进行到点胶的哪个点
        /// 4.某个点完成后 回到避光槽位置 然后进行下一个点
        /// 5.全部完成后 点胶模组回到待命位置  打开UV 定时 关闭UV
        /// 6.微调和吸嘴模组 回到待命位置
        /// 7.流程结束
        /// </summary>
        void DispensingTick()
        {
            ProcessClass Process = m_dispensingprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:
                        m_DispensingIndex = 0;
                        m_DispensingRunList.Clear();

                        Process.NextDuriation = NextDurtimeTmp;

                        bool bOK = true;

                        switch (Process.RelateString)
                        {
                            case "0":
                                Mirror_DispensingProcessIndex = 0;

                                foreach (string str in INI.Instance.Mirror1JamedPosList)
                                    m_DispensingRunList.Add(str);

                                Process.ID = 10;

                                CommonLogClass.Instance.LogMessage("点胶启动Mirror0", Color.Black);
                                break;
                            case "1":
                                Mirror_DispensingProcessIndex = 1;

                                foreach (string str in INI.Instance.Mirror2JamedPosList)
                                    m_DispensingRunList.Add(str);

                                Process.ID = 10;

                                CommonLogClass.Instance.LogMessage("点胶启动Mirror1", Color.Black);
                                break;
                            default:
                                bOK = false;
                                m_mainprocess.Stop();
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("点胶 未定义Mirror的值停止流程", Color.Red);
                                break;
                        }

                        if (bOK)
                        {
                            //设定点胶时间&UV时间
                            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1091, RecipeCHClass.Instance.DispensingTime);
                            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1092, RecipeCHClass.Instance.UVTime);

                            //避光槽位置 9上 7下
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 9, INI.Instance.ShadowPosUp);
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 7, INI.Instance.ShadowPos);

                            CommonLogClass.Instance.LogMessage("设定点胶时间 " + RecipeCHClass.Instance.DispensingTime.ToString() + " 毫秒", Color.Black);
                            CommonLogClass.Instance.LogMessage("设定UV时间 " + RecipeCHClass.Instance.UVTime.ToString() + " 秒", Color.Black);
                        }

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //开始循环设定 点胶位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 8, m_DispensingRunList[m_DispensingIndex]);

                            CommonLogClass.Instance.LogMessage("点胶位置设定 Index=" + m_DispensingIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //点胶启动
                            MACHINE.PLCIO.SetIO(IOConstClass.QB1541, true);

                            CommonLogClass.Instance.LogMessage("点胶启动 Index" + m_DispensingIndex.ToString(), Color.Black);
                            m_DispensingIndex++;

                            Process.NextDuriation = 1500;
                            Process.ID = 30;
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1541))
                            {
                                //单个点胶完成
                                if (m_DispensingIndex < m_DispensingRunList.Count)
                                {
                                    //m_DispensingIndex++;

                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 10;
                                }
                                else
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 40;

                                    //Process.Stop();
                                    CommonLogClass.Instance.LogMessage("点胶完成", Color.Black);

                                    //UVCylinder.Instance.SetFront();
                                }
                            }
                        }
                        break;

                    #region INITIAL POS

                    case 40:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                            {

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 50;

                                //MACHINE.PLCIO.SetIO(IOConstClass.QB1625, true);
                                //MACHINE.PLCIO.SetIO(IOConstClass.QB1665, true);

                                //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                                //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                                //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);


                                //避光槽启动
                                MACHINE.PLCIO.SetIO(IOConstClass.QB1550, true);

                                CommonLogClass.Instance.LogMessage("運動至避光槽启动", Color.Black);
                            }
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1550) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("運動至避光槽完成", Color.Black);
                                //Process.Stop();

                                UVCylinder.Instance.SetFront();

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4010;
                            }
                        }
                        break;

                    //case 40:
                    //    if (Process.IsTimeup)
                    //    {
                    //        //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                    //        {

                    //            Process.NextDuriation = NextDurtimeTmp;
                    //            Process.ID = 50;

                    //            //MACHINE.PLCIO.SetIO(IOConstClass.QB1625, true);
                    //            //MACHINE.PLCIO.SetIO(IOConstClass.QB1665, true);

                    //            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                    //            MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                    //            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);

                    //            CommonLogClass.Instance.LogMessage("点胶模组初始化启动", Color.Black);
                    //        }
                    //    }
                    //    break;
                    //case 50:
                    //    if (Process.IsTimeup)
                    //    {
                    //        if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_DISPENSING, 6) || Universal.IsNoUseIO)
                    //        {
                    //            CommonLogClass.Instance.LogMessage("点胶模组初始化完成", Color.Black);
                    //            //Process.Stop();

                    //            UVCylinder.Instance.SetFront();

                    //            Process.NextDuriation = NextDurtimeTmp;
                    //            Process.ID = 4010;
                    //        }
                    //    }
                    //    break;

                    #endregion

                    case 4010:
                        if (Process.IsTimeup)
                        {
                            if (UVCylinder.Instance.GetFrontOK())
                            {
                                UV.Instance.Seton();
                                CommonLogClass.Instance.LogMessage("UV打开", Color.Black);

                                Process.NextDuriation = RecipeCHClass.Instance.UVTime * 1000;
                                Process.ID = 4020;
                            }
                        }
                        break;
                    case 4020:
                        if (Process.IsTimeup)
                        {
                            if (UVCylinder.Instance.GetFrontOK())
                            {
                                UV.Instance.Setoff();
                                CommonLogClass.Instance.LogMessage("UV关闭", Color.Black);
                                UVCylinder.Instance.SetBack();

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4030;
                            }
                        }
                        break;
                    case 4030:
                        if (Process.IsTimeup)
                        {
                            if (UVCylinder.Instance.GetBackOK())
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 403000;

                                CommonLogClass.Instance.LogMessage("点胶固化完成", Color.Black);

                                MACHINE.PLCIO.SetOutputIndex(4, false);
                                MACHINE.PLCIO.SetOutputIndex(5, true);
                                CommonLogClass.Instance.LogMessage("產品破真空", Color.Black);
                            }
                        }
                        break;
                    case 403000:
                        if (Process.IsTimeup)
                        {
                            if (!MACHINE.PLCIO.GetInputIndex(6))
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 403001;

                                CommonLogClass.Instance.LogMessage("產品破真空完成", Color.Black);

                                MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 4, INI.Instance.sMirrorAdjBackLength.ToString() + ",0,0");
                                CommonLogClass.Instance.LogMessage("微調模組後退位置寫入", Color.Black);

                            }
                        }
                        break;
                    case 403001:
                        if (Process.IsTimeup)
                        {
                            if (UVCylinder.Instance.GetBackOK())
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 403002;

                                MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_ADJUST, 4);

                                CommonLogClass.Instance.LogMessage("微調模組後退位置定位", Color.Black);

                            }
                        }
                        break;
                    case 403002:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_ADJUST, 4))
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4040;
                                CommonLogClass.Instance.LogMessage("微調模組後退完成", Color.Black);

                                MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);

                                //CommonLogClass.Instance.LogMessage("微调模组待命完成", Color.Black);
                                CommonLogClass.Instance.LogMessage("吸嘴模组回待命启动", Color.Black);

                            }
                        }
                        break;
                    case 4040:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4050;

                                CommonLogClass.Instance.LogMessage("吸嘴模组待命完成", Color.Black);

                                MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                                CommonLogClass.Instance.LogMessage("微调模组待命啓動", Color.Black);

                            }
                        }
                        break;
                    case 4050:
                        if (Process.IsTimeup)
                        {
                            if ((MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_ADJUST, 6) && MACHINECollection.AdjustReadyPositionOK()) || Universal.IsNoUseIO)
                            {
                                Process.Stop();

                                CommonLogClass.Instance.LogMessage("微调模组待命完成", Color.Black);
                            }
                        }
                        break;



                }
            }
        }

        ProcessClass baseprocess = new ProcessClass();
        void BaseTick()
        {
            ProcessClass Process = baseprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            Process.Stop();
                        }
                        break;
                }
            }
        }

        #endregion

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

        bool check_coretronic_version()
        {
            string version = GdxCore.GetDllVersion();
            CommonLogClass.Instance.LogMessage("中光電 DLL Version = " + version, Color.Blue);

            bool ok = GdxCore.UpdateParams();
            ok = false;
            string msg = "中光電 DLL UpdateParams() = " + ok;
            CommonLogClass.Instance.LogMessage(msg, ok ? Color.Green : Color.Red);
            if (!ok)
            {
                VsMSG.Instance.Warning(msg.Replace("=", "\n\r"));
            }
            return ok;
        }

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

            //@LETAIN: 自動調整下方 panels
            _auto_adjust_bottom_panels();

            //@LETIAN: 自動調整 DispUI
            _auto_adjust_disp_ui();
        }
        void _auto_adjust_bottom_panels()
        {
            //@LETAIN: 自動調整下方 panels
            _auto_align_to_parent_bottom(0, tabControl1);
            _auto_align_to_parent_bottom(0, groupBox1, groupBox2);
            _auto_align_to_parent_bottom(5, richTextBox1, label5);
            tabControl1.Width = ClientRectangle.Width;
            //@LETIAN: 自動調整下方 六按鈕
            //_auto_adjust_six_major_buttons();
        }
        void _auto_adjust_six_major_buttons()
        {
            //@LETIAN: 自動調整下方 六按鈕
            var header = label11;
            var btns1 = new Control[] { button6, button4, button7 };
            var btns2 = new Control[] { button8, button9, button1 };
            var parent = header.Parent;
            var rcc = parent.ClientRectangle;
            int padding = 2;
            var h = (rcc.Height - header.Bottom - padding * 3) / 2;
            var y = header.Bottom + padding;
            foreach (var btn in btns1)
            {
                btn.Top = y;
                btn.Height = h;
            }
            y += (h + padding);
            foreach (var btn in btns2)
            {
                btn.Top = y;
                btn.Height = h;
            }
        }
        void _auto_adjust_disp_ui()
        {
            var rcc = ClientRectangle;
            dispUI1.Height = rcc.Height - tabControl1.Height;
            dispUI1.Width = tabControl1.Width;
        }
        void _auto_align_to_parent_bottom(int padding, params Control[] panels)
        {
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    var parent = panel.Parent;
                    var rcc = parent.ClientRectangle;
                    //panel.Height = rcc.Height - panel.Top - padding;
                    panel.Top = rcc.Bottom - panel.Height;
                }
            }
        }
        #endregion
    }
}
