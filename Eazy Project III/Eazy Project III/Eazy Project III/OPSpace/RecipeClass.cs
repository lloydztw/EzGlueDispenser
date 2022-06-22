using Eazy_Project_Interface;
using JetEazy;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WorldOfMoveableObjects;

namespace Eazy_Project_III.OPSpace
{
    

    public class RecipeBaseClass
    {
        private string m_path = "";
        private int m_index = 0;
        private string m_format = "000000";

        public string Path
        {
            get { return m_path; }
            //set { m_path = value; }
        }
        public int Index
        {
            get { return m_index; }
            //set { m_index = value; }
        }

        #region INI Access Functions
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        public string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";

            StringBuilder temp = new StringBuilder(200);
            int Length = GetPrivateProfileString(section, key, "", temp, 200, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            //else
            //    retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;
        }
        #endregion
       
        public string INIFILE = "";

        public virtual void Initial(string epath, int eindex)
        {
            m_path = epath;
            //m_index = eindex;

            //INIFILE = m_path + "\\rcp_" + m_index.ToString(m_format) + ".ini";
            //Load();

            ChangeIndex(eindex);
        }
        public virtual void ChangeIndex(int eindex)
        {
            
            m_index = eindex;

            INIFILE = m_path + "\\rcp_" + m_index.ToString(m_format) + ".ini";
            Load();
        }

        public virtual void Load()
        {
           
        }
        public virtual void Save()
        {
        }

        public string PointFToString(PointF PTF)
        {
            return PTF.X.ToString("0.000") + "," + PTF.Y.ToString("0.000");
        }
        public PointF StringToPointF(string Str)
        {
            string[] strs = Str.Split(',');
            return new PointF(float.Parse(strs[0]), float.Parse(strs[1]));
        }

        public string RecttoStringSimple(Rectangle Rect)
        {
            return Rect.X.ToString() + "," + Rect.Y.ToString() + "," + Rect.Width.ToString() + "," + Rect.Height.ToString();
        }
        public Rectangle StringtoRect(string RectStr)
        {
            string[] str = RectStr.Split(',');
            return new Rectangle(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]), int.Parse(str[3]));
        }
        public string RectFtoStringSimple(RectangleF RectF)
        {
            string Str = "";

            Str += RectF.X.ToString() + ",";
            Str += RectF.Y.ToString() + ",";
            Str += RectF.Width.ToString() + ",";
            Str += RectF.Height.ToString();

            return Str;
        }
        public RectangleF StringtoRectF(string RectStr)
        {
            string[] strs = RectStr.Split(',');
            RectangleF rectF = new RectangleF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);
            rectF.Width = float.Parse(strs[2]);
            rectF.Height = float.Parse(strs[3]);

            return rectF;


        }

    }

    public class RecipeCHClass : RecipeBaseClass, IRecipe
    {
        private static readonly RecipeCHClass _instance = new RecipeCHClass();
        public static RecipeCHClass Instance
        {
            get
            {
                return _instance;
            }
        }

        const string Cat0 = "00.中心校正相机参数";

        [CategoryAttribute(Cat0), DescriptionAttribute("中心校正像機的曝光值")]
        [DisplayName("曝光")]
        public int CaliCamExpo { get; set; } = 5000;
        [CategoryAttribute(Cat0), DescriptionAttribute("中心校正像機的偏移校正數值")]
        [DisplayName("偏移校正數值")]
        [Browsable(false)]
        public string CaliCamCaliData { get; set; } = string.Empty;
        [CategoryAttribute(Cat0), DescriptionAttribute("預設圖像校正中間")]
        [DisplayName("中心点")]
        public PointF CaliPicCenter { get; set; } = new PointF(0, 0);

        [Browsable(false)]
        public System.Drawing.Bitmap bmpCaliOrg { get; set; } = new Bitmap(1, 1);
        [Browsable(false)]
        public RectangleF rectCali { get; set; } = new RectangleF(0, 0, 100, 100);
        //[Browsable(false)]
        //public double rectAngle { get; set; } = 0;
        [Browsable(false)]
        public string myMoverStr = string.Empty;


        #region MOVER

        public Mover myMover = new Mover();

        [Browsable(false)]
        public int No { get; set; } = 0;
        [Browsable(false)]
        public int Level { get; set; } = 2;

        Color DefaultColor = Color.FromArgb(0, Color.Red);
        Color DefaultRingColor = Color.FromArgb(60, Color.Red);
        public static char SeperateCharB = '\x1b';

        /// <summary>
        /// 設定Mover 裏的相關資料
        /// </summary>
        /// <param name="relateno"></param>
        /// <param name="relateposition"></param>
        public void RelateMover(int relateno, int relatelevel)
        {
            int i = 0;

            while (i < myMover.Count)
            {
                GraphicalObject grpobj = myMover[i].Source;

                (grpobj as GeoFigure).RelateNo = relateno;
                (grpobj as GeoFigure).RelatePosition = i;
                (grpobj as GeoFigure).RelateLevel = relatelevel;

                i++;
            }
        }

        void SetMoverOffset(Point offsetpoint)
        {
            int i = 0;

            while (i < myMover.Count)
            {
                GraphicalObject grpobj = myMover[i].Source;

                (grpobj as GeoFigure).SetOffset(offsetpoint);

                i++;
            }
        }
        void SetMoverAngle(double adddegree)
        {
            int i = 0;

            while (i < myMover.Count)
            {
                GraphicalObject grpobj = myMover[i].Source;

                (grpobj as GeoFigure).SetAngle(adddegree);

                i++;
            }
        }

        public void SetMoverDefault()
        {
            myMover.Clear();

            JzRectEAG jzrect = new JzRectEAG(Color.FromArgb(0, Color.Red));
            jzrect.RelateNo = No;
            jzrect.RelatePosition = 0;
            jzrect.RelateLevel = Level;

            myMover.Add(jzrect);

        }

        void SetDefaultColor(int level)
        {
            switch ((level - 1) % 7)
            {
                case 0:
                    DefaultColor = Color.FromArgb(0, Color.Red);
                    break;
                case 1:
                    DefaultColor = Color.FromArgb(0, Color.Lime);
                    break;
                case 2:
                    DefaultColor = Color.FromArgb(0, Color.DarkBlue);
                    break;
                case 3:
                    DefaultColor = Color.FromArgb(0, Color.Yellow);
                    break;
                case 4:
                    DefaultColor = Color.FromArgb(0, Color.SkyBlue);
                    break;
                case 5:
                    DefaultColor = Color.FromArgb(0, Color.Orange);
                    break;
                case 6:
                    DefaultColor = Color.FromArgb(0, Color.Purple);
                    break;
            }
        }

        string ToMoverString()
        {
            string retstr = "";
            char seperator = SeperateCharB;

            GraphicalObject grobj;

            for (int i = 0; i < myMover.Count; i++)
            {
                grobj = myMover[i].Source;

                if (grobj is JzRectEAG)
                {
                    retstr += (grobj as JzRectEAG).ToString() + seperator;
                }
                else if (grobj is JzCircleEAG)
                {
                    retstr += (grobj as JzCircleEAG).ToString() + seperator;
                }
                else if (grobj is JzPolyEAG)
                {
                    retstr += (grobj as JzPolyEAG).ToString() + seperator;
                }
                else if (grobj is JzRingEAG)
                {
                    retstr += (grobj as JzRingEAG).ToString() + seperator;
                }
                else if (grobj is JzStripEAG)
                {
                    retstr += (grobj as JzStripEAG).ToString() + seperator;
                }
                else if (grobj is JzIdentityHoleEAG)
                {
                    retstr += (grobj as JzIdentityHoleEAG).ToString() + seperator;
                }
                else if (grobj is JzCircleHoleEAG)
                {
                    retstr += (grobj as JzCircleHoleEAG).ToString() + seperator;
                }
            }
            if (retstr != "")
                retstr = retstr.Substring(0, retstr.Length - 1);

            return retstr;
        }
        void FromMoverString(string fromstr)
        {
            int i = 0;
            char seperator = SeperateCharB;
            string[] strs = fromstr.Split(seperator);

            SetDefaultColor(Level);

            foreach (string str in strs)
            {
                if (str.IndexOf(Figure_EAG.Rectangle.ToString()) > -1)
                {
                    JzRectEAG jzrect = new JzRectEAG(str, DefaultColor);

                    jzrect.RelateNo = No;
                    jzrect.RelatePosition = i;
                    jzrect.RelateLevel = Level;

                    myMover.Add(jzrect);
                }
                else if (str.IndexOf(Figure_EAG.Circle.ToString()) > -1)
                {
                    JzCircleEAG jzcircle = new JzCircleEAG(str, DefaultColor);

                    jzcircle.RelateNo = No;
                    jzcircle.RelatePosition = i;
                    jzcircle.RelateLevel = Level;

                    myMover.Add(jzcircle);
                }
                else if (str.IndexOf(Figure_EAG.ChatoyantPolygon.ToString()) > -1)
                {
                    JzPolyEAG jzpoly = new JzPolyEAG(str, DefaultColor);

                    jzpoly.RelateNo = No;
                    jzpoly.RelatePosition = i;
                    jzpoly.RelateLevel = Level;

                    myMover.Add(jzpoly);
                }
                else if (str.IndexOf(Figure_EAG.Ring.ToString()) > -1 || str.IndexOf(Figure_EAG.ORing.ToString()) > -1)
                {
                    JzRingEAG jzring = new JzRingEAG(str, DefaultRingColor);

                    jzring.RelateNo = No;
                    jzring.RelatePosition = i;
                    jzring.RelateLevel = Level;

                    myMover.Add(jzring);
                }
                else if (str.IndexOf(Figure_EAG.Strip.ToString()) > -1)
                {
                    JzStripEAG jzstrip = new JzStripEAG(str, DefaultColor);

                    jzstrip.RelateNo = No;
                    jzstrip.RelatePosition = i;
                    jzstrip.RelateLevel = Level;

                    myMover.Add(jzstrip);
                }
                else if (str.IndexOf(Figure_EAG.RectRect.ToString()) > -1 || str.IndexOf(Figure_EAG.HexHex.ToString()) > -1)
                {
                    JzIdentityHoleEAG jzidentityhole = new JzIdentityHoleEAG(str, DefaultColor);

                    jzidentityhole.RelateNo = No;
                    jzidentityhole.RelatePosition = i;
                    jzidentityhole.RelateLevel = Level;

                    myMover.Add(jzidentityhole);
                }
                else if (str.IndexOf(Figure_EAG.RectO.ToString()) > -1 || str.IndexOf(Figure_EAG.HexO.ToString()) > -1)
                {
                    JzCircleHoleEAG jzcirclehole = new JzCircleHoleEAG(str, DefaultColor);

                    jzcirclehole.RelateNo = No;
                    jzcirclehole.RelatePosition = i;
                    jzcirclehole.RelateLevel = Level;

                    myMover.Add(jzcirclehole);
                }

                i++;
            }
        }

        #endregion


        const string Cat1 = "01.判斷相機參數";
        [CategoryAttribute(Cat1), DescriptionAttribute("判斷像機的曝光值")]
        [DisplayName("曝光")]
        public int JudgeCamExpo { get; set; } = 5000;
        [CategoryAttribute(Cat1), DescriptionAttribute("判斷像機的偏移校正數值")]
        [DisplayName("偏移校正位置")]
        [Browsable(false)]
        public int JudgeCamCaliData { get; set; } = 5;
        [CategoryAttribute(Cat1), DescriptionAttribute("預設角度偏移值")]
        [DisplayName("偏移值")]
        [Browsable(false)]
        public string JudgeThetaOffset { get; set; } = string.Empty;


        const string Cat2 = "02.點膠參數";
        [CategoryAttribute(Cat2), DescriptionAttribute("點膠時間(ms)")]
        [DisplayName("點膠時間")]
        public int DispensingTime { get; set; } = 5000;
        [CategoryAttribute(Cat2), DescriptionAttribute("UV 光照時間(sec)")]
        [DisplayName("UV时间")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000)]
        public int UVTime { get; set; } = 5;


        const string Cat3 = "03.其他参数";
        [CategoryAttribute(Cat3), DescriptionAttribute("其他有關於調整的參數，可以先以 XXX,YYY,AAA,BBB 先寫在這個字串中，之後再分開。")]
        public string OtherRecipe { get; set; } = string.Empty;



        public override void Load()
        {
            CaliCamExpo = int.Parse(ReadINIValue("Recipe Basic", "CaliCamExpo", CaliCamExpo.ToString(), INIFILE));
            CaliCamCaliData = ReadINIValue("Recipe Basic", "CaliCamCaliData", CaliCamCaliData.ToString(), INIFILE);
            CaliPicCenter = StringToPointF(ReadINIValue("Recipe Basic", "CaliPicCenter", PointFToString(CaliPicCenter), INIFILE));
            JudgeCamExpo = int.Parse(ReadINIValue("Recipe Basic", "JudgeCamExpo", JudgeCamExpo.ToString(), INIFILE));
            JudgeCamCaliData = int.Parse(ReadINIValue("Recipe Basic", "JudgeCamCaliData", JudgeCamCaliData.ToString(), INIFILE));
            JudgeThetaOffset = ReadINIValue("Recipe Basic", "JudgeThetaOffset", JudgeThetaOffset.ToString(), INIFILE);
            DispensingTime = int.Parse(ReadINIValue("Recipe Basic", "DispensingTime", DispensingTime.ToString(), INIFILE));
            UVTime = int.Parse(ReadINIValue("Recipe Basic", "UVTime", UVTime.ToString(), INIFILE));
            OtherRecipe = ReadINIValue("Recipe Basic", "OtherRecipe", OtherRecipe.ToString(), INIFILE);
            rectCali = StringtoRectF(ReadINIValue("Recipe Basic", "rectCali", RectFtoStringSimple(rectCali), INIFILE));
            myMoverStr = ReadINIValue("Recipe Basic", "myMoverStr", myMoverStr.ToString(), INIFILE);
            if (string.IsNullOrEmpty(myMoverStr))
            {
                SetMoverDefault();
                myMoverStr = ToMoverString();
            }
            myMover.Clear();
            FromMoverString(myMoverStr);

            string caliOrgStrPath = INIFILE.Replace(".ini", "caliOrg.bmp");
            if (System.IO.File.Exists(caliOrgStrPath))
            {
                Bitmap bmp = new Bitmap(caliOrgStrPath);
                bmpCaliOrg.Dispose();
                bmpCaliOrg = new Bitmap(bmp);
                bmp.Dispose();
            }

        }
        public override void Save()
        {
            WriteINIValue("Recipe Basic", "CaliCamExpo", CaliCamExpo.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "CaliCamCaliData", CaliCamCaliData.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "CaliPicCenter", PointFToString(CaliPicCenter), INIFILE);
            WriteINIValue("Recipe Basic", "JudgeCamExpo", JudgeCamExpo.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "JudgeCamCaliData", JudgeCamCaliData.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "JudgeThetaOffset", JudgeThetaOffset.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "DispensingTime", DispensingTime.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "UVTime", UVTime.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "OtherRecipe", OtherRecipe.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "rectCali", RectFtoStringSimple(rectCali), INIFILE);
            myMoverStr = ToMoverString();
            WriteINIValue("Recipe Basic", "myMoverStr", myMoverStr.ToString(), INIFILE);

            string caliOrgStrPath = INIFILE.Replace(".ini", "caliOrg.bmp");
            bmpCaliOrg.Save(caliOrgStrPath, System.Drawing.Imaging.ImageFormat.Bmp);


        }

       

    }
}
