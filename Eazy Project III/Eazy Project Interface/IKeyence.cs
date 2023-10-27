namespace Eazy_Project_Interface
{
    public interface IKeyence : IxActuator
    {
        void Seton();           // 
        void Setoff();
        void SetZero();
        string GetStatus();
    }
}
