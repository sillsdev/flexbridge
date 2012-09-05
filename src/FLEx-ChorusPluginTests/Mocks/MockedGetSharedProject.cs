using System;
using System.Windows.Forms;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPluginTests.Mocks
{
	internal class MockedGetSharedProject : IGetSharedProject
	{
		#region Implementation of IGetSharedProject

		public CloneResult GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, Func<string, bool> projectFilter, string baseLocalProjectDir, string preferredClonedFolderName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}