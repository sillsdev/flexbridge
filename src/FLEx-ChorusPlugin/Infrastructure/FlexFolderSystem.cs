using Chorus;
using Chorus.sync;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FlexFolderSystem
	{
		private static void ConfigureChorusProjectFolder(ProjectFolderConfiguration projectFolderConfiguration)
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
			projectFolderConfiguration.ExcludePatterns.Add("Shares/*.*"); // Presumed folder for future LIFT and PT-FLEx repos.
			ProjectFolderConfiguration.AddExcludedVideoExtensions(projectFolderConfiguration);

			projectFolderConfiguration.IncludePatterns.Add("FLExProject.ModelVersion"); // Hope this forces the version file to be done first.
			projectFolderConfiguration.IncludePatterns.Add("FLExProject.CustomProperties"); // Hope this forces the custom props to be done next.

			// Overhead files.
			projectFolderConfiguration.IncludePatterns.Add("do_not_share_project.txt");
			projectFolderConfiguration.IncludePatterns.Add(".hgignore");

			// Common at all levels.
			if (!projectFolderConfiguration.IncludePatterns.Contains("**.ChorusNotes"))
				projectFolderConfiguration.IncludePatterns.Add("**.ChorusNotes"); // Not really needed, since Chorus adds them. But, knows for how long?
			projectFolderConfiguration.IncludePatterns.Add("**.list");
			projectFolderConfiguration.IncludePatterns.Add("**.style");

			// Misc required files.
			projectFolderConfiguration.IncludePatterns.Add("WritingSystemStore/*.*");
			projectFolderConfiguration.IncludePatterns.Add("LinkedFiles/AudioVisual/*.*");
			projectFolderConfiguration.IncludePatterns.Add("LinkedFiles/Others/*.*");
			projectFolderConfiguration.IncludePatterns.Add("LinkedFiles/Pictures/*.*");
			projectFolderConfiguration.IncludePatterns.Add("SupportingFiles/*.*");

			// Linguistics
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/Reversals/**.reversal");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/Lexicon/Lexicon.lexdb");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/TextCorpus/*.textincorpus");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/Inventory/WordformInventory.inventory");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/Discourse/Charting.discourse");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/**.featsys");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/MorphologyAndSyntax/AnalyzingAgents.agents");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/MorphologyAndSyntax/MorphAndSynData.morphdata");
			projectFolderConfiguration.IncludePatterns.Add("Linguistics/Phonology/PhonologicalData.phondata");

			// Anthropology
			projectFolderConfiguration.IncludePatterns.Add("Anthropology/DataNotebook.ntbk");

			// Scripture
			projectFolderConfiguration.IncludePatterns.Add("Other/ReferenceSystem.srs");
			projectFolderConfiguration.IncludePatterns.Add("Other/Drafts.ArchivedDraft");
			projectFolderConfiguration.IncludePatterns.Add("Other/Translations.trans");
			projectFolderConfiguration.IncludePatterns.Add("Other/Settings.ImportSetting");

			// Leftovers
			// Style file and user-defined lists ought to be covered, above.
			projectFolderConfiguration.IncludePatterns.Add("General/FLExFilters.filter");
			projectFolderConfiguration.IncludePatterns.Add("General/FLExAnnotations.annotation");
			projectFolderConfiguration.IncludePatterns.Add("General/LanguageProject.langproj");
			projectFolderConfiguration.IncludePatterns.Add("General/FLExProject.lint");
		}

		/// <summary>
		/// Creates and initializes the ChorusSystem for use in FLExBridge
		/// </summary>
		/// <param name="directoryName"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static ChorusSystem InitializeChorusSystem(string directoryName, string user)
		{
			var system = new ChorusSystem(directoryName, user);
			ConfigureChorusProjectFolder(system.ProjectFolderConfiguration);
			return system;
		}
	}
}
