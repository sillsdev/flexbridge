using Chorus.VcsDrivers.Mercurial;
using SIL.Progress;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Make or split .fwdata file");

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

        var splitCommand = new Command("split", "Split .fwdata file (push Humpty off the wall)");
        var buildCommand = new Command("build", "Rebuild .fwdata file (put Humpty together again)");

        rootCommand.Add(splitCommand);
        rootCommand.Add(buildCommand);

        var filename = new Argument<string>(
            "file",
            "Name of .fwdata file to create or split, or directory to create/split it in"
        );
        splitCommand.Add(filename);
        buildCommand.Add(filename);

        var hgRevOption = new Option<string>(
            ["--rev", "-r"],
            "Revision to check out (default \"tip\")"
        );
        hgRevOption.SetDefaultValue("tip");
        buildCommand.AddGlobalOption(hgRevOption);

        var timeoutOption = new Option<int>(
            ["--timeout", "-t"],
            "Timeout in seconds for Hg commands (default 600)"
        );
        timeoutOption.SetDefaultValue(600);
        buildCommand.AddGlobalOption(timeoutOption);

        var cleanupOption = new Option<bool>(
            ["--cleanup", "-c"],
            "Clean repository after creating .fwdata file (CAUTION: deletes every other file except .fwdata)"
        );
        buildCommand.Add(cleanupOption);

        buildCommand.SetHandler(BuildFwData, filename, verboseOption, quietOption, hgRevOption, timeoutOption, cleanupOption);

        var cleanupOptionForSplit = new Option<bool>(
            ["--cleanup", "-c"],
            "Delete .fwdata file after splitting"
        );
        splitCommand.Add(cleanupOptionForSplit);

        splitCommand.SetHandler(SplitFwData, filename, verboseOption, quietOption, cleanupOptionForSplit);

        return await rootCommand.InvokeAsync(args);
    }

    static FileInfo? MaybeLocateFwDataFile(string input)
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

    static FileInfo LocateFwDataFile(string input)
    {
        var result = MaybeLocateFwDataFile(input);
        if (result != null) return result;
        if (input.EndsWith(".fwdata")) return new FileInfo(input);
        return new FileInfo(input + ".fwdata");
    }

    static Task<int> SplitFwData(string filename, bool verbose, bool quiet, bool cleanup)
    {
        IProgress progress = quiet ? new NullProgress() : new ConsoleProgress();
        progress.ShowVerbose = verbose;
        var file = MaybeLocateFwDataFile(filename);
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
            if (fwdataFile.Exists) {
                fwdataFile.Delete();
                progress.WriteVerbose("Deleted {0}", fwdataFile.FullName);
            } else {
                progress.WriteVerbose("File not found, so not deleting: {0}", fwdataFile.FullName);
            }
        }
        return Task.FromResult(0);
    }

    static Task<int> BuildFwData(string filename, bool verbose, bool quiet, string rev, int timeout, bool cleanup)
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
        var result = HgRunner.Run($"hg checkout {rev}", dir.FullName, timeout, progress);
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
            HgRunner.Run($"hg checkout null", dir.FullName, timeout, progress);
            HgRunner.Run($"hg purge --no-confirm --exclude *.fwdata --exclude hgRunner.log", dir.FullName, timeout, progress);
        }
        return Task.FromResult(0);
    }
}
