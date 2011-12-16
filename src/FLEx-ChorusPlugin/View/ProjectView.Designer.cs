namespace FLEx_ChorusPlugin.View
{
	partial class ProjectView
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
			this._startupNewView = new StartupNewView();
			this._existingSystemView = new ExistingSystemView();
			this.SuspendLayout();
			//
			// _startupNew
			//
			this._startupNewView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._startupNewView.Location = new System.Drawing.Point(3, 3);
			this._startupNewView.MinimumSize = new System.Drawing.Size(332, 158);
			this._startupNewView.Name = "_startupNewView";
			this._startupNewView.Size = new System.Drawing.Size(392, 253);
			this._startupNewView.TabIndex = 1;
			//
			// _existingSystem
			//
			this._existingSystemView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._existingSystemView.Location = new System.Drawing.Point(101, 3);
			this._existingSystemView.Name = "_existingSystemView";
			this._existingSystemView.Size = new System.Drawing.Size(584, 366);
			this._existingSystemView.TabIndex = 0;
			//
			// ProjectControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._startupNewView);
			this.Controls.Add(this._existingSystemView);
			this.Name = "ProjectView";
			this.Size = new System.Drawing.Size(686, 392);
			this.ResumeLayout(false);

		}

		#endregion

		private ExistingSystemView _existingSystemView;
		private StartupNewView _startupNewView;
	}
}
