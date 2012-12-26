namespace TriboroughBridge_ChorusPlugin.View
{
	partial class LogControl
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._logBox = new Palaso.UI.WindowsForms.Progress.LogBox();
			this._btnOk = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _logBox
			//
			this._logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
			this._logBox.CancelRequested = false;
			this._logBox.ErrorEncountered = false;
			this._logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._logBox.GetDiagnosticsMethod = null;
			this._logBox.Location = new System.Drawing.Point(0, 0);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = false;
			this._logBox.ShowDetailsMenuItem = false;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = true;
			this._logBox.Size = new System.Drawing.Size(321, 413);
			this._logBox.TabIndex = 0;
			//
			// _btnOk
			//
			this._btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btnOk.Enabled = false;
			this._btnOk.Location = new System.Drawing.Point(231, 426);
			this._btnOk.Name = "_btnOk";
			this._btnOk.Size = new System.Drawing.Size(75, 23);
			this._btnOk.TabIndex = 1;
			this._btnOk.Text = "Close";
			this._btnOk.UseVisualStyleBackColor = true;
			this._btnOk.Click += new System.EventHandler(this.CloseBtnClicked);
			//
			// LogControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._btnOk);
			this.Controls.Add(this._logBox);
			this.Name = "LogControl";
			this.Size = new System.Drawing.Size(321, 459);
			this.ResumeLayout(false);

		}

		#endregion

		private Palaso.UI.WindowsForms.Progress.LogBox _logBox;
		private System.Windows.Forms.Button _btnOk;
	}
}
