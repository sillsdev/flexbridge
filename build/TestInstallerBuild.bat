REM This script assumes you have already downloaded TeamCity dependencies

setlocal

if "%arch%" == "" set arch=x86

for /f "usebackq tokens=1* delims=: " %%i in (`vswhere -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set InstallDir=%%j
)

call "%InstallDir%\VC\Auxiliary\Build\vcvarsall.bat" %arch% 8.1

set Path=%WIX%\bin;%PATH%

pushd .
(
	MSBuild FLExBridge.proj /target:RestorePackages
) && (
	MSBuild FLExBridge.proj /target:Installer /p:Configuration=Debug /p:Platform="Any CPU" %*
)
popd