using Eazy_Project_III;
using Eazy_Project_III.OPSpace;
using JetEazy;
using JetEazy.ControlSpace.MotionSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsCommon.ControlSpace.MachineSpace;

namespace VsCommon.ControlSpace
{


    public class MachineCollectionClass
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        public GeoMachineClass MACHINE;
        public MachineCollectionClass()
        {

        }

        public void Intial(VersionEnum version,OptionEnum option, GeoMachineClass machine)
        {
            VERSION = version;
            OPTION = option;

            MACHINE = machine;

            MotorSpeed();


            SetIniPara();

            MACHINE.TriggerAction += MACHINE_TriggerAction;
        }

        private void MACHINE_TriggerAction(MachineEventEnum machineevent)
        {
            OnTrigger(machineevent);
        }


        public void MotorSpeed()
        {
            foreach (PLCMotionClass MOTION in MACHINE.PLCMOTIONCollection)
            {
                MOTION.SetSpeed(SpeedTypeEnum.HOMESLOW);
                MOTION.SetSpeed(SpeedTypeEnum.HOMEHIGH);
                MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
                MOTION.SetSpeed(SpeedTypeEnum.GO);
            }
        }
        /// <summary>
        /// 設定單軸的速度類型
        /// </summary>
        /// <param name="iMotorIndex">軸編號0-8 吸嘴XYZ 微調U thetay thetaz 點膠XYZ</param>
        /// <param name="eSpeedType">速度類型</param>
        public void SetSingleMotorSpeed(int iMotorIndex,SpeedTypeEnum eSpeedType)
        {
            if (iMotorIndex < 0 || iMotorIndex >= MACHINE.PLCMOTIONCollection.Length)
                return;
            MACHINE.PLCMOTIONCollection[iMotorIndex].SetSpeed(eSpeedType);
        }


        #region 設定INI的相關資料

        Eazy_Project_III.ControlSpace.MachineSpace.DispensingX1MachineClass machineX1
        {
            get { return (Eazy_Project_III.ControlSpace.MachineSpace.DispensingX1MachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE; }
        }

        Eazy_Project_III.ControlSpace.MachineSpace.DispensingMachineClass machineX3
        {
            get { return (Eazy_Project_III.ControlSpace.MachineSpace.DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE; }
        }

        public void SetIniPara()
        {
            switch(VERSION)
            {
                case VersionEnum.PROJECT:

                    switch(OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            //初始化位置 固定6 
                            machineX3.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, Eazy_Project_III.Universal.MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK),false);
                            machineX3.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 6, Eazy_Project_III.Universal.MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING), false);
                            machineX3.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 6, Eazy_Project_III.Universal.MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST), false);

                            //设定点胶时间&UV时间
                            machineX3.PLCIO.SetMWIndex(Eazy_Project_III.ControlSpace.IOSpace.IOConstClass.MW1091, Eazy_Project_III.OPSpace.RecipeCHClass.Instance.DispensingTime);
                            machineX3.PLCIO.SetMWIndex(Eazy_Project_III.ControlSpace.IOSpace.IOConstClass.MW1092, RecipeCHClass.Instance.UVTime);
                            machineX3.PLCIO.ADR_UPDELAYTIME = INI.Instance.DispensingUpDelayTime;
                            //避光槽位置 9上 7下
                            machineX3.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 9, INI.Instance.ShadowPosUp, false);
                            machineX3.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 7, INI.Instance.ShadowPos, false);
                            machineX3.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 4, INI.Instance.sMirrorAdjBackLength.ToString() + ",0,0", false);

                            break;

                        case OptionEnum.DISPENSINGX1:


                            //初始化位置 固定6

                            int ix = 0;
                            while (ix < Eazy_Project_III.Universal.MACHINECollection.GetMotorCount())
                            {
                                machineX1.PLCIO.MotorSinglePosition(ix, 6, Eazy_Project_III.Universal.MACHINECollection.GetSingleAXISPositionForReady(ix));
                                ix++;
                            }

                            //寫入INI的位置
                            machineX1.PLCIO.MotorSinglePosition(2, 1, INI.Instance.GetPos1.ToString());
                            machineX1.PLCIO.MotorSinglePosition(2, 2, INI.Instance.GetPos2.ToString());
                            machineX1.PLCIO.MotorSinglePosition(2, 3, INI.Instance.PutPos1.ToString());
                            machineX1.PLCIO.MotorSinglePosition(2, 4, INI.Instance.PutPos2.ToString());
                            machineX1.PLCIO.MotorSinglePosition(3, 1, INI.Instance.UVWorkPos.ToString());

                            //待命位置寫入
                            machineX1.PLCIO.MotorDynamicPosition(5, INI.Instance.SafePosReady.ToString());
                            machineX1.PLCIO.MotorDynamicPosition(7, INI.Instance.DispendingPosReady.ToString());

                            //寫入點膠位置數據

                            machineX1.PLCIO.SetMWIndex(Eazy_Project_III.ControlSpace.IOSpace.DX1_IOConstClass.MW1093, INI.Instance.DispensingX1_1PosList.Count);
                            machineX1.PLCIO.SetMWIndex(Eazy_Project_III.ControlSpace.IOSpace.DX1_IOConstClass.MW1094, INI.Instance.DispensingX1_2PosList.Count);

                            int dispensingindex = 0;
                            foreach (string str in INI.Instance.DispensingX1_1PosList)
                            {
                                machineX1.PLCIO.MotorDispensing1Position(dispensingindex, str);
                                dispensingindex++;
                            }
                            dispensingindex = 0;
                            foreach (string str in INI.Instance.DispensingX1_2PosList)
                            {
                                machineX1.PLCIO.MotorDispensing2Position(dispensingindex, str);
                                dispensingindex++;
                            }


                            break;
                    }

                    break;
            }
        }

        #endregion



        #region 模组相关操作



        /// <summary>
        /// 获取模组的位置
        /// </summary>
        /// <param name="eModuleIndex">模组名称</param>
        /// <returns>返回格式 X,Y,Z </returns>
        public string GetModulePosition(ModuleName eModuleIndex)
        {
            string posstr = string.Empty;

            switch(VERSION)
            {
                case VersionEnum.PROJECT:

                    switch(OPTION)
                    {
                        case OptionEnum.DISPENSINGX1:

                            #region 第一站
                            switch (eModuleIndex)
                            {
                                case ModuleName.MODULE_DISPENSING:
                                    posstr += MACHINE.PLCMOTIONCollection[0].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[1].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[4].PositionNowString;
                                    break;
                            }
                            #endregion

                            break;
                        case OptionEnum.DISPENSING:

                            #region 第三站
                            switch (eModuleIndex)
                            {
                                case ModuleName.MODULE_PICK:
                                    posstr += MACHINE.PLCMOTIONCollection[0].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[1].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[2].PositionNowString;
                                    break;
                                case ModuleName.MODULE_DISPENSING:
                                    posstr += MACHINE.PLCMOTIONCollection[3].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[4].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[5].PositionNowString;
                                    break;
                                case ModuleName.MODULE_ADJUST:
                                    posstr += MACHINE.PLCMOTIONCollection[6].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[7].PositionNowString + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[8].PositionNowString;
                                    break;
                            }
                            #endregion

                            break;
                    }

                    break;
            }

            

            return posstr;
        }

        /// <summary>
        /// 获取模组待命位置
        /// </summary>
        /// <param name="eModuleIndex">模组名称</param>
        /// <returns>返回格式 X,Y,Z</returns>
        public string GetModulePositionForReady(ModuleName eModuleIndex)
        {
            string posstr = string.Empty;

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSINGX1:

                            #region 第一站
                            switch (eModuleIndex)
                            {
                                case ModuleName.MODULE_DISPENSING:
                                    posstr += MACHINE.PLCMOTIONCollection[0].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[1].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[4].READYPOSITION;
                                    break;
                            }
                            #endregion

                            break;
                        case OptionEnum.DISPENSING:

                            #region 第三站
                            switch (eModuleIndex)
                            {
                                case ModuleName.MODULE_PICK:
                                    posstr += MACHINE.PLCMOTIONCollection[0].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[1].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[2].READYPOSITION;
                                    break;
                                case ModuleName.MODULE_DISPENSING:
                                    posstr += MACHINE.PLCMOTIONCollection[3].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[4].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[5].READYPOSITION;
                                    break;
                                case ModuleName.MODULE_ADJUST:
                                    posstr += MACHINE.PLCMOTIONCollection[6].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[7].READYPOSITION + ",";
                                    posstr += MACHINE.PLCMOTIONCollection[8].READYPOSITION;
                                    break;
                            }
                            #endregion

                            break;
                    }

                    break;
            }

            

            return posstr;
        }
        /// <summary>
        /// 判断微调模组X轴 待命OK
        /// </summary>
        /// <returns>true: 到达 false: 没到达</returns>
        public bool AdjustReadyPositionOK()
        {
            return IsAxisCurrentPositionCheckOK(MACHINE.PLCMOTIONCollection[6], MACHINE.PLCMOTIONCollection[6].READYPOSITION);
        }

        /// <summary>
        /// 判断轴当前位置与设定位置 一致 即是否到达正确位置
        /// </summary>
        /// <param name="eAxis">轴号</param>
        /// <param name="eSetPosition">设定位置</param>
        /// <returns></returns>
        bool IsAxisCurrentPositionCheckOK(PLCMotionClass eAxis, float eSetPosition)
        {

            return IsInRange(eAxis.PositionNow, eSetPosition, 0.005f);

            //return false;
        }

        bool IsInRange(float FromValue, float CompValue, float DiffValue)
        {
            return Math.Abs(FromValue - CompValue) < DiffValue;
        }


        public string GetSingleAXISPositionForReady(int eIndex)
        {
            string posstr = MACHINE.PLCMOTIONCollection[eIndex].READYPOSITION.ToString();
            return posstr;
        }
        /// <summary>
        /// 單個軸的當前位置
        /// </summary>
        /// <param name="eIndex">軸號  從0開始</param>
        /// <returns></returns>
        public string GetSingleAXISPositionNow(int eIndex)
        {
            string posstr = MACHINE.PLCMOTIONCollection[eIndex].PositionNow.ToString();
            return posstr;
        }


        ///// <summary>
        ///// 模组1位置xyz 
        ///// </summary>
        ///// <returns>x,y,z</returns>
        //public string GetPosition1XYZ()
        //{
        //    string posstr = "";

        //    posstr += MACHINE.PLCMOTIONCollection[0].PositionNowString + ",";
        //    posstr += MACHINE.PLCMOTIONCollection[1].PositionNowString + ",";
        //    posstr += MACHINE.PLCMOTIONCollection[2].PositionNowString;

        //    return posstr;

        //}
        ///// <summary>
        ///// 模组2位置xyz 
        ///// </summary>
        ///// <returns>x,y,z</returns>
        //public string GetPosition2XYZ()
        //{
        //    string posstr = "";

        //    posstr += MACHINE.PLCMOTIONCollection[3].PositionNowString + ",";
        //    posstr += MACHINE.PLCMOTIONCollection[4].PositionNowString + ",";
        //    posstr += MACHINE.PLCMOTIONCollection[5].PositionNowString;

        //    return posstr;

        //}
        ///// <summary>
        ///// 模组3位置xyz 
        ///// </summary>
        ///// <returns>x,y,z</returns>
        //public string GetPosition3XYZ()
        //{
        //    string posstr = "";

        //    posstr += MACHINE.PLCMOTIONCollection[6].PositionNowString + ",";
        //    posstr += MACHINE.PLCMOTIONCollection[7].PositionNowString + ",";
        //    posstr += MACHINE.PLCMOTIONCollection[8].PositionNowString;

        //    return posstr;

        //}

        #endregion

        public int GetMotorCount()
        {
            return MACHINE.PLCMOTIONCollection.Length;
        }


        public string GetPosition()
        {
            string posstr = "";


            return posstr;

        }
        public void GoPosition(string opstr)
        {

        }

        public void GoHome()
        {
        }

        public void Close()
        {
            MACHINE.Close();
        }
        public string PLCFps()
        {
            return MACHINE.PLCFps();
        }

        public void Tick()
        {
            MACHINE.Tick();
        }

        public delegate void TriggerHandler(MachineEventEnum machineevent);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(MachineEventEnum machineevent)
        {
            if (TriggerAction != null)
            {
                TriggerAction(machineevent);
            }
        }


    }
}
