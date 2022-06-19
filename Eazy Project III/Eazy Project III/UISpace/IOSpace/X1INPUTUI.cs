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
    public partial class X1INPUTUI : UserControl
    {

        VersionEnum VERSION;
        OptionEnum OPTION;

        const int INPUT_COUNT = 48;
        Label[] lblInput = new Label[INPUT_COUNT];
        DispensingX1MachineClass MACHINE;
        public X1INPUTUI()
        {
            InitializeComponent();
            InitUI();
        }

        string[] myText = new string[INPUT_COUNT]
        {
            "急停",
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
"總真空表",
"吸產品真空表A",
"料座真空表",
"吸產品真空表B",
"總氣壓錶",
"光幕",
"門禁",
"速度",
"速度",
"手動/自動",
"急停",
"按鈕1",
"按鈕2",
"按鈕3",
"按鈕4",
"按鈕5",
"按鈕6",
"按鈕7",
"按鈕8",
"按鈕9",
"按鈕10",
"UV燈上下",
"載具伸出縮回",
"取料手臂左_縮回",
"取料手臂左_伸出",
"取料手臂右_縮回",
"取料手臂右_伸出",
"PIN_伸出縮回",
"PIN_伸出縮回",
"料座真空",
"彈簧針伸出縮回",
"彈簧針縮回",
"彈簧針伸出",
"預留",
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
            while (i < INPUT_COUNT)
            {
                lblInput[i] = new Label();
                lblInput[i].BackColor = System.Drawing.Color.Green;
                lblInput[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                lblInput[i].ForeColor = System.Drawing.Color.Yellow;
                lblInput[i].Size = new System.Drawing.Size(100, 20);
                lblInput[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                //lblInput[i].Text = myText[i];

                lblInput[i].Text = myText[i];
                //lblInput[i].Name = "lbl" + i.ToString();
                lblInput[i].Tag = i;
                lblInput[i].MouseEnter += IO_INPUTUI_MouseEnter;


                //lblInput[i].Name = "label3";

                lblInput[i].Name = "IX" + k.ToString() + "." + l.ToString();

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

                lblInput[i].Location = new System.Drawing.Point(x * 4 + x * lblInput[i].Width, y * 4 + y * lblInput[i].Height);
                this.Controls.Add(lblInput[i]);

                i++;
                x++;
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
