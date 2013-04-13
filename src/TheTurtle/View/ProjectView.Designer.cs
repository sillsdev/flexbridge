namespace TheTurtle.View
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
			this._existingSystemView = new ExistingSystemView();
			this.SuspendLayout();
			//
			// _existingSystemView
			//
			this._existingSystemView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._existingSystemView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._existingSystemView.Location = new System.Drawing.Point(0, 0);
			this._existingSystemView.Margin = new System.Windows.Forms.Padding(0);
			this._existingSystemView.Name = "_existingSystemView";
			this._existingSystemView.Size = new System.Drawing.Size(686, 392);
			this._existingSystemView.TabIndex = 0;
			//
			// ProjectView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._existingSystemView);
			this.Name = "ProjectView";
			this.Size = new System.Drawing.Size(686, 392);
			this.ResumeLayout(false);

		}

		#endregion

		private ExistingSystemView _existingSystemView;
	}
}
