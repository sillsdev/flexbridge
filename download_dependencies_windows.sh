#!/bin/bash
# server=build.palaso.org
# build_type=FLExBridge_FLExBridgeDevelopWin32Continuous
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
copy_curl "$1" "$2"
elif [ "$where_wget" != "" ]
then
copy_wget "$1" "$2"
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
curl -# -L -z "$2" -o "$2" "$1"
else
curl -# -L -o "$2" "$1"
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f1=$(basename $1)
f2=$(basename $2)
cd $(dirname $2)
wget -q -L -N "$1"
# wget has no true equivalent of curl's -o option.
# Different versions of wget handle (or not) % escaping differently.
# A URL query is the only reason why $f1 and $f2 should differ.
if [ "$f1" != "$f2" ]; then mv $f2\?* $f2; fi
cd -
}


# *** Results ***
# build: FLEx Bridge-develop-Win32-Continuous (FLExBridge_FLExBridgeDevelopWin32Continuous)
# project: FLEx Bridge
# URL: https://build.palaso.org/viewType.html?buildTypeId=FLExBridge_FLExBridgeDevelopWin32Continuous
# VCS: git@github.com:sillsdev/flexbridge.git [develop]
# dependencies:
# [0] build: Chorus-Documentation (bt216)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt216
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus_Help.chm"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorushelp.git [master]
# [1] build: chorus-win32-master Continuous (Chorus_Win32masterContinuous)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Chorus_Win32masterContinuous
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"MercurialExtensions"=>"MercurialExtensions", "Autofac.dll"=>"lib/common", "Chorus.exe"=>"lib/Release", "LibChorus.dll"=>"lib/Release", "ChorusMerge.exe"=>"lib/Release", "Mercurial.zip"=>"lib/Release", "LibChorus.TestUtilities.dll"=>"lib/Release", "*.pdb"=>"lib/Release", "ChorusMergeModule.msm"=>"lib/Release", "Microsoft_VC90_CRT_x86.msm"=>"lib/Release", "policy_9_0_Microsoft_VC90_CRT_x86.msm"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [2] build: chorus-win32-master Continuous (Chorus_Win32masterContinuous)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Chorus_Win32masterContinuous
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus.exe"=>"lib/Debug", "LibChorus.dll"=>"lib/Debug", "ChorusMerge.exe"=>"lib/Debug", "Mercurial.zip"=>"lib/Debug", "LibChorus.TestUtilities.dll"=>"lib/Debug", "*.pdb"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [3] build: IPC continuous (bt278)
#     project: IPC Library
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt278
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/Release"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [4] build: IPC continuous (bt278)
#     project: IPC Library
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt278
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/Debug"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [5] build: palaso-win32-master Continuous (Libpalaso_PalasoWin32masterContinuous)
#     project: libpalaso
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Libpalaso_PalasoWin32masterContinuous
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"SIL.Core.Desktop.dll"=>"lib/Release", "SIL.Core.Desktop.pdb"=>"lib/Release", "SIL.Core.dll"=>"lib/Release", "SIL.Core.pdb"=>"lib/Release", "SIL.Lift.dll"=>"lib/Release", "SIL.Lift.pdb"=>"lib/Release", "SIL.TestUtilities.dll"=>"lib/Release", "SIL.TestUtilities.pdb"=>"lib/Release", "SIL.Windows.Forms.dll"=>"lib/Release", "SIL.Windows.Forms.pdb"=>"lib/Release", "SIL.WritingSystems.dll"=>"lib/Release", "SIL.WritingSystems.pdb"=>"lib/Release", "L10NSharp.dll"=>"lib/Release", "L10NSharp.pdb"=>"lib/Release", "debug/SIL.Core.Desktop.dll"=>"lib/Debug", "debug/SIL.Core.Desktop.pdb"=>"lib/Debug", "debug/SIL.Core.dll"=>"lib/Debug", "debug/SIL.Core.pdb"=>"lib/Debug", "debug/SIL.Lift.dll"=>"lib/Debug", "debug/SIL.Lift.pdb"=>"lib/Debug", "debug/SIL.TestUtilities.dll"=>"lib/Debug", "debug/SIL.TestUtilities.pdb"=>"lib/Debug", "debug/SIL.Windows.Forms.dll"=>"lib/Debug", "debug/SIL.Windows.Forms.pdb"=>"lib/Debug", "debug/SIL.WritingSystems.dll"=>"lib/Debug", "debug/SIL.WritingSystems.pdb"=>"lib/Debug", "debug/L10NSharp.dll"=>"lib/Debug", "debug/L10NSharp.pdb"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/libpalaso.git [refs/heads/master]

# make sure output directories exist
mkdir -p ./MercurialExtensions
mkdir -p ./lib/Debug
mkdir -p ./lib/Release
mkdir -p ./lib/common

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt216/latest.lastSuccessful/Chorus_Help.chm ./lib/common/Chorus_Help.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/MercurialExtensions ./MercurialExtensions/MercurialExtensions
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Autofac.dll ./lib/common/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Chorus.exe ./lib/Release/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.dll ./lib/Release/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusMerge.exe ./lib/Release/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Mercurial.zip ./lib/Release/Mercurial.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.TestUtilities.dll ./lib/Release/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Chorus.pdb ./lib/Release/Chorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusHub.pdb ./lib/Release/ChorusHub.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusHubApp.pdb ./lib/Release/ChorusHubApp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusMerge.pdb ./lib/Release/ChorusMerge.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.TestUtilities.pdb ./lib/Release/LibChorus.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.pdb ./lib/Release/LibChorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/SIL.Core.Desktop.pdb ./lib/Release/SIL.Core.Desktop.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/Chorus.pdb ./lib/Release/Chorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/ChorusHub.pdb ./lib/Release/ChorusHub.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/ChorusMerge.pdb ./lib/Release/ChorusMerge.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/LibChorus.TestUtilities.pdb ./lib/Release/LibChorus.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/LibChorus.pdb ./lib/Release/LibChorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusMergeModule.msm ./lib/Release/ChorusMergeModule.msm
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Microsoft_VC90_CRT_x86.msm ./lib/Release/Microsoft_VC90_CRT_x86.msm
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/policy_9_0_Microsoft_VC90_CRT_x86.msm ./lib/Release/policy_9_0_Microsoft_VC90_CRT_x86.msm
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Chorus.exe ./lib/Debug/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.dll ./lib/Debug/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusMerge.exe ./lib/Debug/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Mercurial.zip ./lib/Debug/Mercurial.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.TestUtilities.dll ./lib/Debug/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/Chorus.pdb ./lib/Debug/Chorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusHub.pdb ./lib/Debug/ChorusHub.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusHubApp.pdb ./lib/Debug/ChorusHubApp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/ChorusMerge.pdb ./lib/Debug/ChorusMerge.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.TestUtilities.pdb ./lib/Debug/LibChorus.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/LibChorus.pdb ./lib/Debug/LibChorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/SIL.Core.Desktop.pdb ./lib/Debug/SIL.Core.Desktop.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/Chorus.pdb ./lib/Debug/Chorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/ChorusHub.pdb ./lib/Debug/ChorusHub.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/ChorusMerge.pdb ./lib/Debug/ChorusMerge.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/LibChorus.TestUtilities.pdb ./lib/Debug/LibChorus.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Win32masterContinuous/latest.lastSuccessful/debug/LibChorus.pdb ./lib/Debug/LibChorus.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt278/latest.lastSuccessful/IPCFramework.dll ./lib/Release/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt278/latest.lastSuccessful/IPCFramework.dll ./lib/Debug/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Core.Desktop.dll ./lib/Release/SIL.Core.Desktop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Core.Desktop.pdb ./lib/Release/SIL.Core.Desktop.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Core.dll ./lib/Release/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Core.pdb ./lib/Release/SIL.Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Lift.dll ./lib/Release/SIL.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Lift.pdb ./lib/Release/SIL.Lift.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.TestUtilities.dll ./lib/Release/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.TestUtilities.pdb ./lib/Release/SIL.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Windows.Forms.dll ./lib/Release/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.Windows.Forms.pdb ./lib/Release/SIL.Windows.Forms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.WritingSystems.dll ./lib/Release/SIL.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/SIL.WritingSystems.pdb ./lib/Release/SIL.WritingSystems.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/L10NSharp.dll ./lib/Release/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/L10NSharp.pdb ./lib/Release/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Core.Desktop.dll ./lib/Debug/SIL.Core.Desktop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Core.Desktop.pdb ./lib/Debug/SIL.Core.Desktop.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Core.dll ./lib/Debug/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Core.pdb ./lib/Debug/SIL.Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Lift.dll ./lib/Debug/SIL.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Lift.pdb ./lib/Debug/SIL.Lift.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.TestUtilities.dll ./lib/Debug/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.TestUtilities.pdb ./lib/Debug/SIL.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Windows.Forms.dll ./lib/Debug/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.Windows.Forms.pdb ./lib/Debug/SIL.Windows.Forms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.WritingSystems.dll ./lib/Debug/SIL.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/SIL.WritingSystems.pdb ./lib/Debug/SIL.WritingSystems.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/L10NSharp.dll ./lib/Debug/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoWin32masterContinuous/latest.lastSuccessful/debug/L10NSharp.pdb ./lib/Debug/L10NSharp.pdb
# End of script
