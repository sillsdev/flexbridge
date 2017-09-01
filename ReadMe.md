**FLExBridge** is an add-on to FieldWorks (http://fieldworks.sil.org; https://github.com/sillsdev/FwDocumentation/wiki)
that supports using Chorus (https://github.com/sillsdev/chorus) to allow multiple users to share data.

## Build notes:
FLEx Bridge depends on several assemblies from Chorus and Palaso.
Versions of these assemblies are no longer in the repo.
Therefore, to build FLEx Bridge, you must get the latest versions of these assemblies by running this in a Bash window:

Windows
download_dependencies_windows.sh

Linux
download_dependencies_linux.sh

If necessary, both download_dependencies can be updated using the tool at https://github.com/chrisvire/BuildUpdate (requires Ruby).

If you plan to work on Chorus,
- clone the Chorus and LibPalaso repos from https://github.com/sillsdev/chorus and https://github.com/sillsdev/libpalaso into the
   same parent directory as flexbridge without changing their repository names
- run GetAndBuildThis.bat to: Download the latest commit on your branch of FLExBridge (if you have no uncommitted changes),
   GetAndBuild LibPalaso and Chorus recursively, copy dependencies from LibPalaso to Chorus to FLExBridge,
   and build FLExBridge

### Special Mono dependencies:
	$ cp ../libpalaso/lib/Debug/icu.net.dll* ../libpalaso/lib/DebugMono
	$ PATH=/usr/bin:$PATH make [debug|release] #This will prefer the System Mono over fieldworks-mono

### Mercurial
To run FLExBridge you must unzip `chorus/lib/common/Mercurial.zip` to the root of flexbridge.  Then, edit the `mercurial.ini`
file in the Mercurial folder. Add a line like this (with the appropriate path for your flexbridge folder):

	fixutf8 = C:\Dev\flexbridge\MercurialExtensions\fixutf8\fixutf8.py

Note that this is in addition to unzipping this folder per the Chorus ReadMe.

## Connecting FieldWorks to FLExBridge:
Add the following keys to your registry (32-bit OS: omit 'Wow6432Node\', Mono: export env var FLEXBRIDGEDIR):

[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge\9]
	"InstallationDir"="C:\Dev\flexbridge\output\Debug"

Also, if you are working on Chorus, set up the FieldWorks build to copy locally-built Chorus and Palaso artifacts (instructions are located in the [FwDocumentation wiki](https://github.com/sillsdev/FwDocumentation/wiki))

## Updating Release Notes for a new version

When releasing FLExBridge be sure to do the following:

1. Update the version and changelogs / release notes.
    * If in Windows:
        1. If you are making a major or minor version number jump, update the the first two digits in `version`
        - Update the src/Installer/ReleaseNotes.md with the user-facing change information, adding another heading for the previous version
        - Run the following to update dependant Release Notes files:

                @REM this sets up the path to msbuild. Check GetAndBuildThis.bat for the latest path to vsvars32.bat
                "%VS120COMNTOOLS%vsvars32.bat"
                @REM Replace Alpha here with Beta or Stable as appropriate.
                msbuild build/build.common.proj  /t:PreparePublishingArtifacts /p:UploadFolder=Alpha /p:RootDir=..

    * If in Linux:

        1. `cd ~/fwrepo/flexbridge`
        * Set new version number, such as:

            `echo 2.5.1 > version`

        * Create an entry atop ReleaseNotes.md:

            `sed -i '1i ##\n* New version.' src/Installer/ReleaseNotes.md`

        * Edit src/Installer/ReleaseNotes.md , replacing 'New version.'

        * `CHANNEL=Alpha` # or Beta or Stable. On 2016-12-16 we are using Alpha for Dictionary branch.
        * Fill in debian/changelog and ReleaseNotes.md, make html file:

            `(source environ && cd build && xbuild build.common.proj /t:PreparePublishingArtifacts /p:RootDir=.. /p:UploadFolder=$CHANNEL)`

- The windows version is released through two jobs in TeamCity: "Installer-sans Publish" and "Publish Installer"; the final version number comes from the TC job on "Installer-sans Publish". If you need to make a fix before publishing, you can avoid incrementing the version number by setting the buid counter back on the Installer-sans Publish job and re-running it before running the publish job.
- Make a Linux package for release by doing the following.

    1. Go to the Jenkins job for this branch of flexbridge.
    * Click Build with Parameters.
    * Change Suite to "main" (or maybe "updates" for a hotfix).
    * Unselect AppendNightlyToVersion.
    * Optionally set Committish to an older commit, such as where the changelog entry was updated.
    * Click Build.
