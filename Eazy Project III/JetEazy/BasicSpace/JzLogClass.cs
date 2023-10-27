#define OPT_ASYNC_LOG

using System;
using System.Drawing;
using System.Windows.Forms;



namespace JetEazy.BasicSpace
{
    public class CommonLogClass
    {
        #region PRIVATE_DATA
        bool m_IsRecordLog = true;
        object m_fileSync = new object();
        string m_log_path = Application.StartupPath + @"\log";
        string m_log_filename = "log_filename";
        #endregion


        #region SINGLETON
        private static readonly CommonLogClass m_log = new CommonLogClass();
        #endregion


        public static CommonLogClass Instance
        {
            get { return m_log; }
        }
        public string LogPath
        {
            set { m_log_path = value; }
        }
        public string LogFilename
        {
            set { m_log_filename = value; }
        }


        #region PRIVATE_DATA_FOR_RichTextBox
        public delegate void LogAppendDelegate(Color color, string text);
        RichTextBox richTextBoxRemote = null;
        #endregion
      

        public RichTextBox SetRichTextBox(RichTextBox ertb, bool eIsRecord = true)
        {
            //@ LETIAN: 2022/07/11 傳回舊的 richTextBox 以便 runtime swapping.
            var old = richTextBoxRemote;
            richTextBoxRemote = ertb;
            m_IsRecordLog = eIsRecord;
            return old;
        }

        /// <summary>  
        /// 追加显示文本  
        /// </summary>  
        /// <param name="color">文本颜色</param>  
        /// <param name="text">显示文本</param>  
        public void LogAppend(Color color, string text)
        {
            if (richTextBoxRemote.InvokeRequired)
            {
                Action<Color, string> func = LogAppend;
                richTextBoxRemote.Invoke(func, color, text);
            }
            else
            {
                if (richTextBoxRemote.TextLength >= 20000)
                    richTextBoxRemote.Text = "";

                //richTextBoxRemote.AppendText("/n");
                richTextBoxRemote.SelectionColor = color;
                richTextBoxRemote.AppendText(text + Environment.NewLine);

                richTextBoxRemote.SelectionStart = richTextBoxRemote.TextLength;
                richTextBoxRemote.ScrollToCaret();
            }
        }

        /// <summary>  
        /// 显示错误日志  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogError(string text)
        {
#if (OPT_ASYNC_LOG)
            LogMessage(text, Color.Red);
#else
            if (m_IsRecordLog)
                _log(text);
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Red, DateTime.Now.ToString("HH:mm:ss ") + text);
#endif
        }

        /// <summary>  
        /// 显示警告信息  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogWarning(string text)
        {
#if (OPT_ASYNC_LOG)
            LogMessage(text, Color.Violet);
#else
            if (m_IsRecordLog)
                _log(text);
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Violet, DateTime.Now.ToString("HH:mm:ss ") + text);
#endif
        }

        /// <summary>  
        /// 显示信息 (綠色)  
        /// </summary>  
        /// <param name="text"></param>
        public void LogMessageOK(string text)
        {
#if (OPT_ASYNC_LOG)
            LogMessage(text, Color.Lime);
#else
            if (m_IsRecordLog)
                _log(text);
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Lime, DateTime.Now.ToString("HH:mm:ss ") + text);
#endif
        }

        /// <summary>  
        /// 显示信息 (黑色)  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogMessage(string text)
        {
#if (OPT_ASYNC_LOG)
            LogMessage(text, Color.Black);
#else
            if (m_IsRecordLog)
                _log(text);
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, Color.Black, DateTime.Now.ToString("HH:mm:ss ") + text);
#endif
        }

        /// <summary>  
        /// 显示信息  
        /// </summary>  
        /// <param name="text"></param>  
        public void LogMessage(string text, Color eColor)
        {
#if(OPT_ASYNC_LOG)
            //@ LETIAN: 2022/10/26 加入 async 的手法.
            var asyncFunc = new Action<string, Color, DateTime>((txt, color, tm) =>
            {
                if (m_IsRecordLog)
                {
                    _log(txt);
                }
                txt = tm.ToString("HH:mm:ss.fff ") + LanguageExClass.Instance.ToTraditionalChinese(txt);
                LogAppend(color, txt);
            });
            asyncFunc.BeginInvoke(text, eColor, DateTime.Now, null, null);
#else
            if (m_IsRecordLog)
                _log(text);
            text = LanguageExClass.Instance.ToTraditionalChinese(text);
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            richTextBoxRemote.Invoke(la, eColor, DateTime.Now.ToString("HH:mm:ss.fff ") + text);
#endif
        }


        #region PRIVATE_DATA_FOR_ListBox
        ListBox m_LstLogMessage = null;
        int ilogCount = 10;
        #endregion


        #region 2_LISTBOX_型態的_LOG_(沒用到)
        public void SetListBox(ListBox eLstLogMessage)
        {
            m_LstLogMessage = eLstLogMessage;
        }
        public void LogListbox(string _message, string strMode = "1")
        {
            //return;//先不记录

            if (m_LstLogMessage != null)
            {
                switch (strMode)
                {
                    default:

                        //SkinListBoxItem _skitem = new SkinListBoxItem("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " + _message);
                        string _skitem = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " + _message;

                        m_LstLogMessage.Invoke(new Action(() =>
                        {
                            m_LstLogMessage.Items.Add(_skitem);
                            if (m_LstLogMessage.Items.Count > 5)
                                m_LstLogMessage.SelectedIndex = m_LstLogMessage.Items.Count - 1;
                            if (m_LstLogMessage.Items.Count > ilogCount)
                            {
                                int i = 0;
                                while (i < ilogCount / 2)
                                {
                                    m_LstLogMessage.Items.RemoveAt(0);
                                    i++;
                                }
                            }
                        }));

                        break;
                }
            }

            _log(_message);
        }
        #endregion


        #region 主要的_FILE_LOG_FUCTIONS
        public void Log(string _message, string strExt = ".log")
        {
            _log(_message, strExt);
        }
        private void _log(string strMsg, string strExt = ".log")
        {
            lock (m_fileSync)
            {
                string strPath = m_log_path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
                if (!System.IO.Directory.Exists(strPath))
                    System.IO.Directory.CreateDirectory(strPath);

                string strFileName = strPath + DateTime.Now.ToString("yyyyMMdd_HH") + strExt;
                System.IO.StreamWriter stm = null;

                try
                {
                    stm = new System.IO.StreamWriter(strFileName, true, System.Text.Encoding.UTF8);
                    stm.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    stm.Write(",");
                    stm.Write(Application.ProductVersion);
                    stm.Write(", ");
                    stm.WriteLine(strMsg);
                    stm.Flush();
                    stm.Close();
                    stm.Dispose();
                    stm = null;
                }
                catch
                {
                }

                if (stm != null)
                    stm.Dispose();
            }
        }
        #endregion


        #region 其他_FILE_LOG_FUNCTIONS_(沒用到)

        public void Log2(string _message, string strExt = ".log")
        {
            _log2(_message, strExt);
        }
        private void _log2(string strMsg, string strExt = ".log")
        {
            //lock (LAST_RECIPE_NAME)
            //{
            string strPath = m_log_path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!System.IO.Directory.Exists(strPath))
                System.IO.Directory.CreateDirectory(strPath);

            string strFileName = strPath + m_log_filename + strExt;
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(strFileName, true, System.Text.Encoding.UTF8);
                stm.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                stm.Write(",");
                stm.Write(Application.ProductVersion);
                stm.Write(", ");
                stm.WriteLine(strMsg);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch
            {
            }

            if (stm != null)
                stm.Dispose();
            //}
        }

        #endregion
    }
}
