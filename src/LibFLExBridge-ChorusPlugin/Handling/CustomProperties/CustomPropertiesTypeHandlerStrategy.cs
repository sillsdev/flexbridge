// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.CustomProperties
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class CustomPropertiesTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private const string Key = "key";

		private IFieldWorksFileHandler AsIFieldWorksFileHandler
		{
			get { return this; }
		}

		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, AsIFieldWorksFileHandler.Extension) &&
					Path.GetFileName(pathToFile) == FlexBridgeConstants.CustomPropertiesFilename;
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
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

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				FlexBridgeConstants.CustomField, Key);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc,
				false); // We don't want (or even need) the custom properties to be added to the MDC, while merging the custom props file itself.
						// We won't even know what they are until after the merge is done.
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.CustomProperties; }
		}

		#endregion
	}
}