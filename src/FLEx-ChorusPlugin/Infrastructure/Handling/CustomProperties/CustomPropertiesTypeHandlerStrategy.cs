using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.CustomProperties
{
	internal sealed class CustomPropertiesTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		private const string CustomField = "CustomField";
		private const string Key = "key";

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.CustomProperties) &&
				   Path.GetFileName(pathToFile) == SharedConstants.CustomPropertiesFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
/*
customPropertyDeclarations.Add(new XElement("CustomField",
	new XAttribute("name", customFieldInfo.m_fieldname),
	new XAttribute("class", customFieldInfo.m_classname),
	new XAttribute("type", GetFlidTypeAsString(customFieldInfo.m_fieldType)),
	(customFieldInfo.m_destinationClass != 0) ? new XAttribute("destclass", customFieldInfo.m_destinationClass.ToString()) : null,
	(customFieldInfo.m_fieldWs != 0) ? new XAttribute("wsSelector", customFieldInfo.m_fieldWs.ToString()) : null,
	(!String.IsNullOrEmpty(customFieldInfo.m_fieldHelp)) ? new XAttribute("helpString", customFieldInfo.m_fieldHelp) : null,
	(customFieldInfo.m_fieldListRoot != Guid.Empty) ? new XAttribute("listRoot", customFieldInfo.m_fieldListRoot.ToString()) : null,
	(customFieldInfo.Label != customFieldInfo.m_fieldname) ? new XAttribute("label", customFieldInfo.Label) : null));
*/
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.AdditionalFieldsTag)
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
				foreach (var customFieldElement in root.Elements(CustomField))
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
				}

				MetadataCache.MdCache.AddCustomPropInfo(pathToFile);

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
				CustomField, Key);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			// NB: Doesn't need the mdc updated with custom props.
			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCustomPropertyMergingStrategy(mergeOrder.MergeSituation),
				null,
				CustomField, Key, WritePreliminaryCustomPropertyInformation);
		}

		public string Extension
		{
			get { return SharedConstants.CustomProperties; }
		}

		#endregion

		private static void WritePreliminaryCustomPropertyInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(SharedConstants.AdditionalFieldsTag);
			reader.Read();
		}
	}
}