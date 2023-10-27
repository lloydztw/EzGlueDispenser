/*
 *  以下 Interface 可以由 IxActuator + Name 取代:
 *  
 *      IVac
 *      ICylinder
 *      IDispensing
 *      IUV
 *      ILight
 * 
 */


namespace Eazy_Project_Interface
{
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

    public interface ILight : IxActuator
    {
        void Seton();
        void Setoff();
    }
}
