call "c:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"

pushd .
MSbuild FLExBridge.build.win.proj /target:Installer /p:Configuration=Debug /p:Platform="Any CPU" /p:teamcity_build_checkoutDir=..\  /p:teamcity_dotnet_nunitlauncher_msbuild_task="notthere" /p:BUILD_NUMBER="0.3.1" /p:Minor="1" /p:BUILD_VCS_NUMBER=0
popd
PAUSE