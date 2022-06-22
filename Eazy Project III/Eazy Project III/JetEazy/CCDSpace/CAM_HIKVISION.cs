#define MVS
#if MVS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.ObjectModel;
using JetEazy.BasicSpace;
using JetEazy;

namespace JetEazy.ControlSpace
{
    public class CAM_HIKVISION
    {
        /// <summary>
        /// 记录已连接的相机编号
        /// </summary>
        public static string StrIsHaveConnectedIndex = ",";

        //private string m_ConfigPath = "";
        private string m_cam_serialnumber = "";

        PictureBox picForMyCameraHandle;// = new PictureBox();


        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        private MyCamera m_pMyCamera;
        bool m_bGrabbing;
        int m_number_no = 0;

        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        UInt32 m_nBufSizeForDriver = 3072 * 2048 * 3;
        byte[] m_pBufForDriver = new byte[3072 * 2048 * 3];

        // ch:用于保存图像的缓存 | en:Buffer for saving image
        UInt32 m_nBufSizeForSaveImage = 3072 * 2048 * 3 * 3 + 2048;
        byte[] m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];

        public CAM_HIKVISION(PictureBox pbx,int icam_no)
        {
            picForMyCameraHandle = pbx;
            m_number_no = icam_no;
        }

        //public void Init(string eConfigPath)
        //{
        //    m_ConfigPath = eConfigPath;

        //    m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        //    m_bGrabbing = false;
        //    DeviceListAcq();

        //    //StopGrab();
        //    //CloseDevice();

        //    OpenDevice();
        //    StartGrab();
        //}
        public void Init(string eCameraSerialNumber)
        {
            m_cam_serialnumber = eCameraSerialNumber;

            m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            m_bGrabbing = false;
            DeviceListAcq();

            //StopGrab();
            //CloseDevice();

            OpenDevice();
            StartGrab();
        }
        public void Dispose()
        {
            StopGrab();
            CloseDevice();
        }
        private void OpenDevice()
        {
            if (m_pDeviceList.nDeviceNum == 0)// || cbDeviceList.SelectedIndex == -1)
            {
                ShowErrorMsg("No device, please select", 0);
                return;
            }

            //if(m_number_no >= m_pDeviceList.nDeviceNum)
            //{
            //    //ShowErrorMsg("over device count", 0);
            //    //return;
            //    m_number_no = m_number_no - (int)m_pDeviceList.nDeviceNum;
            //}

            //string CCDSEQStr = m_ConfigPath + "\\HIKCCDSEQ.INI";
            string Str = "";

            //if (File.Exists(CCDSEQStr))
            //{
            //    ReadData(ref Str, CCDSEQStr);

            //    string[] strs;

            //    Str = Str.Replace(Environment.NewLine, "@");
            //    strs = Str.Split('@');

            //    int iiindex = 0;
            //    while (iiindex < strs.Length)
            //    {
            //        //如果读到已连接的序号
            //        if (StrIsHaveConnectedIndex.IndexOf("," + iiindex.ToString() + ",") > -1)
            //        {

            //        }
            //        else
            //        {
            //            break;
            //        }

            //        iiindex++;
            //    }

            //    m_number_no = GetDeviceNumber(strs[iiindex]);
            //}
            //else
            //{
            //    ShowErrorMsg("No HIKCCDSEQ.INI", 0);
            //    return;
            //}

            m_number_no = GetDeviceNumber(m_cam_serialnumber);

            if (m_number_no >= m_pDeviceList.nDeviceNum)
            {
                ShowErrorMsg("over device count", 0);
                return;
            }

            int nRet = -1;

            //deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));//获取设备

            // ch:获取选择的设备信息 | en:Get selected device information //选择第一颗相机 目前只接一颗 将来再列表
            MyCamera.MV_CC_DEVICE_INFO device =
            (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[m_number_no],
                                                          typeof(MyCamera.MV_CC_DEVICE_INFO));

            StrIsHaveConnectedIndex += m_number_no.ToString() + ",";

            ////MyCamera.MV_CC_DEVICE_INFO device = null;
            //for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            //{
            //    device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
            //}

            // ch:打开设备 | en:Open device
            if (null == m_pMyCamera)
            {
                m_pMyCamera = new MyCamera();
                if (null == m_pMyCamera)
                {
                    return;
                }
            }

            nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                return;
            }

            nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_pMyCamera.MV_CC_DestroyDevice_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return;
            }

            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = m_pMyCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = m_pMyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (nRet != MyCamera.MV_OK)
                    {
                        Console.WriteLine("Warning: Set Packet Size failed {0:x8}", nRet);
                    }
                }
                else
                {
                    Console.WriteLine("Warning: Get Packet Size failed {0:x8}", nPacketSize);
                }
            }

            // ch:触发源选择:
            //           0 - Line0; | en:Trigger source select:0 - Line0;
            //           1 - Line1;
            //           2 - Line2;
            //           3 - Line3;
            //           4 - Counter;
            //           7 - Software;

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode

            //0 single frame 2 continue frame
            m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);// ch:工作在连续模式 | en:Acquisition On Continuous Mode
            //m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 7);    // ch:连续模式 | en:Continuous

            TriggerMode(0);

            //m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);    // ch:连续模式 | en:Continuous

            //bnGetParam_Click(null, null);// ch:获取参数 | en:Get parameters

            // ch:控件操作 | en:Control operation
            //SetCtrlWhenOpen();
        }
        private void CloseDevice()
        {
            // ch:关闭设备 | en:Close Device
            int nRet;

            nRet = m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);    // ch:连续模式 | en:Continuous

            nRet = m_pMyCamera.MV_CC_CloseDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                return;
            }

            nRet = m_pMyCamera.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                return;
            }

            // ch:控件操作 | en:Control Operation
            //SetCtrlWhenClose();

            // ch:取流标志位清零 | en:Reset flow flag bit
            m_bGrabbing = false;
            StrIsHaveConnectedIndex = ",";
        }

        private void StartGrab()
        {
            int nRet;

            // ch:开始采集 | en:Start Grabbing
            nRet = m_pMyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Trigger Fail!", nRet);
                return;
            }

            // ch:控件操作 | en:Control Operation
            //SetCtrlWhenStartGrab();

            // ch:标志位置位true | en:Set position bit true
            m_bGrabbing = true;


            // ch:显示 | en:Display
            nRet = m_pMyCamera.MV_CC_Display_NET(picForMyCameraHandle.Handle);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Display Fail！", nRet);
            }
        }
        private void StopGrab()
        {
            int nRet = -1;
            // ch:停止采集 | en:Stop Grabbing
            nRet = m_pMyCamera.MV_CC_StopGrabbing_NET();
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!", nRet);
            }

            // ch:标志位设为false | en:Set flag bit false
            m_bGrabbing = false;

            // ch:控件操作 | en:Control Operation
            //SetCtrlWhenStopGrab();
        }

        public int GetFloatValue_NET(ref MyCamera.MVCC_FLOATVALUE stParam, string strKey = "ExposureTime")
        {
            //MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_pMyCamera.MV_CC_GetFloatValue_NET(strKey, ref stParam);
            //if (MyCamera.MV_OK == nRet)
            //{

            //}
            return nRet;
        }
        public void SetExposure(float fexposure)
        {
            m_pMyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", fexposure);
        }
        public void SetGain(float fgain)
        {
            m_pMyCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
            m_pMyCamera.MV_CC_SetFloatValue_NET("Gain", fgain);
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="iMode">0 off 连续 1 on 单次</param>
        public void TriggerMode(uint iMode)
        {
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", iMode);
        }
        /// <summary>
        /// 触发模式 ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0; 1 - Line1; 2 - Line2; 3 - Line3;4 - Counter; 7 - Software
        /// </summary>
        /// <param name="iMode"> ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0; 1 - Line1; 2 - Line2; 3 - Line3;4 - Counter; 7 - Software</param>
        public void TriggerSource(uint iSource)
        {
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", iSource);
        }
        public void TriggerSoftwareX()
        {
            int nRet;
            // ch:触发命令 | en:Trigger command
            nRet = m_pMyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            //if (MyCamera.MV_OK != nRet)
            //{
            //    ShowErrorMsg("Trigger Software Fail!", nRet);
            //}
        }
        public Bitmap CaptureBmp(int roattion)
        {
            int nRet;
            UInt32 nPayloadSize = 0;
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            nRet = m_pMyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Get PayloadSize failed", nRet);
                return null;
            }
            nPayloadSize = stParam.nCurValue;
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                m_nBufSizeForDriver = nPayloadSize;
                m_pBufForDriver = new byte[m_nBufSizeForDriver];

                // ch:同时对保存图像的缓存做大小判断处理 | en:Determine the buffer size to save image
                // ch:BMP图片大小：width * height * 3 + 2048(预留BMP头大小) | en:BMP image size: width * height * 3 + 2048 (Reserved for BMP header)
                m_nBufSizeForSaveImage = m_nBufSizeForDriver * 3 + 2048;
                m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
            }

            IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            // ch:超时获取一帧，超时时间为1秒 | en:Get one frame timeout, timeout is 1 sec
            nRet = m_pMyCamera.MV_CC_GetOneFrameTimeout_NET(pData, m_nBufSizeForDriver, ref stFrameInfo, 1000);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("No Data!", nRet);
                return null;
            }

            MyCamera.MvGvspPixelType enDstPixelType;
            if (IsMonoData(stFrameInfo.enPixelType))
            {
                enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
            }
            else if (IsColorData(stFrameInfo.enPixelType))
            {
                //enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            }
            else
            {
                ShowErrorMsg("No such pixel type!", 0);
                return null;
            }

            IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
            MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
            MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
            stConverPixelParam.nWidth = stFrameInfo.nWidth;
            stConverPixelParam.nHeight = stFrameInfo.nHeight;
            stConverPixelParam.pSrcData = pData;
            stConverPixelParam.nSrcDataLen = stFrameInfo.nFrameLen;
            stConverPixelParam.enSrcPixelType = stFrameInfo.enPixelType;
            stConverPixelParam.enDstPixelType = enDstPixelType;
            stConverPixelParam.pDstBuffer = pImage;
            stConverPixelParam.nDstBufferSize = m_nBufSizeForSaveImage;
            nRet = m_pMyCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
            if (MyCamera.MV_OK != nRet)
            {
                return null;
            }

            if (enDstPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
            {
                //************************Mono8 转 Bitmap*******************************
                Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 1, PixelFormat.Format8bppIndexed, pImage);

                ColorPalette cp = bmp.Palette;
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                // set palette back
                bmp.Palette = cp;

                switch (roattion)
                {
                    case 90:
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 270:
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    case 180:
                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                }

                //bmp.Save("image.bmp", ImageFormat.Bmp);
                return bmp;
            }
            else
            {
                //*********************RGB8 转 Bitmap**************************
                for (int i = 0; i < stFrameInfo.nHeight; i++)
                {
                    for (int j = 0; j < stFrameInfo.nWidth; j++)
                    {
                        byte chRed = m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3];
                        m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3] = m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3 + 2];
                        m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3 + 2] = chRed;
                    }
                }
                try
                {
                    Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pImage);
                    //bmp.Save("image.bmp", ImageFormat.Bmp);

                    switch (roattion)
                    {
                        case 90:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 270:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 180:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                    }

                    return bmp;
                }
                catch
                {
                }

            }

            return null;
            //ShowErrorMsg("Save Succeed!", 0);
        }
        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            //MessageBox.Show(errorMsg, "PROMPT");
        }

        private Boolean IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;

                default:
                    return false;
            }
        }

        /************************************************************************
         *  @fn     IsColorData()
         *  @brief  判断是否是彩色数据
         *  @param  enGvspPixelType         [IN]           像素格式
         *  @return 成功，返回0；错误，返回-1 
         ************************************************************************/
        private Boolean IsColorData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YCBCR411_8_CBYYCRYY:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 通过相机序列号 对应相机的编号
        /// </summary>
        /// <param name="strSerialNumber">相机序列号</param>
        /// <returns></returns>
        private int GetDeviceNumber(string strSerialNumber)
        {
            int iNumber = -1;

            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            //cbDeviceList.Items.Clear();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return iNumber;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    if (gigeInfo.chSerialNumber == strSerialNumber)
                    {
                        iNumber = i;
                        break;
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));

                    if (usbInfo.chSerialNumber == strSerialNumber)
                    {
                        iNumber = i;
                        break;
                    }

                }
            }

            return iNumber;
        }
        private void DeviceListAcq()
        {
            
            //string CCDSEQStr = m_ConfigPath + "\\HIKCCDSEQ.INI";
            string Str = "";

            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            //cbDeviceList.Items.Clear();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    //if (gigeInfo.chUserDefinedName != "")
                    //{
                    //    //cbDeviceList.Items.Add("GigE: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                    //}
                    //else
                    //{
                    //    //cbDeviceList.Items.Add("GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    //}

                    Str += gigeInfo.chSerialNumber + Environment.NewLine;
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    //if (usbInfo.chUserDefinedName != "")
                    //{
                    //    //cbDeviceList.Items.Add("USB: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                    //}
                    //else
                    //{
                    //    //cbDeviceList.Items.Add("USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    //}

                    Str += usbInfo.chSerialNumber + Environment.NewLine;
                }

                
            }

            //if (!System.IO.File.Exists(CCDSEQStr))
            //{
            //    Str = RemoveLastChar(Str, 2);
            //    SaveData(Str, CCDSEQStr);
            //}

            //// ch:选择第一项 | en:Select the first item
            //if (m_pDeviceList.nDeviceNum != 0)
            //{
            //    //cbDeviceList.SelectedIndex = 0;
            //}
        }

        string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }

        void SaveData(string DataStr, string FileName)
        {
            StreamWriter Swr = new StreamWriter(FileName, false, Encoding.Default);

            Swr.Write(DataStr);

            Swr.Flush();
            Swr.Close();
            Swr.Dispose();
        }
        void ReadData(ref string DataStr, string FileName)
        {
            StreamReader Srr = new StreamReader(FileName, Encoding.Default);

            DataStr = Srr.ReadToEnd();

            Srr.Close();
            Srr.Dispose();
        }

    }
}

#endif
