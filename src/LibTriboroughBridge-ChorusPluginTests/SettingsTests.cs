// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.PlatformUtilities;
using SIL.Settings;

namespace LibTriboroughBridge_ChorusPluginTests
{
	public class SettingsTests
	{
		private const string AppName = "FLExBridge";
		private const string OldDirName = AppName + ".exe_Url_SomebodyThoughtGeneratingALongStringWouldBeAGoodIdea";
		private const string SettingsFileName = "user.config";

		private static readonly string LocalAppData;
		private static readonly string OldCompanyDirPath;
		private static readonly string OldAppDirPath;
		private static readonly string NewCompanyDirPath;
		private static readonly string NewAppDirPath;
		private static readonly IFileSystem EmptyFileSystem = new MockFileSystem();

		static SettingsTests()
		{
			LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			OldCompanyDirPath = Path.Combine(LocalAppData, Platform.IsWindows ? "SIL_International" : "SIL International");
			OldAppDirPath = Path.Combine(OldCompanyDirPath, OldDirName);
			NewCompanyDirPath = Path.Combine(LocalAppData, Application.CompanyName);
			NewAppDirPath = Path.Combine(NewCompanyDirPath, AppName);
		}

		[Test]
		public void MigrateCrossPlatformSettings_MigratesLatest([Values("Url", "StrongName")] string dirPart)
		{
			const string prevVer = "3.1.1";
			const string prevConfigXml = "<configuration><configSections></configuration>";
			var oldSettingsFileTemplate = Path.Combine(OldCompanyDirPath, $"{AppName}.exe_{dirPart}_ItsAllMeaninglessToMe", "{0}", SettingsFileName);
			var mockData = new MockFileData("<configuration/>");
			var latestData = new MockFileData(prevConfigXml);
			var fs = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{ string.Format(oldSettingsFileTemplate, prevVer), latestData },
				{ string.Format(oldSettingsFileTemplate, "3.0"), mockData },
				{ string.Format(oldSettingsFileTemplate, "corrupt"), mockData },
				{ Path.Combine(OldAppDirPath, "1.0", SettingsFileName), mockData }
			});

			// SUT
			LibTriboroughBridgeChorusPlugin.Properties.Settings.MigrateNonCrossPlatformSettings(fs, AppName);

			var newSettingsFilePath = Path.Combine(NewAppDirPath, prevVer, SettingsFileName);
			var newSettingsFileInfo = fs.FileInfo.FromFileName(newSettingsFilePath);
			Assert.That(newSettingsFileInfo.Exists);
			Assert.That(fs.File.ReadAllText(newSettingsFilePath), Is.EqualTo(prevConfigXml));
		}

		[Test]
		public void MigrateCrossPlatformSettings_DoesNotOverwrite()
		{
			const string prevVer = "3.1.1";
			const string existingConfigXml = "<configuration><configSections></configuration>";
			var oldSettingsFilePath = Path.Combine(OldAppDirPath, prevVer, SettingsFileName);
			var newSettingsFilePath = Path.Combine(NewAppDirPath, prevVer, SettingsFileName);
			var oldFileData = new MockFileData("<configuration/>");
			var existingData = new MockFileData(existingConfigXml);
			var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
				{ oldSettingsFilePath, oldFileData },
				{ newSettingsFilePath, existingData }
			});

			// SUT
			LibTriboroughBridgeChorusPlugin.Properties.Settings.MigrateNonCrossPlatformSettings(fs, AppName);

			Assert.That(fs.File.ReadAllText(newSettingsFilePath), Is.EqualTo(existingConfigXml));
		}

		[Test]
		public void PotentialOldAppSettingsDirs_NoOldCompanyDir_Empty()
		{
			var fs = new MockFileSystem();
			var dirInfo = fs.DirectoryInfo.FromDirectoryName(OldCompanyDirPath);
			// SUT
			var appDirs = LibTriboroughBridgeChorusPlugin.Properties.Settings.PotentialOldAppSettingsDirs(dirInfo, "dne");
			Assert.That(appDirs, Is.Empty);
		}

		[Test]
		public void PotentialOldAppSettingsDirs_OneOldDir()
		{
			const string appName = "RepoUtil";
			const string oldDirName = appName + ".exe_Url_WhoKnowsHowThisIsGenerated";
			var fs = new MockFileSystem();
			var dirInfo = fs.DirectoryInfo.FromDirectoryName(OldCompanyDirPath);
			dirInfo.Create();
			dirInfo.CreateSubdirectory(oldDirName);
			
			// SUT
			var appDirs = LibTriboroughBridgeChorusPlugin.Properties.Settings.PotentialOldAppSettingsDirs(dirInfo, appName).ToList();
			
			Assert.That(appDirs.Count, Is.EqualTo(1));
			Assert.That(appDirs[0].FullName, Is.EqualTo(Path.Combine(OldCompanyDirPath, oldDirName)));
		}

		[Test]
		public void PotentialOldAppSettingsDirs_TwoOldDirsAndExtras_ExcludesMismatches()
		{
			const string oldDirName2 = AppName + ".exe_StrongName_BoyDoesThisLookUgly";
			var fs = new MockFileSystem();
			var dirInfo = fs.DirectoryInfo.FromDirectoryName(OldCompanyDirPath);
			dirInfo.CreateSubdirectory(OldDirName);
			dirInfo.CreateSubdirectory(oldDirName2);
			dirInfo.CreateSubdirectory($"{AppName}_Competitor_Settings");
			// SUT
			var appDirs = LibTriboroughBridgeChorusPlugin.Properties.Settings.PotentialOldAppSettingsDirs(dirInfo, AppName).Select(d => d.FullName);
			Assert.That(appDirs, Is.EquivalentTo(new[] {OldDirName, oldDirName2}.Select(name => Path.Combine(OldCompanyDirPath, name))));
		}

		[Test]
		public void LatestVersionSettingsDir_ReturnsHighestVer()
		{
			const string prevVersion = "3.22.111";
			var fs = new MockFileSystem();
			var dirInfo = fs.DirectoryInfo.FromDirectoryName(OldDirName);
			dirInfo.CreateSubdirectory("1.0.1");
			var expected = dirInfo.CreateSubdirectory(prevVersion);
			dirInfo.CreateSubdirectory("3.4.5");
			// SUT
			var verDir = LibTriboroughBridgeChorusPlugin.Properties.Settings.LatestVersionSettingsDir(dirInfo.EnumerateDirectories().ToList());
			Assert.That(verDir.FullName, Is.EqualTo(expected.FullName));
		}

		[TestCase("bad", "bad", ExpectedResult = 0)]
		[TestCase("2.0", "2.0", ExpectedResult = 0)]
		[TestCase("bad", "1.0.0", ExpectedResult = -1)]
		[TestCase("1.0.1", "bad", ExpectedResult = 1)]
		[TestCase("1.0.1", "1.2.3", ExpectedResult = -1)]
		[TestCase("3.11.0", "3.4.6", ExpectedResult = 1)]
		public int CompareVersion(string lhs, string rhs)
		{
			return LibTriboroughBridgeChorusPlugin.Properties.Settings.CompareVersion(DirInfoFromVersion(lhs), DirInfoFromVersion(rhs));
		}

		[Test]
		public void AllSettingsUseCrossPlatformProvider()
		{
			ValidateProperties(LibTriboroughBridgeChorusPlugin.Properties.Settings.Default);
		}

		private static IDirectoryInfo DirInfoFromVersion(string version)
		{
			return EmptyFileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(OldAppDirPath, version));
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