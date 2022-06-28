
namespace Eazy_Project_III.FormSpace
{
    partial class FormQCLaserMeasurement
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblTitleName = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.rdoMirror0 = new System.Windows.Forms.RadioButton();
            this.rdoMirror1 = new System.Windows.Forms.RadioButton();
            this.btnSnap = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(237, 204);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 39);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(130, 204);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 39);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "寫入";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblTitleName
            // 
            this.lblTitleName.BackColor = System.Drawing.Color.Transparent;
            this.lblTitleName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitleName.ForeColor = System.Drawing.Color.Black;
            this.lblTitleName.Location = new System.Drawing.Point(43, 39);
            this.lblTitleName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitleName.Name = "lblTitleName";
            this.lblTitleName.Size = new System.Drawing.Size(131, 31);
            this.lblTitleName.TabIndex = 15;
            this.lblTitleName.Text = "QC Laser ";
            this.lblTitleName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DecimalPlaces = 3;
            this.numericUpDown1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numericUpDown1.Location = new System.Drawing.Point(160, 43);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(145, 27);
            this.numericUpDown1.TabIndex = 16;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // rdoMirror0
            // 
            this.rdoMirror0.AutoSize = true;
            this.rdoMirror0.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoMirror0.Location = new System.Drawing.Point(139, 92);
            this.rdoMirror0.Name = "rdoMirror0";
            this.rdoMirror0.Size = new System.Drawing.Size(90, 23);
            this.rdoMirror0.TabIndex = 17;
            this.rdoMirror0.TabStop = true;
            this.rdoMirror0.Text = "Mirror 0";
            this.rdoMirror0.UseVisualStyleBackColor = true;
            // 
            // rdoMirror1
            // 
            this.rdoMirror1.AutoSize = true;
            this.rdoMirror1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoMirror1.Location = new System.Drawing.Point(139, 126);
            this.rdoMirror1.Name = "rdoMirror1";
            this.rdoMirror1.Size = new System.Drawing.Size(90, 23);
            this.rdoMirror1.TabIndex = 18;
            this.rdoMirror1.TabStop = true;
            this.rdoMirror1.Text = "Mirror 1";
            this.rdoMirror1.UseVisualStyleBackColor = true;
            // 
            // btnSnap
            // 
            this.btnSnap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnSnap.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSnap.Location = new System.Drawing.Point(323, 41);
            this.btnSnap.Margin = new System.Windows.Forms.Padding(4);
            this.btnSnap.Name = "btnSnap";
            this.btnSnap.Size = new System.Drawing.Size(99, 27);
            this.btnSnap.TabIndex = 19;
            this.btnSnap.Text = "讀取";
            this.btnSnap.UseVisualStyleBackColor = false;
            this.btnSnap.Click += new System.EventHandler(this.btnSnap_Click);
            // 
            // FormQCLaserMeasurement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 331);
            this.ControlBox = false;
            this.Controls.Add(this.btnSnap);
            this.Controls.Add(this.rdoMirror1);
            this.Controls.Add(this.rdoMirror0);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.lblTitleName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormQCLaserMeasurement";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Text = "QC Laser 手動設定";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.Label lblTitleName;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.RadioButton rdoMirror0;
        private System.Windows.Forms.RadioButton rdoMirror1;
        private System.Windows.Forms.Button btnSnap;
    }
}