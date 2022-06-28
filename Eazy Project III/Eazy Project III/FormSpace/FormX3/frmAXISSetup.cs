using Common;
using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_Measure;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VsCommon.ControlSpace;

namespace Eazy_Project_III.FormSpace
{
    public partial class frmAXISSetup : Form
    {

        const int AXIS_COUNT = 9;
        VsTouchMotorUI[] VSAXISUI = new VsTouchMotorUI[AXIS_COUNT];
        MotionTouchPanelUIClass[] AXISUI = new MotionTouchPanelUIClass[AXIS_COUNT];

        Timer mMotorTimer = null;

        Button btnManualAuto;
        Button btnLESnap;
        TextBox txtHeightData;
        Button btnCapturePlaneHeight;
        Button btnLEAttractMeasure;
        Button btnQCLaserMeasure;


        #region 点胶模组操作

        Button btnDispeningGo;
        Button btnDispeningHome;
        Button btnDispeningManual;
        ComboBox cboDispensingTimeList;
        Button btnOnekeyDispensing;

        #endregion


        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        DispensingMachineClass MACHINE
        {
            get { return (DispensingMachineClass)MACHINECollection.MACHINE; }
        }

        public frmAXISSetup()
        {
            InitializeComponent();

            this.TopMost = true;

            this.Load += FrmAXISSetup_Load;
            this.FormClosed += FrmAXISSetup_FormClosed;
        }

        private void FrmAXISSetup_FormClosed(object sender, FormClosedEventArgs e)
        {
            //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
            //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
            //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));

            //MACHINE.SetNormalTemp(false);
            Universal.IsOpenMotorWindows = false;
        }

        private void FrmAXISSetup_Load(object sender, EventArgs e)
        {
            this.Text = "轴设定视窗";
            Init();
        }

        void Init()
        {
            tabPage4.Text = "调试操作界面";
            btnManualAuto = button1;
            btnLESnap = button2;
            txtHeightData = textBox1;
            btnCapturePlaneHeight = button3;
            btnLEAttractMeasure = button4;

            btnDispeningGo = button5;
            btnDispeningHome = button6;
            btnDispeningManual = button7;
            cboDispensingTimeList = comboBox1;
            btnOnekeyDispensing = button8;
            btnQCLaserMeasure = button9;

            btnManualAuto.Click += BtnManualAuto_Click;
            btnLESnap.Click += BtnLESnap_Click;
            btnCapturePlaneHeight.Click += BtnCapturePlaneHeight_Click;
            btnLEAttractMeasure.Click += BtnLEAttractMeasure_Click;
            btnQCLaserMeasure.Click += BtnQCLaserMeasure_Click;

            btnDispeningGo.Click += BtnDispeningGo_Click;
            btnDispeningHome.Click += BtnDispeningHome_Click;
            btnDispeningManual.Click += BtnDispeningManual_Click;
            btnOnekeyDispensing.Click += BtnOnekeyDispensing_Click;

            #region 位置设定控件

            tabPage1.Text = "模组1 吸嘴XYZ(AXIS 012)";
            tabPage2.Text = "模组2 点胶XYZ(AXIS 345)";
            tabPage3.Text = "模组3 微调Zθ-Yθ-Z(AXIS 678)";

            VSAXISUI[0] = vsTouchMotorUI3;
            VSAXISUI[1] = vsTouchMotorUI2;
            VSAXISUI[2] = vsTouchMotorUI1;
            VSAXISUI[3] = vsTouchMotorUI6;
            VSAXISUI[4] = vsTouchMotorUI5;
            VSAXISUI[5] = vsTouchMotorUI4;
            VSAXISUI[6] = vsTouchMotorUI9;
            VSAXISUI[7] = vsTouchMotorUI8;
            VSAXISUI[8] = vsTouchMotorUI7;

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i] = new MotionTouchPanelUIClass(VSAXISUI[i]);
                AXISUI[i].Initial(MACHINE.PLCMOTIONCollection[i]);

                i++;
            }

            #endregion

            mMotorTimer = new Timer();
            mMotorTimer.Interval = 50;
            mMotorTimer.Enabled = true;
            mMotorTimer.Tick += MMotorTimer_Tick;

            MACHINE.TriggerAction += MACHINE_TriggerAction;

            PG_PosSafe.SelectedObject = MotorConfig.XPropsInstance;

            //FillDisplay();

            //LanguageExClass.Instance.EnumControls(this);
        }

        int MainMirrorIndex = 0;//左邊還是右邊  0左邊 1右邊

        private void BtnQCLaserMeasure_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0 && !Universal.IsNoUseIO)
            {
                VsMSG.Instance.Warning("手动模式下，无法启动，请检查。");
                return;
            }

            if (!qclasermeasureprocess.IsOn)
            {
                string msg0 = "是否要進行QC鐳射量測 (手動輸入)";
                if (VsMSG.Instance.Question(msg0) == DialogResult.OK)
                {
                    using (var frm = new FormQCLaserMeasurement(0))
                    {
                        frm.ShowDialog(this);
                        return;
                    }
                }
            }

            string onStrMsg = "是否要進行QC鐳射量測？";
            string offStrMsg = "是否要停止QC鐳射量測？";
            string msg = (qclasermeasureprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!qclasermeasureprocess.IsOn)
                {
                    frmUserSelect myUserSelectForm = new frmUserSelect(false);
                    this.TopMost = false;
                    if (myUserSelectForm.ShowDialog() == DialogResult.OK)
                    {
                        //MainGroupIndex = myUserSelectForm.GetIndex;
                        MainMirrorIndex = myUserSelectForm.PutIndex;
                        //MainAloneToMirror = myUserSelectForm.IsAloneToMirror;

                        qclasermeasureprocess.Start();
                    }
                    this.TopMost = true;
                    myUserSelectForm.Dispose();
                    myUserSelectForm = null;
                }
                else
                {
                    qclasermeasureprocess.Stop();
                }
            }
        }

        private void BtnOnekeyDispensing_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法采集，请检查。");
                return;
            }

            string onStrMsg = "是否要測試點膠？";
            string offStrMsg = "是否要停止測試點膠？";
            string msg = (dispensingonekeyprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!dispensingonekeyprocess.IsOn)
                    dispensingonekeyprocess.Start();
                else
                    dispensingonekeyprocess.Stop();
            }
        }

        int m_NextTimeTemp = 200;

        int m_DispensingIndex = 0;
        List<string> m_DispensingRunList = new List<string>();

        ProcessClass dispensingonekeyprocess = new ProcessClass();
        void DispensingOnekeyTick()
        {
            ProcessClass Process = dispensingonekeyprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        m_DispensingIndex = 0;
                        m_DispensingRunList.Clear();

                        //设定点胶时间&UV时间
                        MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1091, RecipeCHClass.Instance.DispensingTime);
                        //MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1092, RecipeCHClass.Instance.UVTime);

                        foreach (string str in INI.Instance.MirrorTestDispensingPosList)
                            m_DispensingRunList.Add(str);

                        //點膠Z 下降位置
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 9, INI.Instance.sMirrorTestDispensingReady);

                        Process.NextDuriation = m_NextTimeTemp;
                        Process.ID = 10;


                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //开始循环设定 点胶位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 8, m_DispensingRunList[m_DispensingIndex]);

                            CommonLogClass.Instance.LogMessage("一鍵測試點膠 Index=" + m_DispensingIndex.ToString(), Color.Black);

                            Process.NextDuriation = m_NextTimeTemp;
                            Process.ID = 20;
                        }
                        break;

                    case 20:
                        if (Process.IsTimeup)
                        {
                            //点胶启动
                            MACHINE.PLCIO.SetIO(IOConstClass.QB1551, true);

                            CommonLogClass.Instance.LogMessage("一鍵測試點膠 Index" + m_DispensingIndex.ToString(), Color.Black);
                            m_DispensingIndex++;

                            Process.NextDuriation = m_NextTimeTemp;
                            Process.ID = 30;
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1551))
                            {
                                //单个点胶完成
                                if (m_DispensingIndex < m_DispensingRunList.Count)
                                {
                                    Process.NextDuriation = m_NextTimeTemp;
                                    Process.ID = 10;
                                }
                                else
                                {
                                    Process.NextDuriation = m_NextTimeTemp;
                                    Process.ID = 40;

                                    CommonLogClass.Instance.LogMessage("一鍵測試點膠完成", Color.Black);


                                    dispensinghomeprocess.Start();

                                    CommonLogClass.Instance.LogMessage("一鍵測試點膠回待命", Color.Black);
                                }
                            }
                        }
                        break;


                    case 15:
                        if (Process.IsTimeup)
                        {
                            if(!dispensinghomeprocess.IsOn)
                            {
                                Process.Stop();

                                CommonLogClass.Instance.LogMessage("一鍵測試點膠回完成", Color.Black);
                            }
                        }
                        break;
                }
            }
        }

        private void BtnDispeningManual_Click(object sender, EventArgs e)
        {
            int delaytime = 1;
            bool bOK = int.TryParse(cboDispensingTimeList.Text, out delaytime);
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
                Task task = new Task(() =>
                {
                    MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_SWITCH_DISPENSING, true);
                    System.Threading.Thread.Sleep(itime);
                    MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_SWITCH_DISPENSING, false);
                });
                task.Start();
            }
        }

        private void BtnDispeningHome_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法采集，请检查。");
                return;
            }

            string onStrMsg = "是否要點膠模組回待命？";
            string offStrMsg = "是否要停止膠模組回待命？";
            string msg = (dispensinghomeprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!dispensinghomeprocess.IsOn)
                    dispensinghomeprocess.Start();
                else
                    dispensinghomeprocess.Stop();
            }
        }

        private void BtnDispeningGo_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法采集，请检查。");
                return;
            }

            string onStrMsg = "是否要點膠模組定位至避光槽？";
            string offStrMsg = "是否要停止點膠模組定位至避光槽？";
            string msg = (dispensinggoprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!dispensinggoprocess.IsOn)
                    dispensinggoprocess.Start();
                else
                    dispensinggoprocess.Stop();
            }
        }

        ProcessClass dispensinggoprocess = new ProcessClass();
        void DispensingGoTick()
        {
            ProcessClass Process = dispensinggoprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        //寫入避光槽位置
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 7, INI.Instance.ShadowPos);


                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {

                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_DISPENSING, 7);

                            Process.NextDuriation = 500;
                            Process.ID = 15;
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_DISPENSING, 7))
                            {
                                Process.Stop();
                            }
                        }
                        break;
                }
            }
        }
        ProcessClass dispensinghomeprocess = new ProcessClass();
        void DispensingHomeTick()
        {
            ProcessClass Process = dispensinghomeprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 1000;
                        Process.ID = 10;

                        MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);

                        break;
                    case 10:
                        if(Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_DISPENSING, 6) || Universal.IsNoUseIO)
                            {
                                Process.Stop();
                            }
                        }
                        break;
                }
            }
        }


        private void BtnLEAttractMeasure_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法采集，请检查。");
                return;
            }

            string onStrMsg = "是否要測量鐳雕頭與吸嘴的相對位置？";
            string offStrMsg = "是否要停止測量鐳雕頭與吸嘴的相對位置？";
            string msg = (leattractprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!leattractprocess.IsOn)
                    leattractprocess.Start();
                else
                    leattractprocess.Stop();
            }
        }

        bool IsEMCTriggered = false;

        private void MACHINE_TriggerAction(MachineEventEnum machineevent)
        {
            switch (machineevent)
            {
                case MachineEventEnum.ALARM_SERIOUS:
                    //IsAlarmsSeriousX = true;
                    //SetAbnormalLight();
                    break;
                case MachineEventEnum.ALARM_COMMON:
                    //IsAlarmsCommonX = true;
                    //SetAbnormalLight();
                    break;
                case MachineEventEnum.EMC:
                    IsEMCTriggered = true;
                    break;
            }
        }

        private void BtnCapturePlaneHeight_Click(object sender, EventArgs e)
        {
            //判断是否在手动状态
            if (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 0)
            {
                VsMSG.Instance.Warning("手动模式下，无法采集，请检查。");
                return;
            }

            string onStrMsg = "是否要进行块规平面度采集测试？";
            string offStrMsg = "是否要停止块规平面度采集测试？";
            string msg = (m_PlaneHeightprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_PlaneHeightprocess.IsOn)
                    m_PlaneHeightprocess.Start();
                else
                    m_PlaneHeightprocess.Stop();
            }
        }

        private void BtnLESnap_Click(object sender, EventArgs e)
        {
            double dHeight = LEClass.Instance.Snap();
            txtHeightData.Text = dHeight.ToString();
        }

        private void BtnManualAuto_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1090, MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? 0 : 1);
        }

        int m_PlaneIndex = 0;
        /// <summary>
        /// 缓存平面度需要到达的位置
        /// </summary>
        List<string> m_PlaneRunList = new List<string>();
        /// <summary>
        /// 缓存平面度量测的高度
        /// </summary>
        List<string> m_PlaneRunDataList = new List<string>();

        ProcessClass m_PlaneHeightprocess = new ProcessClass();
        void PlaneHeightTick()
        {
            ProcessClass Process = m_PlaneHeightprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        m_PlaneRunDataList.Clear();

                        m_PlaneIndex = 0;
                        m_PlaneRunList.Clear();
                        foreach (string str in INI.Instance.Mirror0PlanePosList)
                            m_PlaneRunList.Add(str);

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //开始循环设定 块规 平面度位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, m_PlaneRunList[m_PlaneIndex]);

                            CommonLogClass.Instance.LogMessage("块规平面度位置设定 Index=" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = 500;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //GO
                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                            CommonLogClass.Instance.LogMessage("启动 Index" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = 500;
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

                                Process.NextDuriation = 500;
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
                                Process.NextDuriation = 500;
                                Process.ID = 10;
                            }
                            else
                            {

                                Process.Stop();
                                //将采集的数据 填充到list中 保存

                                INI.Instance.sMirror0PlaneHeightPosList = string.Empty;
                                //INI.Instance.Mirror0PlaneHeightPosList.Clear();
                                foreach (string str in m_PlaneRunDataList)
                                    INI.Instance.sMirror0PlaneHeightPosList += str + ";";

                                INI.Instance.sMirror0PlaneHeightPosList = RemoveLastChar(INI.Instance.sMirror0PlaneHeightPosList, 1);
                                INI.Instance.SavePlaneHeight();

                                CommonLogClass.Instance.LogMessage("块规平面度资料采集完成", Color.Black);
                            }
                        }
                        break;
                }
            }
        }

        double d_disLE_x = 0;
        ProcessClass leattractprocess = new ProcessClass();
        void LEAttractTick()
        {
            ProcessClass Process = leattractprocess;

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
                            //設定 鐳射的 位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, INI.Instance.LEPos);

                            CommonLogClass.Instance.LogMessage("設定鐳射頭至塊歸位置：" + INI.Instance.LEPos, Color.Black);

                            Process.NextDuriation = 500;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //GO
                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                            CommonLogClass.Instance.LogMessage("鐳射頭至塊歸位置 启动", Color.Black);

                            Process.NextDuriation = 500;
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
                                string[] lepos = INI.Instance.LEPos.Split(',').ToArray();

                                d_disLE_x = double.Parse(lepos[0]) + z;

                                CommonLogClass.Instance.LogMessage("鐳射頭至塊歸距離：" + d_disLE_x.ToString("0.000"), Color.Black);


                                string[] attractpos = INI.Instance.AttractPos.Split(',').ToArray();
                                double offset_x = d_disLE_x - double.Parse(attractpos[0]);

                                INI.Instance.Offset_LEAttract = offset_x;
                                INI.Instance.Save();

                                //INI.Instance.sMirrorPutAdjDeep1Length = attractpos.Length;

                                CommonLogClass.Instance.LogMessage("鐳射頭與吸嘴相對距離：" + offset_x.ToString("0.000"), Color.Black);

                                Process.NextDuriation = 500;
                                Process.ID = 40;
                            }
                        }
                        break;

                    #region INITIAL POS

                    case 40:
                        if (Process.IsTimeup)
                        {
                            Process.NextDuriation = 500;
                            Process.ID = 50;

                            MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                            CommonLogClass.Instance.LogMessage("LETICK吸嘴模组回待命启动", Color.Black);
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("LETICK吸嘴模组待命完成", Color.Black);
                                Process.Stop();

                                //Process.NextDuriation = 500;
                                //Process.ID = 60;

                            }
                        }
                        break;

                    #endregion

                    #region 測試吸嘴位置

                    //case 60:
                    //    if (Process.IsTimeup)
                    //    {
                    //        //設定 鐳射的 位置
                    //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, INI.Instance.AttractPos);

                    //        CommonLogClass.Instance.LogMessage("設定吸嘴至塊歸位置：" + INI.Instance.AttractPos, Color.Black);

                    //        Process.NextDuriation = 500;
                    //        Process.ID = 70;
                    //    }
                    //    break;
                    //case 70:
                    //    if (Process.IsTimeup)
                    //    {
                    //        //GO
                    //        MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                    //        CommonLogClass.Instance.LogMessage("吸嘴至塊歸位置 启动", Color.Black);

                    //        Process.NextDuriation = 500;
                    //        Process.ID = 80;
                    //    }
                    //    break;
                    //case 80:
                    //    if (Process.IsTimeup)
                    //    {
                    //        if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                    //        {
                    //            string[] attractpos = INI.Instance.AttractPos.Split(',').ToArray();
                    //            double offset_x = d_disLE_x - double.Parse(attractpos[0]);

                    //            INI.Instance.Offset_LEAttract = offset_x;
                    //            INI.Instance.Save();

                    //            //INI.Instance.sMirrorPutAdjDeep1Length = attractpos.Length;

                    //            CommonLogClass.Instance.LogMessage("鐳射頭與吸嘴相對距離：" + offset_x.ToString("0.000"), Color.Black);

                    //            Process.NextDuriation = 500;
                    //            Process.ID = 90;
                    //        }
                    //    }
                    //    break;

                    #endregion

                    #region INITIAL POS

                    //case 90:
                    //    if (Process.IsTimeup)
                    //    {
                    //        Process.NextDuriation = 500;
                    //        Process.ID = 100;

                    //        MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                    //        //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                    //        //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                    //        CommonLogClass.Instance.LogMessage("LETICK-2吸嘴模组回待命启动", Color.Black);
                    //    }
                    //    break;
                    //case 100:
                    //    if (Process.IsTimeup)
                    //    {
                    //        if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                    //        {
                    //            CommonLogClass.Instance.LogMessage("LETICK-2吸嘴模组待命完成", Color.Black);

                    //            Process.Stop();

                    //        }
                    //    }
                    //    break;

                        #endregion

                }
            }
        }
        ProcessClass qclasermeasureprocess = new ProcessClass();
        void QcLaserMeasureTick()
        {
            ProcessClass Process = qclasermeasureprocess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        double x = 0;
                        double y = 0;
                        double z = 0;

                        //獲取坐標
                        GdxCore.GetQCMotorPos(MainMirrorIndex, out x, out y, out z);
                        CommonLogClass.Instance.LogMessage("QC鐳射量測 " + (MainMirrorIndex == 0 ? "左邊" : "右邊"), Color.Black);
                        CommonLogClass.Instance.LogMessage("QC鐳射量測 讀取坐標={" + x.ToString() + "," + y.ToString() + "," + z.ToString() + "}", Color.Black);

                        //BY PASS  先看坐標是否正常 確定沒問題再實作  Gaara
                        Process.Stop();
                        return;
                        
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, x.ToString() + "," + y.ToString() + "," + z.ToString());

                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);

                            Process.NextDuriation = 500;
                            Process.ID = 15;
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1) && !Universal.IsNoUseMotor)
                            {
                                //读数据 
                                //@LETIAN: 雷射讀值 (命名 laserZ 以防與 馬達 XYZ 搞混)
                                double laserZ = ax_read_laser();
                                if (Math.Abs(laserZ) < 0.0001)
                                {
                                    CommonLogClass.Instance.LogMessage("雷射讀值異常", Color.Red);
                                    //_LOG("雷射讀值異常", Color.Red);
                                    Process.NextDuriation = 500;
                                    Process.ID = 3020;
                                    return;
                                }

                                if (MainMirrorIndex == 0)
                                {
                                    INI.Instance.Mirror1_Offset_Adj = laserZ;
                                }
                                else
                                {
                                    INI.Instance.Mirror2_Offset_Adj = laserZ;
                                }
                                INI.Instance.SaveQCLaser();

                                string err = GdxCore.SetQcLaserMeasurement(MainMirrorIndex, laserZ);
                                if (err != null)
                                {
                                    CommonLogClass.Instance.LogMessage("QC雷射設定異常: " + err, Color.Red);
                                    //_LOG("雷射讀值異常", Color.Red);
                                    Process.NextDuriation = 500;
                                    Process.ID = 3020;
                                    return;
                                }

                                Process.NextDuriation = 500;
                                Process.ID = 4010;

                            }
                        }
                        break;

                    #region INITIAL POS

                    case 3010:  //@LETIAN: 馬達定位沒到達 INI 所指定位置 (重複利用 4010 退出程序)
                    case 3020:  //@LETIAN: 雷射讀值異常 
                    case 4010:  //平面度超標
                        if (Process.IsTimeup)
                        {
                            Process.NextDuriation = 500;
                            Process.ID = 4020;

                            MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                            CommonLogClass.Instance.LogMessage("吸嘴模组回待命启动", Color.Black);
                        }
                        break;
                    case 4020:  //吸嘴模组待命完成
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("吸嘴模组待命完成", Color.Black);
                                Process.Stop();
                            }
                        }
                        break;

                        #endregion
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

        public void StopAllProcess(string eStrMode = "")
        {
            m_PlaneHeightprocess.Stop();
            qclasermeasureprocess.Stop();
        }

        private void MMotorTimer_Tick(object sender, EventArgs e)
        {
            if (!Universal.IsNoUseIO)
            {
                if (IsEMCTriggered)
                {
                    //SetAbnormalLight();

                    IsEMCTriggered = false;
                    StopAllProcess();
                    //OnTrigger(ActionEnum.ACT_ISEMC, "");
                }
            }

            PlaneHeightTick();
            LEAttractTick();
            QcLaserMeasureTick();

            DispensingHomeTick();
            DispensingGoTick();
            DispensingOnekeyTick();

            btnManualAuto.BackColor = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? Color.Red : Color.Lime);
            btnManualAuto.Text = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? "自动模式" : "手动模式");

            btnCapturePlaneHeight.BackColor = (m_PlaneHeightprocess.IsOn ? Color.Red : Color.Lime);
            btnCapturePlaneHeight.Text = (m_PlaneHeightprocess.IsOn ? "块规平面度采集中" : "建立块规平面流程");

            btnLEAttractMeasure.BackColor = (leattractprocess.IsOn ? Color.Red : Color.Lime);
            btnLEAttractMeasure.Text = (leattractprocess.IsOn ? "測量中" : "自動校正鐳射頭與吸嘴相對位置");

            btnDispeningGo.BackColor = (dispensinggoprocess.IsOn ? Color.Red : Color.Lime);
            btnDispeningHome.BackColor = (dispensinghomeprocess.IsOn ? Color.Red : Color.Lime);
            btnOnekeyDispensing.BackColor = (dispensinghomeprocess.IsOn ? Color.Red : Color.Lime);

            btnCapturePlaneHeight.Text = LanguageExClass.Instance.ToTraditionalChinese(btnCapturePlaneHeight.Text);
            btnManualAuto.Text = LanguageExClass.Instance.ToTraditionalChinese(btnManualAuto.Text);

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i].Tick();
                i++;
            }
        }

        string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }

        private void btnTopMost_Click(object sender, EventArgs e)
        {
            //this.TopMost = !this.TopMost;
            //btnTopMost.BackColor = (this.TopMost ? Color.Red : Control.DefaultBackColor);
        }

        private void btnWriteToPlc_Click(object sender, EventArgs e)
        {

            string msg = "是否要向plc写入安全位置参数";
            if (VsMSG.Instance.Question(msg) != DialogResult.OK)
            {
                return;
            }

            MotorConfig.Instance.Save();

            int i = 0;
            while (i < 20)
            {
                if (i == 6 || i == 7 || i == 8)
                {
                    MACHINE.PLCIO.SetMWIndex(1340 + i, MotorConfig.Instance.PosSafe[i]);
                }
                else
                {
                    MACHINE.PLCIO.SetMWIndex(1340 + i, MotorConfig.Instance.PosSafe[i] * 100);
                }

                i++;
            }

            MACHINE.PLCIO.SetIO("0:QB1549.0", MotorConfig.Instance.PogoPinMotorMode);
            MACHINE.PLCIO.SetMWIndex(1370, MotorConfig.Instance.VirtureZero);
            MACHINE.PLCIO.SetMWIndex(1372, MotorConfig.Instance.TheaYVirtureZero);
            MACHINE.PLCIO.SetMWIndex(1373, MotorConfig.Instance.TheaZVirtureZero);
            MACHINE.PLCIO.SetMWIndex(1371, MotorConfig.Instance.Mirror0Thickness);
            MACHINE.PLCIO.SetMWIndex(1371, MotorConfig.Instance.Mirror1Thickness);

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        #region PRIVATE FUNTION

        /// <summary>
        /// 讀取雷測量測距離 <br/>
        /// @LETIAN: 包裝為 function 準備將來模擬使用
        /// </summary>
        private double ax_read_laser()
        {
            if (!GdxGlobal.Facade.IsSimPLC())
            {
                return LEClass.Instance.Snap();
            }
            else
            {
                //@ Gaara 看以後是否由 LEClass 直接 Math.Round
                var laser = GdxGlobal.Facade.GetLaser();
                double dist = laser.Snap();
                return System.Math.Round(dist, 4);
            }
        }

        #endregion
    }
}
