// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

namespace TriboroughBridge_ChorusPlugin.View
{
	public partial class BridgeConflictView
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
			this.components = new System.ComponentModel.Container();
			this._splitContainer = new System.Windows.Forms.SplitContainer();
			this.label1 = new System.Windows.Forms.Label();
			this._warninglabel1 = new System.Windows.Forms.Label();
			this._label1 = new System.Windows.Forms.Label();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
			this._splitContainer.Panel1.SuspendLayout();
			this._splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _splitContainer
			//
			this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this._splitContainer.IsSplitterFixed = true;
			this._splitContainer.Location = new System.Drawing.Point(0, 0);
			this._splitContainer.Name = "_splitContainer";
			this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// _splitContainer.Panel1
			//
			this._splitContainer.Panel1.Controls.Add(this.label1);
			this._splitContainer.Panel1.Controls.Add(this._warninglabel1);
			this._splitContainer.Panel1.Controls.Add(this._label1);
			this._splitContainer.Size = new System.Drawing.Size(904, 510);
			this._splitContainer.TabIndex = 0;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "BridgeConflictView.Project");
			this.label1.Location = new System.Drawing.Point(11, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 17);
			this.label1.TabIndex = 23;
			this.label1.Text = "Project:";
			//
			// _warninglabel1
			//
			this._warninglabel1.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._warninglabel1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._warninglabel1, null);
			this.l10NSharpExtender1.SetLocalizingId(this._warninglabel1, "BridgeConflictView.ProjectInse");
			this._warninglabel1.Location = new System.Drawing.Point(382, 6);
			this._warninglabel1.Name = "_warninglabel1";
			this._warninglabel1.Size = new System.Drawing.Size(148, 13);
			this._warninglabel1.TabIndex = 22;
			this._warninglabel1.Text = "The selected project is in use.";
			//
			// _label1
			//
			this._label1.AutoSize = true;
			this._label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._label1, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._label1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._label1, "BridgeConflictView.Dummy");
			this._label1.Location = new System.Drawing.Point(81, 19);
			this._label1.Name = "_label1";
			this._label1.Size = new System.Drawing.Size(58, 20);
			this._label1.TabIndex = 20;
			this._label1.Text = "Project";
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "FlexBridge";
			this.l10NSharpExtender1.PrefixForNewItems = "BridgeConflictView";
			//
			// BridgeConflictView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._splitContainer);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "BridgeConflictView.WindowTitle");
			this.Name = "BridgeConflictView";
			this.Text = "Conflict Report";
			this.Size = new System.Drawing.Size(904, 510);
			this._splitContainer.Panel1.ResumeLayout(false);
			this._splitContainer.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
			this._splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer _splitContainer;
		private System.Windows.Forms.Label _label1;
		private System.Windows.Forms.Label _warninglabel1;
		private System.Windows.Forms.Label label1;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;

	}
}
