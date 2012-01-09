namespace FwdataNester
{
	partial class NestFwdataFile
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
			this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.label1 = new System.Windows.Forms.Label();
			this._fwdataPathname = new System.Windows.Forms.TextBox();
			this._btnBrowse = new System.Windows.Forms.Button();
			this._btnNest = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _openFileDialog
			//
			this._openFileDialog.Filter = "Fwdata Files|*.fwdata";
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "fwdata file:";
			//
			// _fwdataPathname
			//
			this._fwdataPathname.Enabled = false;
			this._fwdataPathname.Location = new System.Drawing.Point(78, 9);
			this._fwdataPathname.Name = "_fwdataPathname";
			this._fwdataPathname.Size = new System.Drawing.Size(381, 20);
			this._fwdataPathname.TabIndex = 1;
			this._fwdataPathname.WordWrap = false;
			//
			// _btnBrowse
			//
			this._btnBrowse.Location = new System.Drawing.Point(465, 8);
			this._btnBrowse.Name = "_btnBrowse";
			this._btnBrowse.Size = new System.Drawing.Size(75, 23);
			this._btnBrowse.TabIndex = 2;
			this._btnBrowse.Text = "Browse...";
			this._btnBrowse.UseVisualStyleBackColor = true;
			this._btnBrowse.Click += new System.EventHandler(this.BrowseForFile);
			//
			// _btnNest
			//
			this._btnNest.Enabled = false;
			this._btnNest.Location = new System.Drawing.Point(13, 47);
			this._btnNest.Name = "_btnNest";
			this._btnNest.Size = new System.Drawing.Size(75, 23);
			this._btnNest.TabIndex = 3;
			this._btnNest.Text = "Nest File";
			this._btnNest.UseVisualStyleBackColor = true;
			this._btnNest.Click += new System.EventHandler(this.NestFile);
			//
			// NestFwdataFile
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(554, 91);
			this.Controls.Add(this._btnNest);
			this.Controls.Add(this._btnBrowse);
			this.Controls.Add(this._fwdataPathname);
			this.Controls.Add(this.label1);
			this.Name = "NestFwdataFile";
			this.Text = "Nest an fwdata file";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog _openFileDialog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _fwdataPathname;
		private System.Windows.Forms.Button _btnBrowse;
		private System.Windows.Forms.Button _btnNest;
	}
}
