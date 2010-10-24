using System.Collections.Generic;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// This implementation is suitable for FieldWorks developers,
	/// who may have projects in:
	///		1. DistFiles\Projects,
	///		2A. ProgramData\SIL\FieldWorks 7\Projects (Vista or Windows 7)
	///		2B. ??? (XP)
	/// and/or
	/// </summary>
	internal class DeveloperSystemProjectPathLocator : IProjectPathLocator
	{
		#region Implementation of IProjectPathLocator

		public HashSet<string> BaseFolderPaths
		{
			get
			{
				return new HashSet<string>
								{
									FieldWorksProjectServices.ProjectsPath,
									FieldWorksProjectServices.StandardInstallDir
								};
			}
		}

		#endregion
	}
}