#!/bin/bash
mono ILRepack.exe /target:library /out:../../output/common/ChorusPlus.dll Chorus.exe LibChorus.dll Palaso.dll Autofac.dll NDesk.DBus.dll ../common/icu.net.dll
mono ILRepack.exe /out:../../output/common/ChorusMerge.exe ChorusMerge.exe LibChorus.dll Palaso.dll Autofac.dll NDesk.DBus.dll ../common/icu.net.dll
