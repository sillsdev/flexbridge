// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Infrastructure.RoundTrip
{
	/// <summary>
	/// Semantic comparisons of two .fwdata documents. Objects are matched by guid, so the order of
	/// &lt;rt&gt; elements, the order of properties within an object, and whitespace do not matter.
	/// </summary>
	internal static class FwdataAssert
	{
		/// <summary>Both documents hold the same objects: same guids, classes and owners.</summary>
		internal static void SameObjects(XDocument expected, XDocument actual)
		{
			Assert.That(ObjectSummaries(actual), Is.EqualTo(ObjectSummaries(expected)), "objects (guid, class, owner)");
		}

		/// <summary>Both documents declare the same custom fields with the same attributes.</summary>
		internal static void SameCustomFields(XDocument expected, XDocument actual)
		{
			Assert.That(CustomFieldSummaries(actual), Is.EqualTo(CustomFieldSummaries(expected)), "custom fields");
		}

		/// <summary>
		/// Both documents hold the same objects, each with the same properties and property contents.
		/// </summary>
		internal static void SameObjectContents(XDocument expected, XDocument actual)
		{
			SameObjects(expected, actual);
			var actualObjects = SplitUnifyHarness.ObjectsByGuid(actual);
			foreach (var expectedObject in SplitUnifyHarness.ObjectsByGuid(expected))
			{
				Assert.That(Normalize(actualObjects[expectedObject.Key]).ToString(),
					Is.EqualTo(Normalize(expectedObject.Value).ToString()),
					"contents of " + expectedObject.Key);
			}
		}

		private static List<string> ObjectSummaries(XDocument fwdata)
		{
			return SplitUnifyHarness.ObjectsByGuid(fwdata)
				.Select(pair => string.Format("{0} {1} owner={2}",
					pair.Key,
					pair.Value.Attribute(FlexBridgeConstants.Class).Value,
					((string)pair.Value.Attribute(FlexBridgeConstants.OwnerGuid) ?? "none").ToLowerInvariant()))
				.OrderBy(summary => summary)
				.ToList();
		}

		private static List<string> CustomFieldSummaries(XDocument fwdata)
		{
			var additionalFields = fwdata.Root.Element(FlexBridgeConstants.AdditionalFieldsTag);
			if (additionalFields == null)
				return new List<string>();
			return additionalFields.Elements(FlexBridgeConstants.CustomField)
				.Select(cf => string.Join(" ", cf.Attributes()
					.OrderBy(attr => attr.Name.LocalName)
					.Select(attr => attr.Name.LocalName + "=" + attr.Value)))
				.OrderBy(summary => summary)
				.ToList();
		}

		/// <summary>A copy of an element with attributes and child elements sorted and whitespace-only text dropped.</summary>
		private static XElement Normalize(XElement element)
		{
			var copy = new XElement(element.Name,
				element.Attributes().OrderBy(attr => attr.Name.LocalName).Select(attr => new XAttribute(attr)));
			if (element.HasElements)
			{
				// Owning and reference sequences keep their order: only the order of an object's properties is free.
				var children = element.Name.LocalName == FlexBridgeConstants.RtTag
					? element.Elements().OrderBy(child => child.Name.LocalName).ThenBy(child => (string)child.Attribute(FlexBridgeConstants.Name))
					: element.Elements();
				copy.Add(children.Select(Normalize));
			}
			else if (!string.IsNullOrWhiteSpace(element.Value))
			{
				copy.Add(new XText(element.Value));
			}
			return copy;
		}
	}
}
