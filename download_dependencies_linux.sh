#!/bin/bash
# server=build.palaso.org
# build_type=FLExBridgeDevelopLinux64Continuous
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
# build: FLEx Bridge-develop-Linux64-Continuous (FLExBridgeDevelopLinux64Continuous)
# project: FLEx Bridge
# URL: https://build.palaso.org/viewType.html?buildTypeId=FLExBridgeDevelopLinux64Continuous
# VCS: git@github.com:sillsdev/flexbridge.git [develop]
# dependencies:
# [0] build: Chorus-Documentation (bt216)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt216
#     clean: false
#     revision: fb-for-fw-9.0.15.tcbuildtag
#     branch: chorus-3.0
#     paths: {"Chorus_Help.chm"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorushelp.git [master]
# [1] build: chorus-linux64-master Continuous (Chorus_Linux64masterContinuous)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Chorus_Linux64masterContinuous
#     clean: false
#     revision: fb-for-fw-9.0.15.tcbuildtag
#     branch: chorus-3.0
#     paths: {"debug/Chorus.exe"=>"lib/DebugMono", "debug/LibChorus.dll"=>"lib/DebugMono", "debug/ChorusHub.exe"=>"lib/DebugMono", "debug/ChorusMerge.exe"=>"lib/DebugMono", "Mercurial-i686.zip"=>"lib/DebugMono", "Mercurial-x86_64.zip"=>"lib/DebugMono", "debug/LibChorus.TestUtilities.dll"=>"lib/DebugMono", "MercurialExtensions/**"=>"MercurialExtensions"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [2] build: chorus-linux64-master Continuous (Chorus_Linux64masterContinuous)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Chorus_Linux64masterContinuous
#     clean: false
#     revision: fb-for-fw-9.0.15.tcbuildtag
#     branch: chorus-3.0
#     paths: {"Chorus.exe"=>"lib/ReleaseMono", "LibChorus.dll"=>"lib/ReleaseMono", "ChorusHub.exe"=>"lib/ReleaseMono", "ChorusMerge.exe"=>"lib/ReleaseMono", "Mercurial-i686.zip"=>"lib/ReleaseMono", "Mercurial-x86_64.zip"=>"lib/ReleaseMono", "LibChorus.TestUtilities.dll"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [3] build: chorus-linux64-master Continuous (Chorus_Linux64masterContinuous)
#     project: Chorus
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Chorus_Linux64masterContinuous
#     clean: false
#     revision: fb-for-fw-9.0.15.tcbuildtag
#     branch: chorus-3.0
#     paths: {"Autofac.dll"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [4] build: IPC-Precise64 (bt279)
#     project: IPC Library
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt279
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/ReleaseMono"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [5] build: IPC-Precise64 (bt279)
#     project: IPC Library
#     URL: https://build.palaso.org/viewType.html?buildTypeId=bt279
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/DebugMono"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [6] build: palaso-linux64-master Continuous (Libpalaso_PalasoLinux64masterContinuous)
#     project: libpalaso
#     URL: https://build.palaso.org/viewType.html?buildTypeId=Libpalaso_PalasoLinux64masterContinuous
#     clean: false
#     revision: fw-9.1.1.tcbuildtag
#     paths: {"SIL.Core.dll*"=>"lib/ReleaseMono", "SIL.Core.Desktop.dll*"=>"lib/ReleaseMono", "SIL.TestUtilities.dll*"=>"lib/ReleaseMono", "SIL.Windows.Forms.dll*"=>"lib/ReleaseMono", "SIL.Windows.Forms.GeckoBrowserAdapter.dll"=>"lib/ReleaseMono", "SIL.Lift.dll"=>"lib/ReleaseMono", "L10NSharp.dll"=>"lib/ReleaseMono", "NDesk.*"=>"lib/ReleaseMono", "debug/SIL.Core.dll*"=>"lib/DebugMono", "debug/SIL.Core.Desktop.dll*"=>"lib/DebugMono", "debug/SIL.TestUtilities.dll*"=>"lib/DebugMono", "debug/SIL.Windows.Forms.dll*"=>"lib/DebugMono", "debug/SIL.Windows.Forms.GeckoBrowserAdapter.dll*"=>"lib/DebugMono", "debug/SIL.Lift.dll*"=>"lib/DebugMono", "debug/L10NSharp.dll"=>"lib/DebugMono", "debug/NDesk.*"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/libpalaso.git [refs/heads/master]

# make sure output directories exist
mkdir -p ./MercurialExtensions
mkdir -p ./MercurialExtensions/fixutf8
mkdir -p ./lib/DebugMono
mkdir -p ./lib/ReleaseMono
mkdir -p ./lib/common

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt216/latest.lastSuccessful/Chorus_Help.chm ./lib/common/Chorus_Help.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/debug/Chorus.exe?branch=chorus-3.0 ./lib/DebugMono/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/debug/LibChorus.dll?branch=chorus-3.0 ./lib/DebugMono/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/debug/ChorusHub.exe?branch=chorus-3.0 ./lib/DebugMono/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/debug/ChorusMerge.exe?branch=chorus-3.0 ./lib/DebugMono/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/Mercurial-i686.zip?branch=chorus-3.0 ./lib/DebugMono/Mercurial-i686.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/Mercurial-x86_64.zip?branch=chorus-3.0 ./lib/DebugMono/Mercurial-x86_64.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/debug/LibChorus.TestUtilities.dll?branch=chorus-3.0 ./lib/DebugMono/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/.guidsForInstaller.xml?branch=chorus-3.0 ./MercurialExtensions/.guidsForInstaller.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/Dummy.txt?branch=chorus-3.0 ./MercurialExtensions/Dummy.txt
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/.gitignore?branch=chorus-3.0 ./MercurialExtensions/fixutf8/.gitignore
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/.guidsForInstaller.xml?branch=chorus-3.0 ./MercurialExtensions/fixutf8/.guidsForInstaller.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/.hg_archival.txt?branch=chorus-3.0 ./MercurialExtensions/fixutf8/.hg_archival.txt
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/.hgignore?branch=chorus-3.0 ./MercurialExtensions/fixutf8/.hgignore
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/README.?branch=chorus-3.0 ./MercurialExtensions/fixutf8/README.
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/buildcpmap.py?branch=chorus-3.0 ./MercurialExtensions/fixutf8/buildcpmap.py
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/cpmap.pyc?branch=chorus-3.0 ./MercurialExtensions/fixutf8/cpmap.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/fixutf8.py?branch=chorus-3.0 ./MercurialExtensions/fixutf8/fixutf8.py
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/fixutf8.pyc?branch=chorus-3.0 ./MercurialExtensions/fixutf8/fixutf8.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/fixutf8.pyo?branch=chorus-3.0 ./MercurialExtensions/fixutf8/fixutf8.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/osutil.py?branch=chorus-3.0 ./MercurialExtensions/fixutf8/osutil.py
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/osutil.pyc?branch=chorus-3.0 ./MercurialExtensions/fixutf8/osutil.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/osutil.pyo?branch=chorus-3.0 ./MercurialExtensions/fixutf8/osutil.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/win32helper.py?branch=chorus-3.0 ./MercurialExtensions/fixutf8/win32helper.py
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/win32helper.pyc?branch=chorus-3.0 ./MercurialExtensions/fixutf8/win32helper.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/MercurialExtensions/fixutf8/win32helper.pyo?branch=chorus-3.0 ./MercurialExtensions/fixutf8/win32helper.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/Chorus.exe?branch=chorus-3.0 ./lib/ReleaseMono/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/LibChorus.dll?branch=chorus-3.0 ./lib/ReleaseMono/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/ChorusHub.exe?branch=chorus-3.0 ./lib/ReleaseMono/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/ChorusMerge.exe?branch=chorus-3.0 ./lib/ReleaseMono/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/Mercurial-i686.zip?branch=chorus-3.0 ./lib/ReleaseMono/Mercurial-i686.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/Mercurial-x86_64.zip?branch=chorus-3.0 ./lib/ReleaseMono/Mercurial-x86_64.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/LibChorus.TestUtilities.dll?branch=chorus-3.0 ./lib/ReleaseMono/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Chorus_Linux64masterContinuous/fb-for-fw-9.0.15.tcbuildtag/Autofac.dll?branch=chorus-3.0 ./lib/common/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt279/latest.lastSuccessful/IPCFramework.dll ./lib/ReleaseMono/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt279/latest.lastSuccessful/IPCFramework.dll ./lib/DebugMono/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.Core.dll ./lib/ReleaseMono/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.Core.Desktop.dll ./lib/ReleaseMono/SIL.Core.Desktop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.TestUtilities.dll ./lib/ReleaseMono/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.Windows.Forms.dll ./lib/ReleaseMono/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.Windows.Forms.dll.config ./lib/ReleaseMono/SIL.Windows.Forms.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.dll ./lib/ReleaseMono/SIL.Windows.Forms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/SIL.Lift.dll ./lib/ReleaseMono/SIL.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/L10NSharp.dll ./lib/ReleaseMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/NDesk.DBus.dll ./lib/ReleaseMono/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/NDesk.DBus.dll.config ./lib/ReleaseMono/NDesk.DBus.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.Core.dll ./lib/DebugMono/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.Core.Desktop.dll ./lib/DebugMono/SIL.Core.Desktop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.TestUtilities.dll ./lib/DebugMono/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.Windows.Forms.dll ./lib/DebugMono/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.Windows.Forms.dll.config ./lib/DebugMono/SIL.Windows.Forms.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.Windows.Forms.GeckoBrowserAdapter.dll ./lib/DebugMono/SIL.Windows.Forms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/SIL.Lift.dll ./lib/DebugMono/SIL.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/L10NSharp.dll ./lib/DebugMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/NDesk.DBus.dll ./lib/DebugMono/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoLinux64masterContinuous/fw-9.1.1.tcbuildtag/debug/NDesk.DBus.dll.config ./lib/DebugMono/NDesk.DBus.dll.config
# End of script
