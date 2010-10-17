using Chorus.sync;

namespace SIL.LiftBridge
{
	/// <summary>
	/// Copy of the WeSay project configuration class, so LiftBridge doens; thave to take a dependency on WeSay.
	/// </summary>
	public sealed class LiftBridgeProjectFolderConfiguration : ProjectFolderConfiguration
	{
		public LiftBridgeProjectFolderConfiguration(string folderPath)
			: base(folderPath)
		{
			// 'Borrowed' from WeSay, to not have a dependency on it.
			//exclude has precedence, but these are redundant as long as we're using the policy
			//that we explicitly include all the files we understand.  At least someday, when these
			//effect what happens in a more persistent way (e.g. be stored in the hgrc), these would protect
			//us a bit from other apps that might try to do a *.* include

			// Excludes
			ExcludePatterns.Add("**/cache");
			ExcludePatterns.Add("**/Cache");
			ExcludePatterns.Add("autoFonts.css");
			ExcludePatterns.Add("autoLayout.css");
			ExcludePatterns.Add("defaultDictionary.css");
			ExcludePatterns.Add("*.old");
			ExcludePatterns.Add("*.WeSayUserMemory");
			ExcludePatterns.Add("*.tmp");
			ExcludePatterns.Add("*.bak");

			// Includes.
			IncludePatterns.Add("audio/*.*");
			IncludePatterns.Add("pictures/*.*");
			IncludePatterns.Add("**.css"); //stylesheets
			IncludePatterns.Add("export/*.lpconfig");//lexique pro
			IncludePatterns.Add("**.lift");
			IncludePatterns.Add("**.WeSayConfig");
			IncludePatterns.Add("**.WeSayUserConfig");
			IncludePatterns.Add("**.xml");
			IncludePatterns.Add(".hgIgnore");
			IncludePatterns.Add(".ldml");
			IncludePatterns.Add(".lift-ranges");
		}
	}
}