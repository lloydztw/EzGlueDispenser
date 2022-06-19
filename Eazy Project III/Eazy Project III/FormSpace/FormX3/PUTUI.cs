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
    public partial class PUTUI : UserControl
    {
        public PUTUI()
        {
            InitializeComponent();
            mirrorUI1.lblGrp.Visible = false;
        }

        public int MirrorIndex
        {
            set
            {
                mirrorUI1.lblMirror0.BackColor = (value == 0 ? Color.Green : Control.DefaultBackColor);
                mirrorUI1.lblMirror1.BackColor = (value == 1 ? Color.Green : Control.DefaultBackColor);
                mirrorUI1.lblMirror0.Enabled = value == 0;
                mirrorUI1.lblMirror1.Enabled = value == 1;
            }
        }
    }
}
