
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Z = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.U = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.theta_y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.theta_z = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTitleName = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStepRun
            // 
            this.btnStepRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnStepRun.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnStepRun.Location = new System.Drawing.Point(341, 19);
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
            this.btnCancel.Location = new System.Drawing.Point(448, 19);
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
            this.btnOpenMotorsTool.Location = new System.Drawing.Point(27, 19);
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
            this.btnContinueRun.Location = new System.Drawing.Point(234, 19);
            this.btnContinueRun.Margin = new System.Windows.Forms.Padding(4);
            this.btnContinueRun.Name = "btnContinueRun";
            this.btnContinueRun.Size = new System.Drawing.Size(99, 39);
            this.btnContinueRun.TabIndex = 10;
            this.btnContinueRun.Text = "連續執行";
            this.btnContinueRun.UseVisualStyleBackColor = false;
            this.btnContinueRun.Click += new System.EventHandler(this.btnFreeRun_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column0,
            this.X,
            this.Y,
            this.Z,
            this.U,
            this.theta_y,
            this.theta_z,
            this.Column2});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(4, 35);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(826, 319);
            this.dataGridView1.TabIndex = 12;
            // 
            // Column0
            // 
            this.Column0.HeaderText = "項目";
            this.Column0.MinimumWidth = 6;
            this.Column0.Name = "Column0";
            this.Column0.Width = 66;
            // 
            // X
            // 
            this.X.HeaderText = "X";
            this.X.MinimumWidth = 6;
            this.X.Name = "X";
            this.X.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.X.Width = 23;
            // 
            // Y
            // 
            this.Y.HeaderText = "Y";
            this.Y.MinimumWidth = 6;
            this.Y.Name = "Y";
            this.Y.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Y.Width = 23;
            // 
            // Z
            // 
            this.Z.HeaderText = "Z";
            this.Z.MinimumWidth = 6;
            this.Z.Name = "Z";
            this.Z.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Z.Width = 22;
            // 
            // U
            // 
            this.U.HeaderText = "U";
            this.U.MinimumWidth = 6;
            this.U.Name = "U";
            this.U.Width = 46;
            // 
            // theta_y
            // 
            this.theta_y.HeaderText = "θ_y";
            this.theta_y.MinimumWidth = 6;
            this.theta_y.Name = "theta_y";
            this.theta_y.Width = 65;
            // 
            // theta_z
            // 
            this.theta_z.HeaderText = "θ_z";
            this.theta_z.MinimumWidth = 6;
            this.theta_z.Name = "theta_z";
            this.theta_z.Width = 64;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "備註";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.Width = 66;
            // 
            // lblTitleName
            // 
            this.lblTitleName.BackColor = System.Drawing.Color.Black;
            this.lblTitleName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitleName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitleName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitleName.ForeColor = System.Drawing.Color.White;
            this.lblTitleName.Location = new System.Drawing.Point(4, 0);
            this.lblTitleName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitleName.Name = "lblTitleName";
            this.lblTitleName.Size = new System.Drawing.Size(826, 31);
            this.lblTitleName.TabIndex = 15;
            this.lblTitleName.Text = "Module Name";
            this.lblTitleName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblTitleName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(834, 442);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOpenMotorsTool);
            this.panel1.Controls.Add(this.btnContinueRun);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnStepRun);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 361);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(828, 78);
            this.panel1.TabIndex = 17;
            // 
            // FormCompensationStepTracer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 458);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormCompensationStepTracer";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Text = "單步調試";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStepRun;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenMotorsTool;
        private System.Windows.Forms.Button btnContinueRun;
        private System.Windows.Forms.DataGridView dataGridView1;
        public System.Windows.Forms.Label lblTitleName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column0;
        private System.Windows.Forms.DataGridViewTextBoxColumn X;
        private System.Windows.Forms.DataGridViewTextBoxColumn Y;
        private System.Windows.Forms.DataGridViewTextBoxColumn Z;
        private System.Windows.Forms.DataGridViewTextBoxColumn U;
        private System.Windows.Forms.DataGridViewTextBoxColumn theta_y;
        private System.Windows.Forms.DataGridViewTextBoxColumn theta_z;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
    }
}