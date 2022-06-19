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
using JetEazy.ProcessSpace;

namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 投影補償 (Projection Compensate) 流程 <br/>
    /// --------------------------------------------
    /// @LETIAN: 20220619 開始實作
    /// </summary>
    public class MirrorBlackboxProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion

        #region PRIVATE_DATA
        #endregion

        #region SINGLETON
        static MirrorBlackboxProcess _singleton = null;
        private MirrorBlackboxProcess()
        {
        }
        #endregion

        public static MirrorBlackboxProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MirrorBlackboxProcess();
                return _singleton;
            }
        }

        /// <summary>
        /// 第一個參數決定 Mirror_CalibrateProcessIndex
        /// </summary>
        /// <param name="args"></param>
        public override void Start(params object[] args)
        {
            // (1) 可以直接嘗試將 args[0] 轉型
            //      try { Mirror_CalibrateProcessIndex = (int)args[0]; }
            //      catch { }
            // (2) 目前為了相容 Tick() 舊碼 ,
            //      暫時透過 base (ProcessClass.RelateString) 傳遞
            //      (a little awkwardly)
            base.Start(args[0]);
        }

        public override void Tick()
        {
            var Process = this;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 1000;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            Process.Stop();
                        }
                        break;
                }
            }
        }
    }
}
