using System;
using System.Threading;


namespace JetEazy.Drivers.Laser
{   
    public interface IxLaser : IDisposable
    {
        event EventHandler<double> OnScanned;
        double Distance { get; }
        double Scan();
        bool IsAutoScanning();
        void StartAutoScan();
        void StopAutoScan();
    }
}
