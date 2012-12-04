using System.Collections.Generic;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// This implementation is suitable for FieldWorks end users,
	/// who may have projects in: HKLM.software.SIL.FieldWorks.7.0.ProjectsDir
	/// </summary>
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