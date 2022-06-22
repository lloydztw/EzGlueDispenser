using Eazy_Project_III.ControlSpace.IOSpace;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.ControlSpace.PLCSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsCommon.ControlSpace.MachineSpace;

namespace Eazy_Project_III.ControlSpace.MachineSpace
{
    public class DispensingMachineClass : GeoMachineClass
    {
        const int MSDuriation = 10;

        public DispensingIOClass PLCIO;
        public EventClass EVENT;

        public DispensingMachineClass(Machine_EA machineea, string opstr, string workpath, bool isnouseplc)
        {
            IsNoUseIO = isnouseplc;

            myMachineEA = machineea;
            //VERSION = version;
            //OPTION = option;

            WORKPATH = workpath;

            GetOPString(opstr);

            MainProcess = new ProcessClass();

            myJzTimes = new JzTimes();
            myJzTimes.Cut();
        }
        public override void GetOPString(string opstr)
        {
            string[] strs = opstr.Split(',');

            PLCCount = int.Parse(strs[0]);
            MotionCount = int.Parse(strs[1]);

            if (PLCCount > 0)
                PLCCollection = new VsCommPLC[PLCCount];

            //if (PLCCount > 0)
            //    PLCCollection = new FatekPLCClass[PLCCount];

            if (MotionCount > 0)
                PLCMOTIONCollection = new PLCMotionClass[MotionCount];
        }
        public override bool Initial(bool isnouseio, bool isnousemotor)
        {
            int i = 0;
            bool ret = true;

            IsNoUseIO = isnouseio;
            IsNoUseMotor = isnousemotor;

            i = 0;
            while (i < PLCCount)
            {
                PLCCollection[i] = new VsCommPLC();

                //@LETIAN: for off-line simulation
                if (true || !isnouseio)
                    ret &= PLCCollection[i].Open(WORKPATH + "\\" + myMachineEA.ToString() + "\\PLCCONTROL" + i.ToString() + ".INI", isnouseio);

                PLCCollection[i].Name = "PLC" + i.ToString();
                PLCCollection[i].ReadAction += ReadAction;
                PLCCollection[i].ReadListAction += DispensingMachineClass_ReadListAction;
                PLCCollection[i].ReadListUintAction += DispensingMachineClass_ReadListUintAction;

                i++;
            }

            i = 0;
            while (i < MotionCount)
            {
                PLCMOTIONCollection[i] = new PLCMotionClass();
                PLCMOTIONCollection[i].Intial(WORKPATH + "\\" + myMachineEA.ToString(), (MotionEnum)i, PLCCollection, IsNoUseMotor);

                i++;
            }

            PLCIO = new DispensingIOClass();
            PLCIO.Initial(WORKPATH + "\\" + myMachineEA.ToString(), PLCCollection);

            EVENT = new EventClass(WORKPATH + "\\EVENT.jdb");

            return ret;
        }

        private void DispensingMachineClass_ReadListUintAction(short[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(short[] readbuffer, string operationstring)
        {
            switch (operationstring)
            {
                case "Get MW0":
                    PLC0GetMW0000(readbuffer);
                    break;
                case "Get MW1000":
                    PLC0GetMW1000(readbuffer);
                    break;
                case "Get MW1300":
                    PLC0GetMW1300(readbuffer);
                    break;
                case "Get MW1320":
                    PLC0GetMW1320(readbuffer);
                    break;
            }
        }
        void PLC0GetMW0000(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(0 + i, ison);

                //UInt32 GetInt = ison;
                int j = 0;
                while (j < 16)
                {
                    bool isonj = ((ison >> j) % 2) == 1;

                    PLCCollection[0].IOData.SetMXBit(0 + i * 16 + j, isonj);

                    j++;
                }

                i++;
            }
        }
        void PLC0GetMW1000(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(1000 + i, ison);

                i++;
            }
        }
        void PLC0GetMW1300(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(1300 + i, ison);

                i++;
            }
        }
        void PLC0GetMW1320(short[] readbuffer)
        {
            int i = 0;
            while (i < readbuffer.Length)
            {
                short ison = readbuffer[i];
                PLCCollection[0].IOData.SetMWData(1320 + i, ison);

                i++;
            }
        }

        private void DispensingMachineClass_ReadListAction(bool[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(bool[] readbuffer, string operationstring)
        {
            switch (operationstring)
            {
                case "Get QX0":
                    PLC0GetQX0(readbuffer);
                    break;
                case "Get QX1016":
                    PLC0GetQX1016(readbuffer);
                    break;
                case "Get QX1144":
                    PLC0GetQXFX(1144,readbuffer);
                    break;
                case "Get QX1272":
                    PLC0GetQXFX(1272, readbuffer);
                    break;
                case "Get QX1400":
                    PLC0GetQXFX(1400, readbuffer);
                    break;
                case "Get QX1528":
                    PLC0GetQXFX(1528, readbuffer);
                    break;
                case "Get QX1656":
                    PLC0GetQXFX(1656, readbuffer);
                    break;
                case "Get QX1784":
                    PLC0GetQXFX(1784, readbuffer);
                    break;
                case "Get IX0":
                    PLC0GetIX0(readbuffer);
                    break;
            }
        }
        void PLC0GetQX0(bool[] readbuffer)
        {

            int i = 0;
            while (i < readbuffer.Length)
            {
                bool ison = readbuffer[i];
                PLCCollection[0].IOData.SetQXBit(0 * 8 + i, ison);

                i++;
            }
        }
        void PLC0GetQX1016(bool[] readbuffer)
        {

            int i = 0;
            while (i < readbuffer.Length)
            {
                bool ison = readbuffer[i];
                PLCCollection[0].IOData.SetQXBit(1016 * 8 + i, ison);

                i++;
            }
        }
        void PLC0GetQXFX(int iaddress, bool[] readbuffer)
        {

            int i = 0;
            while (i < readbuffer.Length)
            {
                bool ison = readbuffer[i];
                PLCCollection[0].IOData.SetQXBit(iaddress * 8 + i, ison);

                i++;
            }
        }
        void PLC0GetIX0(bool[] readbuffer)
        {

            int i = 0;
            while (i < readbuffer.Length)
            {
                bool ison = readbuffer[i];
                PLCCollection[0].IOData.SetIXBit(0 * 8 + i, ison);

                i++;
            }
        }

        private void ReadAction(char[] readbuffer, string operationstring, string myname)
        {
            switch (myname)
            {
                case "PLC0":
                    PLC0ReadAction(readbuffer, operationstring);
                    break;
            }
        }
        void PLC0ReadAction(char[] readbuffer, string operationstring)
        {
            switch (operationstring)
            {
                case "Get All M":
                    PLC0GetAllMEX(readbuffer);
                    break;
                case "Get All X":
                    PLC0GetAllX(readbuffer);
                    break;
                case "Get All Y":
                    PLC0GetAllY(readbuffer);
                    break;
            }
        }
        void PLC0GetAllX(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 10); //X0000

            UInt32 GetInt = HEX32(Str);
            int i = 0;
            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetXBit(0 + i, ison);

                i++;
            }

            //UInt32 GetInt = HEX32(Str.Substring(0, 4));
            //int i = 0;

            //while (i < Str.Length)
            //{
            //    bool ison = Str.Substring(i, 1) == "1";

            //    PLCCollection[0].IOData.SetXBit(i, ison);

            //    i++;
            //}
        }
        void PLC0GetAllY(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 10); //Y0000
            UInt32 GetInt = HEX32(Str);
            // string Yio = Convert.ToString(GetInt, 2);
            int i = 0;
            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetYBit(0 + i, ison);

                i++;
            }

            //UInt32 GetInt = HEX32(Str.Substring(0, 4));
            //int i = 0;

            //while (i < Str.Length)
            //{
            //    //bool ison = (GetInt & (1 << i)) == (1 << i);
            //    bool ison = Str.Substring(i, 1) == "1";

            //    PLCCollection[0].IOData.SetYBit(i, ison);

            //    i++;
            //}
        }
        void PLC0GetAllMEX(char[] readbuffer)
        {
            String Str = new string(readbuffer, 6, 8); //M0048

            UInt32 GetInt = HEX32(Str);
            int i = 0;

            while (i < 32)
            {
                bool ison = ((GetInt >> i) % 2) == 1;

                PLCCollection[0].IOData.SetMBit(48 + i, ison);

                i++;
            }

            //Str = new string(readbuffer, 14, 8); //M00
            //GetInt = HEX32(Str);
            //i = 0;

            //while (i < 32)
            //{
            //    bool ison = ((GetInt >> i) % 2) == 1;

            //    PLCCollection[0].IOData.SetMBit(1040 + i, ison);
            //    i++;
            //}

            //Str = new string(readbuffer, 22, 8); //M1200
            //GetInt = HEX32(Str);
            //i = 0;

            //while (i < 32)
            //{
            //    bool ison = ((GetInt >> i) % 2) == 1;

            //    PLCCollection[0].IOData.SetMBit(1200 + i, ison);
            //    i++;
            //}

        }
        public override void Tick()
        {
            if (myJzTimes.msDuriation < MSDuriation)
                return;

            CheckEvent();

            myJzTimes.Cut();
        }

        public override void GoHome()
        {
        }


        #region Alarms Define


        bool AlarmSeriousTrigered = false;
        bool AlarmSeriousnow = false;
        public bool IsAlarmSerious
        {
            get
            {
                return AlarmSeriousnow;
            }
            set
            {
                if (AlarmSeriousnow != value)
                {
                    if (value)
                        AlarmSeriousTrigered = true;
                    else
                        AlarmSeriousTrigered = false;

                    AlarmSeriousnow = value;
                }
                else
                    AlarmSeriousTrigered = false;
            }
        }

        bool AlarmCommonTrigered = false;
        bool AlarmCommonnow = false;
        public bool IsAlarmCommon
        {
            get
            {
                return AlarmCommonnow;
            }
            set
            {
                if (AlarmCommonnow != value)
                {
                    if (value)
                        AlarmCommonTrigered = true;
                    else
                        AlarmCommonTrigered = false;

                    AlarmCommonnow = value;
                }
                else
                    AlarmCommonTrigered = false;
            }
        }

        bool EMCTrigered = false;
        bool IsEMCnow = false;
        public bool IsEMC
        {
            get
            {
                return IsEMCnow;
            }
            set
            {
                if (IsEMCnow != value)
                {
                    if (value)
                        EMCTrigered = true;
                    else
                        EMCTrigered = false;

                    IsEMCnow = value;
                }
                else
                    EMCTrigered = false;
            }
        }


        public bool ClearAlarm
        {
            set
            {
                if (value)
                {
                    AlarmSeriousnow = false;
                    AlarmCommonnow = false;
                }
                PLCIO.CLEARALARMS = value;
            }
        }

        #endregion

        public override void CheckEvent()
        {
            IsAlarmSerious = PLCIO.IsAlarmsSerious;
            if (AlarmSeriousTrigered)
            {
                OnTrigger(MachineEventEnum.ALARM_SERIOUS);
            }

            IsAlarmCommon = PLCIO.IsAlarmsCommon;
            if (AlarmCommonTrigered)
            {
                OnTrigger(MachineEventEnum.ALARM_COMMON);
            }

            IsEMC = PLCIO.ADR_ISEMC || PLCIO.ADR_ISSCREEN;
            if (EMCTrigered)
            {
                OnTrigger(MachineEventEnum.EMC);
            }

            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.Tick();
            }
        }
        public override void GetStart(bool isdirect, bool isnouseplc)
        {
            throw new NotImplementedException();
        }
        public override void SetDelayTime()
        {
            throw new NotImplementedException();
        }
        public override void MainProcessTick()
        {
            throw new NotImplementedException();
        }
        public void PLCRetry()
        {
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.RetryConn();
            }
        }

        public override void Close()
        {
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.Close();
            }
        }
        public override string PLCFps()
        {
            string str = string.Empty;
            foreach (VsCommPLC plc in PLCCollection)
            {
                str += plc.SerialCount.ToString() + ",";
            }
            return str;
        }

        public override void SetNormalTemp(bool ebTemp)
        {
            foreach (VsCommPLC plc in PLCCollection)
            {
                plc.SetNormalTemp(ebTemp);
            }
        }

    }
}
