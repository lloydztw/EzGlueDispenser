using JetEazy.BasicSpace;
using JetEazy.QMath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JetEazy.GdxCore3.Model
{
    /// <summary>
    /// 局域平面座標系統 <br/>
    /// @LETIAN: 20220625 creation
    /// </summary>
    class GdxLocalPlaneCoord
    {
        public bool IsBuilt
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// 馬達基點 (Xc, Yc, Zc) (mm)
        /// </summary>
        public QVector MotorBasePos
        {
            get;
            private set;
        }

        /// <summary>
        /// 所有量測到的雷射點位最小值
        /// </summary>
        public double Lmin
        {
            get
            {
                return FacadeCenter != null ? FacadeCenter[3] : 0;
            }
        }

        /// <summary>
        /// Y-Z 平面的中心點: (Xc, Yc, Zc, Lmin)
        /// </summary>
        public QVector FacadeCenter
        {
            get;
            private set;
        }

        /// <summary>
        /// 參考 RealSurfaceCenter 解說
        /// </summary>
        public double LcDepth
        {
            get;
            private set;
        }

        /// <summary>
        /// (X,Y,Z,L) 實際空間中心點: (Xc, Yc, Zc, Lmin + LcDepth)
        /// </summary>
        public QVector RealSurfaceCenter
        {
            get;
            private set;
        }

        /// <summary>
        /// 法向量
        /// </summary>
        public QVector PlaneNormalVector
        {
            get;
            private set;
        }

        /// <summary>
        /// degree
        /// </summary>
        public double ThetaY
        {
            get;
            private set;
        }

        /// <summary>
        /// degree
        /// </summary>
        public double ThetaZ
        {
            get;
            private set;
        }

        /// <summary>
        /// (Y,Z,L) 應射到 laser標點 相對座標 (lx, ly, lz) <br/>
        /// 相對於 (Yc,Zc,Lmin)
        /// </summary>
        QPoint3D[] RelativePts;

        /// <summary>
        /// 建立空間轉換資料
        /// </summary>
        public void BuildTransform(List<QVector> pointsXYZL)
        {
            //(0) Check Numbers
            int N = pointsXYZL.Count;
            if (N < 3)
            {
                IsBuilt = false;
                return;
            }

            //(1) Xc, Yc, Zc, Lmin
            double lmin = double.MaxValue;
            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double zmin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;
            double zmax = double.MinValue;
            for (int i = 0; i < N; i++)
            {
                var pos = pointsXYZL[i];              
                lmin = Math.Min(lmin, pos[3]);
                xmin = Math.Min(xmin, pos[0]);
                xmax = Math.Max(xmax, pos[0]);
                ymin = Math.Min(ymin, pos[1]);
                ymax = Math.Max(ymax, pos[1]);
                zmin = Math.Min(zmin, pos[2]);
                zmax = Math.Max(zmax, pos[2]);
            }
            double Xc = (xmin + xmax) / 2;
            double Yc = (ymin + ymax) / 2;
            double Zc = (zmin + zmax) / 2;
            FacadeCenter = new QVector(Xc, Yc, Zc, lmin);
            MotorBasePos = new QVector(Xc, Yc, Zc);

            //(2) Relative Points
            //    (lx, ly, lz) = (Y, Z, L) - (Yc, Zc, Lmin)
            //    var org = new QVector(FacadeCenter);
            var pts = new QPoint3D[N];
            for (int i = 0; i < N; i++)
            {
                var v = pointsXYZL[i];
                var lx = v[1]-Yc;
                var ly = v[2]-Zc;
                var lz = v[3]-lmin;
                pts[i] = new QPoint3D(lx, ly, lz);
            }
            RelativePts = pts;

            //-----------------------------------------------------------------------------
            //(3) Plane Formula
            QPlane plane = new QPlane();
            plane.LeastSquareFit(pts);
            //(4) Plane Normal Vector
            //      Ax + By + C = z
            //      ax + by + cz + d = 0
            //      a=A, b=B, c=-1, d=C
            //-----------------------------------------------------------------------------
            // 注意數值問題
            var old_percision = QVector.Percision;
            QVector.Percision = 10;
            //-----------------------------------------------------------------------------
            var normalV = new QVector(plane.A, plane.B, -1);
            normalV /= normalV.NormLength;
            // 法向量朝 z 正方向
            if (normalV.Z < 0)
                normalV = normalV * (-1);
            PlaneNormalVector = normalV;
            //-----------------------------------------------------------------------------
            QVector.Percision = old_percision;
            //-----------------------------------------------------------------------------

            //(4) LcDepth
            // lz = A * lx + B * ly + C (at 0,0,0)
            //    = plane.C
            LcDepth = plane.C;

            //(5) RealSurface Center
            RealSurfaceCenter = new QVector(Xc, Yc, Zc, Lmin + LcDepth);

            //(6.1) angle_lx
            double angle_lx = Math.Atan2(normalV.y, normalV.z);
            ThetaZ = angle_lx * 180.0 / Math.PI;

            //(6.2) angle_ly
            double angle_ly = Math.Atan2(normalV.x, normalV.z);
            ThetaY = angle_ly * 180.0 / Math.PI;

            IsBuilt = true;
        }
    }


    /// <summary>
    /// 雷射量測中心補償 <br/>
    /// @LETIAN: 20220625 creation
    /// </summary>
    public class GdxLaserCenterCompensator : IDisposable
    {
        #region PRIVATE_DATA
        /// <summary>
        /// (X,Y,Z,L)
        /// </summary>
        List<QVector>[] _laserRunPts = new List<QVector>[] { 
            new List<QVector>(),
            new List<QVector>(),
        };
        #endregion

        #region PRIVATE_PLANE_COORDS
        GdxLocalPlaneCoord m_xplaneGolden = new GdxLocalPlaneCoord();
        GdxLocalPlaneCoord m_xplaneMirror0 = new GdxLocalPlaneCoord();
        GdxLocalPlaneCoord m_xplaneMirror1 = new GdxLocalPlaneCoord();
        #endregion

        public GdxLaserCenterCompensator()
        {
        }
        public void Dispose()
        {
            // RESERVED.
        }

        public void ResetLaserPtsOnMirror(int mirrorIdx)
        {
            _laserRunPts[mirrorIdx].Clear();
        }
        public void AddLaserPtOnMirror(int mirrorIdx, QVector pos)
        {
            _laserRunPts[mirrorIdx].Add(pos);
        }
        public void BuildMirrorPlaneTransform(int mirrorIdx)
        {
            BuildGoldenPlaneFormula();
            var xplane = mirrorIdx == 0 ? m_xplaneMirror0 : m_xplaneMirror1;
            xplane.BuildTransform(_laserRunPts[mirrorIdx]);
            Save();
        }
        public void BuildGoldenPlaneFormula()
        {
            m_xplaneGolden.BuildTransform(GdxGlobal.INI.GaugeBlockPlanePoints);
        }
        
        public bool CanCompensate(int mirrorIdx)
        {
            if (mirrorIdx == 0)
                return m_xplaneGolden.IsBuilt && m_xplaneMirror0.IsBuilt;
            else
                return m_xplaneGolden.IsBuilt && m_xplaneMirror1.IsBuilt;
        }
        public QVector CalcCompensation(int mirrorIndex, QVector currentMotorPosAx6)
        {
            int N6 = currentMotorPosAx6.Dimensions;
            if (!CanCompensate(mirrorIndex))
                return new QVector(N6); // no increment

            var xplane = (mirrorIndex == 0) ? m_xplaneMirror0 : m_xplaneMirror1;

            // pickerTouchPos: INI 分開設定 U軸, (INI.AttractPos 只有三軸)
            var pickerTouchPos = GdxGlobal.INI.AttractPos;
            var pickerTouchU = GdxGlobal.INI.Offset_ModuleZ;

            // 三軸 to 四軸坐標系
            var pickerTouchPosAx4 = pickerTouchPos.Expand(4);
            pickerTouchPosAx4[3] = pickerTouchU;
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[0] == pickerTouchPos[0]);
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[1] == pickerTouchPos[1]);
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[2] == pickerTouchPos[2]);
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[3] == pickerTouchU);

            // Offsets
            var centerDiff = xplane.RealSurfaceCenter - m_xplaneGolden.RealSurfaceCenter;
            var targetAx4 = pickerTouchPosAx4 + centerDiff;

            // Laser and U
            double laserD = targetAx4[3];
            double currU = currentMotorPosAx6[3];
            double incrU = (laserD - currU) * 0.999;

            // 四軸 to 六軸坐標系
            var targetAx6 = targetAx4.Expand(N6);
            var incr = targetAx6 - currentMotorPosAx6;

            // Adjust U axis
            incr[3] = incrU;
            return incr;
        }

        public void Save(string fileName = null)
        {
            if (fileName == null)
                fileName = getDefaultFileName();

            var list = new List<GdxLocalPlaneCoord>();
            if (m_xplaneGolden.IsBuilt)
                list.Add(m_xplaneGolden);

            if (m_xplaneMirror0.IsBuilt)
                list.Add(m_xplaneMirror0);

            if (m_xplaneMirror1.IsBuilt)
                list.Add(m_xplaneMirror1);

            string jstr = JsonConvert.SerializeObject(list, Formatting.Indented);
            using (var stm = new System.IO.StreamWriter(fileName, false))
            {
                stm.Write(jstr);
                stm.Flush();
                stm.Close();
            }
        }
        public void Load(string fileName = null)
        {
            if (fileName == null)
                fileName = getDefaultFileName();

            // RESERVED
        }

        #region PRIVATE_FUNCTIONS
        string getDefaultFileName()
        {
            return @"D:\EVENTLOG\Nlogs\" + GetType().Name + ".json";
        }
        #endregion
    }
}
