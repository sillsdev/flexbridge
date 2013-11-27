# Since Chorus and Palaso libraries change frequently, you will likely need to get those
# projects and be able to build them.  Run this script to build and cp those libraries.
# This script assumes that the Chorus and Palaso directories are on the same level as
# this one, and that the FieldWorks repo is in C:/fwrepo.
# It copies the needed libraries into the lib folder.

cp ../chorus/Download/L10NSharp.dll* lib/common/
# Uncomment these two lines if you are working on L10NSharp
# cp ../l10nsharp/output/DebugMono/L10NSharp.dll lib/common/
# cp ../l10nsharp/output/DebugMono/L10NSharp.* lib/DebugMono/

cp ../chorus/output/DebugMono/LibChorus.TestUtilities.dll* lib/DebugMono/
cp ../chorus/output/DebugMono/LibChorus.TestUtilities.dll* output/DebugMono/
cp ../chorus/output/DebugMono/LibChorus.dll* lib/DebugMono/
cp ../chorus/output/DebugMono/LibChorus.dll* output/DebugMono/

cp ../chorus/output/DebugMono/Chorus.exe* lib/DebugMono/
cp ../chorus/output/DebugMono/Chorus.exe* output/DebugMono/

cp ../chorus/output/DebugMono/ChorusMerge.exe* lib/DebugMono/
cp ../chorus/output/DebugMono/ChorusMerge.exe* output/DebugMono/

cp ../chorus/output/DebugMono/ChorusHub.exe* lib/DebugMono/
cp ../chorus/output/DebugMono/ChorusHub.exe* output/DebugMono/

cp ../chorus/output/DebugMono/Palaso*.dll* lib/DebugMono/
cp ../chorus/output/DebugMono/Palaso*.dll* output/DebugMono/
