using System.Windows.Forms;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.View;

namespace LiftBridgeTests.MockedViews
{
	internal class MockedGetSharedProject : IGetSharedProject
	{
		internal ExtantRepoSource RepoSource { get; private set; }

		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared Lift project from the specified source.
		/// </summary>
		/// <returns>
		/// 'true' if the shared project was cloned, otherwise 'false'.
		/// </returns>
		public bool GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, LiftProject project)
		{
			RepoSource = extantRepoSource;
			return true;
		}

		#endregion
	}
}