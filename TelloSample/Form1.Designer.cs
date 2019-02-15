namespace TelloSample
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.txtCmd = new System.Windows.Forms.TextBox();
            this.btnCmd = new System.Windows.Forms.Button();
            this.txtRet = new System.Windows.Forms.TextBox();
            this.txtSts = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(101, 45);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "開始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtCmd
            // 
            this.txtCmd.Location = new System.Drawing.Point(12, 74);
            this.txtCmd.Name = "txtCmd";
            this.txtCmd.Size = new System.Drawing.Size(202, 19);
            this.txtCmd.TabIndex = 1;
            // 
            // btnCmd
            // 
            this.btnCmd.Location = new System.Drawing.Point(226, 69);
            this.btnCmd.Name = "btnCmd";
            this.btnCmd.Size = new System.Drawing.Size(46, 28);
            this.btnCmd.TabIndex = 2;
            this.btnCmd.Text = "送信";
            this.btnCmd.UseVisualStyleBackColor = true;
            this.btnCmd.Click += new System.EventHandler(this.btnCmd_Click);
            // 
            // txtRet
            // 
            this.txtRet.Location = new System.Drawing.Point(12, 99);
            this.txtRet.Name = "txtRet";
            this.txtRet.Size = new System.Drawing.Size(260, 19);
            this.txtRet.TabIndex = 3;
            // 
            // txtSts
            // 
            this.txtSts.Location = new System.Drawing.Point(12, 124);
            this.txtSts.Multiline = true;
            this.txtSts.Name = "txtSts";
            this.txtSts.Size = new System.Drawing.Size(202, 348);
            this.txtSts.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 481);
            this.Controls.Add(this.txtSts);
            this.Controls.Add(this.txtRet);
            this.Controls.Add(this.btnCmd);
            this.Controls.Add(this.txtCmd);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtCmd;
        private System.Windows.Forms.Button btnCmd;
        private System.Windows.Forms.TextBox txtRet;
        private System.Windows.Forms.TextBox txtSts;
    }
}

