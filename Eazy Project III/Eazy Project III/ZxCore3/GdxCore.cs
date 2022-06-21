﻿
using Eazy_Project_III;
using Eazy_Project_Interface;
using JetEazy.BasicSpace;
using JetEazy.Drivers.Laser;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System;
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
        public static bool CheckCompensate(Bitmap bmp, string mirrorPutPosStr, PointF recipePoint, float tolerane)
        {
            bool go = true;

            int[] motorParam = new int[6];
            CoretronicsAPI.setCenterCompInitial();

            // unsafe
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                CoretronicsAPI.setCenterCompImg(rect.Width, rect.Height, 3, bmpd.Scan0);
                bmp.UnlockBits(bmpd);
            }

            CoretronicsAPI.CenterCompProcess();
            CoretronicsAPI.getCenterCompInfo(motorParam);

            var msg = string.Format("CoretronicsAPI, CenterCompensate, ({0},{1},{2}), ({3},{4},{5})",
                    motorParam[0], motorParam[1], motorParam[2],
                    motorParam[3], motorParam[4], motorParam[5]
                );

            CommonLogClass.Instance.LogMessage(msg, Color.Blue);
            // 暫時不 go
            go = false;

            var mirrorPutPos = QVector.Parse(mirrorPutPosStr);
            GdxGlobal.LOG.Debug("$$$ MirrorPutPos = {0}", mirrorPutPos);
            GdxGlobal.LOG.Debug("$$$ recipePoint = {0}", recipePoint);

            return go;
        }
        public static void CalcProjCompensation(Bitmap bmp, int[] motorParams)
        {
            CoretronicsAPI.setProjCompInitial();

            // unsafe
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                CoretronicsAPI.setProjCompImg(rect.Width, rect.Height, 3, bmpd.Scan0, 0);
                bmp.UnlockBits(bmpd);
            }

            CoretronicsAPI.ProjCompProcess();
            CoretronicsAPI.getProjCompInfo(0, motorParams);

            if (false)
            {
                GdxGlobal.LOG.Debug("CoretronicsAPI, ProjCompensate, ({0},{1},{2}), ({3},{4},{5})",
                        motorParams[0], motorParams[1], motorParams[2],
                        motorParams[3], motorParams[4], motorParams[5]
                    );
            }
        }

        public static void Trace(string tag, object process, params object[] args)
        {
            //> 略過 NLog Trace.
#if !OPT_LETIAN_DEBUG
            return;
#endif

            try
            {
                string[] strs = tag.Split('.');
                switch (strs[0])
                {
                    case "MirrorCalibration":
                    case "MirrorDispenser":
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


        #region PRIVATE_FUNCTION
        class XWait
        {
            public string Name;
            public int Count = 0;
        }
        static XWait _last_process_wait = new XWait();
        static XWait _last_module_wait = new XWait();
        static XWait _last_motors_wait = new XWait();
        static void trace_MirrorOperations(string tag, string[] strs, object ps, params object[] args)
        {
            if (check_wait_count(_last_process_wait, tag, strs) > 2)
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
                            LOG.Trace("{0}, ps={1}", tag, ps_state);
                            break;
                        }
                }
            }
        }
        static void trace_Module_Motion(string tag, string[] strs, params object[] args)
        {
            if (check_wait_count(_last_module_wait, tag, strs) > 1)
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
        static void sim_motors(ModuleName module, int index, string posStr)
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

            var pos = QVector.Parse(posStr);
            for (int i = 0; i < pos.Dimensions; i++, axisID++)
            {
                var motor = GdxGlobal.Facade.GetMotor(axisID);
                motor.Go(pos[0], 0);
                //motor.SetActionSpeed(0);
                //motor.SetManualSpeed(0);
            }
        }
        static int check_wait_count(XWait last_wait, string tag, string[] strs)
        {
            if (last_wait.Name == tag)
                return ++last_wait.Count;
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
        #endregion
    }
}
