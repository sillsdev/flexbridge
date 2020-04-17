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
pushd ..\l10n
(
	MSBuild l10n.proj /t:restore
) && (
	MSBuild l10n.proj /t:CopyL10nsToDistFiles
) && (
	cd ..\build
) && (
	MSBuild FLExBridge.proj /t:RestoreBuildTasks;RestorePackages
) && (
	MSBuild FLExBridge.proj /p:Platform="Any CPU" %*
)
popd