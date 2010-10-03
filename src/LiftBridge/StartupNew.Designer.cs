namespace SIL.LiftBridge
{
	partial class StartupNew
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this._rbFirstToUseFlexBridge = new System.Windows.Forms.RadioButton();
			this._rbUseExistingSystem = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._btnContinue = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(124, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Welcome to FLEx Bridge";
			//
			// _rbFirstToUseFlexBridge
			//
			this._rbFirstToUseFlexBridge.AutoSize = true;
			this._rbFirstToUseFlexBridge.Location = new System.Drawing.Point(19, 41);
			this._rbFirstToUseFlexBridge.Name = "_rbFirstToUseFlexBridge";
			this._rbFirstToUseFlexBridge.Size = new System.Drawing.Size(301, 17);
			this._rbFirstToUseFlexBridge.TabIndex = 1;
			this._rbFirstToUseFlexBridge.Text = "I am the first member of the team to start using FLEx Bridge";
			this._rbFirstToUseFlexBridge.UseVisualStyleBackColor = true;
			this._rbFirstToUseFlexBridge.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// _rbUseExistingSystem
			//
			this._rbUseExistingSystem.AutoSize = true;
			this._rbUseExistingSystem.Location = new System.Drawing.Point(19, 65);
			this._rbUseExistingSystem.Name = "_rbUseExistingSystem";
			this._rbUseExistingSystem.Size = new System.Drawing.Size(278, 17);
			this._rbUseExistingSystem.TabIndex = 2;
			this._rbUseExistingSystem.Text = "Another teammate has already used FLEx Bridge, and";
			this._rbUseExistingSystem.UseVisualStyleBackColor = true;
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(36, 81);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(265, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "now I want to synchronize with them using the internet,";
			this.label2.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(36, 96);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(170, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "USB flash drive, or a local network";
			//
			// _btnContinue
			//
			this._btnContinue.Enabled = false;
			this._btnContinue.Location = new System.Drawing.Point(19, 115);
			this._btnContinue.Name = "_btnContinue";
			this._btnContinue.Size = new System.Drawing.Size(75, 23);
			this._btnContinue.TabIndex = 5;
			this._btnContinue.Text = "Get Started";
			this._btnContinue.UseVisualStyleBackColor = true;
			this._btnContinue.Click += new System.EventHandler(this.ContinueBtnClicked);
			//
			// StartupNew
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._btnContinue);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._rbUseExistingSystem);
			this.Controls.Add(this._rbFirstToUseFlexBridge);
			this.Controls.Add(this.label1);
			this.MinimumSize = new System.Drawing.Size(332, 158);
			this.Name = "StartupNew";
			this.Size = new System.Drawing.Size(332, 158);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton _rbFirstToUseFlexBridge;
		private System.Windows.Forms.RadioButton _rbUseExistingSystem;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button _btnContinue;
	}
}
