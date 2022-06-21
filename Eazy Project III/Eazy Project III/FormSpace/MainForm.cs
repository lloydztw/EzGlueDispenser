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
using Eazy_Project_III.UISpace;
using JetEazy.GdxCore3;

namespace Eazy_Project_III
{
    public partial class MainForm : Form
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

        BannerForm BANNERFORM;

        EssUI ESSUI;
        RunUI RUNUI;
        RcpUI RCPUI;
        IniUI SETUPUI;
        CtrlUI CTRLUI;
        
        MainControlUI MAINUI;

        Timer mMainTick;
        //JzTimes mImageTime = new JzTimes();

        //string MoveString = "";
        //bool IsLiveCapturing = true;

        AccDBClass ACCDB
        {
            get
            {
                return Universal.ACCDB;
            }
        }
        EsssDBClass ESSDB
        {
            get
            {
                return Universal.ESSDB;
            }
        }
        RCPDBClass RCPDB
        {
            get
            {
                return Universal.RCPDB;
            }
        }
        RUNDBClass RUNDB
        {
            get
            {
                return Universal.RUNDB;
            }
        }

        RCPItemClass RCPItemNow
        {
            get
            {
                return RCPDB.RCPItemNow;
            }
        }

        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            this.Load += MainForm_Load;
            this.FormClosed += MainForm_FormClosed;
            this.SizeChanged += MainForm_SizeChanged;

        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            BANNERFORM = new BannerForm();
            BANNERFORM.Show();
            BANNERFORM.Refresh();

            Init();

            BANNERFORM.Close();
            BANNERFORM.Dispose();

            Universal.MainFormLocation = new Point(this.Location.X, this.Location.Y);

#if OPT_LETIAN_DEBUG
            // To fit into my screen for debug.
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
#endif

            _show_simulation_info_to_log();
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //@LETIAN:
            //  最後補漏:
            //  有時候程式退出時
            //      ESSStatusEnum.EXIT 不會被觸發!
            GdxCore.Dispose();
        }
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //@LETIAN
            _auto_layout();
        }


        void Init()
        {
            switch(VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            this.Text = "第三站点胶 Ver:" + Application.ProductVersion;
                            break;
                        case OptionEnum.DISPENSINGX1:
                            this.Text = "第一站点胶 Ver:" + Application.ProductVersion;
                            break;
                        case OptionEnum.DISPENSINGX2:
                            this.Text = "第二站点胶 Ver:" + Application.ProductVersion;
                            break;
                        case OptionEnum.DISPENSINGX4:
                            this.Text = "第四站点胶 Ver:" + Application.ProductVersion;
                            break;
                        default:
                            this.Text = "宇宙无敌XXX Ver:" + Application.ProductVersion;
                            break;
                    }

                    break;
            }

            INI.Instance.Initial();
            Universal.Initial(0);

            ESSUI = essUI1;
            RUNUI = runUI1;
            RCPUI = rcpUI1;
            SETUPUI = iniUI1;
            CTRLUI = ctrlUI1;
            MAINUI = mainControlUI1;

            RUNUI.Location = new Point(1212, 237);
            RCPUI.Location = RUNUI.Location;
            SETUPUI.Location = RUNUI.Location;
            //USERLOTUI.Location = CTRLUI.Location;
            //CTRLALLREGIONUI.Location = CTRLUI.Location;

            InitialESSUI();
            InitialRCPUI();
            InitialSETUPUI();
            InitialCTRLUI();
            InitialMAINUI();
            //InitialRESULT();
            InitialRUNUI();

            mMainTick = new Timer();
            mMainTick.Interval = 20;
            mMainTick.Enabled = true;
            mMainTick.Tick += MMainTick_Tick;

#if !OPT_LETIAN_DEBUG
            // 很慢
            LanguageExClass.Instance.EnumControls(this);
#endif

        }

        void InitialESSUI()
        {
            ESSUI.Initial(ESSDB, ACCDB, Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION, 200);
            //ESSUI.Set111(Universal.WORKPATH + "\\111.BMP");
            ESSUI.SetRecipeCombo(RCPDB.GetRecipeStringList());
            ESSUI.TriggerAction += new EssUI.TriggerHandler(ESSUI_TriggerAction);

            ESSUI.SetMainStatus(ESSStatusEnum.RUN);
        }
        void InitialRUNUI()
        {
            RUNUI.Initial(Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION);
            RUNUI.TriggerAction += new RunUI.TriggerHandler(RUNUI_TriggerAction);
        }
        void InitialRCPUI()
        {
            RCPUI.Initial(Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION, RCPDB);
            RCPUI.TriggerAction += new RcpUI.TriggerHandler(RCPUI_TriggerAction);
            RCPUI.TriggerActionForSetupDetail += new RcpUI.TriggerHandlerForSetupDetail(RCPUI_TriggerActionForSetupDetail);
        }
        void InitialSETUPUI()
        {
            SETUPUI.Initial(Universal.UIPATH, INI.Instance.LANGUAGE, Universal.VERSION, Universal.OPTION);
            SETUPUI.TriggerAction += new IniUI.TriggerHandler(SETUPUI_TriggerAction);
            SETUPUI.TriggerStringAction += SETUPUI_TriggerStringAction;
        }
        private void SETUPUI_TriggerStringAction(string statusstr)
        {
            //MoveString = statusstr;
        }

        void InitialCTRLUI()
        {
            CTRLUI.BackColor = SystemColors.Control;
            CTRLUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
        }
        void InitialMAINUI()
        {
            MAINUI.BackColor = SystemColors.Control;
            MAINUI.Initial(VERSION, OPTION, MACHINECollection.MACHINE);
        }

        void ESSUI_TriggerAction(ESSStatusEnum status)
        {
            switch (status)
            {
                case ESSStatusEnum.EXIT:

                    MAINUI.Close();
                    Universal.Close();
                    this.Close();

                    break;
                case ESSStatusEnum.RUN:
                case ESSStatusEnum.RECIPE:
                case ESSStatusEnum.SETUP:

                    RUNUI.Visible = status == ESSStatusEnum.RUN;
                    RCPUI.Visible = status == ESSStatusEnum.RECIPE;
                    SETUPUI.Visible = status == ESSStatusEnum.SETUP;
                    //USERLOTUI.Visible = status == ESSStatusEnum.RUN;

                    //CTRLUI.Visible = (status == ESSStatusEnum.SETUP);
                    //CTRLALLREGIONUI.Visible = (status == ESSStatusEnum.RECIPE && Universal.OPT == OptionEnum.AUTO && INI.MistDebugging);

                    break;
                case ESSStatusEnum.LOGIN:

                    //picResult.Visible = false;
                    //btnOK.Visible = false;

                    if (ACCDB.AccNow.IsAllowSetupINI)
                    {
                        //MAINUI.Enabled = true;
                        CTRLUI.SetEnable(true);
                    }

                    break;
                case ESSStatusEnum.LOGOUT:

                    //picResult.Visible = false;
                    //btnOK.Visible = false;

                    //MAINUI.Enabled = false;
                    CTRLUI.SetEnable(false);

                    break;
                case ESSStatusEnum.RESET:

                    //if (MessageBox.Show("是否要將所有馬達歸位?", "SYS", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    //    RESULT.StartResetMotorProcess();

                    //if (USEIO.IsProjectorOnsite)
                    //{
                    //    MessageBox.Show("請移開光機. ");
                    //    return;
                    //}


                    //if (RESULT.IsResetProcessOn)
                    //{
                    //    if (MessageBox.Show("是否要停止重置流程?", "SYS", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    //        return;
                    //}
                    //else
                    //    if (MessageBox.Show("請將所有光機清空才可繼續，是否要開始重置流程?", "SYS", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    //        return;

                    //RESULT.StartResetProcess();

                    break;
                case ESSStatusEnum.RECIPESELECTED:

                    //RCPDB.GetRCPItem(ESSDB.LastRecipeIndex);
                    RCPDB.Indicator = ESSDB.LastRecipeIndex;

                    RecipeCHClass.Instance.ChangeIndex(ESSDB.LastRecipeIndex);

                    //ViewPreload();
                    RCPUI.ChangeRecipe(true);
                    //ESSDB.RecipeChange(RCPItemNow.Index);
                    //RESULT.InitialBMPResult();
                    //DisplayStatus = DisplayStatusEnum.LIVE;

                    break;
                case ESSStatusEnum.FASTCAL:

                    //if (!PhotoMainProcess.IsOn)
                    //    PhotoMainProcess.Start();

                    //TestMethod = TestMethodEnum.BUTTON;

                    //RUNUI.SetBarcodeEnable = false;

                    //RESULT.StartCalProcess(VIEW,
                    //    TestMethod,
                    //    RUNUI.IsSaveRaw,
                    //    RUNUI.IsSaveNGRaw,
                    //    RUNUI.IsSaveDebug,
                    //    RUNUI.GetOPBarcode(),
                    //    Universal.IsDebug,
                    //    RUNUI.GetProductBarcode());

                    break;
            }
        }
        void SETUPUI_TriggerAction(INIStatusEnum status)
        {
            switch (status)
            {
                case INIStatusEnum.CHANGELANGUAGE:

                    ESSUI.SetLanguage(INI.Instance.LANGUAGE);
                    SETUPUI.SetLanguage(INI.Instance.LANGUAGE);

                    break;
                case INIStatusEnum.EDIT:
                    ESSUI.Disable = true;

                    //MoveString = "";
                    //m_DispUI.SetDisplayType(DisplayTypeEnum.ADJUST);

                    break;
                case INIStatusEnum.EXIT:
                    ESSUI.Disable = false;

                    //m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);

                    break;

            }
        }
        void RCPUI_TriggerAction(RCPStatusEnum status)
        {
            switch (status)
            {
                case RCPStatusEnum.EDIT:
                    ESSUI.Disable = true;
                    break;
                case RCPStatusEnum.MODIFYCOMPLETE:
                    ESSUI.Disable = false;
                    ESSDB.RecipeChange(RCPItemNow.Index);
                    //RESULT.InitialBMPResult();
                    ESSUI.SetRecipeCombo(RCPDB.GetRecipeStringList());

                    ESSUI.FillDisplay();

                    //For Test Only
                    //RUNUI.InitialRun(VIEW);

                    break;
                case RCPStatusEnum.MODIFYCANCEL:
                    ESSUI.Disable = false;
                    break;
            }
        }
        //DetailForm DETAILFRM;
        void RCPUI_TriggerActionForSetupDetail(RCPStatusEnum status, int setupindex)
        {
            switch (status)
            {
                case RCPStatusEnum.SHOWDETAIL:
                    break;
            }
        }
        void RUNUI_TriggerAction(RunStatusEnum Status)
        {
            switch (Status)
            {
                case RunStatusEnum.STARTRUN:
                    break;
                case RunStatusEnum.SHINNIGEND:
                    break;
            }
        }

        //主程序扫描时间
        JzTimes JzMainScanTime = new JzTimes();
        int JzScanTimeMS = 0;

        private void MMainTick_Tick(object sender, EventArgs e)
        {
            JzScanTimeMS = JzMainScanTime.msDuriation;
            JzMainScanTime.Cut();

            MACHINECollection.Tick();

            ESSUI.Tick();
            CTRLUI.Tick();
            RUNUI.Tick();
            MAINUI.Tick();

            ESSUI.ShowPLC_RxTime(Universal.VersionDate + "_" +
                                 Universal.OPTION.ToString() + " " +
                                 JzScanTimeMS.ToString() + "ms " +
                                 MACHINECollection.PLCFps());
        }

        private void _show_simulation_info_to_log()
        {
            //@ LETIAN:20220622
            if (Universal.IsNoUseIO)
                CommonLogClass.Instance.LogMessage("模擬 PLC", Color.OrangeRed);
            if (Universal.IsNoUseMotor)
                CommonLogClass.Instance.LogMessage("模擬 Motor", Color.OrangeRed);
            for (int i = 0, N = Universal.CAMERAS.Length; i < N; i++)
            {
                if (Universal.CAMERAS[i].IsSim())
                    CommonLogClass.Instance.LogMessage("模擬 Cam" + i, Color.OrangeRed);
            }
        }
        private void _auto_layout()
        {
#if OPT_LETIAN_DEBUG
            // 暫時, 自動調整 layout
            // To be continued.
            var rcc = ClientRectangle;
            mainControlUI1.Width = rcc.Width - essUI1.Width - 2;
            mainControlUI1.Height = rcc.Height;
            ctrlUI1.Height = rcc.Bottom - ctrlUI1.Top;
            var panels = new Control[]
            {
                essUI1,
                runUI1,
                rcpUI1,
                iniUI1,
                ctrlUI1,
            };
            foreach (var panel in panels)
            {
                panel.Left = rcc.Width - panel.Width;
            }
#endif
        }
    }
}
