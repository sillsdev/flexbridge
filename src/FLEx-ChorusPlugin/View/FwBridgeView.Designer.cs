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
			this._sendReceiveButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._cbProjects = new System.Windows.Forms.ComboBox();
			this._projectView = new ProjectView();
			this._warninglabel1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._warninglabel2 = new System.Windows.Forms.Label();
			this._splitContainer.Panel1.SuspendLayout();
			this._splitContainer.Panel2.SuspendLayout();
			this._splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
			this._splitContainer.Panel1.Controls.Add(this.pictureBox1);
			this._splitContainer.Panel1.Controls.Add(this._warninglabel1);
			this._splitContainer.Panel1.Controls.Add(this._sendReceiveButton);
			this._splitContainer.Panel1.Controls.Add(this.label1);
			this._splitContainer.Panel1.Controls.Add(this._cbProjects);
			//
			// _splitContainer.Panel2
			//
			this._splitContainer.Panel2.Controls.Add(this._projectView);
			this._splitContainer.Size = new System.Drawing.Size(852, 456);
			this._splitContainer.TabIndex = 0;
			//
			// _sendReceiveButton
			//
			this._sendReceiveButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._sendReceiveButton.Image = ((System.Drawing.Image)(resources.GetObject("_sendReceiveButton.Image")));
			this._sendReceiveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._sendReceiveButton.Location = new System.Drawing.Point(337, 5);
			this._sendReceiveButton.Name = "_sendReceiveButton";
			this._sendReceiveButton.Size = new System.Drawing.Size(132, 38);
			this._sendReceiveButton.TabIndex = 21;
			this._sendReceiveButton.Text = "Send/Receive";
			this._sendReceiveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._sendReceiveButton.UseVisualStyleBackColor = false;
			this._sendReceiveButton.Click += new System.EventHandler(this.SendReceiveButtonClick);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 13);
			this.label1.TabIndex = 20;
			this.label1.Text = "Project";
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
			this._warninglabel1.Location = new System.Drawing.Point(515, 10);
			this._warninglabel1.Name = "_warninglabel1";
			this._warninglabel1.Size = new System.Drawing.Size(148, 13);
			this._warninglabel1.TabIndex = 22;
			this._warninglabel1.Text = "The selected project is in use,";
			//
			// pictureBox1
			//
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(475, 9);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 30);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 23;
			this.pictureBox1.TabStop = false;
			//
			// _warninglabel2
			//
			this._warninglabel2.AutoSize = true;
			this._warninglabel2.Location = new System.Drawing.Point(515, 31);
			this._warninglabel2.Name = "_warninglabel2";
			this._warninglabel2.Size = new System.Drawing.Size(107, 13);
			this._warninglabel2.TabIndex = 24;
			this._warninglabel2.Text = "so cannot be shared.";
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
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer _splitContainer;
		private System.Windows.Forms.Button _sendReceiveButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox _cbProjects;
		private ProjectView _projectView;
		private System.Windows.Forms.Label _warninglabel1;
		private System.Windows.Forms.Label _warninglabel2;
		private System.Windows.Forms.PictureBox pictureBox1;

	}
}
