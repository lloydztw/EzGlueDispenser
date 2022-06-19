using Eazy_Project_Measure;
using JetEazy.Drivers.Laser;
using JetEazy.GdxCore3.Model;
using System;
using System.Threading;


namespace JetEazy.GdxCore3
{
    /// <summary>
    /// 對 Gaara LEClass 的包裝
    /// </summary>
    public class GdxLaser : IxLaser
    {
        public event EventHandler<double> OnScanned;

        #region PRIVATE_DATA
        /// <summary>
        /// 精度位數
        /// </summary>
        const int PERCISION = 6;
        /// <summary>
        /// 量測到的距離 cache data
        /// </summary>
        private double _distance = 0;
        #endregion

        #region PRIVATE_LASER_IMPLEMENT
        LEClass _ga_laser;
        #endregion

        public GdxLaser(LEClass ga_laser)
        {
            _ga_laser = ga_laser;
        }
        public double Distance
        {
            get
            {
                double value = 0;
                Interlocked.Exchange(ref value, _distance);
                return value;
            }
        }
        public double Scan()
        {
            double dist = read_dist_from_hardware();
            Interlocked.Exchange(ref _distance, dist);
            return dist;
        }

        public bool IsAutoScanning()
        {
            return (_runFlag || _thread != null);
        }
        public void StartAutoScan()
        {
            start_scan_thread();
        }
        public void StopAutoScan()
        {
            stop_scan_thread();
        }
        public void Dispose()
        {
            StopAutoScan();
        }


        #region PRIVATE_THREAD_FUNCTIONS
        Thread _thread = null;
        bool _runFlag = false;
        bool is_thread_running()
        {
            return _runFlag || _thread != null;
        }
        void start_scan_thread()
        {
            if (!is_thread_running())
            {
                _runFlag = true;
                _thread = new Thread(polling_func);
                _thread.Start();
            }
            else
            {
                GdxGlobal.LOG.Warn("有 Thread 尚未結束");
            }
        }
        void stop_scan_thread(int timeout = 3000)
        {
            if (is_thread_running())
            {
                _runFlag = false;
                quit_gaara_laser();
                try
                {
                    if (!_thread.Join(timeout))
                        _thread.Abort();
                    _thread = null;
                }
                catch(Exception ex)
                {
                    GdxGlobal.LOG.Warn(ex, "無法終止 Thread!");
                }
            }
        }
        void polling_func()
        {
            while (_runFlag)
            {
                Thread.Sleep(10);
                try
                {
                    double dist = Scan();

                    // Fire Event
                    if (OnScanned != null)
                    {
                        OnScanned.Invoke(this, _distance);
                    }
                }
                catch (Exception ex)
                {
                    if (_runFlag)
                        GdxGlobal.LOG.Warn(ex, "laser polling 異常!");
                }
            }
            _runFlag = false;
            _thread = null;
        }
        #endregion


        #region PRIVATE_HW_ACCESS_FUNCTIONS
        double read_dist_from_hardware()
        {
            if (_ga_laser != null)
            {
                double d = _ga_laser.Snap();
                d = Math.Round(d, PERCISION);
                return d;
            }
            return 0;
        }
        void quit_gaara_laser()
        {
            // 加快退出時間
            if (_ga_laser != null)
                _ga_laser.CaptureCnt = 1;
        }
        #endregion
    }
}
