using System;
using System.Drawing;



namespace JetEazy.GdxCore3
{
    /// <summary>
    /// 中光電補償運算之總合結果 (含有許多圖標)
    /// </summary>
    public class CoreCompInfo
    {
        public int CompType;           //0: 綠,  //1: 紅
        public CoreCompRect[] Rects;
        public Point[] GoldenPts;

        public CoreCompInfo(int compType, params CoreCompRect[] rects)
        {
            CompType = compType;
            Rects = new CoreCompRect[rects.Length];
            Array.Copy(rects, Rects, Rects.Length);
        }
        public CoreCompInfo(int compType, Point[] goldenPts, params CoreCompRect[] rects):
            this(compType, rects)
        {
            if (goldenPts != null)
            {
                GoldenPts = new Point[goldenPts.Length];
                Array.Copy(goldenPts, GoldenPts, GoldenPts.Length);
            }
        }
    }


    /// <summary>
    /// 中光電補償運算之圖標 (Rect2D)
    /// </summary>
    public class CoreCompRect
    {
        public PointF Center;
        public SizeF Size;
        public float Angle;

        public CoreCompRect(float x, float y, float width, float height, float angle = 0)
        {
            Center = new PointF(x, y);
            Size = new SizeF(width, height);
            Angle = angle;
        }
        public CoreCompRect(float[] info, int startIdx = 0)
        {
            Center = new PointF(info[startIdx + 0], info[startIdx + 1]);
            Angle = info[startIdx + 2];
            Size = new SizeF(info[startIdx + 3], info[startIdx + 4]);
        }
    }
}
