using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.OPSpace;
using JetEazy.BasicSpace;
using JetEazy.GdxCore3;
using JetEazy.ProcessSpace;
using System.Drawing;
using System.Linq;

namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 中心點偏移補償流程
    /// @LETIAN: 20220619 重新包裝
    /// </summary>
    public class MirrorCalibProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        BaseProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        #endregion

        #region PRIVATE_DATA
        /// <summary>
        /// 判断校正组 跑哪一个Mirror 左边还是右边
        /// </summary>
        int Mirror_CalibrateProcessIndex = 0;
        #endregion

        #region SINGLETON
        static MirrorCalibProcess _singleton = null;
        private MirrorCalibProcess()
        {
        }
        #endregion

        public static MirrorCalibProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MirrorCalibProcess();
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
                //> GdxCore.Trace("MirrorCalibration.Tick", Process, 0);

                switch (Process.ID)
                {
                    case 5:
                        Process.NextDuriation = NextDurtimeTmp;
                        switch (Process.RelateString)
                        {
                            case "0":
                                Mirror_CalibrateProcessIndex = 0;

                                Process.ID = 10;
                                MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                                CommonLogClass.Instance.LogMessage("校正启动Mirror0", Color.Black);
                                GdxCore.Trace("MirrorCalibration.Start", Process, 0);
                                break;
                            case "1":
                                Mirror_CalibrateProcessIndex = 1;

                                Process.ID = 10;
                                MACHINE.PLCIO.ADR_SMALL_LIGHT = true;
                                CommonLogClass.Instance.LogMessage("校正启动Mirror1", Color.Black);
                                GdxCore.Trace("MirrorCalibration.Start", Process, 1);
                                break;
                            default:
                                m_mainprocess.Stop();
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("校正 未定义Mirror的值停止流程", Color.Red);
                                break;
                        }
                        break;

                    case 10:
                        if (Process.IsTimeup)
                        {
                            //if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //到达位置 打开灯光 设定曝光

                                ICamForCali.SetExposure(RecipeCHClass.Instance.CaliCamExpo);

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 20;

                            }
                        }
                        break;

                    case 20:
                        if (Process.IsTimeup)
                        {
                            CommonLogClass.Instance.LogMessage("擷取影像", Color.Black);

                            ICamForCali.Snap();
                            Bitmap bmp = new Bitmap(ICamForCali.GetSnap());
                            bmp.Save("image0.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                            //> ==============================================
                            //> 使用 event 通知 GUI 更新 Bitmap
                            //> Receiver 必須負責 Bitmap 的 LifeCycle !!!
                            //> ==============================================
                            //> m_DispUI.SetDisplayImage(bmp);
                            var e = new ProcessEventArgs()
                            {
                                Message = "Image.Captured",
                                Tag = bmp
                            };
                            FireMessage(e);

                            //> ==============================================
                            //计算偏移值
                            //参数中算的解析度或是手动输入
                            //参数中先记录Mirror的中心位置
                            //测试中算的Mirror的中心位置
                            //计算两个中心位置之差
                            //补偿的是吸嘴模组的y和z轴 相当于画面中的 x和y 
                            //画面中向左为正 向下为正
                            //> ==============================================
                            PointF ptfOffset = new PointF(0, 0);
                            ptfOffset.X -= RecipeCHClass.Instance.CaliPicCenter.X;
                            ptfOffset.Y -= RecipeCHClass.Instance.CaliPicCenter.Y;

                            //补偿放入的位置
#if (false)
                            string posPutAdjust = string.Empty;
                            string mirrorPutPos = string.Empty;
                            switch (Mirror_CalibrateProcessIndex)
                            {
                                case 0:
                                    mirrorPutPos = INI.Instance.Mirror1PutPos;
                                    posPutAdjust = ToolAdjustData(INI.Instance.Mirror1PutPos, ptfOffset.X, ptfOffset.Y);
                                    break;
                                case 1:
                                    mirrorPutPos = INI.Instance.Mirror2PutPos;
                                    posPutAdjust = ToolAdjustData(INI.Instance.Mirror2PutPos, ptfOffset.X, ptfOffset.Y);
                                    break;
                            }
#else
                            string mirrorPutPos = string.Empty;
                            switch (Mirror_CalibrateProcessIndex)
                            {
                                case 0:
                                    mirrorPutPos = INI.Instance.Mirror1PutPos;
                                    break;
                                case 1:
                                    mirrorPutPos = INI.Instance.Mirror2PutPos;
                                    break;
                            }
                            string posPutAdjust = ToolAdjustData(mirrorPutPos, ptfOffset.X, ptfOffset.Y);
#endif

                            GdxCore.Trace("MirrorCalibration.Compensate", Process, bmp, mirrorPutPos, ptfOffset);
                            bool go = GdxCore.CheckCompensate(Process, bmp, mirrorPutPos, ptfOffset, 20f);
                            if (!go)
                            {

                            }


                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 3, posPutAdjust);

                            MACHINE.PLCIO.ADR_SMALL_LIGHT = false;

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 3010;

                        }
                        break;

                    case 3010:
                        if (Process.IsTimeup)
                        {
                            MACHINE.PLCIO.SetIO(IOConstClass.QB1543, true);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 30;
                        }
                        break;

                    case 30:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorCalibration.IO.Wait", Process, "QB1543", false);

                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1543))
                            {
                                //微調模組到達 0的位置 方便下面 微調
                                CommonLogClass.Instance.LogMessage("吸嘴模组到达位置", Color.Black);

                                switch (Mirror_CalibrateProcessIndex)
                                {
                                    case 0:
                                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, "0,0,0");
                                        break;
                                    case 1:
                                        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, "0,0,0");
                                        break;
                                }

                                //switch (Mirror_CalibrateProcessIndex)
                                //{
                                //    case 0:
                                //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, INI.Instance.sMirrorPutAdjDeep1Length + ",0,0");
                                //        break;
                                //    case 1:
                                //        MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, INI.Instance.sMirrorPutAdjDeep2Length + ",0,0");
                                //        break;
                                //}


                                //MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 1, INI.Instance.sMirrorPutAdjDeep1Length + ",0,0");
                                MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_ADJUST, 1);

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 40;
                            }
                        }
                        break;

                    case 40:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_ADJUST, 1))
                            {
                                CommonLogClass.Instance.LogMessage("微调模组到达位置", Color.Black);
                                Process.Stop();
                                CommonLogClass.Instance.LogMessage("校正完成", Color.Black);
                                GdxCore.Trace("MirrorCalibration.Completed", Process);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 工具 对位置进行补偿
        /// </summary>
        /// <param name="eOrg">原始位置 格式x,y,z</param>
        /// <param name="eOffsetX">x补偿</param>
        /// <param name="eOffsetY">y补偿</param>
        /// <returns>补偿后的位置 格式x,y,z</returns>
        private string ToolAdjustData(string eOrg, float eOffsetX, float eOffsetY)
        {
            string[] orgs = eOrg.Split(',').ToArray();
            float x = float.Parse(orgs[1]) + eOffsetX;
            float y = float.Parse(orgs[2]) + eOffsetY;
            string res = orgs[0] + "," + x.ToString() + "," + y.ToString();
            return res;
        }

    }
}
