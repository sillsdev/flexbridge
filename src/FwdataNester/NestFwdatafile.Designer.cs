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
			this.components = new Container();
			ComponentResourceManager resources = new ComponentResourceManager(typeof(NestFwdataFile));
			this._openFileDialog = new OpenFileDialog();
			this._btnBrowse = new Button();
			this._fwdataPathname = new TextBox();
			this.label1 = new Label();
			this._btnRunSelected = new Button();
			this._cbNestFile = new CheckBox();
			this._cbRoundTripData = new CheckBox();
			this._cbVerify = new CheckBox();
			this._cbCheckOwnObjsur = new CheckBox();
			this._cbValidate = new CheckBox();
			this._rebuildDataFile = new CheckBox();
			this._btnRestoreProjects = new Button();
			this._btnClearCheckboxes = new Button();
			this._cbCheckAmbiguousElements = new CheckBox();
			this.revisionBox = new TextBox();
			this.revisionlabel = new Label();
			this.toolTip = new ToolTip(this.components);
			this.toolTip.InitialDelay = 100;
			this.toolTip.SetToolTip(this.revisionBox, "Set and check Rebuild data file to recover a revision from a repository.");
			this.toolTip.SetToolTip(this.revisionlabel, "Set and check Rebuild data file to recover a revision from a repository.");
			this.SuspendLayout();
			//
			// _openFileDialog
			//
			this._openFileDialog.Filter = "Fwdata Files|*.fwdata";
			//
			// _btnBrowse
			//
			this._btnBrowse.Location = new Point(479, 11);
			this._btnBrowse.Name = "_btnBrowse";
			this._btnBrowse.Size = new Size(66, 23);
			this._btnBrowse.TabIndex = 11;
			this._btnBrowse.Text = "Browse...";
			this._btnBrowse.UseVisualStyleBackColor = true;
			this._btnBrowse.Click += new EventHandler(this.BrowseForFile);
			//
			// _fwdataPathname
			//
			this._fwdataPathname.Enabled = false;
			this._fwdataPathname.Location = new Point(70, 12);
			this._fwdataPathname.Name = "_fwdataPathname";
			this._fwdataPathname.Size = new Size(403, 20);
			this._fwdataPathname.TabIndex = 10;
			this._fwdataPathname.WordWrap = false;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new Point(7, 16);
			this.label1.Name = "label1";
			this.label1.Size = new Size(58, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "fwdata file:";
			//
			// _btnRunSelected
			//
			this._btnRunSelected.Enabled = false;
			this._btnRunSelected.Location = new Point(7, 203);
			this._btnRunSelected.Name = "_btnRunSelected";
			this._btnRunSelected.Size = new Size(107, 23);
			this._btnRunSelected.TabIndex = 8;
			this._btnRunSelected.Text = "Run Selected";
			this._btnRunSelected.UseVisualStyleBackColor = true;
			this._btnRunSelected.Click += new EventHandler(this.RunSelected);
			//
			// _cbNestFile
			//
			this._cbNestFile.AutoSize = true;
			this._cbNestFile.Location = new Point(7, 42);
			this._cbNestFile.Name = "_cbNestFile";
			this._cbNestFile.Size = new Size(67, 17);
			this._cbNestFile.TabIndex = 12;
			this._cbNestFile.Text = "Nest File";
			this._cbNestFile.UseVisualStyleBackColor = true;
			//
			// _cbRoundTripData
			//
			this._cbRoundTripData.AutoSize = true;
			this._cbRoundTripData.Checked = true;
			this._cbRoundTripData.CheckState = CheckState.Checked;
			this._cbRoundTripData.Location = new Point(7, 66);
			this._cbRoundTripData.Name = "_cbRoundTripData";
			this._cbRoundTripData.Size = new Size(105, 17);
			this._cbRoundTripData.TabIndex = 13;
			this._cbRoundTripData.Text = "Round Trip Data";
			this._cbRoundTripData.UseVisualStyleBackColor = true;
			//
			// _cbVerify
			//
			this._cbVerify.AutoSize = true;
			this._cbVerify.Checked = true;
			this._cbVerify.CheckState = CheckState.Checked;
			this._cbVerify.Location = new Point(116, 66);
			this._cbVerify.Name = "_cbVerify";
			this._cbVerify.Size = new Size(102, 17);
			this._cbVerify.TabIndex = 14;
			this._cbVerify.Text = "Verify It Worked";
			this._cbVerify.UseVisualStyleBackColor = true;
			//
			// _cbCheckOwnObjsur
			//
			this._cbCheckOwnObjsur.AutoSize = true;
			this._cbCheckOwnObjsur.Location = new Point(80, 44);
			this._cbCheckOwnObjsur.Name = "_cbCheckOwnObjsur";
			this._cbCheckOwnObjsur.Size = new Size(85, 17);
			this._cbCheckOwnObjsur.TabIndex = 15;
			this._cbCheckOwnObjsur.Text = "Check t=\"o\"";
			this._cbCheckOwnObjsur.UseVisualStyleBackColor = true;
			//
			// _cbValidate
			//
			this._cbValidate.AutoSize = true;
			this._cbValidate.Checked = true;
			this._cbValidate.CheckState = CheckState.Checked;
			this._cbValidate.Location = new Point(222, 66);
			this._cbValidate.Name = "_cbValidate";
			this._cbValidate.Size = new Size(88, 17);
			this._cbValidate.TabIndex = 16;
			this._cbValidate.Text = "Validate Files";
			this._cbValidate.UseVisualStyleBackColor = true;
			//
			// _rebuildDataFile
			//
			this._rebuildDataFile.AutoSize = true;
			this._rebuildDataFile.Location = new Point(7, 167);
			this._rebuildDataFile.Name = "_rebuildDataFile";
			this._rebuildDataFile.Size = new Size(200, 17);
			this._rebuildDataFile.TabIndex = 17;
			this._rebuildDataFile.Text = "Rebuild fwdata file from spit data files";
			this._rebuildDataFile.UseVisualStyleBackColor = true;
			//
			// _btnRestoreProjects
			//
			this._btnRestoreProjects.Location = new Point(438, 73);
			this._btnRestoreProjects.Name = "_btnRestoreProjects";
			this._btnRestoreProjects.Size = new Size(107, 23);
			this._btnRestoreProjects.TabIndex = 18;
			this._btnRestoreProjects.Text = "Restore All Projects";
			this._btnRestoreProjects.UseVisualStyleBackColor = true;
			this._btnRestoreProjects.Click += new EventHandler(this.RestoreProjects);
			//
			// _btnClearCheckboxes
			//
			this._btnClearCheckboxes.Location = new Point(438, 42);
			this._btnClearCheckboxes.Name = "_btnClearCheckboxes";
			this._btnClearCheckboxes.Size = new Size(107, 23);
			this._btnClearCheckboxes.TabIndex = 19;
			this._btnClearCheckboxes.Text = "Clear Checkboxes";
			this._btnClearCheckboxes.UseVisualStyleBackColor = true;
			this._btnClearCheckboxes.Click += new EventHandler(this.ClearCheckboxes);
			//
			// _cbCheckAmbiguousElements
			//
			this._cbCheckAmbiguousElements.AutoSize = true;
			this._cbCheckAmbiguousElements.Checked = true;
			this._cbCheckAmbiguousElements.CheckState = CheckState.Checked;
			this._cbCheckAmbiguousElements.Location = new Point(25, 82);
			this._cbCheckAmbiguousElements.Name = "_cbCheckAmbiguousElements";
			this._cbCheckAmbiguousElements.Size = new Size(171, 17);
			this._cbCheckAmbiguousElements.TabIndex = 20;
			this._cbCheckAmbiguousElements.Text = "Check for ambiguous elements";
			this._cbCheckAmbiguousElements.UseVisualStyleBackColor = true;
			//
			// revisionBox
			//
			this.revisionBox.Location = new Point(70, 122);
			this.revisionBox.Name = "revisionBox";
			this.revisionBox.Size = new Size(100, 20);
			this.revisionBox.TabIndex = 21;
			//
			// revisionlabel
			//
			this.revisionlabel.AutoSize = true;
			this.revisionlabel.Location = new Point(7, 125);
			this.revisionlabel.Name = "revisionlabel";
			this.revisionlabel.Size = new Size(61, 13);
			this.revisionlabel.TabIndex = 22;
			this.revisionlabel.Text = "Revision #:";
			//
			// NestFwdataFile
			//
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.GradientInactiveCaption;
			this.ClientSize = new Size(554, 238);
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
			this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NestFwdataFile";
			this.Text = "Test an fwdata file";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenFileDialog _openFileDialog;
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
	}
}
