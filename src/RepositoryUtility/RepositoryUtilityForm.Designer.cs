// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

namespace RepositoryUtility
{
	partial class RepositoryUtilityForm
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
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				if (_chorusSystem != null)
					_chorusSystem.Dispose();
			}
			_chorusSystem = null;
			_repoFolder = null;

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.ToolStripSeparator toolStripMenuItemBlank1;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RepositoryUtilityForm));
			this._menuStrip = new System.Windows.Forms.MenuStrip();
			this.repositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openLocalRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.restoreToRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.updateToRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.sendBackToSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItemBlank1 = new System.Windows.Forms.ToolStripSeparator();
			this._menuStrip.SuspendLayout();
			this.SuspendLayout();
			//
			// toolStripMenuItemBlank1
			//
			toolStripMenuItemBlank1.Name = "toolStripMenuItemBlank1";
			toolStripMenuItemBlank1.Size = new System.Drawing.Size(184, 6);
			//
			// _menuStrip
			//
			this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.repositoryToolStripMenuItem});
			this._menuStrip.Location = new System.Drawing.Point(0, 0);
			this._menuStrip.Name = "_menuStrip";
			this._menuStrip.Size = new System.Drawing.Size(629, 24);
			this._menuStrip.TabIndex = 0;
			this._menuStrip.Text = "menuStrip";
			//
			// repositoryToolStripMenuItem
			//
			this.repositoryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.cloneToolStripMenuItem,
			this.openLocalRepositoryToolStripMenuItem,
			this.restoreToRevisionToolStripMenuItem,
			this.updateToRevisionToolStripMenuItem,
			this.sendBackToSourceToolStripMenuItem,
			toolStripMenuItemBlank1,
			this.exitToolStripMenuItem});
			this.repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
			this.repositoryToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
			this.repositoryToolStripMenuItem.Text = "&Repository";
			//
			// cloneToolStripMenuItem
			//
			this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
			this.cloneToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.cloneToolStripMenuItem.Text = "&Clone";
			this.cloneToolStripMenuItem.Click += new System.EventHandler(this.HandleCloneMenuClick);
			//
			// openLocalRepositoryToolStripMenuItem
			//
			this.openLocalRepositoryToolStripMenuItem.Name = "openLocalRepositoryToolStripMenuItem";
			this.openLocalRepositoryToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.openLocalRepositoryToolStripMenuItem.Text = "&Open local repository";
			this.openLocalRepositoryToolStripMenuItem.Click += new System.EventHandler(this.HandleOpenLocalRepositoryClick);
			//
			// restoreToRevisionToolStripMenuItem
			//
			this.restoreToRevisionToolStripMenuItem.Name = "restoreToRevisionToolStripMenuItem";
			this.restoreToRevisionToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.restoreToRevisionToolStripMenuItem.Text = "&Restore to revision";
			this.restoreToRevisionToolStripMenuItem.Click += new System.EventHandler(this.HandleRestoreToRevisionMenuClick);
			//
			// updateToRevisionToolStripMenuItem
			//
			this.updateToRevisionToolStripMenuItem.Name = "updateToRevisionToolStripMenuItem";
			this.updateToRevisionToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.updateToRevisionToolStripMenuItem.Text = "&Update to revision";
			this.updateToRevisionToolStripMenuItem.Click += new System.EventHandler(this.HandleUpdateToRevisionMenuClick);
			//
			// exitToolStripMenuItem
			//
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitMenuClick);
			//
			// sendBackToSourceToolStripMenuItem
			//
			this.sendBackToSourceToolStripMenuItem.Name = "sendBackToSourceToolStripMenuItem";
			this.sendBackToSourceToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.sendBackToSourceToolStripMenuItem.Text = "&Send Back to Source";
			this.sendBackToSourceToolStripMenuItem.Click += new System.EventHandler(this.HandleSendBackToSourceMenuClick);
			//
			// RepositoryUtilityForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(629, 396);
			this.Controls.Add(this._menuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this._menuStrip;
			this.Name = "RepositoryUtilityForm";
			this.Text = "Repository Utility";
			this._menuStrip.ResumeLayout(false);
			this._menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip _menuStrip;
		private System.Windows.Forms.ToolStripMenuItem repositoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem updateToRevisionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem restoreToRevisionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openLocalRepositoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem sendBackToSourceToolStripMenuItem;
	}
}
