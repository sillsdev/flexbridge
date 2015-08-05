REM If you are working on FLExBridge, there is a good chance you are working on Chorus as
REM it relates to FieldWorks.  To update these dependencies, first, build FLEx and Chorus
REM (GetAndBuildThis.bat in either flexbridge or chorus will build Chorus); then, run
REM this script to copy the dependencies.
REM This script assumes that the Chorus and Palaso directories are on the same level as
REM this one, and that the FieldWorks repo is in C:\fwrepo.
REM It copies the needed libraries into the debug folder.

copy /Y ..\chorus\output\debug\Autofac.dll \fwrepo\fw\Downloads\

copy /Y ..\chorus\output\debug\LibChorus.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\debug\LibChorus.dll \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\LibChorus.pdb \fwrepo\fw\Output\Debug\

copy /Y ..\chorus\output\debug\Chorus*.exe \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\debug\Chorus*.exe \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\Chorus*.pdb \fwrepo\fw\Output\Debug\

REM Uncomment this line if you are working on L10NSharp:
REM copy /Y ..\l10nsharp\output\debug\L10NSharp.* \fwrepo\fw\Downloads\
REM copy /Y ..\l10nsharp\output\debug\L10NSharp.* \fwrepo\fw\Output\Debug\

copy /Y ..\chorus\output\debug\Palaso*.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\debug\Palaso*.dll \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\Palaso*.pdb \fwrepo\fw\Output\Debug\

copy /Y ..\chorus\output\debug\SIL.*.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\debug\SIL.*.dll \fwrepo\fw\Output\Debug\
copy /Y ..\chorus\output\debug\SIL.*.pdb \fwrepo\fw\Output\Debug\