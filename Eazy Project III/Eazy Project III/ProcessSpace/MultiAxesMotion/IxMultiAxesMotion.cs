using JetEazy.ControlSpace.MotionSpace;
using System;


namespace Eazy_Project_III.ProcessSpace.MultiAxesMotion
{
    using QVector = Array;

    public interface IxMultiAxesMotion
    {
        string Name { get; }
        string[] AxisNames { get; }
        int[] AxisIDs { get; }
        int Dimensions { get; }

        bool IsReady();
        bool IsError();
        bool IsTimeout();

        QVector GetCurrentPos(bool pollingHardware = true);
        void SetSpeed(SpeedTypeEnum mode);
        void StartMoveTo(QVector pos);
    }
}
