using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Eazy_Project_III.ProcessSpace
{
    public class ZxReportGenerator
    {
        #region CONFIGURATIONS_組態
#if(OPT_SIM)
        const int EXPIRE_HOURS = 1;
        const int VALID_MINUTES = 10;
#else
        const int EXPIRE_HOURS = 24;
        const int VALID_MINUTES = 10;
#endif
        #endregion


        public void GenerateReports(string barcode, string ng, int mirrorIdx)
        {
            var a = new Action(() =>
            {
                var tm = DateTime.Now;
                GdxCore.getProjCompType(mirrorIdx, out Color mcolor);
                string mirrorTag = mcolor == Color.DarkGreen ? "G" : "R";
                System.Threading.Thread.Sleep(250);
                _generateTxtReports(barcode, ng, mirrorTag, tm);
                _pickImageFiles(barcode, ng, mirrorTag, tm);
            });
            a.BeginInvoke(null, null);
        }

        void _generateTxtReports(string barcode, string ng, string mirrorTag, DateTime tm)
        { 
            string timeTag = tm.ToString("yyyyMMdd_HHmmss");
            string passTag = ng == null ? "OK" : "NG";
            string stemName = $"{barcode}_{mirrorTag}_{passTag}_{timeTag}";
            string pathName = Path.Combine(Universal.OUTPUT_PATH, tm.ToString("yyyyMMdd"));
            string fileName = Path.Combine(pathName, stemName + ".txt");
            try
            {
                if (!Directory.Exists(pathName))
                    Directory.CreateDirectory(pathName);

                using (var stm = new System.IO.StreamWriter(fileName, false))
                {
                    stm.WriteLine(stemName);
                    if (ng != null)
                        stm.WriteLine(ng);
                    stm.Flush();
                    stm.Close();
                }
                GdxGlobal.LOG.Log("保存文檔", (stemName + ".txt"), Color.DarkMagenta);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        void _pickImageFiles(string barcode, string ng, string mirrorTag, DateTime tm)
        {
            string srcPathName = Path.Combine(Universal.LOG_IMG_PATH, "_tmp");
            string bckPathName = Path.Combine(Universal.LOG_IMG_PATH, "_backup");
            string dstPathName = Path.Combine(Universal.LOG_IMG_PATH, tm.ToString("yyyyMMdd"));

            if (!Directory.Exists(bckPathName))
                Directory.CreateDirectory(bckPathName);

            if (!Directory.Exists(dstPathName))
                Directory.CreateDirectory(dstPathName);

            //(2.1) Sort by creation-time descending 
            var dirInfo = new DirectoryInfo(srcPathName);
            var allFileInfos = dirInfo.GetFiles();
            Array.Sort(allFileInfos, delegate (FileInfo f1, FileInfo f2)
            {
                return f2.CreationTime.CompareTo(f1.CreationTime);
            });

            //(2.2) 挑出小於 10 min 內的檔案
            var fileInfos = new List<FileInfo>();
            foreach (var fi in allFileInfos)
            {
                var ts = tm - fi.CreationTime;
                if (ts.TotalMinutes < VALID_MINUTES)
                    fileInfos.Add(fi);
            }

            //(2.3) 挑出 tags
            string[] tags = new string[]
            {
                $"{barcode}_{mirrorTag}_center",
                $"{barcode}_{mirrorTag}_projection",
            };
            foreach (var tag in tags)
            {
                foreach (var fi in fileInfos)
                {
                    if (fi.Name.Contains(tag))
                    {
                        var fileName = Path.Combine(dstPathName, fi.Name);
                        File.Copy(fi.FullName, fileName);
                        GdxGlobal.LOG.Log("保存圖檔", fi.Name, Color.DarkMagenta);
                        break;
                    }
                }
            }

            // (2.4) move to backup
            foreach (var fi in allFileInfos)
            {
                try
                {
                    var fileName = Path.Combine(bckPathName, fi.Name);
                    File.Move(fi.FullName, fileName);
                }
                catch
                {
                    // do nothing
                }
            }

            // (2.5) clean files in backup
            dirInfo = new DirectoryInfo(bckPathName);
            allFileInfos = dirInfo.GetFiles();
            foreach (var fi in allFileInfos)
            {
                try
                {
                    var ts = tm - fi.CreationTime;
                    if (ts.TotalHours > EXPIRE_HOURS)
                        File.Delete(fi.FullName);
                }
                catch
                {
                    // do nothing
                }
            }
        }
    }
}
