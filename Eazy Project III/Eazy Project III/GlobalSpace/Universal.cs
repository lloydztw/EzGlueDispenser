using JetEazy;
using JetEazy.ControlSpace;
using JetEazy.DBSpace;
//using PhotoMachine.ControlSpace.MachineSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsCommon.ControlSpace;
using Eazy_Project_Interface;
using Eazy_Project_Module;
using Eazy_Project_III.ControlSpace.MachineSpace;
using VsCommon.ControlSpace.MachineSpace;
using Eazy_Project_III.OPSpace;
using JetEazy.CCDSpace;
using Eazy_Project_Measure;

namespace Eazy_Project_III
{
    public class Universal : JetEazy.Universal
    {
        public static bool IsNoUseCCD = false;
        public static bool IsNoUseIO = false;
        public static bool IsNoUseMotor = false;

        public static string VersionDate = "2022/06/21";

        public static VersionEnum VERSION = VersionEnum.PROJECT;
        public static OptionEnum OPTION = OptionEnum.DISPENSING;


        //Environment Variables
        public static int MAINTICK = 100;
        //更新主程序画面
        public static int DISPLAYTICK = 500;

        public static bool IsSaveRaw = false;
        public static bool IsMultiThreadUseToRun = true;

        /// <summary>
        /// 种子功能
        /// </summary>
        public static bool IsUseSeedFuntion = false;

        public static string CODEPATH = @"D:\AUTOMATION";
        public static string VEROPT = VERSION.ToString() + "-" + OPTION.ToString();
        public static string MAINPATH = @"D:\JETEAZY\" + VEROPT;

        public static string DBPATH = MAINPATH + @"\DB";
        public static string RCPPATH = MAINPATH + @"\PIC";
        public static string UIPATH = CODEPATH + @"\" + VERSION.ToString() + "UI";


        public static string COLLECT = @"D:\JETEAZY\" + VEROPT + @"\COLLECT";
        public static string LOGDBPATH = @"D:\JETEAZY\" + VEROPT + @"\LOGDB";
        public static string BACKUPDBPATH = @"D:\JETEAZY\" + VEROPT + @"\BACKUPDB";
        public static string LOGTXTPATH = @"D:\JETEAZY\" + VEROPT + @"\LOGTXT";
        public static string WORKPATH = @"D:\JETEAZY\" + VEROPT + @"\WORK";
        public static string DEBUGRAWPATH = @"D:\JETEAZY\" + VEROPT + @"\ORG";              //偵錯儲存的原圖位置
        public static string DEBUGRESULTPATH = @"D:\JETEAZY\" + VEROPT + @"\DEBUG";         //偵錯結果圖位置
        public static string TESTRESULTPATH = @"D:\COPYDATA";                               //偵錯結果圖位置
        public static string DEBUGSRCPATH = @"D:\JETEAZY\" + VEROPT + @"\SRCDEBUG";         //離線測試用的原圖位置
        public static string OCRIMAGEPATH = @"D:\LOA\OCR\";                                 //保存的OCR测试图位置  
        public static string BarcodeIMAGEPATH = @"D:\LOA\Barcode\";                         //保存的OCR测试图位置  
        /// <summary>
        /// 跑线时读到SN.txt里的东西
        /// </summary>
        public static string DATASNTXT = "";
        public static string RELATECOLORSTR = "";
        public static string SHOWBMPSTRING = "view.png";
        public static string PlayerPASSPATH = WORKPATH + @"\TADA.wav";
        public static string PlayerFAILPATH = WORKPATH + @"\RoutingNG.wav";
        public static string PlayerOPPWRATPATH = WORKPATH + @"\OPPWRAP.wav";
        public static string RunDebugOrRelease = "";
        public static string FAILBARCODE = "";

        public static string MainX6_Path = "D:\\CollectPictures\\Inspection\\";

        static string DATACNNSTRING = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DBPATH + @"\DATA.mdb;Jet OLEDB:Database Password=12892414;";

        static int LanguageIndex = 0;
        public static string InitialErrorString = "";

        public static System.Drawing.Point MainFormLocation = new System.Drawing.Point(0, 0);

        /// <summary>
        /// MAIN_SD调试马达测试窗口是否打开标志
        /// </summary>
        public static bool IsOpenMotorWindows = false;
        /// <summary>
        /// 离线模式自动登入账户admin
        /// </summary>
        public static bool IsOfflineUserAutoLogin = false;

        public static CAMERAClass[] CAMERAS;
        //public static CCDCollectionClass CCDCollection;
        public static CCDCollectionClass CCDCollection;
        public static MachineCollectionClass MACHINECollection;
        

        public static AccDBClass ACCDB;
        public static EsssDBClass ESSDB;
        public static RCPDBClass RCPDB;
        public static RUNDBClass RUNDB;

        public static bool Initial(int langindex)
        {
            //TestProgram();

            bool ret = true;
            WORKPATH = MAINPATH + @"\WORK";

            string ccd_type_filepath = "";

            //Vac vac = new Vac();
            //IVac vac1 = Vac.Instance;
            //vac1.Seton();

            //初始化语言
            JetEazy.BasicSpace.LanguageExClass.Instance.Load(WORKPATH);
            JetEazy.BasicSpace.LanguageExClass.Instance.LanguageIndex = 1;



            switch (Universal.OPTION)
            {
                case OptionEnum.MAIN_X6:

                    //switch (INI.CHANGE_FILE_PATH)
                    //{
                    //    //TYPE18 1800W
                    //    case 1:

                    //        ccd_type_filepath = "_TYPE18";

                    //        DBPATH = DBPATH + ccd_type_filepath;
                    //        RCPPATH = RCPPATH + ccd_type_filepath;
                    //        DEBUGSRCPATH = DEBUGSRCPATH + ccd_type_filepath;

                    //        break;
                    //}

                    break;
            }


            //string spath = System.AppDomain.CurrentDomain.BaseDirectory;
            //if (File.Exists(spath + @"\license.dat"))
            //    File.Delete(spath + @"\license.dat");
            //if (File.Exists(WORKPATH + @"\JetAu.dat"))
            //    File.Copy(WORKPATH + @"\JetAu.dat", spath + "license.dat");

            LanguageIndex = langindex;

            CreateDebugDirectories();

            ACCDB = new AccDBClass(DBPATH + @"\ACCDB.jdb");
            ESSDB = new EsssDBClass(DBPATH + @"\ESSDB.jdb");
            RCPDB = new RCPDBClass(DBPATH + @"\RCPDB.jdb", RCPPATH, ESSDB.LastRecipeIndex);
            RUNDB = new RUNDBClass(DBPATH + @"\RUNDB.jdb");

            //参数加载
            RecipeCHClass.Instance.Initial(RCPPATH, RCPDB.RCPItemNow.Index);


            ret &= InitialMachineCollection();

            if (!ret)
            {
                //InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
                JetEazy.BasicSpace.VsMSG.Instance.Warning("plc连接错误，请检查。");
                //return false;
            }

            ret &= InitialMeasureHeight();

            if (!ret)
            {
                //InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
                JetEazy.BasicSpace.VsMSG.Instance.Warning("LE测量高度连接错误，请检查。");
                //return false;
            }

            //if (!ret)
            //{
            //    InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
            //    return false;
            //}

            ret &= InitialCCD();

            if (!ret)
            {
                //InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
                //return false;
            }

            return ret;
        }
        public static void SetLanguage(int langindex)
        {
            LanguageIndex = langindex;
        }
        static bool InitialMachineCollection()
        {
            bool ret = true;

            string opstr = "";

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch(OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            #region 点胶第三站 

                            opstr += "1,";  //1個 PLC  
                            opstr += "9,";   //9個軸

                            DispensingMachineClass machine = new DispensingMachineClass(Machine_EA.DISPENSING, opstr, WORKPATH, IsNoUseIO);
                            ret = machine.Initial(IsNoUseIO, IsNoUseMotor);

                            MACHINECollection = new MachineCollectionClass();
                            MACHINECollection.Intial(VERSION, OPTION, machine);


                            if (ret)
                            {
                                ////初始化位置 固定6
                                //machine.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
                                //machine.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
                                //machine.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));
                            }

                            //这里模组初始化

                            Vac.Instance.SetMachine(machine);
                            UVCylinder.Instance.SetMachine(machine);
                            Dispensing.Instance.SetMachine(machine);
                            UV.Instance.SetMachine(machine);
                            Projector.Instance.SetMachine(machine);
                            Light.Instance.SetMachine(machine);
                            Keyence.Instance.SetMachine(machine);

                            //LEClass.Instance.Init(INI.Instance.CfgPath, INI.Instance.HWCPath, !INI.Instance.IsUseMeasureHeight);

                            MotorConfig.Instance.Initial(WORKPATH);

                            int i = 0;
                            while (i < 20)
                            {
                                if (i == 6 || i == 7 || i == 8)
                                {
                                    machine.PLCIO.SetMWIndex(1340 + i, MotorConfig.Instance.PosSafe[i]);
                                }
                                else
                                {
                                    machine.PLCIO.SetMWIndex(1340 + i, MotorConfig.Instance.PosSafe[i] * 100);
                                }
                                i++;
                            }

                            #endregion




                            break;
                        case OptionEnum.DISPENSINGX1:

                            #region 点胶第一站 

                            opstr += "1,";  //1個 PLC  
                            opstr += "5,";   //9個軸

                            DispensingX1MachineClass machineX1 = new DispensingX1MachineClass(Machine_EA.DISPENSINGX1, opstr, WORKPATH, IsNoUseIO);
                            ret = machineX1.Initial(IsNoUseIO, IsNoUseMotor);

                            MACHINECollection = new MachineCollectionClass();
                            MACHINECollection.Intial(VERSION, OPTION, machineX1);


                            if (ret)
                            {

                                ////初始化位置 固定6

                                //int ix = 0;
                                //while (ix < MACHINECollection.GetMotorCount())
                                //{
                                //    machineX1.PLCIO.MotorSinglePosition(ix, 6, MACHINECollection.GetSingleAXISPositionForReady(ix));
                                //    ix++;
                                //}

                                ////現在可以不用寫 可以在啓動的時候再寫入位置
                                ////寫入INI的位置
                                //machineX1.PLCIO.MotorSinglePosition(2, 1, INI.Instance.GetPos1.ToString());
                                //machineX1.PLCIO.MotorSinglePosition(2, 2, INI.Instance.GetPos2.ToString());
                                //machineX1.PLCIO.MotorSinglePosition(2, 3, INI.Instance.PutPos1.ToString());
                                //machineX1.PLCIO.MotorSinglePosition(2, 4, INI.Instance.PutPos2.ToString());
                                //machineX1.PLCIO.MotorSinglePosition(3, 1, INI.Instance.UVWorkPos.ToString());

                                ////寫入點膠位置數據

                                //machineX1.PLCIO.SetMWIndex(ControlSpace.IOSpace.DX1_IOConstClass.MW1093, INI.Instance.DispensingX1_1PosList.Count);
                                //machineX1.PLCIO.SetMWIndex(ControlSpace.IOSpace.DX1_IOConstClass.MW1094, INI.Instance.DispensingX1_2PosList.Count);

                                //int dispensingindex = 0;
                                //foreach (string str in INI.Instance.DispensingX1_1PosList)
                                //{
                                //    machineX1.PLCIO.MotorDispensing1Position(dispensingindex, str);
                                //    dispensingindex++;
                                //}
                                //dispensingindex = 0;
                                //foreach (string str in INI.Instance.DispensingX1_2PosList)
                                //{
                                //    machineX1.PLCIO.MotorDispensing2Position(dispensingindex, str);
                                //    dispensingindex++;
                                //}

                                //machine.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
                                //machine.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
                                //machine.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));
                            }

                            ////这里模组初始化

                            //Vac.Instance.SetMachine(machine);
                            //UVCylinder.Instance.SetMachine(machine);
                            //Dispensing.Instance.SetMachine(machine);
                            //UV.Instance.SetMachine(machine);
                            //Projector.Instance.SetMachine(machine);
                            //Light.Instance.SetMachine(machine);
                            //Keyence.Instance.SetMachine(machine);

                            //LEClass.Instance.Init(INI.Instance.CfgPath, INI.Instance.HWCPath, !INI.Instance.IsUseMeasureHeight);

                            MotorConfig.Instance.Initial(WORKPATH);

                            i = 0;
                            while (i < 20)
                            {
                                if (i == 6 || i == 7 || i == 8)
                                {
                                    machineX1.PLCIO.SetMWIndex(1340 + i, MotorConfig.Instance.PosSafe[i]);
                                }
                                else
                                {
                                    machineX1.PLCIO.SetMWIndex(1340 + i, MotorConfig.Instance.PosSafe[i] * 100);
                                }
                                i++;
                            }

                            #endregion

                            break;
                    }

                    break;

                case VersionEnum.ALLINONE:
                    break;
                case VersionEnum.AUDIX:
                    break;
                default:
                    break;
            }

            return ret;
        }
        static bool InitialMeasureHeight()
        {
            bool ret = true;

            string opstr = "";

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            LEClass.Instance.Init(INI.Instance.CfgPath, INI.Instance.HWCPath, !INI.Instance.IsUseMeasureHeight);
                            ret = LEClass.Instance.Open() == 0;

                            break;
                    }

                    break;

                case VersionEnum.ALLINONE:
                    break;
                case VersionEnum.AUDIX:
                    break;
                default:
                    break;
            }

            return ret;
        }
        static bool InitialCCD()
        {
            bool ret = true;

            CameraConfig.Instance.Initial(WORKPATH);
            //CameraConfig.Instance.Save();
            CAMERAS = new CAMERAClass[CameraConfig.Instance.COUNT];
            int i = 0;
            while (i < CameraConfig.Instance.COUNT)
            {
                CAMERAS[i] = new CAMERAClass();
                CAMERAS[i].Initial(CameraConfig.Instance.cameras[i].ToCameraString());
            
                i++;
            }


            //CCDCollection = new CCDCollectionClass(WORKPATH, IsNoUseCCD, VERSION, OPTION);

            //ret = CCDCollection.Initial(WORKPATH);

            //if (ret)
            //    CCDCollection.GetBmpAll(-2);


            //CCD = new CCDGroupClass(INI.CCD_TOTALHEAD);

            //int ccdkind = 0;

            //CCDClass.EPIXFMTPath = WORKPATH;

            //while (ccdkind < INI.CCD_KIND)
            //{
            //    CCDClass ccd = new CCDClass();

            //    switch (ccdkind)
            //    {
            //        case 0:
            //            ret &= ccd.Initial(INI.CCD_HEAD.ToString() + "@" + INI.CCD_WIDTH.ToString() + "@" + INI.CCD_HEIGHT + "@" + WORKPATH + "@" + "0" + "@" + INI.CCD_ROTATE, INI.CCD_TYPE, INI.ISNOLIVE);
            //            break;
            //        case 1:
            //            ret &= ccd.Initial(INI.CCD2_HEAD.ToString() + "@" + INI.CCD2_WIDTH.ToString() + "@" + INI.CCD2_HEIGHT + "@" + WORKPATH + "@" + INI.CCD_HEAD.ToString() + "@" + INI.CCD2_ROTATE, INI.CCD2_TYPE, INI.ISNOLIVE);
            //            break;
            //        case 2:
            //            ret &= ccd.Initial(INI.CCD3_HEAD.ToString() + "@" + INI.CCD3_WIDTH.ToString() + "@" + INI.CCD3_HEIGHT + "@" + WORKPATH + "@" + (INI.CCD_HEAD + INI.CCD2_HEAD).ToString() + "@" + INI.CCD3_ROTATE, INI.CCD3_TYPE, INI.ISNOLIVE);
            //            break;
            //        case 3:
            //            ret &= ccd.Initial(INI.CCD4_HEAD.ToString() + "@" + INI.CCD4_WIDTH.ToString() + "@" + INI.CCD4_HEIGHT + "@" + WORKPATH + "@" + (INI.CCD_HEAD + INI.CCD2_HEAD + INI.CCD3_HEAD).ToString() + "@" + INI.CCD4_ROTATE, INI.CCD4_TYPE, INI.ISNOLIVE);
            //            break;
            //    }

            //    if (!ret)
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        CCD.Add(ccd);
            //    }
            //    ccdkind++;
            //}


            return ret;
        }
        static void CreateDebugDirectories()
        {
            //if (!Directory.Exists(COLLECT))
            //    Directory.CreateDirectory(COLLECT);
        }
        public static void Close()
        {

            if (CAMERAS != null)
            {
                int i = 0;
                while (i < CameraConfig.Instance.COUNT)
                {
                    //CAMERAS[i] = new CAMERAClass();
                    //CAMERAS[i].Initial(CameraConfig.Instance.cameras[i].ToCameraString());
                    CAMERAS[i].Close();
                    i++;
                }
            }

            //CCDCollection.Close();

            //MACHINECollection.MACHINE.PLCCollection[0].Close();
            MACHINECollection.Close();

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            LEClass.Instance.Close();

                            break;
                    }

                    break;
            }


            

        }


    }
}
