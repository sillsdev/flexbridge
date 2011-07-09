using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FieldWorksBridge.Infrastructure
{
	internal static class ObjectFinderServices
	{
		internal static void CollectPossibilities(
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput,
			XContainer ownerElement)
		{
			CollectObjects(ownerElement, "Possibilities", "SubPossibilities", classData, guidToClassMapping, multiClassOutput);
			CollectStText(classData, guidToClassMapping, ownerElement, "Discussion", multiClassOutput);
		}

		internal static void CollectStText(IDictionary<string, SortedDictionary<string, byte[]>> classData, IDictionary<string, string> guidToClassMapping, XContainer ownerElement, string propertyName, IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput)
		{
			var guids = GetGuids(ownerElement, propertyName);
			if (guids.Count == 0)
				return;

			// StText, StFootnote, or StJournalText are possible concrete classes of StText.
			var stTextGuid = guids[0];
			var className = guidToClassMapping[stTextGuid];
			var currentSortedDictionary = classData[className];
			var currentBytes = currentSortedDictionary[stTextGuid];
			currentSortedDictionary.Remove(stTextGuid);
			SortedDictionary<string, byte[]> output;
			if (!multiClassOutput.TryGetValue(className, out output))
			{
				output = new SortedDictionary<string, byte[]>();
				multiClassOutput.Add(className, output);
			}
			output.Add(stTextGuid, currentBytes);

			// Drill down on StText.
			// StText instances own:
			//	<owning num="1" id="Paragraphs" card="seq" sig="StPara"/> -- StPara is abstract
			var currentElement = XElement.Parse(MultipleFileServices.Utf8.GetString(currentBytes));
			guids = GetGuids(currentElement, "Paragraphs");
			if (guids.Count > 0)
			{
				foreach (var guid in guids)
				{
					className = guidToClassMapping[guid];
					currentSortedDictionary = classData[className];
					currentBytes = currentSortedDictionary[stTextGuid];
					currentSortedDictionary.Remove(stTextGuid);
					if (!multiClassOutput.TryGetValue(className, out output))
					{
						output = new SortedDictionary<string, byte[]>();
						multiClassOutput.Add(className, output);
					}
					output.Add(guid, currentBytes);
					var currentPara = XElement.Parse(MultipleFileServices.Utf8.GetString(currentBytes));
				}
				//	StTxtPara(StPara) (or possibly ScrTxtPara, but ScrTxtPara adds no new owning properties) owns:
				//		<owning num="6" id="Segments" sig="Segment" card="seq"/>
				//		Segment owns:
				//			<owning num="4" id="Notes" sig="Note" card="seq"/> (Owns nothing.)
				//		<owning num="8" id="Translations" card="col" sig="CmTranslation">
				//		CmTranslation owns:  (Owns nothing.)
				//		<owning num="5" id="AnalyzedTextObjects" card="seq" sig="CmObject"> - NOT in this BC, so don't store here. (Don't really know the BC here, so add them in.)
			}
			//	<owning num="3" id="Tags" sig="TextTag" card="col"/> - NOT in this BC, so don't store here. (Don't really know the BC here, so add them in.)
			guids = GetGuids(currentElement, "Tags");
			if (guids.Count == 0)
				return;

			// TODO: Process "Tags" property data.
		}

		internal static void CollectReversalEntries(IDictionary<string, byte[]> inputEntries, IDictionary<string, byte[]> outputEntries, XContainer ownerElement)
		{
			CollectUniformObjects(ownerElement,
				"Entries", "Subentries",
				inputEntries, outputEntries);
		}

		private static void CollectUniformObjects(XContainer ownerElement, string propertyName, string subPropertyName, IDictionary<string, byte[]> inputEntries, IDictionary<string, byte[]> outputEntries)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value.ToLowerInvariant()))
			{
				var bytes = inputEntries[guid];
				inputEntries.Remove(guid);
				outputEntries.Add(guid, bytes);
				// Recurse to the end, where subPropertyName is the nexted owning property name.
				CollectUniformObjects(
					XElement.Parse(MultipleFileServices.Utf8.GetString(bytes)),
					subPropertyName, subPropertyName,
					inputEntries, outputEntries);
			}
// ReSharper restore PossibleNullReferenceException
		}

		private static void CollectObjects(
			XContainer ownerElement,
			string propertyName, string subProperty,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

			IDictionary<string, byte[]> inputData = null;
// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value.ToLowerInvariant()))
			{
				var classname = guidToClassMapping[guid];
				if (inputData == null)
					inputData = classData[classname];
				CollectObjects(guid, subProperty, inputData, guidToClassMapping[guid], guidToClassMapping, multiClassOutput);
			}
// ReSharper restore PossibleNullReferenceException
		}

		private static void CollectObjects(
			XElement ownerElement,
			string propertyName, string subProperty,
			IDictionary<string, byte[]> inputData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

// ReSharper disable PossibleNullReferenceException
			var classname = ownerElement.Attribute("class").Value;
// ReSharper restore PossibleNullReferenceException
			SortedDictionary<string, byte[]> output;
			if (!multiClassOutput.TryGetValue(classname, out output))
			{
				output = new SortedDictionary<string, byte[]>();
				multiClassOutput.Add(classname, output);
			}

// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value.ToLowerInvariant()))
				CollectObjects(guid, subProperty, inputData, guidToClassMapping[guid], guidToClassMapping, multiClassOutput);
// ReSharper restore PossibleNullReferenceException
		}

		private static void CollectObjects(
			string guid, string subProperty,
			IDictionary<string, byte[]> inputData,
			string classname,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput)
		{
			var bytes = inputData[guid];
			inputData.Remove(guid);
			SortedDictionary<string, byte[]> output;
			if (!multiClassOutput.TryGetValue(classname, out output))
			{
				output = new SortedDictionary<string, byte[]>();
				multiClassOutput.Add(classname, output);
			}
			output.Add(guid, bytes);

			CollectObjects(
				XElement.Parse(MultipleFileServices.Utf8.GetString(bytes)),
				subProperty, subProperty,
				inputData,
				guidToClassMapping,
				multiClassOutput);
		}

		private static List<string> GetGuids(XContainer textElement, string propertyName)
		{
			var propElement = textElement.Element(propertyName);

			return (propElement == null) ? new List<string>() : (from osEl in propElement.Elements("objsur")
																 select osEl.Attribute("guid").Value.ToLowerInvariant()).ToList();
		}
	}
}