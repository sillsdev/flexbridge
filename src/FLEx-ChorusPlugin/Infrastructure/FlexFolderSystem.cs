using System.IO;
using Chorus;
using Chorus.sync;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FlexFolderSystem
	{
		private static void ConfigureChorusProjectFolder(ProjectFolderConfiguration projectFolderConfiguration)
		{
			// Exclude has precedence, but these are redundant as long as we're using the policy
			// that we explicitly include all the files we understand. At least someday, when these
			// affect what happens in a more persistent way (e.g. be stored in the hgrc), these would protect
			// us a bit from other apps that might try to do a *.* include.
			projectFolderConfiguration.ExcludePatterns.Add("*.fwdata");
			projectFolderConfiguration.ExcludePatterns.Add("*.bak");
			projectFolderConfiguration.ExcludePatterns.Add("*.lock");
			projectFolderConfiguration.ExcludePatterns.Add("*.tmp");
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("Temp", "**.*"));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("BackupSettings", "**.*"));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("ConfigurationSettings", "**.*"));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("WritingSystemStore", Path.Combine("trash", "**.*")));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("WritingSystemStore", "WritingSystemsToIgnore.xml"));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("WritingSystemStore", "WritingSystemsToIgnore.xml.ChorusNotes"));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("WritingSystemStore", "idchangelog.xml"));
			projectFolderConfiguration.ExcludePatterns.Add(Path.Combine("Shares", "**.*")); // Presumed folder for future LIFT and PT-FLEx repos.
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
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("WritingSystemStore", "*.*"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("LinkedFiles", Path.Combine("AudioVisual", "*.*")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("LinkedFiles", Path.Combine("Others", "*.*")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("LinkedFiles", Path.Combine("Pictures", "*.*")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("SupportingFiles", "*.*"));

			// Linguistics
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("Reversals", "**.reversal")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("Lexicon", "Lexicon.lexdb")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("TextCorpus", "*.textincorpus")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("Inventory", "WordformInventory.inventory")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("Discourse", "Charting.discourse")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", "**.featsys"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("MorphologyAndSyntax", "AnalyzingAgents.agents")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("MorphologyAndSyntax", "MorphAndSynData.morphdata")));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Linguistics", Path.Combine("Phonology", "PhonologicalData.phondata")));

			// Anthropology
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Anthropology", "DataNotebook.ntbk"));

			// Scripture
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Other", "ReferenceSystem.srs"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Other", "Drafts.ArchivedDraft"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Other", "Translations.trans"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("Other", "Settings.ImportSetting"));

			// Leftovers
			// Style file and user-defined lists ought to be covered, above.
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("General", "FLExFilters.filter"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("General", "FLExAnnotations.annotation"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("General", "LanguageProject.langproj"));
			projectFolderConfiguration.IncludePatterns.Add(Path.Combine("General", "FLExProject.lint"));
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
