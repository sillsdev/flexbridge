using Chorus.sync;

namespace FieldWorksBridge
{
	public class FieldWorksProjectFolderConfiguration : ProjectFolderConfiguration
	{
		public FieldWorksProjectFolderConfiguration(string folderPath)
			: base(folderPath)
		{
			//exclude has precedence, but these are redundant as long as we're using the policy
			//that we explicitly include all the files we understand.  At least someday, when these
			//effect what happens in a more persisten wayt (e.g. be stored in the hgrc), these would protect
			//us a bit from other apps that might try to do a *.* include
			ExcludePatterns.Add("*.bak");
			ExcludePatterns.Add("*.lock");
			ExcludePatterns.Add("*.tmp");
			ExcludePatterns.Add("**/Temp");
			ExcludePatterns.Add("**/BackupSettings");
			ExcludePatterns.Add("**/ConfigurationSettings");

			IncludePatterns.Add("WritingSystemStore/*.*");
			IncludePatterns.Add("LinkedFiles/AudioVisual/*.*");
			IncludePatterns.Add("LinkedFiles/Others/*.*");
			IncludePatterns.Add("LinkedFiles/Pictures/*.*");
			IncludePatterns.Add("Keyboards/*.*");
			IncludePatterns.Add("Fonts/*.*");
			IncludePatterns.Add("*.fwdata");
			IncludePatterns.Add(".hgignore");
		}
	}
}