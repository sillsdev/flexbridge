// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using TriboroughBridge_ChorusPlugin;
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
			var fwFiles = Directory.GetFiles(fwProjectPath, "*" + SharedConstants.FwXmlExtension).ToList();
			if (fwFiles.Count == 0)
				fwFiles = Directory.GetFiles(fwProjectPath, "*" + SharedConstants.FwDb4oExtension).ToList();
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
