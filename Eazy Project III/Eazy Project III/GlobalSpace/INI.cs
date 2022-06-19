using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

//using JetEazy;
using JetEazy.BasicSpace;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Eazy_Project_III.FormSpace;
//using Eazy_Project_III;

namespace Eazy_Project_III
{

    public class GetPositionPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext pContext)
        {
            if (pContext != null && pContext.Instance != null)
            {
                //以「...」按鈕的方式顯示
                //UITypeEditorEditStyle.DropDown    下拉選單
                //UITypeEditorEditStyle.None        預設的輸入欄位
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(pContext);
        }
        public override object EditValue(ITypeDescriptorContext pContext, IServiceProvider pProvider, object pValue)
        {
            IWindowsFormsEditorService editorService = null;
            if (pContext != null && pContext.Instance != null && pProvider != null)
            {
                editorService = (IWindowsFormsEditorService)pProvider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    //將顯示得視窗放在這邊，並透過ShowDialog方式來呼叫
                    //取得到的值再回傳回去
                    //MessageBox.Show("sfsf");
                    frmDataGridViewPosition msrdataform = new frmDataGridViewPosition(pContext.PropertyDescriptor.DisplayName,
                        pContext.PropertyDescriptor.Name,(string)pValue);
                    //msrdataform.Show();
                    if (msrdataform.ShowDialog() == DialogResult.OK)
                    {
                        pValue = JzToolsClass.PassingString;
                    }

                    //pValue = "FUCK YOU!";
                }
            }
            return pValue;
        }
    }

    public class GetFilePathPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext pContext)
        {
            if (pContext != null && pContext.Instance != null)
            {
                //以「...」按鈕的方式顯示
                //UITypeEditorEditStyle.DropDown    下拉選單
                //UITypeEditorEditStyle.None        預設的輸入欄位
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(pContext);
        }
        public override object EditValue(ITypeDescriptorContext pContext, IServiceProvider pProvider, object pValue)
        {
            IWindowsFormsEditorService editorService = null;
            if (pContext != null && pContext.Instance != null && pProvider != null)
            {
                editorService = (IWindowsFormsEditorService)pProvider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {

                    OpenFileDialog fileDlg = new OpenFileDialog();

                    switch(pContext.PropertyDescriptor.Name)
                    {
                        case "CfgPath":
                            fileDlg.Filter = "控制器配置文件|*.dcfg";
                            fileDlg.Title = "请选择匹配的控制器配置文件";
                            break;
                        case "HWCPath":
                            fileDlg.Filter = "控制器位移校准文件|*.hwc";
                            fileDlg.Title = "请选择匹配的控制器位移校准文件";
                            break;
                        default:
                            break;
                    }
                   
                    if (fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        pValue = fileDlg.FileName;
                    }
                }
            }
            return pValue;
        }
    }

    public class INI
    {
        private static readonly INI _instance = new INI();
        public static INI Instance
        {
            get
            {
                return _instance;
            }
        }

        #region INI Access Functions
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
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

        string MAINPATH = "";
        string INIFILE = "";

        //static JzToolsClass JzTools = new JzToolsClass();

        public int LANGUAGE = 0;

        /// <summary>
        /// 第一站顯示的項目
        /// </summary>
        const bool X1Visable = false;
        /// <summary>
        /// 第二站顯示的項目
        /// </summary>
        const bool X2Visable = false;
        /// <summary>
        /// 第三站顯示的項目
        /// </summary>
        const bool X3Visable = true;
        /// <summary>
        /// 第四站顯示的項目
        /// </summary>
        const bool X4Visable = false;


        #region 第一站 INI

        const string X1Cat0 = "A00.設置";

        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("點膠位置1集合")]
        [Browsable(X1Visable)]
        /// <summary>
        /// 點膠1取得位置(可擴充多個)
        /// </summary>
        public string sDispensingX1_1PosList { get; set; } = string.Empty;
        public List<string> DispensingX1_1PosList = new List<string>();


        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("點膠位置2集合")]
        [Browsable(X1Visable)]
        /// <summary>
        /// 點膠2取得位置(可擴充多個)
        /// </summary>
        public string sDispensingX1_2PosList { get; set; } = string.Empty;
        public List<string> DispensingX1_2PosList = new List<string>();

        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("取料位置1")]
        [Browsable(X1Visable)]
        /// <summary>
        /// 取料位置1
        /// </summary>
        public double GetPos1 { get; set; } = 0;
        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("取料位置2")]
        [Browsable(X1Visable)]
        /// <summary>
        /// 取料位置2
        /// </summary>
        public double GetPos2 { get; set; } = 0;

        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("放料位置1")]
        [Browsable(X1Visable)]
        /// <summary>
        /// 放料位置1
        /// </summary>
        public double PutPos1 { get; set; } = 0;
        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("放料位置2")]
        [Browsable(X1Visable)]
        /// <summary>
        /// 放料位置2
        /// </summary>
        public double PutPos2 { get; set; } = 0;

        [CategoryAttribute(X1Cat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("UV工作位置")]
        [Browsable(X1Visable)]
        /// <summary>
        /// UV工作位置
        /// </summary>
        public double UVWorkPos { get; set; } = 0;


        const string X1Cat1 = "A01.位置設置";

        [CategoryAttribute(X1Cat1), DescriptionAttribute("即從待命位置直綫運動至此位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01安全待命位置")]
        [Browsable(X1Visable)]
        public string SafePosReady { get; set; } = string.Empty;

        [CategoryAttribute(X1Cat1), DescriptionAttribute("即點膠完成一點后停留位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02點膠待命位置")]
        [Browsable(X1Visable)]
        public string DispendingPosReady { get; set; } = string.Empty;


        #endregion


        #region 第三站 INI

        const string Cat00 = "A00.鐳射頭與吸嘴設置";


        [CategoryAttribute(Cat00), DescriptionAttribute("鐳射頭測量塊歸位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01鐳射頭位置")]
        [Browsable(X3Visable)]
        public string LEPos { get; set; } = string.Empty;
        [CategoryAttribute(Cat00), DescriptionAttribute("吸嘴吸住塊歸位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02吸嘴位置")]
        [Browsable(X3Visable)]
        public string AttractPos { get; set; } = string.Empty;

        [CategoryAttribute(Cat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03相對位置")]
        [Browsable(X3Visable)]
        public double Offset_LEAttract { get; set; } = 0;

        [CategoryAttribute(Cat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04微調Z伸進距離")]
        [Browsable(X3Visable)]
        public double Offset_ModuleZ { get; set; } = 0;

        [CategoryAttribute(Cat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05Mirror1相對位置補償")]
        [Browsable(X3Visable)]
        public double Mirror1_Offset_Adj { get; set; } = 0;
        [CategoryAttribute(Cat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("06Mirror2相對位置補償")]
        [Browsable(X3Visable)]
        public double Mirror2_Offset_Adj { get; set; } = 0;


        //public string InitialPos { get; set; } = string.Empty;//放到各个plc motion 里面了
        //public string InitialTheta { get; set; } = string.Empty;

        const string Cat0 = "A02.拾取設置";

        [CategoryAttribute(Cat0), DescriptionAttribute("第一個 Mirror 的取得位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror1位置集合")]
        [Browsable(X3Visable)]
        /// <summary>
        /// 第一個 Mirror 的取得位置(可擴充多個)
        /// </summary>
        public string sMirror1PosList { get; set; } = string.Empty;
        public List<string> Mirror1PosList = new List<string>();

        [CategoryAttribute(Cat0), DescriptionAttribute("第二個 Mirror 的取得位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror2位置集合")]
        [Browsable(X3Visable)]
        public string sMirror2PosList { get; set; } = string.Empty;
        public List<string> Mirror2PosList = new List<string>();

        [CategoryAttribute(Cat0), DescriptionAttribute("拾取前进距离 即微调模组 进去吸料的位置")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror1拾取前进位置")]
        [Browsable(X3Visable)]
        public int sMirrorAdjDeep1Length { get; set; } = 0;
        [CategoryAttribute(Cat0), DescriptionAttribute("拾取前进距离 即微调模组 进去吸料的位置")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror2拾取前进位置")]
        [Browsable(X3Visable)]
        public int sMirrorAdjDeep2Length { get; set; } = 0;

        [CategoryAttribute(Cat0), DescriptionAttribute("拾取后退距离 即微调模组 吸料后出来的位置")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("拾取后退位置")]
        [Browsable(X3Visable)]
        public int sMirrorAdjBackLength { get; set; } = 0;

        const string Cat2 = "A01.平面度設置";

        [CategoryAttribute(Cat2), DescriptionAttribute("第一個 Mirror 取得平面資料的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror1平面资料集合")]
        [Browsable(X3Visable)]
        public string sMirror1PlanePosList { get; set; } = string.Empty;
        /// <summary>
        /// Mirror1平面资料集合
        /// </summary>
        public List<string> Mirror1PlanePosList = new List<string>();
        [CategoryAttribute(Cat2), DescriptionAttribute("第二個 Mirror 取得平面資料的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror2平面资料集合")]
        [Browsable(X3Visable)]
        public string sMirror2PlanePosList { get; set; } = string.Empty;
        /// <summary>
        /// Mirror2平面资料集合
        /// </summary>
        public List<string> Mirror2PlanePosList = new List<string>();

        [CategoryAttribute(Cat2), DescriptionAttribute("塊規取得平面資料的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("鐳射頭塊規平面資料位置")]
        [Browsable(X3Visable)]
        public string sMirror0PlanePosList { get; set; } = string.Empty;
        /// <summary>
        /// 塊規平面資料位置
        /// </summary>
        public List<string> Mirror0PlanePosList = new List<string>();

        [CategoryAttribute(Cat2), DescriptionAttribute("塊規平面度資料 采用了模组1的yz 作为xy数据 LE读取的高度作为z数据")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("塊規平面度資料")]
        [Browsable(false)]
        public string sMirror0PlaneHeightPosList { get; set; } = string.Empty;
        public List<string> Mirror0PlaneHeightPosList = new List<string>();


        const string Cat3 = "A03.校正設置";

        [CategoryAttribute(Cat3), DescriptionAttribute("Mirror1 檢測補償偏移的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror1檢測補償")]
        [Browsable(X3Visable)]
        public string Mirror1CaliPos { get; set; } = string.Empty;
        [CategoryAttribute(Cat3), DescriptionAttribute("Mirror2 檢測補償偏移的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror2檢測補償")]
        [Browsable(X3Visable)]
        public string Mirror2CaliPos { get; set; } = string.Empty;
        [CategoryAttribute(Cat3), DescriptionAttribute("校正檢測補償偏移的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("校正檢測補償")]
        [Browsable(X3Visable)]
        public string Mirror0CaliPos { get; set; } = string.Empty;


        const string Cat4 = "A04.放入設置";


        [CategoryAttribute(Cat4), DescriptionAttribute("Mirror1 開始調整位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror1 開始調整位置")]
        [Browsable(X3Visable)]
        public string Mirror1PutPos { get; set; } = string.Empty;
        [CategoryAttribute(Cat4), DescriptionAttribute("Mirror2 開始調整位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror2 開始調整位置")]
        [Browsable(X3Visable)]
        public string Mirror2PutPos { get; set; } = string.Empty;

        [CategoryAttribute(Cat4), DescriptionAttribute("放入前进位置 即微调模组 进去放入的位置")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror1放入前进位置")]
        [Browsable(X3Visable)]
        [ReadOnly(true)]
        public int sMirrorPutAdjDeep1Length { get; set; } = 0;
        [CategoryAttribute(Cat4), DescriptionAttribute("放入前进位置 即微调模组 进去放入的位置")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mirror2放入前进位置")]
        [Browsable(X3Visable)]
        [ReadOnly(true)]
        public int sMirrorPutAdjDeep2Length { get; set; } = 0;


        const string Cat1 = "A05.點膠設置";

        #region 点胶设置

        [CategoryAttribute(Cat1), DescriptionAttribute("第一個 Mirror 的點膠位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.A Mirror 點膠位置")]
        [Browsable(X3Visable)]
        public string sMirror1JamedPosList { get; set; } = string.Empty;
        public List<string> Mirror1JamedPosList = new List<string>();
        [CategoryAttribute(Cat1), DescriptionAttribute("第二個 Mirror 的點膠位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.B Mirror 點膠位置")]
        [Browsable(X3Visable)]
        public string sMirror2JamedPosList { get; set; } = string.Empty;
        public List<string> Mirror2JamedPosList = new List<string>();

        [CategoryAttribute(Cat1), DescriptionAttribute("避光槽位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("06.避光槽位置")]
        [Browsable(X3Visable)]
        public string ShadowPos { get; set; } = string.Empty;
        [CategoryAttribute(Cat1), DescriptionAttribute("點膠待機位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05.點膠待機位置")]
        [Browsable(X3Visable)]
        public string ShadowPosUp { get; set; } = string.Empty;

        [CategoryAttribute(Cat1), DescriptionAttribute("第一個 Mirror 的 UV 光位置(可多點循環照射)")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [Browsable(false)]
        [DisplayName("03.A Mirror UV光位置")]
        public string sMirror1UVPosList { get; set; } = string.Empty;
        public List<string> Mirror1UVPosList = new List<string>();
        [CategoryAttribute(Cat1), DescriptionAttribute("第二個 Mirror 的 UV 光位置(可多點循環照射)")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [Browsable(false)]
        [DisplayName("04.B Mirror UV光位置")]
        public string sMirror2UVPosList { get; set; } = string.Empty;
        public List<string> Mirror2UVPosList = new List<string>();


        [CategoryAttribute(Cat1), DescriptionAttribute("一鍵測試點膠的位置集合 可設多點")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [Browsable(X3Visable)]
        [DisplayName("07.測試點膠位置")]
        public string sMirrorTestDispensingPosList { get; set; } = string.Empty;
        public List<string> MirrorTestDispensingPosList = new List<string>();

        [CategoryAttribute(Cat1), DescriptionAttribute("一鍵測試點膠待命位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [Browsable(X3Visable)]
        [DisplayName("08.測試點膠待命位置")]
        public string sMirrorTestDispensingReady { get; set; } = string.Empty;


        [CategoryAttribute(Cat1), DescriptionAttribute("點膠機在首次使用時所需要先放出多少膠(ms)")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("07.點膠機首次使用时间")]
        [Browsable(false)]
        public int DispensingInitialTime { get; set; } = 3000;

        [CategoryAttribute(Cat1), DescriptionAttribute("點膠機在初始化放出多少膠的位置")]
        [Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("07.點膠機初始化位置")]
        [Browsable(false)]
        public string InitialDispensingPos { get; set; } = string.Empty;

        #endregion

        #region 文件路径设置

        const string Cat5 = "A06.文件路徑設置";

        [CategoryAttribute(Cat5), DescriptionAttribute("LE控制器配置文件")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.控制器配置文件")]
        [ReadOnly(true)]
        [Browsable(X3Visable)]
        public string CfgPath { get; set; } = string.Empty;

        [CategoryAttribute(Cat5), DescriptionAttribute("LE控制器位移校准文件")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.控制器位移校准文件")]
        [ReadOnly(true)]
        [Browsable(X3Visable)]
        public string HWCPath { get; set; } = string.Empty;

        #endregion

        #region 其他设置

        const string Cat6 = "A07.其他設置";

        [CategoryAttribute(Cat6), DescriptionAttribute("true:啓用 false:不啓用")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.是否啓用高度測量")]
        [Browsable(X3Visable)]
        public bool IsUseMeasureHeight { get; set; } = false;

        [CategoryAttribute(Cat6), DescriptionAttribute("測試點膠時間 单位(ms)")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.測試點膠時間")]
        [Browsable(X3Visable)]
        public int DispensingMsTime { get; set; } = 50;

        #endregion

        #endregion


        public void Initial()
        {
            MAINPATH = Universal.MAINPATH;
            INIFILE = MAINPATH + "\\CONFIG.ini";

            Load();
        }

        public void Load()
        {
            Offset_ModuleZ = double.Parse(ReadINIValue("Basic", "Offset_ModuleZ", Offset_ModuleZ.ToString(), INIFILE));
            Offset_LEAttract = double.Parse(ReadINIValue("Basic", "Offset_LEAttract", Offset_LEAttract.ToString(), INIFILE));
            Mirror1_Offset_Adj = double.Parse(ReadINIValue("Basic", "Mirror1_Offset_Adj", Mirror1_Offset_Adj.ToString(), INIFILE));
            Mirror2_Offset_Adj = double.Parse(ReadINIValue("Basic", "Mirror2_Offset_Adj", Mirror2_Offset_Adj.ToString(), INIFILE));

            LEPos = ReadINIValue("Basic", "LEPos", LEPos.ToString(), INIFILE);
            AttractPos = ReadINIValue("Basic", "AttractPos", AttractPos.ToString(), INIFILE);

            sMirror1PosList = ReadINIValue("Basic", "sMirror1PosList", sMirror1PosList.ToString(), INIFILE);
            Mirror1PosList = sMirror1PosList.Split(';').ToList();
            sMirror2PosList = ReadINIValue("Basic", "sMirror2PosList", sMirror2PosList.ToString(), INIFILE);
            Mirror2PosList = sMirror2PosList.Split(';').ToList();

            sMirror1PlanePosList = ReadINIValue("Basic", "sMirror1PlanePosList", sMirror1PlanePosList.ToString(), INIFILE);
            Mirror1PlanePosList = sMirror1PlanePosList.Split(';').ToList();
            sMirror2PlanePosList = ReadINIValue("Basic", "sMirror2PlanePosList", sMirror2PlanePosList.ToString(), INIFILE);
            Mirror2PlanePosList = sMirror2PlanePosList.Split(';').ToList();

            sMirror0PlanePosList = ReadINIValue("Basic", "sMirror0PlanePosList", sMirror0PlanePosList.ToString(), INIFILE);
            Mirror0PlanePosList = sMirror0PlanePosList.Split(';').ToList();
            sMirror0PlaneHeightPosList = ReadINIValue("Basic", "sMirror0PlaneHeightPosList", sMirror0PlaneHeightPosList.ToString(), INIFILE);
            Mirror0PlaneHeightPosList = sMirror0PlaneHeightPosList.Split(';').ToList();

            Mirror1CaliPos = ReadINIValue("Basic", "Mirror1CaliPos", Mirror1CaliPos.ToString(), INIFILE);
            Mirror2CaliPos = ReadINIValue("Basic", "Mirror2CaliPos", Mirror2CaliPos.ToString(), INIFILE);
            Mirror0CaliPos = ReadINIValue("Basic", "Mirror0CaliPos", Mirror0CaliPos.ToString(), INIFILE);
            Mirror1PutPos = ReadINIValue("Basic", "Mirror1PutPos", Mirror1PutPos.ToString(), INIFILE);
            Mirror2PutPos = ReadINIValue("Basic", "Mirror2PutPos", Mirror2PutPos.ToString(), INIFILE);

            sMirror1JamedPosList = ReadINIValue("Basic", "sMirror1JamedPosList", sMirror1JamedPosList.ToString(), INIFILE);
            Mirror1JamedPosList = sMirror1JamedPosList.Split(';').ToList();
            sMirror2JamedPosList = ReadINIValue("Basic", "sMirror2JamedPosList", sMirror2JamedPosList.ToString(), INIFILE);
            Mirror2JamedPosList = sMirror2JamedPosList.Split(';').ToList();


            sMirrorTestDispensingPosList = ReadINIValue("Basic", "sMirrorTestDispensingPosList", sMirrorTestDispensingPosList.ToString(), INIFILE);
            MirrorTestDispensingPosList = sMirrorTestDispensingPosList.Split(';').ToList();
            sMirrorTestDispensingReady = ReadINIValue("Basic", "sMirrorTestDispensingReady", sMirrorTestDispensingReady.ToString(), INIFILE);

            ShadowPos = ReadINIValue("Basic", "ShadowPos", ShadowPos.ToString(), INIFILE);
            ShadowPosUp = ReadINIValue("Basic", "ShadowPosUp", ShadowPosUp.ToString(), INIFILE);

            sMirror1UVPosList = ReadINIValue("Basic", "sMirror1UVPosList", sMirror1UVPosList.ToString(), INIFILE);
            Mirror1UVPosList = sMirror1UVPosList.Split(';').ToList();
            sMirror2UVPosList = ReadINIValue("Basic", "sMirror2UVPosList", sMirror2UVPosList.ToString(), INIFILE);
            Mirror2UVPosList = sMirror2UVPosList.Split(';').ToList();

            DispensingInitialTime = int.Parse(ReadINIValue("Basic", "DispensingInitialTime", DispensingInitialTime.ToString(), INIFILE));
            InitialDispensingPos = ReadINIValue("Basic", "InitialDispensingPos", InitialDispensingPos.ToString(), INIFILE);

            CfgPath = ReadINIValue("FilePath", "CfgPath", CfgPath.ToString(), INIFILE);
            HWCPath = ReadINIValue("FilePath", "HWCPath", HWCPath.ToString(), INIFILE);

            IsUseMeasureHeight = ReadINIValue("Other", "IsUseMeasureHeight", (IsUseMeasureHeight ? "1" : "0"), INIFILE) == "1";
            DispensingMsTime = int.Parse(ReadINIValue("Other", "DispensingMsTime", DispensingMsTime.ToString(), INIFILE));

            sMirrorAdjDeep1Length = int.Parse(ReadINIValue("Basic", "sMirrorAdjDeepLength", sMirrorAdjDeep1Length.ToString(), INIFILE));
            sMirrorAdjDeep2Length = int.Parse(ReadINIValue("Basic", "sMirrorAdjDeep2Length", sMirrorAdjDeep2Length.ToString(), INIFILE));
            sMirrorPutAdjDeep1Length = int.Parse(ReadINIValue("Basic", "sMirrorPutAdjDeepLength", sMirrorPutAdjDeep1Length.ToString(), INIFILE));
            sMirrorPutAdjDeep2Length = int.Parse(ReadINIValue("Basic", "sMirrorPutAdjDeep2Length", sMirrorPutAdjDeep2Length.ToString(), INIFILE));
            sMirrorAdjBackLength = int.Parse(ReadINIValue("Basic", "sMirrorAdjBackLength", sMirrorAdjBackLength.ToString(), INIFILE));



            sDispensingX1_1PosList = ReadINIValue("DX1", "sDispensingX1_1PosList", sDispensingX1_1PosList.ToString(), INIFILE);
            DispensingX1_1PosList = sDispensingX1_1PosList.Split(';').ToList();
            sDispensingX1_2PosList = ReadINIValue("DX1", "sDispensingX1_2PosList", sDispensingX1_2PosList.ToString(), INIFILE);
            DispensingX1_2PosList = sDispensingX1_2PosList.Split(';').ToList();

            GetPos1 = double.Parse(ReadINIValue("DX1", "GetPos1", GetPos1.ToString(), INIFILE));
            GetPos2 = double.Parse(ReadINIValue("DX1", "GetPos2", GetPos2.ToString(), INIFILE));
            PutPos1 = double.Parse(ReadINIValue("DX1", "PutPos1", PutPos1.ToString(), INIFILE));
            PutPos2 = double.Parse(ReadINIValue("DX1", "PutPos2", PutPos2.ToString(), INIFILE));
            UVWorkPos = double.Parse(ReadINIValue("DX1", "UVWorkPos", UVWorkPos.ToString(), INIFILE));

            SafePosReady = ReadINIValue("DX1", "SafePosReady", SafePosReady.ToString(), INIFILE);
            DispendingPosReady = ReadINIValue("DX1", "DispendingPosReady", DispendingPosReady.ToString(), INIFILE);

        }
        public void Save()
        {
            WriteINIValue("Basic", "Offset_ModuleZ", Offset_ModuleZ.ToString(), INIFILE);
            WriteINIValue("Basic", "Offset_LEAttract", Offset_LEAttract.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror1_Offset_Adj", Mirror1_Offset_Adj.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror2_Offset_Adj", Mirror2_Offset_Adj.ToString(), INIFILE);


            WriteINIValue("Basic", "LEPos", LEPos.ToString(), INIFILE);
            WriteINIValue("Basic", "AttractPos", AttractPos.ToString(), INIFILE);

            WriteINIValue("Basic", "sMirror1PosList", sMirror1PosList.ToString(), INIFILE);
            Mirror1PosList = sMirror1PosList.Split(';').ToList();
            WriteINIValue("Basic", "sMirror2PosList", sMirror2PosList.ToString(), INIFILE);
            Mirror2PosList = sMirror2PosList.Split(';').ToList();

            WriteINIValue("Basic", "sMirror1PlanePosList", sMirror1PlanePosList.ToString(), INIFILE);
            Mirror1PlanePosList = sMirror1PlanePosList.Split(';').ToList();
            WriteINIValue("Basic", "sMirror2PlanePosList", sMirror2PlanePosList.ToString(), INIFILE);
            Mirror2PlanePosList = sMirror2PlanePosList.Split(';').ToList();

            WriteINIValue("Basic", "sMirror0PlanePosList", sMirror0PlanePosList.ToString(), INIFILE);
            Mirror0PlanePosList = sMirror0PlanePosList.Split(';').ToList();
            WriteINIValue("Basic", "sMirror0PlaneHeightPosList", sMirror0PlaneHeightPosList.ToString(), INIFILE);
            Mirror0PlaneHeightPosList = sMirror0PlaneHeightPosList.Split(';').ToList();

            WriteINIValue("Basic", "Mirror1CaliPos", Mirror1CaliPos.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror2CaliPos", Mirror2CaliPos.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror0CaliPos", Mirror0CaliPos.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror1PutPos", Mirror1PutPos.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror2PutPos", Mirror2PutPos.ToString(), INIFILE);

            WriteINIValue("Basic", "sMirror1JamedPosList", sMirror1JamedPosList.ToString(), INIFILE);
            Mirror1JamedPosList = sMirror1JamedPosList.Split(';').ToList();
            WriteINIValue("Basic", "sMirror2JamedPosList", sMirror2JamedPosList.ToString(), INIFILE);
            Mirror2JamedPosList = sMirror2JamedPosList.Split(';').ToList();

            WriteINIValue("Basic", "sMirrorTestDispensingPosList", sMirrorTestDispensingPosList.ToString(), INIFILE);
            MirrorTestDispensingPosList = sMirrorTestDispensingPosList.Split(';').ToList();
            WriteINIValue("Basic", "sMirrorTestDispensingReady", sMirrorTestDispensingReady.ToString(), INIFILE);

            WriteINIValue("Basic", "ShadowPos", ShadowPos.ToString(), INIFILE);
            WriteINIValue("Basic", "ShadowPosUp", ShadowPosUp.ToString(), INIFILE);

            WriteINIValue("Basic", "sMirror1UVPosList", sMirror1UVPosList.ToString(), INIFILE);
            Mirror1UVPosList = sMirror1UVPosList.Split(';').ToList();
            WriteINIValue("Basic", "sMirror2UVPosList", sMirror2UVPosList.ToString(), INIFILE);
            Mirror2UVPosList = sMirror2UVPosList.Split(';').ToList();

            WriteINIValue("Basic", "DispensingInitialTime", DispensingInitialTime.ToString(), INIFILE);
            WriteINIValue("Basic", "InitialDispensingPos", InitialDispensingPos.ToString(), INIFILE);

            WriteINIValue("FilePath", "CfgPath", CfgPath.ToString(), INIFILE);
            WriteINIValue("FilePath", "HWCPath", HWCPath.ToString(), INIFILE);

            WriteINIValue("Other", "IsUseMeasureHeight", (IsUseMeasureHeight ? "1" : "0"), INIFILE);
            WriteINIValue("Other", "DispensingMsTime", DispensingMsTime.ToString(), INIFILE);

            WriteINIValue("Basic", "sMirrorAdjDeepLength", sMirrorAdjDeep1Length.ToString(), INIFILE);
            WriteINIValue("Basic", "sMirrorAdjDeep2Length", sMirrorAdjDeep2Length.ToString(), INIFILE);
            WriteINIValue("Basic", "sMirrorPutAdjDeepLength", sMirrorPutAdjDeep1Length.ToString(), INIFILE);
            WriteINIValue("Basic", "sMirrorPutAdjDeep2Length", sMirrorPutAdjDeep2Length.ToString(), INIFILE);
            WriteINIValue("Basic", "sMirrorAdjBackLength", sMirrorAdjBackLength.ToString(), INIFILE);


            WriteINIValue("DX1", "sDispensingX1_1PosList", sDispensingX1_1PosList.ToString(), INIFILE);
            DispensingX1_1PosList = sDispensingX1_1PosList.Split(';').ToList();
            WriteINIValue("DX1", "sDispensingX1_2PosList", sDispensingX1_2PosList.ToString(), INIFILE);
            DispensingX1_2PosList = sDispensingX1_2PosList.Split(';').ToList();
            WriteINIValue("DX1", "GetPos1", GetPos1.ToString(), INIFILE);
            WriteINIValue("DX1", "GetPos2", GetPos2.ToString(), INIFILE);
            WriteINIValue("DX1", "PutPos1", PutPos1.ToString(), INIFILE);
            WriteINIValue("DX1", "PutPos2", PutPos2.ToString(), INIFILE);
            WriteINIValue("DX1", "UVWorkPos", UVWorkPos.ToString(), INIFILE);


            WriteINIValue("DX1", "SafePosReady", SafePosReady.ToString(), INIFILE);
            WriteINIValue("DX1", "DispendingPosReady", DispendingPosReady.ToString(), INIFILE);

        }

        public void SavePlaneHeight()
        {
            WriteINIValue("Basic", "sMirror0PlaneHeightPosList", sMirror0PlaneHeightPosList.ToString(), INIFILE);
            Mirror0PlaneHeightPosList = sMirror0PlaneHeightPosList.Split(';').ToList();
        }
    }
}
