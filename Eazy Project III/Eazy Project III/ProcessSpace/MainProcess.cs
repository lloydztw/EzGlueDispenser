using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Eazy_Project_Interface;
using Eazy_Project_III.ControlSpace.MachineSpace;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.UISpace;
using JzDisplay;
using JzDisplay.UISpace;
using PhotoMachine.UISpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VsCommon.ControlSpace;
using Eazy_Project_III.OPSpace;
using Eazy_Project_III.UISpace;
using Eazy_Project_III.ControlSpace.IOSpace;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.ControlSpace;
using VsCommon.ControlSpace.IOSpace;
using Eazy_Project_Measure;
using Eazy_Project_III.FormSpace;
using Eazy_Project_III.UISpace.IOSpace;
using JetEazy.GdxCore3;



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
        

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        SetRunningLight();

                        //Process.NextDuriation = NextDurtimeTmp;
                        //Process.ID = 510;
                        SetNext(510);
                        CommonLogClass.Instance.LogMessage("主流程开始", Color.Black);

                        //m_PickIndex = MainGroupIndex;
                        //MainMirrorIndex = 0;

                        break;
                    case 510:
                        if (Process.IsTimeup)
                        {

                            Process.NextDuriation = 500;
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
                                Process.NextDuriation = 500;
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
                                Process.NextDuriation = 500;
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
                                Process.NextDuriation = 500;
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

                                if (MainMirrorIndex < 1 && !MainAloneToMirror)
                                {
                                    MainMirrorIndex++;//左边搞完 搞右边 先Mirror0 然后 Mirror1
                                    Process.NextDuriation = 500;
                                    Process.ID = 510;
                                }
                                else
                                {
                                    Process.NextDuriation = 500;
                                    Process.ID = 50;

                                    //m_BuzzerIndex = 0;
                                    //m_BuzzerCount = 3;//测试完成叫三声
                                    m_BuzzerProcess.Start();
                                }
                            }
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            if (!m_BuzzerProcess.IsOn)
                            {
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("主流程结束", Color.Black);

                                SetNormalLight();
                            }
                        }
                        break;
                }
            }
        }
    }
}
