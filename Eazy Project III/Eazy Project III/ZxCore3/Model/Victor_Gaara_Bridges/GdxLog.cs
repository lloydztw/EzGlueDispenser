using JetEazy.BasicSpace;
using System;
using System.Drawing;
using System.Text;

namespace JetEazy.GdxCore3.Model
{
    /// <summary>
    /// Adapter to NLog or CommonLogClass <br/>
    /// NLog 與 CommonLogClass 轉接器 <br/>
    /// @LETIAN: 202206
    /// </summary>
    class GdxLog
    {
        /// <summary>
        /// 是否另外產生 NLogs 
        /// </summary>
        static bool INCLUDE_NLOG = true;

        #region PRIVATE_DATA_MEMBERS
        private static NLog.Logger s_nlog = null;
        static NLog.Logger _NLog
        {
            get
            {
                if (s_nlog == null)
                {
                    s_nlog = NLog.LogManager.GetCurrentClassLogger();
                }
                return s_nlog;
            }
        }
        static CommonLogClass _JLog
        {
            get
            {
                return CommonLogClass.Instance;
            }
        }
        #endregion

        #region SINGLETON
        static GdxLog _singleton = null;
        protected GdxLog()
        {
        }
        #endregion

        public static GdxLog Singleton()
        {            
            if (_singleton == null)
            {
                _singleton = new GdxLog();
            }
            return _singleton;
        }

        public void Info(string msg, params object[] args)
        {
            if (INCLUDE_NLOG)
                _NLog.Info(msg, args);
        }
        public void Trace(string msg, params object[] args)
        {
            if (INCLUDE_NLOG)
                _NLog.Trace(msg, args);
        }
        public void Debug(string msg, params object[] args)
        {
            if (INCLUDE_NLOG)
                _NLog.Debug(msg, args);
        }
        public void Warn(string msg, params object[] args)
        {
            if (INCLUDE_NLOG)
                _NLog.Warn(msg, args);
        }
        public void Warn(Exception ex, string msg)
        {
            if (INCLUDE_NLOG)
                _NLog.Warn(ex, msg);
        }
        public void Error(Exception ex, string msg)
        {
            if (INCLUDE_NLOG)
                _NLog.Error(ex, msg);
        }

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 指定紅色 會調用 NLog.Warning 其他則調用 NLog.Debug <br/>
        /// (將來有待抽離 GUI 之部分) <br/>
        /// @LETIAN: 202206
        /// </summary>
        public void Log(string msg, params object[] args)
        {
            Color color = Color.Black;

            int N = args.Length;
            if (N > 0 && args[N - 1] is Color)
            {
                color = (Color)args[N - 1];
                N -= 1;
            }

            var sb = new StringBuilder();

            //sb.Append(Name);
            //sb.Append(", ");

            sb.Append(msg);

            for (int i = 0; i < N; i++)
            {
                sb.Append(", ");
                sb.Append(args[i]);
            }

            msg = sb.ToString();
            _JLog.LogMessage(msg, color);

            if (INCLUDE_NLOG)
            {
                if (color == Color.Red)
                    _NLog.Warn(msg);
                else
                    _NLog.Debug(msg);
            }
        }

        /// <summary>
        /// Generic LOG (dual) <br/>
        /// 會額外調用 NLog.Warning <br/>
        /// @LETIAN: 202206
        /// </summary>
        public void Log(Exception ex, string msg)
        {
            _JLog.LogMessage(msg, Color.Red);

            if (INCLUDE_NLOG)
                _NLog.Warn(ex, msg);
        }
    }
}
