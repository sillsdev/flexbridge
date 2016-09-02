#!/bin/bash
# server=build.palaso.org
# build_type=FLExBridgeLfmergePrecise64Continuous
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
# build: FLEx Bridge-lfmerge-Precise64-Continuous (FLExBridgeLfmergePrecise64Continuous)
# project: FLEx Bridge
# URL: http://build.palaso.org/viewType.html?buildTypeId=FLExBridgeLfmergePrecise64Continuous
# VCS: https://github.com/sillsdev/flexbridge.git [lfmerge]
# dependencies:
# [0] build: Chorus-Documentation (bt216)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt216
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus_Help.chm"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorushelp.git [master]
# [1] build: chorus-precise64-lfmerge Continuous (ChorusPrecise64lfmergeCont)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=ChorusPrecise64lfmergeCont
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"debug/Chorus.exe"=>"lib/DebugMono", "debug/LibChorus.dll"=>"lib/DebugMono", "debug/ChorusHub.exe"=>"lib/DebugMono", "debug/ChorusMerge.exe"=>"lib/DebugMono", "Mercurial-i686.zip"=>"lib/DebugMono", "Mercurial-x86_64.zip"=>"lib/DebugMono", "debug/LibChorus.TestUtilities.dll"=>"lib/DebugMono", "MercurialExtensions/**"=>"MercurialExtensions"}
#     VCS: https://github.com/sillsdev/chorus.git [lfmerge]
# [2] build: chorus-precise64-lfmerge Continuous (ChorusPrecise64lfmergeCont)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=ChorusPrecise64lfmergeCont
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Chorus.exe"=>"lib/ReleaseMono", "LibChorus.dll"=>"lib/ReleaseMono", "ChorusHub.exe"=>"lib/ReleaseMono", "ChorusMerge.exe"=>"lib/ReleaseMono", "Mercurial-i686.zip"=>"lib/ReleaseMono", "Mercurial-x86_64.zip"=>"lib/ReleaseMono", "LibChorus.TestUtilities.dll"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/chorus.git [lfmerge]
# [3] build: chorus-precise64-lfmerge Continuous (ChorusPrecise64lfmergeCont)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=ChorusPrecise64lfmergeCont
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Autofac.dll"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorus.git [lfmerge]
# [4] build: Helpprovider (bt225)
#     project: Helpprovider
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt225
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Vulcan.Uczniowie.HelpProvider.dll"=>"lib/common"}
#     VCS: http://hg.palaso.org/helpprovider []
# [5] build: IPC-Precise64 (bt279)
#     project: IPC Library
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt279
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/ReleaseMono"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [6] build: IPC-Precise64 (bt279)
#     project: IPC Library
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt279
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"IPCFramework.*"=>"lib/DebugMono"}
#     VCS: https://bitbucket.org/smcconnel/ipcframework [develop]
# [7] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/ReleaseMono"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp []
# [8] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/DebugMono"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp []
# [9] build: palaso-precise64-lfmerge Continuous (Libpalaso_PalasoPrecise64lfmergeContinuous)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Libpalaso_PalasoPrecise64lfmergeContinuous
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Palaso.dll"=>"lib/ReleaseMono", "Palaso.TestUtilities.dll"=>"lib/ReleaseMono", "PalasoUIWindowsForms.dll"=>"lib/ReleaseMono", "PalasoUIWindowsForms.GeckoBrowserAdapter.dll"=>"lib/ReleaseMono", "Palaso.Lift.dll"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/libpalaso.git [lfmerge]
# [10] build: palaso-precise64-lfmerge Continuous (Libpalaso_PalasoPrecise64lfmergeContinuous)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Libpalaso_PalasoPrecise64lfmergeContinuous
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"debug/Palaso.dll"=>"lib/DebugMono", "debug/Palaso.TestUtilities.dll"=>"lib/DebugMono", "debug/PalasoUIWindowsForms.dll"=>"lib/DebugMono", "debug/PalasoUIWindowsForms.GeckoBrowserAdapter.dll"=>"lib/DebugMono", "debug/Palaso.Lift.dll"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/libpalaso.git [lfmerge]
# [11] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu*.dll"=>"lib/ReleaseMono", "icu*.config"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [12] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu*.dll"=>"lib/DebugMono", "icu*.config"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]

# make sure output directories exist
mkdir -p ./MercurialExtensions
mkdir -p ./MercurialExtensions/fixutf8
mkdir -p ./lib/DebugMono
mkdir -p ./lib/ReleaseMono
mkdir -p ./lib/common

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt216/latest.lastSuccessful/Chorus_Help.chm ./lib/common/Chorus_Help.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/debug/Chorus.exe?branch=%3Cdefault%3E ./lib/DebugMono/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/debug/LibChorus.dll?branch=%3Cdefault%3E ./lib/DebugMono/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/debug/ChorusHub.exe?branch=%3Cdefault%3E ./lib/DebugMono/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/debug/ChorusMerge.exe?branch=%3Cdefault%3E ./lib/DebugMono/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/Mercurial-i686.zip?branch=%3Cdefault%3E ./lib/DebugMono/Mercurial-i686.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/Mercurial-x86_64.zip?branch=%3Cdefault%3E ./lib/DebugMono/Mercurial-x86_64.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/debug/LibChorus.TestUtilities.dll?branch=%3Cdefault%3E ./lib/DebugMono/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/.guidsForInstaller.xml?branch=%3Cdefault%3E ./MercurialExtensions/.guidsForInstaller.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/Dummy.txt?branch=%3Cdefault%3E ./MercurialExtensions/Dummy.txt
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/.gitignore?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/.gitignore
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/.guidsForInstaller.xml?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/.guidsForInstaller.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/.hg_archival.txt?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/.hg_archival.txt
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/.hgignore?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/.hgignore
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/README.?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/README.
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/buildcpmap.py?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/buildcpmap.py
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/cpmap.pyc?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/cpmap.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/fixutf8.py?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/fixutf8.py
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/fixutf8.pyc?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/fixutf8.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/fixutf8.pyo?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/fixutf8.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/osutil.py?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/osutil.py
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/osutil.pyc?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/osutil.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/osutil.pyo?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/osutil.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/win32helper.py?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/win32helper.py
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/win32helper.pyc?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/win32helper.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/MercurialExtensions/fixutf8/win32helper.pyo?branch=%3Cdefault%3E ./MercurialExtensions/fixutf8/win32helper.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/Chorus.exe?branch=%3Cdefault%3E ./lib/ReleaseMono/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/LibChorus.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/ChorusHub.exe?branch=%3Cdefault%3E ./lib/ReleaseMono/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/ChorusMerge.exe?branch=%3Cdefault%3E ./lib/ReleaseMono/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/Mercurial-i686.zip?branch=%3Cdefault%3E ./lib/ReleaseMono/Mercurial-i686.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/Mercurial-x86_64.zip?branch=%3Cdefault%3E ./lib/ReleaseMono/Mercurial-x86_64.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/LibChorus.TestUtilities.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/ChorusPrecise64lfmergeCont/latest.lastSuccessful/Autofac.dll?branch=%3Cdefault%3E ./lib/common/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt225/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ./lib/common/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt279/latest.lastSuccessful/IPCFramework.dll ./lib/ReleaseMono/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt279/latest.lastSuccessful/IPCFramework.dll ./lib/DebugMono/IPCFramework.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll ./lib/ReleaseMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll ./lib/DebugMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/Palaso.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/Palaso.TestUtilities.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/PalasoUIWindowsForms.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/PalasoUIWindowsForms.GeckoBrowserAdapter.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/PalasoUIWindowsForms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/Palaso.Lift.dll?branch=%3Cdefault%3E ./lib/ReleaseMono/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/debug/Palaso.dll?branch=%3Cdefault%3E ./lib/DebugMono/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/debug/Palaso.TestUtilities.dll?branch=%3Cdefault%3E ./lib/DebugMono/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll?branch=%3Cdefault%3E ./lib/DebugMono/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/debug/PalasoUIWindowsForms.GeckoBrowserAdapter.dll?branch=%3Cdefault%3E ./lib/DebugMono/PalasoUIWindowsForms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Libpalaso_PalasoPrecise64lfmergeContinuous/latest.lastSuccessful/debug/Palaso.Lift.dll?branch=%3Cdefault%3E ./lib/DebugMono/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ./lib/ReleaseMono/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ./lib/ReleaseMono/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ./lib/DebugMono/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ./lib/DebugMono/icu.net.dll.config
# End of script
