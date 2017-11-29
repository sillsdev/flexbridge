// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using TriboroughBridge_ChorusPlugin;

namespace FwdataTestApp
{
	partial class NestFwdataFile
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NestFwdataFile));
			this._btnBrowse = new System.Windows.Forms.Button();
			this._fwdataPathname = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._btnRunSelected = new System.Windows.Forms.Button();
			this._cbNestFile = new System.Windows.Forms.CheckBox();
			this._cbRoundTripData = new System.Windows.Forms.CheckBox();
			this._cbVerify = new System.Windows.Forms.CheckBox();
			this._cbCheckOwnObjsur = new System.Windows.Forms.CheckBox();
			this._cbValidate = new System.Windows.Forms.CheckBox();
			this._rebuildDataFile = new System.Windows.Forms.CheckBox();
			this._btnRestoreProjects = new System.Windows.Forms.Button();
			this._btnClearCheckboxes = new System.Windows.Forms.Button();
			this._cbCheckAmbiguousElements = new System.Windows.Forms.CheckBox();
			this.revisionBox = new System.Windows.Forms.TextBox();
			this.revisionlabel = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._btnRunLoop = new System.Windows.Forms.Button();
			this._folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this._listView = new System.Windows.Forms.ListView();
			this.FwdataPathname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._btnToggle = new System.Windows.Forms.Button();
			this._cbFindDanglingRefs = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			//
			// _btnBrowse
			//
			this._btnBrowse.Location = new System.Drawing.Point(479, 11);
			this._btnBrowse.Name = "_btnBrowse";
			this._btnBrowse.Size = new System.Drawing.Size(66, 23);
			this._btnBrowse.TabIndex = 11;
			this._btnBrowse.Text = "Browse...";
			this._btnBrowse.UseVisualStyleBackColor = true;
			this._btnBrowse.Click += new System.EventHandler(this.BrowseForFolder);
			//
			// _fwdataPathname
			//
			this._fwdataPathname.Enabled = false;
			this._fwdataPathname.Location = new System.Drawing.Point(88, 12);
			this._fwdataPathname.Name = "_fwdataPathname";
			this._fwdataPathname.Size = new System.Drawing.Size(385, 20);
			this._fwdataPathname.TabIndex = 10;
			this._fwdataPathname.Text = "C:\\ProgramData\\SIL\\FieldWorks\\TestProjects";
			this._fwdataPathname.WordWrap = false;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Project Folder:";
			//
			// _btnRunSelected
			//
			this._btnRunSelected.Enabled = false;
			this._btnRunSelected.Location = new System.Drawing.Point(-1, 629);
			this._btnRunSelected.Name = "_btnRunSelected";
			this._btnRunSelected.Size = new System.Drawing.Size(107, 23);
			this._btnRunSelected.TabIndex = 8;
			this._btnRunSelected.Text = "Run Selected";
			this._btnRunSelected.UseVisualStyleBackColor = true;
			this._btnRunSelected.Click += new System.EventHandler(this.RunSelected);
			//
			// _cbNestFile
			//
			this._cbNestFile.AutoSize = true;
			this._cbNestFile.Location = new System.Drawing.Point(-1, 403);
			this._cbNestFile.Name = "_cbNestFile";
			this._cbNestFile.Size = new System.Drawing.Size(67, 17);
			this._cbNestFile.TabIndex = 12;
			this._cbNestFile.Text = "Nest File";
			this._cbNestFile.UseVisualStyleBackColor = true;
			//
			// _cbRoundTripData
			//
			this._cbRoundTripData.AutoSize = true;
			this._cbRoundTripData.Checked = true;
			this._cbRoundTripData.CheckState = System.Windows.Forms.CheckState.Checked;
			this._cbRoundTripData.Location = new System.Drawing.Point(-1, 427);
			this._cbRoundTripData.Name = "_cbRoundTripData";
			this._cbRoundTripData.Size = new System.Drawing.Size(105, 17);
			this._cbRoundTripData.TabIndex = 13;
			this._cbRoundTripData.Text = "Round Trip Data";
			this._cbRoundTripData.UseVisualStyleBackColor = true;
			//
			// _cbVerify
			//
			this._cbVerify.AutoSize = true;
			this._cbVerify.Checked = true;
			this._cbVerify.CheckState = System.Windows.Forms.CheckState.Checked;
			this._cbVerify.Location = new System.Drawing.Point(108, 427);
			this._cbVerify.Name = "_cbVerify";
			this._cbVerify.Size = new System.Drawing.Size(102, 17);
			this._cbVerify.TabIndex = 14;
			this._cbVerify.Text = "Verify It Worked";
			this._cbVerify.UseVisualStyleBackColor = true;
			//
			// _cbCheckOwnObjsur
			//
			this._cbCheckOwnObjsur.AutoSize = true;
			this._cbCheckOwnObjsur.Location = new System.Drawing.Point(72, 405);
			this._cbCheckOwnObjsur.Name = "_cbCheckOwnObjsur";
			this._cbCheckOwnObjsur.Size = new System.Drawing.Size(85, 17);
			this._cbCheckOwnObjsur.TabIndex = 15;
			this._cbCheckOwnObjsur.Text = "Check t=\"o\"";
			this._cbCheckOwnObjsur.UseVisualStyleBackColor = true;
			//
			// _cbValidate
			//
			this._cbValidate.AutoSize = true;
			this._cbValidate.Checked = true;
			this._cbValidate.CheckState = System.Windows.Forms.CheckState.Checked;
			this._cbValidate.Location = new System.Drawing.Point(214, 427);
			this._cbValidate.Name = "_cbValidate";
			this._cbValidate.Size = new System.Drawing.Size(88, 17);
			this._cbValidate.TabIndex = 16;
			this._cbValidate.Text = "Validate Files";
			this._cbValidate.UseVisualStyleBackColor = true;
			//
			// _rebuildDataFile
			//
			this._rebuildDataFile.AutoSize = true;
			this._rebuildDataFile.Location = new System.Drawing.Point(-1, 487);
			this._rebuildDataFile.Name = "_rebuildDataFile";
			this._rebuildDataFile.Size = new System.Drawing.Size(200, 17);
			this._rebuildDataFile.TabIndex = 17;
			this._rebuildDataFile.Text = "Rebuild fwdata file from spit data files";
			this._rebuildDataFile.UseVisualStyleBackColor = true;
			//
			// _btnRestoreProjects
			//
			this._btnRestoreProjects.Location = new System.Drawing.Point(430, 434);
			this._btnRestoreProjects.Name = "_btnRestoreProjects";
			this._btnRestoreProjects.Size = new System.Drawing.Size(107, 23);
			this._btnRestoreProjects.TabIndex = 18;
			this._btnRestoreProjects.Text = "Restore All Projects";
			this._btnRestoreProjects.UseVisualStyleBackColor = true;
			this._btnRestoreProjects.Click += new System.EventHandler(this.RestoreProjects);
			//
			// _btnClearCheckboxes
			//
			this._btnClearCheckboxes.Location = new System.Drawing.Point(430, 403);
			this._btnClearCheckboxes.Name = "_btnClearCheckboxes";
			this._btnClearCheckboxes.Size = new System.Drawing.Size(107, 23);
			this._btnClearCheckboxes.TabIndex = 19;
			this._btnClearCheckboxes.Text = "Clear Checkboxes";
			this._btnClearCheckboxes.UseVisualStyleBackColor = true;
			this._btnClearCheckboxes.Click += new System.EventHandler(this.ClearCheckboxes);
			//
			// _cbCheckAmbiguousElements
			//
			this._cbCheckAmbiguousElements.AutoSize = true;
			this._cbCheckAmbiguousElements.Checked = true;
			this._cbCheckAmbiguousElements.CheckState = System.Windows.Forms.CheckState.Checked;
			this._cbCheckAmbiguousElements.Location = new System.Drawing.Point(17, 443);
			this._cbCheckAmbiguousElements.Name = "_cbCheckAmbiguousElements";
			this._cbCheckAmbiguousElements.Size = new System.Drawing.Size(171, 17);
			this._cbCheckAmbiguousElements.TabIndex = 20;
			this._cbCheckAmbiguousElements.Text = "Check for ambiguous elements";
			this._cbCheckAmbiguousElements.UseVisualStyleBackColor = true;
			//
			// revisionBox
			//
			this.revisionBox.Location = new System.Drawing.Point(62, 464);
			this.revisionBox.Name = "revisionBox";
			this.revisionBox.Size = new System.Drawing.Size(100, 20);
			this.revisionBox.TabIndex = 21;
			this.toolTip.SetToolTip(this.revisionBox, "Set and check Rebuild data file to recover a revision from a repository.");
			//
			// revisionlabel
			//
			this.revisionlabel.AutoSize = true;
			this.revisionlabel.Location = new System.Drawing.Point(-1, 467);
			this.revisionlabel.Name = "revisionlabel";
			this.revisionlabel.Size = new System.Drawing.Size(61, 13);
			this.revisionlabel.TabIndex = 22;
			this.revisionlabel.Text = "Revision #:";
			this.toolTip.SetToolTip(this.revisionlabel, "Set and check Rebuild data file to recover a revision from a repository.");
			//
			// toolTip
			//
			this.toolTip.AutoPopDelay = 5000;
			this.toolTip.InitialDelay = 100;
			this.toolTip.ReshowDelay = 100;
			//
			// _btnRunLoop
			//
			this._btnRunLoop.Location = new System.Drawing.Point(140, 629);
			this._btnRunLoop.Name = "_btnRunLoop";
			this._btnRunLoop.Size = new System.Drawing.Size(75, 23);
			this._btnRunLoop.TabIndex = 23;
			this._btnRunLoop.Text = "Run Loop";
			this._btnRunLoop.UseVisualStyleBackColor = true;
			this._btnRunLoop.Click += new System.EventHandler(this.RunLoopClicked);
			//
			// _folderBrowserDialog
			//
			this._folderBrowserDialog.Description = "Select FW Project Folder";
			this._folderBrowserDialog.SelectedPath = "C:\\ProgramData\\SIL\\FieldWorks\\TestProjects";
			this._folderBrowserDialog.ShowNewFolderButton = false;
			//
			// _listView
			//
			this._listView.CheckBoxes = true;
			this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.FwdataPathname});
			this._listView.Location = new System.Drawing.Point(10, 79);
			this._listView.Name = "_listView";
			this._listView.Size = new System.Drawing.Size(527, 318);
			this._listView.TabIndex = 24;
			this._listView.UseCompatibleStateImageBehavior = false;
			this._listView.View = System.Windows.Forms.View.Details;
			//
			// FwdataPathname
			//
			this.FwdataPathname.Text = "FW Fathname";
			this.FwdataPathname.Width = 475;
			//
			// _btnToggle
			//
			this._btnToggle.Location = new System.Drawing.Point(13, 49);
			this._btnToggle.Name = "_btnToggle";
			this._btnToggle.Size = new System.Drawing.Size(75, 23);
			this._btnToggle.TabIndex = 25;
			this._btnToggle.Text = "Toggle";
			this._btnToggle.UseVisualStyleBackColor = true;
			this._btnToggle.Click += new System.EventHandler(this.ToggleCheckBoxes);
			//
			// _cbFindDanglingRefs
			//
			this._cbFindDanglingRefs.AutoSize = true;
			this._cbFindDanglingRefs.Location = new System.Drawing.Point(-1, 510);
			this._cbFindDanglingRefs.Name = "_cbFindDanglingRefs";
			this._cbFindDanglingRefs.Size = new System.Drawing.Size(218, 17);
			this._cbFindDanglingRefs.TabIndex = 26;
			this._cbFindDanglingRefs.Text = "Find dangling reference props in main file";
			this._cbFindDanglingRefs.UseVisualStyleBackColor = true;
			//
			// NestFwdataFile
			//
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(554, 657);
			this.Controls.Add(this._cbFindDanglingRefs);
			this.Controls.Add(this._btnToggle);
			this.Controls.Add(this._listView);
			this.Controls.Add(this._btnRunLoop);
			this.Controls.Add(this.revisionlabel);
			this.Controls.Add(this.revisionBox);
			this.Controls.Add(this._cbCheckAmbiguousElements);
			this.Controls.Add(this._btnClearCheckboxes);
			this.Controls.Add(this._btnRestoreProjects);
			this.Controls.Add(this._rebuildDataFile);
			this.Controls.Add(this._cbValidate);
			this.Controls.Add(this._cbCheckOwnObjsur);
			this.Controls.Add(this._cbVerify);
			this.Controls.Add(this._cbRoundTripData);
			this.Controls.Add(this._cbNestFile);
			this.Controls.Add(this._btnBrowse);
			this.Controls.Add(this._fwdataPathname);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._btnRunSelected);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NestFwdataFile";
			this.Text = "Test an fwdata file";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Button _btnBrowse;
		private TextBox _fwdataPathname;
		private Label label1;
		private Button _btnRunSelected;
		private CheckBox _cbNestFile;
		private CheckBox _cbRoundTripData;
		private CheckBox _cbVerify;
		private CheckBox _cbCheckOwnObjsur;
		private CheckBox _cbValidate;
		private CheckBox _rebuildDataFile;
		private Button _btnRestoreProjects;
		private Button _btnClearCheckboxes;
		private CheckBox _cbCheckAmbiguousElements;
		private TextBox revisionBox;
		private Label revisionlabel;
		private ToolTip toolTip;
		private Button _btnRunLoop;
		private FolderBrowserDialog _folderBrowserDialog;
		private ListView _listView;
		private ColumnHeader FwdataPathname;
		private Button _btnToggle;
		private CheckBox _cbFindDanglingRefs;
	}
}
