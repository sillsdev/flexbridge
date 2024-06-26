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
            "Name of .fwdata file to create, or directory to create it in"
        );
        rootCommand.Add(filename);

        var hgRevOption = new Option<string>(
            ["--rev", "-r"],
            "Revision to check out (default \"tip\")"
        );
        hgRevOption.SetDefaultValue("tip");
        rootCommand.Add(hgRevOption);

        var cleanupOption = new Option<bool>(
            ["--cleanup", "-c"],
            "Clean repository after creating .fwdata file (deletes every other file except .fwdata)"
        );
        rootCommand.Add(cleanupOption);

        rootCommand.SetHandler(Run, filename, verboseOption, quietOption, hgRevOption, cleanupOption);

        return await rootCommand.InvokeAsync(args);
    }

    static FileInfo LocateFwDataFile(string input)
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
            if (input.EndsWith(".fwdata")) return new FileInfo(input);
            return new FileInfo(input + ".fwdata");
        }
    }

    static Task<int> Run(string filename, bool verbose, bool quiet, string rev, bool cleanup)
    {
        IProgress progress = quiet ? new NullProgress() : new ConsoleProgress();
        progress.ShowVerbose = verbose;
        var file = LocateFwDataFile(filename);
        if (file.Exists) {
            progress.WriteWarning("File {0} already exists and will be overwritten", file.FullName);
        }
        var dir = file.Directory;
        if (dir == null || !dir.Exists) {
            progress.WriteError("Could not find directory {0}. MkFwData needs a Mercurial repo to work with.", dir?.FullName ?? "(null)");
            return Task.FromResult(1);
        }
        string name = file.FullName;
        progress.WriteMessage("Checking out {0}", rev);
        var result = HgRunner.Run($"hg checkout {rev}", dir.FullName, 30, progress);
        if (result.ExitCode != 0)
        {
            progress.WriteMessage("Could not find Mercurial repo in directory {0}. MkFwData needs a Mercurial repo to work with.", dir.FullName ?? "(null)");
            return Task.FromResult(result.ExitCode);
        }
        progress.WriteVerbose("Creating {0} ...", name);
        LfMergeBridge.LfMergeBridge.ReassembleFwdataFile(progress, writeVerbose: true, name);
        progress.WriteMessage("Created {0}", name);
        if (cleanup)
        {
            progress.WriteVerbose("Cleaning up...");
            HgRunner.Run($"hg checkout null", dir.FullName, 30, progress);
            HgRunner.Run($"hg purge --no-confirm --exclude *.fwdata --exclude hgRunner.log", dir.FullName, 30, progress);
        }
        return Task.FromResult(0);
    }
}
