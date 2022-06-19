using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eazy_Project_III.FormSpace
{
    public partial class PICKUI : UserControl
    {
        const int COUNT = 4;

        public PICKUI()
        {
            InitializeComponent();

            //int i = 0;
            //while (i < COUNT)
            //{
            //    i++;
            //}

            //int i = 0;
            //foreach (MirrorUI mirror in this.Controls)
            //{
            //    //if (mirror.Name.IndexOf("mirrorUI") > -1)
            //    {
            //        mirror.lblGrp.Text = "组" + i.ToString();
            //        i++;
            //    }
            //}

            mirrorUI1.lblGrp.Text = "组0";
            mirrorUI2.lblGrp.Text = "组1";
            mirrorUI3.lblGrp.Text = "组2";
            mirrorUI4.lblGrp.Text = "组3";
        }

        public int MirrorIndex
        {
            set
            {
                foreach (MirrorUI mirror in this.Controls)
                {
                    //if (mirror.Name.IndexOf("mirrorUI") > -1)
                    {
                        mirror.lblMirror0.BackColor = (value == 0 ? Color.Green : Control.DefaultBackColor);
                        mirror.lblMirror1.BackColor = (value == 1 ? Color.Green : Control.DefaultBackColor);
                        mirror.lblMirror0.Enabled = value == 0;
                        mirror.lblMirror1.Enabled = value == 1;
                    }
                }

                //mirrorUI1.lblMirror0.BackColor = (value == 0 ? Color.Green : Control.DefaultBackColor);
                //mirrorUI2.lblMirror0.BackColor = (value == 0 ? Color.Green : Control.DefaultBackColor);
                //mirrorUI3.lblMirror0.BackColor = (value == 0 ? Color.Green : Control.DefaultBackColor);
                //mirrorUI4.lblMirror0.BackColor = (value == 0 ? Color.Green : Control.DefaultBackColor);

                //mirrorUI1.lblMirror1.BackColor = (value == 1 ? Color.Green : Control.DefaultBackColor);
                //mirrorUI2.lblMirror1.BackColor = (value == 1 ? Color.Green : Control.DefaultBackColor);
                //mirrorUI3.lblMirror1.BackColor = (value == 1 ? Color.Green : Control.DefaultBackColor);
                //mirrorUI4.lblMirror1.BackColor = (value == 1 ? Color.Green : Control.DefaultBackColor);

                //mirrorUI1.lblMirror0.Enabled = value == 0;
                //mirrorUI2.lblMirror0.Enabled = value == 0;
                //mirrorUI3.lblMirror0.Enabled = value == 0;
                //mirrorUI4.lblMirror0.Enabled = value == 0;

                //mirrorUI1.lblMirror1.Enabled = value == 1;
                //mirrorUI2.lblMirror1.Enabled = value == 1;
                //mirrorUI3.lblMirror1.Enabled = value == 1;
                //mirrorUI4.lblMirror1.Enabled = value == 1;
            }
        }

        public void SetMirrorGrpIndex(int eGrpIndex)
        {
            foreach (MirrorUI mirror in this.Controls)
            {
                //if (mirror.Name.IndexOf("mirrorUI") > -1)
                {
                    if (mirror.lblMirror0.Enabled)
                    {
                        mirror.lblMirror0.BackColor = (mirror.lblGrp.Text == "组" + eGrpIndex.ToString() ? Color.Lime : Color.Green);
                    }
                    else if (mirror.lblMirror1.Enabled)
                    {
                        mirror.lblMirror1.BackColor = (mirror.lblGrp.Text == "组" + eGrpIndex.ToString() ? Color.Lime : Color.Green);
                    }
                }
            }
        }

    }
}
