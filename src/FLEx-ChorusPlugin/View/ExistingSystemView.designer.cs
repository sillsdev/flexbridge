namespace FLEx_ChorusPlugin.View
{
	internal partial class ExistingSystemView
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
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._tcMain = new System.Windows.Forms.TabControl();
			this._tpNotes = new System.Windows.Forms.TabPage();
			this._tpHistory = new System.Windows.Forms.TabPage();
			this._tpAbout = new System.Windows.Forms.TabPage();
			this._webBrowser = new System.Windows.Forms.WebBrowser();
			this._tpSendReceive = new System.Windows.Forms.TabPage();
			this._tpSettings = new System.Windows.Forms.TabPage();
			this._tpTroubleshoot = new System.Windows.Forms.TabPage();
			this._tcMain.SuspendLayout();
			this._tpAbout.SuspendLayout();
			this.SuspendLayout();
			//
			// _tcMain
			//
			this._tcMain.Controls.Add(this._tpNotes);
			this._tcMain.Controls.Add(this._tpHistory);
			this._tcMain.Controls.Add(this._tpSendReceive);
			this._tcMain.Controls.Add(this._tpSettings);
			this._tcMain.Controls.Add(this._tpTroubleshoot);
			this._tcMain.Controls.Add(this._tpAbout);
			this._tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tcMain.Location = new System.Drawing.Point(0, 0);
			this._tcMain.Margin = new System.Windows.Forms.Padding(0);
			this._tcMain.Name = "_tcMain";
			this._tcMain.Padding = new System.Drawing.Point(0, 0);
			this._tcMain.SelectedIndex = 0;
			this._tcMain.Size = new System.Drawing.Size(450, 309);
			this._tcMain.TabIndex = 0;
			//
			// _tpNotes
			//
			this._tpNotes.Location = new System.Drawing.Point(4, 22);
			this._tpNotes.Margin = new System.Windows.Forms.Padding(0);
			this._tpNotes.Name = "_tpNotes";
			this._tpNotes.Size = new System.Drawing.Size(442, 283);
			this._tpNotes.TabIndex = 1;
			this._tpNotes.Text = "Notes";
			this._tpNotes.UseVisualStyleBackColor = true;
			//
			// _tpHistory
			//
			this._tpHistory.Location = new System.Drawing.Point(4, 22);
			this._tpHistory.Margin = new System.Windows.Forms.Padding(0);
			this._tpHistory.Name = "_tpHistory";
			this._tpHistory.Size = new System.Drawing.Size(442, 283);
			this._tpHistory.TabIndex = 2;
			this._tpHistory.Text = "History";
			this._tpHistory.UseVisualStyleBackColor = true;
			//
			// _tpAbout
			//
			this._tpAbout.Controls.Add(this._webBrowser);
			this._tpAbout.Location = new System.Drawing.Point(4, 22);
			this._tpAbout.Margin = new System.Windows.Forms.Padding(0);
			this._tpAbout.Name = "_tpAbout";
			this._tpAbout.Size = new System.Drawing.Size(442, 283);
			this._tpAbout.TabIndex = 3;
			this._tpAbout.Text = "About";
			this._tpAbout.UseVisualStyleBackColor = true;
			//
			// _webBrowser
			//
			this._webBrowser.AllowWebBrowserDrop = false;
			this._webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this._webBrowser.IsWebBrowserContextMenuEnabled = false;
			this._webBrowser.Location = new System.Drawing.Point(0, 0);
			this._webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this._webBrowser.Name = "_webBrowser";
			this._webBrowser.Size = new System.Drawing.Size(442, 283);
			this._webBrowser.TabIndex = 0;
			this._webBrowser.Url = new System.Uri("", System.UriKind.Relative);
			//
			// _tpSendReceive
			//
			this._tpSendReceive.Location = new System.Drawing.Point(4, 22);
			this._tpSendReceive.Name = "_tpSendReceive";
			this._tpSendReceive.Padding = new System.Windows.Forms.Padding(3);
			this._tpSendReceive.Size = new System.Drawing.Size(442, 283);
			this._tpSendReceive.TabIndex = 4;
			this._tpSendReceive.Text = "Send/Receive";
			this._tpSendReceive.UseVisualStyleBackColor = true;
			//
			// _tpSettings
			//
			this._tpSettings.Location = new System.Drawing.Point(4, 22);
			this._tpSettings.Name = "_tpSettings";
			this._tpSettings.Size = new System.Drawing.Size(442, 283);
			this._tpSettings.TabIndex = 5;
			this._tpSettings.Text = "Settings";
			this._tpSettings.UseVisualStyleBackColor = true;
			//
			// _tpTroubleshoot
			//
			this._tpTroubleshoot.Location = new System.Drawing.Point(4, 22);
			this._tpTroubleshoot.Name = "_tpTroubleshoot";
			this._tpTroubleshoot.Size = new System.Drawing.Size(442, 283);
			this._tpTroubleshoot.TabIndex = 6;
			this._tpTroubleshoot.Text = "Troubleshoot";
			this._tpTroubleshoot.UseVisualStyleBackColor = true;
			//
			// ExistingSystemView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._tcMain);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "ExistingSystemView";
			this.Size = new System.Drawing.Size(450, 309);
			this.Load += new System.EventHandler(this.ExistingSystemViewLoad);
			this._tcMain.ResumeLayout(false);
			this._tpAbout.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.TabPage _tpNotes;
		internal System.Windows.Forms.TabPage _tpHistory;
		internal System.Windows.Forms.TabPage _tpAbout;
		internal System.Windows.Forms.TabControl _tcMain;
		private System.Windows.Forms.WebBrowser _webBrowser;
		private System.Windows.Forms.TabPage _tpSendReceive;
		private System.Windows.Forms.TabPage _tpSettings;
		private System.Windows.Forms.TabPage _tpTroubleshoot;
	}
}
