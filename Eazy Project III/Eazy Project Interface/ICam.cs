using System.Drawing;


namespace Eazy_Project_Interface
{
    public interface ICam
    {
        bool IsSim();
        void Initial(string inipara);
        void SetExposure(int val);
        void StartCapture();
        void StopCapture();
        void Snap();
        Bitmap GetSnap(int msec = 1000);
        int RotateAngle { get; set; }
    }
}
