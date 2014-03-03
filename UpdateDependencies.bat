REM Since Chorus and Palaso libraries change frequently, you will likely need to get those
REM projects and be able to build them.  Run this script to build and copy those libraries.
REM This script assumes that the Chorus and Palaso directories are on the same level as
REM this one, and that the FieldWorks repo is also at the same level.
REM It copies the needed libraries into the lib folder.

IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

pushd .
cd ..\chorus
call GetAndBuildThis.bat %BUILD_CONFIG% %2
popd

mkdir output\%BUILD_CONFIG%

REM FieldWorks build downloads this but we don't.  Another option is to generate and run
REM buildupdate.sh using the generator at github.com/chrisvire/BuildUpdate
copy /Y \fwrepo\fw\Downloads\IPCFramework.dll lib\%BUILD_CONFIG%\

copy /Y ..\chorus\Download\L10NSharp.dll lib\common\
REM Uncomment these lines if you are working on L10NSharp
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.dll lib\common\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* lib\%BUILD_CONFIG%\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\LibChorus.TestUtilities.dll lib\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\LibChorus.dll lib\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\LibChorus.pdb output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\Chorus.exe lib\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Chorus.pdb output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\ChorusMerge.exe lib\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\ChorusMerge.pdb output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\ChorusHub.exe lib\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\ChorusHub.pdb output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\Palaso*.dll lib\%BUILD_CONFIG%\
copy /Y ..\chorus\output\%BUILD_CONFIG%\Palaso*.pdb output\%BUILD_CONFIG%\

copy /Y ..\chorus\output\%BUILD_CONFIG%\icu.net.dll lib\%BUILD_CONFIG%\
copy /Y ..\chorus\lib\%BUILD_CONFIG%\icu.net.dll.config lib\%BUILD_CONFIG%\
copy /Y ..\chorus\lib\%BUILD_CONFIG%\icudt40.dll lib\%BUILD_CONFIG%\
copy /Y ..\chorus\lib\%BUILD_CONFIG%\icuin40.dll lib\%BUILD_CONFIG%\
copy /Y ..\chorus\lib\%BUILD_CONFIG%\icuuc40.dll lib\%BUILD_CONFIG%\

copy /Y ..\chorus\lib\common\Vulcan.Uczniowie.HelpProvider.dll lib\common

copy /Y ..\chorus\output\%BUILD_CONFIG%\Autofac.dll lib\common