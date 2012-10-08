using System;
using System.Collections.Generic;
using System.Text;
using SIL.LiftBridge.Properties;

namespace LiftBridge
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// args are:
			// -u username [Required]
			// -p entire pathname to fwdata file including extension. [Required]
			// -v kind of S/R operation: obtain, start, send_receive, view_notes [Required]
			// -lpid Guid of the Language Project. [Optional. If present, the LB will try and move the extant repo from its hiding spot into the SharedData\LIFT folder.]
			// No args at all: Error, as Lift Bridge must be run by an app like FLEx.
		}

		private static Dictionary<string, string> ParseCommandLineArgs(string[] args)
		{
			var options = new Dictionary<string, string>();
			if (args != null && args.Length > 0)
			{
				string currentKey = null;
				foreach (var arg in args)
				{
					//not all options are followed by input, so just add them as a key
					if (arg.StartsWith("-") || arg.StartsWith("/"))
					{
						currentKey = arg;
						options[currentKey] = null;
					}
					else //this is input which apparently follows an option, added it as the value in the dictionary
					{
						if (currentKey != null && options[currentKey] == null)
						{
							//this option goes with the flag that came before it
							options[currentKey] = arg;
						}
						else //there was no flag before this option.
						{
							//This is an unparsable command line
							Console.WriteLine(Resources.kInvalidCommandLine);
							//Signal FLEx or other apps
							throw new ApplicationException("Invalid command line options.");
						}
					}
				}
			}
			return options;
		}
	}
}
