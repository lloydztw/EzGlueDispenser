using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_Interface;
using Eazy_Project_Measure;
using JetEazy.ControlSpace.PLCSpace;
using JetEazy.Drivers.Laser;


namespace JetEazy.GdxCore3.Model
{
    /// <summary>
    /// GxFacade 
    /// <para>負責 連結到 Victor/Gaara Space 的 統合橋接器</para>
    /// </summary>
    public class GdxFacade
    {
        #region PRIVATE_MEMBER
        static GdxFacade _singleton = null;
        private IxLaser _laser = null;
        #endregion

        public static GdxFacade Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new GdxFacade();
                return _singleton;
            }
        }
        private GdxFacade()
        {
            // 強制使用 singleton
        }
        internal GdxFacadeIO IO
        {
            get;
            private set;
        }

        public void Init()
        {
            if (IO == null)
            {
                IO = new GdxFacadeIO();
                IO.BindIoPoints();
            }
            if (_laser == null)
            {
                _laser = new GdxLaser(getGaaraLaser());
            }
        }
        public void Dispose()
        {
            if (_laser != null)
            {
                _laser.Dispose();
                _laser = null;
            }
        }

        public bool IsSimCamera(int camID)
        {
            // Universal.IsNoUseCCD 無法反映 camera 是否為模擬
            // 改用 ICam.IsSim()
            var cam = camID == 0 ? CameraCali : CameraBlackBox;
            if (cam == null)
                return true;
            return cam.IsSim();
        }
        public bool IsSimMotor()
        {
            return Eazy_Project_III.Universal.IsNoUseMotor;
        }
        public bool IsSimPLC()
        {
            return Eazy_Project_III.Universal.IsNoUseIO;
        }

        public IxLaser GetLaser(int id = 0)
        {
            return _laser;
        }

        /// <summary>
        /// 检查校正的相机
        /// </summary>
        public ICam CameraCali
        {
            get { return Eazy_Project_III.Universal.CAMERAS[0]; }
        }

        /// <summary>
        /// 投影的校正相机
        /// </summary>
        public ICam CameraBlackBox
        {
            get { return Eazy_Project_III.Universal.CAMERAS[1]; }
        }

        /// <summary>
        /// Motor
        /// </summary>
        public IAxis GetMotor(int axisID)
        {
            var machine = (DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE;
            return machine.PLCMOTIONCollection[axisID];
        }


        #region INTERNAL_FUNCTIONS
        internal VsCommPLC GetPLC(int plcID = 0)
        {
            var machine = (DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE;
            var plc = machine.PLCCollection[plcID];
            return plc;
        }
        internal DispensingIOClass GetIoMeta()
        {
            var machine = (DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE;
            return machine.PLCIO;
        }
        LEClass getGaaraLaser()
        {
            return LEClass.Instance;
        }
        #endregion
    }
}
