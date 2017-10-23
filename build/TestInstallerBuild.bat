REM This script assumes you have already downloaded TeamCity dependencies

setlocal
call "c:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"

pushd .
(
	MSBuild FLExBridge.proj /target:RestorePackages
) && (
	MSBuild FLExBridge.proj /target:Installer /p:Configuration=Debug /p:Platform="Any CPU" /p:BUILD_NUMBER="0.3.1" /p:Minor="1" /p:BUILD_VCS_NUMBER=0
)
popd