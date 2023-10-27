// Copyright (c) 2010-2023 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.PlatformUtilities;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// This class validates all of the varoius command line options that can be fed into the FLEx Bridge program.
	/// Some options are always expected, but others are used only in combination with another option.
	/// </summary>
	internal static class CommandLineProcessor
	{
// ReSharper disable InconsistentNaming
		internal const string u = "-u"; // the name to use in the commit log
		internal const string p = "-p"; // project directory
		internal const string v = "-v"; // command
		internal const string f = "-f"; // FixItAppPathname
		internal const string g = "-g"; // projectGuid
		internal const string projDir = "-projDir"; // projects directory
		internal const string fwAppsDir = "-fwAppsDir"; // The directory containing the FW executable
		internal const string fwmodel = "-fwmodel"; // Tell Flex Bridge which model version of data are expected by FLEx.
		internal const string liftmodel = "-liftmodel"; // LIFT model version number
		internal const string pipeID = "-pipeID";
		internal const string locale = "-locale";
		internal const string uri = "-uri"; // Full project URI
		internal const string project = "-project"; // Project name to clone into
		internal const string repositoryIdentifier = "-repositoryIdentifier"; // Identifier to match projects across repo's

		internal const string obtain = "obtain";						// -p <$fwroot>
		internal const string obtain_lift = "obtain_lift";				// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string send_receive = "send_receive";			// -p <$fwroot>\foo\foo.fwdata
		internal const string send_receive_lift = "send_receive_lift";	// -p <$fwroot>\foo\foo.fwdata
		internal const string view_notes = "view_notes";				// -p <$fwroot>\foo\foo.fwdata
		internal const string view_notes_lift = "view_notes_lift";		// -p <$fwroot>\foo\foo.fwdata
		internal const string undo_export = "undo_export";				// Not supported.
		internal const string undo_export_lift = "undo_export_lift";	// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string move_lift = "move_lift";					// -p <$fwroot>\foo\foo.fwdata
		internal const string check_for_updates = "check_for_updates";	// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string about_flex_bridge = "about_flex_bridge";	// -p <$fwroot>\foo where 'foo' is the project folder name
// ReSharper restore InconsistentNaming

		internal static Dictionary<string, string> ParseCommandLineArgs(ICollection<string> args)
		{
			var commandLineArgs = new Dictionary<string, string>();
			if (args != null && args.Count > 0)
			{
				string currentKey = null;
				foreach (var arg in args)
				{
					//not all options are followed by input, so just add them as a key
					if (arg.StartsWith("-") ||
						(Platform.IsWindows && arg.StartsWith("/")))
					{
						currentKey = arg.Trim();
						commandLineArgs[currentKey] = null;
					}
					else //this is input which apparently follows an option, added it as the value in the dictionary
					{
						if (currentKey != null && commandLineArgs[currentKey] == null)
						{
							//this option goes with the flag that came before it
							commandLineArgs[currentKey] = arg.Trim();
						}
						else //there was no flag before this option.
						{
							//This is an unparsable command line; signal FLEx or other apps
							throw new ApplicationException(CommonResources.kInvalidCommandLineOptions);
						}
					}
				}
			}
			ValidateCommandLineArgs(commandLineArgs);
			return commandLineArgs;
		}

		internal static void ValidateCommandLineArgs(Dictionary<string, string> commandLineArgs)
		{
			string pOption;
			string vOption;
			string projDirOption;
			ValidateBasicsForRequiredOptions(commandLineArgs,
				out pOption, out vOption, out projDirOption);

			var supportedOperations = new HashSet<string>
				{
					move_lift,
					about_flex_bridge,
					check_for_updates,
					obtain,
					obtain_lift,
					send_receive,
					send_receive_lift,
					// Not supported yet: undo_export,
					undo_export_lift,
					view_notes,
					view_notes_lift
				};

			// internal const string undo_export = "undo_export";				// Not supported.
			if (!supportedOperations.Contains(vOption))
				throw new CommandLineException("-v", "is not supported");

			// '-g' is required to NOT be present for any '-v' option cases, except 'move_lift'.
			if (vOption != move_lift)
			{
				if (commandLineArgs.ContainsKey(g))
					throw new CommandLineException("-g", "is present");
			}

			// '-f' is required to NOT be present for any '-v' option cases, except 'send_receive'.
			if (vOption != send_receive)
			{
				if (commandLineArgs.ContainsKey(f))
					throw new CommandLineException("-f", "is present");
			}

			string liftFolder;
			switch (vOption)
			{
				case move_lift:
					// internal const string move_lift = "move_lift";					// -p <$fwroot>\foo\foo.fwdata OR -p <$fwroot>\foo\foo.fwdb
					if (!commandLineArgs.ContainsKey(g) || String.IsNullOrEmpty(commandLineArgs[g]))
						throw new CommandLineException("-g", "is missing");
					var guid = Guid.Parse(commandLineArgs[g]); // Throws FormatException, if it isn't a guid.
					if (guid == Guid.Empty)
						throw new CommandLineException("-g", "is not a valid project guid");
					// Make sure it ends with some data file and that it exists.
					ValidatePOptionIsExtantFwXmlOrDb4oFile(pOption);
					break;

				case about_flex_bridge: // Fall through
					// internal const string about_flex_bridge = "about_flex_bridge";	// -p <$fwroot>\foo\foo.fwdata OR -p <$fwroot>\foo\foo.fwdb
				case check_for_updates:
					// internal const string check_for_updates = "check_for_updates";	// -p <$fwroot>\foo\foo.fwdata OR -p <$fwroot>\foo\foo.fwdb
					break; // Other options are irrelevant.  REVIEW (Hasso) 2014.01: would it be worthwhile to skip the basic validation for these?

				case obtain:
					//internal const string obtain = "obtain";						// -p <$fwroot>
					// xml or Db4o isn't relevant for this option.
					if (projDirOption != pOption)
						throw new CommandLineException("-v, -p and -projDir", "are incompatible, since '-p' and '-projDir' are different");
					break;

				case obtain_lift:
					// internal const string obtain_lift = "obtain_lift";				// -p <$fwroot>\foo where 'foo' is the project folder name
					// xml or Db4o isn't relevant for this option.
					if (Path.GetExtension(pOption) == ".fwdata")
						throw new CommandLineException("-v & -p", "are incompatible, since '-p' contains a Flex fwdata file");
					ValidatePOptionIsExtantFwProjectFolder(projDirOption, pOption);
					liftFolder = TriboroughBridgeUtilities.LiftOffset(pOption);
					if (Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is an extant Lift repository");
					break;

				case send_receive:
					// internal const string send_receive = "send_receive";			// -p <$fwroot>\foo\foo.fwdata
					// Must be xml file, not Db4o.
					ValidatePOptionIsExtantFwDataFile(pOption);
					// Must have -f option with fix it app in it.
					ValidateFOptionIsExtantFixItFile(commandLineArgs[fwAppsDir], commandLineArgs[f]);
					break;

				case send_receive_lift:
					// internal const string send_receive_lift = "send_receive_lift";	// -p <$fwroot>\foo\foo.fwdata OR -p <$fwroot>\foo\foo.fwdb
					ValidatePOptionIsExtantFwXmlOrDb4oFile(pOption);
					liftFolder = TriboroughBridgeUtilities.LiftOffset(Path.GetDirectoryName(pOption));
					if (!Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift folder");
					if (!Directory.GetFiles(liftFolder, "*.lift").Any())
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift file");
					break;

				case undo_export:
					throw new CommandLineException("-v", "'undo_export' is not supported");

				case undo_export_lift:
					// internal const string undo_export_lift = "undo_export_lift";	// -p <$fwroot>\foo where 'foo' is the project folder name
					// xml or Db4o isn't relevant for this option.
					ValidatePOptionIsExtantFwProjectFolder(projDirOption, pOption);
					liftFolder = TriboroughBridgeUtilities.LiftOffset(pOption);
					if (!Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift folder");
					if (!Directory.Exists(Path.Combine(liftFolder, ".hg")))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift repository");
					break;

				case view_notes:
					// internal const string view_notes = "view_notes";				// -p <$fwroot>\foo\foo.fwdata
					ValidatePOptionIsExtantFwDataFile(pOption);
					if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(pOption), ".hg")))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant FW repository, and thus, no main set of notes");
					break;

				case view_notes_lift:
					// internal const string view_notes_lift = "view_notes_lift";		// -p <$fwroot>\foo\foo.fwdata OR -p <$fwroot>\foo\foo.fwdb
					ValidatePOptionIsExtantFwXmlOrDb4oFile(pOption);
					liftFolder = TriboroughBridgeUtilities.LiftOffset(Path.GetDirectoryName(pOption));
					if (!Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift folder, and thus, no Lift notes");
					if (!Directory.Exists(Path.Combine(liftFolder, ".hg")))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift repository, and thus, no Lift notes");
					break;
			}
		}

		private static void ValidateBasicsForRequiredOptions(Dictionary<string, string> commandLineArgs,
			out string pOption, out string vOption, out string projDirOption)
		{
			if (!commandLineArgs.ContainsKey(u) || String.IsNullOrEmpty(commandLineArgs[u]))
				throw new CommandLineException("-u", "is missing");

			if (!commandLineArgs.ContainsKey(p) || String.IsNullOrEmpty(commandLineArgs[p]))
				throw new CommandLineException("-p", "is missing");
			pOption = commandLineArgs[p];
			if (!commandLineArgs.ContainsKey(v) || String.IsNullOrEmpty(commandLineArgs[v]))
				throw new CommandLineException("-v", "is missing");
			vOption = commandLineArgs[v];

			projDirOption = ValidateProjDirOption(commandLineArgs);

			if (!commandLineArgs.ContainsKey(fwAppsDir) || String.IsNullOrEmpty(commandLineArgs[fwAppsDir]))
				throw new CommandLineException("-fwAppsDir", "is missing");
			var fwAppsDirOption = commandLineArgs[fwAppsDir];
			if (!Directory.Exists(fwAppsDirOption))
				throw new CommandLineException("-fwAppsDir", "folder does not exist");

			if (vOption == send_receive)
			{
				if (!commandLineArgs.ContainsKey(f) || String.IsNullOrEmpty(commandLineArgs[f]))
					throw new CommandLineException("-f", "is missing");
				var fOption = commandLineArgs[f];
				const string fixitAppName = "FixFwData.exe";
				var fixitAppPathnameCalculated = Path.Combine(fwAppsDirOption, fixitAppName);
				if (fOption != fixitAppPathnameCalculated)
					throw new CommandLineException("-f & -fwAppsDir", "are mis-matched");
				if (!File.Exists(fixitAppPathnameCalculated))
					throw new CommandLineException("-f", "missing 'FixFwData.exe'");
			}

			// I believe this is actually only used by the "obtain" and possibly "obtain_lift" actions.
			// Send/receive, at least, does not use it, but obtains model version data from the fwdata file itself.
			// (JohnT, March 23, 2020).
			if (!commandLineArgs.ContainsKey(fwmodel) || String.IsNullOrEmpty(commandLineArgs[fwmodel]))
				throw new CommandLineException("-fwmodel", "is missing");
			var fwmodelOption = uint.Parse(commandLineArgs[fwmodel]);
			if (fwmodelOption < 7000066)
				throw new CommandLineException("-fwmodel", "is below the minimum supported FLEx data model version of 7000066");

			// Required
			if (!commandLineArgs.ContainsKey(locale) || String.IsNullOrEmpty(commandLineArgs[locale]))
				throw new CommandLineException("-locale", "is missing");

			// Required
			if (!commandLineArgs.ContainsKey(liftmodel) || String.IsNullOrEmpty(commandLineArgs[liftmodel]))
				throw new CommandLineException("-liftmodel", "is missing");
			// So that we can support a liftmodel parameter that looks like "0.13_ldml3"
			if (!commandLineArgs[liftmodel].StartsWith("0.13"))
				throw new CommandLineException("-liftmodel", "is not the supported FLEx LIFT model version of 0.13");

			// AddArg(ref args, "-pipeID", _pipeID);
			// Required
			if (!commandLineArgs.ContainsKey(pipeID) || String.IsNullOrEmpty(commandLineArgs[pipeID]))
				throw new CommandLineException("-pipeID", "is missing");
			// What else can be validated in 'pipeID'?
		}

		private static string ValidateProjDirOption(Dictionary<string, string> options)
		{
			if (!options.ContainsKey(projDir) || String.IsNullOrEmpty(options[projDir]))
				throw new CommandLineException("-projDir", "is missing");
			var projDirOption = options[projDir];
			if (!Directory.Exists(projDirOption))
				throw new CommandLineException("-projDir", "folder does not exist");
			return projDirOption;
		}

		private static void ValidatePOptionIsExtantFwDataFile(string pOption)
		{
			if (Path.GetExtension(pOption) != ".fwdata")
				throw new CommandLineException("-p", "has no fwdata file");
			if (!File.Exists(pOption))
				throw new CommandLineException("-p", "has no fwdata file");
		}

		private static void ValidatePOptionIsExtantFwXmlOrDb4oFile(string pOption)
		{
			var recognizedFwExtensions = new HashSet<string>
				{
					".fwdata",
					".fwdb"
				};
			if (!recognizedFwExtensions.Contains(Path.GetExtension(pOption)))
				throw new CommandLineException("-p", "has no fwdata/fwdb file");
			if (!File.Exists(pOption))
				throw new CommandLineException("-p", "has no fwdata/fwdb file");
		}

		private static void ValidateFOptionIsExtantFixItFile(string flexAppsDir, string fOption)
		{
			if (!fOption.StartsWith(flexAppsDir))
				throw new CommandLineException("-f & -fwAppsDir", "are mis-matched");
			if (Path.GetFileName(fOption) != "FixFwData.exe")
				throw new CommandLineException("-f", "has no 'fix it' program");
			if (!File.Exists(fOption))
				throw new CommandLineException("-f", "has no 'fix it' program");
		}

		private static void ValidatePOptionIsExtantFwProjectFolder(string projDirOption, string pOption)
		{
			if (projDirOption == pOption)
				throw new CommandLineException("-p", "is the same as '-projDir'");
			if (!Directory.Exists(pOption))
				throw new CommandLineException("-p", "is not an existing folder");
		}
	}
}
