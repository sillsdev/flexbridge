# Linux/Mono Makefile for FlexBridge.

BUILD_NUMBER=0.3.1
MINOR=1

release:
	cd build && xbuild FLExBridge.build.mono.proj /t:Build /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:Minor=$(MINOR) /p:Configuration=ReleaseMono /v:detailed

debug:
	cd build && xbuild FLExBridge.build.mono.proj /t:Build /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:Minor=$(MINOR) /p:Configuration=DebugMono /v:detailed
