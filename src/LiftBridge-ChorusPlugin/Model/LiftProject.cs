using System;
using System.IO;
using System.Linq;
using TriboroughBridge_ChorusPlugin;

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

		/// <summary>
		/// NOTE: BasePath is the main FLEx project folder, not the lift folder.
		/// </summary>
		private string BasePath { get; set; }

		internal string PathToProject
		{
			get
			{
				var flexProjName = Path.GetFileName(BasePath);
				return Path.Combine(BasePath, Utilities.OtherRepositories, flexProjName + '_' + Utilities.LIFT);
			}
		}

		internal string ProjectName
		{
			get { return Path.GetFileNameWithoutExtension(PathToFirstFwFile(BasePath)); }
		}

		private static string PathToFirstFwFile(string basePath)
		{
			var fwFiles = Directory.GetFiles(basePath, "*" + Utilities.FwXmlExtension).ToList();
			if (fwFiles.Count == 0)
				fwFiles = Directory.GetFiles(basePath, "*" + Utilities.FwDb4oExtension).ToList();
			return fwFiles.Count == 0 ? null : (from file in fwFiles
												  where HasOnlyOneDot(file)
												  select file).FirstOrDefault();
		}

		private static string PathToFirstLiftFile(LiftProject project)
		{
			var liftFiles = Directory.GetFiles(project.PathToProject, "*" + Utilities.LiftExtension).ToList();
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
