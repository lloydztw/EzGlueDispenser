
namespace Eazy_Project_III.FormSpace
{
    partial class FormCompensationStepTracer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStepRun = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenMotorsTool = new System.Windows.Forms.Button();
            this.btnContinueRun = new System.Windows.Forms.Button();
            this.vsCompensationRangeUI1 = new Common.VsCompensationRangeUI();
            this.SuspendLayout();
            // 
            // btnStepRun
            // 
            this.btnStepRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnStepRun.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnStepRun.Location = new System.Drawing.Point(351, 242);
            this.btnStepRun.Margin = new System.Windows.Forms.Padding(4);
            this.btnStepRun.Name = "btnStepRun";
            this.btnStepRun.Size = new System.Drawing.Size(99, 39);
            this.btnStepRun.TabIndex = 5;
            this.btnStepRun.Text = "單步";
            this.btnStepRun.UseVisualStyleBackColor = false;
            this.btnStepRun.Click += new System.EventHandler(this.btnStepRun_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(458, 242);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 39);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "中止";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOpenMotorsTool
            // 
            this.btnOpenMotorsTool.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnOpenMotorsTool.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenMotorsTool.Location = new System.Drawing.Point(23, 242);
            this.btnOpenMotorsTool.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenMotorsTool.Name = "btnOpenMotorsTool";
            this.btnOpenMotorsTool.Size = new System.Drawing.Size(99, 39);
            this.btnOpenMotorsTool.TabIndex = 6;
            this.btnOpenMotorsTool.Text = "馬達軸";
            this.btnOpenMotorsTool.UseVisualStyleBackColor = false;
            this.btnOpenMotorsTool.Click += new System.EventHandler(this.btnOpenMotorsTool_Click);
            // 
            // btnContinueRun
            // 
            this.btnContinueRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnContinueRun.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnContinueRun.Location = new System.Drawing.Point(244, 242);
            this.btnContinueRun.Margin = new System.Windows.Forms.Padding(4);
            this.btnContinueRun.Name = "btnContinueRun";
            this.btnContinueRun.Size = new System.Drawing.Size(99, 39);
            this.btnContinueRun.TabIndex = 10;
            this.btnContinueRun.Text = "連續執行";
            this.btnContinueRun.UseVisualStyleBackColor = false;
            this.btnContinueRun.Click += new System.EventHandler(this.btnFreeRun_Click);
            // 
            // vsCompensationRangeUI1
            // 
            this.vsCompensationRangeUI1.BackColor = System.Drawing.SystemColors.Control;
            this.vsCompensationRangeUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.vsCompensationRangeUI1.Location = new System.Drawing.Point(8, 8);
            this.vsCompensationRangeUI1.Margin = new System.Windows.Forms.Padding(4);
            this.vsCompensationRangeUI1.Name = "vsCompensationRangeUI1";
            this.vsCompensationRangeUI1.Size = new System.Drawing.Size(755, 214);
            this.vsCompensationRangeUI1.TabIndex = 11;
            this.vsCompensationRangeUI1.TitleName = "X, Y, Z, U, theta_y, theta_z";
            // 
            // FormCompensationStepTracer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 368);
            this.ControlBox = false;
            this.Controls.Add(this.btnContinueRun);
            this.Controls.Add(this.btnOpenMotorsTool);
            this.Controls.Add(this.btnStepRun);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.vsCompensationRangeUI1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormCompensationStepTracer";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Text = "單步調試";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStepRun;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenMotorsTool;
        private System.Windows.Forms.Button btnContinueRun;
        private Common.VsCompensationRangeUI vsCompensationRangeUI1;
    }
}