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

REM Uncomment these lines if you are working on L10NSharp
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* lib\%BUILD_CONFIG%\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* output\%BUILD_CONFIG%\

set CHORUS_DIR="..\chorus"

IF NOT EXIST %CHORUS_DIR% GOTO :EOF

pushd %CHORUS_DIR%
REM Presence of a second argument indicates that the caller has already run vsvars32.bat
call GetAndBuildThis.bat %BUILD_CONFIG% %2
popd

mkdir output\%BUILD_CONFIG%

REM FieldWorks build downloads this but we don't.  Another option is to generate and run
REM buildupdate.sh using the generator at github.com/chrisvire/BuildUpdate
copy /Y \fwrepo\fw\Downloads\IPCFramework.dll lib\%BUILD_CONFIG%\

copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\Autofac.dll lib\%BUILD_CONFIG%\
copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\Autofac.dll lib\common

copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\LibChorus.TestUtilities.dll lib\%BUILD_CONFIG%\
copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\LibChorus.dll lib\%BUILD_CONFIG%\
copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\LibChorus.pdb output\%BUILD_CONFIG%\

copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\Chorus.exe lib\%BUILD_CONFIG%\
copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\Chorus.pdb output\%BUILD_CONFIG%\

copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\ChorusMerge.exe lib\%BUILD_CONFIG%\
copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\ChorusMerge.pdb output\%BUILD_CONFIG%\

copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\ChorusHub.exe lib\%BUILD_CONFIG%\
copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\ChorusHub.pdb output\%BUILD_CONFIG%\

REM copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\SIL.*.dll lib\%BUILD_CONFIG%\
REM copy /Y %CHORUS_DIR%\output\%BUILD_CONFIG%\SIL.*.pdb output\%BUILD_CONFIG%\
