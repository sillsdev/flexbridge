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

        var file = new Argument<FileSystemInfo>(
            "file",
            "Name of .fwdata file to split"
        );
        rootCommand.Add(file);

        var cleanupOption = new Option<bool>(
            ["--cleanup", "-c"],
            "Delete .fwdata file after splitting"
        );
        rootCommand.Add(cleanupOption);

        rootCommand.SetHandler(Run, file, verboseOption, quietOption, cleanupOption);

        return await rootCommand.InvokeAsync(args);
    }

    static Task<int> Run(FileSystemInfo file, bool verbose, bool quiet, bool cleanup)
    {
        IProgress progress = quiet ? new NullProgress() : new ConsoleProgress();
        progress.ShowVerbose = verbose;
        bool isDir = file.Exists && (file.Attributes & FileAttributes.Directory) != 0;
        string name = isDir ? Path.Join(file.FullName, file.Name + ".fwdata") : file.FullName;
        string dir = isDir ? file.FullName : new FileInfo(file.FullName).Directory!.FullName;
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
