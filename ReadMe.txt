FlexBridge is an add-on to FieldWorks (http://fieldworks.sil.org; https://github.com/sillsdev/FwDocumentation/wiki)
that supports using Chorus (http://projects.palaso.org/projects/chorus) to allow multiple users to share data.

***Build notes:***
FlexBridge depends on several assemblies from Chorus and Palaso.
Versions of these assemblies are in the repo, but to avoid bloating it, they are usually not kept up to date.  Therefore,
to build it, you must usually get the latest versions of these DLLs and copy them to appropriate places in the FlexBridge project.

The required DLLs (that are likely to be out of date) are Chorus.exe, ChorusMerge.exe, LibChorus.dll, and Palaso.dll.
For a Windows debug build these currently live in lib/Debug (release build: lib/Release, Mono: TODO).  To do this, either:
- clone the Chorus repo from http://hg.palaso.org/chorus, open the solution, build it, and then copy them from
   Chorus\output\debug (or Chorus\output\release).
   There is a script to copy these dependencies in the AddUpdateDependenciesScript branch in the fwbridge repo;
  OR
- download them from the continuous build server, which puts outputs from these and various other projects at http://build.palaso.org/

To run FlexBridge you must unzip chorus/lib/debug/Mercurial.zip to a folder called Mercurial in the root of FlexBridge.
Then, edit the mercurial.ini file in the Mercurial folder. Add a line like this (with the appropriate path for your FlexBridge folder).
fixutf8 = C:\Dev\FLExBridge\MercurialExtensions\fixutf8\fixutf8.py

***Connecting FieldWorks to FlexBridge:***
Add the following keys to your registry (32-bit OS: omit 'Wow6432Node\', Mono: TODO):
<code>
[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge]

[HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\Flex Bridge\8]
"InstallationDir"="C:\\Dev\\FLExBridge\\output\\Debug"
</code>
Also, if you are working on Chorus:
- Copy Chorus.exe, LibChorus.dll, and Palaso dll's to C:\fwrepo\fw\output\Debug (or Release).
   There is a script to copy these dependencies in the AddUpdateDependenciesScript branch in the fwbridge repo
- Comment out the corresponding DownloadFiles tags in C:\fwrepo\fw\Build\mkall.targets
- Rebuild FLEx