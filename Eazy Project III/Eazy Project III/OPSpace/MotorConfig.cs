using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eazy_Project_III.OPSpace
{
    public class MotorConfig
    {
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

        string INIFILE = "MotorConfig.INI";


        private static readonly MotorConfig _instance = new MotorConfig();
        public static MotorConfig Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// POGOPIN MOTOR 方式切換
        /// </summary>
        public bool PogoPinMotorMode = false;
        /// <summary>
        /// 虛擬零點
        /// </summary>
        public int VirtureZero = 0;

        /// <summary>
        /// TheaY虛擬零點
        /// </summary>
        public int TheaYVirtureZero = 0;
        /// <summary>
        /// TheaZ虛擬零點
        /// </summary>
        public int TheaZVirtureZero = 0;

        /// <summary>
        /// 鏡片0厚度
        /// </summary>
        public int Mirror0Thickness = 0;
        /// <summary>
        /// 鏡片1厚度
        /// </summary>
        public int Mirror1Thickness = 0;


        const int iCOUNT = 20;
        public int[] PosSafe = new int[iCOUNT];
        string[] PosSafeDesc = new string[iCOUNT]
        {
"#1安全位置",
"#2安全位置",
"#3安全位置",
"#4安全位置",
"#5安全位置",
"#6安全位置",
"#7安全位置",
"#8安全位置",
"#9安全位置",
"#10安全位置",
            "#1吸料安全位置",
"#2吸料安全位置",
"#1相机测偏移安全位置",
"#2相机测偏移安全位置",
"#1测平面度安全位置",
"#2测平面度安全位置",
"#1安全位置",
"#2安全位置",
"#1安全位置",
"#2安全位置",
        };

        private static readonly XProps m_xprops = new XProps();
        public static XProps XPropsInstance
        {
            get { return m_xprops; }
        }
        public void LoadIniSetup()
        {
            m_xprops.Clear();

            int i = 0;
            while (i < iCOUNT)
            {
                AddProperty("01.安全位置設定", "S" + i.ToString("000") + PosSafeDesc[i], "PosSafe" + i.ToString("000"), PosSafe[i], "plc點位:MW" + (1340 + i).ToString("0000"));
                i++;
            }

            string propertyname = "00.PLC特殊參數設定";
            AddProperty(propertyname, "POGOPIN MOTOR 方式切換", "PogoPinMotorMode", PogoPinMotorMode, "plc點位:QB1549");
            AddProperty(propertyname, "微調Z虛擬零點", "VirtureZero", VirtureZero, "plc點位:MW1370");
            AddProperty(propertyname, "θ-Y虛擬零點", "TheaYVirtureZero", TheaYVirtureZero, "plc點位:MW1372");
            AddProperty(propertyname, "θ-Z虛擬零點", "TheaZVirtureZero", TheaZVirtureZero, "plc點位:MW1373");
            AddProperty(propertyname, "鏡片0厚度", "Mirror0Thickness", Mirror0Thickness, "plc點位:MW1371");
            AddProperty(propertyname, "鏡片1厚度", "Mirror1Thickness", Mirror1Thickness, "plc點位:MW1371");

            //i = 0;
            //while (i < iCOUNT)
            //{
            //    AddProperty("01.安全位置设定", PosSafeDesc[i], "PosSafe" + i.ToString("000"), PosSafe[i], "plc点位:MW" + (1340 + i).ToString("0000"));
            //    i++;
            //}
        }
        public void SaveIniSetup()
        {
            int i = 0;
            foreach (XProp xpropItem in m_xprops)
            {
                if (xpropItem.ReleateName == "PosSafe" + i.ToString("000"))
                {
                    PosSafe[i] = int.Parse(xpropItem.Value.ToString());
                }
                switch(xpropItem.ReleateName)
                {
                    case "PogoPinMotorMode":
                        PogoPinMotorMode= bool.Parse(xpropItem.Value.ToString());
                        break;
                    case "VirtureZero":
                        VirtureZero = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "TheaYVirtureZero":
                        TheaYVirtureZero = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "TheaZVirtureZero":
                        TheaZVirtureZero = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "Mirror0Thickness":
                        Mirror0Thickness = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "Mirror1Thickness":
                        Mirror1Thickness = int.Parse(xpropItem.Value.ToString());
                        break;
                }
                i++;
            }
        }
        void AddProperty(string eProperty, string eName, string eReleateName, object eValue, string eDescription)
        {
            eProperty = LanguageExClass.Instance.ToTraditionalChinese(eProperty);
            eName = LanguageExClass.Instance.ToTraditionalChinese(eName);
            eDescription = LanguageExClass.Instance.ToTraditionalChinese(eDescription);

            XProp xProp1 = new XProp();
            xProp1.Category = eProperty;
            xProp1.ReleateName = eReleateName;
            xProp1.Name = eName;
            xProp1.Value = eValue;
            xProp1.Description = eDescription;

            //switch (eReleateName)
            //{
            //    case "SHOW_CHANGE_TIME":
            //    case "CHANGE_LANGUAGE":

            //        //xProp1.Editor = new GetPlugsPropertyEditor();

            //        break;
            //    default:
            //        break;
            //}

            m_xprops.Add(xProp1);
        }

        public void Initial(string epath)
        {
            INIFILE = epath + "\\MotorConfig.ini";
            Load();
        }

        public void Load()
        {

            int i = 0;
            while (i < iCOUNT)
            {
                PosSafe[i] = int.Parse(ReadINIValue("Basic", "PosSafe" + i.ToString("000") + "_MW" + (1340 + i).ToString("0000"), PosSafe[i].ToString(), INIFILE));
                i++;
            }

            PogoPinMotorMode = int.Parse(ReadINIValue("Basic", "PogoPinMotorMode", (PogoPinMotorMode ? "1" : "0"), INIFILE)) == 1;
            VirtureZero = int.Parse(ReadINIValue("Basic", "VirtureZero", VirtureZero.ToString(), INIFILE));
            Mirror0Thickness = int.Parse(ReadINIValue("Basic", "Mirror0Thickness", Mirror0Thickness.ToString(), INIFILE));
            Mirror1Thickness = int.Parse(ReadINIValue("Basic", "Mirror1Thickness", Mirror1Thickness.ToString(), INIFILE));


            LoadIniSetup();
        }
        public void Save()
        {
            SaveIniSetup();

            int i = 0;
            while (i < iCOUNT)
            {
                WriteINIValue("Basic", "PosSafe" + i.ToString("000") + "_MW" + (1340 + i).ToString("0000"), PosSafe[i].ToString(), INIFILE);
                i++;
            }

            WriteINIValue("Basic", "PogoPinMotorMode", (PogoPinMotorMode ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "VirtureZero", VirtureZero.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror0Thickness", Mirror0Thickness.ToString(), INIFILE);
            WriteINIValue("Basic", "Mirror1Thickness", Mirror1Thickness.ToString(), INIFILE);

        }
    }
}
