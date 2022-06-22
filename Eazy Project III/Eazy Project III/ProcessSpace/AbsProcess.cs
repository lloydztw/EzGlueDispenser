using JetEazy.BasicSpace;
using System;
using System.Threading;

namespace JetEazy.ProcessSpace
{
    /// <summary>
    /// IxProcess <br/>
    /// 最通用廣義的 Interface <br/>
    /// 為避免不熟悉者, 可以使用 AbsProcess 當基礎 <br/>
    /// 將來慢慢將共通的 Methods 收集至此處 <br/>
    /// @LETIAN: 20220619 First Creation
    /// </summary>
    public interface IxProcess
    {
        //event EventHandler OnStateChanged;
        //event EventHandler<ProcessEventArgs> OnError;
        event EventHandler<ProcessEventArgs> OnMessage;
        event EventHandler<ProcessEventArgs> OnCompleted;
        void Tick();
    }


    /// <summary>
    /// AbsProcess <br/>
    /// ---------------------------------------------------------------- <br/>
    /// (1) 跨 project 通用之 abstrace class <br/>
    /// (2) 暫時 Reuse ProcessClass (for backward compatibility) <br/>
    /// (3) ProcessClass 以後有待 優化. <br/>
    /// ---------------------------------------------------------------- <br/>
    /// @LETIAN: 20220619 First Creation
    /// </summary>
    public abstract class AbsProcess : ProcessClass, IxProcess
    {
        #region PROTECTED_DATA_MEMBERS
        protected int _defaultDuration = 100;
        #endregion

        //public event EventHandler OnStateChanged;
        //public event EventHandler<ProcessEventArgs> OnError;
        public event EventHandler<ProcessEventArgs> OnMessage;
        public event EventHandler<ProcessEventArgs> OnCompleted;

        public virtual string Name
        {
            get { return GetType().Name; }
        }
        public virtual void Start(params object[] args)
        {
            if (args.Length > 0)
                base.Start((string)args[0]);
            else
                base.Start();
        }
        public new virtual void Stop()
        {
            base.Stop();
            //FireCompleted();
        }
        public abstract void Tick();

        protected void SetNextState(int id, int nextDuration = -1)
        {
            bool isChanged = (this.ID != id);
            this.ID = id;
            if (nextDuration >= 0)
                this.NextDuriation = nextDuration;
            //if (isChanged)
            //    OnStateChanged?.Invoke(this, null);
        }
        protected void FireMessage(string message)
        {
            var e = new ProcessEventArgs() { Message = message };
            OnMessage?.Invoke(this, e);
        }
        protected void FireMessage(ProcessEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }
        protected void FireCompleted(ProcessEventArgs e = null)
        {
            if (e == null)
            {
                e = new ProcessEventArgs()
                {
                    Message = string.Format("Completed!")
                };
            }
            OnCompleted?.Invoke(this, e);
        }
    }


    public class ProcessEventArgs : EventArgs
    {
        public ProcessEventArgs(string msg = null, object tag= null)
        {
            Message = msg;
            Tag = tag;
        }

        /// <summary>
        /// sender 要通知給 receiver 的訊息.
        /// </summary>
        public string Message = null;
        public object Tag = null;

        /// <summary>
        /// Cancel Flag: 
        /// 必要時可以由 receiver 端 
        /// 來設定來通知 sender 是否要中斷 process
        /// </summary>
        public bool Cancel = false;
        public ManualResetEvent GoControlByClient = null;
    }
}
