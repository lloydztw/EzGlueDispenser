﻿
using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.Drivers.Laser;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;



namespace JetEazy.GdxCore3
{
    public class GdxCore
    {
        public static void Init()
        {
            GdxGlobal.Init();
        }
        public static void Dispose()
        {
            GdxGlobal.Dispose();
        }
        public static IxLaser GetLaser(int id = 0)
        {
            return GdxGlobal.GetLaser(id);
        }

        public static string GetDllVersion()
        {
            StringBuilder version = new StringBuilder(20);
            CoretronicsAPI.getVersion(version);
            return version.ToString();
        }
        public static bool UpdateParams()
        {
            bool ok = CoretronicsAPI.updateParams();

            if (GdxGlobal.Facade.IsSimPLC())
                ok = false; // 故意報錯測試

            return ok;
        }
        public static bool CheckCompensate(object process, Bitmap bmp, string mirrorPutPosStr, PointF recipePoint, float tolerane)
        {
            bool go = true;

            int[] motorParam = new int[6];
            CoretronicsAPI.setCenterCompInitial();

            // unsafe
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                CoretronicsAPI.setCenterCompImg(rect.Width, rect.Height, 3, bmpd.Scan0);
            }
            
            CoretronicsAPI.CenterCompProcess();
            CoretronicsAPI.getCenterCompInfo(motorParam);
            
            GdxGlobal.LOG.Trace("CoretronicsAPI, CenterCompensate, ({0},{1},{2}), ({3},{4},{5})",
                    motorParam[0], motorParam[1], motorParam[2],
                    motorParam[3], motorParam[4], motorParam[5]
                );

            var mirrorPutPos = QVector.Parse(mirrorPutPosStr);
            GdxGlobal.LOG.Debug("$$$ MirrorPutPos = {0}", mirrorPutPos);
            GdxGlobal.LOG.Debug("$$$ recipePoint = {0}", recipePoint);

            return go;
        }       
        public static void Trace(string tag, object process, params object[] args)
        {
            string[] strs = tag.Split('.');
            switch (strs[0])
            {
                case "MirrorCalibration":
                    trace_MirrorCalibration(tag, strs, process, args);
                    break;
                case "ModulePosition":
                    trace_Module_Motion(tag, strs, args);
                    break;
                case "Motor":
                    trace_Motors_Motion(tag, strs, args);
                    break;
            }
        }


        #region PRIVATE_FUNCTION
        static string _last_process_wait = null;
        static string _last_module_wait = null;
        static string _last_motors_wait = null;
        static void trace_MirrorCalibration(string tag, string[] strs, object ps, params object[] args)
        {
            if (check_if_the_same_wait(ref _last_process_wait, tag, strs))
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
                            LOG.Trace("{0}, ps={1}, mirror={2}", tag, ps_state, args[0]);
                            break;
                        }
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
                    default:
                        {
                            LOG.Trace("{0}, ps={1}", tag, ps_state);
                            break;
                        }
                }
            }
        }
        static void trace_Module_Motion(string tag, string[] strs, params object[] args)
        {
            if (check_if_the_same_wait(ref _last_module_wait, tag, strs))
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
            if (check_if_the_same_wait(ref _last_motors_wait, tag, strs))
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
        static void sim_motors(ModuleName module, int index, string posStr)
        {
            int axisID;
            switch(module)
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

            var pos = QVector.Parse(posStr);
            for (int i = 0; i < pos.Dimensions; i++, axisID++)
            {
                var motor = GdxGlobal.Facade.GetMotor(axisID);
                motor.Go(pos[0], 0);
                //motor.SetActionSpeed(0);
                //motor.SetManualSpeed(0);
            }
        }
        static bool check_if_the_same_wait(ref string last_wait, string tag, string[] strs)
        {
            if (last_wait == tag)
                return true;
            if (is_wait(strs))
                last_wait = tag;
            else
                last_wait = null;
            return false;
        }
        static bool is_wait(string[] strs)
        {
            return strs[strs.Length - 1] == "Wait";
        }
        #endregion
    }
}
