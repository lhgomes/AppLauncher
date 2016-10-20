using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AppLauncher
{
    public partial class Update : Form
    {
        public Update()
        {
            InitializeComponent();
        }

        public void UpdateVersion(string SourcePath, string DestinationPath)
        {
            pgbDirectories.Value = 0;
            pgbDirectories.Style = ProgressBarStyle.Marquee;
            lblDirectories.Text = "Searching source directories";
            lblDirectories.Refresh();

            var directories = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories);

            pgbDirectories.Style = ProgressBarStyle.Blocks;
            pgbDirectories.Maximum = directories.Length;
            for (int i = 1; i <= directories.Length; i++)
            {
                pgbDirectories.Value = i;
                lblDirectories.Text = string.Format("Creating directory: {0}/{1}", i, directories.Length);
                lblDirectories.Refresh();
                Directory.CreateDirectory(directories[i-1].Replace(SourcePath, DestinationPath));
            }

            pgbFiles.Value = 0;
            pgbFiles.Style = ProgressBarStyle.Marquee;
            lblFiles.Text = "Searching source files";
            lblFiles.Refresh();

            var files = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);

            pgbFiles.Style = ProgressBarStyle.Blocks;
            pgbFiles.Maximum = files.Length;
            for (int i = 1; i <= files.Length; i++)
            {
                pgbFiles.Value = i;
                lblFiles.Text = string.Format("Copying file: {0}/{1}", i, files.Length);
                lblFiles.Refresh();
                File.Copy(files[i - 1], files[i - 1].Replace(SourcePath, DestinationPath), true);
            }

            this.Close();
        }
    }
}
