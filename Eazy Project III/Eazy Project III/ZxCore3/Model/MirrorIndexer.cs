using JetEazy.QMath;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace JetEazy.GdxCore3.Model
{
    class MirrorIndexer
    {        
        public MirrorIndexer(int groupIdx, int mirrorIdx, int pointIdx = 0)
        {
            //_code = ((uint)groupIdx & 0xFF);
            //_code <<= 8;
            //_code |= ((uint)mirrorIdx & 0xFF);
            //_code <<= 16;
            //_code |= ((uint)pointIdx & 0xFFFF);
            GroupID = groupIdx;
            MirrorID = mirrorIdx;
            PointID = pointIdx;
        }
        public MirrorIndexer()
        {
        }
        public int GroupID
        {
            get; 
            set;
        }
        public int MirrorID
        {
            get;
            set;
        }
        public int PointID
        {
            get;
            set;
        }
        public override string ToString()
        {
            return string.Format("{0}:{1}#{2}", GroupID, MirrorID, PointID);
            //return null;
        }
    }

}
