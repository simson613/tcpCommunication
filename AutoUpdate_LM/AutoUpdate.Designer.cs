namespace AutoUpdate_LM
{
    partial class AutoUpdate
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbrFileUpdate = new System.Windows.Forms.ProgressBar();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pbrFileUpdate
            // 
            this.pbrFileUpdate.Location = new System.Drawing.Point(32, 54);
            this.pbrFileUpdate.Name = "pbrFileUpdate";
            this.pbrFileUpdate.Size = new System.Drawing.Size(253, 27);
            this.pbrFileUpdate.TabIndex = 0;
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoSize = true;
            this.lblUpdate.Location = new System.Drawing.Point(126, 25);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(66, 12);
            this.lblUpdate.TabIndex = 1;
            this.lblUpdate.Text = "Updating...";
            // 
            // AutoUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 102);
            this.Controls.Add(this.lblUpdate);
            this.Controls.Add(this.pbrFileUpdate);
            this.Name = "AutoUpdate";
            this.Text = "AiHelper_LiveM Auto Update";
            this.Shown += new System.EventHandler(this.AutoUpdate_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbrFileUpdate;
        private System.Windows.Forms.Label lblUpdate;
    }
}

