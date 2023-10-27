
using Eazy_Project_III;
using Eazy_Project_Interface;
using JetEazy.BasicSpace;
using JetEazy.Drivers.Laser;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;



namespace JetEazy.GdxCore3
{
    public class GdxCore
    {
        #region CONFIGURATION_組態設定
        static bool OPT_BYPASS_CORETRON_1 = false;
        static bool OPT_BYPASS_CORETRON_2 = false;
        #endregion


        public static void Init()
        {
            GdxGlobal.Init();
            var xyzl = GdxGlobal.INI.GaugeBlockPlanePoints;
            System.Diagnostics.Trace.WriteLine(xyzl);
            GdxGlobal.Facade.LaserCoordsTransform.BuildGoldenPlaneFormula();
        }
        public static void Dispose()
        {
            GdxGlobal.LOG.Trace("GdxCore.Dispose");
            GdxGlobal.Dispose();
        }
        public static IxLaser GetLaser(int id = 0)
        {
            return GdxGlobal.GetLaser(id);
        }


        #region PUBLIC_中光電DLL之相關接口

        public static string GetDllVersion()
        {
            try
            {
                StringBuilder version = new StringBuilder(20);
                CoretronicsAPI.getVersion(version);
                return version.ToString();
            }
            catch (Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "中光電 DLL 異常!");                
                return "unknown";
            }
        }

        public static bool UpdateParams()
        {
            try
            {
                bool ok = CoretronicsAPI.updateParams();

                if (GdxGlobal.Facade.IsSimPLC())
                {
                    //> ok = false; // 故意報錯測試
                }

                return ok;
            }
            catch (Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "中光電 DLL 異常!");
                return false;
            }
        }

        /// <summary>
        /// Mirror 與 CompType + Color 轉換
        /// </summary>
        public static int getProjCompType(int mirrorIdx, out Color color)
        {
            int projCompType;
            if (mirrorIdx == 0)     
            {
                // 紅鏡片 or 紅斑
                projCompType = 1;       
                color = Color.DarkOrange;
            }
            else
            {
                // 綠鏡片 or 綠斑
                projCompType = 0;     
                color = Color.DarkGreen;
            }
            return projCompType;
        }

        /// <summary>
        /// 中光電 Center Compensation <br/>
        /// projCompType : 0==綠, 1==紅
        /// </summary>
        public static bool CheckCenterCompensation(int projCompType, Bitmap bmpSrc)
        {
            if (OPT_BYPASS_CORETRON_1)
            {
                CommonLogClass.Instance.LogMessage("GdxCore.CheckCompensate BYPASS", Color.DarkRed);
                return true;
            }

            try
            {
                bool go = false;

                CoretronicsAPI.setCenterCompInitial();

                // JUST for safety-verification
                // using (Bitmap bmp = new Bitmap(bmpSrc))
                var bmp = bmpSrc;
                {
                    var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    var bmpd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    CoretronicsAPI.setCenterCompImg(rect.Width, rect.Height, 3, bmpd.Scan0, projCompType);
                    bmp.UnlockBits(bmpd);
                }

                // 改為直接由 CenterCompProcess 取得 go/go-no
                go = CoretronicsAPI.CenterCompProcess();

                //> go = CoretronicsAPI.getCenterCompInfo();

                return go;
            }
            catch (Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "中光電 DLL 異常!");
                CommonLogClass.Instance.LogMessage("中光電 DLL 調用異常", Color.DarkRed);
                return false;
            }
        }

        /// <summary>
        /// 中光電 Center Compensation 之 圖示結果 <br/>
        /// projCompType : 0==綠, 1==紅
        /// </summary>
        public static CoreCompInfo GetCenterCompensationInfo(int projCompType)
        {
            var rect = getCenterInfo(projCompType);
            var info = new CoreCompInfo(projCompType, rect);
            return info;
        }

        /// <summary>
        /// 中光電 Projection Compensation (光斑投影) <br/>
        /// projCompType : 0==綠, 1==紅 <br/>
        /// motorParams : 返回 pixel differences. (2022/07/04 修改)
        /// </summary>
        public static bool CalcProjCompensation(Bitmap bmpSrc, int[] motorParams, int projCompType)
        {
            if (OPT_BYPASS_CORETRON_2)
            {
                CommonLogClass.Instance.LogMessage("GdxCore.CalcProjCompensation BYPASS", Color.Orange);
                return true;
            }

            try
            {
                CoretronicsAPI.setProjCompInitial();

                // JUST for safety-verification
                // using (Bitmap bmp = new Bitmap(bmpSrc))
                var bmp = bmpSrc;
                {
                    var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    var bmpd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    CoretronicsAPI.setProjCompImg(rect.Width, rect.Height, 3, bmpd.Scan0, projCompType);
                    bmp.UnlockBits(bmpd);
                }

                bool ok = CoretronicsAPI.ProjCompProcess();
                CoretronicsAPI.getProjCompInfo(projCompType, motorParams);

#if(OPT_SIM)
                sim_projection(ref ok, motorParams);
#endif

                return ok;
            }
            catch (Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "中光電 DLL 異常!");
                CommonLogClass.Instance.LogMessage("中光電 DLL 調用異常", Color.DarkRed);
                return false;
            }
        }

        /// <summary>
        /// 中光電 Projection Compensation (光斑投影) 之 圖示結果 <br/>
        /// projCompType : 0==綠, 1==紅
        /// </summary>
        public static CoreCompInfo GetProjCompensationInfo(int projCompType)
        {
            var goldenPts = getGoldenMarkPoints();
            var rects = projCompType == 0 ?
                        getGBProjInfo() :       // 綠
                        getMProjInfo() ;        // 紅
            var info = new CoreCompInfo(projCompType, goldenPts, rects);
            return info;
        }


        public static bool CheckMarkPoints(Bitmap bmpSrc)
        {
            bool ok = true;
            try
            {
                CoretronicsAPI.setMarkInitial();

                // JUST for safety-verification
                // using (Bitmap bmp = new Bitmap(bmpSrc))
                var bmp = bmpSrc;
                {
                    var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    var bmpd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    CoretronicsAPI.setMarkImg(rect.Width, rect.Height, 3, bmpd.Scan0);
                    bmp.UnlockBits(bmpd);
                }

                ok = CoretronicsAPI.MarkPtProcess();
                
            }
            catch (Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "中光電 DLL 異常!");
                CommonLogClass.Instance.LogMessage("中光電 DLL 調用異常", Color.DarkRed);
            }
            return ok;
        }


        /// <summary>
        /// 中心座標、矩形角度(先假設為方正矩形暫為預留先不使用)、與寬高，
        /// 後續於UI上顯示中心位置、矩形的四個位置(中心座標加減長寬各半距離)
        /// </summary>
        static CoreCompRect getCenterInfo(int projCompType)
        {
            float[] centerInfo = new float[5];
            //中心座標、矩形角度(先假設為方正矩形暫為預留先不使用)、與寬高，
            //後續於UI上顯示中心位置、矩形的四個位置(中心座標加減長寬各半距離)
            //回傳內容為 Center.x, Center.y, Angle, Width, Height
            CoretronicsAPI.getCenterInfo(projCompType, centerInfo);
            var rect = new CoreCompRect(centerInfo);
            return rect;
        }

        /// <summary>
        /// 綠斑 GBProjInfo 座標、矩形角度(先假設為方正矩形暫為預留先不使用)、與寬高，
        /// 後續於UI上顯示中心位置、矩形的四個位置(中心座標加減長寬各半距離)
        /// </summary>
        static CoreCompRect[] getGBProjInfo()
        {
            float[] defectInfo = new float[5];
            //中心座標、矩形角度(先假設為方正矩形暫為預留先不使用)、與寬高，
            //後續於UI上顯示中心位置、矩形的四個位置(中心座標加減長寬各半距離)
            //回傳內容為 Center.x, Center.y, Angle, Width, Height
            CoretronicsAPI.getGBProjInfo(defectInfo);
            var rect = new CoreCompRect(defectInfo);
            return new CoreCompRect[] { rect };
        }

        /// <summary>
        /// 紅斑 MProjInfo 座標、矩形角度(先假設為方正矩形暫為預留先不使用)、與寬高，
        /// 後續於UI上顯示中心位置、矩形的四個位置(中心座標加減長寬各半距離)
        /// </summary>
        static CoreCompRect[] getMProjInfo()
        {
            float[] defectInfo = new float[5 + 5 + 2];
            // Center1.x, Center1.y, Angle1, Width1, Height1,
            // Center2.x, Center2.y, Angle2, Width2, Height2,
            // Center3.x, Center3.y (Midle Center)
            CoretronicsAPI.getMProjInfo(defectInfo);
            var rect1 = new CoreCompRect(defectInfo);
            var rect2 = new CoreCompRect(defectInfo, 5);
            var rect3 = new CoreCompRect(defectInfo[10], defectInfo[11], 0, 0, 0);
            return new CoreCompRect[] { rect1, rect2, rect3 };
        }

        /// <summary>
        /// Golden Points
        /// </summary>
        public static Point[] getGoldenMarkPoints()
        {
            // 新版 dll 改為5點, 包含中心點
            int[] data = new int[10];
            CoretronicsAPI.getGoldMarkPt(data);
            int N = data.Length / 2;
            var pts = new Point[N];
            for (int i = 0, j = 0; i < N; i++, j += 2)
            {
                int x = data[j];
                int y = data[j + 1];
                pts[i] = new Point(x, y);
            }
            return pts;
        }

        /// <summary>
        /// Algo Points
        /// </summary>
        public static Point[] getAlgoMarkPoints()
        {
            // 新版 dll 改為5點, 包含中心點
            int[] data = new int[10];
            CoretronicsAPI.getAlgoMarkPt(data);
            int N = data.Length / 2;
            var pts = new Point[N];
            for (int i = 0, j = 0; i < N; i++, j += 2)
            {
                int x = data[j];
                int y = data[j + 1];
                pts[i] = new Point(x, y);
            }
            return pts;
        }

        /// <summary>
        /// set Mark Points
        /// </summary>
        public static void setMarkPoints(Point[] pts)
        {
            GdxGlobal.LOG.Log("定位點檢, 手動設定", toString(pts));
            int N = pts.Length;
            int[] data = new int[N * 2];
            for (int i = 0, j = 0; i < N; i++, j += 2)
            {
                data[j] = pts[i].X;
                data[j + 1] = pts[i].Y;
            }
            CoretronicsAPI.setMarkPt(data);
        }

        /// <summary>
        /// set Center Points
        /// </summary>
        public static void setGoldenCenterPt(string gb_r, Point[] pts)
        {
            GdxGlobal.LOG.Log($"{gb_r}鏡片, 定位點檢, 手動設定", toString(pts));
            int N = pts.Length;
            int x = 0;
            int y = 0;
            for (int i = 0; i < N; i++)
            {
                x += pts[i].X;
                y += pts[i].Y;
            }
            x /= N;
            y /= N;
            int[] data = new int[] { x, y };
            int ptype = gb_r == "GB" ? 0 : 1;
            CoretronicsAPI.setGoldCenterPt(ptype, data);
        }

        public static Point getGoldenCenterPt(int ptype)
        {
            int[] data = new int[2];
            CoretronicsAPI.getGoldCenterPt(ptype, data);
            return new Point(data[0], data[1]);
        }

        public static string toString(Point[] pts)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0, N = pts.Length; i < N; i++)
            {
                sb.Append(i == 0 ? " (" : ", (");
                sb.Append(pts[i].X);
                sb.Append(",");
                sb.Append(pts[i].Y);
                sb.Append(")");
            }
            return sb.ToString();
        }
        #endregion


        #region PUBLIC_雷射點位標誌相關接口

        public static void InitLaserCoordsTransform()
        {
            var trf = GdxGlobal.Facade.LaserCoordsTransform;
        }
        public static void CollectLaserPt(int mirrorIdx, int pointIdx, double laserDist, string ga_motorPt)
        {
            var trf = GdxGlobal.Facade.LaserCoordsTransform;
            
            // (X,Y,Z,L)
            var motorPosL = QVector.Parse(ga_motorPt + "," + laserDist);
            
            if (pointIdx == 0)
                trf.ResetLaserPtsOnMirror(mirrorIdx);

            trf.AddLaserPtOnMirror(mirrorIdx, motorPosL);


            GdxGlobal.LOG.Trace("MirrorPicker.CollectLaserPos, mirror={0}, @{1}, laser={2:0.0000}, motorPos={3}",
                                    mirrorIdx, pointIdx, laserDist, motorPosL );
        }
        public static void BuildLaserCoordsTransform(int mirrorIdx, List<string> ga_strs)
        {
            var trf = GdxGlobal.Facade.LaserCoordsTransform;
            trf.BuildMirrorPlaneTransform(mirrorIdx);
            System.Diagnostics.Debug.WriteLine(GdxGlobal.INI.GaugeBlockPlanePoints);
        }
        /// <summary>
        /// 取的 QC 雷射複檢的 馬達位置 X Y Z 座標
        /// 單位 mm
        /// </summary>
        public static double GetQCMotorPos(int mirrorIndex, out double X, out double Y, out double Z)
        {
            var trf = GdxGlobal.Facade.LaserCoordsTransform;
            var motorPos = trf.GetQCMotorPos(mirrorIndex);
            X = motorPos.X;
            Y = motorPos.Y;
            Z = motorPos.Z;
            var runSurfaceCenter = trf.GetLastRunSurfaceCenter(mirrorIndex);
            return runSurfaceCenter != null ? runSurfaceCenter[3] : 0;
        }
        public static string SetQcLaserMeasurement(int mirrorIdx, double value)
        {
            var trf = GdxGlobal.Facade.LaserCoordsTransform;
            string err = trf.SetQCLaserMeasurement(mirrorIdx, value);
            return err;
        }

        #endregion


        /// <summary>
        /// 離線模擬用
        /// </summary>
        public static void Trace(string tag, object process, params object[] args)
        {
            bool isSim = (GdxGlobal.Facade != null && GdxGlobal.Facade.IsSimPLC());

            if (!isSim)
                return;     //> 略過 Trace and Simulation

            try
            {
                string[] strs = tag.Split('.');
                switch (strs[0])
                {
                    case "MirrorCalibration":
                    case "MirrorDispenser":
                    case "MirrorPicker":
                        trace_MirrorOperations(tag, strs, process, args);
                        break;
                    case "ModulePosition":
                        trace_Module_Motion(tag, strs, args);
                        break;
                    case "Motor":
                        trace_Motors_Motion(tag, strs, args);
                        break;
                }
            }
            catch (Exception ex)
            {
                GdxGlobal.LOG.Error(ex, "Trace Error");
            }
        }


        #region PRIVATE_離線模擬與測試
        class XWait
        {
            public string Name;
            public int Count = 0;
        }
        static XWait _last_process_wait = new XWait();
        static XWait _last_module_wait = new XWait();
        static XWait _last_motors_wait = new XWait();
        static int m_simCount = 0;
        static void trace_MirrorOperations(string tag, string[] strs, object ps, params object[] args)
        {
            if (check_wait_count(_last_process_wait, tag, strs) > 5)
                return;

            var LOG = GdxGlobal.LOG;
            if (strs.Length > 1)
            {
                string ps_state = (ps != null && ps is ProcessClass) ?
                    ((ProcessClass)ps).ID.ToString() : "";

                switch (strs[1])
                {
                    case "Start":
                        {
                            if(args.Length>1)
                                LOG.Trace("{0}, ps={1}, mirror={2}, GroupIndex={3}", tag, ps_state, args[0], args[1]);
                            else
                                LOG.Trace("{0}, ps={1}, mirror={2}", tag, ps_state, args[0]);
                            break;
                        }

                    case "LaserRun":
                        {
                            LOG.Trace("{0}, ps={1}, {2}", tag, ps_state, pack(args));
                            int mirrorIdx = (int)args[1];
                            int pointIdx = (int)args[3];
                            // QVector pos = QVector.Parse((string)args[5]);
                            sim_laser(mirrorIdx, pointIdx);
                        }
                        break;

                    case "LaserNext":
                        {
                            LOG.Trace("{0}, ps={1}, {2}", tag, ps_state, pack(args));
                            sim_laser(-1, -1);
                        }
                        break;

                    case "Compensate":
                        {
                            var bmp = (Bitmap)args[0];
                            var modulePos = QVector.Parse((string)args[1]);
                            var centerPt = (PointF)args[2];
                            LOG.Trace("{0}, ps={1}, centerPt={2}, modulePos={3}", tag, ps_state, centerPt, modulePos);
                        }
                        break;

                    case "IO":
                        {
                            string ioname = (string)args[0];
                            bool targetValue = args.Length > 1 ? (bool)args[1] : true;
                            LOG.Trace("{0}, ps={1}, {2}, {3}", tag, ps_state, ioname, targetValue);

                            if (GdxGlobal.Facade.IsSimPLC())
                            {
                                GdxGlobal.Facade.IO.setIO(ioname, targetValue);
                            }
                            break;
                        }

                    case "Actuator":
                        {
                            IxActuator driver = (IxActuator)args[0];
                            bool targetValue = args.Length > 1 ? (bool)args[1] : true;
                            LOG.Trace("{0}, ps={1}, {2}, {3}", tag, ps_state, driver.Name, targetValue);

                            if (GdxGlobal.Facade.IsSimPLC())
                            {
                                driver.Set(targetValue);
                            }
                            break;
                        }

                    default:
                        {                            
                            LOG.Trace("{0}, ps={1}, {2}", tag, ps_state, pack(args));
                            break;
                        }
                }
            }
        }
        static void trace_Module_Motion(string tag, string[] strs, params object[] args)
        {
            if (check_wait_count(_last_module_wait, tag, strs) > 5)
                return;

            var LOG = GdxGlobal.LOG;
            if (strs.Length > 1)
            {
                switch (strs[1])
                {
                    case "Set":
                        LOG.Trace("{0}, {1}, {2}, {3}, ({4})", strs[0], strs[1], args[0], args[1], args[2]);
                        if (GdxGlobal.Facade == null)
                            GdxGlobal.Init();
                        if (GdxGlobal.Facade.IsSimMotor())
                            sim_motors((ModuleName)args[0], (int)args[1], (string)args[2]);
                        break;
                    case "GO":
                    case "IsComplete":
                    case "Ready":
                    case "IsReadyComplete":
                        LOG.Trace("{0}, {1}, {2}, {3}", strs[0], strs[1], args[0], args[1]);
                        break;
                }
            }
        }
        static void trace_Motors_Motion(string tag, string[] strs, params object[] args)
        {
            if (check_wait_count(_last_motors_wait, tag, strs) > 1)
                return;

            var LOG = GdxGlobal.LOG;
            if (strs.Length > 1)
            {
                if (is_wait(strs))
                {
                    string ioname = (string)args[0];
                    bool targetValue = args.Length > 1 ? (bool)args[1] : true;
                    LOG.Trace("{0}, {1}, {2}", tag, ioname, targetValue);

                    if (GdxGlobal.Facade.IsSimPLC())
                    {
                        GdxGlobal.Facade.IO.setIO(ioname, targetValue);
                    }
                }
            }
        }
        static void sim_laser(int mirrorIdx, int pointIdx, string ga_str = null)
        {
            var ga_laser = GdxGlobal.Facade.GetLaser();
            if (ga_laser is Sim.GdxLaser)
            {
                var sim_laser = (Sim.GdxLaser)ga_laser;
                if(mirrorIdx==1)
                {
                    double[] lds = new double[]
                    {
                        0.3115, 0.2916, 0.2571
                    };
                    if (pointIdx < lds.Length)
                    {
                        double ld = lds[pointIdx];
                        sim_laser.set_simulation_dist(ld);
                        return;
                    }
                }
                if (mirrorIdx >= 0 && pointIdx >= 0)
                {
                    var mirrorInfo = mirrorIdx == 0 ?
                        GdxGlobal.INI.Mirror1 :
                        GdxGlobal.INI.Mirror2;
                    var planePt = mirrorInfo.PlanePosList[pointIdx];
                    double ld = planePt.Z;
                    sim_laser.set_simulation_dist(ld);
                }
                else
                {
                    sim_laser.set_simulation_dist(-9999);
                }
            }
        }
        static void sim_motors(ModuleName module, int index, string ga_pos_str)
        {
            int axisID;
            switch (module)
            {
                case ModuleName.MODULE_PICK:
                    axisID = 0; break;
                case ModuleName.MODULE_DISPENSING:
                    axisID = 3; break;
                case ModuleName.MODULE_ADJUST:
                    axisID = 6; break;
                default:
                    return;
            }

            var pos = QVector.Parse(ga_pos_str);
            for (int i = 0; i < pos.Dimensions; i++, axisID++)
            {
                GdxGlobal.IO.sim_axis_to_pos(axisID, pos[i]);
            }
        }
        static void sim_projection(ref bool ok, int[] motorParams)
        {
#if(OPT_SIM)
            ++m_simCount;

            if (m_simCount % 5 == 0)
            {
                motorParams[0] = 0;
                motorParams[1] = -6;
            }

            if (m_simCount % 13 == 0)
                ok = false;
            else
                ok = true;
#endif
        }
        static int check_wait_count(XWait last_wait, string tag, string[] strs)
        {
            if (last_wait.Name == tag)
                return ++last_wait.Count % 100;
            if (is_wait(strs))
                last_wait.Name = tag;
            else
                last_wait.Name = null;
            return 0;
        }
        static bool is_wait(string[] strs)
        {
            return strs[strs.Length - 1] == "Wait";
        }
        static string pack(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            string tag;
            for(int i=0; i< args.Length;i++)
            {
                object arg = args[i];
                tag = arg.ToString();
                sb.Append(", ");
                sb.Append(tag);
                if (tag == "Pts" && i < args.Length-1)
                {
                    try
                    {
                        arg = args[i + 1];
                        if (arg is IList)
                        {
                            sb.Append(packPts((IList)arg));
                            i++;
                        }
                        else if (arg is Array)
                        {
                            sb.Append(packPts((Array)arg));
                            i++;
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }
            return sb.ToString();
        }
        static string packPts(IEnumerable pts)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(", [ ");
            foreach (object pt in pts)
            {
                var pos = QVector.Parse(pt.ToString());
                if (pos.Dimensions == 1)
                {
                    sb.Append(",");
                    sb.Append(pos[0]);
                }
                else
                {
                    sb.Append(",");
                    sb.Append(pos.ToString());
                }
            }
            sb.Append(" ]");
            return sb.Replace("[ ,", "[ ").ToString();
        }
        #endregion
    }
}
