#!/bin/bash

# For user machines, assume FieldWorks is installed in /usr/lib/fieldworks,
# and FLExBridge is installed in /usr/lib/flexbridge.
# For developer machines, assume both FieldWorks and FLExBridge source
# repositories are under $HOME.
# We assume a user machine to start out with, since it's simpler.

scriptdir="$(dirname "$0")"
prefix="$(cd "$scriptdir/../.."; /bin/pwd)"

WRITEKEY="$prefix/lib/fieldworks/WriteKey.exe"

if [ ! -f "${WRITEKEY}" ]; then
	WRITEKEY="$(find "$HOME" -name WriteKey.exe -print | grep /fwrepo/fw/Bin/ | head -1)"
	if [ -z "${WRITEKEY}" ]; then
		WRITEKEY="$(find "$HOME" -name WriteKey.exe -print | grep /fw/Bin/ | head -1)"
	fi
fi
READKEY="$(dirname ${WRITEKEY})/ReadKey.exe"

FLEXBRIDGEDIR="$(mono ${READKEY} LM "Software/SIL/Flex Bridge/8" "InstallationDir")"
if [ -n "${FLEXBRIDGEDIR}" -a -d "${FLEXBRIDGEDIR}" ]; then
	if [ -f "${FLEXBRIDGEDIR}/FLExBridge.exe" ]; then
		echo ${FLEXBRIDGEDIR}/FLExBridge.exe already set up
		exit 0
	fi
fi

FLEXBRIDGEDIR="$prefix/lib/flexbridge"

if [ ! -f "${FLEXBRIDGEDIR}/FLExBridge.exe" ]; then
	FLEXBRIDGEEXE="$(find "$HOME" -name FLExBridge.exe -print | sort | head -1)"
	if [ -n "${FLEXBRIDGEEXE}" ]; then
		FLEXBRIDGEDIR="$(dirname "${FLEXBRIDGEEXE}")"
	fi
fi

if [ -f "${WRITEKEY}" -a -f "${FLEXBRIDGEDIR}/FLExBridge.exe" ]; then
	#echo mono "${WRITEKEY}" LM "Software/SIL/Flex Bridge/8" "InstallationDir" "${FLEXBRIDGEDIR}"
	mono "${WRITEKEY}" LM "Software/SIL/Flex Bridge/8" "InstallationDir" "${FLEXBRIDGEDIR}"
else
	echo could not find WriteKey.exe or FLExBridge.exe
	exit 1
fi

if [ -f $scriptdir/Chorus.exe -a ! -f $scriptdir/Mercurial/hg ]; then
	cd $scriptdir
	ARCH="$(/usr/bin/arch)"
	unzip Mercurial-${ARCH}.zip
	chmod +x Mercurial/hg
	chmod +x Mercurial/hg.exe
fi

exit 0
