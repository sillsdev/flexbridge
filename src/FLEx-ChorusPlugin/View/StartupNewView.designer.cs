using Chorus.UI.Clone;
using Localization.UI;

namespace FLEx_ChorusPlugin.View
{
	internal sealed partial class StartupNewView
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupNewView));
			this._useUSBButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useSharedFolderButton = new System.Windows.Forms.Button();
			this.localizer = new Localization.UI.LocalizationExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.localizer)).BeginInit();
			this.SuspendLayout();
			//
			// _useUSBButton
			//
			this._useUSBButton.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this._useUSBButton, "_useUSBButton");
			this.localizer.SetLocalizableToolTip(this._useUSBButton, "USB button tooltip");
			this.localizer.SetLocalizationComment(this._useUSBButton, "Text for USB button on obtain dialog");
			this.localizer.SetLocalizingId(this._useUSBButton, "StartupNewView._useUSBButton");
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.UseVisualStyleBackColor = false;
			this._useUSBButton.Click += new System.EventHandler(this._useUSBButton_Click);
			//
			// _useInternetButton
			//
			this._useInternetButton.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this._useInternetButton, "_useInternetButton");
			this.localizer.SetLocalizableToolTip(this._useInternetButton, "Obtain from the internet");
			this.localizer.SetLocalizationComment(this._useInternetButton, "Button text for using the internet similar to engilsh for \'Internet\'");
			this.localizer.SetLocalizingId(this._useInternetButton, "StartupNewView._useInternetButton");
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.UseVisualStyleBackColor = false;
			this._useInternetButton.Click += new System.EventHandler(this._useInternetButton_Click);
			//
			// _useSharedFolderButton
			//
			this._useSharedFolderButton.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this._useSharedFolderButton, "_useSharedFolderButton");
			this.localizer.SetLocalizableToolTip(this._useSharedFolderButton, "Get project from a network folder.");
			this.localizer.SetLocalizationComment(this._useSharedFolderButton, "Button text meaning \"Shared Network Folder\"");
			this.localizer.SetLocalizingId(this._useSharedFolderButton, "StartupNewView._useSharedFolderButton");
			this._useSharedFolderButton.Name = "_useSharedFolderButton";
			this._useSharedFolderButton.UseVisualStyleBackColor = false;
			this._useSharedFolderButton.Click += new System.EventHandler(this._useSharedFolderButton_Click);
			//
			// localizer
			//
			this.localizer.LocalizationManagerId = "FLExBridge";
			//
			// StartupNewView
			//
			this.Controls.Add(this._useUSBButton);
			this.Controls.Add(this._useInternetButton);
			this.Controls.Add(this._useSharedFolderButton);
			this.localizer.SetLocalizableToolTip(this, null);
			this.localizer.SetLocalizationComment(this, null);
			this.localizer.SetLocalizingId(this, "StartupNewView.StartupNewView");
			this.Name = "StartupNewView";
			resources.ApplyResources(this, "$this");
			((System.ComponentModel.ISupportInitialize)(this.localizer)).EndInit();
			this.ResumeLayout(false);

		}

		private Localization.UI.LocalizationExtender localizer;
		private System.ComponentModel.IContainer components;

	}
}
