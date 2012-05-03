namespace SIL.LiftBridge.View
{
	partial class GetCloneFromSharedNetworkFolderDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetCloneFromSharedNetworkFolderDialog));
			this.label1 = new System.Windows.Forms.Label();
			this._tbSelectedShare = new System.Windows.Forms.TextBox();
			this._btnBrowse = new System.Windows.Forms.Button();
			this._lvRepositorySourceCandidates = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._copyToComputerButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._logBox = new Palaso.Progress.LogBox.LogBox();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 2);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(219, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Locate shared folder on a computer near you";
			//
			// _tbSelectedShare
			//
			this._tbSelectedShare.Enabled = false;
			this._tbSelectedShare.Location = new System.Drawing.Point(7, 19);
			this._tbSelectedShare.Name = "_tbSelectedShare";
			this._tbSelectedShare.Size = new System.Drawing.Size(197, 20);
			this._tbSelectedShare.TabIndex = 1;
			//
			// _btnBrowse
			//
			this._btnBrowse.Location = new System.Drawing.Point(210, 18);
			this._btnBrowse.Name = "_btnBrowse";
			this._btnBrowse.Size = new System.Drawing.Size(75, 23);
			this._btnBrowse.TabIndex = 2;
			this._btnBrowse.Text = "Browse...";
			this._btnBrowse.UseVisualStyleBackColor = true;
			this._btnBrowse.Click += new System.EventHandler(this.BrowseClicked);
			//
			// _lvRepositorySourceCandidates
			//
			this._lvRepositorySourceCandidates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._lvRepositorySourceCandidates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2});
			this._lvRepositorySourceCandidates.Enabled = false;
			this._lvRepositorySourceCandidates.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._lvRepositorySourceCandidates.FullRowSelect = true;
			this._lvRepositorySourceCandidates.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this._lvRepositorySourceCandidates.Location = new System.Drawing.Point(7, 50);
			this._lvRepositorySourceCandidates.MultiSelect = false;
			this._lvRepositorySourceCandidates.Name = "_lvRepositorySourceCandidates";
			this._lvRepositorySourceCandidates.ShowItemToolTips = true;
			this._lvRepositorySourceCandidates.Size = new System.Drawing.Size(281, 177);
			this._lvRepositorySourceCandidates.TabIndex = 3;
			this._lvRepositorySourceCandidates.UseCompatibleStateImageBehavior = false;
			this._lvRepositorySourceCandidates.View = System.Windows.Forms.View.Details;
			//
			// columnHeader1
			//
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 170;
			//
			// columnHeader2
			//
			this.columnHeader2.Text = "Modified Date";
			this.columnHeader2.Width = 120;
			//
			// _copyToComputerButton
			//
			this._copyToComputerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._copyToComputerButton.Location = new System.Drawing.Point(7, 233);
			this._copyToComputerButton.Name = "_copyToComputerButton";
			this._copyToComputerButton.Size = new System.Drawing.Size(116, 23);
			this._copyToComputerButton.TabIndex = 8;
			this._copyToComputerButton.Text = "&Copy To Computer";
			this._copyToComputerButton.UseVisualStyleBackColor = true;
			this._copyToComputerButton.Click += new System.EventHandler(this.CopyToMyComputerClicked);
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(129, 233);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 7;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(210, 233);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 6;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
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
			this._logBox.Location = new System.Drawing.Point(59, 45);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = false;
			this._logBox.ShowDetailsMenuItem = false;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = true;
			this._logBox.Size = new System.Drawing.Size(229, 135);
			this._logBox.TabIndex = 20;
			//
			// GetCloneFromSharedNetworkFolderDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(295, 262);
			this.Controls.Add(this._logBox);
			this.Controls.Add(this._copyToComputerButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._lvRepositorySourceCandidates);
			this.Controls.Add(this._btnBrowse);
			this.Controls.Add(this._tbSelectedShare);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GetCloneFromSharedNetworkFolderDialog";
			this.ShowInTaskbar = false;
			this.Text = "Get Project From Shared Network Folder";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _tbSelectedShare;
		private System.Windows.Forms.Button _btnBrowse;
		private System.Windows.Forms.ListView _lvRepositorySourceCandidates;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button _copyToComputerButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
		private Palaso.Progress.LogBox.LogBox _logBox;
	}
}