using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eazy_Project_III.ControlSpace.MachineSpace;
using JetEazy;

namespace Eazy_Project_III.UISpace.IOSpace
{
    public partial class X1OUTPUTUI : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        const int OUTPUT_COUNT = 32;
        Label[] lblOutput = new Label[OUTPUT_COUNT];
        DispensingX1MachineClass MACHINE;

        public X1OUTPUTUI()
        {
            InitializeComponent();
            InitUI();
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
"產品電磁鐵",
"吸產品真空開關A",
"料座真空開關",
"吸產品真空開關B",
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

        void InitUI()
        {
            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            int x = 0;
            int y = 0;
            while (i < OUTPUT_COUNT)
            {
                lblOutput[i] = new Label();
                lblOutput[i].BackColor = System.Drawing.Color.Green;
                lblOutput[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                lblOutput[i].ForeColor = System.Drawing.Color.Yellow;
                lblOutput[i].Size = new System.Drawing.Size(100, 20);
                lblOutput[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                //lblInput[i].Text = myText[i];

                lblOutput[i].Text = myText[i];
                //lblInput[i].Name = "lbl" + i.ToString();
                lblOutput[i].Tag = i;
                lblOutput[i].MouseEnter += IO_OUTPUTUI_MouseEnter;
                lblOutput[i].DoubleClick += X1OUTPUTUI_DoubleClick;

                //lblInput[i].Name = "label3";

                lblOutput[i].Name = "QX" + k.ToString() + "." + l.ToString();

                if (i % 7 == 0 && i > 0)
                {
                    k++;
                    l = 0;
                }
                else
                {
                    l++;
                }

                if (i % 10 == 0 && i > 0)
                {
                    x = 0;
                    y++;
                }

                lblOutput[i].Location = new System.Drawing.Point(x * 4 + x * lblOutput[i].Width, y * 4 + y * lblOutput[i].Height);
                this.Controls.Add(lblOutput[i]);

                i++;
                x++;
            }


        }

        private void X1OUTPUTUI_DoubleClick(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            int index = (int)lbl.Tag;


            switch(index)
            {
                case 16:

                    DispensingMs();

                    break;
                default:
                    MACHINE.PLCIO.SetOutputIndex((int)index, !MACHINE.PLCIO.GetOutputIndex((int)index));
                    break;
            }
        }

        private void DispensingMs()
        {
            if (!MACHINE.PLCIO.GetOutputIndex(16))
            {
                Task task = new Task(() =>
                {
                    MACHINE.PLCIO.SetOutputIndex(16, true);
                    System.Threading.Thread.Sleep(INI.Instance.DispensingMsTime);
                    MACHINE.PLCIO.SetOutputIndex(16, false);
                });
                task.Start();
            }
        }

        public void Initial(VersionEnum version, OptionEnum option, DispensingX1MachineClass machine)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;
        }
        public void Tick()
        {
            int i = 0;
            while (i < OUTPUT_COUNT)
            {
                lblOutput[i].BackColor = (MACHINE.PLCIO.GetOutputIndex(i) ? Color.Green : Color.Black);
                i++;
            }
        }

        private void IO_OUTPUTUI_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            //int ix = (int)lbl.Tag;
            ToolTip tip = new ToolTip();
            //tip.SetToolTip(lbl, myText[ix]);
            tip.SetToolTip(lbl, lbl.Name);
        }
    }
}
