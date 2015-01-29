FLExBridge is an add-on to FieldWorks (http://fieldworks.sil.org; https://github.com/sillsdev/FwDocumentation/wiki)
that supports using Chorus (https://github.com/sillsdev/chorus) to allow multiple users to share data.

***Build notes:***
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
   same parent directory without changing their repository names
- run GetAndBuildThis.bat to: Download the latest commit on your branch of FLExBridge, GetAndBuild LibPalaso and Chorus
   recursively, copy dependencies from LibPalaso to Chorus to FLExBridge, and build FLExBridge

Special Mono dependencies:
$ cp ../libpalaso/lib/Debug/icu.net.dll* ../libpalaso/lib/DebugMono
$ PATH=/usr/bin:$PATH make [debug|release] #This will prefer the System Mono over fieldworks-mono

To run FLEx Bridge you must unzip chorus/lib/debug/Mercurial.zip to the root of FLEx Bridge.  Then, edit the mercurial.ini file
in the Mercurial folder. Add a line like this (with the appropriate path for your FLExBridge folder).
fixutf8 = C:\Dev\FLExBridge\MercurialExtensions\fixutf8\fixutf8.py

***Connecting FieldWorks to FLEx Bridge:***
Add the following keys to your registry (32-bit OS: omit 'Wow6432Node\', Mono: export env var FLEXBRIDGEDIR):
<code>
[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge\9]
"InstallationDir"="C:\Dev\FLExBridge\output\Debug"
</code>
Also, if you are working on Chorus:
- Copy Chorus.exe, LibChorus.dll, and Palaso dll's to C:\fwrepo\fw\output\Debug (or Release).  You can do this using the
   UpdateFLExDependencies.bat script in the flexbridge repo
These steps are required for only those dependencies bound at compile time (e.g. API changes):
- Rebuild FLEx
