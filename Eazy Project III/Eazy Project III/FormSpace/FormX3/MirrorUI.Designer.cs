
namespace Eazy_Project_III.FormSpace
{
    partial class MirrorUI
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblMirror1 = new System.Windows.Forms.Label();
            this.lblMirror0 = new System.Windows.Forms.Label();
            this.lblGrp = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblMirror1
            // 
            this.lblMirror1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMirror1.Location = new System.Drawing.Point(70, 39);
            this.lblMirror1.Name = "lblMirror1";
            this.lblMirror1.Size = new System.Drawing.Size(50, 101);
            this.lblMirror1.TabIndex = 6;
            this.lblMirror1.Text = "Mirror1\r\n(右)";
            this.lblMirror1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMirror0
            // 
            this.lblMirror0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMirror0.Location = new System.Drawing.Point(10, 39);
            this.lblMirror0.Name = "lblMirror0";
            this.lblMirror0.Size = new System.Drawing.Size(50, 101);
            this.lblMirror0.TabIndex = 5;
            this.lblMirror0.Text = "Mirror0\r\n(左)";
            this.lblMirror0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblGrp
            // 
            this.lblGrp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblGrp.Location = new System.Drawing.Point(10, 10);
            this.lblGrp.Name = "lblGrp";
            this.lblGrp.Size = new System.Drawing.Size(110, 29);
            this.lblGrp.TabIndex = 7;
            this.lblGrp.Text = "组0";
            this.lblGrp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MirrorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SkyBlue;
            this.Controls.Add(this.lblGrp);
            this.Controls.Add(this.lblMirror1);
            this.Controls.Add(this.lblMirror0);
            this.Name = "MirrorUI";
            this.Size = new System.Drawing.Size(130, 150);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblMirror1;
        public System.Windows.Forms.Label lblMirror0;
        public System.Windows.Forms.Label lblGrp;
    }
}
