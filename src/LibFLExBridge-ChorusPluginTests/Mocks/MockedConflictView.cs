// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2017 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Windows.Forms;

namespace LibFLEx_ChorusPluginTests.Mocks
{
	internal class MockedConflictView : Form
	{
		private string _projName;
		private string _nipView;

		public void EnableWarning()
		{
			// do nothing
		}

		public void SetProjectName(string projName)
		{
			_projName = projName;
		}

		public void SetBrowseView(string browser)
		{
			_nipView = browser;
		}
	}
}
