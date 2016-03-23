#!/bin/bash

# If you are working on FLExBridge, there is a good chance you are working on Chorus as
# it relates to FieldWorks.  To update these dependencies, first, build FLEx and Chorus
# (GetAndBuildThis.bat in either flexbridge or chorus will build Chorus); then, run
# this script to copy the dependencies.
# This script assumes that the Chorus and Palaso directories are on the same level as
# this one, and that the FieldWorks repo is in ~/fwrepo.
# It copies the needed libraries into the ${BUILD_CONFIG}Mono folder.

if [ "$1" == "" ]
then
	BUILD_CONFIG=Debug
else
	BUILD_CONFIG=$1
fi

if [ ! -d ~/fwrepo/fw/Downloads ]
then
	mkdir ~/fwrepo/fw/Downloads
fi

if [ ! -d ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG} ]
then
	if [ ! -d ~/fwrepo/fw/Output_x86_64 ]
	then
		mkdir ~/fwrepo/fw/Output_x86_64
	fi
	mkdir ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG}
fi

cp ../chorus/output/${BUILD_CONFIG}Mono/LibChorus.dll* ~/fwrepo/fw/Downloads/
cp ../chorus/output/${BUILD_CONFIG}Mono/LibChorus.dll* ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG}/

cp ../chorus/output/${BUILD_CONFIG}Mono/Chorus*.exe ~/fwrepo/fw/Downloads/
cp ../chorus/output/${BUILD_CONFIG}Mono/Chorus*.exe ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG}/

# Uncomment this line if you are working on L10NSharp:
# cp ../l10nsharp/output/${BUILD_CONFIG}Mono/L10NSharp.* ~/fwrepo/fw/Downloads/
# cp ../l10nsharp/output/${BUILD_CONFIG}Mono/L10NSharp.* ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG}/

cp ../chorus/lib/${BUILD_CONFIG}Mono/Palaso*.dll* ~/fwrepo/fw/Downloads/
cp ../chorus/lib/${BUILD_CONFIG}Mono/Palaso*.dll* ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG}/

cp ../chorus/lib/${BUILD_CONFIG}Mono/SIL.*.dll* ~/fwrepo/fw/Downloads/
cp ../chorus/lib/${BUILD_CONFIG}Mono/SIL.*.dll* ~/fwrepo/fw/Output_x86_64/${BUILD_CONFIG}/
