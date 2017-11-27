// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

namespace LiftFileCheckerApp
{
	partial class LiftFileCheckerApp
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
			this._btnBrowse = new System.Windows.Forms.Button();
			this._btnCheckfile = new System.Windows.Forms.Button();
			this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			//
			// _btnBrowse
			//
			this._btnBrowse.Location = new System.Drawing.Point(24, 24);
			this._btnBrowse.Name = "_btnBrowse";
			this._btnBrowse.Size = new System.Drawing.Size(124, 23);
			this._btnBrowse.TabIndex = 0;
			this._btnBrowse.Text = "Browse for Lift File";
			this._btnBrowse.UseVisualStyleBackColor = true;
			this._btnBrowse.Click += new System.EventHandler(this.BrowseButtonClicked);
			//
			// _btnCheckfile
			//
			this._btnCheckfile.Location = new System.Drawing.Point(24, 66);
			this._btnCheckfile.Name = "_btnCheckfile";
			this._btnCheckfile.Size = new System.Drawing.Size(75, 23);
			this._btnCheckfile.TabIndex = 1;
			this._btnCheckfile.Text = "Test file";
			this._btnCheckfile.UseVisualStyleBackColor = true;
			this._btnCheckfile.Click += new System.EventHandler(this.TestFileButtonClicked);
			//
			// _openFileDialog
			//
			this._openFileDialog.Filter = "Lift files|*.lift";
			this._openFileDialog.Title = "Select a Lift file";
			//
			// LiftFileCheckerApp
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(370, 206);
			this.Controls.Add(this._btnCheckfile);
			this.Controls.Add(this._btnBrowse);
			this.Name = "LiftFileCheckerApp";
			this.Text = "Lift File Checker";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _btnBrowse;
		private System.Windows.Forms.Button _btnCheckfile;
		private System.Windows.Forms.OpenFileDialog _openFileDialog;
	}
}
