using JetEazy.GdxCore3;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.Drawing;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 補償執行中的事件參數 (for 單步調試 使用)
    /// </summary>
    public class CompensatingEventArgs : ProcessEventArgs
    {
        public string PhaseName;
        public QVector FinalTarget;
        public QVector InitPos;
        public QVector CurrentPos;
        public QVector Delta;
        public QVector MaxDelta;
        public bool ContinueToDebug;
        public int[] ShowIDs = null;
    }

    /// <summary>
    /// 補償運算後事件參數 
    /// </summary>
    public class CompensatedInfoEventArgs : ProcessEventArgs
    {
        /// <summary>
        /// CenterComp or ProjectionComp <br/>
        /// (02中心補償 或 03光斑補償)
        /// </summary>
        public string Name;
        /// <summary>
        /// MirrorIndex
        /// </summary>
        public int MirrorIndex;
        /// <summary>
        /// Sender 負責 life cycle
        /// </summary>
        public Bitmap Image
        {
            get { return (Bitmap)Tag; }
            set { Tag = value; }
        }
        /// <summary>
        /// 中光電補償運算之總合結果 (含有許多圖標)
        /// </summary>
        public CoreCompInfo Info;

        public CompensatedInfoEventArgs(string name, int mirrorIndex, Bitmap bmp, CoreCompInfo info)
        {
            this.Name = name;
            this.MirrorIndex = mirrorIndex;
            this.Image = bmp;
            //this.Rects = new GdxCoreRect[rects.Length];
            //Array.Copy(rects, this.Rects, rects.Length);
            this.Info = info;
        }
    }

    /// <summary>
    /// 定位點設定 之 事件參數
    /// </summary>
    public class CoreMarkPointEventArgs : ProcessEventArgs
    {
        public string Name;
        /// <summary>
        /// Sender 負責 life cycle
        /// </summary>
        public Bitmap Image
        {
            get { return (Bitmap)Tag; }
            set { Tag = value; }
        }
        public Point[] GoldenPts;
        public Point[] AlgoPts;

        public CoreMarkPointEventArgs(string name, Bitmap bmp, Point[] goldenPts, Point[] algoPts)
        {
            Name = name;
            Image = bmp;
            GoldenPts = new Point[goldenPts.Length];
            Array.Copy(goldenPts, GoldenPts, GoldenPts.Length);
            AlgoPts = new Point[algoPts.Length];
            Array.Copy(algoPts, AlgoPts, AlgoPts.Length);
        }
    }
}
