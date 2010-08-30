namespace FwLift
{
	partial class LiftBridgeDlg
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
			this.label1 = new System.Windows.Forms.Label();
			this._tbFLexProject = new System.Windows.Forms.TextBox();
			this._gbHistory = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this._btnReview = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this._btnSendReceive = new System.Windows.Forms.Button();
			this._clbTargets = new System.Windows.Forms.CheckedListBox();
			this._llSetup = new System.Windows.Forms.LinkLabel();
			this._gbHistory.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "FLex Project";
			//
			// _tbFLexProject
			//
			this._tbFLexProject.Enabled = false;
			this._tbFLexProject.Location = new System.Drawing.Point(87, 13);
			this._tbFLexProject.Name = "_tbFLexProject";
			this._tbFLexProject.Size = new System.Drawing.Size(254, 20);
			this._tbFLexProject.TabIndex = 1;
			//
			// _gbHistory
			//
			this._gbHistory.Controls.Add(this._btnReview);
			this._gbHistory.Controls.Add(this.label2);
			this._gbHistory.Location = new System.Drawing.Point(16, 52);
			this._gbHistory.Name = "_gbHistory";
			this._gbHistory.Size = new System.Drawing.Size(362, 94);
			this._gbHistory.TabIndex = 2;
			this._gbHistory.TabStop = false;
			this._gbHistory.Text = "History";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(25, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(144, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Last Language Forge Synch:";
			//
			// _btnReview
			//
			this._btnReview.Location = new System.Drawing.Point(270, 58);
			this._btnReview.Name = "_btnReview";
			this._btnReview.Size = new System.Drawing.Size(75, 23);
			this._btnReview.TabIndex = 1;
			this._btnReview.Text = "Review";
			this._btnReview.UseVisualStyleBackColor = true;
			this._btnReview.Click += new System.EventHandler(this.ReviewClick);
			//
			// groupBox1
			//
			this.groupBox1.Controls.Add(this._llSetup);
			this.groupBox1.Controls.Add(this._clbTargets);
			this.groupBox1.Controls.Add(this._btnSendReceive);
			this.groupBox1.Location = new System.Drawing.Point(16, 161);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(362, 147);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Send/Receive";
			//
			// _btnSendReceive
			//
			this._btnSendReceive.Location = new System.Drawing.Point(260, 118);
			this._btnSendReceive.Name = "_btnSendReceive";
			this._btnSendReceive.Size = new System.Drawing.Size(96, 23);
			this._btnSendReceive.TabIndex = 0;
			this._btnSendReceive.Text = "Send/Receive";
			this._btnSendReceive.UseVisualStyleBackColor = true;
			//
			// _clbTargets
			//
			this._clbTargets.FormattingEnabled = true;
			this._clbTargets.Location = new System.Drawing.Point(7, 20);
			this._clbTargets.Name = "_clbTargets";
			this._clbTargets.Size = new System.Drawing.Size(176, 79);
			this._clbTargets.TabIndex = 1;
			//
			// _llSetup
			//
			this._llSetup.AutoSize = true;
			this._llSetup.Location = new System.Drawing.Point(15, 118);
			this._llSetup.Name = "_llSetup";
			this._llSetup.Size = new System.Drawing.Size(47, 13);
			this._llSetup.TabIndex = 2;
			this._llSetup.TabStop = true;
			this._llSetup.Text = "Set up...";
			this._llSetup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetUpClicked);
			//
			// FwLiftDlg
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(401, 323);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this._gbHistory);
			this.Controls.Add(this._tbFLexProject);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FwLiftDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Chorus-Flex Partner";
			this._gbHistory.ResumeLayout(false);
			this._gbHistory.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _tbFLexProject;
		private System.Windows.Forms.GroupBox _gbHistory;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button _btnReview;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button _btnSendReceive;
		private System.Windows.Forms.LinkLabel _llSetup;
		private System.Windows.Forms.CheckedListBox _clbTargets;
	}
}