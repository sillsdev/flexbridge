REM This script assumes you have already downloaded TeamCity dependencies
setlocal
if not "%VS140COMNTOOLS%"=="" (
	call "%VS140COMNTOOLS%vsvars32.bat"
	GOTO Build
)

for /f "usebackq delims=" %%i in (`vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath`) do (
	set InstallDir=%%i
)
call "%InstallDir%\VC\Auxiliary\Build\vcvars32.bat"

:Build
REM pushd ..\l10n
REM (
	REM MSBuild l10n.proj /t:restore
REM ) && (
	REM MSBuild l10n.proj /t:CopyL10nsToDistFiles
REM ) && (
	REM cd ..\build
REM ) && (
	REM MSBuild FLExBridge.proj /t:RestoreBuildTasks;RestorePackages
REM ) && (
	MSBuild FLExBridge.proj /p:Platform="Any CPU" %*
REM )
REM popd