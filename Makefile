# Linux/Mono Makefile for FlexBridge, mainly intended for use during Linux package build

-include gitversion.properties
CPUARCH=$(shell /usr/bin/arch)
BUILD_NUMBER=$(BuildVersion)
ifneq ($(origin BUILD_NUMBER), '')
BUILD_NUMBER=0
endif
BUILD_VCS_NUMBER=$(shell cat vcs_version)
XDG_CONFIG_HOME ?= /tmp/.config
UploadFolder="Alpha"
# Work around proxy bug in older mono to allow dependency downloads
no_proxy := $(no_proxy),*.local
GECKOFX60_VERSION := 60.0.51
PREFIX ?= /usr
CONFIGURATION ?= Release

all: release

download_dependencies:
	cd l10n \
	  && msbuild l10n.proj /t:restore \
	  && msbuild l10n.proj /t:CopyL10nsToDistFiles

release: vcs_version download_dependencies release_build

release_build:
	echo "in Makefile: BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER)"
	# Don't update the assembly info - there's no git repo during package build
	cd build \
	  && msbuild FLExBridge.proj /t:Build /p:GetVersion=false /p:BUILD_NUMBER=$(BUILD_NUMBER) \
			/p:BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER) /p:UploadFolder=$(UploadFolder) \
			/p:Configuration=Release /p:RestorePackages=false /p:UpdateAssemblyInfo=false \
	  /p:WriteVersionInfoToBuildLog=false
	cp -a flexbridge output/Release

debug: vcs_version download_dependencies debug_build

debug_build:
	FBCommonAppData="/tmp/flexbridge"
	if test ! -d "/tmp/flexbridge"; then mkdir -p "/tmp/flexbridge"; fi;
	export FBCommonAppData
	# Don't update the assembly info - there's no git repo during package build
	cd build \
	  && msbuild FLExBridge.proj /t:Build /p:GetVersion=false /p:BUILD_NUMBER=$(BUILD_NUMBER) \
			/p:BUILD_VCS_NUMBER=$(BUILD_VCS_NUMBER) /p:UploadFolder=$(UploadFolder) \
			/p:Configuration=Debug /p:RestorePackages=false /p:UpdateAssemblyInfo=false \
		/p:WriteVersionInfoToBuildLog=false
	# Put flexbridge next to FLExBridge.exe, as it will be in a user's machine, so FW can easily find it on a developer's machine.
	cp -a flexbridge output/Debug

# Create AssemblyInfo files and properties file. When building the package we don't have a git
# repo, so we have to create the files beforehand.
version:
	[ -e .git ] && cd build \
		&& msbuild /t:"RestoreBuildTasks;RestorePackages;UpdateAssemblyInfoForPackage" FLExBridge.proj \
		&& msbuild /t:VersionNumbers FLExBridge.proj || true

# generate the vcs_version file, this hash is used to update the about.htm information
# when building the package we don't have a git repo, so we rely to get the information from the
# build agent
vcs_version: version
	[ -e .git ] && git rev-parse --short HEAD >vcs_version || true

clean:
	cd build && msbuild FLExBridge.proj /t:Clean
	/bin/rm -rf output Download Mercurial

fetch_l10ns:
	dotnet tool update -g overcrowdin || dotnet tool install -g overcrowdin
	bash -c '\
		export PATH="$$PATH:${HOME}/.dotnet/tools" \
	    && cd l10n \
	    && msbuild l10n.proj /t:restore \
	    && msbuild l10n.proj /t:GetlatestL10ns \
	'

install: fetch_l10ns install-core

install-core:
	/usr/bin/install -d $(DESTDIR)$(PREFIX)/lib/flexbridge
	/usr/bin/install output/$(CONFIGURATION)/net461/*.* $(DESTDIR)$(PREFIX)/lib/flexbridge
	/bin/chmod -x $(DESTDIR)$(PREFIX)/lib/flexbridge/*.htm
	/bin/chmod -x $(DESTDIR)$(PREFIX)/lib/flexbridge/*.png
	/bin/chmod -x $(DESTDIR)$(PREFIX)/lib/flexbridge/*.config
	/usr/bin/install flexbridge environ environ-xulrunner $(DESTDIR)$(PREFIX)/lib/flexbridge
	/usr/bin/install lib/common/run-app $(DESTDIR)$(PREFIX)/lib/flexbridge
	# Copy mercurial for both architectures since flexbridge is an any architecture package.
	/usr/bin/install -m644 lib/$(CONFIGURATION)/net461/Mercurial-x86_64.zip $(DESTDIR)$(PREFIX)/lib/flexbridge
	/usr/bin/install -m644 lib/$(CONFIGURATION)/net461/Mercurial-i686.zip $(DESTDIR)$(PREFIX)/lib/flexbridge
	cp -r MercurialExtensions $(DESTDIR)$(PREFIX)/lib/flexbridge
	/usr/bin/install lib/common/Chorus_Help.chm $(DESTDIR)$(PREFIX)/lib/flexbridge
	/usr/bin/install lib/common/chorusmerge $(DESTDIR)$(PREFIX)/lib/flexbridge
	/usr/bin/install -d $(DESTDIR)$(PREFIX)/lib/flexbridge/localizations
	/usr/bin/install -m644 output/$(CONFIGURATION)/net461/localizations/*.* $(DESTDIR)$(PREFIX)/lib/flexbridge/localizations
	/usr/bin/install -d $(DESTDIR)/var/lib/flexbridge/localizations
	/usr/bin/install -m644 output/$(CONFIGURATION)/net461/localizations/*.* $(DESTDIR)/var/lib/flexbridge/localizations
	/usr/bin/install -d $(DESTDIR)$(PREFIX)/bin
	/usr/bin/install lib/common/fieldworks-chorus $(DESTDIR)$(PREFIX)/bin
	/usr/bin/install lib/common/fieldworks-chorushub $(DESTDIR)$(PREFIX)/bin
	/usr/bin/install -d $(DESTDIR)$(PREFIX)/share/pixmaps
	/usr/bin/install -m644 lib/common/chorusHubIcon.png $(DESTDIR)$(PREFIX)/share/pixmaps
	/usr/bin/install -d $(DESTDIR)$(PREFIX)/share/applications
	/usr/bin/install -m644 lib/common/fieldworks-chorushub.desktop $(DESTDIR)$(PREFIX)/share/applications
	# remove unwanted stuff
	/bin/rm -f $(DESTDIR)$(PREFIX)/lib/flexbridge/FwdataTestApp.*
	/bin/rm -f $(DESTDIR)$(PREFIX)/lib/flexbridge/*.TestUtilities.*
	/bin/rm -f $(DESTDIR)$(PREFIX)/lib/flexbridge/*Bridge-ChorusPluginTests.*
	/bin/rm -f $(DESTDIR)$(PREFIX)/lib/flexbridge/nunit.framework.*
	/bin/rm -f $(DESTDIR)$(PREFIX)/lib/flexbridge/TheTurtle.*
	/bin/rm -f $(DESTDIR)$(PREFIX)/lib/flexbridge/NetSparkle.*
