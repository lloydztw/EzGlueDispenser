using Eazy_Project_Interface;
using JetEazy.ControlSpace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace JetEazy.CCDSpace
{
    public class CameraPara
    {
        public int Index { get; set; } = 0;
        public string SerialNumber { get; set; } = string.Empty;

        public bool IsDebug { get; set; } = false;
        public int Rotate { get; set; } = 0;
        public string CfgPath { get; set; } = "WORK";
        public string ToCameraString()
        {
            string str = "";

            str += Index + "@";
            str += SerialNumber + "@";
            str += (IsDebug ? "1" : "0") + "@";
            str += Rotate + "@";
            str += CfgPath + "@";

            return str;
        }
        public void FromCameraString(string eStr)
        {
            string[] strs = eStr.Split('@').ToArray();
            Index = int.Parse(strs[0]);
            SerialNumber = strs[1];
            IsDebug = strs[2] == "1";
            Rotate = int.Parse(strs[3]);
            CfgPath = strs[4];
        }
    }

    public class CAMERAClass : ICam
    {
        
        CAM_HIKVISION _cam = null;
        Bitmap m_BmpError = new Bitmap(1, 1);
        //Bitmap m_BmpDebug = new Bitmap(1, 1);
        List<string> list_debugFiles = new List<string>();
        int dbgIndex = 0;

        CameraPara _camCfg = new CameraPara();
        //public void Dispose()
        //{
        //    if (m_BmpDebug != null)
        //        m_BmpDebug.Dispose();
        //    m_BmpDebug = null;
        //    if (m_BmpError != null)
        //        m_BmpError.Dispose();
        //    m_BmpError = null;
        //}
        public bool IsSim()
        {
            return _camCfg.IsDebug;
        }
        public void Initial(string inipara)
        {
            _camCfg.FromCameraString(inipara);
            string err_bmpPath = _camCfg.CfgPath + "\\Error.bmp";
            if (System.IO.File.Exists(err_bmpPath))
            {
                Bitmap bmp = new Bitmap(err_bmpPath);
                m_BmpError.Dispose();
                m_BmpError = new Bitmap(bmp);
                bmp.Dispose();
            }
            else
            {
                m_BmpError.Dispose();
                m_BmpError = new Bitmap(10000, 10000);
                Graphics g = Graphics.FromImage(m_BmpError);
                g.Clear(Color.Red);
                g.Dispose();
            }
            if (_camCfg.IsDebug)
            {
                list_debugFiles.Clear();
                dbgIndex = 0;
                string dbg_bmppath = _camCfg.CfgPath + "\\cam" + _camCfg.Index.ToString();
                if (System.IO.Directory.Exists(dbg_bmppath))
                {
                    string[] myFiles = System.IO.Directory.GetFiles(dbg_bmppath, "*.bmp");
                    foreach (string str in myFiles)
                        list_debugFiles.Add(str);
                    myFiles = System.IO.Directory.GetFiles(dbg_bmppath, "*.png");
                    foreach (string str in myFiles)
                        list_debugFiles.Add(str);
                    myFiles = System.IO.Directory.GetFiles(dbg_bmppath, "*.jpg");
                    foreach (string str in myFiles)
                        list_debugFiles.Add(str);
                    myFiles = System.IO.Directory.GetFiles(dbg_bmppath, "*.jpeg");
                    foreach (string str in myFiles)
                        list_debugFiles.Add(str);

                    //if(list_debugFiles.Count > 0)
                    //{
                    //    Bitmap bmp = new Bitmap(list_debugFiles[dbgIndex]);
                    //    m_BmpDebug.Dispose();
                    //    m_BmpDebug = new Bitmap(bmp);
                    //    bmp.Dispose();
                    //}
                }

                return;
            }

            _cam = new CAM_HIKVISION(new System.Windows.Forms.PictureBox(), _camCfg.Index);
            _cam.Init(_camCfg.SerialNumber);

        }
        public void Close()
        {
            //if (m_BmpDebug != null)
            //    m_BmpDebug.Dispose();
            //m_BmpDebug = null;
            if (m_BmpError != null)
                m_BmpError.Dispose();
            m_BmpError = null;

            if (_camCfg.IsDebug)
                return;
            if (_cam == null)
                return;

            _cam.Dispose();
        }
        public void SetExposure(int val)
        {
            if (_camCfg.IsDebug)
                return;

            if (_cam == null)
                return;

            MvCamCtrl.NET.MyCamera.MVCC_FLOATVALUE stParam = new MvCamCtrl.NET.MyCamera.MVCC_FLOATVALUE();
            _cam.GetFloatValue_NET(ref stParam);
            _cam.SetExposure((float)val / 1000f * stParam.fMax);
        }
        public void StartCapture()
        {
            if (_camCfg.IsDebug)
                return;
            if (_cam == null)
                return;

            _cam.TriggerMode(0);
        }
        public void StopCapture()
        {
            if (_camCfg.IsDebug)
                return;
            if (_cam == null)
                return;

            _cam.TriggerMode(1);
        }
        public void Snap()
        {
            if (_camCfg.IsDebug)
                return;
            if (_cam == null)
                return;

            _cam.TriggerSoftwareX();
        }
        
        /// <summary>
        /// The caller must maintain the life cycle of the Bitmap returned by this function!!!
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        public Bitmap GetSnap(int msec = 1000)
        {
            if (_camCfg.IsDebug)
            {
                Bitmap ret = null;  //  this ret must be new bitmap or clone() !!

                //is it possible that m_BmpError is null here?
                {
                    if (list_debugFiles.Count <= 0)
                    {
                        ret = (Bitmap)m_BmpError.Clone();
                    }
                    else
                    {
                        if (dbgIndex >= list_debugFiles.Count)
                            dbgIndex = 0;

                        Bitmap bmp = new Bitmap(list_debugFiles[dbgIndex]);
                        ret = new Bitmap(bmp);
                        bmp.Dispose();

                        dbgIndex++;
                    }
                }
                return ret;
            }
            if (_cam == null)
                return (Bitmap)m_BmpError.Clone();  // ok

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                Bitmap newBitmapFrame = _cam.CaptureBmp(_camCfg.Rotate);

                if (newBitmapFrame != null)
                {
                    //var ret= (Bitmap)bmptemp.Clone();
                    //bmptemp.Dispose();
                    //return ret;
                    return newBitmapFrame;
                }

                if (watch.ElapsedMilliseconds > msec)
                    break;
            }
            return (Bitmap)m_BmpError.Clone();
        }
    }
}
