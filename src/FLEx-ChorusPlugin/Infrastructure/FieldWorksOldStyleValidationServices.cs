using System;
using System.Collections.Generic;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// File handler for a FieldWorks 7.0 xml class data file (not the main fwdata file).
	/// </summary>
	internal static class FieldWorksOldStyleValidationServices
	{
		private const string Extension = "ClassData";
		private static readonly Dictionary<string, bool> FilesChecked = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

		internal static bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, Extension))
				return false;

			bool seenBefore;
			if (FilesChecked.TryGetValue(pathToFile, out seenBefore))
				return seenBefore;
			var retval = FieldWorksFileHandlerServices.ValidateFile(pathToFile) == null;
			FilesChecked.Add(pathToFile, retval);
			return retval;
		}
	}
}
