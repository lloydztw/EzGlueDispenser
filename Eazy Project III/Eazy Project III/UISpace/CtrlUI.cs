using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JetEazy;
using Eazy_Project_III.UISpace.CtrlSpace;
using VsCommon.ControlSpace.MachineSpace;
using Eazy_Project_III.ControlSpace.MachineSpace;

namespace PhotoMachine.UISpace
{
    public partial class CtrlUI : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        X3CtrlUI X3Ctrl;
        X1CtrlUI X1Ctrl;

        public CtrlUI()
        {
            InitializeComponent();
            InitialInternal();
        }

        void InitialInternal()
        {

        }

        public void Initial(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            VERSION = version;
            OPTION = option;

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSINGX1:

                            X1Ctrl = new X1CtrlUI();
                            X1Ctrl.Initial(VERSION, OPTION, (DispensingX1MachineClass)machine);
                            X1Ctrl.Location = new Point(0, 0);
                            this.Controls.Add(X1Ctrl);
                            X1Ctrl.Dock = DockStyle.Fill;
                            break;

                        case OptionEnum.DISPENSING:

                            X3Ctrl = new X3CtrlUI();
                            X3Ctrl.Initial(VERSION, OPTION, (DispensingMachineClass)machine);
                            X3Ctrl.Location = new Point(0, 0);
                            this.Controls.Add(X3Ctrl);
                            X3Ctrl.Dock = DockStyle.Fill;
                            break;
                      
                       
                    }

                    break;
                
            }
        }

        //private void AllinoneSDCTRL_TriggerAction(ActionEnum action, string opstr)
        //{
        //    TriggerAction(action, opstr);
        //}

        //private void DFlyCTRL_TriggerAction(ActionEnum action, string opstr)
        //{
        //    TriggerAction(action, opstr);
        //}

        public void Tick()
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSINGX1:
                            X1Ctrl.Tick();
                            break;
                        case OptionEnum.DISPENSING:
                            X3Ctrl.Tick();
                            break;
                    }
                    break;
            }
        }
        public void SetEnable(bool isenable)
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSINGX1:
                            X1Ctrl.SetEnable(isenable);
                            break;
                        case OptionEnum.DISPENSING:
                            X3Ctrl.SetEnable(isenable);
                            break;
                        
                    }
                    break;
            }
        }

        public void myDispose()
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            break;
                    }
                    break;

            }
        }


        public delegate void TriggerHandler(ActionEnum action, string opstr);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(ActionEnum action, string opstr)
        {
            if (TriggerAction != null)
            {
                TriggerAction(action, opstr);
            }
        }

    }
}
