REM If you are working on FLExBridge, there is a good chance you are working on Chorus as
REM it relates to FieldWorks.  To update those dependencies that are bound at build time,
REM first, build Chorus (GetAndBuildThis.bat in either fwbridge or chorus will do this);
REM then, run this script to copy the dependencies.
REM Because the FLEx build script downloads these from build.palaso.org, you will need to
REM comment out the corresponding DownloadFiles tags in C:\fwrepo\fw\Build\mkall.targets.
REM This script assumes that the Chorus and Palaso directories are on the same level as
REM this one, and that the FieldWorks repo is in C:\fwrepo.
REM It copies the needed libraries into the debug folder.

copy /Y ..\chorus\output\debug\LibChorus.dll \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\LibChorus.pdb \fwrepo\fw\Output\Debug\

copy /Y ..\chorus\output\debug\Chorus.exe \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\Chorus.pdb \fwrepo\fw\Output\Debug\

REM Uncomment this line if you are working on L10NSharp:
REM copy /Y ..\l10nsharp\output\debug\L10NSharp.* \fwrepo\fw\Output\Debug\

copy /Y ..\chorus\output\debug\Palaso*.dll \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\Palaso*.pdb \fwrepo\fw\Output\Debug\