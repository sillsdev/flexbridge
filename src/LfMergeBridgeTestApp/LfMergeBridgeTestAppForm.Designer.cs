namespace LfMergeBridgeTestApp
{
	partial class LfMergeBridgeTestAppForm
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
			this._originalSRmethod = new System.Windows.Forms.Button();
			this._newSRmethod = new System.Windows.Forms.Button();
			this._cloneMethod = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _originalSRmethod
			// 
			this._originalSRmethod.Location = new System.Drawing.Point(9, 12);
			this._originalSRmethod.Name = "_originalSRmethod";
			this._originalSRmethod.Size = new System.Drawing.Size(176, 23);
			this._originalSRmethod.TabIndex = 0;
			this._originalSRmethod.Text = "Test original S/R method";
			this._originalSRmethod.UseVisualStyleBackColor = true;
			this._originalSRmethod.Click += new System.EventHandler(this.TestOriginalMethod);
			// 
			// _newSRmethod
			// 
			this._newSRmethod.Location = new System.Drawing.Point(9, 49);
			this._newSRmethod.Name = "_newSRmethod";
			this._newSRmethod.Size = new System.Drawing.Size(186, 23);
			this._newSRmethod.TabIndex = 1;
			this._newSRmethod.Text = "Test S/R Using new method";
			this._newSRmethod.UseVisualStyleBackColor = true;
			this._newSRmethod.Click += new System.EventHandler(this.TestNewMethod);
			// 
			// _cloneMethod
			// 
			this._cloneMethod.Location = new System.Drawing.Point(9, 86);
			this._cloneMethod.Name = "_cloneMethod";
			this._cloneMethod.Size = new System.Drawing.Size(75, 23);
			this._cloneMethod.TabIndex = 2;
			this._cloneMethod.Text = "Test Clone";
			this._cloneMethod.UseVisualStyleBackColor = true;
			this._cloneMethod.Click += new System.EventHandler(this.TestCloneOption);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this._cloneMethod);
			this.Controls.Add(this._newSRmethod);
			this.Controls.Add(this._originalSRmethod);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _originalSRmethod;
		private System.Windows.Forms.Button _newSRmethod;
		private System.Windows.Forms.Button _cloneMethod;
	}
}

