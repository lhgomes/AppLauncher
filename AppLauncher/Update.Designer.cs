namespace AppLauncher
{
    partial class Update
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
            this.lblDirectories = new System.Windows.Forms.Label();
            this.pgbDirectories = new System.Windows.Forms.ProgressBar();
            this.pgbFiles = new System.Windows.Forms.ProgressBar();
            this.lblFiles = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblDirectories
            // 
            this.lblDirectories.AutoSize = true;
            this.lblDirectories.Location = new System.Drawing.Point(13, 13);
            this.lblDirectories.Name = "lblDirectories";
            this.lblDirectories.Size = new System.Drawing.Size(57, 13);
            this.lblDirectories.TabIndex = 0;
            this.lblDirectories.Text = "Directories";
            // 
            // pgbDirectories
            // 
            this.pgbDirectories.Location = new System.Drawing.Point(13, 30);
            this.pgbDirectories.Name = "pgbDirectories";
            this.pgbDirectories.Size = new System.Drawing.Size(419, 23);
            this.pgbDirectories.TabIndex = 1;
            // 
            // pgbFiles
            // 
            this.pgbFiles.Location = new System.Drawing.Point(13, 93);
            this.pgbFiles.Name = "pgbFiles";
            this.pgbFiles.Size = new System.Drawing.Size(419, 23);
            this.pgbFiles.TabIndex = 3;
            // 
            // lblFiles
            // 
            this.lblFiles.AutoSize = true;
            this.lblFiles.Location = new System.Drawing.Point(13, 76);
            this.lblFiles.Name = "lblFiles";
            this.lblFiles.Size = new System.Drawing.Size(28, 13);
            this.lblFiles.TabIndex = 2;
            this.lblFiles.Text = "Files";
            // 
            // Update
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 137);
            this.ControlBox = false;
            this.Controls.Add(this.pgbFiles);
            this.Controls.Add(this.lblFiles);
            this.Controls.Add(this.pgbDirectories);
            this.Controls.Add(this.lblDirectories);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Update";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Version Update";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDirectories;
        private System.Windows.Forms.ProgressBar pgbDirectories;
        private System.Windows.Forms.ProgressBar pgbFiles;
        private System.Windows.Forms.Label lblFiles;
    }
}

