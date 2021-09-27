// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace LibTriboroughBridgeChorusPlugin.Properties
{
	public sealed partial class Settings : IUpgradableSettings
	{
		/// <summary>
		/// Upgrades settings from previous versions, including versions before we started using CrossPlatformSettingsProvider.
		/// If settings need to be upgraded, finds old settings under %LocalAppData%/SIL_International/AppName_URL_CrazyLongGeneratedString or similar
		/// and copies them to %LocalAppData%/SIL/AppName, then upgrades settings.
		/// </summary>
		public static void UpgradeSettingsIfNecessary<T>(T settings, string appName) where T : ApplicationSettingsBase, IUpgradableSettings
		{
			if (!settings.CallUpgrade)
			{
				return;
			}

			MigrateNonCrossPlatformSettings(new FileSystem(), appName);

			settings.Upgrade();
			settings.CallUpgrade = false;
		}

		internal static void MigrateNonCrossPlatformSettings(IFileSystem fs, string appName)
		{
			// Find settings from earlier versions that didn't use CrossPlatformSettingsProvider
			// Copy the latest version's settings from each matching folder (don't overwrite)
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var oldCompanyDirInfo = fs.DirectoryInfo.FromDirectoryName(Path.Combine(localAppData,
				Platform.IsWindows ? "SIL_International" : "SIL International"));
			var oldAppDirs = PotentialOldAppSettingsDirs(oldCompanyDirInfo, appName).SelectMany(di => di.EnumerateDirectories()).ToList();
			if (!oldAppDirs.Any())
			{
				return;
			}

			var latestOldSettingsDir = LatestVersionSettingsDir(oldAppDirs);
			var newAppDirInfo = fs.DirectoryInfo.FromDirectoryName(Path.Combine(localAppData, Application.CompanyName, appName));
			var newSettingsDir = newAppDirInfo.CreateSubdirectory(latestOldSettingsDir.Name);
			const string settingsFileName = "user.config";
			var latestOldSettingsFile = fs.FileInfo.FromFileName(Path.Combine(latestOldSettingsDir.FullName, settingsFileName));
			var newSettingsFilePath = Path.Combine(newSettingsDir.FullName, settingsFileName);
			if (latestOldSettingsFile.Exists && !fs.File.Exists(newSettingsFilePath))
			{
				latestOldSettingsFile.CopyTo(newSettingsFilePath);
			}
		}

		internal static IEnumerable<IDirectoryInfo> PotentialOldAppSettingsDirs(IDirectoryInfo oldCompanyDir, string appName)
		{
			return oldCompanyDir.Exists ? oldCompanyDir.EnumerateDirectories($"{appName}.exe_*") : new IDirectoryInfo[0];
		}

		internal static IDirectoryInfo LatestVersionSettingsDir(List<IDirectoryInfo> versionSettingsDirs)
		{
			versionSettingsDirs.Sort(CompareVersion);
			return versionSettingsDirs.LastOrDefault();
		}

		/// <returns>
		/// 1 if lhs represents a directory whose name is a version higher than that of rhs, or rhs has a name that is not a version;
		/// -1 if lhs represents a directory whose name is a version less than that of rhs, or only rhs has a name that is a version;
		/// 0 if both have a name that is the same version or both have a name that is not a version.</returns>
		internal static int CompareVersion(IDirectoryInfo lhs, IDirectoryInfo rhs)
		{
			return Version.TryParse(lhs.Name, out var lhsVer)
				? Version.TryParse(rhs.Name, out var rhsVer) ? lhsVer.CompareTo(rhsVer) : 1
				: Version.TryParse(rhs.Name, out _) ? -1 : 0;
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