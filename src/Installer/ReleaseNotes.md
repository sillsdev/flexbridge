## DEV_VERSION_NUMBER: DEV_RELEASE_DATE
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
See additional limitations [here](http://projects.palaso.org/projects/fwbridge/wiki/Happy_Path/).
