using Chorus.UI.Clone;

namespace SIL.LiftBridge.View
{
	internal sealed partial class StartupNewView
	{
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupNewView));
			this._useUsbButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useSharedFolderButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _useUsbButton
			//
			this._useUsbButton.BackColor = System.Drawing.Color.White;
			this._useUsbButton.Image = ((System.Drawing.Image)(resources.GetObject("_useUsbButton.Image")));
			this._useUsbButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useUsbButton.Enabled = new CloneFromUsb().GetHaveOneOrMoreUsbDrives();
			this._useUsbButton.Location = new System.Drawing.Point(17, 45);
			this._useUsbButton.Name = "_useUsbButton";
			this._useUsbButton.Size = new System.Drawing.Size(167, 39);
			this._useUsbButton.TabIndex = 3;
			this._useUsbButton.Text = "&USB Flash Drive";
			this._useUsbButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useUsbButton.UseVisualStyleBackColor = false;
			this._useUsbButton.Click += new System.EventHandler(this.UseUsbButtonClick);
			//
			// _useInternetButton
			//
			this._useInternetButton.BackColor = System.Drawing.Color.White;
			this._useInternetButton.Image = ((System.Drawing.Image)(resources.GetObject("_useInternetButton.Image")));
			this._useInternetButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useInternetButton.Location = new System.Drawing.Point(17, 120);
			this._useInternetButton.Enabled = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.Size = new System.Drawing.Size(167, 39);
			this._useInternetButton.TabIndex = 2;
			this._useInternetButton.Text = "&Internet";
			this._useInternetButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useInternetButton.UseVisualStyleBackColor = false;
			this._useInternetButton.Click += new System.EventHandler(this.UseInternetButtonClick);
			//
			// _useSharedFolderButton
			//
			this._useSharedFolderButton.BackColor = System.Drawing.Color.White;
			this._useSharedFolderButton.Enabled = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
			this._useSharedFolderButton.Image = ((System.Drawing.Image)(resources.GetObject("_useSharedFolderButton.Image")));
			this._useSharedFolderButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useSharedFolderButton.Location = new System.Drawing.Point(17, 199);
			this._useSharedFolderButton.Name = "_useSharedFolderButton";
			this._useSharedFolderButton.Size = new System.Drawing.Size(167, 39);
			this._useSharedFolderButton.TabIndex = 1;
			this._useSharedFolderButton.Text = "&Shared Network Folder";
			this._useSharedFolderButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useSharedFolderButton.UseVisualStyleBackColor = false;
			this._useSharedFolderButton.Click += new System.EventHandler(this.UseSharedFolderButtonClick);
			//
			// StartupNewView
			//
			this.Controls.Add(this._useUsbButton);
			this.Controls.Add(this._useInternetButton);
			this.Controls.Add(this._useSharedFolderButton);
			this.Name = "StartupNewView";
			this.Size = new System.Drawing.Size(201, 247);
			this.ResumeLayout(false);
		}

	}
}
