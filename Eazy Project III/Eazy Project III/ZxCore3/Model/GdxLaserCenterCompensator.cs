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
        // static bool BYPASS_QC_LASER = false;
        const int N_MIRRORS = 2;

        #region PRIVATE_DATA
        /// <summary>
        /// (X,Y,Z,L)
        /// </summary>
        List<QVector>[] _laserRunPts = new List<QVector>[N_MIRRORS];
        QVector[] _compResult = new QVector[N_MIRRORS];
        bool[] _isBuiltOk = new bool[N_MIRRORS];
        #endregion

        #region PRIVATE_PLANE_COORDS
        GdxLocalPlaneCoord m_xplaneGolden = new GdxLocalPlaneCoord();
        GdxLocalPlaneCoord[] m_xplaneMirrors = new GdxLocalPlaneCoord[N_MIRRORS];   // runtime
        GdxLocalPlaneCoord[] m_xplaneQCs = new GdxLocalPlaneCoord[N_MIRRORS];       // standard
        double[] m_adjs = new double[N_MIRRORS];
        #endregion

        public GdxLaserCenterCompensator()
        {
            for (int i = 0; i < N_MIRRORS; i++)
            {
                m_xplaneMirrors[i] = new GdxLocalPlaneCoord();
                m_xplaneQCs[i] = new GdxLocalPlaneCoord();
            }
        }
        public void Dispose()
        {
            // RESERVED.
        }

        public void ResetLaserPtsOnMirror(int mirrorIdx)
        {
            if (mirrorIdx < N_MIRRORS)
            {
                _laserRunPts[mirrorIdx] = new List<QVector>();
                _compResult[mirrorIdx] = null;
                _isBuiltOk[mirrorIdx] = false;
            }
        }
        public void AddLaserPtOnMirror(int mirrorIdx, QVector pos)
        {
            if (mirrorIdx < N_MIRRORS && _laserRunPts[mirrorIdx] != null)
                _laserRunPts[mirrorIdx].Add(pos);
        }
        
        public void BuildMirrorPlaneTransform(int mirrorIdx)
        {
            if (mirrorIdx < N_MIRRORS)
            {
                BuildGoldenPlaneFormula();
                var xplane = m_xplaneMirrors[mirrorIdx];
                var pts = _laserRunPts[mirrorIdx];
                xplane.BuildTransform(pts);
                _isBuiltOk[mirrorIdx] = true;
                Save();
            }
        }
        public void BuildGoldenPlaneFormula()
        {
            m_xplaneGolden.BuildTransform(GdxGlobal.INI.GaugeBlockPlanePoints);
            expand_dimensions(GdxGlobal.INI.Mirror1.PlanePosList, 4);               // (XYZ) -> (XYZL)
            expand_dimensions(GdxGlobal.INI.Mirror2.PlanePosList, 4);               // (XYZ) -> (XYZL)            
            m_xplaneQCs[0].BuildTransform(GdxGlobal.INI.Mirror1.PlanePosList);      // 1-base-index
            m_xplaneQCs[1].BuildTransform(GdxGlobal.INI.Mirror2.PlanePosList);      // 1-base-index
            Load();
        }

        /// <summary>
        /// Coordinates (X,Y,Z)
        /// </summary>
        public QVector GetQCMotorPos(int mirrorIdx)
        {
            if (mirrorIdx < N_MIRRORS)
            {
                if (m_xplaneQCs[mirrorIdx] == null)
                    BuildGoldenPlaneFormula();
                var motorPos = m_xplaneQCs[mirrorIdx].FacadeCenter;
                return motorPos.Slice(0, 3);
            }
            return null;
        }
        public QVector GetLastRunSurfaceCenter(int mirrorIdx)
        {
            return m_xplaneMirrors[mirrorIdx].RealSurfaceCenter;
        }

        public string SetQCLaserMeasurement(int mirrorIdx, double qcLaserMeasurement)
        {
            if (!_isBuiltOk[mirrorIdx])
            {
                return "Combiner for Mirror " + mirrorIdx + " 尚未掃過三點雷射點位!";
            }
            var adj = calcGCAdjustment(mirrorIdx, qcLaserMeasurement);
            m_adjs[mirrorIdx] = adj;
            Save();
            return null;
        }
        private double calcGCAdjustment(int mirrorIdx, double qcLaserMeasurement)
        {
            if (mirrorIdx < N_MIRRORS)
            {
                if (m_xplaneQCs[mirrorIdx] == null)
                    BuildGoldenPlaneFormula();

                //double laserQC = mirrorIdx == 0 ?
                //    GdxGlobal.Facade.INI.Mirror1.GetQcLaserMeasuredDist():
                //    GdxGlobal.Facade.INI.Mirror2.GetQcLaserMeasuredDist();

                var plane = m_xplaneMirrors[mirrorIdx];
                double laserCalc = plane.Lmin + plane.LcDepth;

                double adj = qcLaserMeasurement - laserCalc;
                return adj;
            }
            return 0;
        }

        public bool CanCompensate(int mirrorIdx)
        {
            if (mirrorIdx < N_MIRRORS)
                return m_xplaneGolden.IsBuilt && m_xplaneMirrors[mirrorIdx].IsBuilt;
            return false;
        }
        public QVector CalcCompensation(int mirrorIdx, QVector currentMotorPosAx6)
        {
            BuildGoldenPlaneFormula();

            double cosFactor = 0.981;
            int N6 = currentMotorPosAx6.Dimensions;
            if (!CanCompensate(mirrorIdx) && mirrorIdx < N_MIRRORS)
                return new QVector(N6); // zero incr

            var xplane = m_xplaneMirrors[mirrorIdx];

            // pickerTouchPos: INI 分開設定 U軸, (INI.AttractPos 只有三軸)
            var pickerTouchPos = GdxGlobal.INI.AttractPos;
            var pickerTouchU = 0;   // GdxGlobal.INI.Offset_ModuleZ;

            // 三軸 to 四軸坐標系
            var pickerTouchPosAx4 = pickerTouchPos.Expand(4);
            pickerTouchPosAx4[3] = pickerTouchU;
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[0] == pickerTouchPos[0]);
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[1] == pickerTouchPos[1]);
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[2] == pickerTouchPos[2]);
            System.Diagnostics.Debug.Assert(pickerTouchPosAx4[3] == pickerTouchU);

            GdxGlobal.LOG.Trace("pickerTouchPosAx4, {0}", pickerTouchPosAx4);
            GdxGlobal.LOG.Trace("currentMotorPosAx6, {0}", currentMotorPosAx6);

            GdxGlobal.LOG.Trace("golden FacadeCenter, {0}", m_xplaneGolden.FacadeCenter);
            GdxGlobal.LOG.Trace("golden RealSurfaceCenter, {0}", m_xplaneGolden.RealSurfaceCenter);
            GdxGlobal.LOG.Trace("golden theta_y, {0:0.0000}", m_xplaneGolden.ThetaY);
            GdxGlobal.LOG.Trace("golden theta_z, {0:0.0000}", m_xplaneGolden.ThetaZ);

            GdxGlobal.LOG.Trace("conbiner, {0}, RealSurfaceCenter, {1}", mirrorIdx, xplane.RealSurfaceCenter);
            GdxGlobal.LOG.Trace("conbiner, {0}, FacadeCenter, {1}", mirrorIdx, xplane.FacadeCenter);
            GdxGlobal.LOG.Trace("conbiner, {0}, theta_y, {1:0.0000}", mirrorIdx, xplane.ThetaY);
            GdxGlobal.LOG.Trace("conbiner, {0}, theta_z, {1:0.0000}", mirrorIdx, xplane.ThetaZ);

            // Center Offsets
            var centerDiff = xplane.RealSurfaceCenter - m_xplaneGolden.RealSurfaceCenter;
            var targetAx4 = pickerTouchPosAx4 + centerDiff;

            // Angle Offsets
            var d_theta_y = m_xplaneGolden.ThetaY - xplane.ThetaY;
            var d_theta_z = m_xplaneGolden.ThetaZ - xplane.ThetaZ;

            // Laser and U
            double laserD = targetAx4[3];
            double currU = currentMotorPosAx6[3];
            double incrU = (laserD - currU) * 0.999;

            // 四軸 to 六軸坐標系
            var targetAx6 = targetAx4.Expand(N6);
            var incr = targetAx6 - currentMotorPosAx6;

            // Adjust U axis
            incr[0] = incr[1] = incr[2] = 0;
            incr[3] = incrU;
            incr[4] = d_theta_y;
            incr[5] = d_theta_z;

            // Simple L compensation
            double goldenL = m_xplaneGolden.Lmin + m_xplaneGolden.LcDepth;
            double combinerL = xplane.Lmin + xplane.LcDepth;
            double dL = combinerL - goldenL;

            double adj = m_adjs[mirrorIdx];
            double dU = (dL + adj) * cosFactor;

            GdxGlobal.LOG.Trace("mirror, {0}, dL, {1:0.000}, QC adj, {2:0.000}", mirrorIdx, dL, adj);

            incr = new QVector(currentMotorPosAx6.Dimensions);
            incr[3] = dU;

            _compResult[mirrorIdx] = new QVector(incr);

            return incr;
        }
        public QVector GetLastCompensation(int mirrorIdx)
        {
            if (mirrorIdx < N_MIRRORS)
                return _compResult[mirrorIdx];
            return null;
        }


        #region PRIVATE_FUNCTIONS
        void expand_dimensions(List<QVector> vectors, int Dims)
        {
            for (int i = 0; i < vectors.Count; i++)
            {
                vectors[i] = vectors[i].Expand(Dims);
            }
        }
        #endregion


        #region PRIVATE_FILE_FUNCTIONS
        void Save(string fileName = null)
        {
            if (fileName == null)
                fileName = getDefaultFileName();

            //GdxGlobal.INI.Mirror1.QcLaserAdj = m_adjs[0];
            //GdxGlobal.INI.Mirror2.QcLaserAdj = m_adjs[1];

            string jstr = JsonConvert.SerializeObject(m_adjs, Formatting.Indented);
            using (var stm = new System.IO.StreamWriter(fileName, false))
            {
                stm.Write(jstr);
                stm.Flush();
                stm.Close();
            }
        }
        void Load(string fileName = null)
        {
            if (fileName == null)
                fileName = getDefaultFileName();

            //m_adjs[0] = GdxGlobal.INI.Mirror1.QcLaserAdj;                           // 1-base-index
            //m_adjs[1] = GdxGlobal.INI.Mirror2.QcLaserAdj;                           // 1-base-index

            List<double> adjs = new List<double>();

            try
            {
                using (var stm = new System.IO.StreamReader(fileName))
                {
                    string jstr = stm.ReadToEnd();
                    adjs = JsonConvert.DeserializeObject<List<double>>(jstr);
                    stm.Close();
                }
            }
            catch
            {
            }

            if (adjs == null)
            {
                adjs = new List<double>();
            }

            while (adjs.Count < N_MIRRORS)
            {
                adjs.Add(0.0);
            }

            m_adjs = adjs.ToArray();
        }
        string getDefaultFileName()
        {
            string path = System.IO.Path.GetTempPath();
            return System.IO.Path.Combine(path, GetType().Name + ".json");
        }
        #endregion
    }
}
