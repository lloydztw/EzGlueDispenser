
namespace Eazy_Project_III
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.essUI1 = new JetEazy.UISpace.EssUI();
            this.rcpUI1 = new PhotoMachine.UISpace.RcpUI();
            this.iniUI1 = new PhotoMachine.UISpace.IniUI();
            this.ctrlUI1 = new PhotoMachine.UISpace.CtrlUI();
            this.runUI1 = new PhotoMachine.UISpace.RunUI();
            this.mainControlUI1 = new Eazy_Project_III.UISpace.MainControlUI();
            this.SuspendLayout();
            // 
            // essUI1
            // 
            this.essUI1.BackColor = System.Drawing.SystemColors.Control;
            this.essUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.essUI1.Location = new System.Drawing.Point(1212, 0);
            this.essUI1.Name = "essUI1";
            this.essUI1.Size = new System.Drawing.Size(226, 238);
            this.essUI1.TabIndex = 0;
            // 
            // rcpUI1
            // 
            this.rcpUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rcpUI1.Location = new System.Drawing.Point(983, 237);
            this.rcpUI1.Name = "rcpUI1";
            this.rcpUI1.Size = new System.Drawing.Size(228, 417);
            this.rcpUI1.TabIndex = 3;
            // 
            // iniUI1
            // 
            this.iniUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iniUI1.Location = new System.Drawing.Point(749, 237);
            this.iniUI1.Name = "iniUI1";
            this.iniUI1.Size = new System.Drawing.Size(228, 417);
            this.iniUI1.TabIndex = 4;
            // 
            // ctrlUI1
            // 
            this.ctrlUI1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ctrlUI1.Location = new System.Drawing.Point(1212, 660);
            this.ctrlUI1.Name = "ctrlUI1";
            this.ctrlUI1.Size = new System.Drawing.Size(223, 239);
            this.ctrlUI1.TabIndex = 2;
            // 
            // runUI1
            // 
            this.runUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.runUI1.Location = new System.Drawing.Point(1212, 237);
            this.runUI1.Name = "runUI1";
            this.runUI1.Size = new System.Drawing.Size(228, 417);
            this.runUI1.TabIndex = 1;
            // 
            // mainControlUI1
            // 
            this.mainControlUI1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.mainControlUI1.Location = new System.Drawing.Point(0, 0);
            this.mainControlUI1.Name = "mainControlUI1";
            this.mainControlUI1.Size = new System.Drawing.Size(1211, 900);
            this.mainControlUI1.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 900);
            this.Controls.Add(this.rcpUI1);
            this.Controls.Add(this.iniUI1);
            this.Controls.Add(this.ctrlUI1);
            this.Controls.Add(this.runUI1);
            this.Controls.Add(this.essUI1);
            this.Controls.Add(this.mainControlUI1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private JetEazy.UISpace.EssUI essUI1;
        private PhotoMachine.UISpace.RunUI runUI1;
        private PhotoMachine.UISpace.CtrlUI ctrlUI1;
        private PhotoMachine.UISpace.RcpUI rcpUI1;
        private PhotoMachine.UISpace.IniUI iniUI1;
        private UISpace.MainControlUI mainControlUI1;
    }
}

