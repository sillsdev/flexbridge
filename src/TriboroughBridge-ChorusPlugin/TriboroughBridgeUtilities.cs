// Copyright (c) 2010-2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus;
using Chorus.sync;
using L10NSharp;
using LibTriboroughBridgeChorusPlugin;
using SIL.IO;
using SIL.Reporting;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// This class holds constants and methods that are relevant to common bridge operations.
	/// A lot of what it had held earlier, was moved into places like Flex Bridge's SharedConstants class or
	/// into Lift Bridge's LiftUtilities class, when the stuff was only used by one bridge.
	///
	/// Some of the remaining constants could yet be moved at the cost of having duplicates in each bridge's project.
	/// It may be worth that to be rid of bridge-specific stuff in this project.
	/// </summary>
	internal static class TriboroughBridgeUtilities
	{
// ReSharper disable InconsistentNaming
		internal const string LIFT = "LIFT";
		internal const string hg = ".hg";
		private const string FlexBridge = "FlexBridge";
		private const string localizations = "localizations";
		internal const string FlexBridgeEmailAddress = "flex_errors@sil.org";
		internal const string FlexLocalizationEmailAddress = "flex_localization@sil.org";
// ReSharper restore InconsistentNaming

		internal static XElement CreateFromBytes(byte[] xmlData)
		{
			using (var memStream = new MemoryStream(xmlData))
			{
				// This loads the MemoryStream as Utf8 xml. (I checked.)
				return XElement.Load(memStream);
			}
		}

		/// <summary>
		/// Creates and initializes the ChorusSystem for use in FLExBridge
		/// </summary>
		internal static ChorusSystem InitializeChorusSystem(string directoryName, string user, Action<ProjectFolderConfiguration> configure)
		{
			var system = new ChorusSystem(directoryName);
			system.Init(user);
			configure?.Invoke(system.ProjectFolderConfiguration);
			return system;
		}

		internal static string HgDataFolder(string path)
		{
			return Path.Combine(path, hg, "store", "data");
		}

		internal static string LiftOffset(string path)
		{
			var otherPath = Path.Combine(path, LibTriboroughBridgeSharedConstants.OtherRepositories);
			if (Directory.Exists(otherPath))
			{
				var extantLiftFolder = Directory.GetDirectories(otherPath).FirstOrDefault(subfolder => subfolder.EndsWith("_LIFT"));
				if (extantLiftFolder != null)
					return extantLiftFolder;
			}
			return Path.Combine(path, LibTriboroughBridgeSharedConstants.OtherRepositories, Path.GetFileName(path) + "_" + LIFT);
		}

		internal static bool FolderIsEmpty(string folder)
		{
			return Directory.GetDirectories(folder).Length == 0 && Directory.GetFiles(folder).Length == 0;
		}

		internal static Dictionary<string, ILocalizationManager> SetupLocalization(Dictionary<string, string> commandLineArgs)
		{
			var results = new Dictionary<string, ILocalizationManager>(3);

			try
			{
				var desiredUiLangId = commandLineArgs[CommandLineProcessor.locale];
				// ReSharper disable InconsistentNaming
				var installedL10nBaseDir = Path.Combine(
					// ReSharper disable once AssignNullToNotNullAttribute
					Path.GetDirectoryName(PathHelper.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase)), localizations);
				var userL10nBaseDir = Path.Combine("SIL", FlexBridge);
				// ReSharper restore InconsistentNaming

				LocalizationManager.ReturnOnlyApprovedStrings = true;

				// Now set it up for the handful of localizable elements in FlexBridge itself.
				// This is safer than Application.ProductVersion, which might contain words like 'alpha' or 'beta',
				// which (on the SECOND run of the program) fail when L10NSharp tries to make a Version object out of them.
				var versionObj = Assembly.GetExecutingAssembly().GetName().Version;
				// We don't need to reload strings for every "revision" (that might be every time we build). REVIEW (Hasso) 2021.08: then why do we have `build`?
				var version = $"{versionObj.Major}.{versionObj.Minor}.{versionObj.Build}";
				var flexBridgeLocMan = LocalizationManager.Create(TranslationMemory.XLiff, desiredUiLangId, FlexBridge, Application.ProductName,
					version, installedL10nBaseDir, userL10nBaseDir, CommonResources.chorus,
					FlexLocalizationEmailAddress, FlexBridge, "TriboroughBridge_ChorusPlugin", "FLEx_ChorusPlugin", "SIL.LiftBridge");
				results.Add("FlexBridge", flexBridgeLocMan);

				// In case the UI language was unavailable, change it, so we don't frustrate the user with three dialogs.
				desiredUiLangId = LocalizationManager.UILanguageId;

				versionObj = Assembly.GetAssembly(typeof(ChorusSystem)).GetName().Version;
				version = "" + versionObj.Major + "." + versionObj.Minor + "." + versionObj.Build;
				var chorusLocMan = LocalizationManager.Create(TranslationMemory.XLiff, desiredUiLangId, "Chorus", "Chorus",
					version, installedL10nBaseDir, userL10nBaseDir, CommonResources.chorus, FlexLocalizationEmailAddress, "Chorus");
				results.Add("Chorus", chorusLocMan);

				versionObj = Assembly.GetAssembly(typeof(ErrorReport)).GetName().Version;
				version = "" + versionObj.Major + "." + versionObj.Minor + "." + versionObj.Build;
				var palasoLocMan = LocalizationManager.Create(TranslationMemory.XLiff, desiredUiLangId, "Palaso", "Palaso",
					version, installedL10nBaseDir, userL10nBaseDir, CommonResources.chorus, FlexLocalizationEmailAddress, "SIL");
				results.Add("Palaso", palasoLocMan);
			}
			catch (Exception e)
			{
				// For some reason, even though ErrorReport has been initialized, calling it here throws an exception.
				// ReSharper disable LocalizableElement - not localizable if l10n setup failed!
				MessageBox.Show(
					$"There was a problem setting up localizations, probably because they were not installed. Please report this to {FlexBridgeEmailAddress}." +
					$"{Environment.NewLine}{Environment.NewLine}Details:{Environment.NewLine}{e}",
					"Error Localizing FLEx Bridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
				// ReSharper restore LocalizableElement
			}

			return results;
		}
	}
}
