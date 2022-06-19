using Eazy_Project_III.ControlSpace.MachineSpace;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace.MotionSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VsCommon.ControlSpace;

namespace Eazy_Project_III.FormSpace
{
    public partial class INIPositionForm : Form
    {

        [DefaultPropertyAttribute("Environment Position")]
        public class PositionSettings
        {
            private string[] position = new string[10];

            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS0
            {
                get { return position[0]; }
                set { position[0] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS1
            {
                get { return position[1]; }
                set { position[1] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS2
            {
                get { return position[2]; }
                set { position[2] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS3
            {
                get { return position[3]; }
                set { position[3] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS4
            {
                get { return position[4]; }
                set { position[4] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS5
            {
                get { return position[5]; }
                set { position[5] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS6
            {
                get { return position[6]; }
                set { position[6] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS7
            {
                get { return position[7]; }
                set { position[7] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS8
            {
                get { return position[8]; }
                set { position[8] = value; }
            }
            [CategoryAttribute("POS Settings"),
            DefaultValueAttribute("0"), ReadOnly(false)]
            public string POS9
            {
                get { return position[9]; }
                set { position[9] = value; }
            }

            public override string ToString()
            {
                string retstr = "";

                int i = 0;

                while (i < position.Length)
                {
                    retstr += position[i] + ";";
                    i++;
                }
                retstr = retstr.Remove(retstr.Length - 1, 1);

                return retstr;
            }

            public void SetPosition(int index, string str)
            {
                position[index] = str;

            }
            public int GetPosCount()
            {
                return position.Length;
            }

        }
        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        PLCMotionClass AXIS_0
        {
            get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[0]; }
        }
        PLCMotionClass AXIS_1
        {
            get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[1]; }
        }
        PLCMotionClass AXIS_2
        {
            get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[2]; }
        }


        Button btnGo;
        Button btnDel;
        Button btnSetPosition;

        Button btnOK;
        Button btnCancel;

        Button btnAXISFRM;

        PropertyGrid ppgPosition;

        PositionSettings PositionSetting;

        bool IsNeedToChange = false;

        private string genposition = string.Empty;

        public INIPositionForm(string epos)
        {
            genposition = epos;

            InitializeComponent();
            this.Load += INIPositionForm_Load;
        }

        private void INIPositionForm_Load(object sender, EventArgs e)
        {
            btnGo = button4;
            btnDel = button3;
            btnSetPosition = button1;

            btnOK = button2;
            btnCancel = button5;
            btnAXISFRM = button6;

            ppgPosition = propertyGrid3;
            ppgPosition.PropertyValueChanged += PpgPosition_PropertyValueChanged;

            btnGo.Click += BtnGo_Click;
            btnDel.Click += BtnDel_Click;
            btnSetPosition.Click += BtnSetPosition_Click;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnAXISFRM.Click += BtnAXISFRM_Click;

            PositionSetting = new PositionSettings();
            IsNeedToChange = true;

            FillDisplay();

            //LanguageExClass.Instance.EnumControls(this);

        }
        frmAXISSetup mMotorForm = null;
        private void BtnAXISFRM_Click(object sender, EventArgs e)
        {
            //if (!Universal.IsOpenMotorWindows)
            //{
            //    Universal.IsOpenMotorWindows = true;
            //    //MACHINE.PLCReadCmdNormalTemp(true);
            //    //System.Threading.Thread.Sleep(500);
            //    mMotorForm = new frmAXISSetup();
            //    mMotorForm.Show();
            //}
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            JzToolsClass.PassingString = genposition;
            this.DialogResult = DialogResult.OK;
        }

        private void BtnSetPosition_Click(object sender, EventArgs e)
        {
            string strpos = AXIS_0.PositionNowString + "," + AXIS_1.PositionNowString + "," + AXIS_2.PositionNowString;
            SetPosition(strpos);
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            
        }

        private void PpgPosition_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (!IsNeedToChange)
                return;

            WriteBackPosition();
        }

        public void SetPosition(string posstr)
        {
            int i = 0;

            string selectstr = ppgPosition.SelectedGridItem.Label;

            switch (selectstr)
            {
                case "POS0":
                    PositionSetting.POS0 = posstr;
                    break;
                case "POS1":
                    PositionSetting.POS1 = posstr;
                    break;
                case "POS2":
                    PositionSetting.POS2 = posstr;
                    break;
                case "POS3":
                    PositionSetting.POS3 = posstr;
                    break;
                case "POS4":
                    PositionSetting.POS4 = posstr;
                    break;
                case "POS5":
                    PositionSetting.POS5 = posstr;
                    break;
                case "POS6":
                    PositionSetting.POS6 = posstr;
                    break;
                case "POS7":
                    PositionSetting.POS7 = posstr;
                    break;
                case "POS8":
                    PositionSetting.POS8 = posstr;
                    break;
                case "POS9":
                    PositionSetting.POS9 = posstr;
                    break;
            }

            WriteBackPosition();
            FillPosition();
        }
        public string GetPosition()
        {
            int i = 0;

            string retstr = "";

            string selectstr = ppgPosition.SelectedGridItem.Label;

            switch (selectstr)
            {
                case "POS0":
                    retstr = PositionSetting.POS0;
                    break;
                case "POS1":
                    retstr = PositionSetting.POS1;
                    break;
                case "POS2":
                    retstr = PositionSetting.POS2;
                    break;
                case "POS3":
                    retstr = PositionSetting.POS3;
                    break;
                case "POS4":
                    retstr = PositionSetting.POS4;
                    break;
                case "POS5":
                    retstr = PositionSetting.POS5;
                    break;
                case "POS6":
                    retstr = PositionSetting.POS6;
                    break;
                case "POS7":
                    retstr = PositionSetting.POS7;
                    break;
                case "POS8":
                    retstr = PositionSetting.POS8;
                    break;
                case "POS9":
                    retstr = PositionSetting.POS9;
                    break;
            }

            return retstr;

        }

        void WriteBackPosition()
        {
            genposition = PositionSetting.ToString();
           
        }

        void FillDisplay()
        {
            FillPosition();
        }
        void FillPosition()
        {
            IsNeedToChange = false;

            int i = 0;
            string PositionString = genposition;

            string[] positionstrs = PositionString.Split(';');

            i = 0;
            foreach (string str in positionstrs)
            {
                if (i < PositionSetting.GetPosCount())
                    PositionSetting.SetPosition(i, str);
                i++;
            }

            ppgPosition.SelectedObject = PositionSetting;
           
            IsNeedToChange = true;
        }
    }
}
