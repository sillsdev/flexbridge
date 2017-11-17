namespace RepositoryUtility
{
	sealed partial class GetFileFromRevisionRange
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
			this.filename = new System.Windows.Forms.ComboBox();
			this.generateButton = new System.Windows.Forms.Button();
			this.merge = new System.Windows.Forms.TextBox();
			this.ancestor = new System.Windows.Forms.TextBox();
			this.exportLocation = new System.Windows.Forms.ComboBox();
			this.startRevLabel = new System.Windows.Forms.Label();
			this.endRevLabel = new System.Windows.Forms.Label();
			this.exportFolderLabel = new System.Windows.Forms.Label();
			this.fileNameLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// filename
			// 
			this.filename.FormattingEnabled = true;
			this.filename.Location = new System.Drawing.Point(121, 117);
			this.filename.Name = "filename";
			this.filename.Size = new System.Drawing.Size(121, 21);
			this.filename.TabIndex = 7;
			// 
			// generateButton
			// 
			this.generateButton.Location = new System.Drawing.Point(79, 204);
			this.generateButton.Name = "generateButton";
			this.generateButton.Size = new System.Drawing.Size(75, 23);
			this.generateButton.TabIndex = 6;
			this.generateButton.Text = "Generate";
			this.generateButton.UseVisualStyleBackColor = true;
			this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
			// 
			// merge
			// 
			this.merge.Location = new System.Drawing.Point(121, 78);
			this.merge.Name = "merge";
			this.merge.Size = new System.Drawing.Size(121, 20);
			this.merge.TabIndex = 5;
			// 
			// ancestor
			// 
			this.ancestor.Location = new System.Drawing.Point(121, 35);
			this.ancestor.Name = "ancestor";
			this.ancestor.Size = new System.Drawing.Size(121, 20);
			this.ancestor.TabIndex = 4;
			// 
			// exportLocation
			// 
			this.exportLocation.FormattingEnabled = true;
			this.exportLocation.Location = new System.Drawing.Point(120, 161);
			this.exportLocation.Name = "exportLocation";
			this.exportLocation.Size = new System.Drawing.Size(121, 21);
			this.exportLocation.TabIndex = 8;
			// 
			// startRevLabel
			// 
			this.startRevLabel.AutoSize = true;
			this.startRevLabel.Location = new System.Drawing.Point(13, 35);
			this.startRevLabel.Name = "startRevLabel";
			this.startRevLabel.Size = new System.Drawing.Size(89, 13);
			this.startRevLabel.TabIndex = 9;
			this.startRevLabel.Text = "Starting revision#";
			// 
			// endRevLabel
			// 
			this.endRevLabel.AutoSize = true;
			this.endRevLabel.Location = new System.Drawing.Point(13, 78);
			this.endRevLabel.Name = "endRevLabel";
			this.endRevLabel.Size = new System.Drawing.Size(86, 13);
			this.endRevLabel.TabIndex = 10;
			this.endRevLabel.Text = "Ending revision#";
			// 
			// exportFolderLabel
			// 
			this.exportFolderLabel.AutoSize = true;
			this.exportFolderLabel.Location = new System.Drawing.Point(13, 160);
			this.exportFolderLabel.Name = "exportFolderLabel";
			this.exportFolderLabel.Size = new System.Drawing.Size(66, 13);
			this.exportFolderLabel.TabIndex = 12;
			this.exportFolderLabel.Text = "Export folder";
			// 
			// fileNameLabel
			// 
			this.fileNameLabel.AutoSize = true;
			this.fileNameLabel.Location = new System.Drawing.Point(13, 117);
			this.fileNameLabel.Name = "fileNameLabel";
			this.fileNameLabel.Size = new System.Drawing.Size(52, 13);
			this.fileNameLabel.TabIndex = 11;
			this.fileNameLabel.Text = "File name";
			// 
			// GetFileFromRevisionRange
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(259, 273);
			this.Controls.Add(this.exportFolderLabel);
			this.Controls.Add(this.fileNameLabel);
			this.Controls.Add(this.endRevLabel);
			this.Controls.Add(this.startRevLabel);
			this.Controls.Add(this.exportLocation);
			this.Controls.Add(this.filename);
			this.Controls.Add(this.generateButton);
			this.Controls.Add(this.merge);
			this.Controls.Add(this.ancestor);
			this.Name = "GetFileFromRevisionRange";
			this.Text = "GetFileFromRevisionRange";
			this.Load += new System.EventHandler(this.GetFileFromRevisionRange_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox filename;
		private System.Windows.Forms.Button generateButton;
		private System.Windows.Forms.TextBox merge;
		private System.Windows.Forms.TextBox ancestor;
		private System.Windows.Forms.ComboBox exportLocation;
		private System.Windows.Forms.Label startRevLabel;
		private System.Windows.Forms.Label endRevLabel;
		private System.Windows.Forms.Label exportFolderLabel;
		private System.Windows.Forms.Label fileNameLabel;
	}
}