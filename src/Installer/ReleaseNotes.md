## 3.1.1 2020-03-16

* Chorus updated with bugfix for spaces following project names

## 3.1.0 2020-03-04

* FlexBridge should now copy configuration files into a cache folder to avoid losing them on a rollback.
* Update Chorus with bugfix for ldml file merge conflicts


* Support new data model.
* Bugfixes.

## 3.0.0 2017-02-09

* Support FieldWorks 9

## 2.6.2 05-Mar-2018
* Update Chorus with bugfix for timestamps.
## 2.6.1 05/Dec/2017
* Update Chorus with bugfix for ChorusHub timeout problem.
## 2.6.0 09/Nov/2017
* Update version to 2.6.0 to enable updating from the version shipped with
  FieldWorks 8.3.9
## 2.5.4 01/Nov/2017
* FLExBridge: Make sure assembly is properly stamped so that Send/Receive log has useful version info
* FLExBridge: Fix bug LT-18624 - Handle number formats regardless of current culture
* Chorus: Improvement for large projects in resumable Send/Receive
## 2.5.3 18/Jul/2017
* Fix dependency problem on Linux
## 2.5.2 05/Jul/2017
* Update localization files
## 2.5.1 16/Dec/2016
* Support FLEx 8.3 Beta 2 Data model version
## 2.5.0 14/Oct/2016
* FLExBridge: Fix Linux localization issues
* Chorus: Update Send/Receive options to avoid confusion about Language Forge
## 2.4.2 05/Oct/2016
* FLExBridge: Fix handling of data model version 7000069
## 2.4.1 06/Sep/2016
* FLExBridge: Include new Dictionary and Reversal Index Configurations in Send/Receive
* FLExBridge: Add support for FLEx data model version 7000069
## 2.3.14 26/Aug/2016
* Fix linux packaging for 32bit version
## 2.3.13 17/May/2016
* Flex Bridge: Update to work with new Chorus to fix LT-17461
* Chorus: Fix Bug in password entry field with internet S/R
* Chorus: Fix data loss bug when merging certain types of elements
## 2.3.12 20/Apr/2016
* Chorus: Fix problem with internet S/R on windows when using French UI
## 2.3.11 03/Feb/2016
* FLEx Bridge: Update the windows installer dependencies to fix upgrade bug
## 2.3.10 13/Jan/2016
* Flex Bridge: Update Palaso Localizations so that Linux will initialize successfully
## 2.3.9 16/Oct/2015
* Flex Bridge: Update to work with new Chorus to fix LT-17461
* Chorus: Fix Bug in password entry field with internet S/R
* Chorus: Fix data loss bug when merging certain types of elements
* Chorus: Update documentation of Chorus Hub
* FLExBridge: Update documentation of recent fixes
## 2.3.8 24/Sep/2015
* Chorus: Make large internet Send/Receive options more robust (Warning instead of error when retrying a failed Chunk)
* Chorus: Improve Chorus Hub Instructions
* FLExBridge: Provide Chorus Hub Service option on Linux
* FLExBridge: Various linux installer enhancements
## 2.3.5 3/Sep/2015
* L10NSharp: Improve debug information for linux localization
## 2.3.2
* FLEx Bridge: Enhance Windows installer
## 2.3.0
* Chorus: Upgrade Mercurial to version 3
## 2.2.15 4/May/2015
* Chorus: Fix overflow error when receiving very large project data with resumable option
## 2.2.14 3/Feb/2015
* FLEx Bridge: Update localization files for Spanish, French, Chinese and English
## 2.2.13 23/Jan/2015
* Chorus: Improve exception message for unexpected duplicate children.
* Palaso: Fix broken 'Show characters' behavior in Settings Protection.
## 2.2.12 24/Apr/2014
* FLEx Bridge: Fix FLEx Bridge to use new API for the localization system.
* Chorus: Improve message when the user provides invalid login information for Language Depot.
* Chorus: Fix merge crash when two users added the same ldml (Writing System) file.
* L10NSharp: Fix localization library to load and use packaged localization files on Linux.
## 2.2.11 28/Jan/2014
* FLEx Bridge: Fix problem with command line parameters for 'check for updates' option.
## 2.2.10 12/Dec/2013
* Chorus: Add one second to ldml file's internal timestamp, during a merge, to make life easier for FLEx, after a Send/Receive.
* Chorus: Disable improperly displaying "Choose Repositories" page on Linux.
## 2.2.9 10/Dec/2013
* FLEx Bridge: Fixed odd case where FLEx Bridge crashed, while trying to get a new project.
## 2.2.8 5/Dec/2013
* Localization System: Block Linux from allowing editing the localization files, while a bad bug is being fixed.
* Chorus: Fix a crash, while merging LDML (writing system) files.
## 2.2.7 3/Dec/2013
* FLEx Bridge: Improved reporting mechanism that tells users about a failure in FLEx's 'fix it' program.
## 2.2.6 2/Dec/2013
* Palaso: Fix more keyboarding issues (Linux).
* FLEx Bridge: Copy localization files into a writeable location (Linux), so users can save changes.
## 2.2.5 27/Nov/2013
* Chorus: Update instructions (again) for using Chorus Hub and how they are displayed.
* Chorus: Fixed new bug introduced while re-working handling of writing system files (ldml extension).
## 2.2.4 22/Nov/2013
* Palaso: Fix downgraded version numbers to be higher, so users can upgrade from FLEx Bridge 2.1.10 to 2.2.x series.
* Palaso and Chorus: Fix some problems in handling writing system files (ldml extension).
* Chorus: Update instructions for using Chorus Hub.
## 2.2.3 15/Nov/2013
* FLEx Bridge: Fix problem with moving an older Lift Bridge system to its new home.
## 2.2.2 14/Nov/2013
* FLExBridge: Include Launcher for Chorus Hub on Linux.
* FLExBridge: Clean up after cancelling a Send/Receive.
* FLExBridge: Add French, Spanish, and Chinese localizations.
* FLExBridge: Better handling of receiving from a repository with no data in it yet.
* FLExBridge: Help documentation available for Chorus on Linux.
## 2.1.13 11/Oct/2013
* Chorus: No longer add empty notes.
* FLEx Bridge: Shift formal ownership from Randy Regnier to LSDev.
## 2.1.12 09/Oct/2013
* Palaso: Fix problem copying error reporting to clipboard on Linux.
* Chorus: Fix stack overflow problem on Linux when finished using some dialogs.
* Chorus: Fix problem displaying conflict details on Linux.
## 2.1.10 17/Sep/2013
* FLEx Bridge: Fixed problem with FLEx<->FLEx Bridge interaction.
* FLEx Bridge: Don't try to restart FLEx Bridge after updating to a newer version.
## 2.1.9 16/Sep/2013
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
	* Palaso, Chorus, & FLEx Bridge: Add localization support.
	* Chorus: Show technical conflict details on demand.
	* FLEx Bridge: Re-architecture work. Replace Model-View-Controller system with an action handler system that handles the various operations/actions FLEx asks to be done.
	* FLEx Bridge: Update the minimum install version for FLEx.
* Fixes in this release
	* Chorus: Fix merge bug.
	* FLEx Bridge: Make message about 'fix it' program to not be a warning.
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
* Chorus: Remove the older Shared Network support.
* FLEx Bridge: Revise merge system for treating feature structures to allow only one user to make changes to a given feature structure.
## 1.0.16 12/Apr/2013
* FLEx Bridge: Have feature structures only allow one person to makes a change in a given merge.
## 1.0.15 10/Apr/2013
* FLEx Bridge: Initial Beta release.

### Limitations
This version has the following limitations (and probably many others). Feel free to suggest your priorities and suggest additions to the list.

* Only works with FLEx 8+.
* Some merge reports may not be the easiest to read and understand.
* Some types of lexical relations appear to not merge correctly in both Lift data and in the main FLEx data set. The issues are more complex than merging.
