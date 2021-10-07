using System.Configuration;
using System.Linq;
using NUnit.Framework;
using RepositoryUtility.Properties;
using SIL.Settings;

namespace RepositoryUtilityTests
{
	public class SettingsTests
	{
		[Test]
		public void AllSettingsUseCrossPlatformProvider()
		{
			foreach (SettingsProperty property in Settings.Default.Properties)
			{
				Assert.That(property.Provider, Is.AssignableTo<RepositoryUtility.Properties.SettingsProvider>(),
					$"Property '{property.Name}' needs the Provider string set to {typeof(RepositoryUtility.Properties.SettingsProvider)}");
			}
			Assert.That(Settings.Default.Properties.Cast<SettingsProperty>().First().Provider, Is.AssignableTo<CrossPlatformSettingsProvider>(),
				$"Type {typeof(RepositoryUtility.Properties.SettingsProvider)} should inherit from {typeof(CrossPlatformSettingsProvider)}");
		}
	}
}