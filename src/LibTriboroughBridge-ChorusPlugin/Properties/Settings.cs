// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using SIL.LCModel.Utils;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace LibTriboroughBridgeChorusPlugin.Properties
{
	public sealed partial class Settings : IUpgradableSettings
	{
		/// <summary>
		/// Upgrades settings from previous versions, including versions before we started using CrossPlatformSettingsProvider.
		/// If settings need to be upgraded, finds old settings under %LocalAppData%/SIL_International/AppName_URL_CrazyLongGeneratedString or similar
		/// and copies them to %LocalAppData%/SIL/AppName, then upgrades settings.
		/// </summary>
		public static void UpgradeSettingsIfNecessary<T>(T settings, string companyName, string appName) where T : ApplicationSettingsBase, IUpgradableSettings
		{
			if (!settings.CallUpgrade)
			{
				return;
			}

			TryTo(() => MigrateNonCrossPlatformSettings(companyName, appName));

			TryTo(settings.Upgrade);

			// Whether or not we succeeded, don't try again.
			settings.CallUpgrade = false;
		}

		private static void TryTo(Action doThis)
		{
			try
			{
				doThis();
			}
			catch (Exception e)
			{
				ErrorReport.ReportNonFatalExceptionWithMessage(e, "Failed to upgrade settings");
			}
		}

		internal static void MigrateNonCrossPlatformSettings(string companyName, string appName)
		{
			// Find settings from earlier versions that didn't use CrossPlatformSettingsProvider
			// Copy the latest version's settings from each matching folder (don't overwrite)
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var oldCompanyDir = Path.Combine(localAppData, Platform.IsWindows ? "SIL_International" : "SIL International");
			var oldAppDirs = PotentialOldAppSettingsDirs(oldCompanyDir, appName).SelectMany(FileUtils.GetDirectoriesInDirectory).ToList();
			if (!oldAppDirs.Any())
			{
				return;
			}

			var latestOldSettingsDir = LatestVersionSettingsDir(oldAppDirs);
			var newAppDir = Path.Combine(localAppData, companyName, appName);
			var newSettingsDir = Path.Combine(newAppDir, Path.GetFileName(latestOldSettingsDir));
			const string settingsFileName = "user.config";
			var latestOldSettingsFile = Path.Combine(latestOldSettingsDir, settingsFileName);
			var newSettingsFilePath = Path.Combine(newSettingsDir, settingsFileName);
			if (FileUtils.FileExists(latestOldSettingsFile) && !FileUtils.FileExists(newSettingsFilePath))
			{
				FileUtils.EnsureDirectoryExists(newSettingsDir);
				FileUtils.Copy(latestOldSettingsFile, newSettingsFilePath);
			}
		}

		internal static string[] PotentialOldAppSettingsDirs(string oldCompanyDir, string appName)
		{
			return FileUtils.DirectoryExists(oldCompanyDir) ? FileUtils.GetDirectoriesInDirectory(oldCompanyDir, $"{appName}.exe_*") : new string[0];
		}

		internal static string LatestVersionSettingsDir(List<string> versionSettingsDirs)
		{
			versionSettingsDirs.Sort(CompareVersion);
			return versionSettingsDirs.LastOrDefault();
		}

		/// <returns>
		/// 1 if lhs represents a directory whose name is a version higher than that of rhs, or rhs has a name that is not a version;
		/// -1 if lhs represents a directory whose name is a version less than that of rhs, or only rhs has a name that is a version;
		/// 0 if both have a name that is the same version or both have a name that is not a version.</returns>
		internal static int CompareVersion(string lhs, string rhs)
		{
			var rhsName = Path.GetFileName(rhs);
			return Version.TryParse(Path.GetFileName(lhs), out var lhsVer)
				? Version.TryParse(rhsName, out var rhsVer) ? lhsVer.CompareTo(rhsVer) : 1
				: Version.TryParse(rhsName, out _) ? -1 : 0;
		}
	}

	public interface IUpgradableSettings
	{
		/// <summary>
		/// True if the settings need to be upgraded
		/// </summary>
		bool CallUpgrade { get; set; }
	}
}