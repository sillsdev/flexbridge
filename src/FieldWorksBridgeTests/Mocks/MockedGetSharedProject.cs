using System;
using System.Windows.Forms;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
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
		public bool GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}