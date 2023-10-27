using Eazy_Project_III.ControlSpace.MachineSpace;
using Eazy_Project_III.FormSpace.FormX1;
using JetEazy;
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
    public partial class frmDataGridViewPosition : Form
    {

        VersionEnum VERSION
        {
            get
            {
                return Universal.VERSION;
            }
        }
        OptionEnum OPTION
        {
            get
            {
                return Universal.OPTION;
            }
        }

        Button btnADD;
        Button btnDEL;
        Button btnUpdate;
        Button btnGO;
        Button btnOK;
        Button btnCancel;
        Button btnAXIS;

        DataGridView DGVIEW;

        PICKUI myPICKUI;
        PUTUI myPUTUI;

        private string m_pos = string.Empty;
        private string myText = "位置设定窗口";
        private string myEName = string.Empty;
        /// <summary>
        /// 用来判定位置个数
        /// </summary>
        private int m_pos_count = 1;

        ///// <summary>
        ///// 模组编号
        ///// </summary>
        //private int m_motorGroupIndex = 0;

        private ModuleName m_ModuleName = ModuleName.MODULE_PICK;


        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        //PLCMotionClass AXIS_0
        //{
        //    get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[0]; }
        //}
        //PLCMotionClass AXIS_1
        //{
        //    get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[1]; }
        //}
        //PLCMotionClass AXIS_2
        //{
        //    get { return ((DispensingMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[2]; }
        //}

        public frmDataGridViewPosition(string elabel, string ename, string epos)
        {
            myText = elabel;
            m_pos = epos;
            myEName = ename;

            switch (ename)
            {

                #region 第一站
                case "sDispensingX1_1PosList":
                case "sDispensingX1_2PosList":
                    m_pos_count = 4;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_DISPENSING;
                    break;
                case "SafePosReady":
                case "DispendingPosReady":
                    m_pos_count = 1;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_DISPENSING;
                    break;
                #endregion

                #region 第三站
                case "sMirrorTestDispensingPosList":
                    m_pos_count = 100;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_DISPENSING;
                    break;
                case "sMirror1PosList":
                case "sMirror2PosList":
                    m_pos_count = 4;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_PICK;
                    break;
                case "sMirror1JamedPosList":
                case "sMirror2JamedPosList":
                    m_pos_count = 4;
                    //m_motorGroupIndex = 2;
                    m_ModuleName = ModuleName.MODULE_DISPENSING;
                    break;
                case "sMirror1PlanePosList":
                case "sMirror2PlanePosList":
                case "sMirror0PlanePosList":
                case "sMirror1UVPosList":
                case "sMirror2UVPosList":
                    m_pos_count = 100;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_PICK;
                    break;

                case "LEPos":
                case "AttractPos":

                case "Mirror1CaliPos":
                case "Mirror2CaliPos":
                case "Mirror0CaliPos":
                case "Mirror1PutPos":
                case "Mirror2PutPos":
                case "sMirror1ToMirror2ReadyPos":
                    m_pos_count = 1;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_PICK;
                    break;
                case "sMirrorTestDispensingReady":
                case "ShadowPos":
                case "ShadowPosUp":
                case "InitialDispensingPos":
                case "sMirror1ReadyPos":
                case "sMirror2ReadyPos":
                    m_pos_count = 1;
                    //m_motorGroupIndex = 1;
                    m_ModuleName = ModuleName.MODULE_DISPENSING;
                    break;
                    #endregion

            }

            InitializeComponent();
            this.Load += FrmDataGridViewPosition_Load;
        }

        private void FrmDataGridViewPosition_Load(object sender, EventArgs e)
        {


            this.Location = new Point(Universal.MainFormLocation.X + 5, Universal.MainFormLocation.Y);

            this.Text = myText + " " + JzToolsClass.GetEnumDescription(m_ModuleName) + " 最多可設定 " + m_pos_count.ToString() + " 個位置";
            DGVIEW = dataGridView1;

            btnADD = button1;
            btnDEL = button2;
            btnGO = button3;
            btnOK = button4;
            btnCancel = button5;
            btnAXIS = button6;
            btnUpdate = button7;

            btnADD.Click += BtnADD_Click;
            btnDEL.Click += BtnDEL_Click;
            btnGO.Click += BtnGO_Click;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnAXIS.Click += BtnAXIS_Click;
            btnUpdate.Click += BtnUpdate_Click;


            btnGO.Visible = false;


            myPICKUI = pickui1;
            myPUTUI = putui1;

            //if (myEName.IndexOf("sMirror1") > -1)
            //{
            //    myPICKUI.MirrorIndex = 0;
            //}
            //else if (myEName.IndexOf("sMirror2") > -1)
            //{
            //    myPICKUI.MirrorIndex = 1;
            //}
            //else
            //{
            //    myPICKUI.Visible = false;
            //}

            switch (myEName)
            {
                case "sMirror1PosList":
                    myPICKUI.Visible = true;
                    myPICKUI.MirrorIndex = 0;
                    break;
                case "sMirror2PosList":
                    myPICKUI.Visible = true;
                    myPICKUI.MirrorIndex = 1;
                    break;
                case "Mirror1PutPos":
                case "Mirror1CaliPos":
                case "sMirror1PlanePosList":
                case "sMirror1JamedPosList":
                    myPUTUI.Visible = true;
                    myPUTUI.MirrorIndex = 0;
                    break;
                case "Mirror2PutPos":
                case "Mirror2CaliPos":
                case "sMirror2PlanePosList":
                case "sMirror2JamedPosList":
                    myPUTUI.Visible = true;
                    myPUTUI.MirrorIndex = 1;
                    break;
               
            
                case "sMirror0PlanePosList":
                case "sMirror1UVPosList":
                case "sMirror2UVPosList":
                case "Mirror0CaliPos":
                    break;
                case "ShadowPos":
                case "ShadowPosUp":
                case "InitialDispensingPos":
                    break;
            }

            FillDisplay();

            DGVIEW.SelectionChanged += DGVIEW_SelectionChanged;
            DGVIEW.RowPostPaint += DGVIEW_RowPostPaint;
            myPICKUI.SetMirrorGrpIndex(0);

            //LanguageExClass.Instance.EnumControls(this);
        }

        private void DGVIEW_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X,
                                                                               e.RowBounds.Location.Y,
                                                                               DGVIEW.RowHeadersWidth - 4,
                                                                               e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                                                       DGVIEW.RowHeadersDefaultCellStyle.Font,
                                                       rectangle,
                                                       DGVIEW.RowHeadersDefaultCellStyle.ForeColor,
                                                       TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void DGVIEW_SelectionChanged(object sender, EventArgs e)
        {
            if (DGVIEW.Rows.Count <= 0)
                return;

            int rowindex = DGVIEW.CurrentCell.RowIndex;
            if (rowindex == -1)
                return;

            myPICKUI.SetMirrorGrpIndex(rowindex);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (DGVIEW.Rows.Count <= 0)
                return;

            int rowindex = DGVIEW.CurrentCell.RowIndex;
            if (rowindex == -1)
                return;

            string onStrMsg = "更新 表第 " + rowindex + " 行？";
            string offStrMsg = "更新 表第 " + rowindex + " 行？";
            string msg = (true ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) != DialogResult.OK)
            {
                return;
            }

            string[] strpos = MACHINECollection.GetModulePosition(m_ModuleName).Split(',').ToArray();

            if (strpos.Length >= 3)
            {
                this.DGVIEW.Rows[rowindex].Cells[0].Value = strpos[0];
                this.DGVIEW.Rows[rowindex].Cells[1].Value = strpos[1];
                this.DGVIEW.Rows[rowindex].Cells[2].Value = strpos[2];
            }
            
        }

        frmAXISSetup mMotorForm = null;
        frmMotor mMotorFromX1 = null;

        private void BtnAXIS_Click(object sender, EventArgs e)
        {
            if (!Universal.IsOpenMotorWindows)
            {
                Universal.IsOpenMotorWindows = true;
                //MACHINE.PLCReadCmdNormalTemp(true);
                //System.Threading.Thread.Sleep(500);

                switch(VERSION)
                {
                    case VersionEnum.PROJECT:
                        switch(OPTION)
                        {
                            case OptionEnum.DISPENSINGX1:

                                mMotorFromX1 = new frmMotor();
                                mMotorFromX1.StartPosition = FormStartPosition.Manual;
                                mMotorFromX1.Location = new Point(this.Location.X + 5, this.Location.Y + 220);
                                mMotorFromX1.Show();

                                break;
                            case OptionEnum.DISPENSING:
                                mMotorForm = new frmAXISSetup();
                                mMotorForm.StartPosition = FormStartPosition.Manual;
                                mMotorForm.Location = new Point(this.Location.X + 5, this.Location.Y + 220);
                                mMotorForm.Show();
                                break;
                        }
                        break;
                }

               
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {

            string strresult = string.Empty;
            int i = 0;
            while (i < DGVIEW.Rows.Count)
            {
                strresult += DGVIEW.Rows[i].Cells[0].Value.ToString() + ",";
                strresult += DGVIEW.Rows[i].Cells[1].Value.ToString() + ",";
                strresult += DGVIEW.Rows[i].Cells[2].Value.ToString() + ";";
                i++;
            }

            strresult = RemoveLastChar(strresult, 1);
            //m_pos = strresult;
            JetEazy.BasicSpace.JzToolsClass.PassingString = strresult;

            this.DialogResult = DialogResult.OK;
        }

        public string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }

        private void BtnGO_Click(object sender, EventArgs e)
        {
            //if (DGVIEW.Rows.Count <= 0)
            //    return;

            //int rowindex = DGVIEW.CurrentCell.RowIndex;
            //if (rowindex == -1)
            //    return;

            //string onStrMsg = JzToolsClass.GetEnumDescription(m_ModuleName) + " 定位至表第 " + rowindex + " 行位置？";
            //string offStrMsg = JzToolsClass.GetEnumDescription(m_ModuleName) + " 定位至表第 " + rowindex + " 行位置？";
            //string msg = (true ? offStrMsg : onStrMsg);

            //if (VsMSG.Instance.Question(msg) != DialogResult.OK)
            //{
            //    return;
            //}

            //string pos = string.Empty;
            //pos += DGVIEW.Rows[rowindex].Cells[0].Value.ToString() + ",";
            //pos += DGVIEW.Rows[rowindex].Cells[1].Value.ToString() + ",";
            //pos += DGVIEW.Rows[rowindex].Cells[2].Value.ToString();

            //((DispensingMachineClass)MACHINECollection.MACHINE).PLCIO.ModulePositionSet(m_ModuleName, 1, pos);
            //((DispensingMachineClass)MACHINECollection.MACHINE).PLCIO.ModulePositionGO(m_ModuleName, 1);

        }

        private void BtnDEL_Click(object sender, EventArgs e)
        {
            if (DGVIEW.Rows.Count <= 0)
                return;

            int rowindex = DGVIEW.CurrentCell.RowIndex;
            if (rowindex == -1)
                return;

            string onStrMsg = "删除 表第 " + rowindex + " 行？";
            string offStrMsg = "删除 表第 " + rowindex + " 行？";
            string msg = (true ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) != DialogResult.OK)
            {
                return;
            }

            DGVIEW.Rows.RemoveAt(rowindex);

        }

        private void BtnADD_Click(object sender, EventArgs e)
        {
            //string strpos = AXIS_0.PositionNowString + "," + AXIS_1.PositionNowString + "," + AXIS_2.PositionNowString;

            if (this.DGVIEW.Rows.Count < m_pos_count)
            {

                //string[] strpos = "0,0,0".Split(',').ToArray();

                string[] strpos = MACHINECollection.GetModulePosition(m_ModuleName).Split(',').ToArray();

                if (strpos.Length >= 3)
                {
                    int index = this.DGVIEW.Rows.Add();
                    this.DGVIEW.Rows[index].Cells[0].Value = strpos[0];
                    this.DGVIEW.Rows[index].Cells[1].Value = strpos[1];
                    this.DGVIEW.Rows[index].Cells[2].Value = strpos[2];
                }
            }


            //this.DGVIEW.Rows[index].Cells[0].Value = AXIS_0.PositionNowString;
            //this.DGVIEW.Rows[index].Cells[1].Value = AXIS_1.PositionNowString;
            //this.DGVIEW.Rows[index].Cells[2].Value = AXIS_2.PositionNowString;
        }


        void FillDisplay()
        {
            DGVIEW.Rows.Clear();
            List<string> listpos = m_pos.Split(';').ToList();
            if (listpos.Count > 0)
            {
                foreach (string str in listpos)
                {
                    List<string> listpostemp = str.Split(',').ToList();
                    if (listpostemp.Count == 3)
                    {
                        if (string.IsNullOrEmpty(listpostemp[0]) || string.IsNullOrEmpty(listpostemp[1]) || string.IsNullOrEmpty(listpostemp[2]))
                        {

                        }
                        else
                        {
                            if (this.DGVIEW.Rows.Count < m_pos_count)
                            {
                                int index = this.DGVIEW.Rows.Add();
                                this.DGVIEW.Rows[index].Cells[0].Value = listpostemp[0];
                                this.DGVIEW.Rows[index].Cells[1].Value = listpostemp[1];
                                this.DGVIEW.Rows[index].Cells[2].Value = listpostemp[2];
                            }
                        }
                    }
                }
            }
        }

        
    }
}
