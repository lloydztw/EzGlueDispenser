using Eazy_Project_Interface;
using Eazy_Project_Measure;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.Drivers.Laser;
using JetEazy.GdxCore3.Model;
using JetEazy.QMath;
using System;
using System.Threading;


namespace JetEazy.GdxCore3
{
    /// <summary>
    /// ------------------------------------------------ <br/>
    /// 對 Gaara IAxis 的包裝 <br/>
    /// 把單位轉換成物理世界數學運算的單位 <br/>
    /// X, Y, Z, U 為 mm <br/>
    /// θy, θz 為 degree <br/>
    /// ------------------------------------------------ <br/>
    /// 0, 1, 2, ( 3,  4,  5,) 6, 7, 8 <br/>
    /// X, Y, Z, (X2, Y2, Z2,) U, θy, θz <br/>
    /// ------------------------------------------------ <br/>
    /// @LETIAN: 2022/06/26 creation
    /// </summary>
    public class GdxMotor : IAxis
    {
        #region PRIVATE_DATA
        IAxis _axis;
        #endregion

        public GdxMotor(IAxis axis, int id)
        {
            _axis = axis;
            ID = id;
        }
        public int ID
        {
            get;
            private set;
        }

        #region IAxis_FUNCTIONS
        public bool IsError => _axis.IsError;
        public bool IsOK => _axis.IsOK;
        public void Backward()
        {
            _axis.Backward();
        }
        public void Forward()
        {
            _axis.Forward();
        }
        public double GetInitPosition()
        {
            return _axis.GetInitPosition();
        }
        public double GetPos()
        {
            return ToWorld(_axis.GetPos(), ID);
        }
        public string GetStatus()
        {
            return _axis.GetStatus();
        }
        public void Go(double frompos, double offset)
        {
            frompos = ToAxis(frompos, ID);
            offset = ToAxis(offset, ID);
            _axis.Go(frompos, offset);
        }
        public void SetActionSpeed(int val)
        {
            _axis.SetActionSpeed(val);
        }
        public void SetManualSpeed(int val)
        {
            _axis.SetManualSpeed(val);
        }
        public void Stop()
        {
            _axis.Stop();
        }
        #endregion

        readonly static double[] AX_TO_WORLD = new double[]
        {
            1, 1, 1, 
            1, 1, 1, 
            0.001, 0.0167, 0.0167
        };
        readonly static double[] PERCISIONS = new double[]
        {
            0.01,  0.01,   0.01, 
            0.01,  0.01,   0.01, 
            0.001, 0.0167, 0.0167
        };
        public static double ToWorld(double axis_value, int axisID)
        {
            // 搭配底層實作 (中間轉成 float)
            double w = ((float)axis_value * (float)AX_TO_WORLD[axisID]);
            return Math.Round(w, 4);
        }
        public static double ToAxis(double physic_value, int axisID)
        {
            // 搭配底層實作 (中間轉成 float)
            double a = ((float)physic_value / (float)AX_TO_WORLD[axisID]);
            return Math.Round(a, 4);
        }
        public double Percision
        {
            get { return Math.Round(PERCISIONS[ID], 4); }
        }

        /// <summary>
        /// IAxis 速度介面不夠使用, 暫時直接轉型 PLCMotionClass
        /// </summary>
        public void SetSpeed(SpeedTypeEnum mode)
        {
            ((PLCMotionClass)_axis).SetSpeed(mode);
        }

        internal void set_sim_pos(double pos)
        {
            // 暫時利用 gaara 目前的 IsDebug 模式.
            var pmotor = (PLCMotionClass)_axis;
            pos = ToAxis(pos, ID);
            pmotor.Go((float)pos);
        }
    }
}
