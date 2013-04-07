using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin
{
	public static class CommandLineProcessor
	{
// ReSharper disable InconsistentNaming
		internal const string u = "-u";
		internal const string p = "-p";
		internal const string v = "-v";
		internal const string f = "-f";
		internal const string g = "-g";
		internal const string projDir = "-projDir";
		internal const string fwAppsDir = "-fwAppsDir";
		internal const string fwmodel = "-fwmodel";
		internal const string liftmodel = "-liftmodel";
		internal const string pipeID = "-pipeID";

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
		/*
			AddArg(ref args, "-u", userNameActual);
			AddArg(ref args, "-p", projectFolder); (See above)
			AddArg(ref args, "-v", command);
			AddArg(ref args, "-f", FixItAppPathname);
			AddArg(ref args, "-g", projectGuid);
			AddArg(ref args, "-projDir", DirectoryFinder.ProjectsDirectory);
			AddArg(ref args, "-fwAppsDir", FieldWorksAppsDir);
			// Tell Flex Bridge which model version of data are expected by FLEx.
			AddArg(ref args, "-fwmodel", fwmodelVersionNumber.ToString());
			AddArg(ref args, "-liftmodel", liftModelVersionNumber);
			AddArg(ref args, "-pipeID", _pipeID);
			*/
// ReSharper restore InconsistentNaming

		public static Dictionary<string, string> ParseCommandLineArgs(ICollection<string> args)
		{
			var options = new Dictionary<string, string>();
			if (args != null && args.Count > 0)
			{
				string currentKey = null;
				foreach (var arg in args)
				{
					//not all options are followed by input, so just add them as a key
					if(arg.StartsWith("-") || arg.StartsWith("/"))
					{
						currentKey = arg.Trim();
						options[currentKey] = null;
					}
					else //this is input which apparently follows an option, added it as the value in the dictionary
					{
						if (currentKey != null && options[currentKey] == null)
						{
							//this option goes with the flag that came before it
							options[currentKey] = arg.Trim();
						}
						else //there was no flag before this option.
						{
							//This is an unparsable command line
							Console.WriteLine(CommonResources.kInvalidCommandLineOptions);
							//Signal FLEx or other apps
							throw new ApplicationException(String.Format("Invalid command line options: {0}.", args));
						}
					}
				}
			}
			ValidateCommandLineArgs(options);
			return options;
		}

		internal static void ValidateCommandLineArgs(Dictionary<string, string> options)
		{
			string pOption;
			string vOption;
			string projDirOption;
			ValidateBasicsForRequiredOptions(options,
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
					// Not supported yet. undo_export,
					undo_export_lift,
					view_notes,
					view_notes_lift
				};

			// internal const string undo_export = "undo_export";				// Not supported.
			if (!supportedOperations.Contains(vOption))
				throw new CommandLineException("-v", "is not supported");

			// Required to NOT be present for any '-v' option cases, except 'move_lift'.
			if (vOption != move_lift)
			{
				if (options.ContainsKey(g))
					throw new CommandLineException("-g", "is present");
			}

			// Required to NOT be present for any '-v' option cases, except 'move_lift'.
			if (vOption != send_receive)
			{
				if (options.ContainsKey(f))
					throw new CommandLineException("-f", "is present");
			}

			string liftFolder;
			switch (vOption)
			{
				case move_lift:
					// internal const string move_lift = "move_lift";					// -p <$fwroot>\foo\foo.fwdata
					if (!options.ContainsKey(g) || String.IsNullOrEmpty(options[g]))
						throw new CommandLineException("-g", "is missing");
					var guid = Guid.Parse(options[g]); // Throws FormatException, if it isn't a guid.
					if (guid == Guid.Empty)
						throw new CommandLineException("-g", "is not a valid project guid");
					// Make sure it ends with some data file and that it exists.
					ValidatePOptionIsExtantFwDataFile(pOption);
					break;

				case about_flex_bridge: // Fall through
					// internal const string about_flex_bridge = "about_flex_bridge";	// -p <$fwroot>\foo\foo.fwdata
				case check_for_updates:
					// internal const string check_for_updates = "check_for_updates";	// -p <$fwroot>\foo\foo.fwdata
					ValidatePOptionIsExtantFwDataFile(pOption);
					ValidatePOptionIsExtantFwProjectFolder(projDirOption, Path.GetDirectoryName(pOption));
					break;

				case obtain:
					//internal const string obtain = "obtain";						// -p <$fwroot>
					var projectBaseDir = ValidateProjDirOption(options);
					if (projectBaseDir != options[p])
						throw new CommandLineException("-v, -p and -projDir", "are incompatible, since '-p' and '-projDir' are different");
					break;

				case obtain_lift:
					// internal const string obtain_lift = "obtain_lift";				// -p <$fwroot>\foo where 'foo' is the project folder name
					if (Path.GetExtension(pOption) == ".fwdata")
						throw new CommandLineException("-v & -p", "are incompatible, since '-p' contains a Flex fwdata file");
					ValidatePOptionIsExtantFwProjectFolder(projDirOption, pOption);
					liftFolder = Utilities.LiftOffset(pOption);
					if (Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is an extant Lift repository");
					break;

				case send_receive:
					// internal const string send_receive = "send_receive";			// -p <$fwroot>\foo\foo.fwdata
					ValidatePOptionIsExtantFwDataFile(pOption);
					// Must have -f option with fix it app in it.
					ValidateFOptionIsExtantFixItFile(options[fwAppsDir], options[f]);
					break;

				case send_receive_lift:
					// internal const string send_receive_lift = "send_receive_lift";	// -p <$fwroot>\foo\foo.fwdata
					ValidatePOptionIsExtantFwDataFile(pOption);
					liftFolder = Utilities.LiftOffset(Path.GetDirectoryName(pOption));
					if (!Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift folder");
					if (!Directory.GetFiles(liftFolder, "*.lift").Any())
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift file");
					break;

				case undo_export:
					throw new CommandLineException("-v", "'undo_export' is not supported");

				case undo_export_lift:
					// internal const string undo_export_lift = "undo_export_lift";	// -p <$fwroot>\foo where 'foo' is the project folder name
					ValidatePOptionIsExtantFwProjectFolder(projDirOption, pOption);
					liftFolder = Utilities.LiftOffset(pOption);
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
					// internal const string view_notes_lift = "view_notes_lift";		// -p <$fwroot>\foo\foo.fwdata
					ValidatePOptionIsExtantFwDataFile(pOption);
					liftFolder = Utilities.LiftOffset(Path.GetDirectoryName(pOption));
					if (!Directory.Exists(liftFolder))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift folder, and thus, no Lift notes");
					if (!Directory.Exists(Path.Combine(liftFolder, ".hg")))
						throw new CommandLineException("-v & -p", "are incompatible, since there is no extant Lift repository, and thus, no Lift notes");
					break;
			}
		}

		private static void ValidateBasicsForRequiredOptions(Dictionary<string, string> options,
			out string pOption, out string vOption, out string projDirOption)
		{
			if (!options.ContainsKey(u) || String.IsNullOrEmpty(options[u]))
				throw new CommandLineException("-u", "is missing");

			if (!options.ContainsKey(p) || String.IsNullOrEmpty(options[p]))
				throw new CommandLineException("-p", "is missing");
			pOption = options[p];
			if (!options.ContainsKey(v) || String.IsNullOrEmpty(options[v]))
				throw new CommandLineException("-v", "is missing");
			vOption = options[v];

			projDirOption = ValidateProjDirOption(options);

			if (!options.ContainsKey(fwAppsDir) || String.IsNullOrEmpty(options[fwAppsDir]))
				throw new CommandLineException("-fwAppsDir", "is missing");
			var fwAppsDirOption = options[fwAppsDir];
			if (!Directory.Exists(fwAppsDirOption))
				throw new CommandLineException("-fwAppsDir", "folder does not exist");

			if (vOption == send_receive)
			{
				if (!options.ContainsKey(f) || String.IsNullOrEmpty(options[f]))
					throw new CommandLineException("-f", "is missing");
				var fOption = options[f];
				const string fixitAppName = "FixFwData.exe";
				var fixitAppPathnameCalculated = Path.Combine(fwAppsDirOption, fixitAppName);
				if (fOption != fixitAppPathnameCalculated)
					throw new CommandLineException("-f & -fwAppsDir", "are mis-matched");
				if (!File.Exists(fixitAppPathnameCalculated))
					throw new CommandLineException("-f", "missing 'FixFwData.exe'");
			}

			if (!options.ContainsKey(fwmodel) || String.IsNullOrEmpty(options[fwmodel]))
				throw new CommandLineException("-fwmodel", "is missing");
			var fwmodelOption = uint.Parse(options[fwmodel]);
			if (fwmodelOption < 7000066)
				throw new CommandLineException("-fwmodel", "is below the minimum supported FLEx data model version of 7000066");

			// Required
			if (!options.ContainsKey(liftmodel) || String.IsNullOrEmpty(options[liftmodel]))
				throw new CommandLineException("-liftmodel", "is missing");
			if (options[liftmodel] != "0.13")
				throw new CommandLineException("-liftmodel", "is not the supported FLEx LIFT model version of 0.13");

			// AddArg(ref args, "-pipeID", _pipeID);
			// Required
			if (!options.ContainsKey(pipeID) || String.IsNullOrEmpty(options[pipeID]))
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
			if (!pOption.StartsWith(projDirOption))
				throw new CommandLineException("-p", "is not contained within '-projDir'");
			if (!Directory.Exists(pOption))
				throw new CommandLineException("-p", "is not an existing folder");
		}
	}
}