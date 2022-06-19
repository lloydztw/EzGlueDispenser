
namespace Eazy_Project_III.FormSpace
{
    partial class PUTUI
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
            // PUTUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mirrorUI1);
            this.Name = "PUTUI";
            this.Size = new System.Drawing.Size(136, 156);
            this.ResumeLayout(false);

        }

        #endregion

        private MirrorUI mirrorUI1;
    }
}
