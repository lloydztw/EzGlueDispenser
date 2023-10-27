using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using ZxCore3.Gui;
using LiveImageProcess = Eazy_Project_III.ProcessSpace.RcpLiveImageProcess;


namespace Eazy_Project_III.MVC_Control
{
    /// <summary>
    /// 為 DispUI 擴增 即時取像顯示的功能.
    /// </summary>
    class GdxLiveDispControl : IDisposable
    {
        #region PRIVATE_DATA
        protected Form m_owner;
        LiveImageProcess[] _liveProcesses;
        Action<Bitmap>[] _displayFuncs;
        bool _isDisposing = false;
        bool _isShareOneThreadMode;
        int _activeCamID;
        #endregion


        public event EventHandler<int> OnLiveStatusChanged;

        public GdxLiveDispControl(params GdxDispUI[] dispUIs)
        {
            int N = dispUIs.Length;
            m_owner = dispUIs[0].FindForm();
            _displayFuncs = new Action<Bitmap>[N];
            for (int i = 0; i < N; i++)
            {
                _displayFuncs[i] = dispUIs[i].UpdateLiveImage;
            }
            _liveProcesses = new LiveImageProcess[N];
            for (int i = 0; i < N; i++)
            {
                _liveProcesses[i] = LiveImageProcess.Singleton("Rcp");
            }
            _isShareOneThreadMode = (N == 1);
            _activeCamID = -1;
            connect_event_handlers();
        }
        public GdxLiveDispControl(GdxDispUI dispUI, LiveImageProcess ps)
        {
            m_owner = dispUI.FindForm();
            _displayFuncs = new Action<Bitmap>[] { dispUI.UpdateLiveImage };
            _liveProcesses = new LiveImageProcess[] { ps };
            _isShareOneThreadMode = true;
            _activeCamID = -1;
            connect_event_handlers();
        }

        public virtual void Dispose()
        {
            _isDisposing = true;

            bool isAnyLive = false;
            foreach (var ps in _liveProcesses)
                isAnyLive |= (ps != null && ps.IsOn);

            if (isAnyLive)
            {
                StopLiveImage(-1);
            }
            else
            {
                disconnect_event_handlers();
            }
        }
        public bool IsLive(int camIndex = 0)
        {
            var ps = get_process(camIndex);
            return ps != null && ps.IsOn;
        }
        public void StartLiveImage(int camIndex = 0)
        {
            if (_isDisposing)
                return;

            //////var ps = _liveProcesses[0];
            //////if (_isShareOneThreadMode)
            //////{
            //////    if (_activeCamID != camIndex && _activeCamID >= 0)
            //////        StopLiveImage(_activeCamID);
            //////}
            //////else
            //////{
            //////    if (camIndex >= _liveProcesses.Length)
            //////        return;
            //////    else
            //////        ps = _liveProcesses[camIndex];
            //////}
            //////if (!ps.IsOn)
            //////{
            //////    ps.Start(camIndex);
            //////    _activeCamID = camIndex;
            //////    System.Threading.Thread.Sleep(300);
            //////}

            if (_isShareOneThreadMode)
            {
                _activeCamID = camIndex;
            }

            var ps = get_process(camIndex);
            if (ps != null)   //(!ps.IsOn || true)
            {
                ps.Start(camIndex);
                _activeCamID = camIndex;
                //> System.Threading.Thread.Sleep(300);
            }
        }
        public void StopLiveImage(int camIndex = 0)
        {
            if (camIndex < 0)
            {
                // STOP ALL PROCESSES
                foreach (var ps in _liveProcesses)
                {
                    if (ps != null)
                        ps.Stop();
                }
                _activeCamID = -1;
            }
            else
            {
                ////var ps = _liveProcesses[0];
                ////if (_isShareOneThreadMode)
                ////{
                ////    if (camIndex != _activeCamID)
                ////        return;
                ////}
                ////else
                ////{
                ////    if (camIndex >= _liveProcesses.Length)
                ////        return;
                ////    else
                ////        ps = _liveProcesses[camIndex];
                ////}

                var ps = get_process(camIndex);
                if (ps != null && ps.IsOn)
                {
                    ps.Stop();
                    System.Threading.Thread.Sleep(300);
                }
            }
        }


        #region RESERVED        
        protected Bitmap Snapshot(int camIndex = 0)
        {
            // RESERVED!

            ////var ps = _liveProcesses[0];
            ////if (_isShareOneThreadMode)
            ////{
            ////    if (camIndex != _activeCamID)
            ////        return null;
            ////}
            ////else
            ////{
            ////    if (camIndex >= _liveProcesses.Length)
            ////        return null;
            ////    else
            ////        ps = _liveProcesses[camIndex];
            ////}

            ////if (ps != null)
            ////{
            ////    ps.Stop();
            ////    //for (int i = 0; i < 10; i++)
            ////    //{
            ////    //    if (is_thread_running())
            ////    //        System.Threading.Thread.Sleep(200);
            ////    //    else
            ////    //        break;
            ////    //}
            ////    return ps.Snapshot();
            ////}

            return null;
        }
        protected bool this[int camIndex]
        {
            get
            {
                return IsLive(camIndex);
            }
            set
            {
                bool goLive = value;
                if (goLive)
                    StartLiveImage(camIndex);
                else
                    StopLiveImage(camIndex);
            }
        }
        #endregion

        #region EVENT_HANDLERS
        void liveImagingProcess_OnLiveImage(object sender, ProcessEventArgs e)
        {
            try
            {
                if (m_owner == null || _isDisposing)
                    return;

                if (m_owner.InvokeRequired)
                {
                    var func = (EventHandler<ProcessEventArgs>)liveImagingProcess_OnLiveImage;
                    m_owner.Invoke(func, sender, e);
                }
                else
                {
                    int camID = Array.IndexOf(_liveProcesses, sender);
                    if (camID < 0)
                        return;

                    var dispFunc = _displayFuncs[camID];
                    dispFunc((Bitmap)e.Tag);
                }
            }
            catch (Exception ex)
            {
                // _owner 可能已經被 Dispose
                handle_exception(ex);
            }
        }
        void liveImagingProcess_OnMessage(object sender, ProcessEventArgs e)
        {
            try
            {
                if (m_owner == null)
                    return;

                if (m_owner.InvokeRequired)
                {
                    var func = (EventHandler<ProcessEventArgs>)liveImagingProcess_OnMessage;
                    m_owner.Invoke(func, sender, e);
                }
                else
                {
                    if (_isDisposing)
                    {
                        if (e.Message.Contains("Stopped") || e.Message.Contains("Abort"))
                        {
                            disconnect_event_handlers();
                        }
                        return;
                    }

                    int camID = Array.IndexOf(_liveProcesses, sender);
                    if (camID < 0)
                        return;

                    OnLiveStatusChanged?.Invoke(this, camID);
                }
            }
            catch (Exception ex)
            {
                // _owner 可能已經被 Dispose                
                handle_exception(ex);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        LiveImageProcess get_process(int camIndex)
        {
            if (_isShareOneThreadMode)
            {
                var ps = _liveProcesses[0];
                if (_activeCamID == camIndex && ps != null)
                    return ps;
                return null;
            }
            else
            {
                var ps = camIndex < _liveProcesses.Length ? _liveProcesses[camIndex] : null;
                return ps;
            }
        }
        void connect_event_handlers()
        {
            int N = _liveProcesses != null ? _liveProcesses.Length : 0;
            for (int i = 0; i < N; i++)
            {
                try
                {
                    _liveProcesses[i].OnLiveImage += liveImagingProcess_OnLiveImage;
                    _liveProcesses[i].OnMessage += liveImagingProcess_OnMessage;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
        void disconnect_event_handlers()
        {
            _LOG("卸載 event handlers");
            int N = _liveProcesses != null ? _liveProcesses.Length : 0;
            for (int i = 0; i < N; i++)
            {
                try
                {
                    _liveProcesses[i].OnLiveImage -= liveImagingProcess_OnLiveImage;
                    _liveProcesses[i].OnMessage -= liveImagingProcess_OnMessage;
                }
                catch
                {
                }
            }
        }
        void handle_exception(Exception ex)
        {
            m_owner = null;
        }
        void _LOG(string msg, params object[] args)
        {
            if (GdxGlobal.Facade.IsSimPLC())
            {
                GdxGlobal.LOG.Log(GetType().Name + msg, args);
            }
            else
            {
                GdxGlobal.LOG.Debug(GetType().Name + msg);
            }
        }
        #endregion
    }
}
