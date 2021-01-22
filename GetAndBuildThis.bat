setlocal

IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

call UpdateDependencies.bat %BUILD_CONFIG%

pushd build
build /t:build
popd

@rem msbuild "FLExBridge VS2010.sln" /verbosity:quiet /maxcpucount /p:Configuration=%BUILD_CONFIG%
