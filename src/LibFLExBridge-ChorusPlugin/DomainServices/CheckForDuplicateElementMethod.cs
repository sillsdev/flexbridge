// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Handling;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using SIL.Providers;

namespace LibFLExBridgeChorusPlugin.DomainServices
{
	/// <summary>
	/// Class that ensures a CmObject is unique (by guid) in the restored fwdata file.
	/// If an object does not exist in the "sortedData" input, then this class just returns the guid of the given "element".
	/// Otherwise, this class changes the guid of 'element' and everything it owns, and then returns the new guid of 'element'.
	/// If the element's guid is already in "sortedData", then this class will add a new IncompatibleMoveConflict conflict report.
	/// </summary>
	internal static class CheckForDuplicateElementMethod
	{
		/// <summary>
		/// The only entry point in the class. This method (plus its private methods) does the work of the class.
		/// </summary>
		/// <param name="pathname">Pathname of the split up file that contains <paramref name="element"/>.</param>
		/// <param name="sortedData">All of the data being collected that will end up as CmObject elments in the rebuilt fwdata file. These are sorted by guid order.</param>
		/// <param name="element">The xml element to check for a duplicate guid.</param>
		/// <param name="isOwnSeqNode">Out paramenter that is used by caller.</param>
		/// <param name="className">The class of <paramref name="element"/>.</param>
		/// <returns></returns>
		internal static string CheckForDuplicateGuid(string pathname, SortedDictionary<string, XElement> sortedData, XElement element, out bool isOwnSeqNode, out string className)
		{
			var mdc = MetadataCache.MdCache;
			FdoClassInfo classInfo;
			isOwnSeqNode = GetClassInfoFromElement(mdc, element, out classInfo, out className);
			var elementGuid = element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();

			if (!sortedData.ContainsKey(elementGuid))
				return elementGuid;

			// Does LT-12524 "Handle merge in case of conflicting move object to different destination".
			// This need will manifest itself in the guid already being in 'sortedData' and an exception being thrown.
			// At this point element has not been flattened, so stuff it owns will still be in it.
			// That is good, if we go with JohnT's idea of using a new guid for guids that are already in 'sortedData'.
			// By changing it before flattening, then the owned stuff will get the new one for their ownerguid attrs.
			// The owned stuff will also be dup, so the idea is to also change their guids right now. [ChangeGuids is recursive down the owning tree.]
			// Just be sure to change 'elementGuid' to the new one. :-)
			// The first item added to sortedData has been flattened by this point, but not any following ones.
			var oldGuid = elementGuid;
			elementGuid = ChangeGuids(mdc, classInfo, element);
			using (var listener = new ChorusNotesMergeEventListener(ChorusNotesMergeEventListener.GetChorusNotesFilePath(pathname)))
			{
				// Don't try to use something like this:
				// var contextGenerator = new FieldWorkObjectContextGenerator();
				// contextGenerator.GenerateContextDescriptor(element.ToString(), pathname)
				// it will fail for elements in an owning sequence because in the unflattened form
				// the object representing a sequence item has element name <ownseq> which won't generate a useful label.
				var context = FieldWorksMergeServices.GenerateContextDescriptor(pathname, elementGuid, className);
				listener.EnteringContext(context);
				// Adding the conflict to the listener, will result in the ChorusNotes file being updated (created if need be.)
				var conflict = new IncompatibleMoveConflict(className, CmObjectFlatteningService.GetXmlNode(element)) { Situation = new NullMergeSituation() };
				// The order of the next three lines is critical. Each prepares state that the following lines use.
				listener.RecordContextInConflict(conflict);
				conflict.HtmlDetails = MakeHtmlForIncompatibleMove(conflict, oldGuid, elementGuid, element);
				listener.ConflictOccurred(conflict);
				File.WriteAllText(pathname + "." + LibTriboroughBridgeSharedConstants.dupid, "");
			}
			return elementGuid;
		}

		private static string MakeHtmlForIncompatibleMove(IConflict conflict, string oldGuid, string elementGuid, XElement element)
		{
			var doc = new XmlDocument();
			doc.LoadXml(element.ToString());
			var sb = new StringBuilder("<head><style type='text/css'>");
			sb.Append(FieldWorkObjectContextGenerator.DefaultHtmlContextStyles(doc.DocumentElement));
			sb.Append("</style></head><body><div class='description'>");
			sb.Append(conflict.GetFullHumanReadableDescription());
			string className = element.Name.LocalName;
			var classAttr = element.Attribute("class");
			if (classAttr != null)
				className = classAttr.Value;
			sb.Append(String.Format("</div><div> The object that was copied is a {0}:", className));
			sb.Append("</div><div class=\"description\">");
			sb.Append(new FwGenericHtmlGenerator().MakeHtml(doc.DocumentElement));
			sb.Append("</div><div>The original is ");
			MakeSilfwLink(oldGuid, sb);
			sb.Append("</div><div>The copy is ");
			MakeSilfwLink(elementGuid, sb);
			sb.Append("</div></body>");
			return sb.ToString();
		}

		private static void MakeSilfwLink(string guid, StringBuilder sb)
		{
			sb.Append("<a href=\"silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=");
			sb.Append(guid);
			sb.Append("&amp;tag=\">here</a>");
		}

		private static bool GetClassInfoFromElement(MetadataCache mdc, XElement element, out FdoClassInfo classInfo,
													out string className)
		{
			var isOwnSeqNode = element.Name.LocalName == FlexBridgeConstants.Ownseq;
			className = isOwnSeqNode ? element.Attribute(FlexBridgeConstants.Class).Value : element.Name.LocalName;
			classInfo = mdc.GetClassInfo(className);
			return isOwnSeqNode;
		}

		private static string ChangeGuids(MetadataCache mdc, FdoClassInfo classInfo, XElement element)
		{
			var newGuid = GuidProvider.Current.NewGuid().ToString().ToLowerInvariant();

			element.Attribute(FlexBridgeConstants.GuidStr).Value = newGuid;

			// Recurse down through everything that is owned and change those guids.
			foreach (var owningPropInfo in classInfo.AllOwningProperties)
			{
				var isCustomProp = owningPropInfo.IsCustomProperty;
				var owningPropElement = isCustomProp
											? (element.Elements(FlexBridgeConstants.Custom).Where(customProp => customProp.Attribute(FlexBridgeConstants.Name).Value == owningPropInfo.PropertyName)).FirstOrDefault()
											: element.Element(owningPropInfo.PropertyName);
				if (owningPropElement == null || !owningPropElement.HasElements)
					continue;
				foreach (var ownedElement in owningPropElement.Elements())
				{
					FdoClassInfo ownedClassInfo;
					string className;
					GetClassInfoFromElement(mdc, element, out ownedClassInfo, out className);
					ChangeGuids(mdc, ownedClassInfo, ownedElement);
				}
			}

			return newGuid;
		}
	}
}
