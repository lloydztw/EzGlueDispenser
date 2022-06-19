using JetEazy.ControlSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsCommon.ControlSpace.IOSpace;

namespace Eazy_Project_III.ControlSpace.IOSpace
{
    public class DispensingX1IOClass : GeoIOClass
    {

        const int INPUT_COUNT = 48;
        const int OUTPUT_COUNT = 32;
        const int ONEMMSTEP = 100;


        public DispensingX1IOClass()
        {

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            ADDRESSARRAY_INPUT = new AddressClass[INPUT_COUNT];
            ADDRESSARRAY_OUTPUT = new AddressClass[OUTPUT_COUNT];
            PLCALARMS = new PLCAlarmsClass[(int)AlarmsEnum.ALARMSCOUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {

            int i = 0;
            while (i < INPUT_COUNT)
            {
                ADDRESSARRAY_INPUT[i] = new AddressClass(ReadINIValue("Status Address", "IX" + i.ToString("000"), "", INIFILE));
                i++;
            }
            i = 0;
            while (i < OUTPUT_COUNT)
            {
                ADDRESSARRAY_OUTPUT[i] = new AddressClass(ReadINIValue("Operation Address", "QX" + i.ToString("000"), "", INIFILE));
                i++;
            }


            #region ALARMS INI

            #region 读取csv- ALARM

            string alarm0_path = INIFILE.Replace("IO.INI", "ALARMIO0.csv");
            System.IO.StreamReader _sr = null;
            try
            {
                _sr = new System.IO.StreamReader(alarm0_path);
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON] = new PLCAlarmsClass("MW0000:MX0.0,MW0001:MX2.0,MW0002:MX4.0");
                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS] = new PLCAlarmsClass("MW0003:MX6.0");
                string strRead = string.Empty;
                while (!_sr.EndOfStream)
                {
                    strRead = _sr.ReadLine();
                    string[] strs = strRead.Split(',').ToArray();
                    if (strs.Length >= 4)
                    {
                        switch (strs[0])
                        {
                            case "COMMON":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_COMMON].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                            case "SERIOUS":
                                PLCALARMS[(int)AlarmsEnum.ALARMS_ADR_SERIOUS].PLCAlarmsAddDescription("0," + strs[2] + "," + strs[3]);
                                break;
                        }
                    }
                }

                _sr.Close();
                _sr.Dispose();
                _sr = null;
            }
            catch
            {

            }

            if (_sr != null)
                _sr.Dispose();

            #endregion

            #endregion

        }

        public override void SaveData()
        {

        }

        public bool IsAlarmsSerious
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0003");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0) > 0;
            }
        }
        public bool IsAlarmsCommon
        {
            get
            {
                AddressClass address = new AddressClass("0:MW0000,MW0001");
                AddressClass address1 = new AddressClass("0:MW0002");
                return PLC[address.SiteNo].IOData.GetMW(address.Address0) > 0 || PLC[address.SiteNo].IOData.GetMW(address.Address1) > 0
                            || PLC[address1.SiteNo].IOData.GetMW(address1.Address0) > 0;
            }
        }

        public bool CLEARALARMS
        {
            get
            {
                AddressClass address = new AddressClass(IOConstClass.QB1523);
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = new AddressClass(IOConstClass.QB1523);
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }

        public bool GetAlarmsAddress(int iSiteNo, string strAddress)
        {
            return PLC[iSiteNo].IOData.GetBit(strAddress);
        }

        /// <summary>
        /// 离散输入点的总数
        /// </summary>
        public int GetInputCount
        {
            get { return ADDRESSARRAY_INPUT.Length; }
        }
        /// <summary>
        /// 离散输入点的信号
        /// </summary>
        /// <param name="eindex">点位</param>
        /// <returns></returns>
        public bool GetInputIndex(int eindex)
        {
            //get
            //{
            AddressClass address = ADDRESSARRAY_INPUT[eindex];
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            //}
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM];
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }
        /// <summary>
        /// 线圈读取
        /// </summary>
        /// <param name="eindex">点位</param>
        /// <returns></returns>
        public bool GetOutputIndex(int eindex)
        {
            //get
            //{
            AddressClass address = ADDRESSARRAY_OUTPUT[eindex];
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            //}
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)DispensingAddressEnum.ADR_UVBOTTOM];
            //    PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }
        /// <summary>
        /// 线圈设定
        /// </summary>
        /// <param name="eindex">点位</param>
        /// <param name="value">开关</param>
        public void SetOutputIndex(int eindex, bool value)
        {
            //get
            //{
            //AddressClass address = ADDRESSARRAY[eindex];
            //return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            //}
            //set
            //{
            AddressClass address = ADDRESSARRAY_OUTPUT[eindex];
            PLC[address.SiteNo].SetIO(value, address.Address0);
            //}
        }

        public void SetIO(string eAddress, bool eValue)
        {
            AddressClass address = new AddressClass(eAddress);
            PLC[address.SiteNo].SetIO(eValue, address.Address0);
        }
        public bool GetIO(string eAddress)
        {
            AddressClass address = new AddressClass(eAddress);
            return PLC[address.SiteNo].IOData.GetBit(address.Address0);
        }


        /// <summary>
        /// 设定某地址的值
        /// </summary>
        /// <param name="eAddress">地址</param>
        /// <param name="eValue">值</param>
        public void SetMWIndex(int eAddress, int eValue)
        {
            long setH = eValue >> 16;
            long setL = eValue % 65536;
            AddressClass address = new AddressClass("0:MW" + eAddress);
            PLC[address.SiteNo].SetData(ValueToHEX(setL, 4), address.Address0);
        }
        /// <summary>
        /// 获取某地址的值
        /// </summary>
        /// <param name="eAddress">地址</param>
        /// <returns>返回 值</returns>
        public int GetMWIndex(int eAddress)
        {
            AddressClass address = new AddressClass("0:MW" + eAddress);
            return PLC[address.SiteNo].IOData.GetData(address.Address0);
        }


        #region MOTOR CONTROL


        #region 模组相关操作

        #endregion

        /// <summary>
        /// 點膠1
        /// </summary>
        /// <param name="eindex"></param>
        /// <param name="epos"></param>
        public void MotorDispensing1Position(int eindex, string epos)
        {
            string[] mypos = epos.Split(',').ToArray();
            if (mypos.Length < 3)
                return;
            int ix = (int)(float.Parse(mypos[0]) * ONEMMSTEP);
            int iy = (int)(float.Parse(mypos[1]) * ONEMMSTEP);
            int iz = (int)(float.Parse(mypos[2]) * ONEMMSTEP);

            int iaddressx = 1410 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
            //PLC[addressx.SiteNo].SetData(ValueToHEX(setH, 4), addressx.Address1);

            int iaddressy = 1420 + eindex;
            AddressClass addressy = new AddressClass("0:MW" + iaddressy.ToString());

            setH = iy >> 16;
            setL = iy % 65536;

            PLC[addressy.SiteNo].SetData(ValueToHEX(setL, 4), addressy.Address0);
            //PLC[addressy.SiteNo].SetData(ValueToHEX(setH, 4), addressy.Address1);

            int iaddressz = 1430 + eindex;
            AddressClass addressz = new AddressClass("0:MW" + iaddressz.ToString());

            setH = iz >> 16;
            setL = iz % 65536;

            PLC[addressz.SiteNo].SetData(ValueToHEX(setL, 4), addressz.Address0);
            //PLC[addressz.SiteNo].SetData(ValueToHEX(setH, 4), addressz.Address1);

        }
        /// <summary>
        /// 點膠2
        /// </summary>
        /// <param name="eindex"></param>
        /// <param name="epos"></param>
        public void MotorDispensing2Position(int eindex, string epos)
        {
            string[] mypos = epos.Split(',').ToArray();
            if (mypos.Length < 3)
                return;
            int ix = (int)(float.Parse(mypos[0]) * ONEMMSTEP);
            int iy = (int)(float.Parse(mypos[1]) * ONEMMSTEP);
            int iz = (int)(float.Parse(mypos[2]) * ONEMMSTEP);

            int iaddressx = 1440 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
            //PLC[addressx.SiteNo].SetData(ValueToHEX(setH, 4), addressx.Address1);

            int iaddressy = 1450 + eindex;
            AddressClass addressy = new AddressClass("0:MW" + iaddressy.ToString());

            setH = iy >> 16;
            setL = iy % 65536;

            PLC[addressy.SiteNo].SetData(ValueToHEX(setL, 4), addressy.Address0);
            //PLC[addressy.SiteNo].SetData(ValueToHEX(setH, 4), addressy.Address1);

            int iaddressz = 1460 + eindex;
            AddressClass addressz = new AddressClass("0:MW" + iaddressz.ToString());

            setH = iz >> 16;
            setL = iz % 65536;

            PLC[addressz.SiteNo].SetData(ValueToHEX(setL, 4), addressz.Address0);
            //PLC[addressz.SiteNo].SetData(ValueToHEX(setH, 4), addressz.Address1);

        }

        /// <summary>
        /// 點膠動態位置寫入
        /// </summary>
        /// <param name="eindex">第幾個位置</param>
        /// <param name="epos">輸入位置</param>
        public void MotorDynamicPosition(int eindex,string epos)
        {
            string[] mypos = epos.Split(',').ToArray();
            if (mypos.Length < 3)
                return;
            int ix = (int)(float.Parse(mypos[0]) * ONEMMSTEP);
            int iy = (int)(float.Parse(mypos[1]) * ONEMMSTEP);
            int iz = (int)(float.Parse(mypos[2]) * ONEMMSTEP);

            int iaddressx = 1100 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
            //PLC[addressx.SiteNo].SetData(ValueToHEX(setH, 4), addressx.Address1);

            int iaddressy = 1110 + eindex;
            AddressClass addressy = new AddressClass("0:MW" + iaddressy.ToString());

            setH = iy >> 16;
            setL = iy % 65536;

            PLC[addressy.SiteNo].SetData(ValueToHEX(setL, 4), addressy.Address0);
            //PLC[addressy.SiteNo].SetData(ValueToHEX(setH, 4), addressy.Address1);

            int iaddressz = 1140 + eindex;
            AddressClass addressz = new AddressClass("0:MW" + iaddressz.ToString());

            setH = iz >> 16;
            setL = iz % 65536;

            PLC[addressz.SiteNo].SetData(ValueToHEX(setL, 4), addressz.Address0);
            //PLC[addressz.SiteNo].SetData(ValueToHEX(setH, 4), addressz.Address1);
        }

        public void MotorSinglePosition(int eAxisIndex, int eindex, string epos)
        {
            int ix = (int)(float.Parse(epos) * ONEMMSTEP);
            int iaddressx = 1100 + eAxisIndex * 10 + eindex;
            AddressClass addressx = new AddressClass("0:MW" + iaddressx.ToString());

            long setH = ix >> 16;
            long setL = ix % 65536;

            PLC[addressx.SiteNo].SetData(ValueToHEX(setL, 4), addressx.Address0);
        }

        #endregion

        public bool ADR_RED
        {
            get
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[20];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[20];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_YELLOW
        {
            get
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[21];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[21];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_GREEN
        {
            get
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[22];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[22];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool ADR_BUZZER
        {
            get
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[23];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY_OUTPUT[23];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }


        #region 定义的点位操作 MASK掉 已经通过数值去控制

        public bool ADR_ISEMC
        {
            get { return !GetInputIndex(0); }
        }
        public bool ADR_ISSCREEN
        {
            get { return !GetInputIndex(18); }
        }

        #endregion

    }

    public class DX1_IOConstClass
    {
        //注释 6初始化位置 7避光槽下 8点胶位置 9避光槽上
        //          4相机计算玻璃偏移位置 5 pick吸嘴吸料位置
        //          3放入位置


        /// <summary>
        /// 手动自动模式切换地址 0手动 1自动
        /// </summary>
        public const int MW1090 = 1090;
        /// <summary>
        /// 点胶时间 单位ms
        /// </summary>
        public const int MW1091 = 1091;
        /// <summary>
        /// UV时间 单位s
        /// </summary>
        public const int MW1092 = 1092;

        /// <summary>
        /// 清除报警
        /// </summary>
        public const string QB1523 = "0:QB1523.0";

        /// <summary>
        /// 点胶启动 ON启动 OFF完成
        /// </summary>
        public const string QB1541 = "0:QB1541.0";

        /// <summary>
        /// 吸嘴吸料和到达相机测试玻璃偏移位置启动 ON启动 OFF完成
        /// </summary>
        public const string QB1542 = "0:QB1542.0";

        /// <summary>
        /// 吸嘴吸料和到达放入位置启动 ON启动 OFF完成
        /// </summary>
        public const string QB1543 = "0:QB1543.0";

        public const string QB1625 = "0:QB1625.0";
        public const string QB1665 = "0:QB1665.0";



        /// <summary>
        /// 點膠1位置數量
        /// </summary>
        public const int MW1093 = 1093;
        /// <summary>
        /// 點膠2位置數量
        /// </summary>
        public const int MW1094 = 1094;




        /// <summary>
        /// 回原開始
        /// </summary>
        public const string QB1520 = "0:QB1520.0";
        /// <summary>
        /// 回原中
        /// </summary>
        public const string QB1521 = "0:QB1521.0";
        /// <summary>
        /// 回原完成
        /// </summary>
        public const string QB1522 = "0:QB1522.0";

        /// <summary>
        /// 停止流程
        /// </summary>
        public const string QB1524 = "0:QB1524.0";

        /// <summary>
        /// 總流程開始
        /// </summary>
        public const string QB1400 = "0:QB1400.0";
        public const string QB1401 = "0:QB1401.0";
        public const string QB1402 = "0:QB1402.0";

       
        public const string QB1410 = "0:QB1410.0";
        public const string QB1411 = "0:QB1411.0";
        public const string QB1412 = "0:QB1412.0";
        public const string QB1413 = "0:QB1413.0";
        /// <summary>
        /// 產品拍照開始
        /// </summary>
        public const string QB1414 = "0:QB1414.0";
        public const string QB1415 = "0:QB1415.0";
        public const string QB1416 = "0:QB1416.0";

        
        public const string QB1420 = "0:QB1420.0";
        public const string QB1421 = "0:QB1421.0";
        public const string QB1422 = "0:QB1422.0";
        public const string QB1423 = "0:QB1423.0";
        /// <summary>
        /// 取料流程拍照開始
        /// </summary>
        public const string QB1424 = "0:QB1424.0";
        public const string QB1425 = "0:QB1425.0";
        public const string QB1426 = "0:QB1426.0";


        /// <summary>
        /// 放料產品1開始
        /// </summary>
        public const string QB1430 = "0:QB1430.0";
        public const string QB1431 = "0:QB1431.0";
        public const string QB1432 = "0:QB1432.0";
        /// <summary>
        /// 放料產品2開始
        /// </summary>
        public const string QB1440 = "0:QB1440.0";
        public const string QB1441 = "0:QB1441.0";
        public const string QB1442 = "0:QB1442.0";
        /// <summary>
        /// 點膠1
        /// </summary>
        public const string QB1450 = "0:QB1450.0";
        public const string QB1451 = "0:QB1451.0";
        public const string QB1452 = "0:QB1452.0";
        /// <summary>
        /// 點膠2
        /// </summary>
        public const string QB1460 = "0:QB1460.0";
        public const string QB1461 = "0:QB1461.0";
        public const string QB1462 = "0:QB1462.0";
    }
}
