using Eazy_Project_III.ControlSpace.MachineSpace;
using JetEazy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eazy_Project_III.UISpace.IOSpace
{
    public partial class X3INPUTUI : UserControl
    {
        VersionEnum VERSION = VersionEnum.PROJECT;
        OptionEnum OPTION = OptionEnum.DISPENSING;

        const int INPUT_COUNT = 32;
        Label[] lblInput = new Label[INPUT_COUNT];
        DispensingMachineClass MACHINE;

        public X3INPUTUI()
        {
            InitializeComponent();
            InitUI();
        }

        string[] myText = new string[32]
        {
            "急停",
"UV灯上",
"UV灯下",
"散热片上",
"散热片下",
"总真空表",
"产品真空表",
"料座真空表",
"总气压表",
"光幕",
"速度",
"速度",
"手动/自动",
"吸嘴/点胶",
"按钮1",
"按钮2",
"按钮3",
"按钮4",
"按钮5",
"按钮6",
"按钮7",
"按钮8",
"按钮9",
"按钮10",
"按钮11",
"按钮12",
"镭射测距",
"门禁",
"POGO-ON",
"POGO-OFF",
"预留",
"预留",



        };

        void InitUI()
        {



            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            int x = 2;
            int y = 2;
            while (i < INPUT_COUNT)
            {
                lblInput[i] = new Label();
                lblInput[i].BackColor = System.Drawing.Color.Green;
                lblInput[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                lblInput[i].ForeColor = System.Drawing.Color.Yellow;
                lblInput[i].Size = new System.Drawing.Size(70, 20);
                lblInput[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                //lblInput[i].Text = myText[i];

                lblInput[i].Text = myText[i];
                //lblInput[i].Name = "lbl" + i.ToString();
                lblInput[i].Tag = i;
                lblInput[i].MouseEnter += IO_INPUTUI_MouseEnter;


                //lblInput[i].Name = "label3";

                lblInput[i].Name = "IX" + k.ToString() + "." + l.ToString();

                if (i % 8 == 0 && i > 0)
                {
                    k++;
                    l = 0;
                }
                else
                {
                    l++;
                }

                lblInput[i].Location = new System.Drawing.Point(x, y);

                if (i % 15 == 0 && (i > 0 && i != 30))
                {
                    x = 2;
                    y = 2 + lblInput[i].Height + 4;
                }
                else
                {
                    x += 4 + lblInput[i].Width;
                    //y = 2;
                }

                this.Controls.Add(lblInput[i]);

                i++;
            }


        }

        public void Initial(VersionEnum version, OptionEnum option, DispensingMachineClass machine)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;
        }
        public void Tick()
        {
            int i = 0;
            while (i < INPUT_COUNT)
            {
                lblInput[i].BackColor = (MACHINE.PLCIO.GetInputIndex(i) ? Color.Green : Color.Black);
                i++;
            }
        }

        private void IO_INPUTUI_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            //int ix = (int)lbl.Tag;
            ToolTip tip = new ToolTip();
            //tip.SetToolTip(lbl, myText[ix]);
            tip.SetToolTip(lbl, lbl.Name);
        }
    }
}
