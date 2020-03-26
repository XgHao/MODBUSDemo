namespace MODBUSDemo
{
    partial class FrmModBus
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
            this.btn_Connect = new System.Windows.Forms.Button();
            this.btn_ReadReg = new System.Windows.Forms.Button();
            this.lb_Mesage = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_Connect
            // 
            this.btn_Connect.Location = new System.Drawing.Point(144, 83);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(92, 45);
            this.btn_Connect.TabIndex = 0;
            this.btn_Connect.Text = "连接串口";
            this.btn_Connect.UseVisualStyleBackColor = true;
            this.btn_Connect.Click += new System.EventHandler(this.Btn_Connect_Click);
            // 
            // btn_ReadReg
            // 
            this.btn_ReadReg.Location = new System.Drawing.Point(348, 83);
            this.btn_ReadReg.Name = "btn_ReadReg";
            this.btn_ReadReg.Size = new System.Drawing.Size(95, 46);
            this.btn_ReadReg.TabIndex = 1;
            this.btn_ReadReg.Text = "读取寄存器";
            this.btn_ReadReg.UseVisualStyleBackColor = true;
            this.btn_ReadReg.Click += new System.EventHandler(this.Btn_ReadReg_Click);
            // 
            // lb_Mesage
            // 
            this.lb_Mesage.FormattingEnabled = true;
            this.lb_Mesage.ItemHeight = 12;
            this.lb_Mesage.Location = new System.Drawing.Point(89, 175);
            this.lb_Mesage.Name = "lb_Mesage";
            this.lb_Mesage.Size = new System.Drawing.Size(455, 304);
            this.lb_Mesage.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(144, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // FrmModBus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 510);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lb_Mesage);
            this.Controls.Add(this.btn_ReadReg);
            this.Controls.Add(this.btn_Connect);
            this.Name = "FrmModBus";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Connect;
        private System.Windows.Forms.Button btn_ReadReg;
        private System.Windows.Forms.ListBox lb_Mesage;
        private System.Windows.Forms.Label label1;
    }
}

