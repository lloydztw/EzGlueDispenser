using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eazy_Project_III.FormSpace
{
    public partial class frmUserSelect : Form
    {
        Label lblSelectMessage;

        Button[] btnGet = new Button[4];
        Button[] btnPut = new Button[2];

        Button btnOK;
        Button btnCancel;

        int iGetIndex = 0;

        /// <summary>
        /// 2022-11-15 改為先取 mirror1 (右)
        /// </summary>
        int iPutIndex = 1;  

        /// <summary>
        /// 拾取第几组
        /// </summary>
        public int GetIndex
        {
            get { return iGetIndex; }
        }
        /// <summary>
        /// 放入左边还是右边 Mirror0 还是 Mirror1
        /// </summary>
        public int PutIndex
        {
            get { return iPutIndex; }
        }
        /// <summary>
        /// 勾选，单独制作一个Mirror 不勾选则做两个
        /// </summary>
        public bool IsAloneToMirror
        {
            get { return checkBox1.Checked; }
        }

        public frmUserSelect(bool eVisable = true)
        {
            InitializeComponent();
            this.Load += FrmUserSelect_Load;

            label1.Visible = eVisable;
            label2.Visible = eVisable;
            groupBox1.Visible = eVisable;
            checkBox1.Visible = eVisable;
        }

        private void FrmUserSelect_Load(object sender, EventArgs e)
        {
            this.Text = "用户选择拾取和放料的初始位置";
            lblSelectMessage = label2;
            lblSelectMessage.Text = "";

            btnGet[0] = button1;
            btnGet[1] = button2;
            btnGet[2] = button3;
            btnGet[3] = button4;

            btnPut[0] = button8;
            btnPut[1] = button7;

            btnOK = button5;
            btnCancel = button6;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            int i = 0;
            while (i < 4)
            {
                btnGet[i].Tag = i;
                btnGet[i].Click += FrmUserGetSelect_Click;
                btnGet[i].MouseEnter += FrmUserSelect_MouseEnter;
                btnGet[i].MouseLeave += FrmUserSelect_MouseLeave;
                i++;
            }
            i = 0;
            while (i < 2)
            {
                btnPut[i].Tag = i;
                btnPut[i].Click += FrmUserPutSelect_Click;
                i++;
            }

            FillDisplay();

            //LanguageExClass.Instance.EnumControls(this);
            _updateGuiStatus();
        }

        private void FrmUserPutSelect_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int icurrent = (int)btn.Tag;
            iPutIndex = icurrent;

            FillDisplay();
        }

        private void FrmUserSelect_MouseLeave(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int icurrent = (int)btn.Tag;
            //int i = 0;
            //while (i < 4)
            //{
            //    btnGet[i].BackColor = (iGetIndex == i ? Color.Green : Color.FromArgb(192, 255, 192));
            //    i++;
            //}
        }

        private void FrmUserSelect_MouseEnter(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int icurrent = (int)btn.Tag;
            //int i = 0;
            //while (i < 4)
            //{
            //    if(iGetIndex == i)
            //        btnGet[i].BackColor = (iGetIndex == i ? Color.Green : Color.FromArgb(192, 255, 192));
            //    else if(icurrent == i)
            //        btnGet[i].BackColor = (icurrent == i ? Color.Red : Color.FromArgb(192, 255, 192));

            //    i++;
            //}
        }

        private void FrmUserGetSelect_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int icurrent = (int)btn.Tag;
            iGetIndex = icurrent;

            FillDisplay();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _updateGuiStatus();
        }


        void FillDisplay()
        {
            int i = 0;
            while (i < 4)
            {
                btnGet[i].BackColor = (iGetIndex == i ? Color.Green : Color.FromArgb(192, 255, 192));
                i++;
            }
            i = 0;
            while (i < 2)
            {
                btnPut[i].BackColor = (iPutIndex == i ? Color.Green : Color.FromArgb(192, 255, 192));
                i++;
            }

            lblSelectMessage.Text = "用户选择组 " + iGetIndex.ToString() + " Mirror " + iPutIndex.ToString() + (iPutIndex == 0 ? "(左边)" : "(右边)");
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }


        private void _updateGuiStatus()
        {
            // 2022-11-15 改為先取 mirror1 (右)
            if (!this.checkBox1.Checked)
            {
                //button8.PerformClick();
                //button7.Enabled = false;
                button7.PerformClick();
                button8.Enabled = false;
            }
            else
            {
                //button7.Enabled = true;
                button8.Enabled = true;
            }
        }

    }
}
