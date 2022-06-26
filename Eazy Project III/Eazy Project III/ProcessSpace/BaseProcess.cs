
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.BasicSpace;
using JetEazy.Drivers.Laser;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using System;
using System.Drawing;
using System.Text;
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
        protected IUV UvActuactor
        {
            get { return UV.Instance; }
        }
        protected IAxis GetAxis(int axisID)
        {
            return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[axisID];
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

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 指定紅色 會調用 NLog.Warning 其他則調用 NLog.Debug <br/>
        /// (將來有待抽離 GUI 之部分) <br/>
        /// @LETIAN: 202206
        /// </summary>
        protected void _LOG(string msg, params object[] args)
        {
            Color color = Color.Black;

            int N = args.Length;
            if (N > 0 && args[N - 1] is Color)
            {
                color = (Color)args[N - 1];
                N -= 1;
            }

            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(", ");
            sb.Append(msg);

            for (int i = 0; i < N; i++)
            {
                sb.Append(", ");
                sb.Append(args[i]);
            }

            msg = sb.ToString();
            CommonLogClass.Instance.LogMessage(msg, color);
            if (color == Color.Red)
                GdxGlobal.LOG.Warn(msg);
            else
                GdxGlobal.LOG.Debug(msg);
        }

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 會額外調用 NLog.Warning <br/>
        /// @LETIAN: 202206
        /// </summary>
        protected void _LOG(Exception ex, string msg)
        {
            msg = Name + ", " + msg;
            CommonLogClass.Instance.LogMessage(msg, Color.Red);
            GdxGlobal.LOG.Warn(ex, msg);
        }
    }
}
