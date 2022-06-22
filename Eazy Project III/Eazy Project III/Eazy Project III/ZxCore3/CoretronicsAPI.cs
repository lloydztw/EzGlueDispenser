using System;
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
        public static extern void setCenterCompImg(int cols, int rows, int channels, IntPtr data);
        
        [DllImport(DLL_PATH, EntryPoint = "CenterCompProcess", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool CenterCompProcess();

        [DllImport(DLL_PATH, EntryPoint = "getCenterCompInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool getCenterCompInfo();

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
    }
}
