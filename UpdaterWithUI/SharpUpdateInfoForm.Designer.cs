namespace UpdaterWithUI
{
    partial class SharpUpdateInfoForm
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
            this.lbVersions = new System.Windows.Forms.Label();
            this.RBDescription = new System.Windows.Forms.RichTextBox();
            this.B_Close = new System.Windows.Forms.Button();
            this.LBDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbVersions
            // 
            this.lbVersions.Location = new System.Drawing.Point(87, 9);
            this.lbVersions.Name = "lbVersions";
            this.lbVersions.Size = new System.Drawing.Size(168, 54);
            this.lbVersions.TabIndex = 0;
            this.lbVersions.Text = "label1";
            this.lbVersions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RBDescription
            // 
            this.RBDescription.BackColor = System.Drawing.SystemColors.Control;
            this.RBDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RBDescription.Cursor = System.Windows.Forms.Cursors.Default;
            this.RBDescription.Location = new System.Drawing.Point(12, 76);
            this.RBDescription.Name = "RBDescription";
            this.RBDescription.ReadOnly = true;
            this.RBDescription.Size = new System.Drawing.Size(250, 126);
            this.RBDescription.TabIndex = 1;
            this.RBDescription.TabStop = false;
            this.RBDescription.Text = "";
            // 
            // B_Close
            // 
            this.B_Close.Location = new System.Drawing.Point(90, 208);
            this.B_Close.Name = "B_Close";
            this.B_Close.Size = new System.Drawing.Size(75, 23);
            this.B_Close.TabIndex = 2;
            this.B_Close.Text = "Close";
            this.B_Close.UseVisualStyleBackColor = true;
            this.B_Close.Click += new System.EventHandler(this.B_Close_Click);
            // 
            // LBDescription
            // 
            this.LBDescription.AutoSize = true;
            this.LBDescription.Location = new System.Drawing.Point(12, 58);
            this.LBDescription.Name = "LBDescription";
            this.LBDescription.Size = new System.Drawing.Size(60, 13);
            this.LBDescription.TabIndex = 3;
            this.LBDescription.Text = "Description";
            // 
            // SharpUpdateInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 245);
            this.Controls.Add(this.LBDescription);
            this.Controls.Add(this.B_Close);
            this.Controls.Add(this.RBDescription);
            this.Controls.Add(this.lbVersions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SharpUpdateInfoForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbVersions;
        private System.Windows.Forms.RichTextBox RBDescription;
        private System.Windows.Forms.Button B_Close;
        private System.Windows.Forms.Label LBDescription;
    }
}