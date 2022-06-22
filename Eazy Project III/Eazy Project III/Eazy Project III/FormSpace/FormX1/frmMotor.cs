using Common;
using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using JetEazy;
using JetEazy.BasicSpace;
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

namespace Eazy_Project_III.FormSpace.FormX1
{
    public partial class frmMotor : Form
    {

        const int AXIS_COUNT = 5;
        VsTouchMotorUI[] VSAXISUI = new VsTouchMotorUI[AXIS_COUNT];
        MotionTouchPanelUIClass[] AXISUI = new MotionTouchPanelUIClass[AXIS_COUNT];

        Timer mMotorTimer = null;

        #region 点胶模组操作

        Button btnManualAuto;

        Button btnDispeningGo;
        Button btnDispeningHome;
        Button btnDispeningManual;
        ComboBox cboDispensingTimeList;

        #endregion

        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        DispensingX1MachineClass MACHINE
        {
            get { return (DispensingX1MachineClass)MACHINECollection.MACHINE; }
        }

        public frmMotor()
        {
            InitializeComponent();

            this.TopMost = true;

            this.Load += FrmMotor_Load;
            this.FormClosed += FrmMotor_FormClosed;
        }

        private void FrmMotor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Universal.IsOpenMotorWindows = false;
        }

        private void FrmMotor_Load(object sender, EventArgs e)
        {
            this.Text = "轴设定视窗";
            Init();
        }

        void Init()
        {
            #region 位置设定控件

            tabPage1.Text = "模組1 點膠XYZ(AXIS 012)";
            tabPage2.Text = "模組2 (AXIS 34)";

            VSAXISUI[0] = vsTouchMotorUI3;
            VSAXISUI[1] = vsTouchMotorUI2;
            VSAXISUI[2] = vsTouchMotorUI1;
            VSAXISUI[3] = vsTouchMotorUI6;
            VSAXISUI[4] = vsTouchMotorUI5;

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i] = new MotionTouchPanelUIClass(VSAXISUI[i]);
                AXISUI[i].Initial(MACHINE.PLCMOTIONCollection[i]);

                i++;
            }

            cboDispensingTimeList = comboBox1;

            btnManualAuto = button1;
            btnDispeningManual = button7;

            btnManualAuto.Click += BtnManualAuto_Click;
            btnDispeningManual.Click += BtnDispeningManual_Click;

            #endregion

            mMotorTimer = new Timer();
            mMotorTimer.Interval = 50;
            mMotorTimer.Enabled = true;
            mMotorTimer.Tick += MMotorTimer_Tick;

            MACHINE.TriggerAction += MACHINE_TriggerAction;

            //PG_PosSafe.SelectedObject = MotorConfig.XPropsInstance;

            //FillDisplay();

            //LanguageExClass.Instance.EnumControls(this);
        }

        private void BtnDispeningManual_Click(object sender, EventArgs e)
        {
            int delaytime = 1;
            bool bOK = int.TryParse(cboDispensingTimeList.Text, out delaytime);
            if (bOK)
            {
                string msg = "手動出膠 時間 " + delaytime.ToString() + " 秒";
                if (VsMSG.Instance.Question(msg) == DialogResult.OK)
                {
                    DispensingMs(delaytime);
                }
            }
        }

        private void DispensingMs(int itime)
        {
            if (!MACHINE.PLCIO.GetOutputIndex(16))
            {
                Task task = new Task(() =>
                {
                    MACHINE.PLCIO.SetOutputIndex(16, true);
                    System.Threading.Thread.Sleep(itime * 1000);
                    MACHINE.PLCIO.SetOutputIndex(16, false);
                });
                task.Start();
            }
        }

        private void BtnManualAuto_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1090, MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? 0 : 1);
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

        private void MMotorTimer_Tick(object sender, EventArgs e)
        {
            if (!Universal.IsNoUseIO)
            {
                if (IsEMCTriggered)
                {
                    //SetAbnormalLight();

                    IsEMCTriggered = false;
                    //StopAllProcess();
                    //OnTrigger(ActionEnum.ACT_ISEMC, "");
                }
            }

            btnManualAuto.BackColor = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? Color.Red : Color.Lime);
            btnManualAuto.Text = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? "自動模式" : "手動模式");

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i].Tick();
                i++;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
