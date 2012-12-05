using System.IO;
using System.Windows.Forms;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using TriboroughBridge_ChorusPlugin;

namespace SIL.LiftBridge.Services
{
	/// <summary>
	/// This class handles issues related to a FLEx import failures.
	/// </summary>
	internal static class ImportFailureServices
	{
		internal static void RegisterStandardImportFailure(Form parentWindow, LiftProject liftProject)
		{
			// The results (of the FLEx import failure) will be that Lift Bridge will store the fact of the import failure,
			// and then protect the repo from damage by another S/R by Flex
			// by seeing the last import failure, and then requiring the user to re-try the failed import,
			// using the same LIFT file that had failed, before.
			// If that re-try attempt also fails, the user will need to continue re-trying the import,
			// until FLEx is fixed and can do the import.
			MessageBox.Show(parentWindow, Resources.kFlexStandardImportFailureMessage, Resources.kFlexImportFailureTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

			// Write out the failure notice.
			var failurePathname = GetNoticePathname(liftProject);
			File.WriteAllText(failurePathname, Resources.kStandardFailureFileContents);
		}

		internal static void RegisterBasicImportFailure(Form parentWindow, LiftProject liftProject)
		{
			// The results (of the FLEx inital import failure) will be that Lift Bridge will store the fact of the import failure,
			// and then protect the repo from damage by another S/R by Flex
			// by seeing the last import failure, and then requiring the user to re-try the failed import,
			// using the same LIFT file that had failed, before.
			// If that re-try attempt also fails, the user will need to continue re-trying the import,
			// until FLEx is fixed and can do the import.
			MessageBox.Show(parentWindow, Resources.kBasicImportFailureMessage, Resources.kFlexImportFailureTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

			// Write out the failure notice.
			var failurePathname = GetNoticePathname(liftProject);
			File.WriteAllText(failurePathname, Resources.kBasicFailureFileContents);
		}

		internal static ImportFailureStatus GetFailureStatus(LiftProject liftProject)
		{
			var failurePathname = GetNoticePathname(liftProject);
			if (!File.Exists(failurePathname))
				return ImportFailureStatus.NoImportNeeded;

			var fileContents = File.ReadAllText(failurePathname);
			return fileContents.Contains(Resources.kBasicFailureFileContents) ? ImportFailureStatus.BasicImportNeeded : ImportFailureStatus.StandardImportNeeded;
		}

		internal static void ClearImportFailure(LiftProject liftProject)
		{
			var failurePathname = GetNoticePathname(liftProject);
			if (File.Exists(failurePathname))
				File.Delete(failurePathname);
		}

		private static string GetNoticePathname(LiftProject liftProject)
		{
			return Path.Combine(Path.GetDirectoryName(liftProject.LiftPathname), Utilities.FailureFilename);
		}
	}

	internal enum ImportFailureStatus
	{
		BasicImportNeeded,
		StandardImportNeeded,
		NoImportNeeded
	}
}