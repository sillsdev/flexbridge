## FLEx Bridge Localization

The few FlexBridge strings that exist are in the FieldWorks crowdin project (https://crowdin.com/project/fieldworks)

### Updating Crowdin with source string changes

#### Using ExtractXliff - NOT YET IMPLEMENTED

**This cannot yet be implemented because ExtractXliff and FLEx Bridge have incompatible bitnesses**

The L10nSharp tool ExtractXliff is run on the project to get any updates to the source strings resulting in a new FlexBridge.en.xlf file.

Overcrowdin is used to upload this file to Crowdin.

This process is run automatically by a GitHub action if the commit comment mentions any of 'localize, l10n, i18n, internationalize, spelling'

It can also be run manually as follows:
```
dotnet tool install -g overcrowdin
set CROWDIN_API_KEY=TheApiKeyForTheFieldWorksProject
msbuild l10n.proj /t:UpdateCrowdin
```

#### Using the XLF generated when running FLEx Bridge

Run FLEx Bridge, then:
```
dotnet tool install -g overcrowdin
set CROWDIN_API_KEY=TheApiKeyForTheFieldWorksProject
copy %LocalAppData%\SIL\FlexBridge\localizations\FlexBridge.en.xlf .
overcrowdin updatefiles
```

### Update with latest localizations (Run before building an installer, or to test)

Nuget packages for libpalso and chorus are used to get their localized xliff files.
Overcrowdin is used to download the latest translation data for FlexBridge from the FieldWorks project.

The resulting files are copied to the DistFiles/localizations folder.

This process is run by the Installer build on GitHub Actions.

It can also be run manually as follows:
```
dotnet tool install -g overcrowdin
set CROWDIN_API_KEY=TheApiKeyForTheFieldWorksProject
msbuild l10n.proj /t:restore
msbuild l10n.proj /t:GetLatestL10ns
```