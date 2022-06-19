using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using JetEazy.ControlSpace;

namespace Allinone.UISpace
{
    public partial class AdjUI : UserControl
    {
        GroupBox grpIniData;
        CheckBox[] chks;

        bool IsNeedToChange = true;

        public AdjUI()
        {
            InitializeComponent();
            InitialInternal();
        }
        void InitialInternal()
        {
            grpIniData = groupBox1;
        }
        public void Initial(List<CCDRectRelateIndexClass> ccdrelateindexlist,bool ishavebackground)
        {
            InitialCheckBox(ccdrelateindexlist, ishavebackground);
        }
        void InitialCheckBox(List<CCDRectRelateIndexClass> ccdrelateindexlist,bool ishavebackground)
        {
            int RowCount = 4;
            Size GapSize = new Size(42, 30);
            Point OrgLeftop = new Point(4, 23);
            
            //chks = new CheckBox[ccdrelateindexlist.Count - 1];
            chks = new CheckBox[ccdrelateindexlist.Count];

            int i = 0;
            while (i < chks.Length)
            {
                chks[i] = new CheckBox();
                grpIniData.Controls.Add(chks[i]);

                //chks[i].Tag = ccdrelateindexlist[i + 1].Index;
                chks[i].Tag = ccdrelateindexlist[i].Index;
                chks[i].AutoSize = true;
                chks[i].Location = new Point(OrgLeftop.X + (i % RowCount) * GapSize.Width, OrgLeftop.Y + GapSize.Height * (i / RowCount));
                chks[i].CheckedChanged += AdjUI_CheckedChanged;
                //chks[i].Text = "#" + (i + 2).ToString("00");
                chks[i].Text = "#" + (i + 1).ToString("00");

                i++;
            }

            //若有後面的圖像，則第一像機的圖可移動
            if (!ishavebackground)
                chks[0].Enabled = false;
        }

        private void AdjUI_CheckedChanged(object sender, EventArgs e)
        {
            if(IsNeedToChange)
                OnTriggerMoveScreen(GetMoveIndexString());
        }

        public string GetMoveIndexString()
        {
            string retstr = "";

            foreach(CheckBox chk in chks)
            {
                if(chk.Checked)
                    retstr += ((int)chk.Tag).ToString("00") + ",";
            }

            if(retstr != "")
                retstr = retstr.Remove(retstr.Length - 1, 1);

            return retstr;
        }

        public void ResetChks()
        {
            IsNeedToChange = false;

            foreach(CheckBox chk in chks)
            {
                chk.Checked = false;
            }

            IsNeedToChange = true;
        }

        public delegate void TriggerMoveScreenHandler(string movestring);
        public event TriggerMoveScreenHandler TriggerMoveScreen;
        public void OnTriggerMoveScreen(string movestring)
        {
            if (TriggerMoveScreen != null)
            {
                TriggerMoveScreen(movestring);
            }
        }


    }
}
