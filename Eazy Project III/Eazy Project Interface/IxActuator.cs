namespace Eazy_Project_Interface
{
    /// <summary>
    /// 單點 On/Off 型態的制動器之共通接口 <br/>
    /// @LETIAN: 20220619 creation
    /// </summary>
    public interface IxActuator
    {
        string Name { get; }
        void Set(bool on);
        bool IsOn();
        bool IsOff();
    }
}
