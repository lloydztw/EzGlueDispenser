using Eazy_Project_III.ProcessSpace;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using Paso.Aoi.GUI;
using System;
using System.Drawing;
using System.Windows.Forms;
using ZxCore3.Gui;
using LiveImageProcess = Eazy_Project_III.ProcessSpace.RcpLiveImageProcess;


namespace Eazy_Project_III.MVC_Control
{
    /// <summary>
    /// 為 DispUI 擴增 即時取像顯示 & Mark Checking 的功能.
    /// </summary>
    class GdxRcpLiveDispCtrl : GdxLiveDispControl
    {
        #region PRIVATE_DATA
        CvFreeQuadrilateralBox _cvBox = new CvFreeQuadrilateralBox(Brushes.Lime, 3, 15);
        LiveImageProcess _rcpLiveProcess;
        #endregion

        public event EventHandler OnCompleted;

        public GdxRcpLiveDispCtrl(GdxDispUI dispUI, LiveImageProcess ps) 
            : base(dispUI, ps)
        {
            dispUI.ImageViewer.AddInteractor(_cvBox);
            _cvBox.Visible = false;
            _rcpLiveProcess = ps;
            connect_event_handlers();
        }

        public override void Dispose()
        {
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
        private void rcpLiveProcess_OnMarkPointInfo(object sender, CoreMarkPointEventArgs e)
        {
            try
            {
                if (m_owner == null || !m_owner.Visible)
                    return;

                if (m_owner.InvokeRequired)
                {
                    EventHandler<CoreMarkPointEventArgs> func = rcpLiveProcess_OnMarkPointInfo;
                    m_owner.BeginInvoke(func, e);
                }
                else
                {
                    if (e != null)
                        gdxDispUI1.UpdateLocatedMarks(e.GoldenPts, e.AlgoPts);
                    else
                        gdxDispUI1.UpdateLocatedMarks(null, null);
                }
            }
            catch(Exception ex)
            {

            }
        }
        private void rcpLiveProcess_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (!IsHandleCreated || !Visible)
                return;

            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> func = rcpLiveProcess_OnCompleted;
                this.BeginInvoke(func, e);
            }
            else
            {
                var msg = e.Message;
                MessageBox.Show("已完成: 結果 = " + msg);
            }
        }

        #endregion

        #region PRIVATE_FUNCTIONS
        void connect_event_handlers()
        {
            _rcpLiveProcess.OnMarkPointInfo += rcpLiveProcess_OnMarkPointInfo;
            _rcpLiveProcess.OnCompleted += rcpLiveProcess_OnCompleted;
        }
        void disconnect_event_handlers()
        {
            _LOG("卸載 event handlers");
            _rcpLiveProcess.OnMarkPointInfo -= rcpLiveProcess_OnMarkPointInfo;
            _rcpLiveProcess.OnCompleted -= rcpLiveProcess_OnCompleted;
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
