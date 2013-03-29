using System.Collections.Generic;
using System.ComponentModel.Composition;
using TriboroughBridge_ChorusPlugin;

namespace TheTurtle.Model
{
	/// <summary>
	/// This implementation is suitable for FieldWorks end users,
	/// who may have projects in: HKLM.software.SIL.FieldWorks.7.0.ProjectsDir
	/// </summary>
	[Export(typeof(IProjectPathLocator))]
	internal sealed class RegularUserProjectPathLocator : IProjectPathLocator
	{
		#region Implementation of IProjectPathLocator

		public HashSet<string> BaseFolderPaths
		{
			get
			{
				return new HashSet<string>
								{
									Utilities.ProjectsPath
								};
			}
		}

		#endregion
	}
}