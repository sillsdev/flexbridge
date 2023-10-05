# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Added

- Command to clone a specific project

### Removed

- Command to check for Updates

## [3.8.0] - 2022-12-02

### Added

- Add .NET Standard to LfMergeBridge, LibFLExBridge-ChorusPlugin, and LibTriboroughBridge-ChorusPlugin

### Changed

- Reorganise Chorus Internet Server Settings control, add a help button to the Get from Internet dialog (LT-21209)
- Increase the audio and picture file size limit from 1 MB to 10 MB

## [3.7.1] - 2022-08-09

### Fixed

- Use a cross-process mutex (on Windows) to avoid crashes trying to update the Notes file
- Updated to the latest Mercurial package so users no longer rely on python-is-python2

## [3.6.3] - 2022-07-29

### Fixed

- Use Atomic elements for languages and list patterns in LDML to prevent their multiplication

## [3.6.1] - 2022-06-15

### Fixed

- Clean up Flexbridge login dialog
- Move the location of the local Chorus cache

## [3.6.0] - 2022-02-02

### Fixed

- Enlarge the settings dialog to fit wide comboboxes
- If only one project is available to send and receive, select it automatically
- Use invariant culture in Date.ToString in the premerger

## [3.4.2] - 2021-10-06

### Fixed

- Move settings to a stable location; migrate old settings
- Fix Localization of the shared Palaso library
- Fix a crash in the settings dialog on Linux

### Changed

- Update the embedded browser (Geckofx)

## [3.3.0] - 2021-07-13

### Changed

- Create nuget packages

## [3.2] - older version

[Unreleased]: https://github.com/sillsdev/flexbridge/compare/v3.3.0...develop

[3.3.0]: https://github.com/sillsdev/flexbridge/compare/v3.2.1...v3.3.0
