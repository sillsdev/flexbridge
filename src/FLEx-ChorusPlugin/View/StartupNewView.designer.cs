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
			this._label2 = new System.Windows.Forms.Label();
			this._btnGetStarted = new System.Windows.Forms.Button();
			this._groupBox = new System.Windows.Forms.GroupBox();
			this._rbLocalNetwork = new System.Windows.Forms.RadioButton();
			this._rbInternet = new System.Windows.Forms.RadioButton();
			this._rbUsb = new System.Windows.Forms.RadioButton();
			this._label3 = new System.Windows.Forms.Label();
			this._cbAcceptLimitation = new System.Windows.Forms.CheckBox();
			this._label4 = new System.Windows.Forms.Label();
			this._label5 = new System.Windows.Forms.Label();
			this._groupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// _label2
			//
			this._label2.AutoSize = true;
			this._label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._label2.Location = new System.Drawing.Point(10, 25);
			this._label2.Name = "_label2";
			this._label2.Size = new System.Drawing.Size(224, 13);
			this._label2.TabIndex = 3;
			this._label2.Text = "and now I want to synchronize with them.";
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
			// _groupBox
			//
			this._groupBox.Controls.Add(this._rbLocalNetwork);
			this._groupBox.Controls.Add(this._rbInternet);
			this._groupBox.Controls.Add(this._rbUsb);
			this._groupBox.Enabled = false;
			this._groupBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._groupBox.Location = new System.Drawing.Point(10, 102);
			this._groupBox.Name = "_groupBox";
			this._groupBox.Size = new System.Drawing.Size(341, 93);
			this._groupBox.TabIndex = 6;
			this._groupBox.TabStop = false;
			this._groupBox.Text = "Get teammate\'s project from";
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
			// _label3
			//
			this._label3.AutoSize = true;
			this._label3.Location = new System.Drawing.Point(10, 9);
			this._label3.Name = "_label3";
			this._label3.Size = new System.Drawing.Size(268, 13);
			this._label3.TabIndex = 7;
			this._label3.Text = "Another teammate has already used FieldWorks Bridge,";
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
			// _label4
			//
			this._label4.AutoSize = true;
			this._label4.Location = new System.Drawing.Point(27, 58);
			this._label4.Name = "_label4";
			this._label4.Size = new System.Drawing.Size(256, 13);
			this._label4.TabIndex = 9;
			this._label4.Text = "do not allow me to merge a project that already exists";
			//
			// _label5
			//
			this._label5.AutoSize = true;
			this._label5.Location = new System.Drawing.Point(27, 75);
			this._label5.Name = "_label5";
			this._label5.Size = new System.Drawing.Size(237, 13);
			this._label5.TabIndex = 10;
			this._label5.Text = "on my computer with the project of my teammate.";
			//
			// StartupNewView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._label5);
			this.Controls.Add(this._label4);
			this.Controls.Add(this._cbAcceptLimitation);
			this.Controls.Add(this._label3);
			this.Controls.Add(this._groupBox);
			this.Controls.Add(this._btnGetStarted);
			this.Controls.Add(this._label2);
			this.MinimumSize = new System.Drawing.Size(332, 158);
			this.Name = "StartupNewView";
			this.Size = new System.Drawing.Size(392, 243);
			this._groupBox.ResumeLayout(false);
			this._groupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _label2;
		internal System.Windows.Forms.Button _btnGetStarted;
		private System.Windows.Forms.GroupBox _groupBox;
		internal System.Windows.Forms.RadioButton _rbLocalNetwork;
		internal System.Windows.Forms.RadioButton _rbInternet;
		internal System.Windows.Forms.RadioButton _rbUsb;
		private System.Windows.Forms.Label _label3;
		private System.Windows.Forms.CheckBox _cbAcceptLimitation;
		private System.Windows.Forms.Label _label4;
		private System.Windows.Forms.Label _label5;
	}
}
