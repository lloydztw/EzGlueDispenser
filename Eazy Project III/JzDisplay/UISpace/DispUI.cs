using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using MoveGraphLibrary;

using JetEazy;
using JetEazy.BasicSpace;
using JzDisplay.OPSpace;

namespace JzDisplay.UISpace
{
    public partial class DispUI : UserControl
    {
        Label lblInformation;
        PictureBox picDisplay;
        Label lblTestInformation;

        JzTimes myJzTimes = new JzTimes();
        
        OPDisplay OPDISP;
        public bool ISMOUSEDOWN
        {
            get
            {
                return OPDISP.ISMOUSEDOWN;
            }
        }
        public DispUI()
        {
            InitializeComponent();
            InitialInternal();
        }
        public int DisplayWidth
        {
            get
            {
                return picDisplay.Width;
            }
        }
        public int DisplayHeight
        {
            get
            {
                return picDisplay.Height;
            }
        }
        void InitialInternal()
        {
            lblInformation = label1;
            picDisplay = pictureBox1;
            lblTestInformation = label2;
        }
        public void Initial()
        {
            Initial(10, 0.1f);
        }
        public void Initial(float maxratio,float minratio)
        {
            OPDISP = new OPDisplay(picDisplay, lblInformation, maxratio, minratio);

            OPDISP.MoverAction += OPDISP_MoverAction;
            OPDISP.AdjustAction += OPDISP_AdjustAction;
            OPDISP.DebugAction += OPDISP_DebugAction;
            OPDISP.CaptureAction += OPDISP_CaptureAction;
            
            picDisplay.MouseDown += picDisplay_MouseDown;
            
        }

        public bool DispUIload(Form myForm)
        {
            ProjectForAllinone.ProjectClass project = new ProjectForAllinone.ProjectClass();
            bool isok = project.GetDecode(JetEazy.Universal.MYDECODE);
            if (!isok)
            {
                myForm.Close();
                OPDISP = null;
            }
            return isok;
        }

        private void picDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            picDisplay.Focus();
        }
        private void OPDISP_DebugAction(string opstring)
        {
            //lblTestInformation.Text = opstring;
            lblTestInformation.Invalidate();

            OnDebug(opstring);
        }
        private void OPDISP_AdjustAction(PointF ptfoffset)
        {
            OnAdjustAction(ptfoffset);
        }
        private void OPDISP_MoverAction(MoverOpEnum moverop, string opstring)
        {
            OnMover(moverop, opstring);
        }
        private void OPDISP_CaptureAction(RectangleF rectf)
        {
            OnCapture(rectf);
        }
        protected override void OnLoad(EventArgs e)
        {
            lblInformation.Location = new Point(0, 0);
            picDisplay.Location = new Point(0, 23);

            lblInformation.Width = this.Width;
            lblInformation.Height = 23;

            picDisplay.Width = this.Width;
            picDisplay.Height = this.Height - lblInformation.Height;

            base.OnLoad(e);
        }

        #region Normal Functions
        public void ClearAll()
        {
            OPDISP.ClearDisplay();
        }
        public void ClearMover()
        {
            OPDISP.ClearMover();
        }
        public void SaveStatus(string pathname)
        {
            Bitmap bmp = new Bitmap(OPDISP.GetPaintImage());

            bmp.Save(pathname + "\\TEST.png", ImageFormat.Png);

            string statusstring = OPDISP.ToStatusString();

            StreamWriter sw = new StreamWriter(pathname + "\\Status.jdb");

            sw.Write(statusstring);

            sw.Flush();
            sw.Close();
            sw.Dispose();

            bmp.Dispose();
        }
        public void LoadStatus(string pathname)
        {
            Bitmap bmp = new Bitmap(pathname + "\\TEST.png");

            OPDISP.SetDisplayImage(bmp, true);

            StreamReader sr = new StreamReader(pathname + "\\Status.jdb");

            string statusstring = sr.ReadToEnd();

            OPDISP.FromStatusString(statusstring);

            sr.Close();
            sr.Dispose();

            bmp.Dispose();
        }
        public void SetDisplayImage(Bitmap bmp)
        {
            OPDISP.SetDisplayImage(bmp);
        }
        public void SetDisplayImage()
        {
            OPDISP.SetDisplayImage();
        }
        public void SetDisplayImage(string bmpstring)
        {
            Bitmap bmp = new Bitmap(1, 1);
            GetBMP(bmpstring,ref bmp);

            OPDISP.SetDisplayImage(bmp, true);

            bmp.Dispose();
        }
        public void ReplaceDisplayImage(Bitmap bmp)
        {
            OPDISP.ReplaceDisplayImage(bmp);
        }
        public void RefreshDisplayShape()
        {
          
            OPDISP.RefreshDisplayShape();
          
        }

        public override void Refresh()
        {
            lblInformation.Location = new Point(0, 0);
            picDisplay.Location = new Point(0, 23);

            lblInformation.Width = this.Width;
            lblInformation.Height = 23;

            picDisplay.Width = this.Width;
            picDisplay.Height = this.Height - lblInformation.Height;

            base.Refresh();
        }

        public void DefaultView()
        {
            OPDISP.DefaultView();

            //lblInformation.Location = new Point(0, 0);
            //picDisplay.Location = new Point(0, 23);

            //lblInformation.Width = this.Width;
            //lblInformation.Height = 23;

            //picDisplay.Width = this.Width;
            //picDisplay.Height = this.Height - lblInformation.Height;

            //this.Refresh();


        }
        public void SetDisplayType(DisplayTypeEnum displaytype)
        {
            OPDISP.DISPLAYTYPE = displaytype;
        }
        public void SetMover(Mover mover)
        {
            OPDISP.SetMover(mover);
        }
        public void SetStaticMover(Mover staticmover)
        {
            OPDISP.SetStaticMover(staticmover);
        }
        public void ClearStaticMover()
        {
            OPDISP.ClearStaticMover();
        }

        public void SaveScreen()
        {
            OPDISP.SaveScreen();
        }
        public Bitmap GetScreen()
        {
            return OPDISP.GetScreen();
        }
        public void BackupImage()
        {
            OPDISP.BackupImage();
        }
        public void RestoreImage()
        {
            OPDISP.RestoreImage();
        }
        public void Suicide()
        {
            OPDISP.Suicide();
        }
        void GetBMP(string bmpstring, ref Bitmap bmp)
        {
            Bitmap bmptmp = new Bitmap(bmpstring);

            bmp.Dispose();
            bmp = new Bitmap(bmptmp);

            bmptmp.Dispose();
        }
        public void MoveMover(int x,int y)
        {
            OPDISP.MoveMover(x, y);
            OPDISP.MappingShape();

        }
        public void SizeMover(int x,int y)
        {
            OPDISP.SizeMover(x, y);
        }
        public void Lock(int level,bool isonly)
        {
            OPDISP.Lock(level, isonly);
        }
        public void SetMatching(Bitmap bmp,MatchMethodEnum matchmethod)
        {
            OPDISP.SetMatching(bmp, matchmethod);
        }

        public void GenSearchImage(ref Bitmap bmp)
        {
            OPDISP.GetSerachImage(ref bmp);
        }
        public void ReDraw()
        {
            OPDISP.ReDraw();
        }

        public Bitmap GetOrgBMP()
        {
            return OPDISP.GetOrgBMP();
        }
        /// <summary>
        /// 在Capture時使用的抓圖方式
        /// </summary>
        /// <param name="rectf"></param>
        /// <returns></returns>
        public Bitmap GetOrgBMP(RectangleF rectf)
        {
            Size orgsize = OPDISP.GetOrgBMP().Size;
            RectangleF orgrectf = new RectangleF(0, 0, orgsize.Width, orgsize.Height);

            orgrectf.Intersect(rectf);

            return (Bitmap)OPDISP.GetOrgBMP().Clone(orgrectf, PixelFormat.Format32bppArgb);
        }

        public void SaveOrgBMP(string savefilename)
        {
            OPDISP.GetOrgBMP().Save(savefilename);
        }

        #endregion

        #region TEST Functions
        public void SimWheel()
        {
            OPDISP.SimWheel(1);
        }
        public void AddRect()
        {
            OPDISP.AddRect();
        }
        public void AddShape(ShapeEnum shape)
        {
            OPDISP.AddShape(shape);
        }
        public void DelShape()
        {
            OPDISP.DelShape();
        }
        public void HoldSelect()
        {
            OPDISP.IsHoldForSelct = true;
        }
        public void ReleaseSelect()
        {
            OPDISP.IsHoldForSelct = false;
        }
        public void GetMask(int outrangex, int outrangey)
        {
            OPDISP.GetMask(outrangex, outrangey);

        }
        public void MappingLsbSelect(List<int> lsbselectlist)
        {
            OPDISP.MappingLsbSelect(lsbselectlist);
        }
        public void MappingSelect()
        {
            OPDISP.MappingSelect();
        }
        #endregion

        public delegate void MoverHandler(MoverOpEnum moverop, string opstring);
        public event MoverHandler MoverAction;
        public void OnMover(MoverOpEnum moverop, string opstring)
        {
            if (MoverAction != null)
            {
                MoverAction(moverop, opstring);
            }
        }

        public delegate void AdjustHandler(PointF ptfoffset);
        public event AdjustHandler AdjustAction;
        public void OnAdjustAction(PointF ptfoffset)
        {
            if (AdjustAction != null)
            {
                AdjustAction(ptfoffset);
            }
        }

        public delegate void CaputreHandler(RectangleF rectf);
        public event CaputreHandler CaptureAction;
        public void OnCapture(RectangleF rectf)
        {
            if (CaptureAction != null)
            {
                CaptureAction(rectf);
            }
        }
        public delegate void DebugHandler(string opstring);
        public event DebugHandler DebugAction;
        public void OnDebug(string opstring)
        {
            if (DebugAction != null)
            {
                DebugAction(opstring);
            }
        }
    }
}
