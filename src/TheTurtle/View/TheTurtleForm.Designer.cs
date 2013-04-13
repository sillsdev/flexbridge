using FLEx_ChorusPlugin.Properties;

namespace TheTurtle.View
{
	partial class TheTurtleForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TheTurtleForm));
			this.SuspendLayout();

			_turtleView = new TurtleView();
			//
			// _turtleView
			//
			this._turtleView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._turtleView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._turtleView.Location = new System.Drawing.Point(0, 0);
			this._turtleView.Name = "_turtleView";
			this._turtleView.Size = new System.Drawing.Size(856, 520);
			this._turtleView.TabIndex = 0;
			//
			// TheTurtleForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(856, 520);
			this.Controls.Add(this._turtleView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "TheTurtleForm";
			this.Text = "The Turtle";
			this.ResumeLayout(false);

		}

		#endregion

		private TurtleView _turtleView;
	}
}