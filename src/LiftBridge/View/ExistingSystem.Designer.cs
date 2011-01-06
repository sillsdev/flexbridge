using System.Windows.Forms;

namespace SIL.LiftBridge.View
{
	internal partial class ExistingSystem : UserControl
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
				if (_chorusSystem != null)
					_chorusSystem.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExistingSystem));
			this._tcMain = new System.Windows.Forms.TabControl();
			this._tpNotes = new System.Windows.Forms.TabPage();
			this._tpHistory = new System.Windows.Forms.TabPage();
			this._tpAbout = new System.Windows.Forms.TabPage();
			this._sendReceiveButton = new System.Windows.Forms.Button();
			this._webBrowser = new System.Windows.Forms.WebBrowser();
			this._tcMain.SuspendLayout();
			this._tpAbout.SuspendLayout();
			this.SuspendLayout();
			//
			// _tcMain
			//
			this._tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._tcMain.Controls.Add(this._tpNotes);
			this._tcMain.Controls.Add(this._tpHistory);
			this._tcMain.Controls.Add(this._tpAbout);
			this._tcMain.Location = new System.Drawing.Point(0, 52);
			this._tcMain.Margin = new System.Windows.Forms.Padding(0);
			this._tcMain.Name = "_tcMain";
			this._tcMain.Padding = new System.Drawing.Point(0, 0);
			this._tcMain.SelectedIndex = 0;
			this._tcMain.Size = new System.Drawing.Size(581, 361);
			this._tcMain.TabIndex = 0;
			//
			// _tpNotes
			//
			this._tpNotes.Location = new System.Drawing.Point(4, 22);
			this._tpNotes.Margin = new System.Windows.Forms.Padding(0);
			this._tpNotes.Name = "_tpNotes";
			this._tpNotes.Size = new System.Drawing.Size(573, 335);
			this._tpNotes.TabIndex = 1;
			this._tpNotes.Text = "Notes";
			this._tpNotes.UseVisualStyleBackColor = true;
			//
			// _tpHistory
			//
			this._tpHistory.Location = new System.Drawing.Point(4, 22);
			this._tpHistory.Margin = new System.Windows.Forms.Padding(0);
			this._tpHistory.Name = "_tpHistory";
			this._tpHistory.Size = new System.Drawing.Size(573, 335);
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
			this._tpAbout.Size = new System.Drawing.Size(573, 335);
			this._tpAbout.TabIndex = 3;
			this._tpAbout.Text = "About";
			this._tpAbout.UseVisualStyleBackColor = true;
			//
			// _sendReceiveButton
			//
			this._sendReceiveButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._sendReceiveButton.Image = ((System.Drawing.Image)(resources.GetObject("_sendReceiveButton.Image")));
			this._sendReceiveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._sendReceiveButton.Location = new System.Drawing.Point(4, 8);
			this._sendReceiveButton.Name = "_sendReceiveButton";
			this._sendReceiveButton.Size = new System.Drawing.Size(132, 38);
			this._sendReceiveButton.TabIndex = 15;
			this._sendReceiveButton.Text = "Send/Receive";
			this._sendReceiveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._sendReceiveButton.UseVisualStyleBackColor = false;
			this._sendReceiveButton.Click += new System.EventHandler(this.SendReceiveButtonClick);
			//
			// _webBrowser
			//
			this._webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this._webBrowser.Location = new System.Drawing.Point(0, 0);
			this._webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this._webBrowser.Name = "_webBrowser";
			this._webBrowser.Size = new System.Drawing.Size(573, 335);
			this._webBrowser.TabIndex = 0;
			//
			// ExistingSystem
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._sendReceiveButton);
			this.Controls.Add(this._tcMain);
			this.Name = "ExistingSystem";
			this.Size = new System.Drawing.Size(584, 413);
			this.Load += new System.EventHandler(this.LoadExistingSystem);
			this._tcMain.ResumeLayout(false);
			this._tpAbout.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl _tcMain;
		private System.Windows.Forms.TabPage _tpNotes;
		private System.Windows.Forms.TabPage _tpHistory;
		private System.Windows.Forms.TabPage _tpAbout;
		private System.Windows.Forms.Button _sendReceiveButton;
		private WebBrowser _webBrowser;
	}
}
