using LightEConfocal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Eazy_Project_Measure
{
    public class LEClass
    {
        private const int MAXCOUNT_CHANNEL = 4;          //最大通道数量
        private char[] mAryChar = new char[64];         //保存控制器序列号，该数组最大能保存2个
        private int mDeviceCnt = 0;                    //控制器数量
        private int m_hDevice;                       //控制器句柄
        private int mChnCount = 0;                   //通道数量
        private int m_CaptureCnt = 10;                      //待采集数据点数
        private int m_haveCapturedCnt = 0;                //已采集数据点数
        private bool mbStop = false;                     //主动停止
        private double[] mRstData = new double[40000];  //存储采集到的位移数据
        //存储每个数据的波峰位置光强度
        private int[] mPeakItn = new int[40000];
        private float[] mPeakWave = new float[40000];
        private float[] mUnitRa = new float[40000];

        private double[][] mMultiChnRst = new double[MAXCOUNT_CHANNEL][];       //多通道高度测量结果保存数组

        //public List<double> m_CaptureDataList = new List<double>();

        private string mCfgPath;
        private string mHWCPath;
        private object m_CaptureObj = new object();
        private bool m_ThreadRunning = false;

        #region 事件
        //线程正常采集完数据或被主动停止
        //public delegate void E_FinishMeasure();
        //public delegate void E_UpdateDataToDGV();
        #endregion
        #region 事件响应函数
        private void UpdateDataToDGV()
        {
            try
            {
                for (int ch = 0; ch < mChnCount; ++ch)
                {
                    for (int i = 0; i < m_haveCapturedCnt; ++i)
                    {
                        //DGVRst.Rows[i].Cells[ch].Value = mMultiChnRst[ch][i];
                        //收集数据
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        #endregion

        private bool m_IsDebug = false;
        //public bool Is

        private static readonly LEClass _instance = new LEClass();
        public static LEClass Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Init(string eCfgPath,string eHWCPath,bool eIsDebug)
        {
            m_IsDebug = eIsDebug;

            mCfgPath = eCfgPath;
            mHWCPath = eHWCPath;

            if (m_IsDebug)
                return;

            int iSta = 0;
            LEConfocalDLL.LE_SelectDeviceType(2);                       //选择需使用的控制器类型接口，当前选择USB2代控制器
            iSta = LEConfocalDLL.LE_InitDLL();                          //初始化传感器DLL
            mDeviceCnt = LEConfocalDLL.LE_GetSpecCount();               //获取已连接控制器数量
            //DeviceCount.Text = mDeviceCnt.ToString();
            bool bSta = LEConfocalDLL.LE_GetSpecSN(mAryChar, mDeviceCnt);      //获取已连接控制器序列号    
            for (int i = 0; i < MAXCOUNT_CHANNEL; ++i)
            {
                mMultiChnRst[i] = new double[20000];
            }
            //DGVRst.RowCount = 40000;
            Array.Clear(mRstData, 0, 20000);
            Array.Clear(mPeakItn, 0, 20000);
        }
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns>0 返回成功 -1 配置文件不存在 -2 打开设备失败 -3 未找到设备</returns>
        public int Open()
        {
            if (m_IsDebug)
                return 0;

            int iSta = 0;
            if (mDeviceCnt > 0)
            {
                if (!File.Exists(mCfgPath) || !File.Exists(mHWCPath))
                {
                    return -1;//配置文件不存在
                }

                //if (mCfgPath == "" || mHWCPath == "")
                //{
                //    //MessageBox.Show("配置和校准文件不能为空，请选择！");
                //    return;
                //}
                char[] pSN1 = new char[32];
                for (int i = 0; i < 32; ++i)
                {
                    if (mAryChar[i] != '\0')
                        pSN1[i] = mAryChar[i];
                    else
                        break;
                }
                iSta = LEConfocalDLL.LE_Open(pSN1, ref m_hDevice);
                //打开第一个序列号的传感器
                if (iSta == 1)
                {
#if DEBUG
                    //debug模式下加长控制器连接的心跳时间，否则控制器断点调试时程序中断时间超过心跳时间，控制器将断开与程序的连接
                    iSta = LEConfocalDLL.LE_SetHeartTime(30000, m_hDevice);
#endif
                    StringBuilder paramfilePath = new StringBuilder(mCfgPath);
                    iSta = LEConfocalDLL.LE_LoadDeviceConfigureFromFile(paramfilePath, m_hDevice);                           //载入控制器配置文件，该文件必须路径正确且与当前使用控制器序号匹配
                    mChnCount = LEConfocalDLL.LE_GetChannels(m_hDevice);
                    StringBuilder lwfilePath = new StringBuilder(mHWCPath);
                    //如果有多个通道则启用多个通道，然后加载多个通道的校准数据
                    iSta = LEConfocalDLL.LE_LoadLWCalibrationData(lwfilePath, m_hDevice);                                    //载入控制器校准文件，该文件必须路径正确且与当前使用控制器序号匹配
                    //TBCHCount.Text = mChnCount.ToString();
                    //BTNOpen.Text = "关闭设备";
                    //BTNCapture.Enabled = true;
                    //BTNThreadCapture.Enabled = true;
                    //button3.Enabled = true;
                    //button4.Enabled = true;
                    //button5.Enabled = true;

                    LEConfocalDLL.LE_SetTriggerMode(0, m_hDevice);//连续触发
                    //LEConfocalDLL.LE_SetTriggerMode(2, m_hDevice);//外部触发

                    return 0;

                }
                else
                    return -2;
                    //MessageBox.Show("打开设备失败，请重启软件");
            }
            return -3;
        }
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>0 返回成功</returns>
        public int Close()
        {
            if (m_IsDebug)
                return 0;

            int iSta = 0;
            iSta = LEConfocalDLL.LE_Close(ref m_hDevice);                       //关闭设备

            bool bSta = false;
            mbStop = true;
            if (Monitor.TryEnter(m_CaptureObj, 5000))
            {
                LEConfocalDLL.LE_Close(ref m_hDevice);
                bSta = LEConfocalDLL.LE_UnInitDLL();                        //退出程序，反初始化传感器DLL
            }

            return 0;
        }

        /// <summary>
        /// 采集点数
        /// </summary>
        public int CaptureCnt
        {
            get { return m_CaptureCnt; }
            set { m_CaptureCnt = value; }
        }

        /// <summary>
        /// 触发采集
        /// </summary>
        /// <returns></returns>
        public double Snap()
        {
            if (m_IsDebug)
                return 0;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //if ("" == DataCount.Text)
            //    DataCount.Text = "10";
            //m_CaptureCnt = Convert.ToInt32(DataCount.Text);
            if (m_CaptureCnt > 1000)
                m_CaptureCnt = 1000;                            //当前程序存储结果数据的数组大小为1000，所以采集点数不能超过1000
            int iSta = 0;
            //开始采集数据
            iSta = LEConfocalDLL.LE_StartGetAllValuesEx(mRstData, m_CaptureCnt, m_hDevice, 1, mUnitRa, mPeakWave, null, mPeakItn);

            bool bError = false;
            int iCapturedCnt = 0;
            sw.Restart();
            //检测当前采集任务是否采集完成循环
            while (iCapturedCnt < m_CaptureCnt)
            {
                iSta = LEConfocalDLL.LE_GetCapturedPoints(ref iCapturedCnt, m_hDevice, 1);
                Thread.Sleep(10);
                //判断是否主动停止或超时
                if (sw.Elapsed.TotalSeconds > 6)
                {
                    bError = true;
                    break;
                }

            }
            sw.Stop();
            double rstData = 0;

            if (!bError)
            {
                //计算采集结果数据总和
                for (int i = 0; i < m_CaptureCnt; ++i)
                {
                    rstData += mRstData[i];
                }
                //计算采集结果平均值
                rstData = rstData / m_CaptureCnt / 1000;
                //RSTAVG.Text = rstData.ToString();
                //MessageBox.Show("采集完成");
            }
            else
            {
                rstData = -1000;
            }

            return rstData;
        }
        

        private void CaptureStart()
        {

            //m_CaptureCnt = 10;
            m_haveCapturedCnt = 0;
            mbStop = false;

            m_ThreadRunning = true;
            Thread th = new Thread(new ParameterizedThreadStart(ThreadGetData));
            th.Start();
        }
        private void CaptureStop()
        {
            mbStop = true;
        }

        void ThreadGetData(object obj)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (Monitor.TryEnter(m_CaptureObj, 1000))
            {
                //注意程序窗口关闭前，请先点停止采集按钮关闭线程运行，否则可能或报控件写入错误问题
                while (m_ThreadRunning)
                {
                    if (m_CaptureCnt > 20000)
                        m_CaptureCnt = 20000;                            //当前程序存储结果数据的数组大小为20000
                    int iSta = 0;
                    m_haveCapturedCnt = 0;
                    //开始采集数据
                    for (int i = 0; i < mChnCount; ++i)
                    {
                        iSta = LEConfocalDLL.LE_SetChannelGetAllValues(mMultiChnRst[i], m_CaptureCnt, m_hDevice, i + 1);
                    }
                    iSta = LEConfocalDLL.LE_StartGetChannelsValues(m_hDevice);
                    sw.Restart();
                    //检测当前采集任务是否采集完成循环,测试临时注释while循环
                    while (m_haveCapturedCnt < m_CaptureCnt)
                    {
                        iSta = LEConfocalDLL.LE_GetCapturedPoints(ref m_haveCapturedCnt, m_hDevice);
                        Thread.Sleep(10);
                        //判断是否主动停止或超时
                        if (mbStop || sw.Elapsed.TotalSeconds > 6)
                        {
                            m_ThreadRunning = false;
                            mbStop = false;
                            break;
                        }
                    }
                    sw.Stop();
                    int sta = LEConfocalDLL.LE_GetDeviceStatus(m_hDevice);
                    if (1 == sta)
                        LEConfocalDLL.LE_StopGetPoints(m_hDevice);
                    //Invoke(new E_UpdateDataToDGV(UpdateDataToDGV));                 //窗体未关闭时才刷新测量结果到表格
                    UpdateDataToDGV();
                }
                LEConfocalDLL.LE_StopGetPoints(m_hDevice);
                //Invoke(new E_FinishMeasure(FinishMeasure));                         //切换按钮名称         
                FinishMeasure();
                Monitor.Exit(m_CaptureObj);
            }
        }

        void FinishMeasure()
        {
            mbStop = false;
            //BTNThreadCapture.Text = "使用线程采集";
        }

    }
}
