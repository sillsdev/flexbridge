# FLEx Bridge

**FLEx Bridge** is an add-on to FieldWorks (https://software.sil.org/fieldworks/; https://github.com/sillsdev/FwDocumentation/wiki)
that supports using Chorus (https://github.com/sillsdev/chorus) to allow multiple users to share data.

## Architecture

The "big idea" for the bridge system is that clients do not have to know how talk to *Chorus* directly to do Send/Receive(S/R), or even know that *Chorus* is involved. The architecture allows for any number of "bridges" between clients and *Chorus*, with a new bridge being needed for each major kind of xml data (e.g., *LIFT* or *FLEx*'s fwdata). Each bridge needs to define *Handlers* for the xml file extensions it will have in its data. *Chorus* uses those handlers in its work. Each bridge also needs to define *Actions*, which are the kinds of things it wants to do for S/R.

*Language Forge* (LF) uses *LfMergeBridge* to S/R the full *FLEx* data set (LCModel, fwdata file). LF needs a *Windows.Forms*-free environment since it runs on a server.

*FLEx* uses the *FLExBridge* exe to S/R its own data set and for *LIFT* (also compatible with *WeSay*). *WeSay* talks directly to *Chorus* and does not use the *FLExBridge* exe.

The *TriboroughBridge* project (named after the RFK Triborough Bridge complex connecting three boroughs in New York) contains pieces that are applicable to both *FLExBridge* (Full LCModel) and *LiftBridge* (*LIFT* model, also compatible with *WeSay*).

See diagram:
![FLExBridge Projects Relationships](FLExBridgeRepo.svg)

## Development

### Setup

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

#### Mercurial

To run FLExBridge you must unzip `chorus/lib/common/Mercurial.zip` to the root of flexbridge.  Then, edit the `mercurial.ini`
file in the Mercurial folder. Add a line like this (with the appropriate path for your flexbridge folder):

	fixutf8 = C:\Dev\flexbridge\MercurialExtensions\fixutf8\fixutf8.py

Note that this is in addition to unzipping this folder per the Chorus ReadMe.

#### Connecting FieldWorks to FLExBridge

- In **Windows**, add the following keys to your registry (32-bit OS: omit 'Wow6432Node\'):
[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge\9]
	"InstallationDir"="C:\Dev\flexbridge\output\Debug"
- In **Linux**, `export FLEXBRIDGEDIR=/home/YOURUSERNAME/fwrepo/flexbridge/output/DebugMono`

Also, if you are working on Chorus, set up the FieldWorks build to copy locally-built Chorus and Palaso artifacts
(instructions are located in the [FwDocumentation wiki](https://github.com/sillsdev/FwDocumentation/wiki)).

### Build

* In **Windows**, build solution FLExBridge.sln from Visual Studio 2015 Community Edition.

## Updating Release Notes for a new version

FLExBridge is following the gitflow model for branching

When releasing FLExBridge be sure to do the following:

1. Update the version and changelogs / release notes.

	- For Windows:
		- If you are making a major or minor version number jump, update the the first two digits in `version` (the fourth-place version number is controlled by the TeamCity build counter)
		- Update the src/Installer/ReleaseNotes.md with the user-facing change information, adding another heading for the previous version
        - Run the following to update dependant Release Notes files:

                @REM this sets up the path to msbuild. Check GetAndBuildThis.bat for the latest path to vsvars32.bat
				"%VS140COMNTOOLS%vsvars32.bat"
				@REM Replace Alpha here with Beta or Stable as appropriate. Pass Release=false for a pre-release
				msbuild build/FLExBridge.proj  /t:PreparePublishingArtifacts /p:UploadFolder=Alpha /p:Release=true

		- Generate a new Product ID GUID in `build/WixPatchableInstaller.targets`
		- Tag and Pin the FLEx Bridge Installer build on TeamCity, then update the FLEx Bridge Patcher build to depend on that tag

	- For Linux:

		- `cd ~/fwrepo/flexbridge`
		- Set new version number, such as:

            `echo 2.5.1 > version`

		- Create an entry atop ReleaseNotes.md:

			`sed -i "1i ## $(cat version) UNRELEASED\n\n* New version\n" src/Installer/ReleaseNotes.md`

		- Edit src/Installer/ReleaseNotes.md , replacing 'New version.'

		- `CHANNEL=Alpha` # or Beta or Stable. On 2016-12-16 we are using Alpha for Dictionary branch.

		- Fill in debian/changelog and ReleaseNotes.md, make html file (for a pre-release pass `/p:Release=false` as additional property):

			`(source environ && cd build && xbuild FLExBridge.proj /t:PreparePublishingArtifacts /p:UploadFolder=$CHANNEL)`

2. Build

	- The Windows version is released through two jobs on TeamCity: "Installer" and "Patcher". The first three version numbers come from the `version` file; the fourth-place version number is always 1 for "Installer" and comes from the build counter for "Patcher". If you need to make a fix before publishing a patch, you can avoid incrementing the version number by setting the build counter back before rerunning the Patcher job.

	- Make a Linux package for release by doing the following:
		- Go to the Jenkins job for this branch of flexbridge.
		- Click Build with Parameters.
		- Change Suite to "main" (or maybe "updates" for a hotfix).
		- Unselect AppendNightlyToVersion.
		- Optionally set Committish to an older commit, such as where the changelog entry was updated.
		- Click Build.