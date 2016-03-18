**FLExBridge** is an add-on to FieldWorks (http://fieldworks.sil.org; https://github.com/sillsdev/FwDocumentation/wiki)
that supports using Chorus (https://github.com/sillsdev/chorus) to allow multiple users to share data.

## Build notes
FLEx Bridge depends on several assemblies from Chorus and Palaso.
Versions of these assemblies are no longer in the repo.
Therefore, to build FLEx Bridge, you must get the latest versions of these assemblies by running this in a Bash window:

- In **Windows**, run `download_dependencies_windows.sh`
- In **Linux**, run `download_dependencies_linux.sh`

If necessary, both *download_dependencies* scripts can be updated using the tool at https://github.com/chrisvire/BuildUpdate (requires Ruby).

If you plan to work on Chorus:

- Clone the Chorus and LibPalaso repos from https://github.com/sillsdev/chorus and https://github.com/sillsdev/libpalaso into the same parent directory as flexbridge without changing their repository names.
- In **Windows**, run `GetAndBuildThis.bat` to: Download the latest commit on your branch of FLExBridge (if you have no uncommitted changes), GetAndBuild LibPalaso and Chorus recursively, copy dependencies from LibPalaso to Chorus to FLExBridge, and build FLExBridge.
- In **Linux**, run `UpdateDependencies.sh`, then build in *MonoDevelop* using `FLExBridge VS2010.sln`. You may also need to create the *localizations* folder here `/home/YOURUSERNAME/fwrepo/flexbridge/output/DebugMono/localizations`.

### Mercurial
To run FLExBridge you must unzip `chorus/lib/common/Mercurial.zip` to the root of flexbridge.  Then, edit the `mercurial.ini`
file in the Mercurial folder. Add a line like this (with the appropriate path for your flexbridge folder):

	fixutf8 = C:\Dev\flexbridge\MercurialExtensions\fixutf8\fixutf8.py

Note that this is in addition to unzipping this folder per the Chorus ReadMe.

## Connecting FieldWorks to FLExBridge

- In **Windows**, add the following keys to your registry (32-bit OS: omit 'Wow6432Node\'):
[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge\9]
	"InstallationDir"="C:\Dev\flexbridge\output\Debug"
- In **Linux**, `export FLEXBRIDGEDIR=/home/YOURUSERNAME/fwrepo/flexbridge/output/DebugMono`

Also, if you are working on Chorus:

- Copy Chorus.exe, LibChorus.dll, and Palaso dll's to C:\fwrepo\fw\output\Debug (or Release).  You can do this in **Windows** using the    `UpdateFLExDependencies.bat` script in the flexbridge repo (in **Linux**, run `UpdateFLExDependencies.sh`) . These steps are required for only those dependencies bound at compile time (e.g. API changes):
- Rebuild FLEx

## Release Notes:
When releasing FLExBridge be sure to do the following:

- Update the ReleaseNotes.md and [ReleaseType].htm for the ReleaseType e.g. Alpha, Beta, Stable with the user facing change information.
- If you are making a major or minor version number jump update the the first two digits in build/build.common.proj
- The windows version is released through two jobs in TeamCity, "Installer-sans Publish" and "Publish Installer", the final version number comes from the TC job on "Installer-sans Publish". If you need to make a fix before publishing you can avoid incrementing the version number by setting the buid counter back on the Installer-sans Publish job and re-running it before running the publish job.
- For the Linux release update the Makefile and add a debian/changelog entry and then use the Jenkins FLExBridge release package build to make packages from the commit where the changelog entry was updated.
