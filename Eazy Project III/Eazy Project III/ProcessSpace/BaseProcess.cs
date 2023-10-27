
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using System;
using System.Threading;
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
            // NextDurtimeTmp = 300;
            _initPlcEventHandlers();
        }

        #region COMMON_ACCESS_TO_THE_GLOBAL_COMPONENTS

        ICam ICamForCali
        {
            get { return Universal.CAMERAS[0]; }
        }
        ICam ICamForBlackBox
        {
            get { return Universal.CAMERAS[1]; }
        }

        protected ICam GetCamera(int camID)
        {
            return Universal.CAMERAS[camID];
        }
        protected IUV UvActuactor
        {
            get { return UV.Instance; }
        }
        protected IAxis GetAxis(int axisID)
        {
            return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[axisID];
        }
        protected IProjector ProjectorActuactor
        {
            get { return Projector.Instance; }
        }

        protected RecipeCHClass Recipe
        {
            get { return RecipeCHClass.Instance; }
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
        protected void InitDefaultDelay()
        {
            _defaultDuration = RecipeCHClass.Instance.ProcessDelay;
            _LOG("程序預設 Delay(ms)", _defaultDuration);
        }
        public override void Start(params object[] args)
        {
            //@LETIAN 20221026 啟用
            InitDefaultDelay();
            base.Start(args);
        }
        #endregion

        #region COMMON_MACHINE_FUCTIONS_FOR_STATION_3
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
        protected void Set_Cooling_Module(bool on)
        {
            MACHINE.PLCIO.SetOutputIndex((int)ControlSpace.IOSpace.DispensingAddressEnum.ADR_FINTOP, on);
            MACHINE.PLCIO.SetOutputIndex((int)ControlSpace.IOSpace.DispensingAddressEnum.ADR_FINBOTTOM, !on);
            MACHINE.PLCIO.SetOutputIndex((int)ControlSpace.IOSpace.DispensingAddressEnum.ADR_FAN, on);
            _LOG(on ? "散熱模組打開" : "散熱模組關閉");
        }
        #endregion

        #region PLC_ON_SCANNED_EVENT_HANDLER
        
        #region PRIVATE_MEMBERS
        private ManualResetEvent m_plcScanEv = new ManualResetEvent(false);
        private int m_plcScanCount = 0;
        private void _initPlcEventHandlers()
        {
            var plc = MACHINECollection.MACHINE.PLCCollection[0];
            plc.OnScanned += new EventHandler((sender, e) =>
            {
                OnPlcScanned(sender, e);
            });
        }
        #endregion

        protected virtual void OnPlcScanned(object sender, EventArgs e)
        {
            m_plcScanEv.Set();
            m_plcScanCount++;
        }

        /// <summary>
        /// 清除 Scanned 標記 
        /// </summary>
        protected override void InvalidatePlcScanned()
        {
            m_plcScanEv.Reset();
            m_plcScanCount = 0;
        }

        // <summary>
        // PLC 是否已經 有效 scanned 更新
        // </summary>
        protected bool IsValidPlcScanned(int validScannedCount = 0)
        {
            bool ok = WaitForPlcScanned(0);
            if (ok)
            {
                if (validScannedCount > 1)
                    ok = (m_plcScanCount > validScannedCount);
            }
            return ok;
        }

        /// <summary>
        /// 等待 PLC 有效 scanned 更新
        /// </summary>
        /// <param name="waitTime">ms</param>
        protected bool WaitForPlcScanned(int waitTime)
        {
            bool ok = m_plcScanEv.WaitOne(waitTime);
            return ok;
        }

        /// <summary>
        /// 放棄等待 PLC 更新 <br/>
        /// @LETIAN: 20221022 搭配 WaitForPlcScanned 使用
        /// </summary>
        protected void AbortWaitPlcScanned()
        {
            m_plcScanCount = 1000;
            m_plcScanEv.Set();
        }
        #endregion

        #region COMMON_LOG_FUNCTIONS

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 指定紅色 會調用 NLog.Warning 其他則調用 NLog.Debug <br/>
        /// (將來有待抽離 GUI 之部分) <br/>
        /// @LETIAN: 202206
        /// </summary>
        protected void _LOG(string msg, params object[] args)
        {
#if (false)
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
#endif
            msg = Name + ", " + msg;
            GdxGlobal.LOG.Log(msg, args);
        }

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 會額外調用 NLog.Warning <br/>
        /// @LETIAN: 202206
        /// </summary>
        protected void _LOG(Exception ex, string msg)
        {
#if (false)
            msg = Name + ", " + msg;
            CommonLogClass.Instance.LogMessage(msg, Color.Red);
            GdxGlobal.LOG.Warn(ex, msg);
#endif
            msg = Name + ", " + msg;
            GdxGlobal.LOG.Log(ex, msg);
        }

        #endregion
    }
}
