copy /Y ..\chorus\Download\IPCFramework.dll lib\common\
copy /Y ..\chorus\Download\IPCFramework.dll output\debug\

copy /Y ..\chorus\output\debug\L10NSharp.dll lib\common\
REM Uncomment these two lines if you are working on L10NSharp
REM copy /Y ..\l10nsharp\output\Debug\L10NSharp.dll lib\common\
REM copy /Y ..\l10nsharp\output\Debug\L10NSharp.* lib\debug\

copy /Y ..\chorus\output\debug\LibChorus.dll lib\debug\
copy /Y ..\chorus\output\debug\LibChorus.pdb lib\debug\

copy /Y ..\chorus\output\debug\Chorus.exe lib\debug\
copy /Y ..\chorus\output\debug\Chorus.pdb lib\debug\

copy /Y ..\chorus\output\debug\ChorusMerge.exe lib\debug\
copy /Y ..\chorus\output\debug\ChorusMerge.pdb lib\debug\

copy /Y ..\chorus\output\debug\ChorusHub.exe lib\debug\
copy /Y ..\chorus\output\debug\ChorusHub.pdb lib\debug\

copy /Y ..\chorus\output\debug\Palaso*.dll lib\debug\
copy /Y ..\chorus\output\debug\Palaso*.pdb lib\debug\