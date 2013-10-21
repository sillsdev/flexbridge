// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace RepositoryUtility
{
	public partial class RepositoryUtilityForm : Form
	{
		public RepositoryUtilityForm()
		{
			InitializeComponent();
		}

		private void HandleCloneMenuClick(object sender, EventArgs e)
		{
			MessageBox.Show(this, @"Pending....");
		}

		private void HandleViewHistoryMenuClick(object sender, EventArgs e)
		{
			MessageBox.Show(this, @"Pending....");
		}

		private void HandleUpdateToRevisionMenuClick(object sender, EventArgs e)
		{
			MessageBox.Show(this, @"Pending....");
		}

		private void HandleRestoreToRevisionMenuClick(object sender, EventArgs e)
		{
			MessageBox.Show(this, @"Pending....");
		}

		private void HandleCloseRepositoryClick(object sender, EventArgs e)
		{
			MessageBox.Show(this, @"Pending....");
		}

		private void HandleExitMenuClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
