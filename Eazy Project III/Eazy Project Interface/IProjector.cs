using System.Drawing;


namespace Eazy_Project_Interface
{   
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
}
