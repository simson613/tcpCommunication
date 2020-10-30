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
            this.pbrTotal = new System.Windows.Forms.ProgressBar();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.pbrNow = new System.Windows.Forms.ProgressBar();
            this.lblCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pbrTotal
            // 
            this.pbrTotal.Location = new System.Drawing.Point(33, 101);
            this.pbrTotal.Name = "pbrTotal";
            this.pbrTotal.Size = new System.Drawing.Size(253, 27);
            this.pbrTotal.TabIndex = 0;
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(55, 25);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(62, 12);
            this.lblSpeed.TabIndex = 1;
            this.lblSpeed.Text = "AvgSpeed";
            this.lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbrNow
            // 
            this.pbrNow.Location = new System.Drawing.Point(33, 68);
            this.pbrNow.Name = "pbrNow";
            this.pbrNow.Size = new System.Drawing.Size(253, 27);
            this.pbrNow.TabIndex = 2;
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(208, 25);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(38, 12);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "Count";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AutoUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 150);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.pbrNow);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.pbrTotal);
            this.Name = "AutoUpdate";
            this.Text = "AiHelper_LiveM Auto Update";
            this.Shown += new System.EventHandler(this.AutoUpdate_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbrTotal;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.ProgressBar pbrNow;
        private System.Windows.Forms.Label lblCount;
    }
}

