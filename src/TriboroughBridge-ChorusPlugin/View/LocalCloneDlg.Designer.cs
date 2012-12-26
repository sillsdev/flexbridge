namespace TriboroughBridge_ChorusPlugin.View
{
	partial class LocalCloneDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalCloneDlg));
			this._logBox = new Palaso.UI.WindowsForms.Progress.LogBox();
			this._btnClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _logBox
			//
			this._logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
			this._logBox.CancelRequested = false;
			this._logBox.Dock = System.Windows.Forms.DockStyle.Top;
			this._logBox.ErrorEncountered = false;
			this._logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._logBox.GetDiagnosticsMethod = null;
			this._logBox.Location = new System.Drawing.Point(0, 0);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = true;
			this._logBox.ShowDetailsMenuItem = true;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = true;
			this._logBox.Size = new System.Drawing.Size(351, 395);
			this._logBox.TabIndex = 0;
			//
			// _btnClose
			//
			this._btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._btnClose.Enabled = false;
			this._btnClose.Location = new System.Drawing.Point(270, 401);
			this._btnClose.Name = "_btnClose";
			this._btnClose.Size = new System.Drawing.Size(75, 23);
			this._btnClose.TabIndex = 1;
			this._btnClose.Text = "Ok";
			this._btnClose.UseVisualStyleBackColor = true;
			this._btnClose.Click += new System.EventHandler(this.OkClicked);
			//
			// LocalCloneDlg
			//
			this.AcceptButton = this._btnClose;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(351, 428);
			this.ControlBox = false;
			this.Controls.Add(this._btnClose);
			this.Controls.Add(this._logBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LocalCloneDlg";
			this.ShowInTaskbar = false;
			this.Text = "Move system";
			this.ResumeLayout(false);

		}

		#endregion

		private Palaso.UI.WindowsForms.Progress.LogBox _logBox;
		private System.Windows.Forms.Button _btnClose;

	}
}