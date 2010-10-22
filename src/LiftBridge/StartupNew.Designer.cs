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
			this._btnContinue = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this._rbLocalNetwork = new System.Windows.Forms.RadioButton();
			this._rbInternet = new System.Windows.Forms.RadioButton();
			this._rbUsb = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(154, 19);
			this.label1.TabIndex = 0;
			this.label1.Text = "Welcome to LIFT Bridge";
			//
			// _rbFirstToUseFlexBridge
			//
			this._rbFirstToUseFlexBridge.AutoSize = true;
			this._rbFirstToUseFlexBridge.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rbFirstToUseFlexBridge.Location = new System.Drawing.Point(19, 41);
			this._rbFirstToUseFlexBridge.Name = "_rbFirstToUseFlexBridge";
			this._rbFirstToUseFlexBridge.Size = new System.Drawing.Size(326, 17);
			this._rbFirstToUseFlexBridge.TabIndex = 1;
			this._rbFirstToUseFlexBridge.Text = "I am the first member of the team to start using LIFT Bridge";
			this._rbFirstToUseFlexBridge.UseVisualStyleBackColor = true;
			this._rbFirstToUseFlexBridge.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// _rbUseExistingSystem
			//
			this._rbUseExistingSystem.AutoSize = true;
			this._rbUseExistingSystem.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rbUseExistingSystem.Location = new System.Drawing.Point(19, 65);
			this._rbUseExistingSystem.Name = "_rbUseExistingSystem";
			this._rbUseExistingSystem.Size = new System.Drawing.Size(271, 17);
			this._rbUseExistingSystem.TabIndex = 2;
			this._rbUseExistingSystem.Text = "Another teammate has already used LIFT Bridge,";
			this._rbUseExistingSystem.UseVisualStyleBackColor = true;
			this._rbUseExistingSystem.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(36, 81);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(224, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "and now I want to synchronize with them.";
			//
			// _btnContinue
			//
			this._btnContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btnContinue.Enabled = false;
			this._btnContinue.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._btnContinue.Location = new System.Drawing.Point(287, 216);
			this._btnContinue.Name = "_btnContinue";
			this._btnContinue.Size = new System.Drawing.Size(75, 23);
			this._btnContinue.TabIndex = 5;
			this._btnContinue.Text = "&Get Started";
			this._btnContinue.UseVisualStyleBackColor = true;
			this._btnContinue.Click += new System.EventHandler(this.ContinueBtnClicked);
			//
			// groupBox1
			//
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this._rbLocalNetwork);
			this.groupBox1.Controls.Add(this._rbInternet);
			this.groupBox1.Controls.Add(this._rbUsb);
			this.groupBox1.Enabled = false;
			this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(39, 109);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(323, 93);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Get from";
			//
			// _rbLocalNetwork
			//
			this._rbLocalNetwork.AutoSize = true;
			this._rbLocalNetwork.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rbLocalNetwork.Location = new System.Drawing.Point(12, 65);
			this._rbLocalNetwork.Name = "_rbLocalNetwork";
			this._rbLocalNetwork.Size = new System.Drawing.Size(98, 17);
			this._rbLocalNetwork.TabIndex = 2;
			this._rbLocalNetwork.TabStop = true;
			this._rbLocalNetwork.Text = "Local Network";
			this._rbLocalNetwork.UseVisualStyleBackColor = true;
			this._rbLocalNetwork.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// _rbInternet
			//
			this._rbInternet.AutoSize = true;
			this._rbInternet.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rbInternet.Location = new System.Drawing.Point(12, 42);
			this._rbInternet.Name = "_rbInternet";
			this._rbInternet.Size = new System.Drawing.Size(66, 17);
			this._rbInternet.TabIndex = 1;
			this._rbInternet.TabStop = true;
			this._rbInternet.Text = "Internet";
			this._rbInternet.UseVisualStyleBackColor = true;
			this._rbInternet.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// _rbUsb
			//
			this._rbUsb.AutoSize = true;
			this._rbUsb.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rbUsb.Location = new System.Drawing.Point(12, 19);
			this._rbUsb.Name = "_rbUsb";
			this._rbUsb.Size = new System.Drawing.Size(102, 17);
			this._rbUsb.TabIndex = 0;
			this._rbUsb.TabStop = true;
			this._rbUsb.Text = "USB flash drive";
			this._rbUsb.UseVisualStyleBackColor = true;
			this._rbUsb.Click += new System.EventHandler(this.RadioButtonClicked);
			//
			// StartupNew
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this._btnContinue);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._rbUseExistingSystem);
			this.Controls.Add(this._rbFirstToUseFlexBridge);
			this.Controls.Add(this.label1);
			this.MinimumSize = new System.Drawing.Size(332, 158);
			this.Name = "StartupNew";
			this.Size = new System.Drawing.Size(374, 253);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton _rbFirstToUseFlexBridge;
		private System.Windows.Forms.RadioButton _rbUseExistingSystem;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button _btnContinue;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton _rbLocalNetwork;
		private System.Windows.Forms.RadioButton _rbInternet;
		private System.Windows.Forms.RadioButton _rbUsb;
	}
}
