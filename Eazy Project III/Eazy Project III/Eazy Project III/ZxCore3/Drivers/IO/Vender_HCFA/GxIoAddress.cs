namespace JetEazy.Drivers.IOCtrl.HCFA
{
    /// <summary>
    /// 禾川 PLC 定址
    /// </summary>
    public class GxIoAddress : IoAddress
    {
        public GxIoAddress(string cate, int address, int offset = 0, int plcID = 0, int bits = 0)
        {
            PlcID = (byte)plcID;
            Category = cate;
            Address = (ushort)address;
            BitOffset = (byte)offset;
            if (bits != 1 || bits != 8 || bits != 16 || bits != 32)
                Bits = GetAddressingBits(cate);
        }

        public string Name
        {
            get { return ToString(); }
        }
        public byte PlcID
        {
            get;
            private set;
        }
        public string Category
        {
            //> get { return _ga_addr.Address0.Substring(0, 2).ToUpper(); }
            get;
            private set;
        }
        public ushort Address
        {
            get;
            private set;
        }
        public byte BitOffset
        {
            get;
            private set;
        }
        public byte Bits
        {
            get;
            private set;
        }

        #region OVERRIDE_FUNCTIONS
        public override string ToString()
        {
            if (Bits >= 16)
                return string.Format("{0}{1:0000}", Category, Address);
            else
                return string.Format("{0}{1}.{2}", Category, Address, BitOffset);
        }
        #endregion

        /// <summary>
        /// 轉換成 Mobus 的定址
        /// </summary>
        /// <returns></returns>
        public int GetMobusAddress()
        {
            if (Bits < 16)
                return Address * 8 + BitOffset;
            else
                return Address;

        }

        /// <summary>
        /// 取得 禾川PLC定址 位元數
        /// </summary>
        public static byte GetAddressingBits(string category)
        {
            if (category.Length >= 2)
            {
                switch (category[0])
                {
                    case 'I':
                    case 'Q':
                    case 'M':
                        break;
                    default:
                        return 0;
                }

                switch (category[1])
                {
                    case 'D':
                        return 32;
                    case 'W':
                        return 16;
                    case 'B':
                        return 8;
                    case 'X':
                        return 1;
                    default:
                        return 0;
                }
            }
            return 0;
        }

        public static GxIoAddress operator +(GxIoAddress addr, int offset)
        {
            if (offset <= 0)
                return addr;

            //> offset &= 0xFFFF;

            int address = addr.Address;
            int bitoffset = addr.BitOffset;
            if (addr.Bits >= 16)
            {
                address += offset;
                bitoffset = 0;
            }
            else
            {
                bitoffset += offset;
                if (bitoffset>=8)
                {
                    address += 1;
                    bitoffset = 0;
                }
            }

            GxIoAddress newAddr = new GxIoAddress(
                        addr.Category,
                        address,
                        bitoffset,
                        addr.PlcID,
                        addr.Bits
                    );

            return newAddr;
        }
    }
}
