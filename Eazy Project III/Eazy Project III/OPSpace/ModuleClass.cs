using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eazy_Project_III.OPSpace
{
    public class ModuleClass
    {
        //private static readonly ModuleClass _instance = new ModuleClass();
        //public static ModuleClass VsInstance
        //{
        //    get
        //    {
        //        return _instance;
        //    }
        //}

        public DispensingMachineClass MACHINE = null;
        public virtual void SetMachine(DispensingMachineClass eMachine)
        {
            MACHINE = eMachine;
        }

        public virtual string Name
        {
            get { return GetType().Name; }
        }
    }

    public class Vac : ModuleClass, IVac
    {
        private static readonly Vac _instance = new Vac();
        public static Vac Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// 开启真空，无停止
        /// </summary>
        public void Seton()
        {
            MACHINE.PLCIO.SetOutputIndex(4, true);
        }
        /// <summary>
        /// 关闭真空
        /// </summary>
        public void Setoff()
        {
            MACHINE.PLCIO.SetOutputIndex(4, false);
        }
        /// <summary>
        /// 确定是否真空吸附
        /// </summary>
        /// <returns>true:是 false:否</returns>
        public bool GetVacOK()
        {
            MACHINE.PLCIO.GetInputIndex(6);
            return true;
        }

        public void Set(bool on)
        {
            if (on)
                Seton();
            else
                Setoff();
        }
        public bool IsOn()
        {
            return GetVacOK();
        }
        public bool IsOff()
        {
            return GetVacOK();
        }
    }

    public class UVCylinder : ModuleClass, ICylinder
    {
        private static readonly UVCylinder _instance = new UVCylinder();
        public static UVCylinder Instance
        {
            get
            {
                return _instance;
            }
        }

        public void SetFront()
        {
            MACHINE.PLCIO.SetOutputIndex(1, true);
        }
        public void SetBack()
        {
            MACHINE.PLCIO.SetOutputIndex(0, true);
        }
        
        public bool GetFrontOK()
        {
            if (Universal.IsNoUseIO)
                return true;
            return MACHINE.PLCIO.GetInputIndex(2);
        }
        public bool GetBackOK()
        {
            if (Universal.IsNoUseIO)
                return true;
            return MACHINE.PLCIO.GetInputIndex(1);
        }

        public void Set(bool on)
        {
            if (on)
                SetFront();
            else
                SetBack();
        }
        public bool IsOn()
        {
            return GetFrontOK();
        }
        public bool IsOff()
        {
            return GetBackOK();
        }

    }
    public class Dispensing : ModuleClass, IDispensing
    {
        private static readonly Dispensing _instance = new Dispensing();
        public static Dispensing Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Seton()
        {
            MACHINE.PLCIO.SetOutputIndex(7, true);
        }
        public void Seton(int msec)
        {

        }
        public void Setoff()
        {
            MACHINE.PLCIO.SetOutputIndex(7, false);
        }

        public void Set(bool on)
        {
            if (on)
                Seton();
            else
                Setoff();
        }
        public bool IsOn()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
        public bool IsOff()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }

    }
    public class UV : ModuleClass, IUV
    {
        private static readonly UV _instance = new UV();
        public static UV Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Seton()
        {
            MACHINE.PLCIO.SetOutputIndex(6, true);
        }
        public void Seton(int msec)
        {

        }
        public void Setoff()
        {
            MACHINE.PLCIO.SetOutputIndex(6, false);
        }

        public void Set(bool on)
        {
            if (on)
                Seton();
            else
                Setoff();
        }
        public bool IsOn()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
        public bool IsOff()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
    }

    public class Projector : ModuleClass, IProjector
    {
        private static readonly Projector _instance = new Projector();
        public static Projector Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Seton()
        {

        }
        public void Setoff()
        {

        }

        public void Set(bool on)
        {
            if (on)
                Seton();
            else
                Setoff();
        }
        public bool IsOn()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
        public bool IsOff()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }

    }
    public class Light : ModuleClass, ILight
    {
        private static readonly Light _instance = new Light();
        public static Light Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Seton()
        {
            MACHINE.PLCIO.SetOutputIndex(16, true);
        }
        public void Setoff()
        {
            MACHINE.PLCIO.SetOutputIndex(16, false);
        }

        public void Set(bool on)
        {
            if (on)
                Seton();
            else
                Setoff();
        }
        public bool IsOn()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
        public bool IsOff()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }

    }
    public class Keyence : ModuleClass, IKeyence
    {
        private static readonly Keyence _instance = new Keyence();
        public static Keyence Instance
        {
            get
            {
                return _instance;
            }
        }
        public void Seton()
        {

        }
        public void Setoff()
        {

        }
        public void SetZero()
        {

        }
        public string GetStatus()
        {
            return string.Empty;
        }

        public void Set(bool on)
        {
            if (on)
                Seton();
            else
                Setoff();
        }
        public bool IsOn()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
        public bool IsOff()
        {
            // 有對應點位嗎?
            // 沒有的話內建
            // cache data 反映最後一次的 Set On/Off
            return false;
        }
    }
}
