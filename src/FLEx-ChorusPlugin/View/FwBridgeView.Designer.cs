namespace FLEx_ChorusPlugin.View
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
			this._splitContainer = new System.Windows.Forms.SplitContainer();
			this._label1 = new System.Windows.Forms.Label();
			this._cbProjects = new System.Windows.Forms.ComboBox();
			this._projectView = new ProjectView();
			this._warninglabel1 = new System.Windows.Forms.Label();
			this._pictureBox = new System.Windows.Forms.PictureBox();
			this._warninglabel2 = new System.Windows.Forms.Label();
			this._label2 = new System.Windows.Forms.Label();
			this._tbComment = new System.Windows.Forms.TextBox();
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
			this._splitContainer.Panel1.Controls.Add(this._tbComment);
			this._splitContainer.Panel1.Controls.Add(this._label2);
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
			this._cbProjects.SelectedIndexChanged += new System.EventHandler(this.ProjectsSelectedIndexChanged);
			//
			// _projectView
			//
			this._projectView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._projectView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._projectView.Location = new System.Drawing.Point(0, 0);
			this._projectView.Name = "_projectView";
			this._projectView.Size = new System.Drawing.Size(852, 402);
			this._projectView.TabIndex = 0;
			//
			// _warninglabel1
			//
			this._warninglabel1.AutoSize = true;
			this._warninglabel1.Location = new System.Drawing.Point(365, 10);
			this._warninglabel1.Name = "_warninglabel1";
			this._warninglabel1.Size = new System.Drawing.Size(148, 13);
			this._warninglabel1.TabIndex = 22;
			this._warninglabel1.Text = "The selected project is in use,";
			//
			// _pictureBox
			//
			this._pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this._pictureBox.Location = new System.Drawing.Point(325, 9);
			this._pictureBox.Name = "_pictureBox";
			this._pictureBox.Size = new System.Drawing.Size(32, 30);
			this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._pictureBox.TabIndex = 23;
			this._pictureBox.TabStop = false;
			//
			// _warninglabel2
			//
			this._warninglabel2.AutoSize = true;
			this._warninglabel2.Location = new System.Drawing.Point(365, 31);
			this._warninglabel2.Name = "_warninglabel2";
			this._warninglabel2.Size = new System.Drawing.Size(107, 13);
			this._warninglabel2.TabIndex = 24;
			this._warninglabel2.Text = "so cannot be shared.";
			//
			// _label2
			//
			this._label2.AutoSize = true;
			this._label2.BackColor = System.Drawing.Color.Red;
			this._label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this._label2.ForeColor = System.Drawing.Color.Black;
			this._label2.Location = new System.Drawing.Point(517, 17);
			this._label2.Name = "label2";
			this._label2.Size = new System.Drawing.Size(56, 15);
			this._label2.TabIndex = 25;
			this._label2.Text = "Comment:";
			//
			// _tbComment
			//
			this._tbComment.Location = new System.Drawing.Point(577, 15);
			this._tbComment.Name = "_tbComment";
			this._tbComment.Size = new System.Drawing.Size(270, 20);
			this._tbComment.TabIndex = 26;
			this._tbComment.TextChanged += new System.EventHandler(this.CommentChanged);
			//
			// FwBridgeView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._splitContainer);
			this.Name = "FwBridgeView";
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
		private System.Windows.Forms.TextBox _tbComment;
		private System.Windows.Forms.Label _label2;

	}
}
