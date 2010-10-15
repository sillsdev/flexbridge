namespace SIL.LiftBridge
{
	public partial class ExistingSystem
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
				if (_importerExporter != null)
				{
					_bridgeControl.SyncStarting -= BridgeControlSyncStarting;
					_bridgeControl.SyncFinished -= BridgeControlSyncFinished;
				}
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
			this._tpSendReceive = new System.Windows.Forms.TabPage();
			this._tpNotes = new System.Windows.Forms.TabPage();
			this._tpHistory = new System.Windows.Forms.TabPage();
			this._tpAbout = new System.Windows.Forms.TabPage();
			this._tcMain.SuspendLayout();
			this.SuspendLayout();
			//
			// _tcMain
			//
			this._tcMain.Controls.Add(this._tpSendReceive);
			this._tcMain.Controls.Add(this._tpNotes);
			this._tcMain.Controls.Add(this._tpHistory);
			this._tcMain.Controls.Add(this._tpAbout);
			this._tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tcMain.Location = new System.Drawing.Point(0, 0);
			this._tcMain.Name = "_tcMain";
			this._tcMain.SelectedIndex = 0;
			this._tcMain.Size = new System.Drawing.Size(584, 413);
			this._tcMain.TabIndex = 0;
			//
			// _tpSendReceive
			//
			this._tpSendReceive.Location = new System.Drawing.Point(4, 22);
			this._tpSendReceive.Name = "_tpSendReceive";
			this._tpSendReceive.Padding = new System.Windows.Forms.Padding(3);
			this._tpSendReceive.Size = new System.Drawing.Size(576, 387);
			this._tpSendReceive.TabIndex = 0;
			this._tpSendReceive.Text = "Send/Receive";
			this._tpSendReceive.UseVisualStyleBackColor = true;
			//
			// _tpNotes
			//
			this._tpNotes.Location = new System.Drawing.Point(4, 22);
			this._tpNotes.Name = "_tpNotes";
			this._tpNotes.Padding = new System.Windows.Forms.Padding(3);
			this._tpNotes.Size = new System.Drawing.Size(576, 387);
			this._tpNotes.TabIndex = 1;
			this._tpNotes.Text = "Notes";
			this._tpNotes.UseVisualStyleBackColor = true;
			//
			// _tpHistory
			//
			this._tpHistory.Location = new System.Drawing.Point(4, 22);
			this._tpHistory.Name = "_tpHistory";
			this._tpHistory.Size = new System.Drawing.Size(576, 387);
			this._tpHistory.TabIndex = 2;
			this._tpHistory.Text = "History";
			this._tpHistory.UseVisualStyleBackColor = true;
			//
			// _tpAbout
			//
			this._tpAbout.Location = new System.Drawing.Point(4, 22);
			this._tpAbout.Name = "_tpAbout";
			this._tpAbout.Size = new System.Drawing.Size(576, 387);
			this._tpAbout.TabIndex = 3;
			this._tpAbout.Text = "About";
			this._tpAbout.UseVisualStyleBackColor = true;
			//
			// ExistingSystem
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._tcMain);
			this.Name = "ExistingSystem";
			this.Size = new System.Drawing.Size(584, 413);
			this._tcMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl _tcMain;
		private System.Windows.Forms.TabPage _tpSendReceive;
		private System.Windows.Forms.TabPage _tpNotes;
		private System.Windows.Forms.TabPage _tpHistory;
		private System.Windows.Forms.TabPage _tpAbout;
	}
}
