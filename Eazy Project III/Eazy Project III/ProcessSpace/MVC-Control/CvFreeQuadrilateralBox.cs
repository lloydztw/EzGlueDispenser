/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	    2008/12/01 The class is created by LeTian Chang
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/


using System;
using System.Drawing;
using System.Windows.Forms;
using JetEazy.GUI;
using JetEazy.ImageViewerEx;
using JetEazy.QMath;

namespace Paso.Aoi.GUI
{
    public class CvFreeQuadrilateralBox : CvImageViewerInteractor
    {
        static readonly Pen ACTIVE_PEN = Pens.Blue;
        static readonly Brush ACTIVE_BRUSH = Brushes.Blue;

        public event EventHandler OnSelected;

        #region PRIVATE_DATA_MEMBERS

        //===================================================================================
        // Corners:
        //-----------------------------------------------------------------------------------
        //  [0]---[1]
        //   |     |
        //  [3]---[2]
        //===================================================================================

        const int MAX_CORNERS = 5;
        protected int N_CORNERS = MAX_CORNERS;
        protected Rectangle m_rectBox;          // in world coordinate.
        protected PointF[] m_corners;           // in world coordinate.
        protected Point[] m_ptsHitCorners;      // in view coordinate.

        protected string m_strName;
        protected Brush m_brush;

        private Point m_pointMouseDown;
        private int m_iResizeAtCorner = -1;
        private bool m_bSizable = true;
        private bool m_bMove = false;
        protected int m_iLineWidth;
        protected int m_iCornerSize;
        private bool m_bypass = false;
        private bool m_alt;
        private int m_activeID = -1;
        private bool m_needsToCheckCrossLines = false;
        protected Cursor m_cursorMouseDown = Cursors.Default;
        protected Cursor m_cursorMouseMove = Cursors.Hand;
        #endregion

        public CvFreeQuadrilateralBox(Brush brush, int iLineWidth, int iCornerSize, int cornersNumber = 4)
        {
            _initCornersNumber(cornersNumber);
            //> N_CORNERS = Math.Min(Math.Max(cornersNumber, 3), 4);
            //> m_corners = new PointF[N_CORNERS];       // in world coordinate.
            //> m_ptsHitCorners = new Point[N_CORNERS];   // in view coordinate.

            m_brush = brush;

            if (iCornerSize == 0)
            {
                iCornerSize = 10;
            }

            m_iLineWidth = iLineWidth;
            m_iCornerSize = iCornerSize;

            this.Box = new Rectangle(150, 150, 300, 300);
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }
        public bool Sizable
        {
            get
            {
                return m_bSizable;
            }
            set
            {
                m_bSizable = value;
            }
        }
        public Brush BoxBrush
        {
            get
            {
                return m_brush;
            }
            set
            {
                ////////////////////////////////
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // The new brush only can be
                // one of the Brushes in stock
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                ////////////////////////////////
                Brush brOld = m_brush;
                m_brush = value;
                ////////////////////////////////
                // Sync?
                //if (brOld != null)
                //    brOld.Dispose();
                ////////////////////////////////
            }
        }

        public bool Bypass
        {
            get
            {
                return m_bypass;
            }
            set
            {
                if (m_strName != null && m_strName.Contains("GelBox"))
                    m_bypass = value;
            }
        }
        public Rectangle Box
        {
            get
            {
                return m_rectBox;
            }
            set
            {
                //====================================
                // Corners:
                //------------------------------------
                //  [0]---[1]
                //   |     |
                //  [3]---[2]
                //------------------------------------
                //  [0]\
                //   |  \   
                //  [1]--[2]
                //====================================
                m_rectBox = value;
                m_corners[0] = m_rectBox.Location;

                if (m_corners.Length > 3)
                {
                    m_corners[1] = new Point(m_rectBox.Right, m_rectBox.Top);
                    m_corners[2] = new Point(m_rectBox.Right, m_rectBox.Bottom);
                    m_corners[3] = new Point(m_rectBox.Left, m_rectBox.Bottom);
                }
                else
                {
                    m_corners[1] = new Point(m_rectBox.Right, m_rectBox.Bottom);
                    m_corners[2] = new Point(m_rectBox.Left, m_rectBox.Bottom);
                }
            }
        }

        /// <summary>
        /// In Pixel Coordinates
        /// </summary>
        public virtual PointF[] Corners
        {
            get
            {
                return m_corners;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length != N_CORNERS)
                        _initCornersNumber(value.Length);

                    int iLen = Math.Min(m_corners.Length, value.Length);
                    for (int i = 0; i < iLen; i++)
                        m_corners[i] = value[i];

                    _updateBox();
                }
            }
        }
        public int ActiveCornerID
        {
            get
            {
                return m_activeID;
            }
            set
            {
                if (m_activeID != value)
                {
                    m_activeID = value;
                    m_needsToCheckCrossLines = true;                    
                }
            }
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gx)
        {
            if (m_needsToCheckCrossLines)
            {
                m_needsToCheckCrossLines = false;
                _showCrossLine(viewer, _isDirectClickMode());
            }

            _updateHitCornerPoints(viewer);

#if (OPT_USING_VIEWPORT)

            float fZoomFactor = 1f; // viewer.GetZoomScale();

            bool isWorld = viewer.IsInWorldCoordinate();
            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);

            Pen pen = _createViewportPen(viewer, fZoomFactor);

            if (Bypass)
            {
                pen.Color = Color.WhiteSmoke;
            }

            // LINES
            gx.DrawLines(pen, m_ptsHitCorners);
            gx.DrawLine(pen, m_ptsHitCorners[N_CORNERS - 1], m_ptsHitCorners[0]);

            // CORNERS
            Pen pen2 = viewer.GetOnePixelPen(pen.Color);
            Rectangle rc;
            for (int i = 0; i < N_CORNERS; i++)
            {
                rc = _getHitCornerRect(i, fZoomFactor);
                _drawHitCorner(gx, i, ref rc, pen2);

                Font font = viewer.Font;
                PointF pt = rc.Location;
                float h = font.Size * 1.5f; ;
                switch (i)
                {
                    case 0: pt = new PointF(rc.X - h, rc.Y - h); break;
                    case 1: pt = new PointF(rc.Right, rc.Y - h); break;
                    case 2: pt = new PointF(rc.Right, rc.Bottom); break;
                    case 3: pt = new PointF(rc.X - h, rc.Bottom); break;
                }
                gx.DrawString((i + 1).ToString(), font, m_brush, pt);
            }

            // NAME
            if (!string.IsNullOrEmpty(m_strName))
            {
                /*
                rc = _getHitCornerRect(1, fZoomFactor);
                Point pt = rc.Location;
                pt.X += (int)(viewer.Font.Size * 1.5f);
                pt.Y -= (int)(viewer.Font.Size * 1.5f + rc.Height);
                gx.DrawString(m_strName, viewer.Font, m_brush, pt);
                */

                rc = _getTextHitRect(viewer, fZoomFactor);
                gx.DrawString(m_strName, viewer.Font, m_brush, rc.Location);
                //> gx.DrawRectangle(viewer.GetOnePixelPen(Color.Yellow), rc);
            }

            pen.Dispose();

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);

#else
            Color color = ((SolidBrush)m_brush).Color;
            Pen pen = viewer.GetOnePixelPen(color);

            // LINES
            #region CONTOUR_LINE
            if (N_CORNERS <= 4)
            {
                gx.DrawLines(pen, m_corners);
                gx.DrawLine(pen, m_corners[N_CORNERS - 1], m_corners[0]);
            }
            else
            {
                int N = Math.Min(N_CORNERS, 4);
                for (int i = 0; i < N; i++)
                {
                    int j = (i + 1) % N;
                    gx.DrawLine(pen, m_corners[i], m_corners[j]);
                }
            }
            #endregion

            // Turn To Viewport
            float fZoomFactor = viewer.GetZoomScale();

            bool isWorld = viewer.IsInWorldCoordinate();
            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);

            _drawActiveID(viewer, gx);

            pen = _createViewportPen(viewer, fZoomFactor);

            // CORNERS
            Rectangle rc;
            for (int i = 0; i < N_CORNERS; i++)
            {
                rc = _getHitCornerRect(i, fZoomFactor);
                _drawHitCorner(viewer, gx, i, ref rc, pen);
            }

            // NAME
            if (!string.IsNullOrEmpty(m_strName))
            {
                rc = _getHitCornerRect(0, fZoomFactor);
                Point pt = rc.Location;
                pt.Y -= (int)(viewer.Font.Size + 5);
                gx.DrawString(m_strName, viewer.Font, m_brush, pt);
            }

            // Dispose
            pen.Dispose();

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);

#endif
        }
        public override void OnKeyUp(CvImageViewer viewer, KeyEventArgs e)
        {
            base.OnKeyUp(viewer, e);
            m_alt = e.Alt;
        }
        public override void OnKeyDown(CvImageViewer viewer, KeyEventArgs e)
        {
            base.OnKeyDown(viewer, e);
            m_alt = e.Alt;

            int id = (int)(e.KeyCode - Keys.D1);
            if (id < 0 || id >= N_CORNERS)
                id = -1;

            if (m_activeID != id)
            {
                m_activeID = id;
                _showCrossLine(viewer, _isDirectClickMode());
                viewer.Invalidate();
            }



#if (OPT_RESERVED)
                if (this.Name != null && this.Name.Contains("GelBox"))
                {
                    if (e.KeyCode == Keys.A)
                    {
                        this.Bypass = false;
                    }
                    else
                    {
                        int id = (int)(e.KeyCode - Keys.D0);
                        if (this.Name.Contains(id.ToString()))
                        {
                            this.Bypass = !this.Bypass;
                        }
                    }
                }
#endif
        }
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return false;

#if (OPT_RESERVED)
                if (m_alt)
                {
                    Rectangle rectBoxA = m_rectBox;
                    viewer.TransWorldToViewport(ref rectBoxA);
                    if (rectBoxA.Contains(e.Location))
                    {
                        this.Bypass = !this.Bypass;
                    }
                    return true;
                }
#endif

            Point pt = e.Location;

            _updateHitCornerPoints(viewer);

            if (_isDirectClickMode())
            {
                _updateCornerPoint(viewer, m_activeID, e.Location);
                if (++m_activeID >= N_CORNERS)
                {
                    m_activeID = -1;
                    _showCrossLine(viewer, false);
                }
                OnSelected?.Invoke(this, null);
                return true;
            }


            #region HIT_TEST_TEXT_ZONE
            {
                Rectangle rcTxt = _getTextHitRect(viewer, viewer.GetZoomScale());
                if (rcTxt.Contains(e.Location) && this.Name.Contains("GelBox"))
                {
                    this.Bypass = !this.Bypass;
                    return true;
                }
            }
            #endregion

            //=============================================================
            // Corners:
            //=============================================================
            if (m_bSizable)
            {
                float fZoomFactor = viewer.GetZoomScale();
                for (int i = 0; i < N_CORNERS; i++)
                {
                    Rectangle rcHit = _getHitCornerRect(i, fZoomFactor);
                    if (rcHit.Contains(pt))
                    {
                        m_iResizeAtCorner = i;
                        m_pointMouseDown = e.Location;
                        break;
                    }
                }
            }

            Rectangle rectBox = m_rectBox;
            viewer.TransWorldToViewport(ref rectBox);
            if (rectBox.Contains(pt))
            {
                m_bMove = true;
                m_pointMouseDown = e.Location;
            }

            if (m_bMove)
                viewer.Cursor = m_cursorMouseDown;

            return m_bMove;
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            #region CODE_FOR_CHANGING_CORNERS

            if (m_bSizable && m_iResizeAtCorner >= 0)
            {
#if (false)
                PointF[] ptsNew = null;

                int x = e.Location.X;
                int y = e.Location.Y;
                viewer.TransViewportToWorld(ref x, ref y);

                switch (m_iResizeAtCorner)
                {
                    case 0: ptsNew = _makeFreeShape0(new PointF(x, y)); break;
                    case 1: ptsNew = _makeFreeShape1(new PointF(x, y)); break;
                    case 2: ptsNew = _makeFreeShape2(new PointF(x, y)); break;
                    case 3: ptsNew = _makeFreeShape3(new PointF(x, y)); break;
                }

                if (ptsNew != null)
                {
                #region CHECK_WORLD_BOUNDARY
                    RectangleF rectWorld = viewer.GetWorldRect();
                    for (int i = 0; i < N_CORNERS; i++)
                    {
                        if (!rectWorld.Contains(ptsNew[i]))
                        {
                            ptsNew = null;
                            break;
                        }
                    }
                #endregion
                }
                if (ptsNew != null)
                {
                #region UPDATE
                    for (int i = 0; i < N_CORNERS; i++)
                    {
                        m_corners[i] = ptsNew[i];
                    }
                    _updateHitCornerPoints(viewer);
                    _updateBox();
                    //> _runtimeLayoutScan();
                #endregion
                }
#endif
                _updateCornerPoint(viewer, m_iResizeAtCorner, e.Location);
                viewer.Cursor = Cursors.Hand;
                return true;
            }

            #endregion

            if (m_bMove)
            {
                #region GLOBAL_SHIFTING
                int x = e.Location.X;
                int y = e.Location.Y;
                int x0 = m_pointMouseDown.X;
                int y0 = m_pointMouseDown.Y;
                m_pointMouseDown = e.Location;

                viewer.TransViewportToWorld(ref x, ref y);
                viewer.TransViewportToWorld(ref x0, ref y0);

                int dx = x - x0;
                int dy = y - y0;

                RectangleF rectWorld = viewer.GetWorldRect();
                PointF[] ptsNew = new PointF[N_CORNERS];
                for (int i = 0; i < N_CORNERS; i++)
                {
                    ptsNew[i] = m_corners[i];
                    ptsNew[i].X += dx;
                    ptsNew[i].Y += dy;
                    if (!rectWorld.Contains(ptsNew[i]))
                    {
                        ptsNew = null;
                        break;
                    }
                }
                if (ptsNew != null)
                {
                    for (int i = 0; i < N_CORNERS; i++)
                    {
                        m_corners[i] = ptsNew[i];
                    }
                    _updateHitCornerPoints(viewer);
                    _updateBox();
                }

                viewer.Cursor = m_cursorMouseMove;

                return true;
                #endregion
            }

            #region CODE_FOR_SIZABLE

            if (m_bSizable && e.Button == MouseButtons.None)
            {
                int x = e.Location.X;
                int y = e.Location.Y;

                bool bHit = false;
                for (int i = 0; i < N_CORNERS; i++)
                {
                    Rectangle rc = _getHitCornerRect(i, viewer.GetZoomScale());
                    if (rc.Contains(x, y))
                    {
                        viewer.Cursor = Cursors.Hand;
                        bHit = true;
                        break;
                    }
                }
                if (!bHit)
                {
                    viewer.Cursor = Cursors.Default;
                }
            }

            #endregion

            if (_isDirectClickMode())
                return true;

            return false;
        }
        public override bool OnMouseUp(CvImageViewer viewer, MouseEventArgs e)
        {
            if (m_bMove || m_iResizeAtCorner >= 0)
            {
                _updateHitCornerPoints(viewer);
                _updateBox();
                viewer.Cursor = Cursors.Default;
                OnSelected?.Invoke(viewer, e);
            }

            m_iResizeAtCorner = -1;
            m_bMove = false;
            return false;
        }
        public override void SetCursors(Cursor curMouseDown, Cursor curMouseMove)
        {
            m_cursorMouseDown = curMouseDown;
            m_cursorMouseMove = curMouseMove;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        protected virtual void _initCornersNumber(int cornersNumber)
        {
            N_CORNERS = Math.Min(Math.Max(cornersNumber, 3), MAX_CORNERS);
            m_corners = new PointF[N_CORNERS];       // in world coordinate.
            m_ptsHitCorners = new Point[N_CORNERS];   // in view coordinate.
        }

        private bool _isDirectClickMode()
        {
            return m_activeID >= 0;
        }
        /// <summary>
        /// Viewport Coordinate
        /// </summary>
        private void _drawActiveID(CvImageViewer viewer, Graphics gx)
        {
            // this must be in viewport coordinates
            if (m_activeID >= 0)
            {
                using (var font = new Font(viewer.Font.FontFamily, 72f, FontStyle.Bold))
                {
                    var rcv = viewer.GetViewportRect();
                    float x = rcv.Width / 10f;
                    float y = rcv.Height / 10f;
                    gx.DrawString($" {m_activeID + 1}", font, Brushes.White, new PointF(x, y));
                }
            }
        }
        private void _showCrossLine(CvImageViewer viewer, bool show)
        {
            // BYPASS and RESERVED
            if (viewer is QxImageViewer)
            {
                ((QxImageViewer)viewer).CrosshairsVisible = show;
            }
        }

        /// <summary>
        /// Viewport Coordinate
        /// </summary>
        private void _drawHitCorner(CvImageViewer viewer, Graphics gx, int iCornerID, ref Rectangle rectHitCorner, Pen pen)
        {
            //if (iCornerID % 2 == 0)
            //    gx.FillRectangle(m_brush, rectHitCorner);
            //else
            //gx.FillEllipse(m_brush, rectHitCorner);
            //gx.FillEllipse(m_brush, rectHitCorner);

            var brush = m_brush;
            if (iCornerID == m_activeID)
            {
                pen = ACTIVE_PEN;
                brush = ACTIVE_BRUSH;
            }

            gx.DrawRectangle(pen, rectHitCorner);

            Point pt = m_ptsHitCorners[iCornerID];
            int w = rectHitCorner.Width;
            gx.DrawLine(pen, pt.X - w, pt.Y, pt.X + w, pt.Y);
            gx.DrawLine(pen, pt.X, pt.Y - w, pt.X, pt.Y + w);

            #region DRAW_CORNER_ID
            float x, y;
            float sz = viewer.Font.Size * 2;
            switch (iCornerID)
            {
                case 0:
                    x = rectHitCorner.X - sz;
                    y = rectHitCorner.Y - sz;
                    break;
                case 1:
                    x = rectHitCorner.X + sz;
                    y = rectHitCorner.Y - sz;
                    break;
                case 2:
                    x = rectHitCorner.X + sz;
                    y = rectHitCorner.Y + sz;
                    break;
                case 3:
                default:
                    x = rectHitCorner.X - sz;
                    y = rectHitCorner.Y + sz;
                    break;
            }
            gx.DrawString($"{iCornerID + 1}", viewer.Font, brush, x, y);
            #endregion
        }

        private void _updateBox()
        {
            int xMin = int.MaxValue;
            int yMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMax = int.MinValue;
            for (int i = 0; i < N_CORNERS; i++)
            {
                xMin = (int)Math.Min(xMin, m_corners[i].X);
                yMin = (int)Math.Min(yMin, m_corners[i].Y);
                xMax = (int)Math.Max(xMax, m_corners[i].X);
                yMax = (int)Math.Max(yMax, m_corners[i].Y);
            }
            m_rectBox = new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);

            //================================================================
            // CrossLine0 : c0 c2
            // CrossLine1 : c1 c3
            //================================================================
            if (N_CORNERS > 3)
            {
                m_crossLines[0].Build(m_corners[0], m_corners[2]);
                m_crossLines[1].Build(m_corners[1], m_corners[3]);
            }
        }
        private void _updateHitCornerPoints(CvImageViewer viewer)
        {
            for (int i = 0; i < N_CORNERS; i++)
            {
                int x = (int)m_corners[i].X;
                int y = (int)m_corners[i].Y;
                viewer.TransWorldToViewport(ref x, ref y);
                m_ptsHitCorners[i] = new Point(x, y);
            }
        }
        private void _updateCornerPoint(CvImageViewer viewer, int cornerID, Point viewPt)
        {
            PointF[] ptsNew = null;

            int x = viewPt.X;
            int y = viewPt.Y;
            viewer.TransViewportToWorld(ref x, ref y);

            switch (cornerID)
            {
                case 0: ptsNew = _makeFreeShape0(new PointF(x, y)); break;
                case 1: ptsNew = _makeFreeShape1(new PointF(x, y)); break;
                case 2: ptsNew = _makeFreeShape2(new PointF(x, y)); break;
                case 3: ptsNew = _makeFreeShape3(new PointF(x, y)); break;
                default:
                    ptsNew = _makeFreeShape(new PointF(x, y), cornerID);
                    break;
            }

            if (ptsNew != null)
            {
                #region CHECK_WORLD_BOUNDARY
                RectangleF rectWorld = viewer.GetWorldRect();
                for (int i = 0; i < N_CORNERS; i++)
                {
                    if (!rectWorld.Contains(ptsNew[i]))
                    {
                        ptsNew = null;
                        break;
                    }
                }
                #endregion
            }
            if (ptsNew != null)
            {
                #region UPDATE
                for (int i = 0; i < N_CORNERS; i++)
                {
                    m_corners[i] = ptsNew[i];
                }
                _updateHitCornerPoints(viewer);
                _updateBox();
                //> _runtimeLayoutScan();
                #endregion
            }
        }

        private Rectangle _getHitCornerRect(int iCornerID, float fZoomFactor)
        {
            int w = (int)(fZoomFactor * m_iCornerSize);
            if (w < 8)
                w = 8;
            Rectangle rc = new Rectangle(0, 0, w, w);
            w >>= 1;
            rc.X = m_ptsHitCorners[iCornerID].X - w;
            rc.Y = m_ptsHitCorners[iCornerID].Y - w;
            return rc;
        }
        private Rectangle _getTextHitRect(CvImageViewer viewer, float fZoomFactor)
        {
            Rectangle rc = _getHitCornerRect(1, fZoomFactor);

            rc.X += (int)(viewer.Font.Size * 1.5f);
            rc.Y -= (int)(viewer.Font.Size * 1.5f + rc.Height);

            if (this.Name != null)
            {
                int w = (int)(this.Name.Length * viewer.Font.Size);
                int h = (int)(2f * viewer.Font.Size);
                rc.Width = w;
                rc.Height = h;
            }

            return rc;
        }
        private Pen _createViewportPen(CvImageViewer viewer, float fZoomFactor)
        {
            Pen pen;

            //float fZoomFactor = viewer.GetZoomScale();

            if (m_iLineWidth <= 1)
            {
                pen = new Pen(m_brush, 1.0f);
            }
            else
            {
                float fLineWidth = fZoomFactor * m_iLineWidth;
                if (fLineWidth < 1.0f) fLineWidth = 1.0f;
                pen = new Pen(m_brush, fLineWidth);
            }
            return pen;
        }

        private QxLineFormula[] m_crossLines = new QxLineFormula[] {
                new QxLineFormula(),
                new QxLineFormula()
            };
        private PointF[] _makeFreeShape0(PointF ptNewCorner)
        {
            PointF[] pts = _makeFreeShape(ptNewCorner, 0);

            //----------------------------------------------------------------
            //> if (pts[0].X >= pts[1].X || pts[0].Y >= pts[3].Y)
            //>    return null;
            //----------------------------------------------------------------

            //================================================================
            // CrossLine0 : c0 c2
            // CrossLine1 : c1 c3
            //================================================================
            if (N_CORNERS > 3)
            {
                double sign0 = m_crossLines[1].SignTest(pts[0]);
                double sign2 = m_crossLines[1].SignTest(pts[2]);
                if (sign0 * sign2 >= 0)
                    return null;
            }

            return pts;
        }
        private PointF[] _makeFreeShape1(PointF ptNewCorner)
        {
            PointF[] pts = _makeFreeShape(ptNewCorner, 1);

            //----------------------------------------------------------------
            //> if (pts[1].X <= pts[0].X || pts[1].Y >= pts[2].Y)
            //>    return null;
            //----------------------------------------------------------------

            //================================================================
            // CrossLine0 : c0 c2
            // CrossLine1 : c1 c3
            //================================================================
            if (N_CORNERS > 3)
            {
                double sign1 = m_crossLines[0].SignTest(pts[1]);
                double sign3 = m_crossLines[0].SignTest(pts[3]);
                if (sign1 * sign3 >= 0)
                    return null;
            }

            return pts;
        }
        private PointF[] _makeFreeShape2(PointF ptNewCorner)
        {
            PointF[] pts = _makeFreeShape(ptNewCorner, 2);

            //----------------------------------------------------------------
            //> if (pts[2].X <= pts[3].X || pts[2].Y <= pts[1].Y)
            //>    return null;
            //----------------------------------------------------------------

            //================================================================
            // CrossLine0 : c0 c2
            // CrossLine1 : c1 c3
            //================================================================
            if (N_CORNERS > 3)
            {
                double sign0 = m_crossLines[1].SignTest(pts[0]);
                double sign2 = m_crossLines[1].SignTest(pts[2]);
                if (sign0 * sign2 >= 0)
                    return null;
            }

            return pts;
        }
        private PointF[] _makeFreeShape3(PointF ptNewCorner)
        {
            PointF[] pts = _makeFreeShape(ptNewCorner, 3);

            //----------------------------------------------------------------
            //> if (pts[3].X >= pts[2].X || pts[3].Y <= pts[0].Y)
            //>    return null;
            //----------------------------------------------------------------

            //================================================================
            // CrossLine0 : c0 c2
            // CrossLine1 : c1 c3
            //================================================================
            if (N_CORNERS > 3)
            {
                double sign1 = m_crossLines[0].SignTest(pts[1]);
                double sign3 = m_crossLines[0].SignTest(pts[3]);
                if (sign1 * sign3 >= 0)
                    return null;
            }

            return pts;
        }
        private PointF[] _makeFreeShape(PointF ptNewCorner, int iTargetID)
        {
            //====================================
            // Corners:
            //------------------------------------
            //  [0]---[1]
            //   |     |
            //  [3]---[2]
            //====================================
            /*
            PointF ptCenter = _calcCenter();
            double len = JetEazy.QvMath.QvBox2D._getLength(ptNewCorner, ptCenter);
            if (len < 5)
                return null;

            int idxA = (iTargetID + 1) % 4;
            int idxB = (iTargetID - 1) % 4;
            int idxC = (iTargetID + 2) % 4;
            if (idxB < 0) idxB += 4;

            double thA0 = JetEazy.QvMath.QvBox2D._getTheta(m_corners[iTargetID], m_corners[idxA]);
            double thB0 = JetEazy.QvMath.QvBox2D._getTheta(m_corners[iTargetID], m_corners[idxB]);
            double thA1 = JetEazy.QvMath.QvBox2D._getTheta(ptNewCorner, m_corners[idxA]);
            double thB1 = JetEazy.QvMath.QvBox2D._getTheta(ptNewCorner, m_corners[idxB]);

            double sin0 = Math.Abs(Math.Sin(thA0 - thB0));
            double sin1 = Math.Abs(Math.Sin(thA1 - thB1));
            double sinMin = Math.Sin(75 * Math.PI / 180);
            bool bOutOfAngle = sin1 < sinMin;
            bool bGetBetter = sin1 > sin0;
            */

            PointF[] ptsNew = new PointF[N_CORNERS];
            Array.Copy(m_corners, ptsNew, N_CORNERS);
            ptsNew[iTargetID] = ptNewCorner;

            /*
            if (bOutOfAngle && !bGetBetter)
            {
                double lenC = JetEazy.QvMath.QvBox2D._getLength(m_corners[idxC], ptsNew[iTargetID]);
                double thC = JetEazy.QvMath.QvBox2D._getTheta(m_corners[idxC], ptsNew[iTargetID]);

                thA0 = JetEazy.QvMath.QvBox2D._getTheta(m_corners[idxC], m_corners[idxA]);
                thB0 = JetEazy.QvMath.QvBox2D._getTheta(m_corners[idxC], m_corners[idxB]);
                double lenA = lenC * Math.Cos(thA0 - thC);
                double lenB = lenC * Math.Cos(thB0 - thC);

                ptsNew[idxA] = JetEazy.QvMath.QvBox2D._getEnpPoint(ptsNew[idxC], lenA, thA0);
                ptsNew[idxB] = JetEazy.QvMath.QvBox2D._getEnpPoint(ptsNew[idxC], lenB, thB0);
            }
            */

            return ptsNew;
        }

#if(OPT_NOT_USED)
            private PointF _calcCenter()
            {
                PointF pt = new PointF();
                for (int i = 0; i < N_CORNERS; i++)
                {
                    pt.X += m_corners[i].X;
                    pt.Y += m_corners[i].Y;
                }
                pt.X /= N_CORNERS;
                pt.Y /= N_CORNERS;
                return pt;
            }
            private double _normalizeAngle(double angle)
            {
                angle %= 360;

                if (angle < 0)
                    angle += 360;

                if (360 - angle < angle)
                    return 360 - angle;

                return angle;
            }
            private int _hitTestLine(PointF pt, PointF pt1, PointF pt2)
            {
                //=================================================================================
                // Line Formula
                //---------------------------------------------------------------------------------
                // (0) a x + b y = 1
                //     y = a x + b
                //     a x1 + b y1 = 1
                //     a x2 + b y2 = 1
                //     a (x2-x1) + b (y2-y1) = 0
                //     a (dx) = - b (dy)
                //
                // (1) a = - (dy/dx) b              apply to (0) ==> (-(dy/dx) x1) b + (y1) b = 1
                //     b = 1 / { y1 - (dy/dx) x1 }
                //
                // (2) b = - (dx/dy) a              apply to (0) ==> (x1) a + (-(dx/dy) y1) a = 1
                //     a = 1 / { x1 - (dx/dy) y1 }
                //=================================================================================
                double dx = pt2.X - pt1.X;
                double dy = pt2.Y - pt1.Y;

                double a;
                double b;

                if (Math.Abs(dy) > Math.Abs(dx))
                {
                    if (Math.Abs(dx) < 1)
                        dx = dx < 0 ? -1 : 1;

                    a = 1 / (pt1.X - (dx / dy) * pt1.Y);
                    b = -(dx / dy) * a;
                }
                else
                {
                    if (Math.Abs(dy) < 1)
                        dy = dy < 0 ? -1 : 1;

                    b = 1 / (pt1.Y - (dy / dx) * pt1.X);
                    a = -(dy / dx) * b;
                }

                //=================================================================================
                // Hit Test
                //---------------------------------------------------------------------------------
                // b y > -ax + 1
                // y > (1/b)(-ax + 1)
                //=================================================================================

                double c = a * pt.X + b * pt.Y;

                if (c > 1)
                    return 1;
                else if (c < 1)
                    return -1;
                else
                    return 0;
            }
#endif
        #endregion
    }
}
