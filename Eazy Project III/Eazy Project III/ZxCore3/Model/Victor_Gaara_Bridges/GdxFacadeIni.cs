using Eazy_Project_III;
using JetEazy.QMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetEazy.GdxCore3.Model
{
    class GdxFacadeIni : GdxIniParser
    {
        #region SINGLETON
        static GdxFacadeIni _singleton = null;
        List<QVector> _gaugeBlockPlanePoses = null;
        protected GdxFacadeIni()
        {
            Mirror0 = new GdxMirror0();
            Mirror1 = new GdxMirror(1);
            Mirror2 = new GdxMirror(2);
            Sync(false);
        }
        #endregion

        public static GdxFacadeIni Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new GdxFacadeIni();
                }
                return _singleton;
            }
        }
        public void Sync(bool all = false, bool reload = false)
        {
            if (reload)
                ini.Load();

            // Laser
            LEPos = Parse(ini.LEPos);
            AttractPos = Parse(ini.AttractPos);
            Offset_LEAtract = Math.Round(ini.Offset_LEAttract, 4);
            Offset_ModuleZ = Math.Round(ini.Offset_ModuleZ, 4);

            // Mirrors
            if (all)
            {
                Mirror0.Sync();
                Mirror1.Sync();
                Mirror2.Sync();
            }

            // Shadow
            ShadowPos = Parse(ini.ShadowPos);
            ShadowPosUp = Parse(ini.ShadowPosUp);
        }

        public QVector LEPos;
        public QVector AttractPos;
        public double Offset_LEAtract;
        public double Offset_ModuleZ;

        public GdxMirror0 Mirror0;
        public GdxMirror Mirror1;
        public GdxMirror Mirror2;

        public QVector ShadowPos;
        public QVector ShadowPosUp;

        /// <summary>
        /// 塊規 laser 量測 motors 點位
        /// X,Y,Z,L
        /// </summary>
        public List<QVector> GaugeBlockPlanePoses
        {
            get
            {
                if (_gaugeBlockPlanePoses == null)
                    _gaugeBlockPlanePoses = CombineGaugeBlockPlanePoses();
                return _gaugeBlockPlanePoses;
            }
        }

        /// <summary>
        /// Plane poses on Laser Gauge Block
        /// List< (X,Y,Z,L) >
        /// </summary>
        /// <returns></returns>
        private List<QVector> CombineGaugeBlockPlanePoses()
        {
            var yzl = Mirror0.PlaneHeightPosList;
            var xyz = Mirror0.PlanePosList;
            var xyzl = new List<QVector>();
            System.Diagnostics.Debug.Assert(yzl.Count == xyz.Count);
            for (int i = 0; i < yzl.Count; i++)
            {
                var v1 = xyz[i];
                var v2 = yzl[i];
                System.Diagnostics.Debug.Assert(QVector.AreEqual(v1.Slice(1, 2), v2.Slice(0, 2)));
                var vL = new QVector(v1.X, v1.Y, v1.Z, v2[2]);
                xyzl.Add(vL);
            }
            _gaugeBlockPlanePoses = xyzl;
            return xyzl;
        }
    }



    class GdxMirror0 : GdxIniParser
    {
        public QVector CaliPos;
        public List<QVector> PlaneHeightPosList;
        public List<QVector> PlanePosList;

        public GdxMirror0()
        {
            Sync();
        }
        public void Sync()
        {
            var Mirror0 = this;
            Mirror0.CaliPos = Parse(ini.Mirror0CaliPos);
            Mirror0.PlaneHeightPosList = Parse(ini.Mirror0PlaneHeightPosList);
            Mirror0.PlanePosList = Parse(ini.Mirror0PlanePosList);
        }
    }


    class GdxMirror : GdxIniParser
    {
        #region PRIVATE_DATA
        int _index;
        #endregion

        public QVector CaliPos;
        public List<QVector> JamedPosList;
        public List<QVector> PlanePosList;
        public List<QVector> PosList;
        public QVector PutPos;
        public List<QVector> UVPosList;
        public double _Offset_Adj;
        public int AdjBackLength;
        public int AdjDeepLength;
        public int PutAdjDeepLength;

        public GdxMirror(int index)
        {
            System.Diagnostics.Trace.Assert(index == 1 || index == 2);
            _index = index;
            Sync();
        }
        public void Sync()
        {
            if(_index==1)
            {
                var Mirror1 = this;
                Mirror1.CaliPos = Parse(ini.Mirror1CaliPos);
                Mirror1.JamedPosList = Parse(ini.Mirror1JamedPosList);
                Mirror1.PlanePosList = Parse(ini.Mirror1PlanePosList);
                Mirror1.PosList = Parse(ini.Mirror1PosList);
                Mirror1.PutPos = Parse(ini.Mirror1PutPos);
                Mirror1.UVPosList = Parse(ini.Mirror1UVPosList);
                Mirror1._Offset_Adj = ini.Mirror1_Offset_Adj;
                //
                Mirror1.PutAdjDeepLength = ini.sMirrorPutAdjDeep1Length;
                Mirror1.AdjDeepLength = ini.sMirrorAdjDeep1Length;
                Mirror1.AdjBackLength = ini.sMirrorAdjBackLength;
            }
            else
            {
                var Mirror2 = this;
                Mirror2.CaliPos = Parse(ini.Mirror2CaliPos);
                Mirror2.JamedPosList = Parse(ini.Mirror2JamedPosList);
                Mirror2.PlanePosList = Parse(ini.Mirror2PlanePosList);
                Mirror2.PosList = Parse(ini.Mirror2PosList);
                Mirror2.PutPos = Parse(ini.Mirror2PutPos);
                Mirror2.UVPosList = Parse(ini.Mirror2UVPosList);
                Mirror2._Offset_Adj = ini.Mirror2_Offset_Adj;
                //
                Mirror2.PutAdjDeepLength = ini.sMirrorPutAdjDeep2Length;
                Mirror2.AdjDeepLength = ini.sMirrorAdjDeep2Length;
                Mirror2.AdjBackLength = ini.sMirrorAdjBackLength;
            }
        }
    }


    class GdxIniParser
    { 
        public static List<QVector> Parse(List<string> strs)
        {
            var vectors = new List<QVector>();
            foreach(var s in strs)
            {
                vectors.Add(QVector.Parse(s));
            }
            return vectors;
        }
        public static QVector Parse(string str)
        {
            return QVector.Parse(str);
        }
        protected static INI ini
        {
            get { return INI.Instance; }
        }
    }
}
