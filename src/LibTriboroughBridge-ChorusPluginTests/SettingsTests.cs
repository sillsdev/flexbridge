// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using LibTriboroughBridgeChorusPlugin.Properties;
using NUnit.Framework;
using SIL.LCModel.Utils;
using SIL.PlatformUtilities;
using SIL.Settings;

namespace LibTriboroughBridge_ChorusPluginTests
{
	public class SettingsTests
	{
		private const string AppName = "FLExBridge";
		private const string CompanyName = "SIL";
		private const string OldDirName = AppName + ".exe_Url_SomebodyThoughtGeneratingALongStringWouldBeAGoodIdea";
		private const string SettingsFileName = "user.config";

		private static readonly string LocalAppData;
		private static readonly string OldCompanyDirPath;
		private static readonly string OldAppDirPath;
		private static readonly string NewCompanyDirPath;
		private static readonly string NewAppDirPath;

		private MockFileOS _fileOs;

		static SettingsTests()
		{
			LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			OldCompanyDirPath = Path.Combine(LocalAppData, Platform.IsWindows ? "SIL_International" : "SIL International");
			OldAppDirPath = Path.Combine(OldCompanyDirPath, OldDirName);
			NewCompanyDirPath = Path.Combine(LocalAppData, CompanyName);
			NewAppDirPath = Path.Combine(NewCompanyDirPath, AppName);
		}

		[SetUp]
		public void SetTestUp()
		{
			FileUtils.Manager.SetFileAdapter(_fileOs = new MockFileOS());
		}

		[TearDown]
		public void TearDown()
		{
			FileUtils.Manager.Reset();
		}

		[Test]
		public void MigrateCrossPlatformSettings_MigratesLatest([Values("Url", "StrongName")] string dirPart)
		{
			const string prevVer = "3.1.1";
			const string prevConfigXml = "<configuration><configSections></configuration>";
			var oldSettingsFileTemplate = Path.Combine(OldCompanyDirPath, $"{AppName}.exe_{dirPart}_ItsAllMeaninglessToMe", "{0}", SettingsFileName);
			const string mockData = "<configuration/>";
			_fileOs.AddFile(string.Format(oldSettingsFileTemplate, prevVer), prevConfigXml);
			_fileOs.AddFile(string.Format(oldSettingsFileTemplate, "3.0"), mockData);
			_fileOs.AddFile(string.Format(oldSettingsFileTemplate, "corrupt"), mockData);
			_fileOs.AddFile(Path.Combine(OldAppDirPath, "1.0", SettingsFileName), mockData);

			// SUT
			Settings.MigrateNonCrossPlatformSettings(CompanyName, AppName);

			var newSettingsFilePath = Path.Combine(NewAppDirPath, prevVer, SettingsFileName);
			Assert.That(FileUtils.FileExists(newSettingsFilePath));
			Assert.That(_fileOs.ReadAllText(newSettingsFilePath), Is.EqualTo(prevConfigXml));
			Assert.That(FileUtils.DirectoryExists(Path.GetDirectoryName(newSettingsFilePath)),
				"MockFileOS doesn't always care about directories existing. Real filesystems do.");
		}

		[Test]
		public void MigrateCrossPlatformSettings_DoesNotOverwrite()
		{
			const string prevVer = "3.1.1";
			const string oldConfigXml = "<configuration/>";
			const string existingConfigXml = "<configuration><configSections></configuration>";
			var oldSettingsFilePath = Path.Combine(OldAppDirPath, prevVer, SettingsFileName);
			var newSettingsFilePath = Path.Combine(NewAppDirPath, prevVer, SettingsFileName);
			_fileOs.AddFile(oldSettingsFilePath, oldConfigXml);
			_fileOs.AddFile(newSettingsFilePath, existingConfigXml);

			// SUT
			Settings.MigrateNonCrossPlatformSettings(CompanyName, AppName);

			Assert.That(FileUtils.FileExists(newSettingsFilePath), Is.True, $"Settings should have been copied to {newSettingsFilePath}");
			Assert.That(_fileOs.ReadAllText(newSettingsFilePath), Is.EqualTo(existingConfigXml));
		}

		[Test]
		public void PotentialOldAppSettingsDirs_NoOldCompanyDir_Empty()
		{
			// SUT
			var appDirs = Settings.PotentialOldAppSettingsDirs(OldCompanyDirPath, "dne");
			Assert.That(appDirs, Is.Empty);
		}

		[Test]
		public void PotentialOldAppSettingsDirs_OneOldDir()
		{
			const string appName = "RepoUtil";
			const string oldDirName = appName + ".exe_Url_WhoKnowsHowThisIsGenerated";
			var oldDirPath = Path.Combine(OldCompanyDirPath, oldDirName);
			FileUtils.EnsureDirectoryExists(oldDirPath);
			
			// SUT
			var appDirs = Settings.PotentialOldAppSettingsDirs(OldCompanyDirPath, appName);
			
			Assert.That(appDirs.Count, Is.EqualTo(1));
			Assert.That(appDirs[0], Is.EqualTo(oldDirPath));
		}

		[Test]
		public void PotentialOldAppSettingsDirs_TwoOldDirsAndExtras_ExcludesMismatches()
		{
			const string oldDirName2 = AppName + ".exe_StrongName_BoyDoesThisLookUgly";
			var oldDirPath2 = Path.Combine(OldCompanyDirPath, oldDirName2);
			FileUtils.EnsureDirectoryExists(OldAppDirPath);
			FileUtils.EnsureDirectoryExists(oldDirPath2);
			FileUtils.EnsureDirectoryExists(Path.Combine(OldCompanyDirPath, $"{AppName}_Competitor_Settings"));
			// SUT
			//var appDirs = Settings.PotentialOldAppSettingsDirs(OldCompanyDirPath, AppName);
			//Assert.That(appDirs, Is.EquivalentTo(new[] {OldAppDirPath, oldDirPath2}));
		}

		[Test]
		public void LatestVersionSettingsDir_ReturnsHighestVer()
		{
			const string prevVersion = "3.22.111";
			FileUtils.EnsureDirectoryExists(Path.Combine(OldAppDirPath, "1.0.1"));
			var expected = Path.Combine(OldAppDirPath, prevVersion);
			FileUtils.EnsureDirectoryExists(expected);
			FileUtils.EnsureDirectoryExists(Path.Combine(OldAppDirPath, "3.4.5"));
			// SUT
			var verDir = Settings.LatestVersionSettingsDir(FileUtils.GetDirectoriesInDirectory(OldAppDirPath).ToList());
			Assert.That(verDir, Is.EqualTo(expected));
		}

		[TestCase("bad", "bad", ExpectedResult = 0)]
		[TestCase("2.0", "2.0", ExpectedResult = 0)]
		[TestCase("bad", "1.0.0", ExpectedResult = -1)]
		[TestCase("1.0.1", "bad", ExpectedResult = 1)]
		[TestCase("1.0.1", "1.2.3", ExpectedResult = -1)]
		[TestCase("3.11.0", "3.4.6", ExpectedResult = 1)]
		public int CompareVersion(string lhs, string rhs)
		{
			return Settings.CompareVersion(Path.Combine(OldAppDirPath, lhs), Path.Combine(OldAppDirPath, rhs));
		}

		[Test]
		public void AllSettingsUseCrossPlatformProvider()
		{
			ValidateProperties(Settings.Default);
		}

		/// <summary>
		/// Verifies that each property has its provider set to <see cref="CrossPlatformSettingsProvider"/> or a subclass
		/// </summary>
		public static void ValidateProperties(ApplicationSettingsBase settings)
		{
			foreach (SettingsProperty property in settings.Properties)
			{
				Assert.That(property.Provider, Is.AssignableTo<CrossPlatformSettingsProvider>(),
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}
		}
	}
}