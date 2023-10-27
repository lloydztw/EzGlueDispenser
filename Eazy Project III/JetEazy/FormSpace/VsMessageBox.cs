using JetEazy.BasicSpace;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JetEazy.FormSpace
{
    public partial class VsMessageBox : Form
    {
        Label lblMessageText;
        Button btnOK;
        Button btnCancel;

        public VsMessageBox(string strMessage, bool isWarning = false, bool isInfo = false)
        {
            // 2022 revised by LeTian Chang
            // 新增 Info 型式
            InitializeComponent();
            Init(strMessage, isWarning, isInfo);
        }

        void Init(string eStrMsg, bool isWarning = false, bool isInfo = false)
        {
            this.Text = (isWarning ? "警告视窗" : (!isInfo ? "询问视窗" : "提示视窗"));
            lblMessageText = label1;
            btnCancel = button1;
            btnOK = button2;

            panel1.BackColor = (isWarning ? Color.Red : Color.FromArgb(255, 255, 192));
            //lblMessageText.ForeColor = (isWarning ? Color.White : Color.Black);

            lblMessageText.Text = "提示信息:" + Environment.NewLine + Environment.NewLine;
            lblMessageText.Text += eStrMsg;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            //btnOK.Text = "确定(OK)";
            //btnCancel.Text = "取消(Cancel)";

            btnOK.Text = "确定";
            btnCancel.Text = "取消";

            this.TopMost = true;

            if (isWarning || isInfo)
            {
                btnCancel.Visible = false;
                btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
            }

            LanguageExClass.Instance.EnumControls(this, false);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
