#!/bin/bash

# Since Chorus and Palaso libraries change frequently, you will likely need to get those
# projects and be able to build them.  Run this script to build and cp those libraries.
# This script assumes that the Chorus and Palaso directories are on the same level as
# this one, and that the FieldWorks repo is in C:/fwrepo.
# It copies the needed libraries into the lib folder.

CHORUS_DIR=../chorus

if [ ! -d ${CHORUS_DIR} ]
then
	exit
fi

if [ "$1"=="" ]
then
	BUILD_CONFIG=Debug
else
	BUILD_CONFIG=$1
fi

pushd ${CHORUS_DIR}
./UpdateDependencies.sh ${BUILD_CONFIG}
build/TestBuild.sh ${BUILD_CONFIG}
popd

if [ ! -d output/${BUILD_CONFIG}Mono ]
then
	if [ ! -d output ]
	then
		mkdir output
	fi
	mkdir output/${BUILD_CONFIG}Mono
fi

cp ${CHORUS_DIR}/lib/${BUILD_CONFIG}Mono/L10NSharp.dll* lib/common/
# Uncomment these two lines if you are working on L10NSharp
# cp ../l10nsharp/output/${BUILD_CONFIG}Mono/L10NSharp.dll lib/common/
# cp ../l10nsharp/output/${BUILD_CONFIG}Mono/L10NSharp.* lib/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/Autofac.dll* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/Autofac.dll* lib/common

cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/LibChorus.TestUtilities.dll* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/LibChorus.TestUtilities.dll* output/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/LibChorus.dll* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/LibChorus.dll* output/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/Chorus.exe* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/Chorus.exe* output/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/ChorusMerge.exe* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/ChorusMerge.exe* output/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/chorusmerge lib/common/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/chorusmerge output/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/ChorusHub.exe* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/output/${BUILD_CONFIG}Mono/ChorusHub.exe* output/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/lib/${BUILD_CONFIG}Mono/Palaso*.dll* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/lib/${BUILD_CONFIG}Mono/Palaso*.dll* output/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/lib/${BUILD_CONFIG}Mono/SIL.*.dll* lib/${BUILD_CONFIG}Mono/
cp ${CHORUS_DIR}/lib/${BUILD_CONFIG}Mono/SIL.*.dll* output/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/lib/${BUILD_CONFIG}Mono/icu*.dll* lib/${BUILD_CONFIG}Mono/

cp ${CHORUS_DIR}/lib/common/Vulcan.Uczniowie.HelpProvider.dll* lib/common
