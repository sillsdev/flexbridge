REM Since Chorus and Palaso libraries change frequently, you will likely need to get those
REM projects and be able to build them.  Run this script to build and copy those libraries.
REM This script assumes that the Chorus and Palaso directories are on the same level as
REM this one, and that the FieldWorks repo is in C:\fwrepo.
REM It copies the needed libraries into the lib folder.

pushd ..\chorus
echo y | call GetAndBuildThis.bat
popd

REM FieldWorks build downloads this but we don't (TODO pH 2013.09: fix our build script)
copy /Y \fwrepo\fw\Downloads\IPCFramework.dll lib\Debug\

copy /Y ..\chorus\Download\L10NSharp.dll lib\common\
REM Uncomment these two lines if you are working on L10NSharp
REM copy /Y ..\l10nsharp\output\Debug\L10NSharp.dll lib\common\
REM copy /Y ..\l10nsharp\output\Debug\L10NSharp.* lib\Debug\

copy /Y ..\chorus\output\Debug\LibChorus.TestUtilities.dll lib\Debug\
copy /Y ..\chorus\output\Debug\LibChorus.dll lib\Debug\
copy /Y ..\chorus\output\Debug\LibChorus.pdb lib\Debug\

copy /Y ..\chorus\output\Debug\Chorus.exe lib\Debug\
copy /Y ..\chorus\output\Debug\Chorus.pdb lib\Debug\

copy /Y ..\chorus\output\Debug\ChorusMerge.exe lib\Debug\
copy /Y ..\chorus\output\Debug\ChorusMerge.pdb lib\Debug\

copy /Y ..\chorus\output\Debug\ChorusHub.exe lib\Debug\
copy /Y ..\chorus\output\Debug\ChorusHub.pdb lib\Debug\

copy /Y ..\chorus\output\Debug\Palaso*.dll lib\Debug\
copy /Y ..\chorus\output\Debug\Palaso*.pdb lib\Debug\