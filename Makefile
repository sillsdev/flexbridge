# Linux/Mono Makefile for FlexBridge.

CPUARCH=$(shell /usr/bin/arch)
# this needs to be in sync with debian/changelog and build/build.common.proj
BUILD_NUMBER="2.1.0"
MINOR="1"

all: release

release:
	cd build && xbuild FLExBridge.build.mono.proj /t:Build /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:Configuration=ReleaseMono /v:debug

debug:
	cd build && xbuild FLExBridge.build.mono.proj /t:Build /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:Configuration=DebugMono

clean:
	cd build && xbuild FLExBridge.build.mono.proj /t:Clean /p:RootDir=..
	/bin/rm -rf output Download Mercurial
	/usr/bin/find . -name obj -type d -print | xargs /bin/rm -rf

install: release
	cd build && xbuild FLExBridge.build.mono.proj /t:Prepackaging /p:RootDir=.. /p:teamcity_dotnet_nunitlauncher_msbuild_task=notthere /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:Configuration=ReleaseMono
	/usr/bin/install -d $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install output/ReleaseMono/*.* $(DESTDIR)/usr/lib/flexbridge
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.htm
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.png
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.config
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.md*
	/usr/bin/install lib/common/setup-user.sh $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install -d $(DESTDIR)/usr/lib/flexbridge/localizations
	/usr/bin/install -m644 output/ReleaseMono/localizations/*.* $(DESTDIR)/usr/lib/flexbridge/localizations
	/usr/bin/install -m644 lib/common/Mercurial-$(CPUARCH).zip $(DESTDIR)/usr/lib/flexbridge
	# remove unwanted stuff
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/FwdataTestApp.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/*.TestUtilities.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/*Bridge-ChorusPluginTests.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/nunit.framework.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/TheTurtle.*
	/bin/rm -f $(DESTDIR)/usr/lib/flexbridge/NetSparkle.*
