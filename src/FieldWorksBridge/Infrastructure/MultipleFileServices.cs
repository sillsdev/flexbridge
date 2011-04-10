using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Service that will manage the multiple files and original fwdata file for a full FW data set.
	/// </summary>
	/// <remarks>
	/// The task of the service is twofold:
	/// 1. Break up the main fwdata file into multiple files
	///		A. one for the custom property declarations, and
	///		B. one for each concrete CmObject class instance
	/// 2. Put the multiple files back together into the main fwdata file,
	///		but only if a Send/Receive had new information brought back into the repos.
	///		NB: The client of the service decides if new information was found, and decides to call the service, or not.
	/// </remarks>
	internal static class MultipleFileServices
	{
		internal static void BreakupMainFile(string mainFilePathname)
		{
			CheckPathname(mainFilePathname);
		}

		internal static void RestoreMainFile(string mainFilePathname)
		{
			// NB: Do the write on a new file, and then rename (move/copy) to 'mainFilePathname',
			// just in case it does not finish properly.
			// NB: This should follow current FW write settings practice.
			// There is no particular reason to ensure the order of objects in 'mainFilePathname' is retained,
			// but the custom props element must be first.
			// Q: Where to find the current model version? That may change in a S/R, and we really want to use the latest model version.

			CheckPathname(mainFilePathname);
		}

		private static void CheckPathname(string mainFilePathname)
		{
			var fwFileHandler = new FieldWorksFileHandler();
			if (!fwFileHandler.CanValidateFile(mainFilePathname))
				throw new ApplicationException("Cannot process the given file.");
		}
	}
}
