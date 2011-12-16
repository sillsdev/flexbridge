namespace FLEx_ChorusPlugin.View
{
	partial class StartupNewView
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
			this.label2 = new System.Windows.Forms.Label();
			this._btnGetStarted = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this._rbLocalNetwork = new System.Windows.Forms.RadioButton();
			this._rbInternet = new System.Windows.Forms.RadioButton();
			this._rbUsb = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this._cbAcceptLimitation = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(10, 25);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(224, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "and now I want to synchronize with them.";
			//
			// _btnGetStarted
			//
			this._btnGetStarted.Enabled = false;
			this._btnGetStarted.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._btnGetStarted.Location = new System.Drawing.Point(10, 207);
			this._btnGetStarted.Name = "_btnGetStarted";
			this._btnGetStarted.Size = new System.Drawing.Size(75, 23);
			this._btnGetStarted.TabIndex = 5;
			this._btnGetStarted.Text = "&Get Started";
			this._btnGetStarted.UseVisualStyleBackColor = true;
			this._btnGetStarted.Click += new System.EventHandler(this.ContinueBtnClicked);
			//
			// groupBox1
			//
			this.groupBox1.Controls.Add(this._rbLocalNetwork);
			this.groupBox1.Controls.Add(this._rbInternet);
			this.groupBox1.Controls.Add(this._rbUsb);
			this.groupBox1.Enabled = false;
			this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(10, 102);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(341, 93);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Get teammate\'s project from";
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
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(268, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Another teammate has already used FieldWorks Bridge,";
			//
			// _cbAcceptLimitation
			//
			this._cbAcceptLimitation.AutoSize = true;
			this._cbAcceptLimitation.Location = new System.Drawing.Point(10, 42);
			this._cbAcceptLimitation.Name = "_cbAcceptLimitation";
			this._cbAcceptLimitation.Size = new System.Drawing.Size(296, 17);
			this._cbAcceptLimitation.TabIndex = 8;
			this._cbAcceptLimitation.Text = "I understand, and accept, the limitation that these options";
			this._cbAcceptLimitation.UseVisualStyleBackColor = true;
			this._cbAcceptLimitation.CheckedChanged += new System.EventHandler(this.AcceptLimitationsCheckChanged);
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(27, 58);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(256, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "do not allow me to merge a project that already exists";
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(27, 75);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(237, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "on my computer with the project of my teammate.";
			//
			// StartupNewView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this._cbAcceptLimitation);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this._btnGetStarted);
			this.Controls.Add(this.label2);
			this.MinimumSize = new System.Drawing.Size(332, 158);
			this.Name = "StartupNewView";
			this.Size = new System.Drawing.Size(392, 243);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		internal System.Windows.Forms.Button _btnGetStarted;
		private System.Windows.Forms.GroupBox groupBox1;
		internal System.Windows.Forms.RadioButton _rbLocalNetwork;
		internal System.Windows.Forms.RadioButton _rbInternet;
		internal System.Windows.Forms.RadioButton _rbUsb;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox _cbAcceptLimitation;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
	}
}
