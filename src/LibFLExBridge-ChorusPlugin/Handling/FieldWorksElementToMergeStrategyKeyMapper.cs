// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Code;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPlugin.Handling
{
	internal sealed class FieldWorksElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
	{
		private readonly HashSet<string> _oddElementNames = new HashSet<string>
										{
											FlexBridgeConstants.Refcol,
											FlexBridgeConstants.Ownseq,
											// There is no need for an OwnCol, since order in irrelevant for them.
											FlexBridgeConstants.Refseq,
											FlexBridgeConstants.CmAnnotation,
											FlexBridgeConstants.DsChart
										};

		#region Implementation of IElementToMergeStrategyKeyMapper

		string IElementToMergeStrategyKeyMapper.GetKeyFromElement(HashSet<string> keys, XmlNode element)
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
					if (element.Attributes[FlexBridgeConstants.GuidStr] != null && (keys.Contains(key)) || element.ParentNode == null)
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