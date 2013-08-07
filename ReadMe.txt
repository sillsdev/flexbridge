FlexBridge is an add-on to FieldWorks (todo: url) that supports using Chorus (todo: url) to allow multiple users to share data.

Build notes:
FlexBridge depends on several assemblies from Chorus and Palaso.
Versions of these assemblies are in the repo, but to avoid bloating it, they are usually not kept up to date.
Therefore, to build it, you must usually get the latest versions of these DLLs and copy them to appropriate places in the FlexBridge project.

The required DLLs (that are likely to be out of date) are Chorus.exe, ChorusMerge.exe, LibChorus.dll, and Palaso.dll.
For a Windows debug build these currently live in lib/Debug (release build: lib/Release, todo: Mono).

One way to obtain the latest versions of these is to clone the Chorus repo from http://hg.palaso.org/chorus, open the solution,
build it, and then copy them from Chorus\output\debug (or Chorus\output\release).

Another option is to obtain them from the continuous build server, which puts outputs from these and various other projects at (todo: url).

Connecting FieldWorks to FlexBridge:
To setup the registry open regedit and create HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SIL\FLEx Bridge\8\{InstallationDir = path to fwbridge output debug directory}.

To run FlexBridge you must unzip lib/debug/Mercurial.zip to the root of FlexBridge.

Then, edit the mercurial.ini file in the Mercurial folder. Add a line like this (with the appropriate path for your FlexBridge folder).
fixutf8 = C:\DEV\FlexBridge\MercurialExtensions\fixutf8\fixutf8.py