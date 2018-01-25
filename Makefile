# Linux/Mono Makefile for FlexBridge.

CPUARCH=$(shell /usr/bin/arch)
BUILD_NUMBER=$(shell cat version)
BUILD_VCS_NUMBER=$(shell cat vcs_version)
XDG_CONFIG_HOME ?= /tmp/.config
UploadFolder="Alpha"
# Work around proxy bug in older mono to allow dependency downloads
no_proxy := $(no_proxy),*.local

all: release

release: vcs_version
	echo "in Makefile: BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER)"
	./download_dependencies_linux.sh && . ./environ && cd build && xbuild FLExBridge.proj /t:Build /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER) /p:UploadFolder=$(UploadFolder) /p:Configuration=ReleaseMono /v:debug
	cp -a packages/Geckofx45.64.Linux.45.0.21.0/build/Geckofx-Core.dll.config packages/Geckofx45.64.Linux.45.0.21.0/lib/net40
	cp -a packages/Geckofx45.32.Linux.45.0.21.0/build/Geckofx-Core.dll.config packages/Geckofx45.32.Linux.45.0.21.0/lib/net40
	cp -a flexbridge output/ReleaseMono

debug: vcs_version
	FBCommonAppData="/tmp/flexbridge"
	if test ! -d "/tmp/flexbridge"; then mkdir -p "/tmp/flexbridge"; fi;
	export FBCommonAppData
	./download_dependencies_linux.sh && . ./environ && cd build && xbuild FLExBridge.proj /t:Build /p:BUILD_NUMBER=$(BUILD_NUMBER) /p:BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER) /p:UploadFolder=$(UploadFolder) /p:Configuration=DebugMono
	cp -a packages/Geckofx45.64.Linux.45.0.21.0/build/Geckofx-Core.dll.config packages/Geckofx45.64.Linux.45.0.21.0/lib/net40
	cp -a packages/Geckofx45.32.Linux.45.0.21.0/build/Geckofx-Core.dll.config packages/Geckofx45.32.Linux.45.0.21.0/lib/net40
	# Put flexbridge next to FLExBridge.exe, as it will be in a user's machine, so FW can easily find it on a developer's machine.
	cp -a flexbridge output/DebugMono

# generate the vcs_version file, this hash is used to update the about.htm information
# when building the package we don't have a git repo, so we rely to get the information from the
# build agent
vcs_version:
	[ -d .git ] && git rev-parse --short HEAD >vcs_version || true

clean:
	. ./environ && cd build && xbuild FLExBridge.proj /t:Clean
	/bin/rm -rf output Download Mercurial

install:
	/usr/bin/install -d $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install output/ReleaseMono/*.* $(DESTDIR)/usr/lib/flexbridge
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.htm
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.png
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.config
	/bin/chmod -x $(DESTDIR)/usr/lib/flexbridge/*.md*
	/usr/bin/install flexbridge environ environ-xulrunner $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install lib/common/setup-user.sh $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install lib/common/run-app $(DESTDIR)/usr/lib/flexbridge
	# Copy mercurial for both architectures since flexbridge is an any architecture package.
	/usr/bin/install -m644 lib/ReleaseMono/Mercurial-x86_64.zip $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install -m644 lib/ReleaseMono/Mercurial-i686.zip $(DESTDIR)/usr/lib/flexbridge
	cp -r MercurialExtensions $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install lib/common/Chorus_Help.chm $(DESTDIR)/usr/lib/flexbridge
	/usr/bin/install lib/common/chorusmerge $(DESTDIR)/usr/lib/flexbridge
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
