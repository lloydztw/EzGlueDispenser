using Eazy_Project_III.ProcessSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Eazy_Project_III.FormSpace
{
    public partial class FormQCLaserMeasurement : Form
    {
        public FormQCLaserMeasurement(int mirrorIdx = 0)
        {
            InitializeComponent();

            if (mirrorIdx == 0)
                rdoMirror0.Checked = true;
            else
                rdoMirror1.Checked = false;

            update_laser_value();
        }


        private void btnSnap_Click(object sender, EventArgs e)
        {
            update_laser_value();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            double L = Math.Round((double)numericUpDown1.Value, 4);
            int mirrorIdx = rdoMirror0.Checked ? 0 : 1;

            string err = GdxCore.SetQcLaserMeasurement(mirrorIdx, L);

            if (err != null)
            {
                MessageBox.Show(err, "QC Laser 無法設定");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            int mirrorIdx = rdoMirror0.Checked ? 0 : 1;
            GdxCore.SetQcLaserMeasurement(mirrorIdx, -9999);
        }

        private bool update_laser_value()
        {
            var laser = GdxGlobal.GetLaser();
            double laserZ = laser.Snap();
            numericUpDown1.Value = (decimal)Math.Round(laserZ, 4);
            return true;
        }

    }
}
