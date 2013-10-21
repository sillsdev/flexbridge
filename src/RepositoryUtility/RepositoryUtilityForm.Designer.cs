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
			System.Windows.Forms.ToolStripSeparator toolStripMenuItemBlank1;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RepositoryUtilityForm));
			this._menuStrip = new System.Windows.Forms.MenuStrip();
			this.repositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.updateToRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.restoreToRevisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItemBlank1 = new System.Windows.Forms.ToolStripSeparator();
			this._menuStrip.SuspendLayout();
			this.SuspendLayout();
			//
			// toolStripMenuItemBlank1
			//
			toolStripMenuItemBlank1.Name = "toolStripMenuItemBlank1";
			toolStripMenuItemBlank1.Size = new System.Drawing.Size(168, 6);
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
			this.viewHistoryToolStripMenuItem,
			this.updateToRevisionToolStripMenuItem,
			this.restoreToRevisionToolStripMenuItem,
			toolStripMenuItemBlank1,
			this.closeToolStripMenuItem,
			this.exitToolStripMenuItem});
			this.repositoryToolStripMenuItem.Name = "repositoryToolStripMenuItem";
			this.repositoryToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
			this.repositoryToolStripMenuItem.Text = "&Repository";
			//
			// cloneToolStripMenuItem
			//
			this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
			this.cloneToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.cloneToolStripMenuItem.Text = "&Clone";
			this.cloneToolStripMenuItem.Click += new System.EventHandler(this.HandleCloneMenuClick);
			//
			// viewHistoryToolStripMenuItem
			//
			this.viewHistoryToolStripMenuItem.Name = "viewHistoryToolStripMenuItem";
			this.viewHistoryToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.viewHistoryToolStripMenuItem.Text = "&View history";
			this.viewHistoryToolStripMenuItem.Click += new System.EventHandler(this.HandleViewHistoryMenuClick);
			//
			// updateToRevisionToolStripMenuItem
			//
			this.updateToRevisionToolStripMenuItem.Name = "updateToRevisionToolStripMenuItem";
			this.updateToRevisionToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.updateToRevisionToolStripMenuItem.Text = "&Update to revision";
			this.updateToRevisionToolStripMenuItem.Click += new System.EventHandler(this.HandleUpdateToRevisionMenuClick);
			//
			// restoreToRevisionToolStripMenuItem
			//
			this.restoreToRevisionToolStripMenuItem.Name = "restoreToRevisionToolStripMenuItem";
			this.restoreToRevisionToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.restoreToRevisionToolStripMenuItem.Text = "&Restore to revision";
			this.restoreToRevisionToolStripMenuItem.Click += new System.EventHandler(this.HandleRestoreToRevisionMenuClick);
			//
			// closeToolStripMenuItem
			//
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.closeToolStripMenuItem.Text = "C&lose repository";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.HandleCloseRepositoryClick);
			//
			// exitToolStripMenuItem
			//
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitMenuClick);
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
		private System.Windows.Forms.ToolStripMenuItem viewHistoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem updateToRevisionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem restoreToRevisionToolStripMenuItem;
	}
}
