## DEV_VERSION_NUMBER: DEV_RELEASE_DATE
* Chorus: Users with large numbers of revisions would see the internet hang after an update.
## 2.1.8 3/Sep/2013
* New in this release
	* FLEx Bridge: Revise FLEx<->FLEx Bridge interaction to allow Linux to do Send/Receive.
	* FLEx Bridge: Add support for FLEx's data model version 68.
* Fixes in this release
	* FLEx Bridge: Allow for better LIFT Send/Receive support for Shared network system (Db4o data storage system).
	* FLEx Bridge: Numerous fixes for Linux support.
	* FLEx Bridge: Fixed case where clicking "X" on S/R window caused crash.
## 2.0.20 14/Aug/2013
* L10NSharp: No "OK" button text for unavailable languages
* FLEx Bridge: Prompt only once if localization unavailable
* FLEx Bridge: Fix problem in POS context generator for conflict reports
## 2.0.19 24/Jul/2013
* New in this release
	* FLEx Bridge: Re-architecture work. Replace Model-View-Controller system with an action handler system that handles the various operations/actions FLEx asks to be done.
	* FLEx Bridge: Update the minimum install version for FLEx.
	* Palaso, Chorus, & FLEx Bridge: Add localization support.
	* Chorus: Show technical conflict details on demand.
* Fixes in this release
	* FLEx Bridge: Make message about 'fix it' program to not be a warning.
	* Chorus: Fix merge bug.
## 1.0.31 11/Jul/2013
* FLEx Bridge: Add better support for FLEx data migration DM68.
## 1.0.30 9/Jul/2013
* Chorus: Fix problem with sending unrelated repositories out.
## 1.0.29 1/Jul/2013
Flex Bridge: Fix crash getting an empty repository.
## 1.0.28 25/Jun/2013
* Chorus: Fix problem where source repository had no initial commit.
## 1.0.27 23/Jun/2013
* Chorus: Fix merge bug.
## 1.0.26 10/June/2013
* Palaso: Fix issue with loading large xml files.
## 1.0.24 22/May/2013
* Chorus: Fix issue with unresolved merge (a merge didn't complete).
## 1.0.23 16/May/2013
* Palaso: Fix issue with loading xml files (e.g., Lift files) with comments.
* Chorus: Fix issue with removing some duplicate data in a Lift file.
## 1.0.22 11/May/2013
* Chorus: Fix problem with timeout limit stopping a merge before it is finished.
* FLEx Bridge: Add support for two more FLEx data migrations (DM 67 & DM68).
## 1.0.21 2/May/2013
* FLEx Bridge: Fix problem with VirtualOrder instances and unowned pictures getting lost, if an annotation file is not also present.
## 1.0.20 30/Apr/2013
* Chorus: Fix unreported bug with one delete vs. edit conflict report.
## 1.0.19 30/Apr/2013
* Chorus: Update help file.
* Chorus: Fix problem with "Collection was modified" error.
## 1.0.18 19/Apr/2013
* Chorus: Update Chorus Help file and connect one Help button to the help.
## 1.0.17 15/Apr/2013
* FLEx Bridge: Revise merge system for treating feature structures to allow only one user to make changes to a given feature structure.
* Chorus: Remove the older Shared Network support.
## 1.0.16 12/Apr/2013
* FLEx Bridge: Have feature structures only allow one person to makes a change in a given merge.
## 1.0.15 10/Apr/2013
* FLEx Bridge: Initial Beta release.

### Limitations
This version has the following limitations (and probably many others). Feel free to suggest your priorities and suggest additions to the list.

* Only works with FLEx 8+.
* Not available yet for Linux.
* Some merge reports may not be the easiest to read and understand.
* Some types of lexical relations appear to not merge correctly in both Lift data and in the main FLEx data set. The issues are more complex than merging.
