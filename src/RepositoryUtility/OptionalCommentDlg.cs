// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace RepositoryUtility
{
	public partial class OptionalCommentDlg : Form
	{
		public OptionalCommentDlg()
		{
			InitializeComponent();
		}

		private void OkBtnClicked(object sender, EventArgs e)
		{
			Close();
		}

		private void CancelBtnClicked(object sender, EventArgs e)
		{
			Close();
		}

		internal string OptionalComment
		{
			get
			{
				return _tbOptionalComment.Text;
			}
		}
	}
}
