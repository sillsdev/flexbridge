using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.Discourse
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
			var discourseDir = Path.Combine(linguisticsBaseDir, SharedConstants.DiscourseRootFolder);
			if (!Directory.Exists(discourseDir))
				Directory.CreateDirectory(discourseDir);

			var sortedInstanceData = classData["DsDiscourseData"];
			if (sortedInstanceData.Count == 0)
				return;

			// 'discourseElement' is owned by LangProj in DsDiscourseData prop (OA).
			var discourseElement = Utilities.CreateFromBytes(sortedInstanceData.Values.First());

			// Nest the entire object, and then pull out the owned stuff, and relocate them, as needed.
			CmObjectNestingService.NestObject(
				false,
				discourseElement,
				classData,
				guidToClassMapping);

			var listElement = discourseElement.Element(SharedConstants.ConstChartTempl);
			if (listElement != null)
			{
				// NB: Write list file, but only if discourseElement has the list.
				FileWriterService.WriteNestedFile(Path.Combine(discourseDir, SharedConstants.ConstChartTemplFilename), listElement);
				listElement.RemoveNodes();
			}

			listElement = discourseElement.Element(SharedConstants.ChartMarkers);
			if (listElement != null)
			{
				// NB: Write list file, but only if discourseElement has the list.
				FileWriterService.WriteNestedFile(Path.Combine(discourseDir, SharedConstants.ChartMarkersFilename), listElement);
				listElement.RemoveNodes();
			}

			// <owning num="2" id="Charts" card="col" sig="DsChart"> [Abstract. Owns nothing special, but is subclass of CmMajorObject.]
			//		Disposition: Write in main discourse file as the repeating series of objects, BUT use the abstract class for the repeating element, since Gordon sees new subclasses of it coming along.

			// NB: We will just let the normal nesting code work over discourseElement,
			// which will put in the actual subclass name as the element tag.
			// So, we'll just intercept them out of the owning prop elemtent (if any exist), and patch them up here.
			// NB: If there are no such charts, then do the usual of making one with the empty guid for its Id.
			var root = new XElement(SharedConstants.DiscourseRootFolder);
			var header = new XElement(SharedConstants.Header);
			root.Add(header);
			header.Add(discourseElement);
			// Remove child objsur node from owning LangProg
			var langProjElement = wellUsedElements[SharedConstants.LangProject];
			langProjElement.Element("DiscourseData").RemoveNodes();

			var chartElements = discourseElement.Element("Charts");
			if (chartElements != null && chartElements.HasElements)
			{
				foreach (var chartElement in chartElements.Elements())
				{
					BaseDomainServices.ReplaceElementNameWithAndAddClassAttribute(SharedConstants.DsChart, chartElement);
					// It is already nested.
					root.Add(chartElement);
				}
				chartElements.RemoveNodes();
			}

			FileWriterService.WriteNestedFile(Path.Combine(discourseDir, SharedConstants.DiscourseChartFilename), root);
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var discourseDir = Path.Combine(linguisticsBaseDir, SharedConstants.DiscourseRootFolder);
			if (!Directory.Exists(discourseDir))
				return;
			var chartPathname = Path.Combine(discourseDir, SharedConstants.DiscourseChartFilename);
			if (!File.Exists(chartPathname))
				return;

			// The charts need to be sorted in guid order, before being added back into the owning prop element.
			var doc = XDocument.Load(chartPathname);
			var root = doc.Root;
			var discourseElement = root.Element(SharedConstants.Header).Element("DsDiscourseData");
			// Add lists back into discourseElement.
			foreach (var listPathname in Directory.GetFiles(discourseDir, "*." + SharedConstants.List))
			{
				var listDoc = XDocument.Load(listPathname);
				var listFilename = Path.GetFileName(listPathname);
				var listElement = listDoc.Root.Element(SharedConstants.CmPossibilityList);
				switch (listFilename)
				{
					case SharedConstants.ChartMarkersFilename:
						discourseElement.Element(SharedConstants.ChartMarkers).Add(listElement);
						break;
					case SharedConstants.ConstChartTemplFilename:
						discourseElement.Element(SharedConstants.ConstChartTempl).Add(listElement);
						break;
				}
			}
			// Add the chart elements (except the possible dummy one) into discourseElement.
			var sortedCharts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var chartElement in root.Elements(SharedConstants.DsChart)
				.Where(element => element.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() != SharedConstants.EmptyGuid))
			{
				// No. Add it to discourseElement, BUT in sorted order, below, and then flatten discourseElement.
				// Restore the right main element name from the class attribute.
				var classAttr = chartElement.Attribute(SharedConstants.Class);
				chartElement.Name = classAttr.Value;
				classAttr.Remove();
				sortedCharts.Add(chartElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant(), chartElement);
			}

			if (sortedCharts.Count > 0)
			{
				var discourseElementOwningProp = discourseElement.Element("Charts")
												 ?? CmObjectFlatteningService.AddNewPropertyElement(discourseElement, "Charts");
				foreach (var sortedChartElement in sortedCharts.Values)
					discourseElementOwningProp.Add(sortedChartElement);
			}
			var langProjElement = highLevelData[SharedConstants.LangProject];
			BaseDomainServices.RestoreElement(
				chartPathname,
				sortedData,
				langProjElement,
				"DiscourseData",
				discourseElement);
		}
	}
}