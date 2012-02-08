namespace FLEx_ChorusPlugin.View
{
	internal partial class FwBridgeConflictView
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
			this._splitContainer = new System.Windows.Forms.SplitContainer();
			this._warninglabel2 = new System.Windows.Forms.Label();
			this._pictureBox = new System.Windows.Forms.PictureBox();
			this._warninglabel1 = new System.Windows.Forms.Label();
			this._label1 = new System.Windows.Forms.Label();
			this._cbProjects = new System.Windows.Forms.ComboBox();
			this._splitContainer.Panel1.SuspendLayout();
			this._splitContainer.Panel2.SuspendLayout();
			this._splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
			this.SuspendLayout();
			//
			// _splitContainer
			//
			this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this._splitContainer.IsSplitterFixed = true;
			this._splitContainer.Location = new System.Drawing.Point(0, 0);
			this._splitContainer.Name = "_splitContainer";
			this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// _splitContainer.Panel1
			//
			this._splitContainer.Panel1.Controls.Add(this._warninglabel2);
			this._splitContainer.Panel1.Controls.Add(this._pictureBox);
			this._splitContainer.Panel1.Controls.Add(this._warninglabel1);
			this._splitContainer.Panel1.Controls.Add(this._label1);
			this._splitContainer.Panel1.Controls.Add(this._cbProjects);
			//
			// _splitContainer.Panel2
			//
			this._splitContainer.Panel2.Controls.Add(this._projectView);
			this._splitContainer.Size = new System.Drawing.Size(852, 456);
			this._splitContainer.TabIndex = 0;
			//
			// _warninglabel2
			//
			this._warninglabel2.AutoSize = true;
			this._warninglabel2.Location = new System.Drawing.Point(382, 27);
			this._warninglabel2.Name = "_warninglabel2";
			this._warninglabel2.Size = new System.Drawing.Size(107, 13);
			this._warninglabel2.TabIndex = 24;
			this._warninglabel2.Text = "so cannot be shared.";
			//
			// _pictureBox
			//
			this._pictureBox.Location = new System.Drawing.Point(342, 5);
			this._pictureBox.Name = "_pictureBox";
			this._pictureBox.Size = new System.Drawing.Size(32, 30);
			this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._pictureBox.TabIndex = 23;
			this._pictureBox.TabStop = false;
			//
			// _warninglabel1
			//
			this._warninglabel1.AutoSize = true;
			this._warninglabel1.Location = new System.Drawing.Point(382, 6);
			this._warninglabel1.Name = "_warninglabel1";
			this._warninglabel1.Size = new System.Drawing.Size(148, 13);
			this._warninglabel1.TabIndex = 22;
			this._warninglabel1.Text = "The selected project is in use,";
			//
			// _label1
			//
			this._label1.AutoSize = true;
			this._label1.Location = new System.Drawing.Point(7, 18);
			this._label1.Name = "_label1";
			this._label1.Size = new System.Drawing.Size(40, 13);
			this._label1.TabIndex = 20;
			this._label1.Text = "Project";
			//
			// _cbProjects
			//
			this._cbProjects.FormattingEnabled = true;
			this._cbProjects.Location = new System.Drawing.Point(52, 14);
			this._cbProjects.Name = "_cbProjects";
			this._cbProjects.Size = new System.Drawing.Size(265, 21);
			this._cbProjects.TabIndex = 19;
			//
			// _projectView
			//
			this._projectView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._projectView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._projectView.Location = new System.Drawing.Point(0, 0);
			this._projectView.Name = "_projectView";
			this._projectView.Size = new System.Drawing.Size(852, 402);
			this._projectView.TabIndex = 0;
			this._projectView.Load += new System.EventHandler(this._projectView_Load);
			//
			// FwBridgeConflictView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._splitContainer);
			this.Name = "FwBridgeConflictView";
			this.Size = new System.Drawing.Size(852, 456);
			this._splitContainer.Panel1.ResumeLayout(false);
			this._splitContainer.Panel1.PerformLayout();
			this._splitContainer.Panel2.ResumeLayout(false);
			this._splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer _splitContainer;
		private System.Windows.Forms.Label _label1;
		private System.Windows.Forms.ComboBox _cbProjects;
		private ProjectView _projectView;
		private System.Windows.Forms.Label _warninglabel1;
		private System.Windows.Forms.Label _warninglabel2;
		private System.Windows.Forms.PictureBox _pictureBox;

	}
}
