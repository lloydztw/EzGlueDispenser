using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.OPSpace;
using JetEazy.BasicSpace;
using JetEazy.GdxCore3;
using System.Collections.Generic;
using System.Drawing;

namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 点胶流程 <br/>
    /// ------------------------------------------ <br/>
    /// 1.判断 Mirror_DispensingProcessIndex 值 到达Mirror0 还是 Mirror1 <br/>  
    /// 如果 Mirror_DispensingProcessIndex 的值 不是0和1 将结束此流程 并停止主流 <br/>
    /// 否则 继续下面的流程 <br/>
    /// 2.将位置转存到缓存的list中 m_DispensingRunList <br/>
    /// 3.通过m_DispensingIndex 记录 进行到点胶的哪个点 <br/>
    /// 4.某个点完成后 回到避光槽位置 然后进行下一个点 <br/>
    /// 5.全部完成后 点胶模组回到待命位置  打开UV 定时 关闭UV <br/>
    /// 6.微调和吸嘴模组 回到待命位置 <br/>
    /// 7.流程结束 <br/>
    /// ------------------------------------------ <br/>
    /// @LETIAN: 20220619 重新包裝
    /// </summary>
    public class MirrorDispenseProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion

        #region PRIVATE_DATA
        /// <summary>
        /// 判断点胶组 跑哪一个Mirror 左边还是右边
        /// </summary>
        int Mirror_DispensingProcessIndex = 0;
        int m_DispensingIndex = 0;
        List<string> m_DispensingRunList = new List<string>();

        /// <summary>
        /// UV 照射計時, 每秒產生LOG訊息 (threading) <br/>
        /// @LETIAN:2022/07/05
        /// </summary>
        System.Threading.Timer m_uvTimer;
        int m_uvTimerCount = 0;
        #endregion

        #region SINGLETON
        static MirrorDispenseProcess _singleton = null;
        private MirrorDispenseProcess()
        {
        }
        #endregion

        public static MirrorDispenseProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MirrorDispenseProcess();
                return _singleton;
            }
        }

        /// <summary>
        /// 第一個參數決定 Mirror_DispensingProcessIndex
        /// </summary>
        /// <param name="args"></param>
        public override void Start(params object[] args)
        {
            // (1) 可以直接嘗試將 args[0] 轉型
            //      try { Mirror_DispensingProcessIndex = (int)args[0]; }
            //      catch { }
            // (2) 目前為了相容 Tick() 舊碼 ,
            //      暫時透過 base (ProcessClass.RelateString) 傳遞
            //      (a little awkwardly)
            base.Start(args[0]);
        }
        
        public override void Stop()
        {
            stop_uv_timer();
            base.Stop();
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
                        m_DispensingIndex = 0;
                        m_DispensingRunList.Clear();

                        Process.NextDuriation = NextDurtimeTmp;
                        //NextDurtimeTmp = 50;
                        bool bOK = true;

                        switch (Process.RelateString)
                        {
                            case "0":
                                Mirror_DispensingProcessIndex = 0;

                                foreach (string str in INI.Instance.Mirror1JamedPosList)
                                    m_DispensingRunList.Add(str);

                                Process.ID = 10;

                                CommonLogClass.Instance.LogMessage("点胶启动Mirror0", Color.Black);
                                GdxCore.Trace("MirrorDispenser.Start", Process, 0);
                                break;
                            case "1":
                                Mirror_DispensingProcessIndex = 1;

                                foreach (string str in INI.Instance.Mirror2JamedPosList)
                                    m_DispensingRunList.Add(str);

                                Process.ID = 10;

                                CommonLogClass.Instance.LogMessage("点胶启动Mirror1", Color.Black);
                                GdxCore.Trace("MirrorDispenser.Start", Process, 1);
                                break;
                            default:
                                bOK = false;
                                m_mainprocess.Stop();
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("点胶 未定义Mirror的值停止流程", Color.Red);
                                break;
                        }

                        if (bOK)
                        {
                            //设定点胶时间&UV时间
                            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1091, RecipeCHClass.Instance.DispensingTime);
                            MACHINE.PLCIO.SetMWIndex(IOConstClass.MW1092, RecipeCHClass.Instance.UVTime);

                            //避光槽位置 9上 7下
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 9, INI.Instance.ShadowPosUp);
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 7, INI.Instance.ShadowPos);

                            CommonLogClass.Instance.LogMessage("设定点胶时间 " + RecipeCHClass.Instance.DispensingTime.ToString() + " 毫秒", Color.Black);
                            CommonLogClass.Instance.LogMessage("设定UV时间 " + RecipeCHClass.Instance.UVTime.ToString() + " 秒", Color.Black);
                        }

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //开始循环设定 点胶位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 8, m_DispensingRunList[m_DispensingIndex]);

                            CommonLogClass.Instance.LogMessage("点胶位置设定 Index=" + m_DispensingIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //点胶启动
                            MACHINE.PLCIO.SetIO(IOConstClass.QB1541, true);

                            CommonLogClass.Instance.LogMessage("点胶启动 Index" + m_DispensingIndex.ToString(), Color.Black);
                            m_DispensingIndex++;

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 30;
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.IO.Wait", Process, IOConstClass.QB1541, false);

                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1541))
                            {
                                //单个点胶完成
                                if (m_DispensingIndex == 2)
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 3010;

                                    string _posStr = INI.Instance.ShadowPosUp;

                                    switch (Mirror_DispensingProcessIndex)
                                    {
                                        case 0:
                                            _posStr = INI.Instance.sMirror1ReadyPos;
                                            break;
                                        case 1:
                                            _posStr = INI.Instance.sMirror2ReadyPos;
                                            break;
                                    }

                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 7, _posStr);
                                    CommonLogClass.Instance.LogMessage((Mirror_DispensingProcessIndex == 0 ? "Mirror A" : "Mirror B") + " 待機位置=" + _posStr, Color.Black);

                                }
                                else if (m_DispensingIndex < m_DispensingRunList.Count)
                                {
                                    //m_DispensingIndex++;

                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 10;
                                }
                                else
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 40;

                                    
                                    //Process.Stop();
                                    CommonLogClass.Instance.LogMessage("点胶完成", Color.Black);
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_DISPENSING, 7, INI.Instance.ShadowPos);
                                    CommonLogClass.Instance.LogMessage("寫入點膠待機位置="+ INI.Instance.ShadowPosUp, Color.Black);
                                    //UVCylinder.Instance.SetFront();
                                }
                            }
                        }
                        break;


                    #region 插入流程 點完第二個點后 回避光槽位置 以便進行下面的點膠

                    case 3010:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                            {

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 3020;

                                //避光槽启动
                                MACHINE.PLCIO.SetIO(IOConstClass.QB1550, true);
                                CommonLogClass.Instance.LogMessage("運動至待機位置启动", Color.Black);
                            }
                        }
                        break;
                    case 3020:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.IO.Wait", Process, IOConstClass.QB1550, false);

                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1550) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("運動至待機位置完成", Color.Black);

                                CommonLogClass.Instance.LogMessage("下一個點膠編號=" + (m_DispensingIndex + 1).ToString(), Color.Black);

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 10;
                            }
                        }
                        break;



                    #endregion


                    #region INITIAL POS

                    case 40:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                            {

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 50;

                                //MACHINE.PLCIO.SetIO(IOConstClass.QB1625, true);
                                //MACHINE.PLCIO.SetIO(IOConstClass.QB1665, true);

                                //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                                //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                                //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);


                                //避光槽启动
                                MACHINE.PLCIO.SetIO(IOConstClass.QB1550, true);

                                CommonLogClass.Instance.LogMessage("運動至避光槽启动", Color.Black);
                            }
                        }
                        break;
                    case 50:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.IO.Wait", Process, IOConstClass.QB1550, false);

                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1550) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("運動至避光槽完成", Color.Black);
                                //Process.Stop();

                                UVCylinder.Instance.SetFront();

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4010;
                            }
                        }
                        break;

                    //case 40:
                    //    if (Process.IsTimeup)
                    //    {
                    //        //if (MACHINE.PLCIO.GetOutputIndex((int)DispensingAddressEnum.ADR_RESET_COMPLETE) || Universal.IsNoUseIO)
                    //        {

                    //            Process.NextDuriation = NextDurtimeTmp;
                    //            Process.ID = 50;

                    //            //MACHINE.PLCIO.SetIO(IOConstClass.QB1625, true);
                    //            //MACHINE.PLCIO.SetIO(IOConstClass.QB1665, true);

                    //            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                    //            MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                    //            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);

                    //            CommonLogClass.Instance.LogMessage("点胶模组初始化启动", Color.Black);
                    //        }
                    //    }
                    //    break;
                    //case 50:
                    //    if (Process.IsTimeup)
                    //    {
                    //        if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_DISPENSING, 6) || Universal.IsNoUseIO)
                    //        {
                    //            CommonLogClass.Instance.LogMessage("点胶模组初始化完成", Color.Black);
                    //            //Process.Stop();

                    //            UVCylinder.Instance.SetFront();

                    //            Process.NextDuriation = NextDurtimeTmp;
                    //            Process.ID = 4010;
                    //        }
                    //    }
                    //    break;

                    #endregion

                    case 4010:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.Actuator.Wait", Process, UVCylinder.Instance);

                            if (UVCylinder.Instance.GetFrontOK())
                            {
                                UV.Instance.Seton();
                                CommonLogClass.Instance.LogMessage("UV打开", Color.Black);
                                start_uv_timer();

                                Process.NextDuriation = RecipeCHClass.Instance.UVTime * 1000;
                                Process.ID = 4020;
                            }
                        }
                        break;
                    case 4020:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.Actuator.Wait", Process, UVCylinder.Instance);

                            if (UVCylinder.Instance.GetFrontOK())
                            {
                                UV.Instance.Setoff();
                                CommonLogClass.Instance.LogMessage("UV关闭", Color.Black);
                                UVCylinder.Instance.SetBack();

                                stop_uv_timer();
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4030;
                            }
                        }
                        break;
                    case 4030:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.Actuator.Wait", Process, UVCylinder.Instance, false);

                            if (UVCylinder.Instance.GetBackOK())
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 403000;

                                CommonLogClass.Instance.LogMessage("点胶固化完成", Color.Black);

                                MACHINE.PLCIO.SetOutputIndex(4, false);
                                MACHINE.PLCIO.SetOutputIndex(5, true);
                                CommonLogClass.Instance.LogMessage("產品破真空", Color.Black);
                            }
                        }
                        break;
                    case 403000:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.IO.Wait", Process, MACHINE.PLCIO.GetGaAddress("INPUT",6).Address0, false);

                            if (!MACHINE.PLCIO.GetInputIndex(6))
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 403001;

                                CommonLogClass.Instance.LogMessage("產品破真空完成", Color.Black);

                                //點膠結束后 U退至 -6000  thetaY和thetaZ 給當前位置 相當於不要動
                                string posStr = INI.Instance.sMirrorAdjBackLength.ToString() + ",";
                                posStr += MACHINECollection.GetSingleAXISPositionNow(7) + ",";
                                posStr += MACHINECollection.GetSingleAXISPositionNow(8);
                                MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 4, posStr);
                                CommonLogClass.Instance.LogMessage("微調模組後退位置寫入", Color.Black);
                                MACHINECollection.MotorSpeed();
                                CommonLogClass.Instance.LogMessage("所有軸速度寫入", Color.Black);
                            }
                        }
                        break;
                    case 403001:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorDispenser.Actuator.Wait", Process, UVCylinder.Instance, false);

                            if (UVCylinder.Instance.GetBackOK())
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 403002;

                                MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_ADJUST, 4);

                                CommonLogClass.Instance.LogMessage("微調模組後退位置定位", Color.Black);


                                string _posStrReady = MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK);

                                //插入判斷mirror 作業待命位置 節省時間
                                if (MainProcess.Instance.MainAloneToMirror)
                                {
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, _posStrReady);
                                    CommonLogClass.Instance.LogMessage("吸嘴待命位置寫入(單獨製作) " + _posStrReady, Color.Black);
                                }
                                else
                                {
                                    //switch (Mirror_DispensingProcessIndex)
                                    switch (MainProcess.Instance.CurRunCont)
                                    {
                                        case 0:
                                            _posStrReady = INI.Instance.sMirror1ToMirror2ReadyPos;
                                            CommonLogClass.Instance.LogMessage("吸嘴作業待命位置寫入 " + _posStrReady, Color.Black);
                                            break;
                                        case 1:
                                            _posStrReady = MACHINECollection.GetModulePositionForReady(ModuleName.MODULE_PICK);
                                            CommonLogClass.Instance.LogMessage("吸嘴待命位置寫入 " + _posStrReady, Color.Black);
                                            break;
                                    }
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 6, _posStrReady);
                                }
                            }
                        }
                        break;
                    case 403002:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_ADJUST, 4))
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4060;
                                CommonLogClass.Instance.LogMessage("微調模組後退完成", Color.Black);
                                
                                MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                                Set_Cooling_Module(false);
                                //CommonLogClass.Instance.LogMessage("微调模组待命完成", Color.Black);
                                CommonLogClass.Instance.LogMessage("吸嘴模组回待命启动", Color.Black);

                                //微調 和 吸嘴 同步回待命

                                MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                                CommonLogClass.Instance.LogMessage("微调模组待命啓動", Color.Black);

                            }
                        }
                        break;
                    case 4040:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 4050;

                                CommonLogClass.Instance.LogMessage("吸嘴模组待命完成", Color.Black);

                                MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                                CommonLogClass.Instance.LogMessage("微调模组待命啓動", Color.Black);

                            }
                        }
                        break;

                    case 4050:
                        if (Process.IsTimeup)
                        {
                            if ((MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_ADJUST, 6) && MACHINECollection.AdjustReadyPositionOK()) || Universal.IsNoUseIO)
                            {
                                Process.Stop();

                                CommonLogClass.Instance.LogMessage("微调模组待命完成", Color.Black);
                            }
                        }
                        break;

                    case 4060:
                        if (Process.IsTimeup)
                        {
                            if ((MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) &&
                                MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_ADJUST, 6) &&
                                MACHINECollection.AdjustReadyPositionOK()) ||
                                Universal.IsNoUseIO)
                            {
                                Process.Stop();

                                CommonLogClass.Instance.LogMessage("吸嘴模组待命完成", Color.Black);
                                CommonLogClass.Instance.LogMessage("微调模组待命完成", Color.Black);
                            }
                        }
                        break;
                }
            }
        }


        #region PRIVATE_FUNCTIONS

        /// <summary>
        /// 利用 Threading.Timer, 在 UV照射期間, 
        /// 每秒 發出 一筆 event, 
        /// 以防止被誤認為程式當掉
        /// </summary>
        private void start_uv_timer()
        {
            if (m_uvTimer == null)
            {
                m_uvTimerCount = 0;
                m_uvTimer = new System.Threading.Timer(
                    new System.Threading.TimerCallback((arg) =>
                    {
                        try
                        {
                            // 使用 LOG 會產生很多筆 重複的紀錄
                            // CommonLogClass.Instance.LogMessage("UV照射中", Color.Purple);
                            // string msg = string.Format("UV 照射中 (第{0}秒)", ++m_uvTimerCount);
                            int totalSecs = RecipeCHClass.Instance.UVTime;
                            FireMessage($"UV.照射中 ({++m_uvTimerCount}秒 / {totalSecs}秒)");
                            // Force To Close the residual display label.
                            System.Threading.Thread.Sleep(500);
                            if (!IsOn)
                                FireMessage("UV.Off");
                        }
                        catch
                        {
                        }
                    }));
                m_uvTimer.Change(0, 1000);
            }
        }
        private void stop_uv_timer()
        {
            try
            {
                if (m_uvTimer != null)
                {
                    m_uvTimer.Dispose();
                    m_uvTimer = null;
                    FireMessage("UV.Off");
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
