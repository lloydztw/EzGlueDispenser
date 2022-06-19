
namespace Eazy_Project_III.FormSpace
{
    partial class PICKUI
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
            this.mirrorUI1 = new Eazy_Project_III.FormSpace.MirrorUI();
            this.mirrorUI2 = new Eazy_Project_III.FormSpace.MirrorUI();
            this.mirrorUI3 = new Eazy_Project_III.FormSpace.MirrorUI();
            this.mirrorUI4 = new Eazy_Project_III.FormSpace.MirrorUI();
            this.SuspendLayout();
            // 
            // mirrorUI1
            // 
            this.mirrorUI1.BackColor = System.Drawing.Color.SkyBlue;
            this.mirrorUI1.Location = new System.Drawing.Point(3, 3);
            this.mirrorUI1.Name = "mirrorUI1";
            this.mirrorUI1.Size = new System.Drawing.Size(130, 150);
            this.mirrorUI1.TabIndex = 0;
            // 
            // mirrorUI2
            // 
            this.mirrorUI2.BackColor = System.Drawing.Color.SkyBlue;
            this.mirrorUI2.Location = new System.Drawing.Point(139, 3);
            this.mirrorUI2.Name = "mirrorUI2";
            this.mirrorUI2.Size = new System.Drawing.Size(130, 150);
            this.mirrorUI2.TabIndex = 1;
            // 
            // mirrorUI3
            // 
            this.mirrorUI3.BackColor = System.Drawing.Color.SkyBlue;
            this.mirrorUI3.Location = new System.Drawing.Point(3, 159);
            this.mirrorUI3.Name = "mirrorUI3";
            this.mirrorUI3.Size = new System.Drawing.Size(130, 150);
            this.mirrorUI3.TabIndex = 2;
            // 
            // mirrorUI4
            // 
            this.mirrorUI4.BackColor = System.Drawing.Color.SkyBlue;
            this.mirrorUI4.Location = new System.Drawing.Point(139, 159);
            this.mirrorUI4.Name = "mirrorUI4";
            this.mirrorUI4.Size = new System.Drawing.Size(130, 150);
            this.mirrorUI4.TabIndex = 3;
            // 
            // PICKUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mirrorUI4);
            this.Controls.Add(this.mirrorUI3);
            this.Controls.Add(this.mirrorUI2);
            this.Controls.Add(this.mirrorUI1);
            this.Name = "PICKUI";
            this.Size = new System.Drawing.Size(272, 312);
            this.ResumeLayout(false);

        }

        #endregion

        public MirrorUI mirrorUI1;
        public MirrorUI mirrorUI2;
        public MirrorUI mirrorUI3;
        public MirrorUI mirrorUI4;
    }
}
