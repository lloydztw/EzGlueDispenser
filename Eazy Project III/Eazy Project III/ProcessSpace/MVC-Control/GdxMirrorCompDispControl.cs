using Eazy_Project_III.ProcessSpace;
using JetEazy.GdxCore3;
using JetEazy.ImageViewerEx;
using JetEazy.ProcessSpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using ZxCore3.Gui;

namespace Eazy_Project_III.MVC_Control
{
    /// <summary>
    /// 為 DispUI 擴增 顯示中光電補償圖示
    /// </summary>
    class GdxMirrorCompDispControl
    {
        #region PRIVATE_DATA

        CvCompInfoGlyph m_compInfoGlyph = new CvCompInfoGlyph();
        #endregion

        public GdxMirrorCompDispControl(GdxDispUI dispUI)
        {
            dispUI.ImageViewer.AddInteractor(m_compInfoGlyph);
            m_compInfoGlyph.Visible = false;
        }
        public void Attach(MirrorAbsImageProcess process)
        {

        }

        #region PRIVATE_FUNCTIONS
        
        #endregion
    }


    class CvCompInfoGlyph : CvImageViewerInteractor
    {
        #region PRIVATE_DATA
        int m_contentType = 0;
        int m_mirrorIdx;
        CoreCompInfo m_compInfo;
        #endregion

        public CvCompInfoGlyph()
        {
        }
        public void Attach(string name, int mirrorIdx, CoreCompInfo info)
        {
            m_mirrorIdx = mirrorIdx;
            m_compInfo = info;

            if (name.Contains("光斑"))
                m_contentType = 3;
            else
                m_contentType = 2;
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (m_compInfo == null)
                return;

            if (m_contentType == 3)
            {
                draw_content_03(viewer, gxView);
            }
            else if (m_contentType == 2)
            {
                draw_content_02(viewer, gxView);
            }
            else
            {
            }
        }

        #region PRIVATE_FUNCTIONS
        void draw_content_02(CvImageViewer viewer, Graphics gx)
        {
            bool isWorld = viewer.IsInWorldCoordinate();

            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);

            var rects = m_compInfo.Rects;
            foreach (var rc in rects)
            {
                draw_cross_marks(viewer, gx, rc);
            }

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);
        }
        void draw_content_03(CvImageViewer viewer, Graphics gx)
        {
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
