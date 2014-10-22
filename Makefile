# Linux/Mono Makefile for FlexBridge.

CPUARCH=$(shell /usr/bin/arch)
# this needs to be in sync with debian/changelog and build/build.common.proj
BUILD_NUMBER="2.2.12"
BUILD_VCS_NUMBER="aa832dc7a47125bfb80daf4eaa726f2cd4578154"
UploadFolder="Beta"
# Work around proxy bug in older mono to allow dependency downloads
no_proxy := $(no_proxy),*.local

all: release

release:
	cd build && xbuild FLExBridge.build.mono.proj /t:Build /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER) /p:UploadFolder=$(UploadFolder) /p:Configuration=ReleaseMono /v:debug

debug:
	FBCommonAppData="/tmp/flexbridge"
	if test ! -d "/tmp/flexbridge"; then mkdir -p "/tmp/flexbridge"; fi;
	export FBCommonAppData
	cd build && xbuild FLExBridge.build.mono.proj /t:Build /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER) /p:UploadFolder=$(UploadFolder) /p:Configuration=DebugMono

clean:
	cd build && xbuild FLExBridge.build.mono.proj /t:Clean /p:RootDir=..
	/bin/rm -rf output Download Mercurial

install: release
	cd build && xbuild FLExBridge.build.mono.proj /t:Prepackaging /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:Configuration=ReleaseMono
	/usr/bin/install -d $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install output/ReleaseMono/*.* $(DESTDIR)/usr/lib/flexbridge
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.htm
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.png
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.config
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.md*
	/usr/bin/install lib/common/setup-user.sh $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install lib/common/run-app $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install -m644 lib/common/Mercurial-$(CPUARCH).zip $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install lib/common/Chorus_Help.chm $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install -d $(DESTDIR)/usr/lib/flexbridge/localizations
	/usr/bin/install -m644 output/ReleaseMono/localizations/*.* $(DESTDIR)/usr/lib/flexbridge/localizations
	/usr/bin/install -d $(DESTDIR)/var/lib/flexbridge/localizations
	/usr/bin/install -m644 output/ReleaseMono/localizations/*.* $(DESTDIR)/var/lib/flexbridge/localizations
	/usr/bin/install -d $(DESTDIR)/usr/bin
	/usr/bin/install lib/common/fieldworks-chorus $(DESTDIR)/usr/bin
	/usr/bin/install lib/common/fieldworks-chorushub $(DESTDIR)/usr/bin
	/usr/bin/install -d $(DESTDIR)/usr/share/pixmaps
	/usr/bin/install -m644 lib/common/chorusHubIcon.png $(DESTDIR)/usr/share/pixmaps
	/usr/bin/install -d $(DESTDIR)/usr/share/applications
	/usr/bin/install -m644 lib/common/fieldworks-chorushub.desktop $(DESTDIR)/usr/share/applications
	# remove unwanted stuff
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/FwdataTestApp.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/*.TestUtilities.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/*Bridge-ChorusPluginTests.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/nunit.framework.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/TheTurtle.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/NetSparkle.*
