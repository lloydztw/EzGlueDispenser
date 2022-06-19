
namespace Eazy_Project_III.FormSpace
{
    partial class frmRecipe
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRecipe));
            this.pnlMiddleRight = new System.Windows.Forms.Panel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.dispUI1 = new JzDisplay.UISpace.DispUI();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnCam0Snap = new System.Windows.Forms.ToolStripButton();
            this.btnCam0Live = new System.Windows.Forms.ToolStripButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dispUI2 = new JzDisplay.UISpace.DispUI();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.btnCam1Sanp = new System.Windows.Forms.ToolStripButton();
            this.btnCam1Live = new System.Windows.Forms.ToolStripButton();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.pnlMiddleRight.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMiddleRight
            // 
            this.pnlMiddleRight.Controls.Add(this.propertyGrid1);
            this.pnlMiddleRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlMiddleRight.Location = new System.Drawing.Point(1093, 0);
            this.pnlMiddleRight.Name = "pnlMiddleRight";
            this.pnlMiddleRight.Size = new System.Drawing.Size(347, 654);
            this.pnlMiddleRight.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(347, 654);
            this.propertyGrid1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(1332, 68);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(105, 50);
            this.button2.TabIndex = 16;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(1221, 68);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 50);
            this.button1.TabIndex = 15;
            this.button1.Text = "確定";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // dispUI1
            // 
            this.dispUI1.Cursor = System.Windows.Forms.Cursors.Default;
            this.dispUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dispUI1.Location = new System.Drawing.Point(3, 28);
            this.dispUI1.Name = "dispUI1";
            this.dispUI1.Size = new System.Drawing.Size(1079, 590);
            this.dispUI1.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tabControl1);
            this.panel3.Controls.Add(this.pnlMiddleRight);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 125);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1440, 654);
            this.panel3.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ItemSize = new System.Drawing.Size(100, 25);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1093, 654);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dispUI1);
            this.tabPage1.Controls.Add(this.toolStrip1);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1085, 621);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "校正相機";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCam0Snap,
            this.btnCam0Live});
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1079, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnCam0Snap
            // 
            this.btnCam0Snap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCam0Snap.Image = ((System.Drawing.Image)(resources.GetObject("btnCam0Snap.Image")));
            this.btnCam0Snap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCam0Snap.Name = "btnCam0Snap";
            this.btnCam0Snap.Size = new System.Drawing.Size(36, 22);
            this.btnCam0Snap.Text = "單張";
            this.btnCam0Snap.Click += new System.EventHandler(this.btnCam0Snap_Click);
            // 
            // btnCam0Live
            // 
            this.btnCam0Live.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCam0Live.Image = ((System.Drawing.Image)(resources.GetObject("btnCam0Live.Image")));
            this.btnCam0Live.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCam0Live.Name = "btnCam0Live";
            this.btnCam0Live.Size = new System.Drawing.Size(60, 22);
            this.btnCam0Live.Text = "實時畫面";
            this.btnCam0Live.Click += new System.EventHandler(this.btnCam0Live_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dispUI2);
            this.tabPage2.Controls.Add(this.toolStrip2);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1085, 621);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "判斷相機";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dispUI2
            // 
            this.dispUI2.Cursor = System.Windows.Forms.Cursors.Default;
            this.dispUI2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dispUI2.Location = new System.Drawing.Point(3, 28);
            this.dispUI2.Name = "dispUI2";
            this.dispUI2.Size = new System.Drawing.Size(1079, 590);
            this.dispUI2.TabIndex = 2;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCam1Sanp,
            this.btnCam1Live});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1079, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // btnCam1Sanp
            // 
            this.btnCam1Sanp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCam1Sanp.Image = ((System.Drawing.Image)(resources.GetObject("btnCam1Sanp.Image")));
            this.btnCam1Sanp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCam1Sanp.Name = "btnCam1Sanp";
            this.btnCam1Sanp.Size = new System.Drawing.Size(36, 22);
            this.btnCam1Sanp.Text = "单张";
            this.btnCam1Sanp.Click += new System.EventHandler(this.btnCam1Sanp_Click);
            // 
            // btnCam1Live
            // 
            this.btnCam1Live.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCam1Live.Image = ((System.Drawing.Image)(resources.GetObject("btnCam1Live.Image")));
            this.btnCam1Live.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCam1Live.Name = "btnCam1Live";
            this.btnCam1Live.Size = new System.Drawing.Size(36, 22);
            this.btnCam1Live.Text = "实时";
            this.btnCam1Live.Click += new System.EventHandler(this.btnCam1Live_Click);
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.button3);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1440, 125);
            this.pnlTop.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Location = new System.Drawing.Point(12, 39);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(105, 50);
            this.button3.TabIndex = 16;
            this.button3.Text = "取得校正图像";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.button1);
            this.pnlBottom.Controls.Add(this.button2);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 779);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1440, 121);
            this.pnlBottom.TabIndex = 2;
            // 
            // frmRecipe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 900);
            this.ControlBox = false;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmRecipe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmRecipe";
            this.pnlMiddleRight.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMiddleRight;
        private JzDisplay.UISpace.DispUI dispUI1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnCam0Snap;
        private System.Windows.Forms.ToolStripButton btnCam0Live;
        private System.Windows.Forms.TabPage tabPage2;
        private JzDisplay.UISpace.DispUI dispUI2;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton btnCam1Sanp;
        private System.Windows.Forms.ToolStripButton btnCam1Live;
    }
}