using Eazy_Project_III.ProcessSpace;
using JetEazy.QMath;
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
            m_blackboxArgs.GoControlByClient = new ManualResetEvent(false);
            Load += FormCompensationStepTracer_Load;
            FormClosed += FormCompensationStepTracer_FormClosed;
            lblTitleName.Text = e.PhaseName;
            
        }

        private void FormCompensationStepTracer_Load(object sender, EventArgs e)
        {
            update_data();
            //dataGridView1.Rows[0].Selected = false;
            //dataGridView1.Rows[2].Selected = true;
        }

        private void FormCompensationStepTracer_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_blackboxArgs.GoControlByClient.Set();
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

        private void update_data()
        {
            var e = m_blackboxArgs;
            var dv = dataGridView1;
            int Cols = dv.Columns.Count;
            dv.Rows.Add(6);
            //dv.Rows[0].Cells[0].Value = "可允許最大補償量";
            //dv.Rows[1].Cells[0].Value = "補償絕對上限";
            //dv.Rows[2].Cells[0].Value = "目前位置";
            //dv.Rows[3].Cells[0].Value = "補償絕對下限";
            //dv.Rows[4].Cells[0].Value = "預計下一步補償量";
            //dv.Rows[5].Cells[0].Value = "預計下一步絕對位置";
            var update_row = new Action<int, string, QVector>((r, t, v) =>
            {
                var row = dv.Rows[r];
                row.Cells[0].Value = t;
                for (int c = 1; c < Cols - 1; c++)
                    row.Cells[c].Value = v[c - 1].ToString("0.0000");

            });
            update_row(0, "可允許最大補償量", e.MaxDelta);
            update_row(1, "補償絕對上限", e.InitPos + e.MaxDelta);
            update_row(2, "目前位置", e.CurrentPos);
            update_row(3, "補償絕對下限", e.InitPos - e.MaxDelta);
            update_row(4, "預計下一步補償量", e.Delta);
            update_row(5, "預計下一步絕對位置", e.CurrentPos + e.Delta);

            dv.Rows[0].Selected = false;
            dv.Rows[2].Selected = true;           
        }
    }
}
