setlocal
call UpdateDependencies.bat

call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
hg pull -u --rebase
msbuild "FLExBridge VS2010.sln" /verbosity:quiet /maxcpucount
