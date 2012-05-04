using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.General
{
	/// <summary>
	/// This class handles all remaining elements that appear to be unowned.
	///
	/// Presumably, a good DeLint run in a Data Migration would take remove them.
	/// </summary>
	internal class GeneralDomainOrphansBoundedContext
	{
		internal static void NestContext(string generalBaseDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			// I know of two types of data this method needs to handle:
			//	1. Honestly unowned elements
			//	2. Objects that claim to have an owner, and
			//		A. May not be nesting correctly (bug in nesting code, or more likely the MDC), or
			//		B. The owner may not know it owns it (Lint).
			TrimClassData(classData);

			var ownerlessElementGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var lintElementGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var condidatesDict in classData.Values)
			{
				foreach (var candidateElementKvp in condidatesDict)
				{
					var candidateGuid = candidateElementKvp.Key;
					var candidateElement = candidateElementKvp.Value;
					var ownerGuidAttr = candidateElement.Attribute(SharedConstants.OwnerGuid);
					if (ownerGuidAttr == null)
					{
						// Unowned.
						ownerlessElementGuids.Add(candidateGuid);
						sortedData.Add(candidateGuid, candidateElement);
						// Add special attr.
						AddSpecialAttribute(candidateElement, "unowned");
					}
					else
					{
						if (guidToClassMapping.ContainsKey(ownerGuidAttr.Value))
							continue; // It will be nested.

						// Change OwnerGuid attr name to TempOwnerGuid.
						//candidateElement.Attribute(SharedConstants.OwnerGuid).Name.LocalName = SharedConstants.TempOwnerGuid;
						lintElementGuids.Add(candidateGuid);
						sortedData.Add(candidateGuid, candidateElement);
						// Add special attr.
						AddSpecialAttribute(candidateElement, "lint");
					}
				}
			}

			// Now we have hold of the two possible high-level objects.
			// So, nest each, and add them into a new file (new ext: lint)
			// Add some extra bits to make them all have the same main element name, and maybe a new attr that says 'unowned' vs 'lint'.
			// The should be in the usual sorted order.
			var root = new XElement("curiosities");
			foreach (var curiosityElement in sortedData.Values)
			{
				CmObjectNestingService.NestObject(
					false,
					curiosityElement,
					classData,
					guidToClassMapping);
				// Fix up name.
				BaseDomainServices.ReplaceElementNameWithAndAddClassAttribute(SharedConstants.curiosity, curiosityElement);
				root.Add(curiosityElement);

				// Write file.
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.LintFilename), root);
			}

#if DEBUG
			// By this point, there should be nothing left in either classData or guidToClassMapping.
			Debug.Assert(guidToClassMapping.Count == 0, "Found unexpected data in guidToClassMapping Dictionary at end of nesting.");
			TrimClassData(classData);
			Debug.Assert(classData.Count == 0, "Found unexpected data in classData Dictionary at end of nesting.");
#endif
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string generalBaseDir)
		{
			var lintPathname = Path.Combine(generalBaseDir, SharedConstants.LintFilename);
			if (!File.Exists(lintPathname))
				return;

			var doc = XDocument.Load(lintPathname);
			foreach (var curiosityElement in doc.Root.Elements(SharedConstants.curiosity))
			{
				// Remove "curiositytype" attr.
				curiosityElement.Attribute("curiositytype").Remove();

				// Rename element and remove class attr.
				var classAttr = curiosityElement.Attribute(SharedConstants.Class);
				curiosityElement.Name = classAttr.Value;
				classAttr.Remove();

				CmObjectFlatteningService.FlattenObject(
					lintPathname,
					sortedData,
					curiosityElement,
					null); // May, or may not, claim to have owner.

				// Do this *after the flattening, so the flattener won't get confused.
				// If it has the attr SharedConstants.TempOwnerGuid, then rename it to SharedConstants.OwnerGuid
				var tempOwnerAttr = curiosityElement.Attribute(SharedConstants.TempOwnerGuid);
				if (tempOwnerAttr == null)
					continue;

				var ownerGuid = tempOwnerAttr.Value.ToLowerInvariant();
				tempOwnerAttr.Remove();
				curiosityElement.Add(new XAttribute(SharedConstants.OwnerGuid, ownerGuid));
				var sortedAttrs = new SortedDictionary<string, XAttribute>(StringComparer.OrdinalIgnoreCase);
				foreach (var oldAttr in curiosityElement.Attributes())
					sortedAttrs.Add(oldAttr.Name.LocalName, oldAttr);
				curiosityElement.Attributes().Remove();
				curiosityElement.Add(sortedAttrs.Values);
			}
		}

		private static void AddSpecialAttribute(XElement element, string attributeValue)
		{
			var sortedAttrs = new SortedDictionary<string, XAttribute>(StringComparer.OrdinalIgnoreCase);
			element.Add(new XAttribute("curiositytype", attributeValue));
			var ownerGuidAttr = element.Attribute(SharedConstants.OwnerGuid);
			if (ownerGuidAttr != null)
			{
				element.Add(new XAttribute(SharedConstants.TempOwnerGuid, ownerGuidAttr.Value));
			}
			foreach (var attribute in element.Attributes())
			{
				sortedAttrs.Add(attribute.Name.LocalName, attribute);
			}
			element.Attributes().Remove();
			foreach (var sortedAttr in sortedAttrs.Values)
			{
				element.Add(sortedAttr);
			}
		}

		private static void TrimClassData(IDictionary<string, SortedDictionary<string, XElement>> classData)
		{
			var classesWithNoData = new HashSet<string>(
				classData.Select(mainKvp => new { mainKvp, className = mainKvp.Key }).Select(
			@t => new { @t, dataCollection = @t.mainKvp.Value }).Where(@t => @t.dataCollection.Count == 0).Select(
				@t => @t.@t.className));
			foreach (var noDataClassname in classesWithNoData)
			{
				classData.Remove(noDataClassname);
			}
		}
	}
}