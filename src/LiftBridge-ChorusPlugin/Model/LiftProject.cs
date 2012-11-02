using System;
using System.IO;
using System.Linq;
using Palaso.Extensions;

namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Class that represents a Lift project.
	/// </summary>
	internal class LiftProject
	{
		internal LiftProject(string basePath)
		{
			BasePath = basePath;
		}

		internal string LiftPathname
		{
			get { return PathToFirstLiftFile(this); }
		}

		private string BasePath { get; set; }

		internal string PathToProject
		{
			get
			{
				return BasePath.CombineForPath("OtherRepositories", "LIFT");
			}
		}

		internal string ProjectName
		{
			get { return Path.GetFileNameWithoutExtension(PathToFirstFwFile(BasePath)); }
		}

		private static string PathToFirstFwFile(string basePath)
		{
			var fwFiles = Directory.GetFiles(basePath, "*.fwdata").ToList();
			if (fwFiles.Count == 0)
				fwFiles = Directory.GetFiles(basePath, "*.fwdb").ToList();
			return fwFiles.Count == 0 ? null : (from file in fwFiles
												  where HasOnlyOneDot(file)
												  select file).FirstOrDefault();
		}

		private static string PathToFirstLiftFile(LiftProject project)
		{
			var liftFiles = Directory.GetFiles(project.PathToProject, "*.lift").ToList();
			return liftFiles.Count == 0 ? null : (from file in liftFiles
												  where HasOnlyOneDot(file)
												  select file).FirstOrDefault();
		}

		private static bool HasOnlyOneDot(string pathname)
		{
			var filename = Path.GetFileName(pathname);
			return filename.IndexOf(".", StringComparison.InvariantCulture) == filename.LastIndexOf(".", StringComparison.InvariantCulture);
		}
	}
}
