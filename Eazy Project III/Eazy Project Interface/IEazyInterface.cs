using System.Drawing;


namespace Eazy_Project_Interface
{
    /// <summary>
    /// 單點 On/Off 型態的制動器, 之統一接口 <br/>
    /// @LETIAN: 20220619 creation
    /// </summary>
    public interface IxActuator
    {
        string Name { get; }
        void Set(bool on);
        bool IsOn();
        bool IsOff();
    }

    /// <summary>
    /// 真空接口
    /// </summary>
    public interface IVac : IxActuator
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

    public interface ICylinder : IxActuator
    {
        void SetFront();
        void SetBack();
        bool GetFrontOK();
        bool GetBackOK();
    }
    public interface IDispensing : IxActuator
    {
        void Seton();
        void Seton(int msec);
        void Setoff();
    }
    public interface IUV : IxActuator
    {
        void Seton();
        void Seton(int msec);
        void Setoff();
    }
    public enum ProjectColor
    {
        LightRed = 0,
        LightGreen = 1,
        LightBlue = 2,
    }
    public interface IProjector : IxActuator
    {
        void Seton();
        void Setoff();
        /// <summary>
        /// 控制光機的顔色及開與関
        /// </summary>
        /// <param name="eLightColor">設定光機使用的顔色</param>
        /// <param name="on">開關 true:開 false:関</param>
        void SetColor(ProjectColor eLightColor, bool on);
        /// <summary>
        /// 設定光機輸出的GAIN值
        /// </summary>
        /// <param name="eGainValue">範圍0-100 eg.設定值5 即5%</param>
        void SetGain(int eGainValue);
    }
    public interface ILight : IxActuator
    {
        void Seton();
        void Setoff();
    }
    public interface IKeyence : IxActuator
    {
        void Seton();
        void Setoff();
        void SetZero();
        string GetStatus();
    }

    public interface IAxis
    {
        bool IsError { get; }
        bool IsOK { get; }

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
        bool IsSim();
        void Initial(string inipara);
        void SetExposure(int val);
        void SetGain(float val);
        void StartCapture();
        void StopCapture();
        void Snap();
        System.Drawing.Bitmap GetSnap(int msec = 1000);
        int RotateAngle { get; set; }
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
