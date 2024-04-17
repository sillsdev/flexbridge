setlocal
if not "%VS160COMNTOOLS%"=="" (
	call "%VS160COMNTOOLS%vsvars32.bat"
	GOTO Build
)

for /f "delims=" %%i in ('vswhere -nologo -version "[16.0,)" -requires Microsoft.Component.MSBuild -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath') do (
	set InstallDir=%%i
)
call "%InstallDir%\VC\Auxiliary\Build\vcvars32.bat"

:Build

set Path=%WIX%\bin;%PATH%
echo Starting Build
pushd .
(
REM	MSBuild FLExBridge.proj /t:RestoreBuildTasks;RestorePackages
) && (
REM	pushd ..\l10n
) && (
REM	MSBuild l10n.proj /t:restore
) && (
REM	MSBuild l10n.proj /t:GetLatestL10ns
) && (
REM	popd
) && (
  MSBuild FLExBridge.proj /target:CleanMasterOutputDir;BuildProductBaseMsi /p:Configuration=Debug /p:Platform="Any CPU" %*
)
popd
REM -2147024893/0x80070003