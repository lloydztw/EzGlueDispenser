using Eazy_Project_III.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Drawing;
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
            dv.Rows.Add(7);
            var update_row = new Action<int, string, QVector>((rid, txt, vec) =>
            {
                var row = dv.Rows[rid];
                row.Cells[0].Value = txt;
                for (int c = 1; c < Cols - 1; c++)
                    row.Cells[c].Value = (vec != null) ?
                        vec[c - 1].ToString("0.0000") :
                        "動態決定";
            });
            var update_color = new Action<int, QVector>((rid, vec) =>
            {
                var row = dv.Rows[rid];
                if (vec != null)
                {
                    for (int c = 1; c < Cols - 1; c++)
                    {
                        var v = vec[c - 1];
                        row.Cells[c].Style.BackColor = 
                            (v == 0) ? Color.White :
                            (v > 0) ? Color.Lime : Color.Pink;                            
                    }
                }
            });
            update_row(0, " 可允許最大補償量", e.MaxDelta);
            update_row(1, " 補償絕對上限", e.InitPos + e.MaxDelta);
            update_row(2, " 目前位置", e.CurrentPos);
            update_row(3, " 補償絕對下限", e.InitPos - e.MaxDelta);
            update_row(4, " 預計下一步補償量", e.Delta);
            update_row(5, " 預計下一步絕對位置", e.CurrentPos + e.Delta);
            update_row(6, " 最終目標絕對位置", e.FinalTarget);
            update_color(4, e.Delta);

            dv.Rows[0].Selected = false;
            dv.Rows[2].Selected = true;
            dv.Rows[6].Selected = true;
        }
    }
}
