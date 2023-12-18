using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_Measure;
using JetEazy.BasicSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System;
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
                // return m_indexer.GroupID = m_mainprocess.MainGroupIndex;
                return m_mainprocess.MainGroupIndex;
            }
        }

        /// <summary>
        /// The running index of m_PlaneRunList
        /// </summary>
        int m_PlaneIndex = 0;
        
        /// <summary>
        /// 缓存平面度需要到达的位置 <br/>
        /// (will be load from INI)
        /// </summary>
        List<string> m_PlaneRunList = new List<string>();

        /// <summary>
        /// 有效Laser量測的點位數據 <br/>
        /// The collection of those well laser-measured points
        /// </summary>
        List<QPoint3D> m_wellLaserMeasuredList = new List<QPoint3D>();

        /// <summary>
        /// 记录拾取第几组
        /// </summary>
        int m_PickIndex = 0;
        /// <summary>
        /// 记录放入哪一个Mirror
        /// </summary>
        int m_PickMirrorIndex = 0;
        #endregion

        #region PRIVATE_DATA_LT
        //class XPlanePtsCollector
        //{
        //    List<QVector> GoldenPos = new List<QVector>();
        //    List<QVector> RunPos = new List<QVector>();
        //    int RunIndex
        //    {
        //        get { return RunPos.Count; }
        //    }
        //}
        //XPlanePtsCollector m_xplaner = new XPlanePtsCollector();
        //MirrorIndexer m_indexer = new MirrorIndexer();
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

        public override void Start(params object[] args)
        {
            base.Start(args);
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
                        #region INIT_START

                        Process.NextDuriation = NextDurtimeTmp;
                        m_wellLaserMeasuredList.Clear();
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
                            Set_Cooling_Module(true);
                            Process.ID = 10;
                            CommonLogClass.Instance.LogMessage("拾取启动 Group Index=" + MainGroupIndex.ToString(), Color.Black);
                            GdxCore.Trace("MirrorPicker.Start", Process, m_PickMirrorIndex, MainGroupIndex);
                            GdxCore.InitLaserCoordsTransform();
                            GdxCore.ResetLaserPts(m_PickMirrorIndex);
                            FireMessage("Picker.Start");
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
                            //> 开始循环设定 产品 平面度位置
                            GdxCore.Trace("MirrorPicker.LaserRun", Process, "mirrorIdx", m_PickMirrorIndex, "pointIdx", m_PlaneIndex, "pos", m_PlaneRunList[m_PlaneIndex]);
                            _LOG("雷射量測", "點位", m_PlaneIndex, "寫入馬達定位目標位置");

                            MACHINE.PLCIO.ModulePositionSet(ModuleName.MODULE_PICK, 1, m_PlaneRunList[m_PlaneIndex]);

                            //> CommonLogClass.Instance.LogMessage("产品平面度位置设定 Index=" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            _LOG("雷射量測", "點位", m_PlaneIndex, "啟動馬達");
                            MACHINE.PLCIO.ModulePositionGO(ModuleName.MODULE_PICK, 1);

                            //> CommonLogClass.Instance.LogMessage("启动 Index" + m_PlaneIndex.ToString(), Color.Black);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 2010;
                        }
                        break;
                    case 2010:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //定位完成 延迟一段时间 读取laser的值
                                //BM 20231113 提出
                                Process.NextDuriation = INI.Instance.ReadLaserDelaytime;
                                Process.ID = 30;
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (MACHINE.PLCIO.ModulePositionIsComplete(ModuleName.MODULE_PICK, 1))
                            {
                                //读数据 
                                //@LETIAN: 雷射讀值 (命名 laserZ 以防與 馬達 XYZ 搞混)
                                double laserZ = ax_read_laser();
                                bool isLaserNG = (Math.Abs(laserZ) < 0.0001);

                                if (isLaserNG)
                                {
                                    var cur = ax_read_current_motor_pos();

                                    if (m_PlaneIndex >= m_PlaneRunList.Count - 1)
                                    {
                                        _LOG("雷射讀值異常 (已用完INI設定點位)", laserZ, "@XYZ", cur, Color.Red);
                                        Process.NextDuriation = NextDurtimeTmp;
                                        Process.ID = 3020;
                                        return;
                                    }
                                    else
                                    {
                                        //@LETIAN: 跳選下一個雷射測點
                                        _LOG("雷射讀值不佳 (準備使用下一個合適點位)", laserZ, "@XYZ", cur, Color.Black);

                                        GdxCore.Trace("MirrorPicker.LaserSkip", Process);
                                        m_PlaneIndex++;

                                        Process.NextDuriation = NextDurtimeTmp;
                                        Process.ID = 10;
                                        return;
                                    }
                                }

                                //读数据的xyz位置 提取yz 作为平面度的xy
                                string[] plane_xyz = m_PlaneRunList[m_PlaneIndex].Split(',').ToArray();

                                //组合新位置 用于计算平面度
                                //>>> string planeNew_xyz = plane_xyz[1] + "," + plane_xyz[2] + "," + laserZ.ToString("0.000");
                                var x = double.Parse(plane_xyz[1]);
                                var y = double.Parse(plane_xyz[2]);
                                var measuredPoint = new QPoint3D(x, y, laserZ);
                                m_wellLaserMeasuredList.Add(measuredPoint);

                                //> CommonLogClass.Instance.LogMessage("Index=" + m_PlaneIndex.ToString() + ":" + planeNew_xyz, Color.Black);

                                //@LETIAN: LOG 馬達 當下位置 讀值 (for trace and debug)
                                var curMotorPos = ax_read_current_motor_pos();
                                _LOG("雷射量測", "點位", m_PlaneIndex, "馬達XYZ", curMotorPos, "Laser量測值", laserZ.ToString("0.000"));

                                #region LETIAN_DEBUG
#if OPT_LETIAN_DEBUG
                                //@LETIAN: 比對 馬達 與 ini 設定值 (比對精度 0.001 mm)
                                var iniMotorPos = QVector.Parse(m_PlaneRunList[m_PlaneIndex]);
                                if (!QVector.AreEqual(curMotorPos, iniMotorPos, 3))
                                {
                                    _LOG("異常", "馬達定位與INI設定有誤差", Color.Red);
                                    Process.NextDuriation = NextDurtimeTmp;
                                    Process.ID = 3010;
                                    return;
                                }
#endif
                                #endregion

                                //@LETIAN: 把量測到的雷射點位 存入 GdxCore Module 內
                                GdxCore.CollectLaserPt(m_PickMirrorIndex, m_PlaneIndex, laserZ, m_PlaneRunList[m_PlaneIndex]);

                                //@LETIAN: 下一個雷射測點
                                GdxCore.Trace("MirrorPicker.LaserNext", Process);
                                m_PlaneIndex++;

                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 40;
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            //---------------------------------------------------
                            //@LETIAN: 2023-12-18 BM提議後修改的規則:
                            //   嘗試跑完 INI所設定的 點位過程中,
                            //   只要累積有 "3個" 正常的雷射量測點,
                            //   就可進行平面計算.
                            // PS:
                            //   m_PlaneRunDataList 只收集好的laser量測點
                            //---------------------------------------------------
                            bool hasEnoughPoints = m_wellLaserMeasuredList.Count >= 3;

                            if (!hasEnoughPoints)
                            {
                                Process.NextDuriation = NextDurtimeTmp;
                                Process.ID = 10;
                            }
                            else
                            {
                                //读取数据完成
                                //计算平面度
                                bool bOK = true;

                                //首先判断块规资料是否超过3个 不超过则NG
                                if (INI.Instance.Mirror0PlaneHeightPosList.Count >= 3)
                                {
                                    //@LETIAN: 告知 GdxCore 模組, 已經標測完雷射點位.
                                    GdxCore.BuildLaserCoordsTransform(m_PickMirrorIndex);

                                    //@GAARA: 平面度計算
                                    string myHeightStr = ga_calculate_planeness();

                                    //@LETIAN: 原有的代碼, 計算平面度之後,
                                    //         得到 myHeightStr 只顯示於LOG,
                                    //         卻沒有 判定 Pass 或 NG ?
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

                                    double L = GdxCore.GetQCMotorPos(this.m_PickMirrorIndex, out double X, out double Y, out double Z);
                                    _LOG("mirror", m_PickMirrorIndex, "QC 馬達座標", new QVector(X, Y, Z), "Laser 期望值", L.ToString("0.000"), Color.Purple);
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


                    #region 吸料流程_and_移到测试偏移位置_02預備位置

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
                            //GdxCore.Trace("MirrorPicker.IniData", Process, "Mirror1", "PosList", "Pts", INI.Instance.Mirror1PosList);
                            //GdxCore.Trace("MirrorPicker.IniData", Process, "Mirror1", "CaliPos", INI.Instance.Mirror1CaliPos);
                            //GdxCore.Trace("MirrorPicker.IniData", Process, "MirrorAdjDeep1", "Length", INI.Instance.sMirrorAdjDeep1Length);
                            //GdxCore.Trace("MirrorPicker.IniData", Process, "sMirrorAdjBack", "Length", INI.Instance.sMirrorAdjBackLength);

                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 60;
                        }
                        break;
                    case 60:
                        if (Process.IsTimeup)
                        {
                            //开始启动

                            MACHINE.PLCIO.SetIO(IOConstClass.QB1542, true);
                            MACHINE.PLCIO.ADR_SMALL_LIGHT = true;   //測試偏移的燈 提前打開
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

                    #endregion


                    #region INITIAL POS

                    case 3010:  //@LETIAN: 馬達定位沒到達 INI 所指定位置 (重複利用 4010 退出程序)
                    case 3020:  //@LETIAN: 雷射讀值異常 
                    case 4010:  //平面度超標
                        if (Process.IsTimeup)
                        {
                            Process.NextDuriation = NextDurtimeTmp;
                            Process.ID = 4020;
                            Set_Cooling_Module(false);
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


        /// <summary>
        /// 平面度計算 (I) <br/>
        /// 從 INI 的設定, 計算出 基準 平面
        /// (將 gaara 的源碼整理, 以方便閱讀)
        /// </summary>
        private QPlane ga_get_reference_plane_from_ini()
        {
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
            return myPlane;
        }

        /// <summary>
        /// 平面度計算 (II) <br/>
        /// 計算 laser 量測平面, 與 INI 基準平面 的差異.
        /// (將 gaara 的源碼整理, 以方便閱讀)
        /// </summary>
        private string ga_calculate_planeness()
        {
            var myPlane = ga_get_reference_plane_from_ini();
            string myHeightStr = string.Empty;

            foreach (var measuredPoint in m_wellLaserMeasuredList)
            {
                //string[] runStrPlane = str.Split(',').ToArray();
                //QPoint3D run = new QPoint3D(double.Parse(runStrPlane[0]), double.Parse(runStrPlane[1]), double.Parse(runStrPlane[2]));

                double runheight = myPlane.GetZLocation(measuredPoint);

                //CommonLogClass.Instance.LogMessage("平面度测试正常", Color.Lime);
                myHeightStr += runheight.ToString(" 0.000") + ",";
            }
            return myHeightStr;
        }

        /// <summary>
        /// 讀取雷測量測距離 <br/>
        /// @LETIAN: 包裝為 function 準備將來模擬使用
        /// </summary>
        private double ax_read_laser()
        {
            if (!GdxGlobal.Facade.IsSimPLC())
            {
                return LEClass.Instance.Snap();
            }
            else
            {
                //@ Gaara 看以後是否由 LEClass 直接 Math.Round
                var laser = GdxGlobal.Facade.GetLaser();
                double dist = laser.Snap();
                return System.Math.Round(dist, 4);
            }
        }

        /// <summary>
        /// 讀取馬達XYZ位置 (單位 mm)<br/>
        /// @LETIAN: 2022/06/25
        /// </summary>
        private QVector ax_read_current_motor_pos()
        {
            int N = 3;
            var pos = new QVector(N);
            for (int i = 0; i < N; i++)
            {
                var axis = GetAxis(i);
                pos[i] = axis.GetPos();
            }
            return pos;
        }
    }
}
