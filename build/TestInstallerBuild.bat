REM This script assumes you have already downloaded TeamCity dependencies
setlocal
if not "%VS140COMNTOOLS%"=="" (
	call "%VS140COMNTOOLS%vsvars32.bat"
	GOTO Build
)

for /f "delims=" %%i in ('vswhere -nologo -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath') do (
	set InstallDir=%%i
)
call "%InstallDir%\VC\Auxiliary\Build\vcvars32.bat"

:Build

set Path=%WIX%\bin;%PATH%
echo Starting Build
pushd .
(
	MSBuild FLExBridge.proj /t:RestoreBuildTasks;RestorePackages
) && (
	pushd ..\l10n
) && (
	MSBuild l10n.proj /t:GetLatestL10ns
) && (
	popd
) && (
	MSBuild FLExBridge.proj /target:Installer /p:Configuration=Debug /p:Platform="Any CPU" %*
)
popd
