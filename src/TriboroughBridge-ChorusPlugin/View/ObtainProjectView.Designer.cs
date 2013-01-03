using Chorus.UI.Clone;

namespace TriboroughBridge_ChorusPlugin.View
{
	internal sealed partial class ObtainProjectView
	{
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObtainProjectView));
			this._useUSBButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useSharedFolderButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _useUSBButton
			//
			this._useUSBButton.BackColor = System.Drawing.Color.White;
			this._useUSBButton.Image = ((System.Drawing.Image)(resources.GetObject("_useUSBButton.Image")));
			this._useUSBButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useUSBButton.Enabled = new CloneFromUsb().GetHaveOneOrMoreUsbDrives();
			this._useUSBButton.Location = new System.Drawing.Point(17, 45);
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.Size = new System.Drawing.Size(167, 39);
			this._useUSBButton.TabIndex = 3;
			this._useUSBButton.Text = "&USB Flash Drive";
			this._useUSBButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useUSBButton.UseVisualStyleBackColor = false;
			this._useUSBButton.Click += new System.EventHandler(this._useUSBButton_Click);
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
			this._useInternetButton.Click += new System.EventHandler(this._useInternetButton_Click);
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
			this._useSharedFolderButton.Click += new System.EventHandler(this._useSharedFolderButton_Click);
			//
			// ObtainProjectView
			//
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._useUSBButton);
			this.Controls.Add(this._useInternetButton);
			this.Controls.Add(this._useSharedFolderButton);
			this.Name = "ObtainProjectView";
			this.Size = new System.Drawing.Size(201, 247);
			this.ResumeLayout(false);

		}

	}
}
