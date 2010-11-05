using System;
using System.IO;

namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Provide some services needed by the model.
	/// </summary>
	internal static class LiftProjectServices
	{
		internal static string BasePath
		{
			get
			{
				return Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"LiftBridge");
			}
		}

		internal static string PathToProject(LiftProject project)
		{
			return Path.Combine(BasePath, project.LiftProjectName);
		}

		internal static bool ProjectIsShared(LiftProject project)
		{
			return Directory.Exists(Path.Combine(PathToProject(project), ".hg"));
		}
	}
}