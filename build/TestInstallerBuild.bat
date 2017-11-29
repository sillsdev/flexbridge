REM This script assumes you have already downloaded TeamCity dependencies

setlocal
call "%VS140COMNTOOLS%vsvars32.bat"

pushd .
(
	MSBuild FLExBridge.proj /target:RestorePackages
) && (
	MSBuild FLExBridge.proj /target:Installer /p:Configuration=Debug /p:Platform="Any CPU" %*
)
popd