namespace UpdateSSRDetailUtil
{
    partial class UpdateSSRUtil
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
            this.btnStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblHAP = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblProcessedCount = new System.Windows.Forms.Label();
            this.txtProcessedPNR = new System.Windows.Forms.TextBox();
            this.txtExceptionPNR = new System.Windows.Forms.TextBox();
            this.lblProcessedPNR = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(15, 44);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "HAP: ";
            // 
            // lblHAP
            // 
            this.lblHAP.AutoSize = true;
            this.lblHAP.Location = new System.Drawing.Point(55, 9);
            this.lblHAP.Name = "lblHAP";
            this.lblHAP.Size = new System.Drawing.Size(35, 13);
            this.lblHAP.TabIndex = 2;
            this.lblHAP.Text = "label2";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 111);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "lblStatus";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(113, 44);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 73);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(414, 23);
            this.progressBar1.TabIndex = 5;
            // 
            // lblProcessedCount
            // 
            this.lblProcessedCount.AutoSize = true;
            this.lblProcessedCount.Location = new System.Drawing.Point(12, 134);
            this.lblProcessedCount.Name = "lblProcessedCount";
            this.lblProcessedCount.Size = new System.Drawing.Size(95, 13);
            this.lblProcessedCount.TabIndex = 6;
            this.lblProcessedCount.Text = "lblProcessedCount";
            // 
            // txtProcessedPNR
            // 
            this.txtProcessedPNR.Location = new System.Drawing.Point(12, 175);
            this.txtProcessedPNR.Multiline = true;
            this.txtProcessedPNR.Name = "txtProcessedPNR";
            this.txtProcessedPNR.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtProcessedPNR.Size = new System.Drawing.Size(190, 295);
            this.txtProcessedPNR.TabIndex = 7;
            // 
            // txtExceptionPNR
            // 
            this.txtExceptionPNR.Location = new System.Drawing.Point(236, 175);
            this.txtExceptionPNR.Multiline = true;
            this.txtExceptionPNR.Name = "txtExceptionPNR";
            this.txtExceptionPNR.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExceptionPNR.Size = new System.Drawing.Size(190, 295);
            this.txtExceptionPNR.TabIndex = 8;
            // 
            // lblProcessedPNR
            // 
            this.lblProcessedPNR.AutoSize = true;
            this.lblProcessedPNR.Location = new System.Drawing.Point(12, 159);
            this.lblProcessedPNR.Name = "lblProcessedPNR";
            this.lblProcessedPNR.Size = new System.Drawing.Size(96, 13);
            this.lblProcessedPNR.TabIndex = 9;
            this.lblProcessedPNR.Text = "Prcessed PNR List";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(237, 159);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Exception PNR List";
            // 
            // UpdateSSRUtil
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 482);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblProcessedPNR);
            this.Controls.Add(this.txtExceptionPNR);
            this.Controls.Add(this.txtProcessedPNR);
            this.Controls.Add(this.lblProcessedCount);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblHAP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStart);
            this.Name = "UpdateSSRUtil";
            this.Text = "UpdateSSRUtil 3.5";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateSSRUtil_FormClosing);
            this.Load += new System.EventHandler(this.UpdateSSRUtil_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblHAP;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProcessedCount;
        private System.Windows.Forms.TextBox txtProcessedPNR;
        private System.Windows.Forms.TextBox txtExceptionPNR;
        private System.Windows.Forms.Label lblProcessedPNR;
        private System.Windows.Forms.Label label3;
    }
}

