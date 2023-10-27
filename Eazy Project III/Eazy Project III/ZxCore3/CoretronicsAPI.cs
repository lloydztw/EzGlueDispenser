using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace JetEazy.GdxCore3.Model
{
    /// <summary>
    /// 中光電 DLL
    /// </summary>
    public class CoretronicsAPI
    {
        const string DLL_PATH = "PanelImageTool.dll";

        //-------------------------------------------------------------------------------------------------------
        // Basics
        //-------------------------------------------------------------------------------------------------------
        [DllImport(DLL_PATH, EntryPoint = "updateParams", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool updateParams();

        [DllImport(DLL_PATH, EntryPoint = "getVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getVersion(StringBuilder DLLVersion);

        //-------------------------------------------------------------------------------------------------------
        // Lens Center Compensation
        //-------------------------------------------------------------------------------------------------------
        [DllImport(DLL_PATH, EntryPoint = "setCenterCompInitial", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCenterCompInitial();

        [DllImport(DLL_PATH, EntryPoint = "setCenterCompImg", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCenterCompImg(int cols, int rows, int channels, IntPtr data, int ProjCompType);
        
        [DllImport(DLL_PATH, EntryPoint = "CenterCompProcess", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool CenterCompProcess();

        [DllImport(DLL_PATH, EntryPoint = "getCenterCompInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool getCenterCompInfo();

        [DllImport(DLL_PATH, EntryPoint = "getCenterInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getCenterInfo(int ProjCompType, [MarshalAs(UnmanagedType.LPArray)] float[] CenterInfo);

        //-------------------------------------------------------------------------------------------------------
        // Projection Compensation
        //-------------------------------------------------------------------------------------------------------
        [DllImport(DLL_PATH, EntryPoint = "setProjCompInitial", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setProjCompInitial();
        
        [DllImport(DLL_PATH, EntryPoint = "setProjCompImg", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setProjCompImg(int cols, int rows, int channels, IntPtr data, int ProjCompType);
        
        [DllImport(DLL_PATH, EntryPoint = "ProjCompProcess", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool ProjCompProcess();
        
        [DllImport(DLL_PATH, EntryPoint = "getProjCompInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getProjCompInfo(int ProjCompType, [MarshalAs(UnmanagedType.LPArray)] int[] MotorParams);

        [DllImport(DLL_PATH, EntryPoint = "getGBProjInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getGBProjInfo([MarshalAs(UnmanagedType.LPArray)] float[] DefectInfo);

        [DllImport(DLL_PATH, EntryPoint = "getMProjInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getMProjInfo([MarshalAs(UnmanagedType.LPArray)] float[] DefectInfo);

        [DllImport(DLL_PATH, EntryPoint = "setMarkInitial", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setMarkInitial();
        
        [DllImport(DLL_PATH, EntryPoint = "setMarkImg", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setMarkImg(int cols, int rows, int channels, IntPtr data);
        
        [DllImport(DLL_PATH, EntryPoint = "setMarkPt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setMarkPt([MarshalAs(UnmanagedType.LPArray)] int[] MarkPt);
        
        [DllImport(DLL_PATH, EntryPoint = "MarkPtProcess", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool MarkPtProcess();
        
        [DllImport(DLL_PATH, EntryPoint = "getGoldMarkPt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getGoldMarkPt([MarshalAs(UnmanagedType.LPArray)] int[] GoldMarkPt);
        
        [DllImport(DLL_PATH, EntryPoint = "getAlgoMarkPt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getAlgoMarkPt([MarshalAs(UnmanagedType.LPArray)] int[] AlgoMarkPt);

        /// <summary>
        ///設定Gold鏡片中心位置 <br/>
        ///光斑種類(0:藍綠色、1:紅色)
        /// </summary>
        /// <param name="ProjCompType">光斑種類(0:藍綠色、1:紅色)</param>
        /// <param name="GoldCenterPt">座標1x、座標1y</param>
        [DllImport(DLL_PATH, EntryPoint = "setGoldCenterPt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setGoldCenterPt(int ProjCompType, [MarshalAs(UnmanagedType.LPArray)] int[] GoldCenterPt);

        /// <summary>
        ///取得Gold鏡片中心座標 <br/>
        ///光斑種類(0:藍綠色、1:紅色) <br/>
        ///麻煩在02相機進行演算時，將GoldCenterPt位置用黃圈顯示出來，與03顯示GoldPt一樣 <br/>
        ///而從getCenterInfo得到的中心，改為與03光斑中心藍色圈一樣
        /// </summary>
        /// <param name="ProjCompType">光斑種類(0:藍綠色、1:紅色)</param>
        /// <param name="GoldCenterPt">座標1x、座標1y</param>
        [DllImport(DLL_PATH, EntryPoint = "getGoldCenterPt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getGoldCenterPt(int ProjCompType, [MarshalAs(UnmanagedType.LPArray)] int[] GoldCenterPt);
    }
}
