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
			this._tpNotes = new System.Windows.Forms.TabPage();
			this._tpHistory = new System.Windows.Forms.TabPage();
			this._tpAbout = new System.Windows.Forms.TabPage();
			this._sendReceiveButton = new System.Windows.Forms.Button();
			this._tcPages.SuspendLayout();
			this.SuspendLayout();
			//
			// _cbProjects
			//
			this._cbProjects.FormattingEnabled = true;
			this._cbProjects.Location = new System.Drawing.Point(70, 16);
			this._cbProjects.Name = "_cbProjects";
			this._cbProjects.Size = new System.Drawing.Size(265, 21);
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
			this._tcPages.Controls.Add(this._tpNotes);
			this._tcPages.Controls.Add(this._tpHistory);
			this._tcPages.Controls.Add(this._tpAbout);
			this._tcPages.Location = new System.Drawing.Point(2, 55);
			this._tcPages.Name = "_tcPages";
			this._tcPages.SelectedIndex = 0;
			this._tcPages.Size = new System.Drawing.Size(857, 464);
			this._tcPages.TabIndex = 2;
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
			// _sendReceiveButton
			//
			this._sendReceiveButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._sendReceiveButton.Image = ((System.Drawing.Image)(resources.GetObject("_sendReceiveButton.Image")));
			this._sendReceiveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._sendReceiveButton.Location = new System.Drawing.Point(362, 16);
			this._sendReceiveButton.Name = "_sendReceiveButton";
			this._sendReceiveButton.Size = new System.Drawing.Size(132, 38);
			this._sendReceiveButton.TabIndex = 15;
			this._sendReceiveButton.Text = "Send/Receive";
			this._sendReceiveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._sendReceiveButton.UseVisualStyleBackColor = false;
			//
			// FieldWorksBridge
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(856, 520);
			this.Controls.Add(this._sendReceiveButton);
			this.Controls.Add(this._tcPages);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._cbProjects);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(525, 490);
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
		private System.Windows.Forms.TabPage _tpNotes;
		private System.Windows.Forms.TabPage _tpHistory;
		private System.Windows.Forms.TabPage _tpAbout;
		private System.Windows.Forms.Button _sendReceiveButton;
	}
}
