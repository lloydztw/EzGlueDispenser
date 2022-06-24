using JetEazy.QMath;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace JetEazy.GdxCore3.Model
{
    class MirrorIndexer
    {
        private uint _code;
        public MirrorIndexer(int groupIdx, int mirrorIdx, int pointIdx = 0)
        {
            _code = ((uint)groupIdx & 0xFF);
            _code <<= 8;
            _code |= ((uint)mirrorIdx & 0xFF);
            _code <<= 16;
            _code |= ((uint)pointIdx & 0xFFFF);
        }
        public int GroupID
        {
            get;
            private set;
        }
        public int MirrorID
        {
            get;
            private set;
        }
        public int PointID
        {
            get;
            private set;
        }
        public override string ToString()
        {
            //return string.Format("{0}_{1}_{2}", GroupIndex, MirrorIndex, PointIndex);
            return null;
        }
    }

}
