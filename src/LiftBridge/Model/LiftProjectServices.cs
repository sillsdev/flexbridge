using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

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
			var pathToProj = Path.Combine(BasePath, project.LiftProjectName);

			if (!string.IsNullOrEmpty(project.RepositoryIdentifier))
			{
				// Check for matching repo.
				foreach (var directory in from directory in Directory.GetDirectories(BasePath)
										  where Directory.Exists(Path.Combine(directory, ".hg"))
										  let repo = new HgRepository(directory, new NullProgress())
										  where repo.Identifier == project.RepositoryIdentifier
										  select directory)
				{
					pathToProj = directory;
					break;
				}
			}

			if (!Directory.Exists(pathToProj))
				Directory.CreateDirectory(pathToProj);

			return pathToProj;
		}

		internal static bool ProjectIsShared(LiftProject project)
		{
			return Directory.Exists(PathToMercurialFolder(project));
		}

		internal static string PathToFirstLiftFile(LiftProject project)
		{
			var liftFiles = Directory.GetFiles(PathToProject(project), "*.lift").ToList();
			return liftFiles.Count == 0 ? null : GetMainLiftFile(liftFiles);
		}

		internal static string PathToMercurialFolder(LiftProject project)
		{
			return Path.Combine(PathToProject(project), ".hg");
		}

		private static string GetMainLiftFile(IEnumerable<string> liftFiles)
		{
			return (from file in liftFiles
						where HasOnlyOneDot(file)
						select file).FirstOrDefault();
		}

		private static bool HasOnlyOneDot(string file)
		{
			return file.IndexOf(".") == file.LastIndexOf(".");
		}
	}
}