namespace Eazy_Project_Interface
{
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
}
