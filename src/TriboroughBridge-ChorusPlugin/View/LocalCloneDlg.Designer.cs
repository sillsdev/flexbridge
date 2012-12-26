namespace TriboroughBridge_ChorusPlugin.View
{
	partial class LocalCloneDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalCloneDlg));
			this.logControl1 = new TriboroughBridge_ChorusPlugin.View.LogControl();
			this.SuspendLayout();
			//
			// logControl1
			//
			this.logControl1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.logControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logControl1.Location = new System.Drawing.Point(0, 0);
			this.logControl1.Name = "logControl1";
			this.logControl1.Size = new System.Drawing.Size(351, 428);
			this.logControl1.TabIndex = 0;
			//
			// LocalCloneDlg
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(351, 428);
			this.Controls.Add(this.logControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LocalCloneDlg";
			this.ShowInTaskbar = false;
			this.Text = "Move system";
			this.ResumeLayout(false);

		}

		#endregion

		private LogControl logControl1;

	}
}