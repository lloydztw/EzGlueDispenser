using JetEazy.BasicSpace;
using System;
using System.Drawing;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 主流程
    /// @LETIAN: 20220619 (refactor)
    /// </summary>
    public class MainProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_pickprocess
        {
            get
            {
                return MirrorPickProcess.Instance;
            }
        }
        BaseProcess m_calibrateprocess
        {
            get
            {
                return MirrorCalibProcess.Instance;
            }
        }
        BaseProcess m_blackboxprocess
        {
            get
            {
                return MirrorBlackboxProcess.Instance;
            }
        }
        BaseProcess m_dispensingprocess
        {
            get
            {
                return MirrorDispenseProcess.Instance;
            }
        }
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        #endregion

        #region PUBLIC_DATA
        /// <summary>
        /// 拾取第几组 总共4组  从0开始
        /// </summary>
        public int MainGroupIndex = 0;
        /// <summary>
        /// 记录操作 Mirror0 还是 Mirror1 即左边还是右边
        /// </summary>
        public int MainMirrorIndex = 0;
        /// <summary>
        /// 单独制作一个Mirror
        /// </summary>
        public bool MainAloneToMirror = false;
        /// <summary>
        /// 制作的计数
        /// </summary>
        public int CurRunCont
        {
            get { return m_curRunCount; }
        }
        #endregion

        #region PRIVATE_DATA
        int m_curRunCount = 0;
        #endregion

        #region SINGLETON
        static MainProcess _singleton = null;
        private MainProcess()
        {
        }
        #endregion

        public static MainProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MainProcess();
                return _singleton;
            }
        }
        
        public string Barcode
        {
            get
            {
                if (m_blackboxprocess != null)
                    return ((MirrorAbsImageProcess)m_blackboxprocess).ImageDumpStemName;
                else
                    return "";
            }
            set
            {
                if (m_calibrateprocess != null)
                    ((MirrorAbsImageProcess)m_calibrateprocess).ImageDumpStemName = value;
                if (m_blackboxprocess != null)
                    ((MirrorAbsImageProcess)m_blackboxprocess).ImageDumpStemName = value;
            }
        }
        public DateTime StartTime
        {
            get;
            private set;
        }

        public override void Tick()
        {
            if (!IsValidPlcScanned())
                return;

            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:
                        this.StartTime = DateTime.Now;
                        this.LastNG = null;
                        SetRunningLight();
                        //正常測試流程中開啓偵測門禁和光幕
                        MACHINE.PLCIO.ADR_RUNNING_PLC_ALARM = true;
                        Process.NextDuriation = NextDurtimeTmp;
                        Process.ID = 510;
                        CommonLogClass.Instance.LogMessage("主流程开始", Color.Black);

                        //m_PickIndex = MainGroupIndex;
                        //MainMirrorIndex = 0;
                        m_curRunCount = 0;

                        break;
                    case 510:
                        if (Process.IsTimeup)
                        {

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 10;

                            CommonLogClass.Instance.LogMessage("吸料开始", Color.Black);
                            CommonLogClass.Instance.LogMessage("拾取 组 " + MainGroupIndex.ToString(), Color.Black);

                            m_pickprocess.Start(MainMirrorIndex.ToString());
                            //CommonLogClass.Instance.LogMessage("拾取 Mirror " + MainMirrorIndex.ToString(), Color.Black);

                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (!m_pickprocess.IsOn)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 20;

                                CommonLogClass.Instance.LogMessage("吸料结束", Color.Black);
                                CommonLogClass.Instance.LogMessage("校正开始", Color.Black);
                                m_calibrateprocess.Start(MainMirrorIndex.ToString());
                            }
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (!m_calibrateprocess.IsOn)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 30;

                                CommonLogClass.Instance.LogMessage("校正结束", Color.Black);
                                CommonLogClass.Instance.LogMessage("blackBox开始", Color.Black);
                                m_blackboxprocess.Start(MainMirrorIndex.ToString());
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (!m_blackboxprocess.IsOn)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 40;

                                CommonLogClass.Instance.LogMessage("blackBox结束", Color.Black);
                                CommonLogClass.Instance.LogMessage("点胶开始", Color.Black);
                                m_dispensingprocess.Start(MainMirrorIndex.ToString());
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            if (!m_dispensingprocess.IsOn)
                            {
                                CommonLogClass.Instance.LogMessage("点胶结束", Color.Black);
                                firePartialCompleted(MainMirrorIndex, null);

                                if (m_curRunCount == 0 && !MainAloneToMirror)
                                {
                                    m_curRunCount++;
                                    MainMirrorIndex = (MainMirrorIndex + 1) % 2;
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 510;
                                }
                                else
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 50;

                                    //m_BuzzerIndex = 0;
                                    //m_BuzzerCount = 3;//测试完成叫三声
                                    m_BuzzerProcess.Start();//完成后 直接叫3聲  不判斷是否叫完
                                }
                            }
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            //if (!m_BuzzerProcess.IsOn)
                            {
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("主流程结束", Color.Black);
                                //正常測試流程中開啓偵測門禁和光幕 流程結束關閉
                                MACHINE.PLCIO.ADR_RUNNING_PLC_ALARM = false;
                                SetNormalLight();
								FireCompleted();
                            }
                        }
                        break;
                }
            }
        }

        public void SetNG(string ngMsg)
        {
            LastNG = ngMsg;

            if (ngMsg != null)
            {
                SetAbnormalLight();
                bool isRunning = (this.ID > 5);
                if (isRunning)
                {
                    this.Stop();
                    CommonLogClass.Instance.LogMessage("主流程结束 (NG)", Color.DarkRed);
                    //>>> 正常測試流程中開啓偵測門禁和光幕 流程結束關閉
                    //>>> MACHINE.PLCIO.ADR_RUNNING_PLC_ALARM = false;
                    //>>> FireCompleted();
                    firePartialCompleted(MainMirrorIndex, LastNG);
                    FireCompleted();
                }
                this.ID = 0;
            }
            else
            {
                SetNormalLight();
                Stop();
                this.ID = 0;
            }
        }

        private void firePartialCompleted(int mirrorID, string ngMsg)
        {
            var e = new JetEazy.ProcessSpace.ProcessEventArgs("PartialCompleted", new object[] { mirrorID, ngMsg });
            FireCompleted(e);
        }
    }
}
