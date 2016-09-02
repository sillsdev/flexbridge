REM If you are working on FLExBridge, there is a good chance you are working on Chorus as
REM it relates to FieldWorks.  To update these dependencies, first, build FLEx and Chorus
REM (GetAndBuildThis.bat in either flexbridge or chorus will build Chorus); then, run
REM this script to copy the dependencies.
REM This script assumes that the Chorus and Palaso directories are on the same level as
REM this one, and that the FieldWorks repo is inside C:\fwrepo (if the current drive is C:).
REM It copies the needed libraries into the debug folder.

IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

mkdir \fwrepo\fw\Downloads
mkdir \fwrepo\fw\Output\%BUILD_CONFIG%

copy /Y ..\chorus\output\%BUILD_CONFIG%\Autofac.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Autofac.dll \fwrepo\fw\Output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\LibChorus.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\%BUILD_CONFIG%\LibChorus.dll \fwrepo\fw\Output\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\LibChorus.pdb \fwrepo\fw\Output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\Chorus*.exe \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Chorus*.exe \fwrepo\fw\Output\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Chorus*.pdb \fwrepo\fw\Output\%BUILD_CONFIG%\

REM Uncomment this line if you are working on L10NSharp:
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* \fwrepo\fw\Downloads\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* \fwrepo\fw\Output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\Palaso*.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Palaso*.dll \fwrepo\fw\Output\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Palaso*.pdb \fwrepo\fw\Output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\SIL.*.dll \fwrepo\fw\Downloads\
copy /Y ..\chorus\output\%BUILD_CONFIG%\SIL.*.dll \fwrepo\fw\Output\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\SIL.*.pdb \fwrepo\fw\Output\%BUILD_CONFIG%\