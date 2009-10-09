using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.UI;


namespace Growl
{
    public partial class MainForm : Form
    {
        private Controller controller;
        private List<Panel> panels = new List<Panel>();
        private ToolTip historyTrackBarToolTip = new ToolTip();
        private Timer historyTrackBarTimer = new Timer();

        private Keys closeLastKeyCombo;
        private Keys closeAllKeyCombo;
        private HotKeyManager closeLastHotKey;
        private HotKeyManager closeAllHotKey;

        public MainForm()
        {
            InitializeComponent();

            // localize text
            this.Text = Properties.Resources.SettingsForm_FormTitle;
            this.labelInitializationStage.Text = Properties.Resources.Loading_Initializing;
            this.toolbarButtonGeneral.Text = Properties.Resources.Toolbar_General;
            this.toolbarButtonApplications.Text = Properties.Resources.Toolbar_Applications;
            this.toolbarButtonDisplays.Text = Properties.Resources.Toolbar_Displays;
            this.toolbarButtonNetwork.Text = Properties.Resources.Toolbar_Network;
            this.toolbarButtonSecurity.Text = Properties.Resources.Toolbar_Security;
            this.toolbarButtonHistory.Text = Properties.Resources.Toolbar_History;
            this.toolbarButtonAbout.Text = Properties.Resources.Toolbar_About;

            this.groupBoxIdleSettings.Text = Properties.Resources.General_IdleSettingsTitle;
            this.radioButtonIdleNever.Text = Properties.Resources.General_IdleSettings_NeverIdle;
            this.groupBoxSoundSettings.Text = Properties.Resources.General_SoundSettingsTitle;
            this.labelDefaultSound.Text = Properties.Resources.General_SoundSettings_SoundLabel;
            this.checkBoxMuteAllSounds.Text = Properties.Resources.General_SoundSettings_MuteLabel;
            this.checkBoxAutoStart.Text = Properties.Resources.General_AutoStart;

            this.labelPrefSound.Text = Properties.Resources.Applications_Preferences_SoundLabel;
            this.labelPrefSticky.Text = Properties.Resources.Applications_Preferences_StickyLabel;
            this.labelPrefPriority.Text = Properties.Resources.Applications_Preferences_PriorityLabel;
            this.labelPrefForward.Text = Properties.Resources.Applications_Preferences_ForwardingLabel;
            this.labelPrefDisplay.Text = Properties.Resources.Applications_Preferences_DisplayLabel;
            this.labelPrefDuration.Text = Properties.Resources.Applications_Preferences_DurationLabel;
            this.labelPrefEnabled.Text = Properties.Resources.Applications_Preferences_EnabledLabel;
            this.labelNoApps.Text = Properties.Resources.Applications_NoAppsRegistered_Title;
            this.labelNoAppsDesc.Text = Properties.Resources.Applications_NoAppsRegistered_Description;
            this.removeApplicationToolStripMenuItem.Text = Properties.Resources.Applications_RemoveApplication;
            this.listControlApplications.HeaderText = Properties.Resources.Applications_ApplicationListHeader;
            this.listControlApplicationNotifications.HeaderText = Properties.Resources.Applications_NotificationListHeader;

            this.buttonPreviewDisplay.Text = Properties.Resources.Button_Preview;
            this.buttonSetAsDefault.Text = Properties.Resources.Button_SetAsDefault;
            this.listControlDisplays.HeaderText = Properties.Resources.Displays_DisplayListHeader;
            this.getDisplaysLabel.Text = Properties.Resources.Displays_FindMore;

            this.labelPasswordManager.Text = Properties.Resources.Security_PasswordManager_Title;
            this.checkBoxAllowSubscriptions.Text = Properties.Resources.Security_AllowSubscriptions;
            this.checkBoxAllowWebNotifications.Text = Properties.Resources.Security_AllowWebNotifications;
            this.checkBoxAllowNetworkNotifications.Text = Properties.Resources.Security_AllowNetworkNotifications;
            this.checkBoxRequireLocalPassword.Text = Properties.Resources.Security_RequirePasswordLocalApps;
            this.editToolStripMenuItem.Text = Properties.Resources.Network_EditComputer;
            this.removeComputerToolStripMenuItem.Text = Properties.Resources.Network_RemoveComputer;

            this.checkBoxEnableSubscriptions.Text = Properties.Resources.Network_SubscribeToNotifications;
            this.checkBoxEnableForwarding.Text = Properties.Resources.Network_ForwardNotifications;

            this.buttonClearHistory.Text = Properties.Resources.Button_Clear;
            this.historyDaysGroupBox.Text = Properties.Resources.History_NumberOfDaysTitle;
            this.historySortByGroupBox.Text = Properties.Resources.History_SortByTitle;
            this.historySortByDateRadioButton.Text = Properties.Resources.History_SortBy_Date;
            this.historySortByApplicationRadioButton.Text = Properties.Resources.History_SortBy_Application;

            this.labelAboutBuildNumber.Text = Properties.Resources.About_BuildNumber;
            this.labelAboutUs.Text = Properties.Resources.About_FindInformationLabel;
            this.labelAboutIcons.Text = Properties.Resources.About_IconInfoLabel;
            this.labelAboutIcons2.Text = Properties.Resources.About_IconInfoLabel2;
            this.labelAboutOriginal.Text = Properties.Resources.About_OriginalLabel;
            this.labelAboutOriginal2.Text = Properties.Resources.About_OriginalLabel2;
            this.labelAboutGrowlVersion.Text = Properties.Resources.About_GrowlVersion;

            // handle the 'consider me idle after X seconds' radio button
            // (since the textbox needs to be placed at the appropriate position within the label)
            string idleAfterText = Properties.Resources.General_IdleSettings_IdleAfter;
            if (idleAfterText.Contains("{0}"))
            {
                string before = idleAfterText.Substring(0, idleAfterText.IndexOf('{'));
                string after = idleAfterText.Substring(idleAfterText.IndexOf('}') + 1);
                this.radioButtonIdleAfter.Text = before;
                this.textBoxIdleAfterSeconds.Location = new Point(this.radioButtonIdleAfter.Bounds.Right - 6, this.radioButtonIdleAfter.Location.Y - 1);
                idleAfterText = String.Format(idleAfterText, "         "); // this leaves space for the textbox
            }
            this.radioButtonIdleAfter.Text = idleAfterText;

            try
            {
                KeysConverter kc = new KeysConverter();
                this.closeLastKeyCombo = (Keys)kc.ConvertFromString(Properties.Settings.Default.KeyboardShortcutCloseLast);
                this.closeAllKeyCombo = (Keys)kc.ConvertFromString(Properties.Settings.Default.KeyboardShortcutCloseAll);

                this.closeLastHotKey = new HotKeyManager(this, this.closeLastKeyCombo);
                this.closeAllHotKey = new HotKeyManager(this, this.closeAllKeyCombo);

                this.closeLastHotKey.Register();
                this.closeAllHotKey.Register();
            }
            catch
            {
            }
        }

        # region visual style

        void toolbarPanel_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.Clear(Color.LightBlue);

            int x = 0;
            int y = 0;
            int w = 0;
            int h = 0;
            //Color c1 = Color.FromArgb(220, 220, 220);
            //Color c2 = Color.WhiteSmoke;
            Color c1 = Color.FromArgb(255, 255, 255);
            Color c2 = Color.FromArgb(255, 255, 255);

            w = this.toolbarPanel.Size.Width;
            h = this.toolbarPanel.Size.Height;
            Rectangle r1 = new Rectangle(x, y, w, h);
            System.Drawing.Drawing2D.LinearGradientBrush b1 = new System.Drawing.Drawing2D.LinearGradientBrush(r1, c1, c2, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            using (b1)
            {
                b1.SetSigmaBellShape(0.5f, 1.0f);
                e.Graphics.FillRectangle(b1, r1);
            }

            x = this.toolbarPanel.Location.X;
            y = this.toolbarPanel.Location.Y;
            w = this.toolbarPanel.Size.Width;
            h = 1;
            Rectangle r3 = new Rectangle(x, y, w, h);
            System.Drawing.SolidBrush b3 = new SolidBrush(Color.Black);
            using (b3)
            {
                e.Graphics.FillRectangle(b3, r3);
            }

            x = this.toolbarPanel.Location.X;
            y = this.toolbarPanel.Size.Height - 1;
            w = this.toolbarPanel.Size.Width;
            h = 1;
            Rectangle r4 = new Rectangle(x, y, w, h);
            System.Drawing.SolidBrush b4 = new SolidBrush(Color.Black);
            using (b4)
            {
                e.Graphics.FillRectangle(b4, r4);
            }
        }

        # endregion visual style

        private void MainForm_Load(object sender, EventArgs e)
        {
            // handle form style
            this.toolbarPanel.Paint += new PaintEventHandler(toolbarPanel_Paint);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // FORM
            this.Size = Properties.Settings.Default.FormSize;
            this.Location = Properties.Settings.Default.FormLocation;
            if (this.Location == new Point(-1, -1))
            {
                int x = Screen.PrimaryScreen.WorkingArea.Right - this.Width;
                int y = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
                this.Location = new Point(x, y);
            }

            //StartInterceptingSystemBalloons();
        }

        internal void InitializePreferences()
        {
            // GENERAL
            // start (default to running when launched - handled later in Form_Load)
            this.checkBoxAutoStart.Checked = Properties.Settings.Default.AutoStart;
            this.comboBoxPrefDisplay.Items.Add(Display.None);
            this.comboBoxDefaultSound.DataSource = PrefSound.GetList(false);
            this.comboBoxDefaultSound.SelectedItem = controller.DefaultSound;
            this.checkBoxMuteAllSounds.Checked = Properties.Settings.Default.MuteAllSounds;
            this.textBoxIdleAfterSeconds.Text = controller.IdleAfterSeconds.ToString();
            if (controller.CheckForIdle)
            {
                this.textBoxIdleAfterSeconds.Enabled = true;
                this.radioButtonIdleAfter.Checked = true;
            }

            // APPLICATIONS
            LoadRegisteredApplications();

            // DISPLAYS
            LoadAvailableDisplays();
            this.listControlDisplays.IsDefaultComparer = new Growl.UI.ListControl.IsDefaultComparerDelegate(defaultDisplayComparer);

            // SECURITY
            this.checkBoxRequireLocalPassword.Checked = this.controller.RequireLocalPassword;
            this.checkBoxAllowNetworkNotifications.Checked = this.controller.AllowNetworkNotifications;
            this.checkBoxAllowWebNotifications.Checked = this.controller.AllowWebNotifications;
            this.checkBoxAllowSubscriptions.Checked = this.controller.AllowSubscriptions;
            this.passwordManagerControl1.SetPasswordManager(this.controller.PasswordManager);
            this.passwordManagerControl1.Updated += new EventHandler(passwordManagerControl1_Updated);

            // NETWORK
            this.checkBoxEnableForwarding.Checked = Properties.Settings.Default.AllowForwarding;
            //this.forwardListView.Enabled = this.checkBoxEnableForwarding.Checked;
            //this.buttonAddComputer.Enabled = this.checkBoxEnableForwarding.Checked;
            LoadForwardDestinations();
            this.checkBoxEnableSubscriptions.Checked = Properties.Settings.Default.EnableSubscriptions;
            //this.subscribedListView.Enabled = this.checkBoxEnableSubscriptions.Checked;
            //this.buttonSubscribe.Enabled = this.checkBoxEnableSubscriptions.Checked;
            LoadSubscriptions();

            // HISTORY
            this.historyTrackBarTimer.Tick += new EventHandler(historyTrackBarTimer_Tick);
            this.historyDaysTrackBar.Minimum = HistoryListView.MIN_NUMBER_OF_DAYS;
            this.historyDaysTrackBar.Maximum = HistoryListView.MAX_NUMBER_OF_DAYS;
            this.historyDaysTrackBar.Value = Properties.Settings.Default.HistoryDays;
            this.historyListView.GroupBy = Properties.Settings.Default.HistorySortBy;
            this.historyListView.NumberOfDays = this.historyDaysTrackBar.Value;

            this.historyListView.PastNotifications = controller.PastNotifications;
            if (this.historyListView.GroupBy == HistoryGroupItemsBy.Application)
                this.historySortByApplicationRadioButton.Checked = true;
            else
                this.historySortByDateRadioButton.Checked = true;
            //this.historyListView.Draw();

            // ABOUT
            this.labelAboutGrowlVersion.Text = String.Format(this.labelAboutGrowlVersion.Text, Utility.FileVersionInfo.ProductVersion);
            //this.labelAboutGrowlVersion.Text = String.Format(this.labelAboutGrowlVersion.Text, f.FileVersion);
            this.labelAboutBuildNumber.Text = String.Format(this.labelAboutBuildNumber.Text, Utility.FileVersionInfo.FileVersion);
            //this.labelAboutBuildNumber.Text = String.Format(this.labelAboutBuildNumber.Text, a.GetName().Version.ToString());
            this.labelAboutBuildNumber.Left = this.labelAboutGrowlVersion.Right + 6;
        }

        internal void DoneInitializing()
        {
            // enable toolbar actions
            InitToolbarButtonAndPanel(this.toolbarButtonGeneral, this.panelGeneral);
            InitToolbarButtonAndPanel(this.toolbarButtonApplications, this.panelApplications);
            InitToolbarButtonAndPanel(this.toolbarButtonDisplays, this.panelDisplays);
            InitToolbarButtonAndPanel(this.toolbarButtonNetwork, this.panelNetwork);
            InitToolbarButtonAndPanel(this.toolbarButtonSecurity, this.panelSecurity);
            InitToolbarButtonAndPanel(this.toolbarButtonHistory, this.panelHistory);
            InitToolbarButtonAndPanel(this.toolbarButtonAbout, this.panelAbout);
            this.panelInitializing.Visible = false;
            SwitchPanel(this.toolbarButtonGeneral);
            this.Refresh();
        }

        private void InitToolbarButtonAndPanel(ToolStripButton tb, Panel panel)
        {
            this.panels.Add(panel);
            tb.Tag = panel;
            tb.Click += new EventHandler(toolbarButton_Click);
        }

        internal void UpdateInitializationProgress(string text, int progress)
        {
            this.labelInitializationStage.Text = text;
            this.labelInitializationStage.Refresh();
            this.progressBarInitializing.Value = progress;
            Application.DoEvents();
        }

        internal void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private static void SwitchPanel(ToolStripButton c)
        {
            ToolStrip ts = c.GetCurrentParent();
            foreach (ToolStripButton tsi in ts.Items)
            {
                if (tsi.Name == c.Name)
                {
                    tsi.Checked = true;
                    (tsi.Tag as Panel).Visible = true;
                }
                else
                {
                    tsi.Checked = false;
                    (tsi.Tag as Panel).Visible = false;
                }
            }
        }

        private void LoadRegisteredApplications()
        {
            BindApplicationList();

            if(this.listControlApplications.Items.Count > 0)
                this.listControlApplications.SelectedIndex = 0;
        }

        private void LoadAvailableDisplays()
        {
            BindDisplayList();
        }

        private void LoadForwardDestinations()
        {
            BindForwardList();
        }

        private void LoadSubscriptions()
        {
            BindSubscriptionList();
        }

        private void BindApplicationList()
        {
            SortedDictionary<string, RegisteredApplication> list = new SortedDictionary<string, RegisteredApplication>(controller.RegisteredApplications);

            this.listControlApplications.SuspendLayout();
            this.listControlApplications.Items.Clear();
            foreach (RegisteredApplication app in list.Values)
            {
                ListControlItem lci = new ListControlItem(app.Name, app);
                this.listControlApplications.AddItem(lci);
            }
            this.listControlApplications.ResumeLayout();

            if (this.controller.RegisteredApplications.Count == 0)
                this.panelNoApps.Visible = true;
            else
                this.panelNoApps.Visible = false;
        }

        internal void BindDisplayList()
        {
            SortedDictionary<string, Display> list = new SortedDictionary<string, Display>(controller.AvailableDisplays);

            this.listControlDisplays.SuspendLayout();
            this.listControlDisplays.Items.Clear();
            foreach (Display display in list.Values)
            {
                this.listControlDisplays.Items.Add(display);
            }

            this.listControlDisplays.SelectedIndex = 0;
            this.listControlDisplays.ResumeLayout();
        }

        private void BindForwardList()
        {
            this.forwardListView.SuspendLayout();
            this.forwardListView.Computers = controller.ForwardDestinations;
            this.forwardListView.Draw();
            this.forwardListView.ResumeLayout();
            this.buttonRemoveComputer.Enabled = false;
        }

        private void BindSubscriptionList()
        {
            this.subscribedListView.SuspendLayout();
            this.subscribedListView.Computers = controller.Subscriptions;
            this.subscribedListView.Draw();
            this.subscribedListView.ResumeLayout();
            this.buttonUnsubscribe.Enabled = false;
        }

        internal void UpdateState(string text, Color color)
        {
            this.labelCurrentState.Text = text;
            this.labelCurrentState.ForeColor = color;
        }

        internal void Mute(bool mute)
        {
            this.checkBoxMuteAllSounds.Checked = mute;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // instead of closing, just hide the form (minimized to system tray)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            else
            {
                // make sure to save any settings from an open settings panel
                if (this.panelDisplaySettings.Tag != null)
                {
                    Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)this.panelDisplaySettings.Tag;
                    sp.DeselectPanel();
                }

                // remember some form information for next time
                Properties.Settings.Default.FormSize = this.Size;
                Properties.Settings.Default.FormLocation = this.Location;
                Properties.Settings.Default.Save();
            }
        }

        internal Controller Controller
        {
            get
            {
                return this.controller;
            }
            set
            {
                this.controller = value;
            }
        }

        internal ProgressBar ProgressBarInitializing
        {
            get
            {
                return this.progressBarInitializing;
            }
        }

        internal OnOffButton OnOffButton
        {
            get
            {
                return this.onOffButton1;
            }
        }

        private void toolbarButton_Click(object sender, EventArgs e)
        {
            ToolStripButton c = (ToolStripButton)sender;
            SwitchPanel(c);
        }

        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.checkBoxAutoStart.Checked)
            {
                controller.DisableAutoStart();
            }
            else
            {
                controller.EnableAutoStart();
            }
        }

        internal void OnApplicationRegistered(RegisteredApplication ra)
        {
            BindApplicationList();
            this.controller.SaveApplicationPrefs();
        }

        internal void OnNotificationReceived(Growl.DisplayStyle.Notification n)
        {
            // do nothing for now
        }

        internal void OnNotificationPast(PastNotification pn)
        {
            this.historyListView.AddNotification(pn);
        }

        internal void OnForwardDestinationsUpdated()
        {
            this.BindForwardList();
            this.controller.SaveForwardPrefs();
        }

        internal void OnBonjourServiceUpdated(BonjourForwardDestination bfc)
        {
            //BindForwardList();
            this.forwardListView.Refresh();
        }

        internal void OnSubscriptionsUpdated(bool countChanged)
        {
            if (countChanged)
            {
                BindSubscriptionList();
                this.controller.SaveSubsriptionPrefs();
            }
            else
                this.subscribedListView.Refresh();
        }

        internal void OnSystemTimeChanged()
        {
            this.controller.ReloadPastNotifications();
            this.historyListView.PastNotifications = this.controller.PastNotifications;
            this.historyListView.Draw();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (historyTrackBarToolTip != null)
                {
                    historyTrackBarToolTip.Dispose();
                    historyTrackBarToolTip = null;
                }

                if (historyTrackBarTimer != null)
                {
                    historyTrackBarTimer.Dispose();
                    historyTrackBarTimer = null;
                }

                if (this.closeLastHotKey != null)
                {
                    this.closeLastHotKey.Dispose();
                    this.closeLastHotKey = null;
                }
                if (this.closeAllHotKey != null)
                {
                    this.closeAllHotKey.Dispose();
                    this.closeAllHotKey = null;
                }

                //StopInterceptingSystemBalloons();
            }

            base.Dispose(disposing);
        }

        private void historySortByDateRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.historySortByDateRadioButton.Checked)
            {
                this.historyListView.GroupBy = HistoryGroupItemsBy.Date;
                this.historyListView.Draw();
                Properties.Settings.Default.HistorySortBy = this.historyListView.GroupBy;
                Properties.Settings.Default.Save();
            }
        }

        private void historySortByApplicationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.historySortByApplicationRadioButton.Checked)
            {
                this.historyListView.GroupBy = HistoryGroupItemsBy.Application;
                this.historyListView.Draw();
                Properties.Settings.Default.HistorySortBy = this.historyListView.GroupBy;
                Properties.Settings.Default.Save();
            }
        }

        private void historyDaysTrackBar_Scroll(object sender, EventArgs e)
        {
            this.historyTrackBarTimer.Interval = 200;
            this.historyTrackBarTimer.Start();
        }

        void historyTrackBarTimer_Tick(object sender, EventArgs e)
        {
            this.historyTrackBarTimer.Stop();

            if (this.historyListView.NumberOfDays != this.historyDaysTrackBar.Value)
            {
                int val = this.historyDaysTrackBar.Value;

                if (this.historyTrackBarToolTip != null) this.historyTrackBarToolTip.Hide(this.historyDaysTrackBar);
                int interval = this.historyDaysTrackBar.Width / this.historyDaysTrackBar.Maximum;
                int x = (interval * val) - ((int)(0.70 * interval));
                int y = this.historyDaysTrackBar.Height - 18;
                this.historyTrackBarToolTip.Show(val.ToString(), this.historyDaysTrackBar, x, y, 750);

                this.historyListView.NumberOfDays = val;
                this.historyListView.Draw();

                Properties.Settings.Default.HistoryDays = val;
                Properties.Settings.Default.Save();
            }
        }

        private void labelAboutUsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(ll.Text);
            OpenLink(ll.Text);
        }

        private void labelAboutOriginalLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(ll.Text);
            OpenLink(ll.Text);
        }

        private void labelAboutIconsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(ll.Text);
            OpenLink(ll.Text);
        }

        private void OpenLink(string link)
        {
            System.Threading.ParameterizedThreadStart pts = new System.Threading.ParameterizedThreadStart(OpenLinkAsync);
            System.Threading.Thread t = new System.Threading.Thread(pts);
            t.Start(link);
        }

        private void OpenLinkAsync(object state)
        {
            string link = (string)state;
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(link);
            info.UseShellExecute = true;
            System.Diagnostics.Process.Start(info);
        }

        void panel_SettingsChanged(object sender, EventArgs e)
        {
            Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)sender;
            Display d = (Display)sp.Display;
            d.SettingsCollection = sp.GetSettings();
        }

        private void buttonPreviewDisplay_Click(object sender, EventArgs e)
        {
            panel_SettingsChanged(this.panelDisplaySettings.Tag, EventArgs.Empty);
            Display display = (Display)this.listControlDisplays.SelectedItem;
            this.controller.SendSystemNotification(Properties.Resources.SystemNotification_Preview_Title, String.Format(Properties.Resources.SystemNotification_Preview_Text, display.Name), display);
        }

        private void displayStyleWebsiteLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            System.Diagnostics.Process.Start(ll.Text);
        }

        private void buttonClearHistory_Click(object sender, EventArgs e)
        {
            this.controller.ClearHistory();
            this.historyListView.SuspendLayout();
            this.historyListView.PastNotifications = this.controller.PastNotifications;
            this.historyListView.Draw();
            this.historyListView.ResumeLayout();
            
            // Normally we shouldnt ever explicitly call GC.Collect(), but since the items in the History
            // view could have been taking up a lot of memory, and this is a user-initiated event that 
            // does not occur frequently, this is an OK place to force a collection.
            GC.Collect();
        }

        private void ShowPreferences(IRegisteredObject iro, NotificationPreferences prefs, string text)
        {
            //this.pictureBoxApplicationNotification.Image = new Bitmap(iro.Icon);
            this.pictureBoxApplicationNotification.Image = iro.Icon;
            this.labelApplicationNotification.Text = text;

            this.comboBoxPrefEnabled.DataSource = PrefEnabled.GetList();
            this.comboBoxPrefEnabled.SelectedItem = prefs.PrefEnabled;
            this.comboBoxPrefEnabled.Tag = prefs;

            this.comboBoxPrefDisplay.Items.Clear();
            this.comboBoxPrefDisplay.Items.Add(Display.Default);
            this.comboBoxPrefDisplay.Items.Add(Display.None);
            foreach (Display display in this.controller.AvailableDisplays.Values)
            {
                this.comboBoxPrefDisplay.Items.Add(display);
            }
            this.comboBoxPrefDisplay.SelectedItem = prefs.PrefDisplay;
            this.comboBoxPrefDisplay.Tag = prefs;

            this.comboBoxPrefDuration.DataSource = PrefDuration.GetList(true);
            this.comboBoxPrefDuration.SelectedItem = prefs.PrefDuration;
            this.comboBoxPrefDuration.Tag = prefs;

            this.comboBoxPrefSticky.DataSource = PrefSticky.GetList(true);
            this.comboBoxPrefSticky.SelectedItem = prefs.PrefSticky;
            this.comboBoxPrefSticky.Tag = prefs;

            this.comboBoxPrefForward.DataSource = PrefForward.GetList(true);
            this.comboBoxPrefForward.SelectedItem = prefs.PrefForward;
            this.comboBoxPrefForward.Tag = prefs;

            this.comboBoxPrefPriority.DataSource = PrefPriority.GetList(true);
            this.comboBoxPrefPriority.SelectedItem = prefs.PrefPriority;
            this.comboBoxPrefPriority.Tag = prefs;

            this.comboBoxPrefSound.DataSource = PrefSound.GetList(true);
            this.comboBoxPrefSound.SelectedItem = prefs.PrefSound;
            this.comboBoxPrefSound.Tag = prefs;
        }

        private void comboBoxPrefEnabled_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefEnabled.Tag;
            if (prefs != null)
            {
                PrefEnabled prefEnabled = (PrefEnabled) this.comboBoxPrefEnabled.SelectedItem;
                prefs.PrefEnabled = prefEnabled;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefDisplay_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefDisplay.Tag;
            if (prefs != null)
            {
                Display prefDisplay = (Display)this.comboBoxPrefDisplay.SelectedItem;
                prefs.PrefDisplay = prefDisplay;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void listControlApplications_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.listControlApplicationNotifications.SuspendLayout();
            this.listControlApplicationNotifications.Items.Clear();

            Growl.UI.ListControl lc = sender as Growl.UI.ListControl;
            ListControlItem selectedLCI = lc.SelectedItem as ListControlItem;
            if (selectedLCI != null)
            {
                RegisteredApplication app = (RegisteredApplication)selectedLCI.RegisteredObject;

                //this.pictureBoxApplication.Image = new Bitmap(app.Icon);
                this.pictureBoxApplication.Image = app.Icon;
                this.labelApplication.Text = app.Name;

                // add a default item to the list
                ListControlItem appLCI = new ListControlItem(Properties.Resources.Applications_AllNotifications, app);
                this.listControlApplicationNotifications.AddItem(appLCI);

                foreach (RegisteredNotification rn in app.Notifications.Values)
                {
                    ListControlItem lci = new ListControlItem(rn.Name, rn);
                    this.listControlApplicationNotifications.AddItem(lci);
                }

                this.listControlApplicationNotifications.SelectedIndex = 0;

                ShowPreferences(appLCI.RegisteredObject, app.Preferences, appLCI.Text);
                this.panelSelectedApplication.Visible = true;
                this.listControlApplicationNotifications.Select();
            }
            else
            {
                this.panelSelectedApplication.Visible = false;
            }
            this.listControlApplicationNotifications.ResumeLayout();
        }

        private void listControlApplicationNotifications_SelectedIndexChanged(object sender, EventArgs e)
        {
            Growl.UI.ListControl lc = sender as Growl.UI.ListControl;
            ListControlItem selectedLCI = lc.SelectedItem as ListControlItem;
            if (selectedLCI != null)
            {
                // TODO: consider making .Preferences property on IRegisteredObject to avoid this statement
                if (selectedLCI.RegisteredObject is RegisteredApplication)
                {
                    ApplicationPreferences prefs = ((RegisteredApplication)selectedLCI.RegisteredObject).Preferences;
                    ShowPreferences(selectedLCI.RegisteredObject, prefs, selectedLCI.Text);
                }
                else
                {
                    NotificationPreferences prefs = ((RegisteredNotification)selectedLCI.RegisteredObject).Preferences;
                    ShowPreferences(selectedLCI.RegisteredObject, prefs, selectedLCI.Text);
                }

                this.panelPrefs.Visible = true;
            }
            else
            {
                this.panelPrefs.Visible = false;
            }
        }

        private void listControlDisplays_SelectedIndexChanged(object sender, EventArgs e)
        {
            // hide any current panel
            if (this.panelDisplaySettings.Tag != null && this.panelDisplaySettings.Tag is Growl.DisplayStyle.SettingsPanelBase)
            {
                Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)this.panelDisplaySettings.Tag;
                sp.DeselectPanel();
            }
            this.panelDisplaySettings.Tag = null;
            this.panelDisplaySettingsContainer.Controls.Clear();
            this.displayStyleNameLabel.Text = "";
            this.displayStyleDescriptionLabel.Text = "";
            this.displayStyleAuthorLabel.Text = "";
            this.displayStyleWebsiteLabel.Text = "";
            this.displayStyleVersionLabel.Text = "";
            this.panelDisplaySettings.Visible = false;

            // show the new panel
            if (this.listControlDisplays.SelectedItem != null)
            {
                Display display = (Display)this.listControlDisplays.SelectedItem;
                Growl.DisplayStyle.SettingsPanelBase panel = display.SettingsPanel;
                if (panel != null)
                {
                    panel.Dock = DockStyle.Fill;
                    panel.Display = display;
                    panel.SettingsChanged -= new EventHandler(panel_SettingsChanged);
                    panel.SettingsChanged += new EventHandler(panel_SettingsChanged);
                    panel.SelectPanel();
                    this.panelDisplaySettingsContainer.Controls.Add(panel);
                    this.panelDisplaySettings.Tag = panel;

                    this.displayStyleNameLabel.Text = display.Name;
                    this.displayStyleDescriptionLabel.Text = display.Description;
                    this.displayStyleAuthorLabel.Text = String.Format("{0} {1}", Properties.Resources.Displays_CreatedBy, display.Author);
                    this.displayStyleWebsiteLabel.Text = display.Website;
                    this.displayStyleVersionLabel.Text = display.Version;

                    this.panelDisplaySettings.Visible = true;
                }
            }
        }

        void listControlDisplays_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.listControlDisplays.SelectedItem != null)
            {
                Display newDefault = (Display)this.listControlDisplays.SelectedItem;
                this.controller.DefaultDisplay = newDefault;
                this.listControlDisplays.Refresh();
            }
        }

        private void checkBoxEnableForwarding_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AllowForwarding = this.checkBoxEnableForwarding.Checked;
            Properties.Settings.Default.Save();

            this.forwardListView.AllDisabled = !Properties.Settings.Default.AllowForwarding;
            this.forwardListView.Refresh();

            //this.forwardListView.SelectedIndices.Clear();
            //this.forwardListView.Enabled = this.checkBoxEnableForwarding.Checked;
            //this.buttonAddComputer.Enabled = this.checkBoxEnableForwarding.Checked;
        }

        private void buttonAddComputer_Click(object sender, EventArgs e)
        {
            AddComputer f = new AddComputer();
            f.SetController(this.controller);
            f.ShowDialog(this);
        }

        private void buttonRemoveComputer_Click(object sender, EventArgs e)
        {
            if (this.forwardListView.SelectedItems.Count == 1)
            {
                ListViewItem lvi = this.forwardListView.SelectedItems[0];
                ForwardDestination fc = (ForwardDestination)lvi.Tag;
                if (fc != null && this.controller.ForwardDestinations.ContainsKey(fc.Key))
                {
                    this.controller.ForwardDestinations.Remove(fc.Key);
                    this.forwardListView.Items.Remove(lvi);
                }
            }
        }

        private void comboBoxPrefForward_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences) this.comboBoxPrefForward.Tag;
            if (prefs != null)
            {
                PrefForward prefForward = (PrefForward)this.comboBoxPrefForward.SelectedItem;

                // if they chose 'Custom...', we need to let them pick which computers to forward to.
                if (prefForward.IsCustom)
                {
                    ChooseForwarding f = new ChooseForwarding();
                    f.SetController(this.controller);
                    f.SetPrefs(prefs);
                    DialogResult result = f.ShowDialog(this);

                    // if they didnt save their choices, revert their selection
                    if (result != DialogResult.OK)
                    {
                        this.comboBoxPrefForward.SelectedItem = prefs.PrefForward;
                        return;
                    }
                }

                // save their preference
                prefs.PrefForward = prefForward;

                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefPriority_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefPriority.Tag;
            if (prefs != null)
            {
                PrefPriority prefPriority = (PrefPriority)this.comboBoxPrefPriority.SelectedItem;
                prefs.PrefPriority = prefPriority;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefSticky_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences) this.comboBoxPrefSticky.Tag;
            if (prefs != null)
            {
                PrefSticky prefSticky = (PrefSticky)this.comboBoxPrefSticky.SelectedItem;
                prefs.PrefSticky = prefSticky;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefSound_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefSound.Tag;
            if (prefs != null)
            {
                PrefSound prefSound = (PrefSound)this.comboBoxPrefSound.SelectedItem;
                prefs.PrefSound = prefSound;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void checkBoxRequireLocalPassword_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.RequireLocalPassword = this.checkBoxRequireLocalPassword.Checked;
        }

        private void checkBoxAllowNetworkNotifications_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.AllowNetworkNotifications = this.checkBoxAllowNetworkNotifications.Checked;
        }

        private void removeApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripApplications.Tag != null && this.contextMenuStripApplications.Tag is ListControlItem)
            {
                ListControlItem lci = (ListControlItem)this.contextMenuStripApplications.Tag;
                this.controller.RegisteredApplications.Remove(lci.RegisteredObject.Name);
                this.controller.SaveApplicationPrefs();
            }
            this.contextMenuStripApplications.Tag = null;
            this.contextMenuStripApplications.Hide();
            BindApplicationList();
            if (this.listControlApplications.Items.Count > 0)
                this.listControlApplications.SelectedIndex = 0;
            else
                this.panelSelectedApplication.Visible = false;
        }

        private void listControlApplications_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuStripApplications.Hide();

            if(e.Button == MouseButtons.Right)
            {
                int index = this.listControlApplications.IndexFromPoint(e.Location);
                if(index != ListBox.NoMatches)
                {
                    this.listControlApplications.SelectedIndex = index;
                    this.contextMenuStripApplications.Tag = this.listControlApplications.SelectedItem;
                    this.contextMenuStripApplications.Show(this.listControlApplications, e.Location);
                }
            }
        }

        private void comboBoxDefaultSound_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PrefSound ps = (PrefSound)this.comboBoxDefaultSound.SelectedItem;
            this.controller.DefaultSound = ps;
            try
            {
                if (ps.PlaySound.HasValue && ps.PlaySound.Value)
                {
                    System.Media.SoundPlayer sp = new System.Media.SoundPlayer(ps.SoundFile);
                    using (sp)
                    {
                        sp.Play();
                    }
                }
            }
            catch
            {
                // suppress - dont fail just because the sound could not play
            }
        }

        private void radioButtonIdleAfter_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButtonIdleAfter.Checked)
            {
                this.textBoxIdleAfterSeconds.Enabled = true;
                int idleAfterSeconds = Convert.ToInt32(this.textBoxIdleAfterSeconds.Text);
                this.controller.IdleAfterSeconds = idleAfterSeconds;
                this.controller.CheckForIdle = true;
            }
            else
            {
                this.controller.CheckForIdle = false;
                this.textBoxIdleAfterSeconds.Enabled = false;
                if (String.IsNullOrEmpty(this.textBoxIdleAfterSeconds.Text)) this.textBoxIdleAfterSeconds.Text = "0";
            }
        }

        private void textBoxIdleAfterSeconds_KeyPress(object sender, KeyPressEventArgs e)
        {
            // limit to only numbers and control (backspace, delete, copy/paste)
            // also limit the value to 4 digits
            bool isDigit = Char.IsDigit(e.KeyChar);
            bool isControl = Char.IsControl(e.KeyChar);
            if(isControl || (isDigit && this.textBoxIdleAfterSeconds.Text.Length < 4))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void textBoxIdleAfterSeconds_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxIdleAfterSeconds.Enabled)
            {
                if (!String.IsNullOrEmpty(this.textBoxIdleAfterSeconds.Text))
                {
                    int idleAfterSeconds = Convert.ToInt32(this.textBoxIdleAfterSeconds.Text);
                    this.controller.IdleAfterSeconds = idleAfterSeconds;
                }
            }
        }

        private void forwardListView_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuStripForwardDestinations.Hide();

            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = this.forwardListView.GetItemAt(e.X, e.Y);
                if (item != null && item.Tag != null)
                {
                    ForwardDestination fc = item.Tag as ForwardDestination;
                    if (fc != null)
                    {
                        this.editToolStripMenuItem.Visible = !(fc is SubscribedForwardDestination);

                        this.contextMenuStripForwardDestinations.Tag = fc;
                        this.contextMenuStripForwardDestinations.Show(this.forwardListView, e.Location);
                    }
                }
            }
        }

        private void removeComputerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripForwardDestinations.Tag != null)
            {
                ForwardDestination fc = this.contextMenuStripForwardDestinations.Tag as ForwardDestination;
                if (fc != null)
                {
                    this.controller.RemoveForwardDestination(fc);
                }
            }
            this.contextMenuStripForwardDestinations.Tag = null;
            this.contextMenuStripForwardDestinations.Hide();
        }

        private void buttonSubscribe_Click(object sender, EventArgs e)
        {
            AddComputer f = new AddComputer(true);
            f.SetController(this.controller);
            f.ShowDialog(this);
        }

        private void buttonUnsubscribe_Click(object sender, EventArgs e)
        {
            if (this.subscribedListView.SelectedItems.Count == 1)
            {
                ListViewItem lvi = this.subscribedListView.SelectedItems[0];
                Subscription sub = (Subscription)lvi.Tag;
                if (sub != null && this.controller.Subscriptions.ContainsKey(sub.Key))
                {
                    sub.Kill();
                    this.controller.Subscriptions.Remove(sub.Key);
                    this.subscribedListView.Items.Remove(lvi);
                    sub = null;
                }
            }
        }

        private void checkBoxEnableSubscriptions_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSubscriptions = this.checkBoxEnableSubscriptions.Checked;
            Properties.Settings.Default.Save();

            foreach (Subscription subscription in this.controller.Subscriptions.Values)
            {
                subscription.Allowed = this.checkBoxEnableSubscriptions.Checked;
            }

            this.subscribedListView.AllDisabled = !Properties.Settings.Default.EnableSubscriptions;
            this.subscribedListView.Refresh();

            if (!this.checkBoxEnableSubscriptions.Checked)
                this.buttonUnsubscribe.Enabled = false;
            else
            {
                if (this.subscribedListView.SelectedIndices != null && this.subscribedListView.SelectedIndices.Count > 0) this.buttonUnsubscribe.Enabled = true;
            }
        }

        private void subscribedListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.buttonUnsubscribe.Enabled = e.IsSelected;
        }

        private void forwardListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.buttonRemoveComputer.Enabled = e.IsSelected;
        }

        private void checkBoxAllowWebNotifications_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.AllowWebNotifications = this.checkBoxAllowWebNotifications.Checked;
        }

        private void checkBoxAllowSubscriptions_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.AllowSubscriptions = this.checkBoxAllowSubscriptions.Checked;
        }

        private void buttonSetAsDefault_Click(object sender, EventArgs e)
        {
            Display newDefault = (Display)this.listControlDisplays.SelectedItem;
            this.controller.DefaultDisplay = newDefault;
            this.listControlDisplays.Refresh();
        }

        private bool defaultDisplayComparer(object obj)
        {
            Display display = obj as Display;
            if (display != null)
            {
                if (display.ActualName == this.controller.DefaultDisplay.ActualName)
                    return true;
            }
            return false;
        }

        private void checkBoxMuteAllSounds_CheckedChanged(object sender, EventArgs e)
        {
            ApplicationMain.Program.Mute(this.checkBoxMuteAllSounds.Checked);
        }

        private void comboBoxPrefDuration_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefDuration.Tag;
            if (prefs != null)
            {
                PrefDuration prefDuration = (PrefDuration)this.comboBoxPrefDuration.SelectedItem;
                prefs.PrefDuration = prefDuration;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void getDisplaysLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            string url = (string) ll.Tag;
            OpenLink(url);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripForwardDestinations.Tag != null)
            {
                ForwardDestination fc = this.contextMenuStripForwardDestinations.Tag as ForwardDestination;
                if (fc != null)
                {
                    AddComputer f = new AddComputer(fc);
                    f.SetController(this.controller);
                    f.ShowDialog(this); 
                }
            }
            this.contextMenuStripForwardDestinations.Tag = null;
            this.contextMenuStripForwardDestinations.Hide();
        }

        private void editSubscriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripSubscriptions.Tag != null)
            {
                Subscription sub = this.contextMenuStripSubscriptions.Tag as Subscription;
                if (sub != null)
                {
                    AddComputer f = new AddComputer(sub);
                    f.SetController(this.controller);
                    f.ShowDialog(this);
                }
            }
            this.contextMenuStripSubscriptions.Tag = null;
            this.contextMenuStripSubscriptions.Hide();
        }

        private void unsubscribeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripSubscriptions.Tag != null)
            {
                Subscription sub = this.contextMenuStripSubscriptions.Tag as Subscription;
                if (sub != null)
                {
                    this.controller.RemoveSubscription(sub);
                }
            }
            this.contextMenuStripSubscriptions.Tag = null;
            this.contextMenuStripSubscriptions.Hide();
        }

        private void subscribedListView_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuStripSubscriptions.Hide();

            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = this.subscribedListView.GetItemAt(e.X, e.Y);
                if (item != null && item.Tag != null)
                {
                    Subscription sub = item.Tag as Subscription;
                    if (sub != null)
                    {
                        this.contextMenuStripSubscriptions.Tag = sub;
                        this.contextMenuStripSubscriptions.Show(this.subscribedListView, e.Location);
                    }
                }
            }
        }

        private void contextMenuStripApplications_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            this.listControlApplications.Refresh();
        }

        private void historyFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            this.historyListView.Filter = this.historyFilterTextBox.Text;
        }

        void passwordManagerControl1_Updated(object sender, EventArgs e)
        {
            this.controller.SavePasswordPrefs();
        }

        protected override void WndProc(ref Message m)
        {
            // check if we got a hot key pressed.
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                // we could get the actual key pressed, but it is easier to just check the id
                int id = m.WParam.ToInt32();

                if (id == this.closeLastHotKey.ID)
                    this.controller.CloseLastNotification();
                else if (id == this.closeAllHotKey.ID)
                    this.controller.CloseAllOpenNotifications();
            }

            /* this is for handling intercepted system balloon messages - NOT READY YET
            if (this.sbi != null)
                sbi.ProcessWindowMessage(ref m);
             * */

            base.WndProc(ref m);
        }

        /* THIS IS NOT READY FOR RELEASE YET
        SystemBalloonIntercepter sbi = null;
        private void StartInterceptingSystemBalloons()
        {
            StopInterceptingSystemBalloons();

            this.sbi = new SystemBalloonIntercepter(this.Handle);
            sbi.Start();
        }

        private void StopInterceptingSystemBalloons()
        {
            if (sbi != null)
            {
                this.sbi.Stop();
                this.sbi = null;
            }
        }
         * */
    }
}