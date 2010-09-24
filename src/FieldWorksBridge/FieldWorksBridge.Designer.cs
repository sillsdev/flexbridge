namespace FieldWorksBridge
{
	partial class FieldWorksBridge
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldWorksBridge));
			this._cbProjects = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this._tcPages = new System.Windows.Forms.TabControl();
			this._tpSendReceive = new System.Windows.Forms.TabPage();
			this._tpNotes = new System.Windows.Forms.TabPage();
			this._tpHistory = new System.Windows.Forms.TabPage();
			this._tpAbout = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this._tbComment = new System.Windows.Forms.TextBox();
			this._tpSettings = new System.Windows.Forms.TabPage();
			this._tcPages.SuspendLayout();
			this.SuspendLayout();
			//
			// _cbProjects
			//
			this._cbProjects.FormattingEnabled = true;
			this._cbProjects.Location = new System.Drawing.Point(70, 16);
			this._cbProjects.Name = "_cbProjects";
			this._cbProjects.Size = new System.Drawing.Size(214, 21);
			this._cbProjects.TabIndex = 0;
			this._cbProjects.SelectedIndexChanged += new System.EventHandler(this.SelectedProjectIndexChanged);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Project";
			//
			// _tcPages
			//
			this._tcPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._tcPages.Controls.Add(this._tpSendReceive);
			this._tcPages.Controls.Add(this._tpSettings);
			this._tcPages.Controls.Add(this._tpNotes);
			this._tcPages.Controls.Add(this._tpHistory);
			this._tcPages.Controls.Add(this._tpAbout);
			this._tcPages.Location = new System.Drawing.Point(2, 55);
			this._tcPages.Name = "_tcPages";
			this._tcPages.SelectedIndex = 0;
			this._tcPages.Size = new System.Drawing.Size(857, 464);
			this._tcPages.TabIndex = 2;
			//
			// _tpSendReceive
			//
			this._tpSendReceive.BackColor = System.Drawing.Color.Transparent;
			this._tpSendReceive.Location = new System.Drawing.Point(4, 22);
			this._tpSendReceive.Name = "_tpSendReceive";
			this._tpSendReceive.Padding = new System.Windows.Forms.Padding(3);
			this._tpSendReceive.Size = new System.Drawing.Size(849, 438);
			this._tpSendReceive.TabIndex = 0;
			this._tpSendReceive.Text = "Send/Receive";
			this._tpSendReceive.UseVisualStyleBackColor = true;
			//
			// _tpNotes
			//
			this._tpNotes.Location = new System.Drawing.Point(4, 22);
			this._tpNotes.Name = "_tpNotes";
			this._tpNotes.Padding = new System.Windows.Forms.Padding(3);
			this._tpNotes.Size = new System.Drawing.Size(849, 438);
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
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(304, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(92, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Optional comment";
			//
			// _tbComment
			//
			this._tbComment.Location = new System.Drawing.Point(403, 16);
			this._tbComment.Name = "_tbComment";
			this._tbComment.Size = new System.Drawing.Size(390, 20);
			this._tbComment.TabIndex = 4;
			this._tbComment.Leave += new System.EventHandler(this.LeaveTextBox);
			//
			// _tpSettings
			//
			this._tpSettings.Location = new System.Drawing.Point(4, 22);
			this._tpSettings.Name = "_tpSettings";
			this._tpSettings.Padding = new System.Windows.Forms.Padding(3);
			this._tpSettings.Size = new System.Drawing.Size(849, 438);
			this._tpSettings.TabIndex = 4;
			this._tpSettings.Text = "Settings";
			this._tpSettings.UseVisualStyleBackColor = true;
			//
			// FieldWorksBridge
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(856, 520);
			this.Controls.Add(this._tbComment);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._tcPages);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._cbProjects);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FieldWorksBridge";
			this.Text = "FieldWorks Bridge";
			this.Load += new System.EventHandler(this.LoadForm);
			this._tcPages.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox _cbProjects;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabControl _tcPages;
		private System.Windows.Forms.TabPage _tpSendReceive;
		private System.Windows.Forms.TabPage _tpNotes;
		private System.Windows.Forms.TabPage _tpHistory;
		private System.Windows.Forms.TabPage _tpAbout;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _tbComment;
		private System.Windows.Forms.TabPage _tpSettings;
	}
}
