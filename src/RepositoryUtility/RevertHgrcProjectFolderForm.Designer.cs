namespace RepositoryUtility
{
	partial class RevertHgrcProjectFolderForm
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
			if(disposing && (components != null))
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
			this._listView = new System.Windows.Forms.ListView();
			this.FwdataPathname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._btnBrowse = new System.Windows.Forms.Button();
			this._fwdataPathname = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._hgVersion = new System.Windows.Forms.ListBox();
			this._okButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _listView
			// 
			this._listView.CheckBoxes = true;
			this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FwdataPathname});
			this._listView.Location = new System.Drawing.Point(21, 45);
			this._listView.Name = "_listView";
			this._listView.Size = new System.Drawing.Size(535, 318);
			this._listView.TabIndex = 25;
			this._listView.UseCompatibleStateImageBehavior = false;
			this._listView.View = System.Windows.Forms.View.Details;
			// 
			// FwdataPathname
			// 
			this.FwdataPathname.Text = "Project Path";
			this.FwdataPathname.Width = 475;
			// 
			// _btnBrowse
			// 
			this._btnBrowse.Location = new System.Drawing.Point(490, 15);
			this._btnBrowse.Name = "_btnBrowse";
			this._btnBrowse.Size = new System.Drawing.Size(66, 23);
			this._btnBrowse.TabIndex = 28;
			this._btnBrowse.Text = "Browse...";
			this._btnBrowse.UseVisualStyleBackColor = true;
			this._btnBrowse.Click += new System.EventHandler(this._btnBrowse_Click);
			// 
			// _fwdataPathname
			// 
			this._fwdataPathname.Enabled = false;
			this._fwdataPathname.Location = new System.Drawing.Point(99, 16);
			this._fwdataPathname.Name = "_fwdataPathname";
			this._fwdataPathname.Size = new System.Drawing.Size(385, 20);
			this._fwdataPathname.TabIndex = 27;
			this._fwdataPathname.Text = "C:\\ProgramData\\SIL\\FieldWorks\\TestProjects";
			this._fwdataPathname.WordWrap = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(18, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 13);
			this.label1.TabIndex = 26;
			this.label1.Text = "Project Folder:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(18, 381);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(111, 13);
			this.label2.TabIndex = 30;
			this.label2.Text = "Version of hg to target";
			// 
			// _hgVersion
			// 
			this._hgVersion.FormattingEnabled = true;
			this._hgVersion.Items.AddRange(new object[] {
            "1.5.1",
            "3.3"});
			this._hgVersion.Location = new System.Drawing.Point(135, 381);
			this._hgVersion.Name = "_hgVersion";
			this._hgVersion.Size = new System.Drawing.Size(120, 43);
			this._hgVersion.TabIndex = 29;
			// 
			// _okButton
			// 
			this._okButton.Location = new System.Drawing.Point(400, 427);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 31;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			// 
			// _cancelButton
			// 
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(481, 427);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 32;
			this._cancelButton.Text = "Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			// 
			// RevertHgrcProjectFolderForm
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(577, 465);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._hgVersion);
			this.Controls.Add(this._btnBrowse);
			this.Controls.Add(this._fwdataPathname);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._listView);
			this.MinimizeBox = false;
			this.Name = "RevertHgrcProjectFolderForm";
			this.ShowIcon = false;
			this.Text = "Revert Hgrc Files for Projects";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView _listView;
		private System.Windows.Forms.ColumnHeader FwdataPathname;
		private System.Windows.Forms.Button _btnBrowse;
		private System.Windows.Forms.TextBox _fwdataPathname;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox _hgVersion;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _cancelButton;
	}
}