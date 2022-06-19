
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.ProcessSpace;
using VsCommon.ControlSpace;


namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 中光電 station III processes 共用之 abstract class <br/>
    /// (1) 只與 station III 設備 (model) 相依 <br/>
    /// (2) 抽離所有 GUI 元件 <br/>
    /// (3) CommonLogClass 也有 GUI 相依的部分, 將來也要抽離. <br/>
    /// @LETIAN: 20220619 creation
    /// </summary>
    public abstract class BaseProcess : AbsProcess
    {
        public BaseProcess()
        {
            // 300 ms
            NextDurtimeTmp = 300;
        }

        #region COMMON_ACCESS_TO_THE_GLOBAL_COMPONENTS

        protected ICam ICamForCali
        {
            get { return Universal.CAMERAS[0]; }
        }

        protected ICam ICamForBlackBox
        {
            get { return Universal.CAMERAS[1]; }
        }

        protected IUV m_UV
        {
            get { return UV.Instance; }
        }

        protected IAxis IAXIS_0
        {
            get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[0]; }
        }

        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        protected DispensingMachineClass MACHINE
        {
            get { return (DispensingMachineClass)Universal.MACHINECollection.MACHINE; }
        }

        #endregion

        #region COMMON_DATA_FOR_STATION_3
        // 為了相容以前的舊 Tick 內容
        protected int NextDurtimeTmp
        {
            get { return _defaultDuration; }
            set { _defaultDuration = value; }
        }
        #endregion

        #region COMMON_OPERATION_FUCTIONS_FOR_STATION_3
        protected void SetNormalLight()
        {
            MACHINE.PLCIO.ADR_RED = false;
            MACHINE.PLCIO.ADR_YELLOW = true;
            MACHINE.PLCIO.ADR_GREEN = false;
        }
        protected void SetAbnormalLight()
        {
            MACHINE.PLCIO.ADR_RED = true;
            MACHINE.PLCIO.ADR_YELLOW = false;
            MACHINE.PLCIO.ADR_GREEN = false;
        }
        protected void SetRunningLight()
        {
            MACHINE.PLCIO.ADR_RED = false;
            MACHINE.PLCIO.ADR_YELLOW = false;
            MACHINE.PLCIO.ADR_GREEN = true;
        }
        #endregion
    }
}
