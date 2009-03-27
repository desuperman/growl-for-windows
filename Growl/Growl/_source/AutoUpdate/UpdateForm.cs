using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.AutoUpdate
{
    public partial class UpdateForm : Form
    {
        private Updater updater;

        public UpdateForm()
        {
            InitializeComponent();

            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        public UpdateForm(Updater updater)
            : this()
        {
            this.updater = updater;
            this.updater.DownloadProgressChanged += new ProgressChangedEventHandler(updater_DownloadProgressChanged);
            this.updater.DownloadComplete += new EventHandler(updater_DownloadComplete);
            this.updater.UpdateError += new UpdateErrorEventHandler(updater_UpdateError);
        }

        public void LaunchUpdater(Manifest manifest, bool updateAvailable, UpdateErrorEventArgs args)
        {
            if (this.updater != null)
            {
                if (args != null)
                {
                    this.updater_UpdateError(this.updater, args);
                }
                else if (updateAvailable)
                {
                    this.NoButton.Visible = true;
                    this.YesButton.Visible = true;
                    this.progressBar1.Visible = false;
                    this.OKButton.Visible = false;
                    this.InfoLabel.Text = String.Format("Version {0} of Growl is available. Would you like to update Growl to the latest version? (Current version: {1})", manifest.Version, this.updater.CurrentVersion);
                }
                else
                {
                    this.NoButton.Visible = false;
                    this.YesButton.Visible = false;
                    this.progressBar1.Visible = false;
                    this.OKButton.Visible = true;
                    this.InfoLabel.Text = String.Format("Growl is up-to-date. Current Version: {0}", this.updater.CurrentVersion);
                }
                this.Show();
                this.Activate();
            }
        }

        void updater_UpdateError(Updater sender, UpdateErrorEventArgs args)
        {
            this.NoButton.Visible = false;
            this.YesButton.Visible = false;
            this.progressBar1.Visible = false;
            this.OKButton.Visible = true;

            this.InfoLabel.Text = args.UserMessage;
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            this.InfoLabel.Text = "Downloading update...";
            this.progressBar1.Value = 0;
            this.progressBar1.Visible = true;
            this.NoButton.Enabled = false;
            this.YesButton.Enabled = false;
            this.updater.Update();
        }

        void updater_DownloadComplete(object sender, EventArgs e)
        {
            this.InfoLabel.Text = "Download complete. Starting installation...";
        }

        void updater_DownloadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}