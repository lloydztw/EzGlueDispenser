using Eazy_Project_III.ProcessSpace;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Eazy_Project_III.FormSpace
{
    public partial class FormCompensationStepTracer : Form
    {
        private CompensatingEventArgs m_blackboxArgs;
        public FormCompensationStepTracer()
        {
            InitializeComponent();
        }
        public FormCompensationStepTracer(CompensatingEventArgs e)
        {
            InitializeComponent();
            m_blackboxArgs = e;
            vsCompensationRangeUI1.TitleName = e.PhaseName;
            vsCompensationRangeUI1.lblMaxDelta.Text = e.MaxDelta.ToString();
            vsCompensationRangeUI1.lblMax.Text = (e.InitPos + e.MaxDelta).ToString();
            vsCompensationRangeUI1.lblCur.Text = e.CurrentPos.ToString();
            vsCompensationRangeUI1.lblMin.Text = (e.InitPos - e.MaxDelta).ToString();
            vsCompensationRangeUI1.lblNextDelta.Text = e.Delta.ToString();
            vsCompensationRangeUI1.lblNextPos.Text = (e.CurrentPos + e.Delta).ToString();
        }

        private void btnFreeRun_Click(object sender, EventArgs e)
        {
            m_blackboxArgs.ContinueToDebug = false;
            m_blackboxArgs.Cancel = false;
            DialogResult = DialogResult.Yes;
            Close();
        }
        private void btnStepRun_Click(object sender, EventArgs e)
        {
            m_blackboxArgs.ContinueToDebug = true;
            m_blackboxArgs.Cancel = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            m_blackboxArgs.Cancel = true;
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
