// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using LibTriboroughBridgeChorusPlugin;


namespace SIL.LiftBridge.Infrastructure
{
	internal static class LiftUtilties
	{
		internal const string LiftExtension = ".lift";
		internal const string git = ".git";

		internal static string GetLiftProjectName(string fwProjectDir)
		{
			return Path.GetFileNameWithoutExtension(PathToFirstFwFile(fwProjectDir));
		}

		private static string PathToFirstFwFile(string fwProjectPath)
		{
			var fwFiles = Directory.GetFiles(fwProjectPath, "*" + LibTriboroughBridgeSharedConstants.FwXmlExtension).ToList();
			if (fwFiles.Count == 0)
				fwFiles = Directory.GetFiles(fwProjectPath, "*" + LibTriboroughBridgeSharedConstants.FwDb4oExtension).ToList();
			return fwFiles.Count == 0 ? null : (from file in fwFiles
												where HasOnlyOneDot(file)
												select file).FirstOrDefault();
		}

		internal static string PathToFirstLiftFile(string pathToLiftProject)
		{
			var liftFiles = Directory.GetFiles(pathToLiftProject, "*" + LiftExtension).ToList();
			return liftFiles.Count == 0 ? null : (from file in liftFiles
												  where HasOnlyOneDot(file)
												  select file).FirstOrDefault();
		}

		private static bool HasOnlyOneDot(string pathname)
		{
			var filename = Path.GetFileName(pathname);
			return filename.IndexOf(".", StringComparison.InvariantCulture) == filename.LastIndexOf(".", StringComparison.InvariantCulture);
		}

		internal const string FailureFilename = "FLExImportFailure.notice";
	}
}
