using Chorus.sync;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FlexFolderSystem
	{
		internal static void ConfigureChorusProjectFolder(ProjectFolderConfiguration projectFolderConfiguration)
		{
			// Exclude has precedence, but these are redundant as long as we're using the policy
			// that we explicitly include all the files we understand.  At least someday, when these
			// affect what happens in a more persistent way (e.g. be stored in the hgrc), these would protect
			// us a bit from other apps that might try to do a *.* include.
			projectFolderConfiguration.ExcludePatterns.Add("*.fwdata");
			projectFolderConfiguration.ExcludePatterns.Add("*.bak");
			projectFolderConfiguration.ExcludePatterns.Add("*.lock");
			projectFolderConfiguration.ExcludePatterns.Add("*.tmp");
			projectFolderConfiguration.ExcludePatterns.Add("**/Temp");
			projectFolderConfiguration.ExcludePatterns.Add("**/BackupSettings");
			projectFolderConfiguration.ExcludePatterns.Add("**/ConfigurationSettings");
			projectFolderConfiguration.ExcludePatterns.Add("WritingSystemStore/trash/*.*");
			projectFolderConfiguration.ExcludePatterns.Add("WritingSystemStore/WritingSystemsToIgnore.xml");
			projectFolderConfiguration.ExcludePatterns.Add("WritingSystemStore/WritingSystemsToIgnore.xml.ChorusNotes");

			projectFolderConfiguration.IncludePatterns.Add("*.ModelVersion"); // Hope this forces the version file to be done first.
			projectFolderConfiguration.IncludePatterns.Add("*.CustomProperties"); // Hope this forces the custom props to be done next.

			// Overhead files.
			projectFolderConfiguration.IncludePatterns.Add("do_not_share_project.txt");
			projectFolderConfiguration.IncludePatterns.Add(".hgignore");

			// Misc required files.
			projectFolderConfiguration.IncludePatterns.Add("WritingSystemStore/*.*");
			projectFolderConfiguration.IncludePatterns.Add("LinkedFiles/AudioVisual/*.*");
			projectFolderConfiguration.IncludePatterns.Add("LinkedFiles/Others/*.*");
			projectFolderConfiguration.IncludePatterns.Add("LinkedFiles/Pictures/*.*");
			projectFolderConfiguration.IncludePatterns.Add("SupportingFiles/*.*");

			// Linguistics
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/**.ChorusNotes");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/Reversals/*.reversal");
			// TODO: Add others.

			// Anthropology
			projectFolderConfiguration.IncludePatterns.Add("Anthropology/**.ChorusNotes");
			projectFolderConfiguration.IncludePatterns.Add("Anthropology/DataNotebook.ntbk");

			// Scripture
			projectFolderConfiguration.IncludePatterns.Add("Scripture/**.ChorusNotes");
			projectFolderConfiguration.IncludePatterns.Add("Scripture/ScriptureReferenceSystem.srs");
			projectFolderConfiguration.IncludePatterns.Add("Scripture/ScriptureStyleSheet.style");
			projectFolderConfiguration.IncludePatterns.Add("Scripture/*.list");
			projectFolderConfiguration.IncludePatterns.Add("Scripture/ScriptureTranslation.trans");
			projectFolderConfiguration.IncludePatterns.Add("Scripture/Settings.ImportSetting");

			// Leftovers
			// TODO: Add whatever doesn't readily fit into the three main domains.

			// Older style files, hopefully, soon to be obsolete.
			projectFolderConfiguration.IncludePatterns.Add("DataFiles/**.ClassData");
			projectFolderConfiguration.IncludePatterns.Add("DataFiles/**.ChorusNotes");
		}
	}
}
