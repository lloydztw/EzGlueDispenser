﻿using Eazy_Project_III.ControlSpace.IOSpace;
using JetEazy.BasicSpace;
using System.Drawing;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// INIT流程 即初始化流程 所有轴在手动模式下归位 归位完成后 运动至各轴初始化位置 <br/>
    /// @LETIAN: 20220619 重新包裝
    /// </summary>
    public class ResetProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        #endregion

        #region SINGLETON
        static ResetProcess _singleton = null;
        private ResetProcess()
        {
        }
        #endregion

        public static ResetProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new ResetProcess();
                return _singleton;
            }
        }

        public override void Tick()
        {
            if (!IsValidPlcScanned())
                return;

            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        SetRunningLight();

                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 6, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));

                        //U AXIS 後退的距離
                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 4, INI.Instance.sMirrorAdjBackLength.ToString() + ",0,0");

                        CommonLogClass.Instance.LogMessage("模组初始化位置设定", Color.Black);

                        Process.NextDuriation = 2000;
                        Process.ID = 10;

                        MACHINE.PLCIO.SetOutputIndex((int)DispensingAddressEnum.ADR_RESET_START, true);
                        CommonLogClass.Instance.LogMessage("所有轴复位中", Color.Black);

                        m_Stopwatch.Restart();

                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                            {
                                m_Stopwatch.Stop();
                                //CommonLogClass.Instance.LogMessage("所有轴复位完成", Color.Lime);
                                //Process.Stop();

                                //SetNormalLight();

                                //m_BuzzerIndex = 0;
                                //m_BuzzerCount = 1;//复位完成叫一声

                                switch (Process.RelateString)
                                {
                                    case "CloseWindows":
                                        break;
                                    default:
                                        m_BuzzerProcess.Start(1);
                                        break;
                                }

                                Set_Cooling_Module(false);
                                Process.NextDuriation = 100;
                                Process.ID = 20;

                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK));
                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 1, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_DISPENSING));
                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_ADJUST));

                                //CommonLogClass.Instance.LogMessage("模组初始化位置设定", Color.Black);
                            }
                            else if (m_Stopwatch.ElapsedMilliseconds >= 60 * 1000)
                            {
                                m_Stopwatch.Stop();
                                //Time out
                                Process.Stop();
                                switch (Process.RelateString)
                                {
                                    case "CloseWindows":
                                        break;
                                    default:
                                        CommonLogClass.Instance.LogMessage("所有轴复位超時", Color.Red);
                                        m_BuzzerProcess.Start(3);
                                        Set_Cooling_Module(false);
                                        SetAbnormalLight();
                                        MACHINE.PLCIO.ADR_STOP_PLC_SIGN = true;
                                        break;
                                }
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //if (!m_BuzzerProcess.IsOn)
                            {
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("所有轴复位完成", Color.Black);
                                SetNormalLight();
                                FireCompleted();
                            }
                        }
                        break;
                        //case 20:
                        //    if (Process.IsTimeup)
                        //    {
                        //        //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                        //        {

                        //            Process.NextDuriation = 500;
                        //            Process.ID = 30;

                        //            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                        //            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_DISPENSING, 1);
                        //            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_ADJUST, 1);

                        //            CommonLogClass.Instance.LogMessage("模组开始启动", Color.Red);
                        //        }
                        //    }
                        //    break;
                        //case 30:
                        //    if (Process.IsTimeup)
                        //    {
                        //        if ((MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1)
                        //            && MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_DISPENSING, 1)
                        //            //&& MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_ADJUST, 1)  //目前没有微调模组 不判断
                        //            )
                        //            || Universal.IsNoUseIO)
                        //        {
                        //            CommonLogClass.Instance.LogMessage("模组定位完成", Color.Lime);
                        //            Process.Stop();
                        //        }
                        //    }
                        //    break;
                }
            }
        }
    }
}
