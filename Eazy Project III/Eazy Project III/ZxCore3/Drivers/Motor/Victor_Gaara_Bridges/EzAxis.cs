using Eazy_Project_III.OPSpace;
using Eazy_Project_Interface;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.GdxCore3;
using JetEazy.GdxCore3.Model;
using JetEazy.ProcessSpace;
using JetEazy.QMath;
using System;
using System.CodeDom;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;



namespace JetEazy.GdxCore3
{   
    /// <summary>
    /// 對 IAxis 之擴充包裝 <br/>
    /// @LETIAN: 20221022 refactor
    /// </summary>
    internal class EzAxis : IAxis
    {
        /// <summary>
        /// 連續檢查到達定位的次數
        /// </summary>
        const int N_CHECKS = 3;

        #region PRIVATE_DATA
        IAxis _axis;
        double _lastGoPos;
        double _inPosPercision = 1e-5;
        int _inPosCheckCount = 0;
        DateTime? _lastTime = null;
        #endregion

        public EzAxis(IAxis axis, int id)
        {
            ID = id;
            _axis = axis;
            _lastGoPos = axis.GetPos();
        }

        #region 原有的_IAxis_Functions
        public bool IsError
        {
            get
            {
                return Bypass ? false : _axis.IsError;
            }
        }
        public bool IsOK
        {
            get
            {
                bool ok = Bypass ? true : _axis.IsOK;

#if(OPT_SIM)
                // DEBUG ++
                //if (ID == 1)
                //    ok = false;
                // DEBUG --
#endif

                if (ok)
                    _resetTiming("@IsOK");
                return ok;
            }
        }
        public void Backward()
        {
            if (!Bypass)
                _axis.Backward();
        }
        public void Forward()
        {
            if (!Bypass)
                _axis.Forward();
        }
        public double GetInitPosition()
        {
            return _axis.GetInitPosition();
        }
        public double GetPos()
        {
            return _axis.GetPos();
        }
        public string GetStatus()
        {
            return _axis.GetStatus();
        }
        public void Go(double frompos, double offset)
        {
            if (!Bypass)
            {                
                _axis.Go(frompos, offset);
                _lastGoPos = (frompos + offset);
            }
            else
            {
                _lastGoPos = GetPos();
            }
            _inPosCheckCount = 0;
            _resetTiming("@Go");
        }
        public void SetActionSpeed(int val)
        {
            if (!Bypass || true)
                _axis.SetActionSpeed(val);
            _resetTiming("@SetActionSpeed");
        }
        public void SetManualSpeed(int val)
        {
            if (!Bypass || true)
                _axis.SetManualSpeed(val);
            _resetTiming("@SetManualSpeed");
        }
        public void Stop()
        {
            _axis.Stop();
            _resetTiming("@Stop");
        }
        #endregion

        /// <summary>
        /// 取得 PlcMotionClass
        /// </summary>
        public static explicit operator PLCMotionClass(EzAxis axis)
        {
            return (PLCMotionClass)axis._axis;
        }

        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 略過馬達執行指令
        /// </summary>
        public bool Bypass
        {
            get; 
            set;
        } = false;

        /// <summary>
        /// 單位 ms
        /// </summary>
        public int Timeout 
        {
            get;
            set;
        }

        /// <summary>
        /// InPos 定位允許誤差
        /// </summary>
        public double InPosPercision
        {
            get { return _inPosPercision; }
            set { _inPosPercision = value; }
        }
        
        /// <summary>
        /// 取得目標值與目前位置之差異
        /// </summary>
        public double GetTargetDiff()
        {
            return _lastGoPos - GetPos();
        }

        /// <summary>
        /// 目前馬達位置是否已到達定位點 <br/>
        /// </summary>
        public bool IsInPos()
        {
            var diff = Math.Abs(GetTargetDiff());
            if (diff < _inPosPercision)
            {
                /// 連續 N_CHECKS 次, 才算
                if (++_inPosCheckCount >= N_CHECKS)
                {
                    _resetTiming("@IsInPos");
                    return true;
                }
            }
            else
            {
                _inPosCheckCount = 0;
            }
            return false;
        }

        public bool SmartCheckReady()
        {
            bool ok = IsOK;
            if (!ok)
            {
                // 如果 timeout 給最後機會
                // 使用 IsInPos 來判定 Ready
                if (IsTimeout())
                {
                    ok = IsInPos();
                    GdxGlobal.LOG.Log($"馬達 [{ID}軸]", "Timeout", $"嘗試使用 IsInPos = {ok}", Color.Purple);
                }
            }
            return ok;
        }

        public bool IsTimeout()
        {
            var tm = _lastTime;
            if (tm == null)
            {
                // 利用 obj 防止 thread race condition.
                DateTime? obj = DateTime.Now;
                _lastTime = obj;
                return false;
            }
            else
            {
                var ts = DateTime.Now - tm.Value;
                if (ts.TotalMilliseconds > Timeout)
                    return true;
                return false;
            }
        }

        #region PRIVATE_FUCNTIONS
        private void _resetTiming(object tag)
        {
            _lastTime = null;
        }
        #endregion
    }
}
