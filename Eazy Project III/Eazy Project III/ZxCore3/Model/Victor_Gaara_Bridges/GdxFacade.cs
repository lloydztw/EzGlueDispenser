using Eazy_Project_III.ControlSpace.IOSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_Interface;
using Eazy_Project_Measure;
using JetEazy.ControlSpace.PLCSpace;
using JetEazy.Drivers.Laser;
using System.Collections.Generic;

namespace JetEazy.GdxCore3.Model
{
    /// <summary>
    /// GxFacade 
    /// <para>負責 連結到 Victor/Gaara Space 的 統合橋接器</para>
    /// </summary>
    public class GdxFacade
    {
        #region PRIVATE_MEMBER
        private IxLaser _laser = null;
        //private IAxis[] _motors = new IAxis[9];
        #endregion

        #region SINGLETON
        static GdxFacade _singleton = null;
        private GdxFacade()
        {
            // 強制使用 singleton
        }
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
        internal GdxFacadeIO IO
        {
            get;
            private set;
        }
        internal GdxFacadeIni INI
        {
            get { return GdxFacadeIni.Singleton; }
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
                if (IsSimPLC())
                    _laser = new Sim.GdxLaser();
                else
                    _laser = new GdxLaser(getGaaraLaser());
            }

            if (LaserCoordsTransform == null)
            {
                LaserCoordsTransform = new GdxLaserCenterCompensator();
            }

            if (MotorCoordsTransform == null)
            {
                MotorCoordsTransform = new GdxMotorCoordsTransform();
            }
        }
        public void Dispose()
        {
            if (_laser != null)
            {
                _laser.Dispose();
                _laser = null;
            }
            if (LaserCoordsTransform != null)
            {
                LaserCoordsTransform.Dispose();
                LaserCoordsTransform = null;
            }
            if (MotorCoordsTransform != null)
            {
                MotorCoordsTransform.Dispose();
                MotorCoordsTransform = null;
            }
        }

        public bool IsSimCamera(int camID)
        {
            var cameras = Eazy_Project_III.Universal.CAMERAS;
            if (cameras == null || camID >= cameras.Length || cameras[camID] == null)
                return true;
            return cameras[camID].IsSim();
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
        public ICam CameraCali
        {
            get { return Eazy_Project_III.Universal.CAMERAS[0]; }
        }
        public ICam CameraBlackBox
        {
            get { return Eazy_Project_III.Universal.CAMERAS[1]; }
        }
        public IAxis GetMotor(int axisID)
        {
#if(false)
            if (axisID < _motors.Length)
            {
                var motor = _motors[axisID];
                if (motor == null)
                {
                    var machine = (DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE;
                    var ga_axis = machine.PLCMOTIONCollection[axisID];
                    //> motor = new GdxMotor(ga_axis, axisID);
                    _motors[axisID] = ga_axis;
                }
                return motor;
            }
            return null;
#endif
            var machine = (DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE;
            var ga_axis = machine.PLCMOTIONCollection[axisID];
            return ga_axis;
        }


        public GdxLaserCenterCompensator LaserCoordsTransform
        {
            get;
            private set;
        }

        public GdxMotorCoordsTransform MotorCoordsTransform
        {
            get;
            private set;
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
