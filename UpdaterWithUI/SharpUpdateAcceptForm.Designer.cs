namespace UpdaterWithUI
{
    partial class SharpUpdateAcceptForm
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
            this.LB_UpdateAvailable = new System.Windows.Forms.Label();
            this.LB_NewVersion = new System.Windows.Forms.Label();
            this.B_Yes = new System.Windows.Forms.Button();
            this.B_No = new System.Windows.Forms.Button();
            this.B_Details = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LB_UpdateAvailable
            // 
            this.LB_UpdateAvailable.Location = new System.Drawing.Point(12, 9);
            this.LB_UpdateAvailable.Name = "LB_UpdateAvailable";
            this.LB_UpdateAvailable.Size = new System.Drawing.Size(237, 41);
            this.LB_UpdateAvailable.TabIndex = 0;
            this.LB_UpdateAvailable.Text = "An update is available!\r\nWould you like to update?";
            this.LB_UpdateAvailable.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LB_NewVersion
            // 
            this.LB_NewVersion.Location = new System.Drawing.Point(82, 43);
            this.LB_NewVersion.Name = "LB_NewVersion";
            this.LB_NewVersion.Size = new System.Drawing.Size(100, 23);
            this.LB_NewVersion.TabIndex = 1;
            this.LB_NewVersion.Text = "ver";
            this.LB_NewVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // B_Yes
            // 
            this.B_Yes.Location = new System.Drawing.Point(12, 76);
            this.B_Yes.Name = "B_Yes";
            this.B_Yes.Size = new System.Drawing.Size(75, 23);
            this.B_Yes.TabIndex = 2;
            this.B_Yes.Text = "Yes";
            this.B_Yes.UseVisualStyleBackColor = true;
            this.B_Yes.Click += new System.EventHandler(this.B_Yes_Click);
            // 
            // B_No
            // 
            this.B_No.Location = new System.Drawing.Point(93, 76);
            this.B_No.Name = "B_No";
            this.B_No.Size = new System.Drawing.Size(75, 23);
            this.B_No.TabIndex = 3;
            this.B_No.Text = "No";
            this.B_No.UseVisualStyleBackColor = true;
            this.B_No.Click += new System.EventHandler(this.B_No_Click);
            // 
            // B_Details
            // 
            this.B_Details.Location = new System.Drawing.Point(174, 76);
            this.B_Details.Name = "B_Details";
            this.B_Details.Size = new System.Drawing.Size(75, 23);
            this.B_Details.TabIndex = 4;
            this.B_Details.Text = "Details";
            this.B_Details.UseVisualStyleBackColor = true;
            this.B_Details.Click += new System.EventHandler(this.B_Details_Click);
            // 
            // SharpUpdateAcceptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 108);
            this.Controls.Add(this.B_Details);
            this.Controls.Add(this.B_No);
            this.Controls.Add(this.B_Yes);
            this.Controls.Add(this.LB_NewVersion);
            this.Controls.Add(this.LB_UpdateAvailable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SharpUpdateAcceptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LB_UpdateAvailable;
        private System.Windows.Forms.Label LB_NewVersion;
        private System.Windows.Forms.Button B_Yes;
        private System.Windows.Forms.Button B_No;
        private System.Windows.Forms.Button B_Details;
    }
}