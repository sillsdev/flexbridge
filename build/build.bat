REM This script assumes you have already downloaded TeamCity dependencies
setlocal
if not "%VS140COMNTOOLS%" == "" (
	call "%VS140COMNTOOLS%vsvars32.bat"
) else (
	REM TODO (Hasso) 2018.03 (LT-19015): use a more-principaled way to find Visual Studio, like the Locator NuGet package
	call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"
)

pushd .
(
	MSBuild FLExBridge.proj /target:RestorePackages
) && (
	MSBuild FLExBridge.proj /p:Platform="Any CPU" %*
)
popd