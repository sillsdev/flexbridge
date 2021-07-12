if "%FLExBridgeTestPatchVersion%"=="" set FLExBridgeTestPatchVersion=1
set /a FLExBridgeTestPatchVersion=%FLExBridgeTestPatchVersion%+1

setlocal
call "%VS160COMNTOOLS%vsvars32.bat"

pushd .
(
	MSBuild FLExBridge.proj /target:RestorePackages
) && (
	MSBuild FLExBridge.proj /target:Patcher /p:Configuration=Debug /p:Platform="Any CPU" /p:BuildCounter=%FLExBridgeTestPatchVersion% %*
)
popd