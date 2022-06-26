using JetEazy.ControlSpace.MotionSpace;
using JetEazy.Drivers.IOCtrl;
using JetEazy.Drivers.IOCtrl.HCFA;
using System;



namespace JetEazy.GdxCore3.Model
{
    /// <summary>
    /// GxFacadeIO
    /// <para>負責將 Victor/Gaara IO 點位 橋接成 IoPoint</para>
    /// To be continued ...
    /// </summary>
    public class GdxFacadeIO
    {
        #region PRIVATE_MEMBERS
        private GdxFacade Facade
        {
            get { return GdxGlobal.Facade; }
        }
        #endregion

        internal GdxFacadeIO()
        {
            // 強制同模組內使用
        }

        #region PUBLIC_IO_POINTS
        /// Reserved for the future.
        object IoEMG;
        /// Reserved for the future.
        object IoLightGated;
        #endregion

        /// <summary>
        /// 將 Gaara 的 IO 點位 橋接成 IoPoint
        /// Reserved for the future.
        /// </summary>
        internal void BindIoPoints()
        {
            if (IoEMG != null)
                return;
            //var gaIO = Facade.GetIoMeta();
            //IoEMG = new GxIoPoint(new GdxAddressConvertor(gaIO.GetGaAddress("INPUT", 0)), false);
            //IoLightGated = new GxIoPoint(new GdxAddressConvertor(gaIO.GetGaAddress("INPUT", 9)), false);
            IoEMG = new object();
            // Simulation
            if (Facade.IsSimPLC())
            {
                sim_init();
            }
        }
        internal void setIO(string ioname, bool on)
        {
            var addr = GdxAddressConvertor.FromGaara(ioname);
            write_to_hardware(addr, on);
        }
        internal void setIO(IoAddress addr, bool on)
        {
            write_to_hardware(addr, on);
        }

        #region SIMULATION_FUNCTIONS
        internal void sim_axis_to_pos(int axisID, double axis_pos)
        {
            // 此處 axis_pos 是以 gaara PLCMotionClass 設定為單位.
            var motor = GdxGlobal.Facade.GetMotor(axisID);
            var ga_axis = (PLCMotionClass)motor;
            ga_axis.Go((float)axis_pos);
        }
        void OnPlcSim_WriteUint16(short[] readbuffer, string operationstring, string myname)
        {
            if (operationstring.StartsWith("SIM_"))
            {
                if (myname != "PLC0")
                    return;
                string ioname = operationstring.Split('_')[1];
                sim_update_cache_int16(0, ioname, readbuffer);
            }
        }
        void OnPlcSim_WriteIO(bool[] readbuffer, string operationstring, string myname)
        {
            if (operationstring.StartsWith("SIM_"))
            {
                if (myname != "PLC0")
                    return;
                string ioname = operationstring.Split('_')[1];
                sim_update_cache_bit(0, ioname, readbuffer);
            }
        }
        void sim_update_cache_int16(int plcID, string ioname, params short[] buf)
        {
            //@LETIAN:20220613
            // 暫時重複利用 gaara cache data (int16)
            // for MW points 

#if false
            if (!GxIoAddress.parse_gaara_str(
                    ioname,
                    out string category,
                    out int address,
                    out int offset))
                return;

            if (category != "MW")
                return;

            //> System.Diagnostics.Debug.WriteLine("$$$ update_cache: {0}{1:0000}", category, address);
            var plc = Facade.GetPLC(plcID);
            var cache = plc.IOData;

            for (int i = 0; i < buf.Length; i++)
            {
                int index = address + i;
                var value = buf[i];
                //> System.Diagnostics.Debug.WriteLine("$$$ SIM: {0}{1:0000} <= {2:X4} ({3})", category, address+i, value, value);
                GxGlobal.LOG.Debug("PLC0: {0}{1:0000}, <=, {2:X4}, {3}", category, address + i, value, value);
                cache.SetMWData(index, value);
            }
#endif

            GxIoAddress addr = GdxAddressConvertor.FromGaara(ioname, plcID);
            if (addr == null)
                return;

            var category = addr.Category;
            var address = addr.Address;
            var plc = Facade.GetPLC(plcID);
            var cache = plc.IOData;

            for (int i = 0; i < buf.Length; i++)
            {
                int index = address + i;
                var value = buf[i];
                GdxGlobal.LOG.Debug("PLC0: {0}{1:0000}, <=, {2:X4}, {3}", category, address + i, value, value);
                cache.SetMWData(index, value);
            }
        }
        void sim_update_cache_bit(int plcID, string ioname, params bool[] buf)
        {
            //@LETIAN:20220613
            // 暫時重複利用 gaara cache data (bit)
            // IX, QX, QB

            //if (!GxIoAddress.parse_gaara_str(
            //        ioname,
            //        out string category,
            //        out int address,
            //        out int offset))
            //    return;

            GxIoAddress addr = GdxAddressConvertor.FromGaara(ioname, plcID);
            if (addr == null)
                return;

            string category = addr.Category;
            int address = addr.Address;
            int offset = addr.BitOffset;
            var plc = Facade.GetPLC(plcID);
            int addressBase;
            Action<int, bool> updateFunc;

            switch (category)
            {
                case "IX":
                    addressBase = address * 8 + offset;
                    updateFunc = plc.IOData.SetIXBit;
                    break;
                case "QX":
                    addressBase = address * 8 + offset;
                    updateFunc = plc.IOData.SetQXBit;
                    break;
                case "QB":
                    addressBase = address * 8 + offset;
                    updateFunc = plc.IOData.SetQXBit;
                    break;
                default:
                    return;
            }

            for (int i = 0; i < buf.Length; i++)
            {
                int index = addressBase + i;
                var value = buf[i];
                GdxGlobal.LOG.Debug("PLC0: {0}{1}.{2}, <=, {3}", category, address, i, value);
                updateFunc(index, value);
            }
        }
        void sim_init()
        {
            //(0) 掛上 event handler 來模擬 點位寫入 plc
            var plc = Facade.GetPLC();
            plc.ReadListAction += OnPlcSim_WriteIO;
            plc.ReadListUintAction += OnPlcSim_WriteUint16;

            //(1) 清除警報
            var gaIO = Facade.GetIoMeta();
            //(1.1) 清除 急停 (normal close)
            //      IoEMG.Set(false);                
            GxIoAddress addrEMG = new GdxAddressConvertor(gaIO.GetGaAddress("INPUT", 0));
            write_to_hardware(addrEMG, true);
            //(1.2) 清除 光柵 (normal close)
            //      IoLightGated.Set(false);
            GxIoAddress addrLightGated = new GdxAddressConvertor(gaIO.GetGaAddress("INPUT", 9));
            write_to_hardware(addrLightGated, true);
        }
        #endregion


        #region INTERNAL_IO_READ_WRITE_FUNCTIONS
        internal uint read_cache_data(IoAddress addr)
        {
            var cache = Facade.GetPLC(addr.PlcID).IOData;
            if (addr.Bits == 16)
            {
                //var bitstr = addr.get_gaara_address_str();
                var bitstr = GdxAddressConvertor.ToGaaraIoCacheAddress(addr);
                uint data = QxConvert.U32(cache.GetMW(bitstr));
                return data;
            }
            else if (addr.Bits == 32)
            {
                ////var bitstrL = addr.get_gaara_address_str();
                ////var bitstrH = addr.get_gaara_address_str(true);
                var bitstrL = GdxAddressConvertor.ToGaaraIoCacheAddress(addr);
                var bitstrH = GdxAddressConvertor.ToGaaraIoCacheAddress(addr, true);
                uint dataL = QxConvert.U32(cache.GetMW(bitstrL));
                uint dataH = (bitstrH != null) ? QxConvert.U32(cache.GetMW(bitstrH)) : 0;
                uint data32 = dataL | (dataH << 16);
                return data32;
            }
            else
            {
                return 0u;
            }
        }
        internal bool read_cache_bit(IoAddress addr)
        {
            //var bitstr = addr.get_gaara_address_str();
            var bitstr = GdxAddressConvertor.ToGaaraIoCacheAddress(addr);
            var cache = Facade.GetPLC(addr.PlcID).IOData;
            return cache.GetBit(bitstr);
        }
        internal void write_to_hardware(IoAddress addr, uint value)
        {
            var plc = Facade.GetPLC(addr.PlcID);
            if (addr.Bits == 16)
            {
                var ioname = GdxAddressConvertor.ToGaaraIoCacheAddress(addr);
                string data = QxConvert.HEX(value & 0xFFFF, 4);     //ValueToHEX(value & 0xFFFF, 4);
                plc.SetData(data, ioname);
            }
            else if (addr.Bits == 32)
            {
                //var ionameL = addr.get_gaara_address_str();
                //var ionameH = addr.get_gaara_address_str(true);
                var ionameL = GdxAddressConvertor.ToGaaraIoCacheAddress(addr);
                var ionameH = GdxAddressConvertor.ToGaaraIoCacheAddress(addr, true);
                string dataL = QxConvert.HEX(value & 0xFFFF, 4);
                string dataH = QxConvert.HEX(value >> 16, 4);
                plc.SetData(dataL, ionameL);
                plc.SetData(dataH, ionameH);  // 如果 ionameH is empty , plc.SetData 會自動跳過.
            }
            else
            {
            }
        }
        internal void write_to_hardware(IoAddress addr, bool value)
        {
            //var bitstr = addr.get_gaara_address_str();
            var ioname = GdxAddressConvertor.ToGaaraIoCacheAddress(addr);
            var plc = Facade.GetPLC(addr.PlcID);
            plc.SetIO(value, ioname);
        }
        #endregion


        #region UNIT_TEST_FUNCTIONS
        public static void UnitTest()
        {
#if (DEBUG)
            var values = new int[] {
                1, -1, 1000, -1000, Int16.MaxValue, Int16.MinValue
            };

            var _ValueToHEX = new Func<long, int, string>((value, length) =>
            {
                string ret = ("00000000" + value.ToString("X")).Substring(("00000000" + value.ToString("X")).Length - length, length);
                return ret;
            });

            foreach (var v in values)
            {
                string hex_ga = _ValueToHEX(v, 4);
                string hex = QxConvert.HEX((short)v, 4);
                System.Diagnostics.Debug.WriteLine("{0} = {1}, {2}, {3}", hex, v, v & 0xFFFF, QxConvert.I16((uint)v));
                System.Diagnostics.Debug.Assert(hex == hex_ga, "格式與Gaara不相容!");
            }
#endif
        }
        #endregion
    }
}
