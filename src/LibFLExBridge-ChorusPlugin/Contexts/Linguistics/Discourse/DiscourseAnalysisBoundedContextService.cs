// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics.Discourse
{
	/// <summary>
	/// Read/Write the Discourse Analysis Bounded Context.
	///
	/// This will be the DsDiscourseData instance and all it owns.
	/// </summary>
	internal static class DiscourseAnalysisBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var discourseDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.DiscourseRootFolder);
			if (!Directory.Exists(discourseDir))
				Directory.CreateDirectory(discourseDir);

			var sortedInstanceData = classData["DsDiscourseData"];
			if (sortedInstanceData.Count == 0)
				return;

			// 'discourseElement' is owned by LangProj in DsDiscourseData prop (OA).
			var discourseElement = LibFLExBridgeUtilities.CreateFromBytes(sortedInstanceData.Values.First());

			// Nest the entire object, and then pull out the owned stuff, and relocate them, as needed.
			CmObjectNestingService.NestObject(
				false,
				discourseElement,
				classData,
				guidToClassMapping);

			var listElement = discourseElement.Element(FlexBridgeConstants.ConstChartTempl);
			if (listElement != null)
			{
				// NB: Write list file, but only if discourseElement has the list.
				FileWriterService.WriteNestedFile(Path.Combine(discourseDir, FlexBridgeConstants.ConstChartTemplFilename), listElement);
				listElement.RemoveNodes();
			}

			listElement = discourseElement.Element(FlexBridgeConstants.ChartMarkers);
			if (listElement != null)
			{
				// NB: Write list file, but only if discourseElement has the list.
				FileWriterService.WriteNestedFile(Path.Combine(discourseDir, FlexBridgeConstants.ChartMarkersFilename), listElement);
				listElement.RemoveNodes();
			}

			// <owning num="2" id="Charts" card="col" sig="DsChart"> [Abstract. Owns nothing special, but is subclass of CmMajorObject.]
			//		Disposition: Write in main discourse file as the repeating series of objects, BUT use the abstract class for the repeating element, since Gordon sees new subclasses of it coming along.

			// NB: We will just let the normal nesting code work over discourseElement,
			// which will put in the actual subclass name as the element tag.
			// So, we'll just intercept them out of the owning prop elemtent (if any exist), and patch them up here.
			// NB: If there are no such charts, then do the usual of making one with the empty guid for its Id.
			var root = new XElement(FlexBridgeConstants.DiscourseRootFolder);
			var header = new XElement(FlexBridgeConstants.Header);
			root.Add(header);
			header.Add(discourseElement);
			// Remove child objsur node from owning LangProg
			var langProjElement = wellUsedElements[FlexBridgeConstants.LangProject];
			langProjElement.Element("DiscourseData").RemoveNodes();

			var chartElements = discourseElement.Element("Charts");
			if (chartElements != null && chartElements.HasElements)
			{
				foreach (var chartElement in chartElements.Elements())
				{
					BaseDomainServices.ReplaceElementNameWithAndAddClassAttribute(FlexBridgeConstants.DsChart, chartElement);
					// It is already nested.
					root.Add(chartElement);
				}
				chartElements.RemoveNodes();
			}

			FileWriterService.WriteNestedFile(Path.Combine(discourseDir, FlexBridgeConstants.DiscourseChartFilename), root);
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var discourseDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.DiscourseRootFolder);
			if (!Directory.Exists(discourseDir))
				return;
			var chartPathname = Path.Combine(discourseDir, FlexBridgeConstants.DiscourseChartFilename);
			if (!File.Exists(chartPathname))
				return;

			// The charts need to be sorted in guid order, before being added back into the owning prop element.
			var doc = XDocument.Load(chartPathname);
			var root = doc.Root;
			var discourseElement = root.Element(FlexBridgeConstants.Header).Element("DsDiscourseData");
			// Add lists back into discourseElement.
			foreach (var listPathname in Directory.GetFiles(discourseDir, "*." + FlexBridgeConstants.List))
			{
				var listDoc = XDocument.Load(listPathname);
				var listFilename = Path.GetFileName(listPathname);
				var listElement = listDoc.Root.Element(FlexBridgeConstants.CmPossibilityList);
				switch (listFilename)
				{
					case FlexBridgeConstants.ChartMarkersFilename:
						discourseElement.Element(FlexBridgeConstants.ChartMarkers).Add(listElement);
						break;
					case FlexBridgeConstants.ConstChartTemplFilename:
						discourseElement.Element(FlexBridgeConstants.ConstChartTempl).Add(listElement);
						break;
				}
			}
			// Add the chart elements (except the possible dummy one) into discourseElement.
			var sortedCharts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var chartElement in root.Elements(FlexBridgeConstants.DsChart)
				.Where(element => element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() != FlexBridgeConstants.EmptyGuid))
			{
				// No. Add it to discourseElement, BUT in sorted order, below, and then flatten discourseElement.
				// Restore the right main element name from the class attribute.
				var classAttr = chartElement.Attribute(FlexBridgeConstants.Class);
				chartElement.Name = classAttr.Value;
				classAttr.Remove();
				sortedCharts.Add(chartElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(), chartElement);
			}

			if (sortedCharts.Count > 0)
			{
				var discourseElementOwningProp = discourseElement.Element("Charts")
												 ?? CmObjectFlatteningService.AddNewPropertyElement(discourseElement, "Charts");
				foreach (var sortedChartElement in sortedCharts.Values)
					discourseElementOwningProp.Add(sortedChartElement);
			}
			var langProjElement = highLevelData[FlexBridgeConstants.LangProject];
			BaseDomainServices.RestoreElement(
				chartPathname,
				sortedData,
				langProjElement,
				"DiscourseData",
				discourseElement);
		}
	}
}