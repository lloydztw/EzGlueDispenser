using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Eazy_Project_Interface
{
    /// <summary>
    /// 真空接口
    /// </summary>
    public interface IVac
    {
        /// <summary>
        /// 开启真空，无停止
        /// </summary>
        void Seton();
        /// <summary>
        /// 关闭真空
        /// </summary>
        void Setoff();
        /// <summary>
        /// 确定是否真空吸附
        /// </summary>
        /// <returns>true:是 false:否</returns>
        bool GetVacOK();
    }

    public interface ICylinder
    {
        void SetFront();
        void SetBack();
        bool GetFrontOK();
        bool GetBackOK();
    }
    public interface IDispensing
    {
        void Seton();
        void Seton(int msec);
        void Setoff();
    }
    public interface IUV
    {
        void Seton();
        void Seton(int msec);
        void Setoff();
    }
    public interface IProjector
    {
        void Seton();
        void Setoff();
    }
    public interface ILight
    {
        void Seton();
        void Setoff();
    }
    public interface IKeyence
    {
        void Seton();
        void Setoff();
        void SetZero();
        string GetStatus();
    }

    public interface IAxis
    {
        void Forward();
        void Backward();
        void Stop();
        void Go(double frompos, double offset);
        void SetManualSpeed(int val);
        void SetActionSpeed(int val);
        double GetPos();
        string GetStatus();

        double GetInitPosition();

    }

    public interface ICam
    {
        void Initial(string inipara);
        void SetExposure(int val);
        void StartCapture();
        void StopCapture();
        void Snap();
        System.Drawing.Bitmap GetSnap();
    }

    public interface IRecipe
    {

        int CaliCamExpo { get; set; }
        string CaliCamCaliData { get; set; }
        PointF CaliPicCenter { get; set; }
        int JudgeCamExpo { get; set; }
        int JudgeCamCaliData { get; set; }
        string JudgeThetaOffset { get; set; }
        int DispensingTime { get; set; }
        int UVTime { get; set; }
        string OtherRecipe { get; set; }
    }

}
