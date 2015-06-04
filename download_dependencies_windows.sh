#!/bin/bash
# server=build.palaso.org
# build_type=FLExBridge_FLExBridgeSILWritingSysteWin32Continuous
# root_dir=.
# Auto-generated by https://github.com/chrisvire/BuildUpdate.
# Do not edit this file by hand!

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl $1 $2
elif [ "$where_wget" != "" ]
then
copy_wget $1 $2
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z $2 -o $2 $1
else
curl -# -L -o $2 $1
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f1=$(basename $1)
f2=$(basename $2)
cd $(dirname $2)
wget -q -L -N $1
# wget has no true equivalent of curl's -o option.
# Different versions of wget handle (or not) % escaping differently.
# A URL query is the only reason why $f1 and $f2 should differ.
if [ "$f1" != "$f2" ]; then mv $f2\?* $f2; fi
cd -
}


# *** Results ***
# build: FLEx Bridge-SILWritingSystems-Win32-Continuous (FLExBridge_FLExBridgeSILWritingSysteWin32Continuous)
# project: FLEx Bridge
# URL: http://build.palaso.org/viewType.html?buildTypeId=FLExBridge_FLExBridgeSILWritingSysteWin32Continuous
# VCS: https://github.com/sillsdev/flexbridge.git [SILWritingSystems]
# dependencies:
# [0] build: Chorus-Documentation (bt216)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt216
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus_Help.chm"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorushelp.git [master]
# [1] build: chorus-win32-SILWritingSystems Continuous (bt453)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt453
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus.exe"=>"lib/Release", "Chorus.pdb"=>"lib/Release", "LibChorus.dll"=>"lib/Release", "LibChorus.pdb"=>"lib/Release", "ChorusMerge.exe"=>"lib/Release", "ChorusMerge.pdb"=>"lib/Release", "MercurialExtensions/**"=>"MercurialExtensions", "LibChorus.TestUtilities.dll"=>"lib/Release", "LibChorus.TestUtilities.pdb"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/chorus.git [SILWritingSystems]
# [2] build: chorus-win32-SILWritingSystems Continuous (bt453)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt453
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus_Help.chm"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorushelp.git [master]
# [1] build: chorus-win32-chorus-2.5-nostrongname Continuous (ChorusWin32v25nostrongCont)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=ChorusWin32v25nostrongCont
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"MercurialExtensions"=>"MercurialExtensions", "Autofac.dll"=>"lib/common", "Chorus.exe"=>"lib/Debug", "LibChorus.dll"=>"lib/Debug", "ChorusMerge.exe"=>"lib/Debug", "Mercurial.zip"=>"lib/Debug", "LibChorus.TestUtilities.dll"=>"lib/Debug", "*.pdb"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/chorus.git [chorus-2.5]
# [4] build: Helpprovider (bt225)
#     project: Helpprovider
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt225
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Vulcan.Uczniowie.HelpProvider.dll"=>"lib/common"}
#     VCS: http://hg.palaso.org/helpprovider []
# [5] build: IPC continuous (bt278)
#     project: IPC Library
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt278
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/Release"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [6] build: IPC continuous (bt278)
#     project: IPC Library
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt278
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/Debug"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [7] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/Release", "L10NSharp.pdb"=>"lib/Release"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp []
# [8] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/Debug", "L10NSharp.pdb"=>"lib/Debug"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp []
# [9] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu*.dll"=>"lib/Release", "icu*.config"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [10] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu*.dll"=>"lib/Debug", "icu*.config"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [9] build: palaso-win32-libpalaso-2.6-nostrongname Continuous (PalasoWin32v26nostrongCont)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=PalasoWin32v26nostrongCont
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Palaso.BuildTasks.dll"=>"lib/Release", "Palaso.dll"=>"lib/Release", "Palaso.pdb"=>"lib/Release", "Palaso.TestUtilities.dll"=>"lib/Release", "Palaso.TestUtilities.pdb"=>"lib/Release", "PalasoUIWindowsForms.dll"=>"lib/Release", "PalasoUIWindowsForms.pdb"=>"lib/Release", "Palaso.Lift.dll"=>"lib/Release", "Palaso.Lift.pdb"=>"lib/Release", "debug/Palaso.BuildTasks.dll"=>"lib/Debug", "debug/Palaso.dll"=>"lib/Debug", "debug/Palaso.pdb"=>"lib/Debug", "debug/Palaso.TestUtilities.dll"=>"lib/Debug", "debug/Palaso.TestUtilities.pdb"=>"lib/Debug", "debug/PalasoUIWindowsForms.dll"=>"lib/Debug", "debug/PalasoUIWindowsForms.pdb"=>"lib/Debug", "debug/Palaso.Lift.dll"=>"lib/Debug", "debug/Palaso.Lift.pdb"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/libpalaso.git [SILWritingSystems]
# [12] build: palaso-win32-SILWritingSystems Continuous (bt440)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt440
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"debug/SIL.Core.dll"=>"lib/Debug", "debug/SIL.Core.pdb"=>"lib/Debug", "debug/SIL.TestUtilities.dll"=>"lib/Debug", "debug/SIL.TestUtilities.pdb"=>"lib/Debug", "debug/SIL.Windows.Forms.dll"=>"lib/Debug", "debug/SIL.Windows.Forms.pdb"=>"lib/Debug", "debug/SIL.Lift.dll"=>"lib/Debug", "debug/SIL.Lift.pdb"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/libpalaso.git [SILWritingSystems]

# make sure output directories exist
mkdir -p ./MercurialExtensions
mkdir -p ./MercurialExtensions/fixutf8
mkdir -p ./lib/Debug
mkdir -p ./lib/Release
mkdir -p ./lib/common

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt216/latest.lastSuccessful/Chorus_Help.chm ./lib/common/Chorus_Help.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/MercurialExtensions ./MercurialExtensions/MercurialExtensions
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/Autofac.dll ./lib/common/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/Chorus.exe ./lib/Debug/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/LibChorus.dll ./lib/Debug/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/ChorusMerge.exe ./lib/Debug/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/Mercurial.zip ./lib/Debug/Mercurial.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/LibChorus.TestUtilities.dll ./lib/Debug/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/Chorus.pdb ./lib/Debug/Chorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/ChorusHub.pdb ./lib/Debug/ChorusHub.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/ChorusHubApp.pdb ./lib/Debug/ChorusHubApp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/ChorusMerge.pdb ./lib/Debug/ChorusMerge.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/LibChorus.TestUtilities.pdb ./lib/Debug/LibChorus.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusWin32v25nostrongCont/latest.lastSuccessful/LibChorus.pdb ./lib/Debug/LibChorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt225/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ./lib/common/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt278/latest.lastSuccessful/IPCFramework.dll ./lib/Release/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt278/latest.lastSuccessful/IPCFramework.dll ./lib/Debug/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ./lib/Release/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb ./lib/Release/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ./lib/Debug/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb ./lib/Debug/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ./lib/Release/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt54.dll ./lib/Release/icudt54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin54.dll ./lib/Release/icuin54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc54.dll ./lib/Release/icuuc54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config ./lib/Release/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ./lib/Debug/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt54.dll ./lib/Debug/icudt54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin54.dll ./lib/Debug/icuin54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc54.dll ./lib/Debug/icuuc54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config ./lib/Debug/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.BuildTasks.dll ./lib/Release/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.dll ./lib/Release/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.pdb ./lib/Release/Palaso.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.TestUtilities.dll ./lib/Release/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.TestUtilities.pdb ./lib/Release/Palaso.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/PalasoUIWindowsForms.dll ./lib/Release/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/PalasoUIWindowsForms.pdb ./lib/Release/PalasoUIWindowsForms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.Lift.dll ./lib/Release/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/Palaso.Lift.pdb ./lib/Release/Palaso.Lift.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.BuildTasks.dll ./lib/Debug/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.dll ./lib/Debug/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.pdb ./lib/Debug/Palaso.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.TestUtilities.dll ./lib/Debug/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.TestUtilities.pdb ./lib/Debug/Palaso.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll ./lib/Debug/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/PalasoUIWindowsForms.pdb ./lib/Debug/PalasoUIWindowsForms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.Lift.dll ./lib/Debug/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/PalasoWin32v26nostrongCont/latest.lastSuccessful/debug/Palaso.Lift.pdb ./lib/Debug/Palaso.Lift.pdb
# End of script
