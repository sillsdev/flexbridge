// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.CustomProperties
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class CustomPropertiesTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		private const string Key = "key";

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, FlexBridgeConstants.CustomProperties) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.CustomPropertiesFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.AdditionalFieldsTag)
					return "Not valid custom properties file";
				if (!root.HasElements)
					return null; // CustomFields are optional.

				var requiredAttrs = new HashSet<string>
										{
											"name",
											"class",
											"type",
											Key // Special attr added so fast xml splitter can find each one.
										};
				var optionalAttrs = new HashSet<string>
										{
											"destclass",
											"wsSelector",
											"helpString",
											"listRoot",
											"label"
										};
				foreach (var customFieldElement in root.Elements(FlexBridgeConstants.CustomField))
				{
					if (requiredAttrs
						.Any(requiredAttr => customFieldElement.Attribute(requiredAttr) == null))
					{
						return "Missing required custom property attribute";
					}
					if (customFieldElement.Attributes()
						.Any(attribute => !requiredAttrs.Contains(attribute.Name.LocalName)
							&& !optionalAttrs.Contains(attribute.Name.LocalName)))
					{
						return "Contains unrecognized attribute";
					}
					// Make sure 'key' attr is class+name.
					if (customFieldElement.Attribute("class").Value + customFieldElement.Attribute("name").Value != customFieldElement.Attribute(Key).Value)
						return "Mis-matched 'key' attribute with property class+name atributes";

					if (customFieldElement.HasElements)
						return "Contains illegal child element";
				}

				MetadataCache.MdCache.AddCustomPropInfo(new MergeOrder(
					pathToFile, pathToFile, pathToFile,
					new MergeSituation(pathToFile, "", "", "", "", MergeOrder.ConflictHandlingModeChoices.WeWin)));

				return null;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				FlexBridgeConstants.CustomField, Key);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc,
				false); // We don't want (or even need) the custom properties to be added to the MDC, while merging the custom props file itself.
						// We won't even know what they are until after the merge is done.
		}

		public string Extension
		{
			get { return FlexBridgeConstants.CustomProperties; }
		}

		#endregion
	}
}