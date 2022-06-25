﻿using JetEazy.BasicSpace;
using JetEazy.QMath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JetEazy.GdxCore3.Model
{
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
            var sum = new QVector(4);
            for (int i = 0; i < N; i++)
            {
                var pos = pointsXYZL[i];
                lmin = Math.Min(Lmin, pos[3]);
                sum += pos;
            }
            FacadeCenter = sum / N;
            FacadeCenter[3] = lmin;
            double Xc = FacadeCenter[0];
            double Yc = FacadeCenter[1];
            double Zc = FacadeCenter[2];
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
            PlaneNormalVector = normalV;
            //-----------------------------------------------------------------------------
            QVector.Percision = old_percision;
            //-----------------------------------------------------------------------------

            //(4) LcDepth
            LcDepth = plane.GetZLocation(new QPoint3D(0, 0, 0));

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


    public class GdxLaserCenterCompensator : IDisposable
    {
        #region PRIVATE_DATA
        /// <summary>
        /// (X,Y,Z,L)
        /// </summary>
        List<QVector>[] _runPtsCollection = new List<QVector>[] { 
            new List<QVector>(),
            new List<QVector>(),
        };
        #endregion

        #region PUBLIC_DATA
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
            _runPtsCollection[mirrorIdx].Clear();
        }
        public void AddLaserPtOnMirror(int mirrorIdx, QVector pos)
        {
            _runPtsCollection[mirrorIdx].Add(pos);
        }
        public bool BuildMirrorPlaneTransform(int mirrorIdx)
        {
            var xplane = mirrorIdx == 0 ? m_xplaneMirror0 : m_xplaneMirror1;
            xplane.BuildTransform(_runPtsCollection[mirrorIdx]);
            Save();
            return false;
        }
        public bool BuildGoldenPlaneFormula()
        {
            //(0) Golden Source List<(X, Y, Z, Laser)>
            m_xplaneGolden.BuildTransform(GdxGlobal.INI.GaugeBlockPlanePoses);

            //(1) Motor Offset Vector (X, Y, Z)
            //var laserTouchPos = GdxGlobal.INI.LEPos;
            //var suckerTouchPos = GdxGlobal.INI.AttractPos;
            //var OffsetSL = suckerTouchPos - laserTouchPos;
            // 6-Axis Motor Offset Vector
            //m_xplaneGolden.MotorOffset = new QVector(6);
            //m_xplaneGolden.MotorOffset[0] = offset[0];
            //m_xplaneGolden.MotorOffset[1] = offset[1];
            //m_xplaneGolden.MotorOffset[2] = offset[2];
            return true;
        }
        
        public bool CanCompensate(int mirrorIdx)
        {
            if (mirrorIdx == 0)
                return m_xplaneGolden.IsBuilt && m_xplaneMirror0.IsBuilt;
            else
                return m_xplaneGolden.IsBuilt && m_xplaneMirror1.IsBuilt;
        }
        public QVector Compensate(int mirrorIndex, QVector currentXYZU)
        {
            if (!CanCompensate(mirrorIndex))
                return new QVector(currentXYZU.Dimensions); // no increment

            var xplane = (mirrorIndex == 0) ? m_xplaneMirror0 : m_xplaneMirror1;

            // pickerTouchPos 沒有設定 U
            var pickerTouchPos = GdxGlobal.INI.AttractPos;
            var pickerTouchPosU = new QVector(4);
            pickerTouchPosU[0] = pickerTouchPos[0];
            pickerTouchPosU[1] = pickerTouchPos[1];
            pickerTouchPosU[2] = pickerTouchPos[2];
            pickerTouchPosU[3] = 0;

            var centerDiff = xplane.RealSurfaceCenter - m_xplaneGolden.RealSurfaceCenter;
            var target = pickerTouchPosU + centerDiff;
            var targetLD = target[3];
            var currU = currentXYZU[3];
            var incrU = (targetLD - currU) * 0.999;
            var incr = target - currentXYZU;
            incr[3] = incrU;
            return incr;
        }

        public void Save(string fileName = null)
        {
            if (fileName == null)
                fileName = getDefaultFileName();

            string jstr = JsonConvert.SerializeObject(m_xplaneGolden);
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

        }
        string getDefaultFileName()
        {
            return @"D:\EVENTLOG\Nlogs\" + GetType().Name + ".json";
        }
    }
}
