using JetEazy.Drivers.IOCtrl;
using JetEazy.Drivers.IOCtrl.HCFA;
using JetEazy.GdxCore3.Model;
using System;
using GaaraAddress = JetEazy.ControlSpace.AddressClass;


namespace JetEazy.GdxCore3
{
    /// <summary>
    /// GxIoConvertor
    /// 負責 Gaara IO Address 轉換成 GxIoAddress
    /// </summary>
    class GdxAddressConvertor
    {
        #region PRIVATE_DATA
        GxIoAddress m_ioAddress;
        #endregion

        #region PUBLIC_隱式自動轉換
        public GdxAddressConvertor(GaaraAddress addr)
        {
            m_ioAddress = FromGaara(addr);
        }
        public static implicit operator GxIoAddress(GdxAddressConvertor thisObj)
        {
            return thisObj.m_ioAddress;
        }
        #endregion

        public static GxIoAddress FromGaara(GaaraAddress src)
        {
            GxIoAddress addr = FromGaara(src.Address0, src.SiteNo);

            // 暫時不處理 Gaara 2nd Address
            if (false && !string.IsNullOrEmpty(src.Address1))
            {
                var addr1 = FromGaara(src.Address1);
                if (addr1 != null)
                {
                    //System.Diagnostics.Debug.Assert(category == cateH, "高位址有誤!");
                    //System.Diagnostics.Debug.Assert(addrH == address + 1, "高位址必須連續!");
                    //System.Diagnostics.Debug.WriteLine(offsetH);
                }
            }

            return addr;
        }
        public static GxIoAddress FromGaara(string gaara_ioname, int plcID = 0)
        {
            if (gaara_ioname.Contains(":"))
            {
                var strs = gaara_ioname.Split(':');
                if (strs.Length >= 2 && plcID == 0)
                {
                    int.TryParse(strs[0], out plcID);
                    gaara_ioname = strs[1].Trim();
                }
            }

            if (!parse_gaara_str(
                gaara_ioname,
                out string category,
                out int address,
                out int offset))
                return null;

            int bits = GxIoAddress.GetAddressingBits(category);
            if (bits == 0)
                return null;

            return new GxIoAddress(category, address, offset, plcID, bits);
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 解析 Gaara bitstr 字串
        /// </summary>
        static bool parse_gaara_str(string bitstr, out string category, out int address, out int offset)
        {
            category = null;
            address = 0;
            offset = 0;
            if (string.IsNullOrEmpty(bitstr))
                return false;

            try
            {
                category = bitstr.Substring(0, 2).ToUpper();
                string[] strs = bitstr.Substring(2).Split('.');
                if (strs.Length >= 2)
                {
                    address = int.Parse(strs[0]);
                    offset = int.Parse(strs[1]);
                }
                else
                {
                    address = int.Parse(strs[0]);
                }
                return true;
            }
            catch(Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "Gaara bitstr 字串格式有誤!");
                return false;
            }
        }
        #endregion

        /// <summary>
        /// 轉換成 Gaara IO點位 快取內存 (cache data) 的定址字串
        /// </summary>
        public static string ToGaaraIoCacheAddress(IoAddress addr, bool high_16bits = false)
        {
            if (high_16bits)
            {
                if (addr.Bits == 32)
                    //> return _ga_addr.Address1;
                    return string.Format("{0}{1}.{2}", addr.Category, addr.Address + 1, addr.BitOffset);
                else
                    return null;
            }
            else
            {
                //> return _ga_addr.Address0;
                return string.Format("{0}{1}.{2}", addr.Category, addr.Address, addr.BitOffset);
            }
        }
    }
}
