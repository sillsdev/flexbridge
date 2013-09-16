using System.Collections.Generic;
using TheTurtle;

namespace TheTurtleTests.Mocks
{
	internal class MockedProjectPathLocator : IProjectPathLocator
	{
		private readonly HashSet<string> _baseFolderPaths;

		internal MockedProjectPathLocator(HashSet<string> baseFolderPaths)
		{
			_baseFolderPaths = baseFolderPaths;
		}

		#region Implementation of IProjectPathLocator

		public HashSet<string> BaseFolderPaths
		{
			get { return new HashSet<string>(_baseFolderPaths); }
		}

		#endregion
	}
}