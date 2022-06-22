using System;
using System.Windows.Forms;

namespace Eazy_Project_III.FormSpace
{
    public partial class FormCompensationStepTracer : Form
    {
        public FormCompensationStepTracer(string msg, string title = null)
        {
            InitializeComponent();
            richTextBox1.Text = msg;
            if (!string.IsNullOrEmpty(title))
                this.Text = title;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOpenMotorsTool_Click(object sender, EventArgs e)
        {
            using (var frm = new frmAXISSetup())
            {
                frm.ShowDialog();
            }
        }
    }
}
