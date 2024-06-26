using Chorus.VcsDrivers.Mercurial;
using SIL.Progress;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Make .fwdata file");

        var verboseOption = new Option<bool>(
            ["--verbose", "-v"],
            "Display verbose output"
        );
        rootCommand.AddGlobalOption(verboseOption);

        var quietOption = new Option<bool>(
            ["--quiet", "-q"],
            "Suppress all output (overrides --verbose if present)"
        );
        rootCommand.AddGlobalOption(quietOption);

        var filename = new Argument<string>(
            "file",
            "Name of .fwdata file to split"
        );
        rootCommand.Add(filename);

        var cleanupOption = new Option<bool>(
            ["--cleanup", "-c"],
            "Delete .fwdata file after splitting"
        );
        rootCommand.Add(cleanupOption);

        rootCommand.SetHandler(Run, filename, verboseOption, quietOption, cleanupOption);

        return await rootCommand.InvokeAsync(args);
    }

    static FileInfo? LocateFwDataFile(string input)
    {
        if (Directory.Exists(input)) {
            var dirInfo = new DirectoryInfo(input);
            var fname = dirInfo.Name + ".fwdata";
            return new FileInfo(Path.Join(input, fname));
        } else if (File.Exists(input)) {
            return new FileInfo(input);
        } else if (File.Exists(input + ".fwdata")) {
            return new FileInfo(input + ".fwdata");
        } else {
            return null;
        }
    }

    static Task<int> Run(string filename, bool verbose, bool quiet, bool cleanup)
    {
        IProgress progress = quiet ? new NullProgress() : new ConsoleProgress();
        progress.ShowVerbose = verbose;
        var file = LocateFwDataFile(filename);
        if (file == null || !file.Exists) {
            progress.WriteError("Could not find {0}", filename);
            return Task.FromResult(1);
        }
        string name = file.FullName;
        progress.WriteVerbose("Splitting {0} ...", name);
        LfMergeBridge.LfMergeBridge.DisassembleFwdataFile(progress, writeVerbose: true, name);
        progress.WriteMessage("Finished splitting {0}", name);
        if (cleanup)
        {
            progress.WriteVerbose("Cleaning up...");
            var fwdataFile = new FileInfo(name);
            if (fwdataFile.Exists) { fwdataFile.Delete(); progress.WriteVerbose("Deleted {0}", fwdataFile.FullName); } else { progress.WriteVerbose("File not found, so not deleting: {0}", fwdataFile.FullName); }
        }
        return Task.FromResult(0);
    }
}
