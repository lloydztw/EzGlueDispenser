
namespace ZxCore3.Gui
{
    partial class GdxDispUI
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GdxDispUI));
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.chkAutoZoom = new System.Windows.Forms.CheckBox();
            this.lblCoordInfo = new System.Windows.Forms.Label();
            this.qxImageViewer1 = new JetEazy.GUI.QxImageViewer();
            this.panelTitleBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.Black;
            this.panelTitleBar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelTitleBar.BackgroundImage")));
            this.panelTitleBar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelTitleBar.Controls.Add(this.chkAutoZoom);
            this.panelTitleBar.Controls.Add(this.lblCoordInfo);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Padding = new System.Windows.Forms.Padding(3);
            this.panelTitleBar.Size = new System.Drawing.Size(723, 38);
            this.panelTitleBar.TabIndex = 0;
            // 
            // chkAutoZoom
            // 
            this.chkAutoZoom.AutoSize = true;
            this.chkAutoZoom.BackColor = System.Drawing.Color.Transparent;
            this.chkAutoZoom.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.chkAutoZoom.ForeColor = System.Drawing.Color.White;
            this.chkAutoZoom.Location = new System.Drawing.Point(603, 7);
            this.chkAutoZoom.Name = "chkAutoZoom";
            this.chkAutoZoom.Padding = new System.Windows.Forms.Padding(0, 2, 6, 0);
            this.chkAutoZoom.Size = new System.Drawing.Size(120, 25);
            this.chkAutoZoom.TabIndex = 22;
            this.chkAutoZoom.Text = "Auto Zoom";
            this.chkAutoZoom.UseVisualStyleBackColor = false;
            this.chkAutoZoom.Visible = false;
            this.chkAutoZoom.CheckedChanged += new System.EventHandler(this.chkAutoZoom_CheckedChanged);
            // 
            // lblCoordInfo
            // 
            this.lblCoordInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblCoordInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCoordInfo.ForeColor = System.Drawing.Color.White;
            this.lblCoordInfo.Location = new System.Drawing.Point(3, 3);
            this.lblCoordInfo.Name = "lblCoordInfo";
            this.lblCoordInfo.Padding = new System.Windows.Forms.Padding(10, 8, 8, 8);
            this.lblCoordInfo.Size = new System.Drawing.Size(717, 32);
            this.lblCoordInfo.TabIndex = 0;
            this.lblCoordInfo.Text = "Info";
            this.lblCoordInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // qxImageViewer1
            // 
            this.qxImageViewer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.qxImageViewer1.ClearLiveBackgroundEnabled = true;
            this.qxImageViewer1.CrosshairsColor = System.Drawing.Color.Gold;
            this.qxImageViewer1.CrosshairsVisible = false;
            this.qxImageViewer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.qxImageViewer1.GridLinesVisible = false;
            this.qxImageViewer1.Image = null;
            this.qxImageViewer1.Location = new System.Drawing.Point(0, 5);
            this.qxImageViewer1.Margin = new System.Windows.Forms.Padding(5);
            this.qxImageViewer1.Name = "qxImageViewer1";
            this.qxImageViewer1.RulerVisible = false;
            this.qxImageViewer1.Size = new System.Drawing.Size(723, 512);
            this.qxImageViewer1.TabIndex = 1;
            // 
            // GdxDispUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.panelTitleBar);
            this.Controls.Add(this.qxImageViewer1);
            this.Name = "GdxDispUI";
            this.Size = new System.Drawing.Size(723, 517);
            this.panelTitleBar.ResumeLayout(false);
            this.panelTitleBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Label lblCoordInfo;
        private JetEazy.GUI.QxImageViewer qxImageViewer1;
        private System.Windows.Forms.CheckBox chkAutoZoom;
    }
}
