using Eazy_Project_III.MVC_Control;
using Eazy_Project_III.OPSpace;
using Eazy_Project_III.ProcessSpace;
using JetEazy.BasicSpace;
using JetEazy.GdxCore3;
using JetEazy.ProcessSpace;
using Paso.Aoi.GUI;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace Eazy_Project_III.FormSpace
{
    public partial class FormRecipe : Form
    {
        #region PRIVATE_GUI_MEMBERS
        PropertyGrid propertyGrid;
        RadioButton[] rdoCams;
        RichTextBox rtbParentRichTextBox;
        /// <summary>
        /// Center-Calibration (camID==0) GB len
        /// </summary>
        CvFreeQuadrilateralBox m_cvBoxGB;
        /// <summary>
        /// Center-Calibration (camID==0) R len
        /// </summary>
        CvFreeQuadrilateralBox m_cvBoxR;
        /// <summary>
        /// Projection-Calibration (camID==1)
        /// </summary>
        CvFreeQuadrilateralBox m_cvBoxProj;
        bool m_bypassWindowEvent = false;
        int m_activeCamID = -1;
        #endregion


        #region JEZ_COMPONENTS
        RecipeCHClass _recipe
        {
            get { return RecipeCHClass.Instance; }
        }
        #endregion


        #region RCP_LIVE_IMAGING_PROCESS
        RcpLiveImageProcess m_rcpLiveProcess
        {
            get
            {
                return RcpLiveImageProcess.Singleton("Rcp");
            }
        }
        GdxLiveDispControl m_dispCtrl;
        #endregion


        public FormRecipe()
        {
            InitializeComponent();
            initEventHandlers();
            this.Load += FormRecipe_Load;
            this.FormClosed += FormRecipe_FormClosed;
            this.SizeChanged += FormRecipe_SizeChanged;
        }


        #region WINDOW_EVENT_HANDLERS
        private void FormRecipe_FormClosed(object sender, FormClosedEventArgs e)
        {
            cleanup();
        }
        private void FormRecipe_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            updateRecipeData();
            //>> LanguageExClass.Instance.EnumControls(this);
            applyCameraExposureAndGain(0, 0);
            applyCameraExposureAndGain(1, 0);
            startLiveImage(m_activeCamID = 0);
        }
        private void FormRecipe_SizeChanged(object sender, EventArgs e)
        {
            _auto_layout();
        }
        private void Rdo_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bypassWindowEvent)
                return;

            center_enableManualMarking("");
            proj_enableManualMarking(false);
            gdxDispUI1.UpdateCompensatedInfo(null);
            gdxDispUI1.UpdateLocatedMarks(null);

            int idx = Array.IndexOf(rdoCams, sender);
            if (idx >= 0 && rdoCams[idx].Checked)
            {
                startLiveImage(m_activeCamID = idx);
                // updateFuncButtonGuiStatus(false);
            }
        }
        private void PropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            if (m_bypassWindowEvent)
                return;

            var descriptor = e.NewSelection.PropertyDescriptor;
            if (descriptor == null)
                return;

            switch (descriptor.Name)
            {
                case "CaliCamExpo":
                case "CaliCamGain":
                    applyCameraExposureAndGain(0, 0);
                    rdoCams[0].Checked = true;
                    break;
                case "CaliCamExpoM1":
                case "CaliCamGainM1":
                    applyCameraExposureAndGain(0, 1);
                    rdoCams[0].Checked = true;
                    break;
                case "CaliCamExpo2":
                case "CaliCamGain2":
                    applyCameraExposureAndGain(0, -99);
                    rdoCams[0].Checked = true;
                    break;

                case "JudgeCamExpo":
                case "JudgeCamGain":
                    applyCameraExposureAndGain(1, 0);
                    rdoCams[1].Checked = true;
                    break;
                case "JudgeCamExpoM1":
                case "JudgeCamGainM1":
                    applyCameraExposureAndGain(1, 1);
                    rdoCams[1].Checked = true;
                    break;
                case "JudgeCamExpo2":
                case "JudgeCamGain2":
                    applyCameraExposureAndGain(1, -99);
                    rdoCams[1].Checked = true;
                    break;
            }
        }
        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (m_bypassWindowEvent)
                return;

            //string changeitemstr = e.ChangedItem.Parent.Label + ";" + e.ChangedItem.PropertyDescriptor.Name;
            if (e.ChangedItem == null)
                return;

            var descriptor = e.ChangedItem.PropertyDescriptor;
            if (descriptor == null)
                return;

            int camID;
            int mirrorIdx;
            int expo;
            float gain;

            switch (descriptor.Name)
            {
                case "CaliCamExpo":
                    if (_recipe.GetCameraExpoAndGain(camID = 0, mirrorIdx = 0, out expo, out gain))
                    {
                        expo = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "CaliCamGain":
                    if (_recipe.GetCameraExpoAndGain(camID = 0, mirrorIdx = 0, out expo, out gain))
                    {
                        gain = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "CaliCamExpoM1":
                    if (_recipe.GetCameraExpoAndGain(camID = 0, mirrorIdx = 1, out expo, out gain))
                    {
                        expo = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "CaliCamGainM1":
                    if (_recipe.GetCameraExpoAndGain(camID = 0, mirrorIdx = 1, out expo, out gain))
                    {
                        gain = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;

                case "CaliCamExpo2":
                    if (_recipe.GetCameraExpoAndGain(camID = 0, mirrorIdx = -99, out expo, out gain))
                    {
                        expo = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "CaliCamGain2":
                    if (_recipe.GetCameraExpoAndGain(camID = 0, mirrorIdx = -99, out expo, out gain))
                    {
                        gain = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;

#if(OPT_RESERVED)
                case "CaliCamCaliData":
                    RecipeCHClass.Instance.CaliCamCaliData = e.ChangedItem.Value.ToString();
                    break;
                case "CaliPicCenter":
                    //RecipeCHClass.Instance.CaliPicCenter = e.ChangedItem.Value.ToString();
                    break;
#endif
                case "JudgeCamExpo":
                    if (_recipe.GetCameraExpoAndGain(camID = 1, mirrorIdx = 0, out expo, out gain))
                    {
                        expo = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "JudgeCamGain":
                    if (_recipe.GetCameraExpoAndGain(camID = 1, mirrorIdx = 0, out expo, out gain))
                    {
                        gain = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;

                case "JudgeCamExpoM1":
                    if (_recipe.GetCameraExpoAndGain(camID = 1, mirrorIdx = 1, out expo, out gain))
                    {
                        expo = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "JudgeCamGainM1":
                    if (_recipe.GetCameraExpoAndGain(camID = 1, mirrorIdx = 1, out expo, out gain))
                    {
                        gain = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;

                case "JudgeCamExpo2":
                    if (_recipe.GetCameraExpoAndGain(camID = 1, mirrorIdx = -99, out expo, out gain))
                    {
                        expo = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
                    break;
                case "JudgeCamGain2":
                    if (_recipe.GetCameraExpoAndGain(camID = 1, mirrorIdx = -99, out expo, out gain))
                    {
                        gain = int.Parse(e.ChangedItem.Value.ToString());
                        _recipe.SetCameraExpoAndGain(camID, mirrorIdx, expo, gain);
                        applyCameraExposureAndGain(camID, mirrorIdx);
                    }
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
            updateRecipeData();
        }
        private void BtnFuncCenterManualGB_Click(object sender, EventArgs e)
        {
            bool enable = !m_cvBoxGB.Visible;
            center_enableManualMarking(enable ? "GB" : "");
        }
        private void BtnFuncCenterManualR_Click(object sender, EventArgs e)
        {
            bool enable = !m_cvBoxR.Visible;
            center_enableManualMarking(enable ? "R" : "");
        }
        private void BtnFuncCenterSaveK_Click(object sender, EventArgs e)
        {
            string gb_r;
            if (m_cvBoxGB.Visible)
                gb_r = "GB";
            else if (m_cvBoxR.Visible)
                gb_r = "R";
            else
                gb_r = "";
            center_saveManualMarkPoints(gb_r);
            center_enableManualMarking("");
            btnFuncCenterSaveK.Enabled = false;
        }
        private void BtnFuncProjAuto_Click(object sender, EventArgs e)
        {
            proj_startAutoMarksChecking();
        }
        private void BtnFuncProjManual_Click(object sender, EventArgs e)
        {
            bool enable = !m_cvBoxProj.Visible;
            proj_enableManualMarking(enable);
        }
        private void BtnFuncProjSaveK_Click(object sender, EventArgs e)
        {
            proj_saveManualMarkPoints();
            proj_enableManualMarking(false);
            btnFuncProjSaveK.Enabled = false;
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            center_loadManualMarkPoints("GB");
            center_loadManualMarkPoints("R");
            proj_loadManualMarkPoints();
            RecipeCHClass.Instance.Load();
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
        #endregion


        #region LIVE_PROCESS_EVENT_HANDLERS
        private void dispCtrl_OnLiveStatusChanged(object sender, int e)
        {
            if (!IsHandleCreated || !Visible)
                return;

            if (InvokeRequired)
            {
                EventHandler<int> func = dispCtrl_OnLiveStatusChanged;
                this.BeginInvoke(func, e);
            }
            else
            {
                updateCameraLiveGuiStatus();
            }
        }
        private void rcpLiveProcess_OnMarkPointInfo(object sender, CoreMarkPointEventArgs e)
        {
            if (!IsHandleCreated || !Visible)
                return;

            if (InvokeRequired)
            {
                EventHandler<CoreMarkPointEventArgs> func = rcpLiveProcess_OnMarkPointInfo;
                this.Invoke(func, sender, e);
            }
            else
            {
                gdxDispUI1.UpdateLocatedMarks(e);
            }
        }
        private void rcpLiveProcess_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (!IsHandleCreated || !Visible)
                return;

            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> func = rcpLiveProcess_OnCompleted;
                this.BeginInvoke(func, sender, e);
            }
            else
            {
                VsMSG.Instance.Question("\n【定位點檢】已完成: 結果 = " + e.Message);
            }
        }
        private void cvBox_OnSelected(object sender, EventArgs e)
        {
            if (sender == m_cvBoxProj)
                btnFuncProjSaveK.Enabled = true;
            else if (sender == m_cvBoxGB || sender == m_cvBoxR)
                btnFuncCenterSaveK.Enabled = true;
            else
                { }
        }
        #endregion


        #region PRIVATE_FUNCTIONS
        void cleanup()
        {
            stopLiveImage(-1);
            unhookRichTextBox();

            if (m_dispCtrl != null)
            {
                m_dispCtrl.Dispose();
                m_dispCtrl = null;
                m_rcpLiveProcess.OnMarkPointInfo -= rcpLiveProcess_OnMarkPointInfo;
                m_rcpLiveProcess.OnCompleted -= rcpLiveProcess_OnCompleted;
            }
        }
        void initEventHandlers()
        {
            rdoCams = new RadioButton[]
            {
                radioButton1,
                radioButton2
            };
            foreach (var rdo in rdoCams)
                rdo.CheckedChanged += Rdo_CheckedChanged;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            propertyGrid = propertyGrid1;
            propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
            propertyGrid.SelectedGridItemChanged += PropertyGrid_SelectedGridItemChanged;
            
            btnFuncCenterManualGB.Click += BtnFuncCenterManualGB_Click;
            btnFuncCenterManualR.Click += BtnFuncCenterManualR_Click;
            btnFuncCenterSaveK.Click += BtnFuncCenterSaveK_Click;

            btnFuncProjAuto.Click += BtnFuncProjAuto_Click;
            btnFuncProjManual.Click += BtnFuncProjManual_Click;
            btnFuncProjSaveK.Click += BtnFuncProjSaveK_Click;

            // cvBoxGB (手動4點)
            if (m_cvBoxGB == null)
            {
                m_cvBoxGB = new CvFreeQuadrilateralBox(Brushes.Lime, 3, 35, 4);
                m_cvBoxGB.OnSelected += cvBox_OnSelected;
                gdxDispUI1.ImageViewer.AddInteractor(m_cvBoxGB);
                m_cvBoxGB.Visible = false;
                center_loadManualMarkPoints("GB");
            }
            // cvBoxR (手動4點)
            if (m_cvBoxR == null)
            {
                m_cvBoxR = new CvFreeQuadrilateralBox(Brushes.DeepPink, 3, 35, 4);
                m_cvBoxR.OnSelected += cvBox_OnSelected;
                gdxDispUI1.ImageViewer.AddInteractor(m_cvBoxR);
                m_cvBoxR.Visible = false;
                center_loadManualMarkPoints("R");
            }
            // cvBoxProj (手動5點)
            if (m_cvBoxProj == null)
            {
                m_cvBoxProj = new CvFreeQuadrilateralBox(Brushes.Coral, 3, 35, 5);
                m_cvBoxProj.OnSelected += cvBox_OnSelected;
                gdxDispUI1.ImageViewer.AddInteractor(m_cvBoxProj);
                m_cvBoxProj.Visible = false;
                proj_loadManualMarkPoints();
            }

            // LiveImaging Event Handlers
            if (m_dispCtrl == null)
            {
                m_dispCtrl = new GdxLiveDispControl(gdxDispUI1, m_rcpLiveProcess);
                m_dispCtrl.OnLiveStatusChanged += dispCtrl_OnLiveStatusChanged;
                m_rcpLiveProcess.OnMarkPointInfo += rcpLiveProcess_OnMarkPointInfo;
                m_rcpLiveProcess.OnCompleted += rcpLiveProcess_OnCompleted;
            }

            hookRichTextBox();
        }

        void hookRichTextBox()
        {
            if (rtbParentRichTextBox == null)
            {
                rtbParentRichTextBox = CommonLogClass.Instance.SetRichTextBox(richTextBox1);
                if (rtbParentRichTextBox != null)
                {
                    richTextBox1.Font = rtbParentRichTextBox.Font;
                    //>>> richTextBox1.Text = rtbParentRichTextBox.Text;
                }
            }
        }
        void unhookRichTextBox()
        {
            if (rtbParentRichTextBox != null)
            {
                CommonLogClass.Instance.SetRichTextBox(rtbParentRichTextBox);
                rtbParentRichTextBox.AppendText(richTextBox1.Text);
                rtbParentRichTextBox = null;
            }
        }
        void updateRecipeData(bool toGui = true)
        {
            if (toGui)
            {
                m_bypassWindowEvent = true;
                propertyGrid.SelectedObject = RecipeCHClass.Instance;
                m_bypassWindowEvent = false;
            }
            else
            {
                // RESERVED
            }
        }

        void updateCameraLiveGuiStatus(bool reset = false)
        {
            m_bypassWindowEvent = true;
            if (reset || !isLive(m_activeCamID))
            {
                this.Cursor = Cursors.AppStarting;
                rdoCams[0].Checked = false;
                rdoCams[1].Checked = false;
                updateFuncButtonGuiStatus(forceDisable: true);
            }
            else
            {
                this.Cursor = Cursors.Default;
                rdoCams[m_activeCamID].Checked = isLive(m_activeCamID);
                updateFuncButtonGuiStatus();
            }
            m_bypassWindowEvent = false;
        }
        void updateFuncButtonGuiStatus(bool forceDisable = false)
        {
            if (forceDisable)
            {
                btnFuncProjAuto.Enabled = false;
                btnFuncProjManual.Enabled = false;
                btnFuncProjSaveK.Enabled = false;
                btnFuncCenterManualGB.Enabled = false;
                btnFuncCenterManualR.Enabled = false;
                btnFuncCenterSaveK.Enabled = false;
            }
            else
            {
                bool enabled;
                // center camera
                enabled = (m_activeCamID == 0);
                btnFuncCenterManualGB.Enabled = enabled;
                btnFuncCenterManualR.Enabled = enabled;
                //> btnFuncCenterSaveK.Enabled = false;
                // proj camera
                enabled = (m_activeCamID == 1);
                btnFuncProjAuto.Enabled = enabled;
                btnFuncProjManual.Enabled = enabled;
                //> btnFuncProjSaveK.Enabled = false;
            }
        }

        bool isLive(int camID)
        {
            return m_dispCtrl.IsLive(camID);
        }
        void startLiveImage(int camID)
        {
            m_dispCtrl.StartLiveImage(camID);
            updateCameraLiveGuiStatus(reset: true);
            //updateFuncButtonGuiStatus(forceDisable: true);
        }
        void stopLiveImage(int camID)
        {
            m_dispCtrl.StopLiveImage(camID);
            updateCameraLiveGuiStatus(reset: true);
        }
        void applyCameraExposureAndGain(int camID, int mirrorIdx)
        {
            try
            {
                //CommonLogClass.Instance.LogMessage($"設定 camID={camID}, 曝光時間= {expo} (ms), 增益= {gain:0.0} (db)");
                //var cam = Universal.CAMERAS[camID];
                //cam.SetExposure(expo);
                //cam.SetGain(gain);

                if (_recipe.GetCameraExpoAndGain(camID, mirrorIdx, out int expo, out float gain))
                {
                    CommonLogClass.Instance.LogMessage($"設定 camID={camID}, mirrorIdx={mirrorIdx}, 曝光時間= {expo} (ms), 增益= {gain:0.0} (db)");
                    var cam = Universal.CAMERAS[camID];
                    cam.SetExposure(expo);
                    cam.SetGain(gain);
                }
            }
            catch (Exception ex)
            {
                CommonLogClass.Instance.LogError($"無法設定 camID={camID}, mirrorIdx={mirrorIdx}, 曝光時間與增益!");
            }
        }

        CvFreeQuadrilateralBox center_getBox(string gb_r)
        {
            if (gb_r == "GB")
                return m_cvBoxGB;
            else if (gb_r == "R")
                return m_cvBoxR;
            else
                return null;
        }
        void center_enableManualMarking(string gb_r)
        {
            var box = center_getBox(gb_r);
            bool enabled = box != null;
            
            if (enabled)
            {
                box.ActiveCornerID = 0;
                m_cvBoxGB.Visible = (m_cvBoxGB == box);
                m_cvBoxR.Visible = (m_cvBoxR == box);
                clip(box);
            }
            else
            {
                m_cvBoxGB.ActiveCornerID = -1;
                m_cvBoxR.ActiveCornerID = -1;
                m_cvBoxGB.Visible = false;
                m_cvBoxR.Visible = false;
            }

            btnFuncCenterSaveK.Enabled = enabled;
            gdxDispUI1.Refresh();

            if (enabled)
            {
                CommonLogClass.Instance.LogMessage($"{gb_r}鏡片,手動設定位點模式:", Color.Blue);
                CommonLogClass.Instance.LogMessage("按 1,2,3,4 鍵, 可以直接點選該點新位置.", Color.DarkBlue);
                CommonLogClass.Instance.LogMessage("按 ESC 鍵, 可以拖拉每一點到新位置.", Color.DarkBlue);
            }
        }
        void center_saveManualMarkPoints(string gb_r)
        {
            var box = center_getBox(gb_r);
            if (box == null)
                return;
            var pts = Round(box.Corners);
            GdxCore.setGoldenCenterPt(gb_r, pts);
            JetEazy.Win32.Win32Ini.SavePoints(pts, getDefaultIniFile(), $"{gb_r}_MarkPts", "ManualPts");
            m_rcpLiveProcess.DumpImage($"{gb_r}鏡片手動點檢");
        }
        void center_loadManualMarkPoints(string gb_r)
        {
            var box = center_getBox(gb_r);
            if (box == null)
                return;

            var pts = Round(box.Corners);
            JetEazy.Win32.Win32Ini.LoadPoints(pts, getDefaultIniFile(), $"{gb_r}_MarkPts", "ManualPts");
            box.Corners = ToPointF(pts);
        }
        
        void proj_startAutoMarksChecking()
        {
            proj_enableManualMarking(false);
            applyCameraExposureAndGain(1, -99);
            m_rcpLiveProcess.Start("CheckMarks");
        }
        void proj_enableManualMarking(bool enabled)
        {
            //> m_cvBox.Visible = enabled;
            if (enabled)
                m_cvBoxProj.ActiveCornerID = 0;
            else
                m_cvBoxProj.ActiveCornerID = -1;

            if (enabled)
                clip(m_cvBoxProj);

            gdxDispUI1.Refresh();
            m_cvBoxProj.Visible = enabled;
            btnFuncProjSaveK.Enabled = enabled;

            if (enabled)
            {
                CommonLogClass.Instance.LogMessage("手動設定位點模式:", Color.Blue);
                CommonLogClass.Instance.LogMessage("按 1,2,3,4,5 鍵, 可以直接點選該點新位置.", Color.DarkBlue);
                CommonLogClass.Instance.LogMessage("按 ESC 鍵, 可以拖拉每一點到新位置.", Color.DarkBlue);
            }
        }
        void proj_saveManualMarkPoints()
        {
            var pts = Round(m_cvBoxProj.Corners);
            GdxCore.setMarkPoints(pts);
            JetEazy.Win32.Win32Ini.SavePoints(pts, getDefaultIniFile(), "MarkPts", "ManualPts");
            m_rcpLiveProcess.DumpImage("proj_手動定位點檢");
        }
        void proj_loadManualMarkPoints()
        {
            var pts = Round(m_cvBoxProj.Corners);
            JetEazy.Win32.Win32Ini.LoadPoints(pts, getDefaultIniFile(), "MarkPts", "ManualPts");
            m_cvBoxProj.Corners = ToPointF(pts);
        }

        bool clip(CvFreeQuadrilateralBox box)
        {
            Rectangle bound = Rectangle.Round(gdxDispUI1.ImageViewer.GetWorldRect());
            bool out_bound = false;
            var pts = box.Corners;

            for (int i = 0; i < pts.Length; i++)
            {
                var pt = pts[i];
                if (pt.X < bound.X)
                {
                    pts[i].X = bound.X;
                    out_bound = true;
                }
                if (pt.X >= bound.Right)
                {
                    pts[i].X = bound.Right - 1;
                    out_bound = true;
                }
                if (pt.Y < bound.Y)
                {
                    pts[i].Y = bound.Y;
                    out_bound = true;
                }
                if (pt.Y >= bound.Bottom)
                {
                    pts[i].Y = bound.Bottom - 1;
                    out_bound = true;
                }
            }
            if (out_bound)
            {
                box.Corners = pts;
            }
            return out_bound;
        }
        #endregion


        #region UTILITIES
        static Point[] Round(PointF[] pts)
        {
            int N = pts.Length;
            var ret = new Point[N];
            for(int i=0;i<N;i++)
            {
                ret[i] = Point.Round(pts[i]);
            }
            return ret;
        }
        static PointF[] ToPointF(Point[] pts)
        {
            int N = pts.Length;
            var ret = new PointF[N];
            for (int i = 0; i < N; i++)
            {
                ret[i] = pts[i];
            }
            return ret;
        }
        string getDefaultIniFile()
        {
            string path = Universal.WORKPATH;
            return System.IO.Path.Combine(path, "coreMarkPts.ini");
        }        
        #endregion


        #region AUTO_LAYOUT
        void _auto_layout()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            _adjust_to_center_vertically(radioButton1, btnFuncCenterManualGB, btnFuncCenterManualR, btnFuncCenterSaveK);
            _adjust_to_center_vertically(radioButton2, btnFuncProjAuto, btnFuncProjManual, btnFuncProjSaveK);
            _adjust_to_center_vertically(btnOK, btnCancel);

            // Adjust radioButton2 ~ btnFuncProjXXX
            int gap0 = btnFuncCenterManualGB.Left - radioButton1.Right;
            int ww = btnFuncProjSaveK.Right - radioButton2.Left;
            int x0 = radioButton1.Left;
            int x = gdxDispUI1.Right - ww - x0;
            x = Math.Max(x, btnFuncCenterSaveK.Right + gap0);
            int dx = x - radioButton2.Left;
            radioButton2.Left += dx;
            btnFuncProjAuto.Left += dx;
            btnFuncProjManual.Left += dx;
            btnFuncProjSaveK.Left += dx;

            // Adjust OK and Cancel Buttons
            int gap2 = (propertyGrid1.Width - gap0 - btnOK.Width * 2) / 2;
            var rcc = btnCancel.Parent.ClientRectangle;
            int y = rcc.Right - gap2;
            btnCancel.Left = y - btnCancel.Width;
            btnOK.Left = btnCancel.Left - gap0 - btnOK.Width;

            richTextBox1.Width = gdxDispUI1.Width - 2;
        }
        void _adjust_to_center_vertically(params Control[] ctrls)
        {
            foreach (Control c in ctrls)
            {
                var rcc = c.Parent.ClientRectangle;
                c.Top = (rcc.Height - c.Height) / 2;
            }
        }
        #endregion
    }
}
