// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SIL.Progress;

namespace LfMergeBridgeTestApp
{
	public sealed partial class LfMergeBridgeTestAppForm : Form
	{
		public LfMergeBridgeTestAppForm()
		{
			InitializeComponent();
		}

		private void TestNewMethod(object sender, EventArgs e)
		{
			var options = new Dictionary<string, string>
			{
				{"projectPath", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)},
				{"languageDepotRepoName", "Language Depot"},
				{"languageDepotRepoUri", "http://CHANGE_ME:ME_TOO@resumable.languagedepot.org/ME_THREE"}
			};
			string somethingForClient;
			LfMergeBridge.LfMergeBridge.Execute("Language_Forge_Send_Receive", new NullProgress(), options, out somethingForClient);
		}

		private void TestCloneOption(object sender, EventArgs e)
		{
			var options = new Dictionary<string, string>
			{
				{"projectPath", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)},
				{"fdoDataModelVersion", "7000068"},
				{"languageDepotRepoName", "Language Depot"},
				{"languageDepotRepoUri", "http://CHANGE_ME:ME_TOO@resumable.languagedepot.org/ME_THREE"}
			};
			string somethingForClient;
			LfMergeBridge.LfMergeBridge.Execute("Language_Forge_Clone", new NullProgress(), options, out somethingForClient);
		}
	}
}
