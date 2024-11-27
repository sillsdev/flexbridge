# How to develop FileTypeHandlers in FlexBridge

## Overview

In the *[Chorus](https://github.com/sillsdev/chorus?tab=readme-ov-file#overview)* project, FileTypeHandlers are used to define custom handling logic for different types of files managed by Chorus.
They are used to merge, identify conflicts, or process files specific to a part of the project data, and present conflicts to the user.
These are discovered at runtime using *[MEF](https://learn.microsoft.com/en-us/dotnet/framework/mef/)*.
 Any plugins loaded into chorus which implement IChorusFileTypeHandler will be used during merging.

FlexBridge implements a single ***[IChorusFileTypeHandler](https://github.com/sillsdev/chorus/blob/master/src/LibChorus/FileTypeHandlers/IChorusFileTypeHandler.cs)***
 in the ***FieldWorksCommonFileHandler*** which uses MEF its self to load
 ***[IFieldWorksFileHandler](https://github.com/sillsdev/flexbridge/blob/develop/src/LibFLExBridge-ChorusPlugin/Handling/IFieldWorksFileHandler.cs)***s
 for the various file types.
 
 A major reason for this is to allow a single version of FlexBridge to support multiple FieldWorks model versions through use of a MetadataCache.

Implementations of ***IFieldWorksFileHandler*** in FlexBridge provide the different file handlers.
 Each ***IFieldWorksFileHandler*** has the methods for validating files, calculating differences, merging differences, and presenting the differences.

FlexBridge breaks the fwdata file into a model based hierarchy.
 Different major portions of the model are split into separate files so that merging logic can be modular and specific.
 FlexBridge also handles the merging of configuration files and other non-model project data.

## Adding a new file type

To handle a new file type:
1. Implement the *IFieldWorksFileHandler* interface.
1.1 Define logic and properties for:
   - File extension handled (*Extension*)
   - File validation support (*CanValidateFile*, *ValidateFile*) for verifying post merge content.
   - Merging (*Find2WayDifferences*, *Do3WayMerge*) handling both the difference detection and merging.
   - Change presentation (*GetChangePresenter*) for generating the user reports on the merge.
2. Include thorough unit tests for your new filetype especially on any specific merge situations

## Debugging the merge code

1. The easiest way to debug is using the unit testing framework.
 There is an extensive library of unit tests on the existing IFieldWorksFileHandler implementations.
 Looking at the differences which are causing a bug (using TortoiseHg for example) should let you construct a unit test to replicate any failures.
2. It is possible to modify *ChorusMerge* in Chorus to pause and allow you to attach a debugger when it is merging files.
 This can enable you to attach FlexBridge to the ChorusMerge.exe process while it is running and step into your merging code.
 This should be considered a last resort if you can't identify the problem and produce a failing unit test