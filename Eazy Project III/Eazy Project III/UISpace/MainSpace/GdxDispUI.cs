using Eazy_Project_III.ProcessSpace;
using JetEazy.GdxCore3;
using JetEazy.GUI;
using JetEazy.ImageViewerEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace ZxCore3.Gui
{
    public partial class GdxDispUI : UserControl
    {
        #region PRIVATE_DATA
        CvCompInfoGlyph m_compInfoGlyph = new CvCompInfoGlyph();
        #endregion

        public GdxDispUI()
        {
            InitializeComponent();
            SizeChanged += GdxDispUI_SizeChanged;
            HandleCreated += GdxDispUI_HandleCreated;
        }

        #region WINDOW_EVENT_HANDLERS
        private void GdxDispUI_HandleCreated(object sender, EventArgs e)
        {
            qxImageViewer1.Attach(lblCoordInfo);
            qxImageViewer1.AddInteractor(m_compInfoGlyph);
            m_compInfoGlyph.Visible = false;
        }
        private void GdxDispUI_SizeChanged(object sender, EventArgs e)
        {
            qxImageViewer1.Height = ClientSize.Height - panelTitleBar.Height;
        }
        private void chkAutoZoom_CheckedChanged(object sender, EventArgs e)
        {
            this.AutoZoomEnabled = chkAutoZoom.Checked;
        }
        #endregion

        [Browsable(false)]
        public IvImageContainer<Bitmap> ImageContainer
        {
            get
            {
                return qxImageViewer1;
            }
        }

        [Browsable(false)]
        public IvImageViewer ImageViewer
        {
            get
            {
                return qxImageViewer1;
            }
        }

        #region GUI_LAYOUT_FUNCTIONS
        public void AlignToTitleBarDockArea(params Control[] args)
        {
            Rectangle rcc = Parent.ClientRectangle;
            int pad = 3;
            int wspan = pad;
            
            List<Control> ctrls = new List<Control>();
            ctrls.Add(chkAutoZoom);
            ctrls.AddRange(args);
            //chkAutoZoom.Visible = true;

            foreach(var c in ctrls)
            {
                wspan += c.Width + pad;
            }
            int top = rcc.Top + pad;
            int h = panelTitleBar.Height - pad * 2;
            int x = rcc.Width - wspan;
            foreach (var c in ctrls)
            {
                c.Left = x;
                c.Top = top;
                c.Height = h;
                x += c.Width + pad;
                c.BringToFront();
            }
        }
        public void DockToTitleBar(Control c)
        {           
            // 會閃爍
            panelTitleBar.Controls.Add(c);
            c.BringToFront();
            c.Dock = DockStyle.Right;
            c.Parent = panelTitleBar;
        }
        #endregion

        protected bool AutoZoomEnabled
        {
            get;
            set;
        } = false;

        internal void UpdateLiveImage(Bitmap bmp)
        {
            ImageContainer.CopyFrom(bmp);
        }
        
        internal void UpdateCompensatedInfo(CompensatedInfoEventArgs e)
        {
            if(e == null)
            {
                m_compInfoGlyph.Visible = false;
                //> _auto_zoom_to(null);
                if (ImageViewer is QxImageViewer)
                {
                    ((QxImageViewer)ImageViewer).CrosshairsVisible = false;
                }
            }
            else
            {
                UpdateLiveImage(e.Image);
                m_compInfoGlyph.Attach(e.Name, e.MirrorIndex, e.Info);
                m_compInfoGlyph.Visible = true;
                _auto_zoom_to(e.Info);
            }
        }

        internal void UpdateLocatedMarks(CoreMarkPointEventArgs e)
        {
            if (e != null)
            {
                UpdateLiveImage(e.Image);
                m_compInfoGlyph.Attach("定位點檢", e.GoldenPts, e.AlgoPts);
                m_compInfoGlyph.Visible = true;
            }
            else
            {
                m_compInfoGlyph.Attach(null, null, null);
                m_compInfoGlyph.Visible = false;                
            }
        }


        #region PRIVATE_FUNCTIONS
        private void _auto_zoom_to(CoreCompInfo info)
        {
            var viewer = qxImageViewer1;

            if (!AutoZoomEnabled)
            {
                viewer.Invalidate();
                return;
            }

            if (info == null)
            {
                viewer.EnableAutoGoldenZoomScale(true);
                viewer.Invalidate();
            }
            else
            {
                var infoBound = _find_boundary(info.Rects);
                if (infoBound.Width > 0 && infoBound.Height > 0)
                {
                    //var camBound = viewer.GetWorldRect();
                    var viewBound = viewer.GetViewportRect();
                    viewer.TransWorldToViewport(ref infoBound);
                    //viewer.TransWorldToViewport(ref camBound);
                    //var zoom1 = camBound.Width / infoBound.Width / 12f;
                    //var zoom2 = camBound.Height / infoBound.Height / 12;
                    var zoom3 = viewBound.Width / infoBound.Width;
                    var zoom4 = viewBound.Height / infoBound.Height;
                    viewer.EnableAutoGoldenZoomScale(true);
                    var zoomG = viewer.GetZoomScale();
                    var zoom = Math.Min(zoom3, zoom4);
                    zoom = zoom * zoomG * 0.7f;
                    //zoom = Math.Min(zoom, viewBound.Width / infoBound.Width / 8f);
                    //zoom = Math.Min(zoom, viewBound.Height / infoBound.Height / 8f);
                    var x = infoBound.X + infoBound.Width / 2;
                    var y = infoBound.Y + infoBound.Height / 2;
                    // viewer.TransWorldToViewport(ref x, ref y);
                    viewer.Zoom(zoom, (int)x, (int)y);
                }
            }
        }
        private RectangleF _find_boundary(CoreCompRect[] rects)
        {
            if (rects != null && rects.Length > 0)
            {
                var rc = _to_rect(rects[0]);
                for (int i = 1; i < rects.Length; i++)
                    rc = RectangleF.Union(rc, _to_rect(rects[i]));
                return rc;
            }
            return RectangleF.Empty;
        }
        private RectangleF _to_rect(CoreCompRect rect)
        {
            var x = rect.Center.X - rect.Size.Width / 2;
            var y = rect.Center.Y - rect.Size.Height / 2;
            return new RectangleF(x, y, rect.Size.Width, rect.Size.Height);
        }
        #endregion

        
    }



    class CvCompInfoGlyph : CvImageViewerInteractor
    {
        #region PRIVATE_DATA
        Action<CvImageViewer, Graphics> m_drawFunc = null;
        int m_mirrorIdx;
        CoreCompInfo m_compInfo;
        Point[] m_goldenPts;
        Point[] m_algoPts;
        #endregion

        public CvCompInfoGlyph()
        {
        }        
        public void Attach(string name, int mirrorIdx, CoreCompInfo info)
        {
            m_mirrorIdx = mirrorIdx;
            m_compInfo = info;

            if (name.Contains("光斑"))
                m_drawFunc = draw_content_03;
            else
                m_drawFunc = draw_content_02;
        }
        public void Attach(string name, Point[] goldenPts, Point[] algoPts)
        {
            m_drawFunc = draw_content_00;
            m_goldenPts = goldenPts;
            m_algoPts = algoPts;
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (m_drawFunc != null)
                m_drawFunc(viewer, gxView);
        }

        #region PRIVATE_FUNCTIONS

        /// <summary>
        /// 定位點檢圖示
        /// </summary>
        void draw_content_00(CvImageViewer viewer, Graphics gx)
        {
            bool isWorld = viewer.IsInWorldCoordinate();

            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gx);

            if (m_algoPts != null)
            {
                foreach (var pt in m_algoPts)
                {
                    draw_center_mark_w(viewer, gx, pt.X, pt.Y, Brushes.Blue);
                }
            }

            if (m_goldenPts != null)
            {
                foreach(var pt in m_goldenPts)
                {
                    draw_golden_mark_w(viewer, gx, pt.X, pt.Y, Pens.Gold);
                }
            }

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gx);
        }

        /// <summary>
        /// 中心補償圖示
        /// </summary>
        void draw_content_02(CvImageViewer viewer, Graphics gx)
        {
            if (m_compInfo == null)
                return;

#if(false)
            bool isWorld = viewer.IsInWorldCoordinate();

            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);

            var rects = m_compInfo.Rects;
            foreach (var rc in rects)
            {
                draw_cross_marks(viewer, gx, rc);
            }

            int ptype = m_mirrorIdx == 0 ? 1 : 0;
            var goldenPt = GdxCore.getGoldenCenterPt(ptype);

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);
#endif

            bool isWorld = viewer.IsInWorldCoordinate();

            var rects = m_compInfo.Rects;

            if (true)
            {
                if (isWorld)
                    viewer.SwitchToViewportCoordinate(gx);

                int N = Math.Min(2, rects.Length);
                for (int i = 0; i < N; i++)
                    draw_rect(viewer, gx, rects[i], Pens.Blue);

                if (isWorld)
                    viewer.SwitchToWorldCoordinate(gx);
            }

            if (true)
            {
                if (!isWorld)
                    viewer.SwitchToWorldCoordinate(gx);

                foreach (var rc in rects)
                {
                    if (rc != null)
                        draw_center_mark_w(viewer, gx, rc.Center.X, rc.Center.Y, Brushes.Blue);
                }

                int ptype = m_mirrorIdx == 0 ? 1 : 0;
                var goldPt = GdxCore.getGoldenCenterPt(ptype);
                draw_golden_mark_w(viewer, gx, goldPt.X, goldPt.Y, Pens.Gold);

                if (!isWorld)
                    viewer.SwitchToViewportCoordinate(gx);
            }
        }

        /// <summary>
        /// 光斑補償圖示
        /// </summary>
        void draw_content_03(CvImageViewer viewer, Graphics gx)
        {
            if (m_compInfo == null)
                return;

            bool isWorld = viewer.IsInWorldCoordinate();

            var rects = m_compInfo.Rects;

            if (true)
            {
                if (isWorld)
                    viewer.SwitchToViewportCoordinate(gx);

                int N = Math.Min(2, rects.Length);
                for (int i = 0; i < N; i++)
                    draw_rect(viewer, gx, rects[i], Pens.Blue);

                if (isWorld)
                    viewer.SwitchToWorldCoordinate(gx);
            }

            if (true)
            {
                if (!isWorld)
                    viewer.SwitchToWorldCoordinate(gx);

                foreach (var rc in rects)
                {
                    if (rc != null)
                        draw_center_mark_w(viewer, gx, rc.Center.X, rc.Center.Y, Brushes.Blue);
                }

                var goldPts = m_compInfo.GoldenPts;
                if (goldPts != null)
                {
                    for (int i = 0; i < goldPts.Length; i++)
                    {
                        draw_golden_mark_w(viewer, gx, goldPts[i].X, goldPts[i].Y, Pens.Gold);
                    }
                }

                if (!isWorld)
                    viewer.SwitchToViewportCoordinate(gx);
            }
        }
        void draw_rect(CvImageViewer viewer, Graphics gx, CoreCompRect rc, Pen pen)
        {
            if (rc == null)
                return;

            var w = rc.Size.Width;
            var h = rc.Size.Height;
            var w2 = w / 2f;
            var h2 = h / 2f;
            var x = rc.Center.X - w2;
            var y = rc.Center.Y - h2;
            var x2 = x + w;
            var y2 = y + h;

            viewer.TransWorldToViewport(ref x, ref y);
            viewer.TransWorldToViewport(ref x2, ref y2);
            gx.DrawRectangle(pen, x, y, x2 - x, y2 - y);
        }
        void draw_cross_marks(CvImageViewer viewer, Graphics gx, CoreCompRect rc)
        {
            if (rc == null)
                return;

            var w = rc.Size.Width;
            var h = rc.Size.Height;
            var w2 = w / 2f;
            var h2 = h / 2f;

            draw_cross_mark(viewer, gx, rc.Center.X - w2, rc.Center.Y - h2, Pens.White);
            draw_cross_mark(viewer, gx, rc.Center.X - w2, rc.Center.Y + h2, Pens.White);
            draw_cross_mark(viewer, gx, rc.Center.X + w2, rc.Center.Y - h2, Pens.White);
            draw_cross_mark(viewer, gx, rc.Center.X + w2, rc.Center.Y + h2, Pens.White);
            draw_cross_mark(viewer, gx, rc.Center.X, rc.Center.Y, Pens.Goldenrod);
        }
        void draw_cross_mark(CvImageViewer viewer, Graphics gx, float x, float y, Pen pen)
        {
            var span = 10f;
            viewer.TransWorldToViewport(ref x, ref y);
            gx.DrawLine(pen, x - span, y, x + span, y);
            gx.DrawLine(pen, x, y - span, x, y + span);
        }
        void draw_center_mark_w(CvImageViewer viewer, Graphics gx, float x, float y, Brush br)
        {
            var span = 15f;
            //viewer.TransWorldToViewport(ref x, ref y);
            // 使用 world coordinates pixels.
            gx.FillEllipse(br, x - span, y - span, span * 2, span * 2);
        }
        void draw_golden_mark_w(CvImageViewer viewer, Graphics gx, float x, float y, Pen pen)
        {
            var span = 20f;
            //viewer.TransWorldToViewport(ref x, ref y);
            // 使用 world coordinates pixels.
            gx.DrawEllipse(pen, x - span, y - span, span * 2, span * 2);
        }
        #endregion
    }
}
