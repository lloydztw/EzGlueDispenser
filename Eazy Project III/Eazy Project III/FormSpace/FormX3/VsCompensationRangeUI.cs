using System.Windows.Forms;

namespace Common
{
    public partial class VsCompensationRangeUI : UserControl
    {
        public VsCompensationRangeUI()
        {
            InitializeComponent();
        }
        public string TitleName
        {
            get
            {
                return lblTitleName.Text;
            }
            set
            {
                lblTitleName.Text = value;
            }
        }
        public void UpdateData(double maxDelta, double max, double cur, double min, double delta, string unit)
        {
            lblMaxDelta.Text = string.Format("{0:0.0000} {1}", maxDelta, unit);
            lblMax.Text = string.Format("{0:0.0000} {1}", max, unit);
            lblCur.Text = string.Format("{0:0.0000} {1}", cur, unit);
            lblMin.Text = string.Format("{0:0.0000} {1}", min, unit);
            lblNextDelta.Text = string.Format("{0:0.0000} {1}", delta, unit);
            lblNextPos.Text = string.Format("{0:0.0000} {1}", cur + delta, unit);
        }
    }
}
