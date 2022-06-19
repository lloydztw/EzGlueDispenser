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
using JetEazy.DBSpace;
using Eazy_Project_III;
using Eazy_Project_Interface;
using Eazy_Project_III.OPSpace;
using Eazy_Project_III.FormSpace;

//using Mist.OPSpace;
//using Mist.DBSpace;

namespace PhotoMachine.UISpace
{
    public partial class RcpUI : UserControl
    {
        enum TagEnum
        {
            ADD,
            MODIFY,

            OK,
            CANCEL,

            DETIAL,
        }

        //VIEWClass VIEW;
        RCPDBClass RCPDB;
        RCPItemClass RCPItemNow
        {
            get
            {
                return RCPDB.RCPItemNow;
            }
        }

        GroupBox grpRcpData;

        TextBox txtName;
        TextBox txtVersion;

        //StpUI STPUI;

        Button btnAdd;
        Button btnModify;

        Button btnOK;
        Button btnCancel;

        Button btnDetial;

        Label lblModifyDateTime;

        RichTextBox rtbComment;
        
        //Language Setup
        JzLanguageClass myLanguage = new JzLanguageClass();

        string UIPath = "";
        int LanguageIndex = 0;

        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;


        //IRecipe IxRecipe
        //{
        //    get { return RecipeCHClass.Instance; }
        //}


        public RcpUI()
        {
            InitializeComponent();
        }

        public void Initial(string uipath,
            int langindex,
            VersionEnum ver,
            OptionEnum opt,
            RCPDBClass rcpdb)
            //VIEWClass view)
        {
            RCPDB = rcpdb;
            //VIEW = view;

            UIPath = uipath;
            LanguageIndex = langindex;
            VER = ver;
            OPT = opt;

            myLanguage.Initial(UIPath + "\\RcpUI.jdb", LanguageIndex, this);

            grpRcpData = groupBox1;
            rtbComment = richTextBox1;

            txtName = textBox1;
            txtVersion = textBox2;

            lblModifyDateTime = label4;

            btnAdd = button1;
            btnModify = button2;
            btnOK = button4;
            btnCancel = button6;
            btnDetial = button3;

            btnAdd.Tag = TagEnum.ADD;
            btnModify.Tag = TagEnum.MODIFY;
            btnOK.Tag = TagEnum.OK;
            btnCancel.Tag = TagEnum.CANCEL;
            btnDetial.Tag = TagEnum.DETIAL;


            btnAdd.Click += new EventHandler(btn_Click);
            btnModify.Click += new EventHandler(btn_Click);
            btnOK.Click += new EventHandler(btn_Click);
            btnCancel.Click += new EventHandler(btn_Click);
            btnDetial.Click += new EventHandler(btn_Click);

            //STPUI = stpUI1;
            //STPUI.Initial(UIPath,LanguageIndex,VER,OPT,VIEW);
            //STPUI.TriggerAction += new StpUI.TriggerHandler(STPUI_TriggerAction);
            //STPUI.TriggerActionForSetupDetail += new StpUI.TriggerHandlerForSetupDetail(STPUI_TriggerActionForSetupDetail);

            FillDisplay(true);

            DBStatus = DBStatusEnum.NONE;

        }

        void STPUI_TriggerActionForSetupDetail(RCPStatusEnum status, int setupindex)
        {
            switch (status)
            {
                default:
                    OnTrigger(status, setupindex);
                    break;
            }
        }

        void STPUI_TriggerAction(RCPStatusEnum status)
        {
            switch (status)
            {
                default:
                    OnTrigger(status);
                    break;
            }
        }


        void btn_Click(object sender, EventArgs e)
        {
            TagEnum KEYS = (TagEnum)((Button)sender).Tag;

            switch (KEYS)
            {
                case TagEnum.ADD:
                    AddAndCopy(false);
                    break;
                case TagEnum.MODIFY:
                    Modify();
                    break;
                case TagEnum.OK:
                    ModifyComplete();
                    break;
                case TagEnum.CANCEL:
                    ModifyCancel();
                    break;
                case TagEnum.DETIAL:

                    m_FrmRecipe = new frmRecipe();
                    m_FrmRecipe.ShowDialog();

                    m_FrmRecipe.Dispose();
                    m_FrmRecipe = null;

                    break;
            }
        }


        frmRecipe m_FrmRecipe = null;

        void AddAndCopy(bool IsCopy)
        {
            OnTrigger(RCPStatusEnum.EDIT);

            RCPDB.AddAndCopy(IsCopy);
            FillDisplay(false || !IsCopy);

            DBStatus = DBStatusEnum.ADD;
        }
        void Modify()
        {
            OnTrigger(RCPStatusEnum.EDIT);

            RCPDB.Backup();
            DBStatus = DBStatusEnum.MODIFY;
        }
        void ModifyComplete()
        {
            if (RCPDB.CheckDuplicate(txtName.Text.Trim() + txtVersion.Text.Trim(), RCPItemNow.Index))
            {

                JetEazy.BasicSpace.VsMSG.Instance.Warning("名称或版本已存在，请检查。");

                //MessageBox.Show(myLanguage.Messages("msg1", INI.LANGUAGE), "SYS", MessageBoxButtons.OK);
                txtName.Focus();
            }
            else
            {
                WriteBack(true);

                //VIEW.Save();

                //STPUI.ModifyComplete();

                //STPUI.ResetcboSetup();

                RecipeCHClass.Instance.Save();

                FillDisplay(true);

                OnTrigger(RCPStatusEnum.MODIFYCOMPLETE);
                DBStatus = DBStatusEnum.NONE;
            }
        }
        void ModifyCancel()
        {
            if (DBStatus == DBStatusEnum.ADD)
                RCPDB.DeleteLast();
            else
                RCPDB.Restore();

            //VIEW.Load();
            ////VIEW.Train();

            //STPUI.ModifyCancel();

            RecipeCHClass.Instance.Load();

            FillDisplay(true);

            //STPUI.ResetcboSetup();

            OnTrigger(RCPStatusEnum.MODIFYCANCEL);
            DBStatus = DBStatusEnum.NONE;
        }

        void WriteBack(bool IsWithChange)
        {
            if (IsWithChange)
            {
                RCPItemNow.Name = txtName.Text;
                RCPItemNow.Version = txtVersion.Text;
                RCPItemNow.Comment = rtbComment.Text.Trim();

                RCPItemNow.ModifyDateTime = JzTimes.DateTimeString;
            }

            RCPDB.Save();
        }
        public void ChangeRecipe(bool IsLoad) //IsLoad is judge is the image is from file or memory
        {
            FillDisplay(IsLoad);
        }
        void FillDisplay(bool IsLoad)   //IsLoad is judge is the image is from file or memory
        {
            txtName.Text = RCPItemNow.Name;
            txtVersion.Text = RCPItemNow.Version;

            lblModifyDateTime.Text = RCPItemNow.ToModifyString();

            txtName.ReadOnly = RCPItemNow.Index == 0;
            txtVersion.ReadOnly = RCPItemNow.Index == 0;

            rtbComment.Text = RCPItemNow.Comment;

            //if (IsLoad)
            //    STPUI.ResetcboSetup();

        }
        DBStatusEnum myDBStatus = DBStatusEnum.NONE;
        DBStatusEnum DBStatus
        {
            get
            {
                return myDBStatus;
            }
            set
            {
                myDBStatus = value;

                switch (myDBStatus)
                {
                    case DBStatusEnum.ADD:
                    case DBStatusEnum.MODIFY:

                        grpRcpData.Enabled = true;

                        btnAdd.Visible = false;
                        btnModify.Visible = false;

                        btnOK.Visible = true;
                        btnCancel.Visible = true;

                        break;
                    case DBStatusEnum.NONE:
                        grpRcpData.Enabled = false;

                        btnAdd.Visible = true;
                        btnModify.Visible = true;

                        btnOK.Visible = false;
                        btnCancel.Visible = false;

                        break;
                }
            }
        }

        public delegate void TriggerHandler(RCPStatusEnum status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(RCPStatusEnum status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(status);
            }
        }

        public delegate void TriggerHandlerForSetupDetail(RCPStatusEnum status, int setupindex);
        public event TriggerHandlerForSetupDetail TriggerActionForSetupDetail;
        public void OnTrigger(RCPStatusEnum status, int setupindex)
        {
            if (TriggerActionForSetupDetail != null)
            {
                TriggerActionForSetupDetail(status, setupindex);
            }
        }
    }
}
