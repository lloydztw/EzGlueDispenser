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

namespace Eazy_Project_III.UISpace.MainSpace
{
    public partial class MainX1UI : UserControl
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
        /// 前相機
        /// </summary>
        ICam ICam0
        {
            get { return Universal.CAMERAS[0]; }
        }
        /// <summary>
        /// 邊邊相機
        /// </summary>
        ICam ICam1
        {
            get { return Universal.CAMERAS[1]; }
        }
        /// <summary>
        /// 頂相機
        /// </summary>
        ICam ICam2
        {
            get { return Universal.CAMERAS[2]; }
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

        Label lblState;
        Label lblAlarm;

        Button btnGetIamgeProcess1;
        Button btnGetIamgeProcess2;


        Button btnPutProcess1;
        Button btnPutProcess2;
        Button btnDispensingProcess1;
        Button btnDispensingProcess2;


        #endregion

        /// <summary>
        /// 前相機
        /// </summary>
        DispUI m_DispUI1;
        /// <summary>
        /// 邊邊相機
        /// </summary>
        DispUI m_DispUI2;
        /// <summary>
        /// 頂相機
        /// </summary>
        DispUI m_DispUI3;
        //IO_INPUTUI _INPUTUI;

        //X3INPUTUI _X3INPUTUI;
        X1INPUTUI _X1INPUTUI;
        X1OUTPUTUI _X1OUTPUTUI;

        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        DispensingX1MachineClass MACHINE
        {
            get { return (DispensingX1MachineClass)Universal.MACHINECollection.MACHINE; }
        }

        //System.Threading.Thread m_thread_LE = null;
        //bool m_LE_Running = false;

        public MainX1UI()
        {
            InitializeComponent();
        }

        public void Init()
        {
            init_Display();
            update_Display();

            CommonLogClass.Instance.SetRichTextBox(richTextBox1);
            //CommonLogClass.Instance.LogMessage("系统初始化......", Color.Black);

            //_INPUTUI = iO_INPUTUI1;
            _X1INPUTUI = x1INPUTUI1;
            _X1OUTPUTUI = x1OUTPUTUI1;

            lblState = label11;
            lblAlarm = label5;

            btnStart = button6;
            btnStop = button4;
            btnReset = button7;

            btnClearAlarm = button8;
            btnMute = button9;


            btnGetIamgeProcess1 = button2;
            btnGetIamgeProcess2 = button3;

            btnPutProcess1 = button5;
            btnPutProcess2 = button10;
            btnDispensingProcess1 = button11;
            btnDispensingProcess2 = button12;

            btnManual_Auto = button1;
            btnManual_Auto.Click += BtnManual_Auto_Click;


            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnClearAlarm.Click += BtnClearAlarm_Click;
            btnMute.Click += BtnMute_Click;

            btnReset.Click += BtnReset_Click;
            //btnPickProcess.Click += BtnPickProcess_Click;
            //btnCalibrateProcess.Click += BtnCalibrateProcess_Click;
            //btnPutProcess.Click += BtnPutProcess_Click;
            //btnDispensingProcess.Click += BtnDispensingProcess_Click;


            btnGetIamgeProcess1.Click += BtnGetIamgeProcess1_Click;
            btnGetIamgeProcess2.Click += BtnGetIamgeProcess2_Click;

            btnPutProcess1.Click += BtnPutProcess1_Click;
            btnPutProcess2.Click += BtnPutProcess2_Click;
            btnDispensingProcess1.Click += BtnDispensingProcess1_Click;
            btnDispensingProcess2.Click += BtnDispensingProcess2_Click;



            //MACHINE.EVENT.Initial(lsbEvent);
            MACHINE.EVENT.Initial(lblAlarm);

            MACHINE.TriggerAction += MACHINE_TriggerAction;
            MACHINE.EVENT.TriggerAlarm += EVENT_TriggerAlarm;

            //_INPUTUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
            _X1INPUTUI.Initial(VERSION, OPTION, MACHINE);
            _X1OUTPUTUI.Initial(VERSION, OPTION, MACHINE);

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            //m_LE_Running = true;
                            //m_thread_LE = new System.Threading.Thread(new System.Threading.ThreadStart(GetLE));
                            //m_thread_LE.Start();

                            break;
                    }

                    break;
            }



            SetNormalLight();

            StopAllProcess("INIT");

        }

        private void BtnGetIamgeProcess2_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }

            string onStrMsg = "是否要執行取料流程？";
            string offStrMsg = "是否要停止取料流程？";
            string msg = (checkimage2process.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!checkimage2process.IsOn && !MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1421))
                {
                    checkimage2process.Start();
                }
                else
                    StopAllProcess("USERSTOP");
            }

            //if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1421))
            //    checkimage2process.Start();
        }

        private void BtnGetIamgeProcess1_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }
            string onStrMsg = "是否要執行拍照流程？";
            string offStrMsg = "是否要停止拍照流程？";
            string msg = (checkimage1process.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!checkimage1process.IsOn && !MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1411))
                {
                    checkimage1process.Start();
                }
                else
                    StopAllProcess("USERSTOP");
            }


            //if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1411))
            //    checkimage1process.Start();
        }

        private void BtnDispensingProcess2_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1461))
                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1460, true);
        }

        private void BtnDispensingProcess1_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1451))
                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1450, true);
        }

        private void BtnPutProcess2_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1441))
                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1440, true);
        }

        private void BtnPutProcess1_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }
            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1431))
                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1430, true);
        }

        public void Close()
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            //m_LE_Running = false;
                            //if (m_thread_LE != null)
                            //{
                            //    m_LE_Running = false;
                            //    m_thread_LE.Abort();
                            //}

                            break;
                    }

                    break;
            }

            SetNormalLight();
        }
        public void Tick()
        {
            _X1INPUTUI.Tick();
            _X1OUTPUTUI.Tick();

            ResetTick();
            //PickTick();
            //CalibrateTick();
            //BlackboxTick();
            //DispensingTick();

            //MainProcessTick();

            MainTick();
            GetImage1Tick();
            GetImage2Tick();
            CheckImage1Tick();
            CheckImage2Tick();

            btnManual_Auto.BackColor = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? Color.Red : Color.Lime);
            btnManual_Auto.Text = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? "自动模式" : "手动模式");
            btnManual_Auto.Text = LanguageExClass.Instance.ToTraditionalChinese(btnManual_Auto.Text);

            btnStart.BackColor = (mainprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnReset.BackColor = (m_resetprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnPickProcess.BackColor = (m_pickprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnCalibrateProcess.BackColor = (m_calibrateprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnPutProcess.BackColor = (m_blackboxprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnDispensingProcess.BackColor = (m_dispensingprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));

            btnGetIamgeProcess1.BackColor = (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1411) ? Color.Red : Color.FromArgb(192, 255, 192));
            btnGetIamgeProcess2.BackColor = (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1421) ? Color.Red : Color.FromArgb(192, 255, 192));

            btnPutProcess1.BackColor = (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1431) ? Color.Red : Color.FromArgb(192, 255, 192));
            btnPutProcess2.BackColor = (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1441) ? Color.Red : Color.FromArgb(192, 255, 192));
            btnDispensingProcess1.BackColor = (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1451) ? Color.Red : Color.FromArgb(192, 255, 192));
            btnDispensingProcess2.BackColor = (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1461) ? Color.Red : Color.FromArgb(192, 255, 192));

            AlarmUITick();
        }

        //private void GetLE()
        //{

        //    while (m_LE_Running)
        //    {

        //        System.Threading.Thread.Sleep(10);
        //        try
        //        {
        //            //读数据 
        //            double d_LE_Data = LEClass.Instance.Snap();
        //            this.Invoke(new Action(() =>
        //            {
        //                lblLEText.Text = "LE : " + d_LE_Data.ToString("0.000") + " mm";
        //            }));
        //        }
        //        catch
        //        {

        //        }
        //    }
        //}

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

            if (!MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522))
            {
                VsMSG.Instance.Warning("請先復位，再啓動。");
                return;
            }

            string onStrMsg = "是否要启动？";
            string offStrMsg = "是否要停止？";
            string msg = (mainprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!mainprocess.IsOn)
                {
                    mainprocess.Start();
                }
                else
                    StopAllProcess("USERSTOP");
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

            mainprocess.Stop();
            getimage1process.Stop();
            getimage2process.Stop();
            checkimage1process.Stop();
            checkimage2process.Stop();


            //m_mainprocess.Stop();
            //m_pickprocess.Stop();
            //m_calibrateprocess.Stop();
            //m_blackboxprocess.Stop();
            //m_dispensingprocess.Stop();
            m_resetprocess.Stop();

            m_BuzzerProcess.Stop();

            switch (eStrMode)
            {
                case "INIT":
                    MACHINE.PLCIO.CLEARALARMS = true;
                    break;
                case "USERSTOP":
                    MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1524, true);
                    SetNormalLight();
                    break;
            }

        }

        private void UpdateStateUI()
        {

            //if (m_TestProcess.IsOn)
            //    lblState.Text = "执行-测试区取像中 " + m_TestProcess.ID.ToString();
            //if (m_dispensingprocess.IsOn)
            //    lblState.Text = "执行-点胶中 " + m_dispensingprocess.ID.ToString();
            //else if (m_blackboxprocess.IsOn)
            //    lblState.Text = "执行-放取校正中  " + m_blackboxprocess.ID.ToString();
            //else if (m_calibrateprocess.IsOn)
            //    lblState.Text = "执行-校正中 " + m_calibrateprocess.ID.ToString();
            //else if (m_pickprocess.IsOn)
            //    lblState.Text = "执行-拾取中 " + m_pickprocess.ID.ToString();
            //else if (m_mainprocess.IsOn)
            //    lblState.Text = "跑线中 " + m_mainprocess.ID.ToString();
            //else if (resetpartialprocess.IsOn)
            //    lblState.Text = "小复位中 " + resetpartialprocess.ID.ToString();
            if (m_resetprocess.IsOn)
                lblState.Text = "复位中 " + m_resetprocess.ID.ToString();
            else if (mainprocess.IsOn)
                lblState.Text = "跑线中 " + mainprocess.ID.ToString();
            else if (checkimage1process.IsOn)
                lblState.Text = "產品拍照中 " + mainprocess.ID.ToString();
            else if (checkimage2process.IsOn)
                lblState.Text = "鏡片取料中 " + mainprocess.ID.ToString();
            else
                lblState.Text = "待机";

            if (Universal.IsNoUseIO)
            {
                lblState.Text = "模擬運行";
                lblState.BackColor = Color.Red;
            }
            else if(MACHINE.PLCIO.ADR_ISEMC)
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


                        int i = 0;
                        while (i < MACHINECollection.GetMotorCount())
                        {
                            MACHINE.PLCIO.MotorSinglePosition(i, 6, MACHINECollection.GetSingleAXISPositionForReady(i));
                            i++;
                        }

                        //MACHINE.PLCIO.MotorSinglePosition(0, MACHINECollection.GetSingleAXISPositionForReady(0));
                        //MACHINE.PLCIO.MotorSinglePosition(1, MACHINECollection.GetSingleAXISPositionForReady(1));
                        //MACHINE.PLCIO.MotorSinglePosition(2, MACHINECollection.GetSingleAXISPositionForReady(2));
                        //MACHINE.PLCIO.MotorSinglePosition(3, MACHINECollection.GetSingleAXISPositionForReady(3));
                        //MACHINE.PLCIO.MotorSinglePosition(4, MACHINECollection.GetSingleAXISPositionForReady(4));

                        CommonLogClass.Instance.LogMessage("初始化位置设定", Color.Black);

                        Process.NextDuriation = 2000;
                        Process.ID = 10;

                        MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1520, true);
                        CommonLogClass.Instance.LogMessage("所有轴复位中", Color.Black);

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1522) || Universal.IsNoUseIO)
                            {
                                m_BuzzerIndex = 0;
                                m_BuzzerCount = 1;//复位完成叫一声

                                m_BuzzerProcess.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 20;
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
                            }
                        }
                        break;
                }
            }
        }


        ProcessClass mainprocess = new ProcessClass();
        void MainTick()
        {
            ProcessClass Process = mainprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1400, true);
                        CommonLogClass.Instance.LogMessage("流程啓動", Color.Black);
                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        //寫入相關參數

                        //设定点胶时间&UV时间
                        MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1091, RecipeCHClass.Instance.DispensingTime);
                        MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1092, RecipeCHClass.Instance.UVTime);

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1402) && !MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1401))
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 50;
                            }
                            else if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1414))
                            {
                                getimage1process.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 20;
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!getimage1process.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 30;
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1402) && !MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1401))
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 50;
                            }
                            else if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1424))
                            {
                                getimage2process.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 40;
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            if (!getimage2process.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 50;
                            }
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1402) && !MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1401))
                            {
                                m_BuzzerIndex = 0;
                                m_BuzzerCount = 3;

                                m_BuzzerProcess.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 60;
                            }
                        }
                        break;
                    case 60:
                        if (Process.IsTimeup)
                        {
                            if (!m_BuzzerProcess.IsOn)
                            {
                                CommonLogClass.Instance.LogMessage("流程完成", Color.Black);
                                Process.Stop();
                                //CommonLogClass.Instance.LogMessage("所有轴复位完成", Color.Black);
                                SetNormalLight();
                            }
                        }
                        break;
                }
            }
        }


        ProcessClass checkimage1process = new ProcessClass();
        void CheckImage1Tick()
        {
            ProcessClass Process = checkimage1process;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1410, true);
                        CommonLogClass.Instance.LogMessage("拍照流程啓動", Color.Black);
                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1414))
                            {
                                getimage1process.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 20;
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!getimage1process.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 30;
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1412) || MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1413))
                            {
                                CommonLogClass.Instance.LogMessage("拍照流程完成", Color.Black);
                                Process.Stop();
                            }
                                
                        }
                        break;
                }
            }
        }
        ProcessClass checkimage2process = new ProcessClass();
        void CheckImage2Tick()
        {
            ProcessClass Process = checkimage2process;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1420, true);
                        CommonLogClass.Instance.LogMessage("取鏡片流程啓動", Color.Black);
                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1424))
                            {
                                getimage2process.Start();

                                Process.NextDuriation = 500;
                                Process.ID = 20;
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!getimage2process.IsOn)
                            {
                                Process.NextDuriation = 500;
                                Process.ID = 30;
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1422) || MACHINE.PLCIO.GetIO(DX1_IOConstClass.QB1423))
                            {
                                CommonLogClass.Instance.LogMessage("取鏡片流程完成", Color.Black);
                                Process.Stop();
                            }
                               
                        }
                        break;
                }
            }
        }


        ProcessClass getimage1process = new ProcessClass();
        void GetImage1Tick()
        {
            ProcessClass Process = getimage1process;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        //延時拍照

                        //ICam2.SetExposure(0);

                        //打開環形燈
                        MACHINE.PLCIO.SetOutputIndex(26, true);
                        CommonLogClass.Instance.LogMessage("頂部拍照啓動", Color.Black);
                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //取像

                            ICam2.Snap();
                            Bitmap bmp = new Bitmap(ICam2.GetSnap());
                            bmp.Save("image2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            m_DispUI3.SetDisplayImage(bmp);


                            //計算

                            bool ispass = true;

                            if(ispass)
                                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1415, true);
                            else
                                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1416, true);


                            Process.NextDuriation = 500;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            CommonLogClass.Instance.LogMessage("頂部拍照完成", Color.Black);
                            MACHINE.PLCIO.SetOutputIndex(26, false);
                            Process.Stop();
                        }
                        break;
                }
            }
        }

        ProcessClass getimage2process = new ProcessClass();
        void GetImage2Tick()
        {
            ProcessClass Process = getimage2process;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        //延時拍照

                        //ICam2.SetExposure(0);
                        //打開背光燈
                        MACHINE.PLCIO.SetOutputIndex(28, true);
                        CommonLogClass.Instance.LogMessage("前邊側邊拍照啓動", Color.Black);
                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //取像

                            ICam0.Snap();
                            Bitmap bmp0 = new Bitmap(ICam0.GetSnap());
                            bmp0.Save("image0.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            m_DispUI1.SetDisplayImage(bmp0);

                            ICam1.Snap();
                            Bitmap bmp1 = new Bitmap(ICam1.GetSnap());
                            bmp1.Save("image1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            m_DispUI2.SetDisplayImage(bmp1);


                            //計算

                            bool ispass = true;

                            if (ispass)
                                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1425, true);
                            else
                                MACHINE.PLCIO.SetIO(DX1_IOConstClass.QB1426, true);

                            Process.NextDuriation = 500;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            CommonLogClass.Instance.LogMessage("前邊側邊拍照完成", Color.Black);
                            MACHINE.PLCIO.SetOutputIndex(28, false);
                            Process.Stop();
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

        void init_Display()
        {
            m_DispUI1 = dispUI1;
            m_DispUI1.Initial(100, 0.01f);
            m_DispUI1.SetDisplayType(DisplayTypeEnum.SHOW);

            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;

            m_DispUI2 = dispUI2;
            m_DispUI2.Initial(100, 0.01f);
            m_DispUI2.SetDisplayType(DisplayTypeEnum.SHOW);


            m_DispUI3 = dispUI3;
            m_DispUI3.Initial(100, 0.01f);
            m_DispUI3.SetDisplayType(DisplayTypeEnum.SHOW);
        }
        void update_Display()
        {
            m_DispUI1.Refresh();
            m_DispUI1.DefaultView();

            m_DispUI2.Refresh();
            m_DispUI2.DefaultView();

            m_DispUI3.Refresh();
            m_DispUI3.DefaultView();
        }
    }
}
