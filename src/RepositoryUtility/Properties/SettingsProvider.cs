using SIL.Settings;

namespace RepositoryUtility.Properties
{
	public class SettingsProvider : CrossPlatformSettingsProvider
	{
		internal const string ProductNameForSettings = "RepositoryUtility";
		protected override string ProductName => ProductNameForSettings;
	}
}