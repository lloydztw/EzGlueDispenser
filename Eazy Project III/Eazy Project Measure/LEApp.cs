/*
 * 此文件为LightE数据处理及应用测量所导出函数。
 * 最后更新日期：20201022
 * 对应DLL版本：ver1.77.4.21
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace LightEConfocal
{
    class LEApp
    {
        /*
        函数功能：无效数据拟合函数。
        参数：
        pRscData-[in,out]需进行拟合的源数据。
        count-[in]pRscData的数据长度。
        RanageLow-[in]有效数据的下边界。
        RanageHeight-[in]有效数据的上边界。
        IDFParam-[in]数据拟合参数结构体。
        返回值：成功返回1,其他返回值请参考返回值定义表。
        */
        [DllImport("ConfocalDLL_x64.dll")]
        public static extern int LE_InvalidDataFit(double[] pRscData, int count, float RanageLow, float RanageHeight, ref InvalidDataFitParam IDFParam);
        /*
        函数功能：小数据段过滤函数。
        参数：
        pRscData-[in,out]需进行过滤的源数据。
        count-[in]pRscData的数据长度。
        continueCountThd-[in]数据段最少有效点数，小于等于此数量的数据段将被过滤。
        RanageLow-[in]有效数据的下边界。
        RanageHeight-[in]有效数据的上边界。
        markValue-[in]待过滤数据填充的标记值。
        返回值：成功返回1,其他返回值请参考返回值定义表。
        */
        [DllImport("ConfocalDLL_x64.dll")]
        public static extern int LE_DeleteDataByContinueCount(double[] pRscData, int count, int continueCountThd, float RanageLow, float RanageHeight,
            double markValue);
        /*
        函数功能：有效数据段两端翘起圆弧数据专用优化函数。
        参数：
        pRscH-[in,out]需进行优化的源数据。
        count-[in]pRscH的数据长度。
        prcParam-[in]数据优化参数结构体。
        RanageLow-[in]有效数据的下边界。
        RanageHeight-[in]有效数据的上边界。
        返回值：成功返回1,其他返回值请参考返回值定义表。
        */
        [DllImport("ConfocalDLL_x64.dll")]
        public static extern int LE_AbnormalDataProcess(double[] pRscH, int count, EDPParam prcParam, float RanageLow, float RanageHeight);

    }
}
