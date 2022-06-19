using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JetEazy;
using JetEazy.BasicSpace;

//using Mist.OPSpace;
//using Mist.DBSpace;
//using PhotoMachine.ControlSpace;

namespace PhotoMachine.UISpace
{
    public partial class RunUI : UserControl
    {
        const int ShinningDuriation = 50;
        const int ShiningTimes = 2;

        bool IsResultPass = false;

        //RESULTClass RESULT;
        //UseIOClass USEIO;

        public bool IsSaveRaw
        {
            get
            {
                return chkIsSaveRaw.Checked;
            }
        }
        public bool IsSaveNGRaw
        {
            get
            {
                return chkIsSaveNGRaw.Checked;
            }
        }
        public bool IsSaveDebug
        {
            get
            {
                return chkIsSaveDebug.Checked;
            }
        }


        Label lblBigPass;

        TextBox txtProductBarcode;
        TextBox txtOPBarcode;
        Label lblDuriation;
        TextBox txtResult;

        CheckBox chkIsSaveDebug;
        CheckBox chkIsSaveRaw;
        CheckBox chkIsSaveNGRaw;

        JzToolsClass JzTools = new JzToolsClass();

        //Language Setup
        JzLanguageClass myLanguage = new JzLanguageClass();

        string UIPath = "";
        int LanguageIndex = 0;

        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;

        public RunUI()
        {
            InitializeComponent();
            Initial();
        }
        void Initial()
        {
            lblBigPass = label4;
            lblDuriation = label2;

            txtOPBarcode = textBox1;
            txtProductBarcode = textBox3;
            txtResult = textBox2;

            chkIsSaveRaw = checkBox1;
            chkIsSaveNGRaw = checkBox2;
            chkIsSaveDebug = checkBox3;

            
            txtProductBarcode.KeyDown += new KeyEventHandler(txtProductBarcode_KeyDown);
            txtOPBarcode.KeyDown += new KeyEventHandler(txtBarcode_KeyDown);

        }
        
        void txtProductBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                txtOPBarcode.Focus();
                txtOPBarcode.SelectAll();
            }
        }

        void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                OnTrigger(RunStatusEnum.STARTRUN);
            }
        }

        public bool SetBarcodeEnable
        {
            set
            {
                txtProductBarcode.Enabled = value;
                txtOPBarcode.Enabled = value;

                if (value)
                {
                    txtOPBarcode.Text = "";
                    txtOPBarcode.Focus();
                }
            }

        }
        public string SetBarcodeString
        {
            set
            {
                txtOPBarcode.Text = value;
                txtOPBarcode.Focus();
            }
        }

        //public void Initial(string uipath,
        //    int langindex,
        //    VersionEnum ver,
        //    OptionEnum opt,
        //    RESULTClass result,
        //    UseIOClass useio)
        //{
        //    UIPath = uipath;
        //    LanguageIndex = langindex;
        //    VER = ver;
        //    OPT = opt;
        //    RESULT = result;

        //    USEIO = useio;

        //}

        public void Initial(string uipath,
            int langindex,
            VersionEnum ver,
            OptionEnum opt)
            //RESULTClass result,
            //UseIOClass useio)
        {
            UIPath = uipath;
            LanguageIndex = langindex;
            VER = ver;
            OPT = opt;
            //RESULT = result;

            //USEIO = useio;

        }
        public string GetProductBarcode()
        {
            return txtProductBarcode.Text.Trim();
        }
        public string GetOPBarcode()
        {
            return txtOPBarcode.Text.Trim();
        }

        public void StartShinnig(bool ispass)
        {
            IsResultPass = ispass;
            ShinningProcess.Start();
        }
        public void SetDuriation(string inputstr)
        {
            lblDuriation.Text = inputstr;
        }

        public bool IsShinning
        {
            get
            {
                return ShinningProcess.IsOn;
            }
        }
        int ShinigCount = 0;
        ProcessClass ShinningProcess = new ProcessClass();
        public void ShinningTick()
        {
            ProcessClass Process = ShinningProcess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        //lblBigPass.Visible = IsPass;
                        if (ShinigCount == 0)
                        {
                            Process.TimeUnit = TimeUnitEnum.ms;
                            lblBigPass.Text = (IsResultPass ? "PASS" : "NG");
                        }

                        if (ShinigCount == 0 || Process.IsTimeup)
                        {
                            lblBigPass.ForeColor = (IsResultPass ? Color.Lime : Color.Red);

                            //if (IsResultPass)
                            //    ShineGreen();
                            //else
                            //    ShineRed();

                            lblBigPass.Refresh();

                            Process.ID = 10;
                            Process.NextDuriation = 100;
                        }
                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            lblBigPass.ForeColor = (IsResultPass ? Color.Green : Color.DarkRed);

                            //ShineNothing();

                            lblBigPass.Refresh();

                            ShinigCount++;

                            if (ShinigCount > ShiningTimes)
                            {

                                ShinigCount = 0;
                                //OnTrigger((IsPass ? StatusEnum.CALPASS : StatusEnum.CALNG));

                                //OnTrigger(StatusEnum.CALEND);
                                OnTrigger(RunStatusEnum.SHINNIGEND);

                                Process.Stop();
                            }
                            else
                                Process.ID = 5;
                        }
                        break;
                }
            }
        }

        //void ShineGreen()
        //{
        //    USEIO.LEDGreen = true;
        //    USEIO.LEDRed = false;
        //    USEIO.LEDYellow = false;
        //}
        //void ShineRed()
        //{
        //    USEIO.LEDGreen = false;
        //    USEIO.LEDRed = true;
        //    USEIO.LEDYellow = false;
        //}
        //void ShineNothing()
        //{
        //    USEIO.LEDGreen = false;
        //    USEIO.LEDRed = false;
        //    USEIO.LEDYellow = false;
        //}
        public void Tick()
        {
            ShinningTick();
        }

        public void ClearResult()
        {
            IsResultPass = false;
            txtResult.Clear();
        }
        public void SetResultLine(string str)
        {
            txtResult.AppendText(str + Environment.NewLine);
        }

        public void SetResultText(string str)
        {
            txtResult.Text = str;
            txtResult.Refresh();
        }

        public void SaveResultLog(string filepath)
        {
            JzTools.SaveData(txtResult.Text, filepath);
        }

        public delegate void TriggerHandler(RunStatusEnum Status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(RunStatusEnum Status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(Status);
            }
        }

        public delegate void BarcodeHandler(string barcode);
        public event BarcodeHandler BarcodeAction;
        public void OnBarcode(string barcode)
        {
            if (BarcodeAction != null)
            {
                BarcodeAction(barcode);
            }
        }

        public delegate void RunHandler(RunStatusEnum Status, string opstring);
        public event RunHandler RunAction;
        public void OnTrigger(RunStatusEnum Status, string opstring)
        {
            if (RunAction != null)
            {
                RunAction(Status, opstring);
            }
        }
    }
}