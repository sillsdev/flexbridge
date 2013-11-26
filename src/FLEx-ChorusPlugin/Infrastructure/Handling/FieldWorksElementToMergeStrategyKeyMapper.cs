// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using Palaso.Code;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal class FieldWorksElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
	{
		private readonly HashSet<string> _oddElementNames = new HashSet<string>
										{
											SharedConstants.Refcol,
											SharedConstants.Ownseq,
											// There is no need for an OwnCol, since order in irrelevant for them.
											SharedConstants.Refseq,
											SharedConstants.CmAnnotation,
											SharedConstants.DsChart
										};

		#region Implementation of IKeyFinder

		public string GetKeyFromElement(HashSet<string> keys, XmlNode element)
		{
			Guard.AgainstNull(keys, "The 'keys' parameter is null.");
			Guard.AgainstNull(element, "The 'element' parameter is null.");

			var key = element.Name;
			switch (key)
			{
				default:
					// Some class names are the same as a property name (e.g., LexDb),
					// so we need to see if 'element' has a guid or not, to know how to fish out the correct key.
					// Property elements never have a guid, and class elements always have one.
					if (element.Attributes[SharedConstants.GuidStr] != null && (keys.Contains(key)) || element.ParentNode == null)
						return key;

					// Not a class, so go for one of the other kludges.
					// Combine parent name + element name as key (for new styled FW properties).
					var combinedKey = _oddElementNames.Contains(element.ParentNode.Name)
						? element.ParentNode.Attributes["class"].Value + "_" + key
						: element.ParentNode.Name + "_" + key;
					if (keys.Contains(combinedKey))
						return combinedKey;
					break;
				// Custom properties.
				case "Custom":
					var customPropName = element.Attributes["name"].Value;
					var className = element.ParentNode.Name;
					if (_oddElementNames.Contains(className))
						className = element.ParentNode.Attributes["class"].Value;
					var combinedCustomKey = key + "_" + className + "_" + customPropName;
					if (keys.Contains(combinedCustomKey))
						return combinedCustomKey;
					break;
			}

			return key;
		}

		#endregion
	}
}