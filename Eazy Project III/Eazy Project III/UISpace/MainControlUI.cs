using JetEazy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eazy_Project_III.UISpace.MainSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace Eazy_Project_III.UISpace
{
    public partial class MainControlUI : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        MainX3UI mainX3;
        MainX1UI mainX1;

        public MainControlUI()
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
                        case OptionEnum.DISPENSING:

                            mainX3 = new MainX3UI();
                            mainX3.Init();
                            mainX3.Location = new Point(0, 0);
                            this.Controls.Add(mainX3);
                            //X3INPUT = new X3INPUTUI();
                            //X3INPUT.Initial(VERSION, OPTION, (DispensingMachineClass)machine);
                            //X3INPUT.Location = new Point(0, 0);
                            //this.Controls.Add(X3INPUT);

                            break;
                        case OptionEnum.DISPENSINGX1:

                            mainX1 = new MainX1UI();
                            mainX1.Init();
                            mainX1.Location = new Point(0, 0);
                            this.Controls.Add(mainX1);
                            //X3INPUT = new X3INPUTUI();
                            //X3INPUT.Initial(VERSION, OPTION, (DispensingMachineClass)machine);
                            //X3INPUT.Location = new Point(0, 0);
                            //this.Controls.Add(X3INPUT);

                            break;


                    }

                    break;

            }
        }
        public void Close()
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            mainX3.Close();
                            break;
                        case OptionEnum.DISPENSINGX1:
                            mainX1.Close();
                            break;
                    }
                    break;
            }
        }

        public void Tick()
        {
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            mainX3.Tick();
                            break;
                        case OptionEnum.DISPENSINGX1:
                            mainX1.Tick();
                            break;
                    }
                    break;
            }
        }

    }
}
