FlexBridge is an add-on to FieldWorks (http://fieldworks.sil.org; https://github.com/sillsdev/FwDocumentation/wiki)
that supports using Chorus (http://projects.palaso.org/projects/chorus) to allow multiple users to share data.

***Build notes:***
FlexBridge depends on several assemblies from Chorus and Palaso.
Versions of these assemblies are in the repo, but to avoid bloating it, they are usually not kept up to date.  Therefore,
to build it, you must usually get the latest versions of these DLLs and copy them to appropriate places in the FlexBridge project.

The required DLLs (that are likely to be out of date) are
 IPCFramework.dll, L10NSharp.dll, Chorus.exe, ChorusMerge.exe, LibChorus.dll, and Palaso.dll.
For a Windows debug build the Chorus and Palaso dependencies currently live in lib/Debug (release build: lib/Release, Mono: TODO);
IPCFrmework and L10NSharp live in lib/common.  You can download these and various other projects from the continuous build server
at http://build.palaso.org/.

If you plan to work on Chorus,
- clone the Chorus and Palaso repos from http://hg.palaso.org/chorus and http://hg.palaso.org/palaso into the same parent
   directory without changing their repository names
- run chorus/GetAndBuildThis.bat to download the latest, build Palaso, copy dependencies from Palaso to Chorus, and build Chorus
- copy Chorus and Palaso dependencies from Chorus\output\Debug (or Chorus\output\Release).  You can do this using the
   UpdateDependencies.bat script in the AddUpdateDependenciesScript branch in the fwbridge repo

To run FlexBridge you must unzip chorus/lib/debug/Mercurial.zip to the root of FlexBridge.
Then, edit the mercurial.ini file in the Mercurial folder. Add a line like this (with the appropriate path for your FlexBridge folder).
fixutf8 = C:\Dev\FLExBridge\MercurialExtensions\fixutf8\fixutf8.py

***Connecting FieldWorks to FlexBridge:***
Add the following keys to your registry (32-bit OS: omit 'Wow6432Node\', Mono: TODO):
<code>
[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge\8]
"InstallationDir"="C:\\Dev\\FLExBridge\\output\\Debug"
</code>
Also, if you are working on Chorus:
- Copy Chorus.exe, LibChorus.dll, and Palaso dll's to C:\fwrepo\fw\output\Debug (or Release).  You can do this using the
   UpdateFLExDependencies script in the AddUpdateDependenciesScript branch in the fwbridge repo
- Comment out the corresponding DownloadFiles tags in C:\fwrepo\fw\Build\mkall.targets
- Rebuild FLEx