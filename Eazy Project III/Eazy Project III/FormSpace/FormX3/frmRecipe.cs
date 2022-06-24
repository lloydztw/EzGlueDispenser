using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.BasicSpace;
using JzDisplay;
using JzDisplay.UISpace;
using MoveGraphLibrary;
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
    public partial class frmRecipe : Form
    {

        Mover myMoverForCam00 = new Mover();

        /// <summary>
        /// 检查校正的相机
        /// </summary>
        ICam ICam00
        {
            get { return Universal.CAMERAS[0]; }
        }
        ICam ICam01
        {
            get { return Universal.CAMERAS[1]; }
        }

        Button btnGetImage0;

        DispUI m_DispUICAM0;
        DispUI m_DispUICAM1;

        Button btnOK;
        Button btnCancel;

        PropertyGrid m_propertyGrid;

        bool IsNeedToChange = false;


        public frmRecipe()
        {
            InitializeComponent();
            this.Load += FrmRecipe_Load;
        }

        private void FrmRecipe_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            init_Display1();
            update_Display1();

            m_propertyGrid = propertyGrid1;
            btnOK = button1;
            btnCancel = button2;
            btnGetImage0 = button3;

            Bitmap bmp = new Bitmap(RecipeCHClass.Instance.bmpCaliOrg);
            m_DispUICAM0.SetDisplayImage(bmp);
            bmp.Dispose();

            myMoverForCam00.Clear();

            int i = 0;
            Mover mover = RecipeCHClass.Instance.myMover;
            while (i < mover.Count)
            {
                GraphicalObject grobj = mover[i].Source;

                myMoverForCam00.Add(grobj);

                i++;
            }

            m_DispUICAM0.RefreshDisplayShape();
            m_DispUICAM0.SetMover(myMoverForCam00);
            m_DispUICAM0.MappingSelect();

            update_Display();

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnGetImage0.Click += BtnGetImage0_Click;

            btnGetImage0.Visible = false;

            m_propertyGrid.PropertyValueChanged += M_propertyGrid_PropertyValueChanged;

            FillDisplay();

            //LanguageExClass.Instance.EnumControls(this);
        }

        private void BtnGetImage0_Click(object sender, EventArgs e)
        {
            ICam00.SetExposure(RecipeCHClass.Instance.CaliCamExpo);
            ICam00.Snap();
            Bitmap bmp = new Bitmap(ICam00.GetSnap());
            m_DispUICAM0.SetDisplayImage(bmp);
            bmp.Dispose();

        }

        private void M_propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (!IsNeedToChange)
                return;


            //string changeitemstr = e.ChangedItem.Parent.Label + ";" + e.ChangedItem.PropertyDescriptor.Name;
            switch(e.ChangedItem.Label)
            {
                case "CaliCamExpo":
                    RecipeCHClass.Instance.CaliCamExpo = int.Parse(e.ChangedItem.Value.ToString());
                    break;
                case "CaliCamCaliData":
                    RecipeCHClass.Instance.CaliCamCaliData = e.ChangedItem.Value.ToString();
                    break;
                case "CaliPicCenter":
                    //RecipeCHClass.Instance.CaliPicCenter = e.ChangedItem.Value.ToString();
                    break;
                case "JudgeCamExpo":
                    RecipeCHClass.Instance.JudgeCamExpo = int.Parse(e.ChangedItem.Value.ToString());
                    break;
                case "JudgeCamCaliData":
                    RecipeCHClass.Instance.JudgeCamCaliData = int.Parse(e.ChangedItem.Value.ToString());
                    break;
                case "JudgeThetaOffset":
                    RecipeCHClass.Instance.JudgeThetaOffset = e.ChangedItem.Value.ToString();
                    break;
                case "DispensingTime":
                    RecipeCHClass.Instance.DispensingTime = int.Parse(e.ChangedItem.Value.ToString());
                    break;
                case "UVTime":
                    RecipeCHClass.Instance.UVTime = int.Parse(e.ChangedItem.Value.ToString());
                    break;
                case "OtherRecipe":
                    RecipeCHClass.Instance.OtherRecipe = e.ChangedItem.Value.ToString();
                    break;
            }


            FillDisplay();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            RecipeCHClass.Instance.Load();
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        void FillDisplay()
        {
            IsNeedToChange = false;

            m_propertyGrid.SelectedObject = RecipeCHClass.Instance;

            IsNeedToChange = true;
        }

        void init_Display()
        {
            m_DispUICAM0 = dispUI1;
            m_DispUICAM0.Initial(100, 0.01f);
            m_DispUICAM0.SetDisplayType(DisplayTypeEnum.NORMAL);

            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;
        }
        void update_Display()
        {
            m_DispUICAM0.Refresh();
            m_DispUICAM0.DefaultView();
        }
        void init_Display1()
        {
            m_DispUICAM1 = dispUI2;
            m_DispUICAM1.Initial(100, 0.01f);
            m_DispUICAM1.SetDisplayType(DisplayTypeEnum.NORMAL);

            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;
        }
        void update_Display1()
        {
            m_DispUICAM1.Refresh();
            m_DispUICAM1.DefaultView();
        }

        private void btnCam0Snap_Click(object sender, EventArgs e)
        {
            CameraRunning[0] = false;
            btnCam0Live.BackColor = (CameraRunning[0] ? Color.Red : Control.DefaultBackColor);

            ICam00.SetExposure(RecipeCHClass.Instance.CaliCamExpo);
            ICam00.Snap();
            Bitmap bmp = new Bitmap(ICam00.GetSnap());
            RecipeCHClass.Instance.bmpCaliOrg = new Bitmap(bmp);
            m_DispUICAM0.SetDisplayImage(bmp);
            bmp.Dispose();
        }

        private void btnCam0Live_Click(object sender, EventArgs e)
        {
            if (!CameraRunning[0])
            {
                ICam00.StartCapture();
                CameraRunning[0] = true;
                btnCam0Live.BackColor = (CameraRunning[0] ? Color.Red : Control.DefaultBackColor);
                CameraLive(0);
            }
            else
            {
                ICam00.StopCapture();
                CameraRunning[0] = false;
            }
            btnCam0Live.BackColor = (CameraRunning[0] ? Color.Red : Control.DefaultBackColor);
        }

        private void btnCam1Sanp_Click(object sender, EventArgs e)
        {
            CameraRunning[1] = false;
            btnCam0Live.BackColor = (CameraRunning[1] ? Color.Red : Control.DefaultBackColor);

            ICam01.SetExposure(RecipeCHClass.Instance.JudgeCamExpo);
            ICam01.Snap();
            Bitmap bmp = new Bitmap(ICam01.GetSnap());
            m_DispUICAM1.SetDisplayImage(bmp);
            bmp.Dispose();
        }

        private void btnCam1Live_Click(object sender, EventArgs e)
        {
            if (!CameraRunning[1])
            {
                ICam01.StartCapture();
                CameraRunning[1] = true;
                btnCam1Live.BackColor = (CameraRunning[1] ? Color.Red : Control.DefaultBackColor);
                CameraLive(1);
            }
            else
            {
                ICam01.StopCapture();
                CameraRunning[1] = false;
            }
            btnCam1Live.BackColor = (CameraRunning[1] ? Color.Red : Control.DefaultBackColor);
        }


        #region TASK 线程

        bool[] CameraRunning = new bool[2];

        private void CameraLive(int eCamIndex)
        {
            Task task = new Task(() =>
            {
                while (true)
                {
                    if (eCamIndex >= 2)
                        break;

                    System.Threading.Thread.Sleep(10);
                    if (!CameraRunning[eCamIndex])
                        break;

                    switch(eCamIndex)
                    {
                        case 0:
                            try
                            {
                                this.Invoke(new Action(() =>
                                {
                                    Bitmap bmp = new Bitmap(ICam00.GetSnap());
                                    m_DispUICAM0.SetDisplayImage(bmp);
                                    bmp.Dispose();
                                }));
                            }
                            catch
                            {

                            }
                            break;
                        case 1:
                            try
                            {
                                this.Invoke(new Action(() =>
                                {
                                    Bitmap bmp = new Bitmap(ICam01.GetSnap());
                                    m_DispUICAM1.SetDisplayImage(bmp);
                                    bmp.Dispose();
                                }));
                            }
                            catch
                            {

                            }
                            break;
                    }

                }
            });
            task.Start();
        }

        #endregion

    }
}
