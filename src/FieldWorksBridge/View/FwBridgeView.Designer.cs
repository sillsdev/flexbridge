namespace FieldWorksBridge.View
{
	internal partial class FwBridgeView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FwBridgeView));
			this._sendReceiveButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._cbProjects = new System.Windows.Forms.ComboBox();
			this._tcPages = new System.Windows.Forms.TabControl();
			this._tpNotes = new System.Windows.Forms.TabPage();
			this._tpHistory = new System.Windows.Forms.TabPage();
			this._tpAbout = new System.Windows.Forms.TabPage();
			this._tcPages.SuspendLayout();
			this.SuspendLayout();
			//
			// _sendReceiveButton
			//
			this._sendReceiveButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._sendReceiveButton.Image = ((System.Drawing.Image)(resources.GetObject("_sendReceiveButton.Image")));
			this._sendReceiveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._sendReceiveButton.Location = new System.Drawing.Point(335, 6);
			this._sendReceiveButton.Name = "_sendReceiveButton";
			this._sendReceiveButton.Size = new System.Drawing.Size(132, 38);
			this._sendReceiveButton.TabIndex = 18;
			this._sendReceiveButton.Text = "Send/Receive";
			this._sendReceiveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._sendReceiveButton.UseVisualStyleBackColor = false;
			this._sendReceiveButton.Click += new System.EventHandler(this.SendReceiveButtonClick);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "Project";
			//
			// _cbProjects
			//
			this._cbProjects.FormattingEnabled = true;
			this._cbProjects.Location = new System.Drawing.Point(50, 15);
			this._cbProjects.Name = "_cbProjects";
			this._cbProjects.Size = new System.Drawing.Size(265, 21);
			this._cbProjects.TabIndex = 16;
			this._cbProjects.SelectedIndexChanged += new System.EventHandler(this.ProjectsSelectedIndexChanged);
			//
			// _tcPages
			//
			this._tcPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._tcPages.Controls.Add(this._tpNotes);
			this._tcPages.Controls.Add(this._tpHistory);
			this._tcPages.Controls.Add(this._tpAbout);
			this._tcPages.Location = new System.Drawing.Point(1, 50);
			this._tcPages.Name = "_tcPages";
			this._tcPages.SelectedIndex = 0;
			this._tcPages.Size = new System.Drawing.Size(848, 406);
			this._tcPages.TabIndex = 19;
			//
			// _tpNotes
			//
			this._tpNotes.Location = new System.Drawing.Point(4, 22);
			this._tpNotes.Name = "_tpNotes";
			this._tpNotes.Padding = new System.Windows.Forms.Padding(3);
			this._tpNotes.Size = new System.Drawing.Size(840, 380);
			this._tpNotes.TabIndex = 1;
			this._tpNotes.Text = "Notes";
			this._tpNotes.UseVisualStyleBackColor = true;
			//
			// _tpHistory
			//
			this._tpHistory.Location = new System.Drawing.Point(4, 22);
			this._tpHistory.Name = "_tpHistory";
			this._tpHistory.Padding = new System.Windows.Forms.Padding(3);
			this._tpHistory.Size = new System.Drawing.Size(849, 438);
			this._tpHistory.TabIndex = 2;
			this._tpHistory.Text = "History";
			this._tpHistory.UseVisualStyleBackColor = true;
			//
			// _tpAbout
			//
			this._tpAbout.Location = new System.Drawing.Point(4, 22);
			this._tpAbout.Name = "_tpAbout";
			this._tpAbout.Padding = new System.Windows.Forms.Padding(3);
			this._tpAbout.Size = new System.Drawing.Size(849, 438);
			this._tpAbout.TabIndex = 3;
			this._tpAbout.Text = "About";
			this._tpAbout.UseVisualStyleBackColor = true;
			//
			// FwBridgeView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._tcPages);
			this.Controls.Add(this._sendReceiveButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._cbProjects);
			this.Name = "FwBridgeView";
			this.Size = new System.Drawing.Size(852, 456);
			this._tcPages.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _sendReceiveButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox _cbProjects;
		private System.Windows.Forms.TabControl _tcPages;
		private System.Windows.Forms.TabPage _tpNotes;
		private System.Windows.Forms.TabPage _tpHistory;
		private System.Windows.Forms.TabPage _tpAbout;
	}
}
