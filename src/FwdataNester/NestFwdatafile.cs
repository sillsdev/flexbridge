using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.Xml;

namespace FwdataNester
{
	public partial class NestFwdataFile : Form
	{
		internal static readonly Encoding Utf8 = Encoding.UTF8;

		public NestFwdataFile()
		{
			InitializeComponent();
		}

		private void BrowseForFile(object sender, EventArgs e)
		{
			_btnNest.Enabled = false;
			_fwdataPathname.Text = null;

			if (_openFileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			_fwdataPathname.Text = _openFileDialog.FileName;
			_btnNest.Enabled = true;
		}

		private void NestFile(object sender, EventArgs e)
		{
			var mdc = MetadataCache.MdCache;
			var interestingPropertiesCache = DataSortingService.CacheInterestingProperties(mdc);

			// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var unownedObjects = new List<XElement>(10000);
			var classData = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
			var guidToClassMapping = new Dictionary<string, string>();
			byte[] optionalFirstElement = null;
			using (var fastSplitter = new FastXmlElementSplitter(_openFileDialog.FileName))
			{
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementBytes("AdditionalFields", "rt", out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// Cache custom prop file for later write.
						var cpElement = DataSortingService.SortCustomPropertiesRecord(Utf8.GetString(record));
						// Add custom property info to MDC, since it may need to be sorted in the data files.
						foreach (var propElement in cpElement.Elements("CustomField"))
						{
							var className = propElement.Attribute("class").Value;
							var propName = propElement.Attribute("name").Value;
							var typeAttr = propElement.Attribute("type");
							var adjustedTypeValue = AdjustedPropertyType(interestingPropertiesCache, className, propName, typeAttr.Value);
							if (adjustedTypeValue != typeAttr.Value)
								typeAttr.Value = adjustedTypeValue;
							var customProp = new FdoPropertyInfo(
								propName,
								typeAttr.Value,
								true);
							// TODO: TLP has a custom prop that has an abstract class, and its className is *not* in interestingPropertiesCache.
							DataSortingService.CacheProperty(interestingPropertiesCache[className], customProp);
							mdc.AddCustomPropInfo(
								className,
								customProp);
						}
						optionalFirstElement = Utf8.GetBytes(cpElement.ToString());
						foundOptionalFirstElement = false;
					}
					else
					{
						unownedObjects.AddRange(CacheDataRecord(interestingPropertiesCache, classData, guidToClassMapping, record));
					}
				}
			}

			var root = new XElement("root");
			var nestedDoc = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				root);
			foreach (var unownedElement in unownedObjects)
			{
				CmObjectNestingService.NestObject(unownedElement,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);
				root.Add(unownedElement);
			}
			nestedDoc.Save(_openFileDialog.FileName + ".nested");
		}

		private static string AdjustedPropertyType(IDictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache, string className, string propName, string rawType)
		{
			string adjustedType;
			switch (rawType)
			{
				default:
					adjustedType = rawType;
					break;

				case "OC":
					adjustedType = "OwningCollection";
					AddCollectionPropertyToCache(sortablePropertiesCache, className, propName);
					break;
				case "RC":
					adjustedType = "ReferenceCollection";
					AddCollectionPropertyToCache(sortablePropertiesCache, className, propName);
					break;

				case "OS":
					adjustedType = "OwningSequence";
					break;

				case "RS":
					adjustedType = "ReferenceSequence";
					break;

				case "OA":
					adjustedType = "OwningAtomic";
					break;

				case "RA":
					adjustedType = "ReferenceAtomic";
					break;
			}
			return adjustedType;
		}

		private static void AddCollectionPropertyToCache(IDictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache, string className, string propName)
		{
			Dictionary<string, HashSet<string>> classProps;
			if (!sortablePropertiesCache.TryGetValue(className, out classProps))
			{
				classProps = new Dictionary<string, HashSet<string>>(2)
								{
									{DataSortingService.Collections, new HashSet<string>()},
									{DataSortingService.MultiAlt, new HashSet<string>()}
								};
				sortablePropertiesCache.Add(className, classProps);
			}
			var collProps = classProps[DataSortingService.Collections];
			collProps.Add(propName);
		}

		private static IEnumerable<XElement> CacheDataRecord(Dictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache, IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping, byte[] record)
		{
			var returnValue = new List<XElement>();
			var rtElement = XElement.Parse(Utf8.GetString(record));
			if (rtElement.Attribute("ownerguid") == null)
				returnValue.Add(rtElement);
			var className = rtElement.Attribute("class").Value;
			var guid = rtElement.Attribute("guid").Value;
			guidToClassMapping.Add(guid.ToLowerInvariant(), className);

			// 1. Remove 'Checksum' from wordforms.
			if (className == "WfiWordform")
			{
				var csElement = rtElement.Element("Checksum");
				if (csElement != null)
					csElement.Remove();
			}

			// 2. Sort <rt>
			DataSortingService.SortMainElement(sortablePropertiesCache, rtElement);

			// 3. Cache it.
			SortedDictionary<string, XElement> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			recordData.Add(guid, rtElement);

			return returnValue;
		}
	}
}
