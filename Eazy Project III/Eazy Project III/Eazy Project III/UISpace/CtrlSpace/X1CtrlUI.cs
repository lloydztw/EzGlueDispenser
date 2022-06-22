using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
//using JetEazy;
using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.ControlSpace.IOSpace;
using System.Threading.Tasks;
using JetEazy;
using Eazy_Project_III.FormSpace.FormX1;

namespace Eazy_Project_III.UISpace.CtrlSpace
{
    public partial class X1CtrlUI : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;
        JzTransparentPanel tpnlCover;
        JzTimes myTime;

        const int OUTPUT_COUNT = 32;
        Label[] lbl_IOS = new Label[OUTPUT_COUNT];

        //Label lblLIGHT;
        Label lblAXIS;

        DispensingX1MachineClass MACHINE;
        //{
        //    get { return (DispensingMachineClass)Universal.MACHINECollection.MACHINE; }
        //}

        public X1CtrlUI()
        {
            InitializeComponent();
            //InitlabelUI();
        }

        string[] myText = new string[OUTPUT_COUNT]
       {
           "UV燈上",
"UV燈下",
"載具伸出",
"載具縮回",
"取料手臂左_縮回",
"取料手臂左_伸出",
"取料手臂右_縮回",
"取料手臂右_伸出",
"PIN_縮回",
"PIN_伸出",
"PIN_伸出",
"預留",
"吸產品真空開關",
"料座真空開關",
"真空",
"UV燈開關",
"點膠閥開關",
"日光燈",
"刹車",
"刹車",
"紅燈",
"黃燈",
"綠燈",
"蜂鳴器",
"電磁鐵",
"真空泵",
"環形燈",
"高頻閥",
"背光板",
"彈簧針縮回",
"彈簧針伸出",
"預留",

       };


        void InitlabelUI()
        {


            int i = 0;
            int k = 0;
            int l = 0;
            int x = 0;
            int y = 0;
            while (i < OUTPUT_COUNT)
            {
                lbl_IOS[i] = new Label();
                lbl_IOS[i].BackColor = System.Drawing.Color.Green;
                lbl_IOS[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                lbl_IOS[i].ForeColor = System.Drawing.Color.Yellow;
                lbl_IOS[i].Size = new System.Drawing.Size(45, 20);
                lbl_IOS[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                //lblInput[i].Text = myText[i];

                lbl_IOS[i].Text = myText[i];
                //lblInput[i].Name = "lbl" + i.ToString();
                lbl_IOS[i].Tag = i;
                //lbl_IOS[i].MouseEnter += IO_INPUTUI_MouseEnter;

                lbl_IOS[i].MouseEnter += lblOUTPUTUI_MouseEnter;
                lbl_IOS[i].DoubleClick += lblOUTPUTUI_DoubleClick;
                //lblInput[i].Name = "label3";

                lbl_IOS[i].Name = "QX" + k.ToString() + "." + l.ToString();

                if (i % 8 == 0 && i > 0)
                {
                    k++;
                    l = 0;
                }
                else
                {
                    l++;
                }

                if (i % 4 == 0 && i > 0)
                {
                    x = 0;
                    y++;
                }
                lbl_IOS[i].Location = new Point(6 + x * 3 + x * lbl_IOS[i].Width, 17 + y * 3 + y * lbl_IOS[i].Height);


                groupBox2.Controls.Add(lbl_IOS[i]);

                i++;
                x++;
            }
        }

        private void lblOUTPUTUI_DoubleClick(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            //DispensingAddressEnum index = (DispensingAddressEnum)lbl.Tag;
            //switch (index)
            //{
            //    case DispensingAddressEnum.ADR_UVTOP:
            //        //MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_UVBOTTOM, false);
            //        MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_UVTOP, true);
            //        break;
            //    case DispensingAddressEnum.ADR_UVBOTTOM:
            //        //MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_UVTOP, false);
            //        MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_UVBOTTOM, true);
            //        break;
            //    case DispensingAddressEnum.ADR_FINTOP:
            //        //MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_FINBOTTOM, false);
            //        MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_FINTOP, true);
            //        break;
            //    case DispensingAddressEnum.ADR_FINBOTTOM:
            //        //MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_FINTOP, false);
            //        MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_FINBOTTOM, true);
            //        break;
            //    case DispensingAddressEnum.ADR_SWITCH_DISPENSING:

                   


            //        break;
            //    case DispensingAddressEnum.ADR_AXIS_BREAK:
            //        break;
            //    case DispensingAddressEnum.ADR_POGO_PIN:
            //        MACHINE.PLCIO.ADR_POGO_PIN = !MACHINE.PLCIO.ADR_POGO_PIN;
            //        break;
            //    default:

            //        MACHINE.PLCIO.SetOutputIndex((int)index, !MACHINE.PLCIO.GetOutputIndex((int)index));

            //        break;
            //}


        }

        private void lblOUTPUTUI_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            //int ix = (int)lbl.Tag;
            ToolTip tip = new ToolTip();
            //tip.SetToolTip(lbl, myText[ix]);
            tip.SetToolTip(lbl, lbl.Name);
        }

        public void Initial(VersionEnum version, OptionEnum option, DispensingX1MachineClass machine)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;

            //lblLIGHT = label1;
            lblAXIS = label2;

            tpnlCover = new JzTransparentPanel();
            tpnlCover.BackColor = System.Drawing.Color.Transparent;
            tpnlCover.Location = new System.Drawing.Point(6, 30);
            tpnlCover.Name = "panel1";
            tpnlCover.Size = this.Size;
            tpnlCover.TabIndex = 0;
            this.Controls.Add(tpnlCover);
            tpnlCover.BringToFront();

            //lblLIGHT.DoubleClick += LblLIGHT_DoubleClick;
            lblAXIS.DoubleClick += LblAXIS_DoubleClick;

            myTime = new JzTimes();
            myTime.Cut();

            SetEnable(false);
        }

        //frmAXISSetup mMotorFrom = null;

        frmMotor mMotorFrom = null;

        private void LblAXIS_DoubleClick(object sender, EventArgs e)
        {
            if (!Universal.IsOpenMotorWindows)
            {
                //OnTrigger(ActionEnum.ACT_MOTOR_SETUP, "");

                //MACHINE.SetNormalTemp(true);

                Universal.IsOpenMotorWindows = true;
                //MACHINE.PLCReadCmdNormalTemp(true);
                //System.Threading.Thread.Sleep(500);
                mMotorFrom = new frmMotor();
                mMotorFrom.Show();
            }
        }

        private void LblLIGHT_DoubleClick(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.ADR_LIGHT = !MACHINE.PLCIO.ADR_LIGHT;
            //MACHINE.PLCIO.UVTOP = !MACHINE.PLCIO.UVTOP;


        }

        public void SetEnable(bool isendable)
        {
            tpnlCover.Visible = !isendable;

            Color fillcolor = SystemColors.Control;

            if (!isendable)
                fillcolor = Color.Silver;
        }

        public void SetEnable()
        {
            bool isenable = !tpnlCover.Visible;
            SetEnable(isenable);
            this.Invalidate();
        }

        public void Tick()
        {
            if (myTime.msDuriation > 100)
            {
                //lblLIGHT.BackColor = (MACHINE.PLCIO.UVTOP ? Color.Green : Color.Black);

                //int i = 0;
                //while (i < OUTPUT_COUNT)
                //{

                //    switch (i)
                //    {
                //        //case 18:
                //        //    lbl_IOS[i].BackColor = (MACHINE.PLCIO.ADR_POGO_PIN ? Color.Green : Color.Black);
                //        //    break;
                //        default:
                //            lbl_IOS[i].BackColor = (MACHINE.PLCIO.GetOutputIndex(i) ? Color.Green : Color.Black);
                //            break;
                //    }

                //    i++;
                //}

                myTime.Cut();
            }

        }
    }
}
