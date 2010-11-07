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
				var basePath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"LiftBridge");

				if (!Directory.Exists(basePath))
					Directory.CreateDirectory(basePath);

				return basePath;
			}
		}

		internal static string PathToProject(LiftProject project)
		{
			return Path.Combine(BasePath, project.LiftProjectName);
		}

		internal static bool ProjectIsShared(LiftProject project)
		{
			return Directory.Exists(PathToMercurialFolder(project));
		}

		internal static string PathToFirstLiftFile(LiftProject project)
		{
			var liftFiles = Directory.GetFiles(PathToProject(project), "*.lift");
			return liftFiles.Length == 0 ? null : liftFiles[0];
		}

		internal static string PathToMercurialFolder(LiftProject project)
		{
			return Path.Combine(PathToProject(project), ".hg");
		}
	}
}