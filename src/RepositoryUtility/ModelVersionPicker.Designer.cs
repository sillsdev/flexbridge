// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

namespace RepositoryUtility
{
	partial class ModelVersionPicker
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
			this._nudModelVersionPicker = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this._btnOk = new System.Windows.Forms.Button();
			this._btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this._nudModelVersionPicker)).BeginInit();
			this.SuspendLayout();
			//
			// _nudModelVersionPicker
			//
			this._nudModelVersionPicker.Location = new System.Drawing.Point(279, 6);
			this._nudModelVersionPicker.Maximum = new decimal(new int[] {
			7999999,
			0,
			0,
			0});
			this._nudModelVersionPicker.Minimum = new decimal(new int[] {
			7000037,
			0,
			0,
			0});
			this._nudModelVersionPicker.Name = "_nudModelVersionPicker";
			this._nudModelVersionPicker.Size = new System.Drawing.Size(66, 20);
			this._nudModelVersionPicker.TabIndex = 0;
			this._nudModelVersionPicker.Value = new decimal(new int[] {
			7000068,
			0,
			0,
			0});
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(260, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Please select the current FLEx model version number:";
			//
			// _btnOk
			//
			this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._btnOk.Location = new System.Drawing.Point(85, 102);
			this._btnOk.Name = "_btnOk";
			this._btnOk.Size = new System.Drawing.Size(75, 23);
			this._btnOk.TabIndex = 2;
			this._btnOk.Text = "Ok";
			this._btnOk.UseVisualStyleBackColor = true;
			//
			// _btnCancel
			//
			this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._btnCancel.Location = new System.Drawing.Point(189, 102);
			this._btnCancel.Name = "_btnCancel";
			this._btnCancel.Size = new System.Drawing.Size(75, 23);
			this._btnCancel.TabIndex = 3;
			this._btnCancel.Text = "Cancel";
			this._btnCancel.UseVisualStyleBackColor = true;
			//
			// ModelVersionPicker
			//
			this.AcceptButton = this._btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._btnCancel;
			this.ClientSize = new System.Drawing.Size(349, 129);
			this.ControlBox = false;
			this.Controls.Add(this._btnCancel);
			this.Controls.Add(this._btnOk);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._nudModelVersionPicker);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ModelVersionPicker";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "FLEx Model Version";
			((System.ComponentModel.ISupportInitialize)(this._nudModelVersionPicker)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.NumericUpDown _nudModelVersionPicker;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _btnOk;
		private System.Windows.Forms.Button _btnCancel;
	}
}