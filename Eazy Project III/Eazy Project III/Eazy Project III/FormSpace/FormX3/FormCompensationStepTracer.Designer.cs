
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenMotorsTool = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(455, 151);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 39);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(562, 151);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 39);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOpenMotorsTool
            // 
            this.btnOpenMotorsTool.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnOpenMotorsTool.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenMotorsTool.Location = new System.Drawing.Point(31, 151);
            this.btnOpenMotorsTool.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenMotorsTool.Name = "btnOpenMotorsTool";
            this.btnOpenMotorsTool.Size = new System.Drawing.Size(99, 39);
            this.btnOpenMotorsTool.TabIndex = 6;
            this.btnOpenMotorsTool.Text = "馬達軸";
            this.btnOpenMotorsTool.UseVisualStyleBackColor = false;
            this.btnOpenMotorsTool.Click += new System.EventHandler(this.btnOpenMotorsTool_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.richTextBox1.Location = new System.Drawing.Point(8, 8);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(673, 136);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // FormCompensationDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 202);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnOpenMotorsTool);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormCompensationDebugger";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Text = "FormCompensationDebugger";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenMotorsTool;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}