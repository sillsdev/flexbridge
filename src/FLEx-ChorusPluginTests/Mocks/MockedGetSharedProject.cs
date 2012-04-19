using System;
using System.Windows.Forms;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPluginTests.Mocks
{
	internal class MockedGetSharedProject : IGetSharedProject
	{
		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared FieldWorks project from the specified source.
		/// </summary>
		/// <returns>
		/// 'true' if the shared project was cloned, otherwsie 'false'.
		/// </returns>
		public bool GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, string flexProjectDir)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Access some current language project data.
		/// Implementations may throw an exception if called before GetSharedProjectUsing().
		/// </summary>
		public LanguageProject CurrentProject
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}