using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_Measure;
using JetEazy.BasicSpace;
using JetEazy.GdxCore3;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// PICKER <br/>
    /// @LETIAN: 20220619 重新包裝
    /// </summary>
    public class MirrorPickProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        MainProcess m_mainprocess
        {
            get { return MainProcess.Instance; }
        }
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        #endregion

        #region PRIVATE_DATA
        int MainGroupIndex
        {
            get
            {
                return m_mainprocess.MainGroupIndex;
            }
        }

        int m_PlaneIndex = 0;
        /// <summary>
        /// 缓存平面度需要到达的位置 <br/>
        /// (will be load from INI)
        /// </summary>
        List<string> m_PlaneRunList = new List<string>();
        /// <summary>
        /// 缓存平面度量测的高度 <br/>
        /// (z coordinate will be measured by laser)
        /// </summary>
        List<string> m_PlaneRunDataList = new List<string>();

        /// <summary>
        /// 记录拾取第几组
        /// </summary>
        int m_PickIndex = 0;
        /// <summary>
        /// 记录放入哪一个Mirror
        /// </summary>
        int m_PickMirrorIndex = 0;
        #endregion

        #region SINGLETON
        static MirrorPickProcess _singleton = null;
        private MirrorPickProcess()
        {
        }
        #endregion

        public static MirrorPickProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new MirrorPickProcess();
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
                        #region INIT_START

                        Process.NextDuriation = NextDurtimeTmp;
                        m_PlaneRunDataList.Clear();
                        m_PlaneIndex = 0;
                        m_PlaneRunList.Clear();

                        bool bInitOK = true;

                        // 指定 m_PlaneRunList 是來自 INI.Instance.Mirror<i+1>PlanePosList
                        // 檢查 MainGroupIndex 是否在 INI.Instance.Mirror<i+1>PosList.Count 範圍內.
                        switch (Process.RelateString)
                        {
                            case "0":
                                m_PickMirrorIndex = 0;

                                foreach (string str in INI.Instance.Mirror1PlanePosList)
                                    m_PlaneRunList.Add(str);

                                //CommonLogClass.Instance.LogMessage("校正启动Mirror0", Color.Lime);

                                if (MainGroupIndex >= INI.Instance.Mirror1PosList.Count)
                                {
                                    bInitOK = false;
                                }

                                break;
                            case "1":
                                m_PickMirrorIndex = 1;

                                foreach (string str in INI.Instance.Mirror2PlanePosList)
                                    m_PlaneRunList.Add(str);

                                //CommonLogClass.Instance.LogMessage("校正启动Mirror1", Color.Lime);

                                if (MainGroupIndex >= INI.Instance.Mirror2PosList.Count)
                                {
                                    bInitOK = false;
                                }

                                break;
                            default:

                                bInitOK = false;

                                break;
                        }

                        if (bInitOK)
                        {
                            Process.ID = 10;
                            CommonLogClass.Instance.LogMessage("拾取启动 index=" + MainGroupIndex.ToString(), Color.Black);
                            GdxCore.Trace("MirrorPicker.Start", Process, m_PickMirrorIndex, MainGroupIndex);
                        }
                        else
                        {
                            m_mainprocess.Stop();
                            Process.Stop();
                            CommonLogClass.Instance.LogMessage("拾取 未定义Mirror的值停止流程", Color.Red);
                        }
                        #endregion
                        break;

                    #region 测试平面度

                    case 10:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorPicker.PlanRun", Process, "PlaneIdx", m_PlaneIndex, "Pt", m_PlaneRunList[m_PlaneIndex]);

                            //开始循环设定 产品 平面度位置
                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, m_PlaneRunList[m_PlaneIndex]);

                            CommonLogClass.Instance.LogMessage("产品平面度位置设定 Index=" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            //GO
                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);
                            CommonLogClass.Instance.LogMessage("启动 Index" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 30;
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {                            
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //读数据 
                                double z = LEClass.Instance.Snap();
                                //读数据的xyz位置 提取yz 作为平面度的xy
                                string[] plane_xyz = m_PlaneRunList[m_PlaneIndex].Split(',').ToArray();

                                //组合新位置 用于计算平面度
                                string planeNew_xyz = plane_xyz[1] + "," + plane_xyz[2] + "," + z.ToString();
                                m_PlaneRunDataList.Add(planeNew_xyz);

                                //> GdxCore.Trace("MirrorPicker.MarkLaserDist", Process, "PlaneIdx", m_PlaneIndex, "laserDist", z, "Pts", plane_xyz);
                                GdxCore.MarkLaserOnRunPiece(MainGroupIndex, m_PickIndex, m_PlaneIndex, z, plane_xyz);
                                CommonLogClass.Instance.LogMessage("Index=" + m_PlaneIndex.ToString() + ":" + planeNew_xyz, Color.Black);

                                m_PlaneIndex++;

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 40;
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            //读取数据完成
                            if (m_PlaneIndex < m_PlaneRunList.Count)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 10;
                            }
                            else
                            {
                                //计算平面度

                                bool bOK = true;

                                //首先判断块规资料是否超过3个 不超过则NG
                                
                                if (INI.Instance.Mirror0PlaneHeightPosList.Count >= 3)
                                {
                                    GdxCore.Trace("MirrorPicker.CheckPlane", Process, "Mirror0PlaneHeightPosList", "Pts", INI.Instance.Mirror0PlaneHeightPosList);
                                    GdxCore.SetGaugeBlockPlanePoints(INI.Instance.Mirror0PlaneHeightPosList);

                                    QPoint3D[] _planeheight = new QPoint3D[INI.Instance.Mirror0PlaneHeightPosList.Count];
                                    int i = 0;
                                    while (i < INI.Instance.Mirror0PlaneHeightPosList.Count)
                                    {
                                        string _strplane = INI.Instance.Mirror0PlaneHeightPosList[i];
                                        string[] _strxyz = _strplane.Split(',').ToArray();
                                        _planeheight[i] = new QPoint3D(double.Parse(_strxyz[0]), double.Parse(_strxyz[1]), double.Parse(_strxyz[2]));
                                        i++;
                                    }

                                    QPlane myPlane = new QPlane();
                                    myPlane.LeastSquareFit(_planeheight);

                                    string myHeightStr = string.Empty;

                                    foreach (string str in m_PlaneRunDataList)
                                    {
                                        string[] runStrPlane = str.Split(',').ToArray();
                                        QPoint3D run = new QPoint3D(double.Parse(runStrPlane[0]), double.Parse(runStrPlane[1]), double.Parse(runStrPlane[2]));
                                        double runheight = myPlane.GetZLocation(run);

                                        //CommonLogClass.Instance.LogMessage("平面度测试正常", Color.Lime);
                                        myHeightStr += runheight.ToString() + ",";
                                    }

                                    CommonLogClass.Instance.LogMessage(myHeightStr, Color.Black);
                                    bOK = true;

                                }
                                else
                                {
                                    bOK = false;
                                }

                                if (bOK)
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 50;

                                    CommonLogClass.Instance.LogMessage("平面度测试正常", Color.Black);
                                }
                                else
                                {
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 4010;

                                    CommonLogClass.Instance.LogMessage("平面度超标", Color.Red);
                                }

                            }
                        }
                        break;

                    #endregion

                    case 50:
                        if (Process.IsTimeup)
                        {
                            //开始吸料流程 及 到达测试偏移位置
                            GdxCore.Trace("MirrorPicker.SetPostions $$$ begin", Process, "GroupIndex", MainGroupIndex);

                            switch (m_PickMirrorIndex)
                            {
                                case 0:
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 5, INI.Instance.Mirror1PosList[MainGroupIndex]);
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 5, INI.Instance.sMirrorAdjDeep1Length.ToString() + ",0,0");
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 4, INI.Instance.Mirror1CaliPos);
                                    break;
                                case 1:
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 5, INI.Instance.Mirror2PosList[MainGroupIndex]);
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 5, INI.Instance.sMirrorAdjDeep2Length.ToString() + ",0,0");
                                    MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 4, INI.Instance.Mirror2CaliPos);
                                    break;
                            }


                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_ADJUST, 4, INI.Instance.sMirrorAdjBackLength.ToString() + ",0,0");

                            CommonLogClass.Instance.LogMessage("吸嘴 测试偏移 位置写入", Color.Black);
                            GdxCore.Trace("MirrorPicker.SetPostions $$$ end", Process);

                            GdxCore.Trace("MirrorPicker.IniData", Process, "Mirror1", "PosList", "Pts", INI.Instance.Mirror1PosList);
                            GdxCore.Trace("MirrorPicker.IniData", Process, "Mirror1", "CaliPos", INI.Instance.Mirror1CaliPos);
                            GdxCore.Trace("MirrorPicker.IniData", Process, "MirrorAdjDeep1", "Length", INI.Instance.sMirrorAdjDeep1Length);
                            GdxCore.Trace("MirrorPicker.IniData", Process, "sMirrorAdjBack", "Length", INI.Instance.sMirrorAdjBackLength);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 60;
                        }
                        break;
                    case 60:
                        if (Process.IsTimeup)
                        {
                            //开始启动

                            MACHINE.PLCIO.SetIO(IOConstClass.QB1542, true);
                            MACHINE.PLCIO.ADR_SMALL_LIGHT = true;//測試偏移的燈 提前打開
                            CommonLogClass.Instance.LogMessage("拾取及到达测试位置 启动", Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 70;

                        }
                        break;
                    case 70:
                        if (Process.IsTimeup)
                        {
                            GdxCore.Trace("MirrorPicker.IO.Wait", Process, "QB1542", false);

                            if (!MACHINE.PLCIO.GetIO(IOConstClass.QB1542))
                            {
                                CommonLogClass.Instance.LogMessage("拾取及到达测试位置 完成", Color.Black);

                                Process.Stop();
                                FireCompleted();
                            }
                        }
                        break;

                    #region INITIAL POS

                    case 4010:  //平面度超標
                        if (Process.IsTimeup)
                        {
                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 4020;

                            MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_PICK, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_DISPENSING, 6);
                            //MACHINE.PLCIO.ModulePositionReady(ModuleName.MODULE_ADJUST, 6);
                            CommonLogClass.Instance.LogMessage("吸嘴模组回待命启动", Color.Black);
                        }
                        break;
                    case 4020:  //吸嘴模组待命完成
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsReadyComplete(ModuleName.MODULE_PICK, 6) || Universal.IsNoUseIO)
                            {
                                CommonLogClass.Instance.LogMessage("吸嘴模组待命完成", Color.Black);

                                //平面度超标 模组1 归位  停止主流程
                                //if (m_mainprocess.IsOn)
                                m_mainprocess.Stop();

                                Process.Stop();

                            }
                        }
                        break;

                        #endregion

                }
            }
        }
    }
}
