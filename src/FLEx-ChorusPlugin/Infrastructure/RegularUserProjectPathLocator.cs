using System.Collections.Generic;

namespace FLEx_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// This implementation is suitable for FieldWorks end users,
	/// who may have projects in: HKLM.software.SIL.FieldWorks.7.0.ProjectsDir
	/// </summary>
	internal class RegularUserProjectPathLocator : IProjectPathLocator
	{
		#region Implementation of IProjectPathLocator

		public HashSet<string> BaseFolderPaths
		{
			get
			{
				return new HashSet<string>
								{
									FieldWorksProjectServices.ProjectsPath
								};
			}
		}

		#endregion
	}
}