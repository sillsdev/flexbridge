//#define TRYUSINGMAINMERGER
using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Xml;
//using System.Xml.Linq;
//using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Services class used by FieldWorksMergingStrategy to create ElementStrategy instances
	/// (some shared and some not shared).
	/// </summary>
	internal static class FieldWorksMergingServices
	{
		internal static void PreMerge(bool isNewStyle, IMergeEventListener eventListener, MergeSituation mergeSituation, MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			// DONE. 1. Deal with DateStamps
			MergeTimestamps(ourEntry, theirEntry);
		}

		internal static void MergeTimestamps(XmlNode ourEntry, XmlNode theirEntry)
		{
			if (ourEntry == null || theirEntry == null)
				return;

			const string xpath = "DateModified | DateResolved | RunDate";
			var ourDateTimeNodes = ourEntry.SelectNodes(xpath);
			var theirDateTimeNodes = theirEntry.SelectNodes(xpath);
			if ((ourDateTimeNodes == null || ourDateTimeNodes.Count == 0) &&
				(theirDateTimeNodes == null || theirDateTimeNodes.Count == 0))
				return;

			for (var i = 0; i < ourDateTimeNodes.Count; ++i)
			{
				var ourNode = ourDateTimeNodes[i];
				var asUtcOurs = GetTimestamp(ourNode);

				var theirNode = theirDateTimeNodes[i];
				var asUtcTheirs = GetTimestamp(theirNode);

				if (asUtcOurs == asUtcTheirs)
					return;

				if (asUtcOurs > asUtcTheirs)
					theirNode.Attributes["val"].Value = ourNode.Attributes["val"].Value;
				else
					ourNode.Attributes["val"].Value = theirNode.Attributes["val"].Value;
			}
		}

		private static DateTime GetTimestamp(XmlNode node)
		{
			var timestamp = node.Attributes["val"].Value;
			var dateParts = timestamp.Split(new[] { '-', ' ', ':', '.' });
			return new DateTime(
				Int32.Parse(dateParts[0]),
				Int32.Parse(dateParts[1]),
				Int32.Parse(dateParts[2]),
				Int32.Parse(dateParts[3]),
				Int32.Parse(dateParts[4]),
				Int32.Parse(dateParts[5]),
				Int32.Parse(dateParts[6]));
		}

		//private static XmlNode GetPropertyNode(XmlNode parentNode, string propertyName)
		//{
		//    return (parentNode == null) ? null : parentNode.SelectSingleNode(propertyName);
		//}

		//private static void CreateObjsurNode(XmlDocument srcDoc, string newValue, string propType, XmlNode srcPropNode)
		//{
		//    var srcObjsurNode = srcDoc.CreateNode(XmlNodeType.Element, SharedConstants.Objsur, null);
		//    srcPropNode.AppendChild(srcObjsurNode);
		//    var srcGuidAttrNode = srcDoc.CreateAttribute(SharedConstants.GuidStr);
		//    srcGuidAttrNode.Value = newValue;
		//    srcObjsurNode.Attributes.Append(srcGuidAttrNode);
		//    var srcPropTypeAttrNode = srcDoc.CreateAttribute("t");
		//    srcPropTypeAttrNode.Value = propType;
		//    srcObjsurNode.Attributes.Append(srcPropTypeAttrNode);
		//}

		//private static void PreMergeReferenceCollectionProperties(IMergeEventListener eventListener, FdoClassInfo classInfo, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		//{
		//    // New and old styles only have objsur elements for ref props.
		//    foreach (var refCollProp in classInfo.AllProperties.Where(prop => prop.DataType == DataType.ReferenceCollection))
		//    {
		//        var propName = refCollProp.PropertyName;
		//        var commonValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		//        var commonPropNode = GetPropertyNode(commonEntry, propName);
		//        if (commonPropNode != null)
		//        {
		//            var guids = from XmlNode objsurNode in commonPropNode.SafeSelectNodes(SharedConstants.Objsur)
		//                        select objsurNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();

		//            commonValues.UnionWith(guids);
		//        }
		//        var ourValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		//        var ourPropNode = GetPropertyNode(ourEntry, propName);
		//        if (ourPropNode != null)
		//        {
		//            var guids = from XmlNode objsurNode in ourPropNode.SafeSelectNodes(SharedConstants.Objsur)
		//                        select objsurNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();

		//            ourValues.UnionWith(guids);
		//            if (!commonValues.SetEquals(ourValues))
		//            {
		//                eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, ourEntry));
		//            }
		//        }
		//        var theirValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		//        var theirPropNode = GetPropertyNode(theirEntry, propName);
		//        if (theirPropNode != null)
		//        {
		//            var guids = from XmlNode objsurNode in theirPropNode.SafeSelectNodes(SharedConstants.Objsur)
		//                        select objsurNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();

		//            theirValues.UnionWith(guids);
		//            if (!commonValues.SetEquals(theirValues))
		//            {
		//                eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, theirEntry));
		//            }
		//        }

		//        // 0. If ours and theirs are the same, there is no conflict.
		//        if (ourValues.SetEquals(theirValues))
		//            continue; // NB: The merger will be told the prop is immutable, so it will not notice if the order is different.

		//        // 1. Keep ones that are in all three. (Excludes removed items.)
		//        var mergedCollection = new HashSet<string>(commonValues, StringComparer.OrdinalIgnoreCase);
		//        mergedCollection.IntersectWith(ourValues);
		//        mergedCollection.IntersectWith(theirValues);

		//        // 2. Re-add ones that were removed by one, but kept by the other.
		//        mergedCollection.UnionWith(commonValues.Intersect(ourValues));
		//        mergedCollection.UnionWith(commonValues.Intersect(theirValues));

		//        // 3. Add ones that either added.
		//        var ourAdditions = ourValues.Except(commonValues);
		//        mergedCollection.UnionWith(ourAdditions);
		//        var theirAdditions = theirValues.Except(commonValues);
		//        mergedCollection.UnionWith(theirAdditions);

		//        // 4. Update ours and theirs to the new collection.
		//        if (mergedCollection.Count == 0)
		//        {
		//            // Remove prop node from both.
		//            var gonerNode = GetPropertyNode(ourEntry, propName);
		//            if (gonerNode != null)
		//                gonerNode.ParentNode.RemoveChild(gonerNode);
		//            gonerNode = GetPropertyNode(theirEntry, propName);
		//            if (gonerNode != null)
		//                gonerNode.ParentNode.RemoveChild(gonerNode);
		//        }
		//        else
		//        {
		//            var ourDoc = ourEntry.OwnerDocument;
		//            var theirDoc = theirEntry.OwnerDocument;
		//            if (ourPropNode == null)
		//            {
		//                ourPropNode = ourDoc.CreateNode(XmlNodeType.Element, propName, null);
		//                ourEntry.AppendChild(ourPropNode);
		//            }
		//            else
		//            {
		//                ourPropNode.RemoveAll();
		//            }
		//            if (theirPropNode == null)
		//            {
		//                theirPropNode = theirDoc.CreateNode(XmlNodeType.Element, propName, null);
		//                theirEntry.AppendChild(theirPropNode);
		//            }
		//            else
		//            {
		//                theirPropNode.RemoveAll();
		//            }
		//            const string propType = "r";
		//            foreach (var newValue in mergedCollection)
		//            {
		//                // Add it to ours and theirs.
		//                CreateObjsurNode(ourDoc, newValue, propType, ourPropNode);
		//                CreateObjsurNode(theirDoc, newValue, propType, theirPropNode);
		//            }
		//        }
		//    }
		//}

		//private static void PreMergeReferenceSequenceProperties(IMergeEventListener eventListener, MetadataCache mdc, FdoClassInfo classInfo, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		//{
		//    // New and old styles only have objsur elements for ref props.
		//    foreach (var refSeqProp in classInfo.AllProperties.Where(prop => prop.DataType == DataType.ReferenceSequence))
		//    {
		//        // TODO: Make this do something. If it does, then look at code that sets up strategies for this type of prop.
		//    }
		//}

		//private static void PreMergeOwningCollectionProperties(bool isNewStyle, IMergeEventListener eventListener, MergeSituation mergeSituation, MetadataCache mdc, FdoClassInfo classInfo, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		//{
		//    foreach (var owningCollectionProperty in classInfo.AllProperties.Where(prop => prop.DataType == DataType.OwningCollection))
		//    {
		//        var propName = owningCollectionProperty.PropertyName;
		//        var commonValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		//        var commonPropNode = GetPropertyNode(commonEntry, propName);
		//        if (commonPropNode != null)
		//        {
		//            var guids = from XmlNode childNode in commonPropNode.ChildNodes
		//                        select childNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();
		//            commonValues.UnionWith(guids);
		//        }
		//        var ourValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		//        var ourPropNode = GetPropertyNode(ourEntry, propName);
		//        if (ourPropNode != null)
		//        {
		//            var guids = from XmlNode childNode in ourPropNode.ChildNodes
		//                        select childNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();
		//            ourValues.UnionWith(guids);
		//            if (!commonValues.SetEquals(ourValues))
		//            {
		//                eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, ourEntry));
		//            }
		//        }
		//        var theirValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		//        var theirPropNode = GetPropertyNode(theirEntry, propName);
		//        if (theirPropNode != null)
		//        {
		//            var guids = from XmlNode childNode in theirPropNode.ChildNodes
		//                        select childNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();
		//            theirValues.UnionWith(guids);
		//            if (!commonValues.SetEquals(theirValues))
		//            {
		//                eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, theirEntry));
		//            }
		//        }

		//        if (isNewStyle)
		//        {
		//            // 1. Pre-merge objects that are in ours and theirs (may not be in common, however).
		//            PreMerge(eventListener, mergeSituation, mdc, commonPropNode, ourPropNode, theirPropNode, theirValues);
		//        }

		//        // 2. If ours and theirs are the same, there is no conflict.
		//        if (ourValues.SetEquals(theirValues))
		//            continue; // NB: The merger will not care if the order is different, but it will need to plow on down and merge the nested objects.

		//        // 3. Keep ones that are in all three. (Excludes removed items.)
		//        var mergedCollection = new HashSet<string>(commonValues, StringComparer.OrdinalIgnoreCase);
		//        mergedCollection.IntersectWith(ourValues);
		//        mergedCollection.IntersectWith(theirValues);

		//        // 4. Add ones that either added.
		//        var ourAdditions = ourValues.Except(commonValues);
		//        mergedCollection.UnionWith(ourAdditions);
		//        var theirAdditions = theirValues.Except(commonValues);
		//        mergedCollection.UnionWith(theirAdditions);

		//        // 5. Update ours and theirs to the new collection.
		//        if (mergedCollection.Count == 0)
		//        {
		//            // Remove prop node from both.
		//            var gonerNode = GetPropertyNode(ourEntry, propName);
		//            if (gonerNode != null)
		//                gonerNode.ParentNode.RemoveChild(gonerNode);
		//            gonerNode = GetPropertyNode(theirEntry, propName);
		//            if (gonerNode != null)
		//                gonerNode.ParentNode.RemoveChild(gonerNode);
		//        }
		//        else
		//        {
		//            var ourDoc = ourEntry == null ? null : ourEntry.OwnerDocument;
		//            var ourOriginalData = new SortedDictionary<string, string>();
		//            var theirDoc = theirEntry == null ? null : theirEntry.OwnerDocument;
		//            var theirOriginalData = new SortedDictionary<string, string>();
		//            if (ourPropNode == null)
		//            {
		//                if (ourEntry != null)
		//                {
		//                    ourPropNode = ourDoc.CreateNode(XmlNodeType.Element, propName, null);
		//                    ourEntry.AppendChild(ourPropNode);
		//                }
		//            }
		//            else
		//            {
		//                foreach (XmlNode ourOriginal in ourPropNode.ChildNodes)
		//                    ourOriginalData.Add(XmlUtilities.GetStringAttribute(ourOriginal, SharedConstants.GuidStr).ToLowerInvariant(), ourOriginal.OuterXml);
		//                ourPropNode.RemoveAll();
		//            }
		//            if (theirPropNode == null)
		//            {
		//                if (theirEntry != null)
		//                {
		//                    theirPropNode = theirDoc.CreateNode(XmlNodeType.Element, propName, null);
		//                    theirEntry.AppendChild(theirPropNode);
		//                }
		//            }
		//            else
		//            {
		//                foreach (XmlNode theirOriginal in theirPropNode.ChildNodes)
		//                    theirOriginalData.Add(XmlUtilities.GetStringAttribute(theirOriginal, SharedConstants.GuidStr).ToLowerInvariant(), theirOriginal.OuterXml);
		//                theirPropNode.RemoveAll();
		//            }
		//            foreach (var newValue in mergedCollection)
		//            {
		//                var ourData = ourOriginalData.ContainsKey(newValue)
		//                                ? ourOriginalData[newValue]
		//                                : theirOriginalData[newValue];
		//                if (ourDoc != null)
		//                    AddNode(ourDoc, ourPropNode, XElement.Parse(ourData));
		//                var theirData = theirOriginalData.ContainsKey(newValue)
		//                                    ? theirOriginalData[newValue]
		//                                    : ourOriginalData[newValue];
		//                if (theirDoc != null)
		//                    AddNode(theirDoc, theirPropNode, XElement.Parse(theirData));
		//            }
		//        }
		//    }
		//}

		//private static void PreMergeOldStyleOwningSequenceProperties(IMergeEventListener eventListener, MetadataCache mdc, FdoClassInfo classInfo, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		//{
		//    foreach (var ownSeqProp in classInfo.AllProperties.Where(prop => prop.DataType == DataType.OwningSequence))
		//    {
		//        // TODO: Maybe do nothing as those old style ones are slated to go away.
		//    }
		//}

//        private static void PreMergeOwningSequenceProperties(IMergeEventListener eventListener, MergeSituation mergeSituation, MetadataCache mdc, FdoClassInfo classInfo, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
//        {
//            foreach (var ownSeqProp in classInfo.AllProperties.Where(prop => prop.DataType == DataType.OwningSequence))
//            {
//                var propName = ownSeqProp.PropertyName;
//                var commonPropNode = GetPropertyNode(commonEntry, propName);
//#if !TRYUSINGMAINMERGER
//                var commonValues = new List<string>();
//                var commonNodes = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
//                if (commonPropNode != null)
//                {
//                    foreach (XmlNode commonNode in commonPropNode.ChildNodes)
//                    {
//                        var guid = commonNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();
//                        commonNodes.Add(guid, commonNode);
//                        commonValues.Add(guid);
//                    }
//                }
//#endif

//                var ourPropNode = GetPropertyNode(ourEntry, propName);
//#if !TRYUSINGMAINMERGER
//                var ourValues = new List<string>();
//                var ourContinuingValues = new List<string>();
//                var ourNewbieValues = new List<string>();
//                var ourGonerValues = new List<string>();
//                var ourNodes = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
//                if (ourPropNode != null)
//                {
//                    foreach (XmlNode ourNode in ourPropNode.ChildNodes)
//                    {
//                        var guid = ourNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();
//                        ourNodes.Add(guid, ourNode);
//                        ourValues.Add(guid);
//                    }
//                    if (!commonValues.SequenceEqual(ourValues, StringComparer.OrdinalIgnoreCase))
//                    {
//                        // Bad idea to be so generic.
//                        //eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, ourEntry));
//                        ourContinuingValues.AddRange(commonValues.Where(ourValues.Contains));
//                        ourNewbieValues.AddRange(ourValues.Where(ourPreviousValue => !commonValues.Contains(ourPreviousValue)));
//                        ourGonerValues.AddRange(commonValues.Where(commonPreviousValue => !ourValues.Contains(commonPreviousValue)));
//                    }
//                }
//#endif

//                var theirPropNode = GetPropertyNode(theirEntry, propName);
//                // TODO: This will do the basics on a temporary set of nodes sent to the merger,
//                // but we need to account for edit-remove conflicts,
//                // Q: and other changes? A: Maybe just let them go and the regular merge system can deal with them. In fact, maybe just skip any special treatment for owning seq props, but have the merge strat for that class of props decalre order to be important.
//                var result = TryUsingMainMerger(mergeSituation, commonPropNode, ourPropNode, theirPropNode);
//#if !TRYUSINGMAINMERGER
//                var theirValues = new List<string>();
//                var theirContinuingValues = new List<string>();
//                var theirNewbieValues = new List<string>();
//                var theirGonerValues = new List<string>();
//                var theirNodes = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
//                if (theirPropNode != null)
//                {
//                    foreach (XmlNode theirNode in theirPropNode.ChildNodes)
//                    {
//                        var guid = theirNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant();
//                        theirNodes.Add(guid, theirNode);
//                        theirValues.Add(guid);
//                    }

//                    if (!commonValues.SequenceEqual(theirValues, StringComparer.OrdinalIgnoreCase))
//                    {
//                        // Bad idea to be so generic.
//                        //eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, theirEntry));
//                        theirContinuingValues.AddRange(commonValues.Where(theirValues.Contains));
//                        theirNewbieValues.AddRange(theirValues.Where(theirPreviousValue => !commonValues.Contains(theirPreviousValue)));
//                        theirGonerValues.AddRange(commonValues.Where(commonPreviousValue => !theirValues.Contains(commonPreviousValue)));
//                    }
//                }

//                // Process elements that were deleted by both.
//                var bothZappedList = ourGonerValues.Where(theirGonerValues.Contains).ToList();
//                foreach (var bothZapped in bothZappedList)
//                {
//                    eventListener.ChangeOccurred(new XmlBothDeletionChangeReport(mergeSituation.PathToFileInRepository, commonPropNode.SelectSingleNode("[@guid]='" + bothZapped + "']")));
//                    ourGonerValues.Remove(bothZapped);
//                    theirGonerValues.Remove(bothZapped);
//                    ourValues.Remove(bothZapped); // Good idea, or not?
//                    theirValues.Remove(bothZapped); // Good idea, or not?
//                }

//                // So much for the easy stuff. :-(
//                // The ones left in either goner list, were uniquely deleted by we or they:
//                // 1. May have been edited by the other, or
//                // 2. May have been moved, or
//                // 3. May have been simple nobody cares deleted.
//                foreach (var ourDeleted in ourGonerValues)
//                {
//                    var commonNode = commonNodes[ourDeleted];
//                    var theirNode = theirNodes[ourDeleted];
//                    if (XmlUtilities.AreXmlElementsEqual(commonNode, theirNode))
//                    {
//                        // #3. Who cares deletion, or #2 moved.
//                        eventListener.ChangeOccurred(new XmlDeletionChangeReport(mergeSituation.PathToFileInRepository, commonNodes[ourDeleted], theirNodes[ourDeleted]));
//                    }
//                    else
//                    {
//                        // Messy conflict (we removed, they edited). We need to restore 'ours', by putting 'theirs' into ours,
//                        // so it will end up in the final output xml string.
//                        // Since 'ours' will then be identical to 'theirs' for any following comparisons,
//                        // it ought not generate any new reports (changes or conflicts) between ours and theirs,
//                        // but it will add some changes between it and common.
//                        // TODO: Needs the describer.
//                        eventListener.ConflictOccurred(new RemovedVsEditedElementConflict(commonNode.Name, null, theirNode, commonNode, mergeSituation, null, mergeSituation.BetaUserId));
//                        ourNodes.Add(commonNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant(), theirNode); // Wrong doc, but try to fix that later.
//                    }
//                }
//                foreach (var theirDeleted in theirGonerValues)
//                {
//                    var commonNode = commonNodes[theirDeleted];
//                    var ourNode = ourNodes[theirDeleted];
//                    if (XmlUtilities.AreXmlElementsEqual(commonNode, ourNode))
//                    {
//                        // #3. Who cares deletion, or #2 moved.
//                        eventListener.ChangeOccurred(new XmlDeletionChangeReport(mergeSituation.PathToFileInRepository, commonNodes[theirDeleted], ourNodes[theirDeleted]));
//                    }
//                    else
//                    {
//                        // Messy conflict (we removed, they edited). We need to restore 'ours', by putting 'theirs' into ours,
//                        // so it will end up in the final output xml string.
//                        // Since 'ours' will then be identical to 'theirs' for any following comparisons,
//                        // it ought not generate any new reports (changes or conflicts) between ours and theirs,
//                        // but it will add some changes between it and common.
//                        // TODO: Needs the describer.
//                        eventListener.ConflictOccurred(new EditedVsRemovedElementConflict(commonNode.Name, ourNode, null, commonNode, mergeSituation, null, mergeSituation.AlphaUserId));
//                        // Not needed, since it is already in ourNodes
//                        //ourNodes.Add(commonNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant(), ourNode); // Wrong doc, but try to fix that later.
//                        theirNodes.Add(commonNode.GetStringAttribute(SharedConstants.GuidStr).ToLowerInvariant(), ourNode); // Wrong doc, but try to fix that later.
//                    }
//                }

//                // Have mostly have taken care of the goners by this point.
//                // There are the additions, and where to put them, still.


//                /*****************************************/

//                // 1. Pre-merge objects that are in ours and theirs (may not be in common, however).
//                PreMerge(eventListener, mergeSituation, mdc, commonPropNode, ourPropNode, theirPropNode, new HashSet<string>(theirValues));

//                if (commonValues.SequenceEqual(ourValues, StringComparer.OrdinalIgnoreCase) && commonValues.SequenceEqual(theirValues, StringComparer.OrdinalIgnoreCase))
//                    continue; // No changes in the owning  property, so skip the harder merge attempt.

//                var ourDoc = ourEntry == null ? null : ourEntry.OwnerDocument;
//                var ourOriginalData = new Dictionary<string, string>();
//                var theirDoc = theirEntry == null ? null : theirEntry.OwnerDocument;
//                var theirOriginalData = new Dictionary<string, string>();
//                if (ourPropNode == null)
//                {
//                    if (ourDoc != null)
//                    {
//                        // Create the missing prop node in our doc.
//                        ourPropNode = ourDoc.CreateNode(XmlNodeType.Element, propName, null);
//                        ourEntry.AppendChild(ourPropNode);
//                    }
//                }
//                else
//                {
//                    ourOriginalData = ourPropNode
//                        .ChildNodes
//                        .Cast<XmlNode>()
//                        .ToDictionary(
//                                ourOriginal => XmlUtilities.GetStringAttribute(ourOriginal, SharedConstants.GuidStr).ToLowerInvariant(),
//                                ourOriginal => ourOriginal.OuterXml);
//                    ourPropNode.RemoveAll(); // Zap everthing in the original property, as it will be replaced later on.
//                }

//                if (theirPropNode == null)
//                {
//                    if (theirDoc != null)
//                    {
//                        // Create the missing prop node in their doc.
//                        theirPropNode = theirDoc.CreateNode(XmlNodeType.Element, propName, null);
//                        theirEntry.AppendChild(theirPropNode);
//                    }
//                }
//                else
//                {
//                    theirOriginalData = theirPropNode
//                        .ChildNodes
//                        .Cast<XmlNode>()
//                        .ToDictionary(
//                                theirOriginal => XmlUtilities.GetStringAttribute(theirOriginal, SharedConstants.GuidStr).ToLowerInvariant(),
//                                theirOriginal => theirOriginal.OuterXml);
//                    theirPropNode.RemoveAll(); // Zap everthing in the original property, as it will be replaced later on.
//                }

//                // Finish the regular part of the merge.
//                if (commonValues.Count > 0)
//                {
//                    // ancestor had data.
//                    for (var idxInCommon = 0; idxInCommon < commonValues.Count; ++idxInCommon)
//                    {
//                        var commonValue = commonValues[idxInCommon];
//                        var idxInOurs = ourValues.IndexOf(commonValue);
//                        var idxInTheirs = theirValues.IndexOf(commonValue);
//                        if (idxInOurs == -1)
//                        {
//                            if (idxInTheirs == -1)
//                            {
//                                // Both removed it, so no conflict.
//                                eventListener.ChangeOccurred(new XmlBothDeletionChangeReport(mergeSituation.PathToFileInRepository, commonEntry));
//                            }
//                            else
//                            {
//                                // We removed it. But, edit trumps delete, so check.
//                                if (!XmlUtilities.AreXmlElementsEqual(theirEntry, commonEntry))
//                                {
//                                    // TODO: Get Element describer.
//                                    eventListener.ConflictOccurred(new RemovedVsEditedElementConflict(commonEntry.Name, null, theirEntry, commonEntry, mergeSituation, null, mergeSituation.AlphaUserId));
//                                }
//                                else
//                                {
//                                    theirValues.Remove(commonValue);
//                                }
//                            }
//                            continue;
//                        }

//                        if (idxInTheirs == -1)
//                        {
//                            if (idxInOurs == -1)
//                            {
//                                // Can't get here. But if it did , then both removed it.
//                                eventListener.ChangeOccurred(new XmlBothDeletionChangeReport(mergeSituation.PathToFileInRepository, commonEntry));
//                            }
//                            else
//                            {
//                                // They deleted. Did we edit?
//                                if (!XmlUtilities.AreXmlElementsEqual(ourEntry, commonEntry))
//                                {
//                                    // They deleted, but we edited, so keep ours.
//                                    // TODO: Element describer.
//                                    eventListener.ConflictOccurred(new EditedVsRemovedElementConflict(commonEntry.Name, ourEntry, null, commonEntry, mergeSituation, null, mergeSituation.BetaUserId));
//                                    //theirValues.Insert(0, commonValue); // Put it back.
//                                }
//                                else
//                                {
//                                    //theirValues.Remove(commonValue);
//                                    ourValues.Remove(commonValue);
//                                }
//                            }
//                            continue;
//                        }

//                        var stop = false;
//                        if (idxInOurs > 0)
//                        {
//                            // Add the new ones that are in ours, but before commonValue. 'ours' wins if idxInTheirs is greater than -1.
//                            while (idxInOurs > 0)
//                            {
//                                var ourCurrent = ourValues[0];
//                                var ourData = ourOriginalData[ourCurrent];
//                                var ourOrigElement = XElement.Parse(ourData);
//                                if (ourDoc != null)
//                                    AddNode(ourDoc, ourPropNode, ourOrigElement);
//                                if (theirOriginalData.ContainsKey(ourCurrent))
//                                {
//                                    AddNode(theirDoc, theirPropNode, XElement.Parse(theirOriginalData[ourCurrent]));
//                                    theirOriginalData.Remove(ourCurrent);
//                                    theirValues.Remove(ourCurrent);
//                                    idxInTheirs = theirValues.IndexOf(commonValue);
//                                }
//                                else
//                                {
//                                    if (theirDoc != null)
//                                        AddNode(theirDoc, theirPropNode, XElement.Parse(ourOriginalData[ourCurrent]));
//                                }
//                                idxInOurs = ourValues.IndexOf(commonValue);
//                            }
//                            stop = true;
//                        }
//                        else if (idxInTheirs > 0)
//                        {
//                            while (idxInTheirs > 0)
//                            {
//                                var theirCurrent = theirValues[0];
//                                var theirData = ourOriginalData[theirCurrent];
//                                var theirOrigElement = XElement.Parse(theirData);
//                                if (theirDoc != null)
//                                    AddNode(theirDoc, theirPropNode, theirOrigElement);
//                                if (ourOriginalData.ContainsKey(theirCurrent))
//                                {
//                                    if (ourDoc != null)
//                                        AddNode(ourDoc, ourPropNode, XElement.Parse(ourOriginalData[theirCurrent]));
//                                    ourOriginalData.Remove(theirCurrent);
//                                    ourValues.Remove(theirCurrent);
//                                    idxInOurs = ourValues.IndexOf(commonValue);
//                                }
//                                else
//                                {
//                                    if (ourDoc != null)
//                                        AddNode(ourDoc, ourPropNode, XElement.Parse(ourOriginalData[theirCurrent]));
//                                }
//                                idxInTheirs = theirValues.IndexOf(commonValue);
//                            }
//                            stop = true;
//                        }
//                        if (stop)
//                            continue;

//                        // Add respective data back into each, since they each still have it.
//                        if (ourDoc != null)
//                            AddNode(ourDoc, ourPropNode, XElement.Parse(ourOriginalData[commonValue]));
//                        ourOriginalData.Remove(commonValue);
//                        ourValues.Remove(commonValue);
//                        if (theirDoc != null)
//                            AddNode(theirDoc, theirPropNode, XElement.Parse(theirOriginalData[commonValue]));
//                        theirOriginalData.Remove(commonValue);
//                        theirValues.Remove(commonValue);
//                    }
//                }
//                else
//                {
//                    // ours and/or theirs added new stuff, so add them in some order, and report an ambigious insert.
//                    // Just make sure they are added only once, if they are in both ours and theirs.
//                    foreach (var ourValue in ourValues)
//                    {
//                        var ourData = ourOriginalData[ourValue];
//                        var ourOrigElement = XElement.Parse(ourData);
//                        if (ourDoc != null)
//                            AddNode(ourDoc, ourPropNode, ourOrigElement);
//                        if (theirValues.Contains(ourValue))
//                        {
//                            // Both have it, but store possibly different data from theirs.
//                            if (theirDoc != null)
//                                AddNode(theirDoc, theirPropNode, XElement.Parse(theirOriginalData[ourValue]));
//                            theirValues.Remove(ourValue);
//                            theirOriginalData.Remove(ourValue);
//                        }
//                        else
//                        {
//                            if (theirDoc != null)
//                                AddNode(theirDoc, theirPropNode, ourOrigElement);
//                        }
//                    }
//                    foreach (var theirValue in theirValues)
//                    {
//                        var theirData = theirOriginalData[theirValue];
//                        var theirOrigElement = XElement.Parse(theirData);
//                        if (ourDoc != null)
//                            AddNode(ourDoc, ourPropNode, theirOrigElement);
//                        if (theirDoc != null)
//                            AddNode(theirDoc, theirPropNode, theirOrigElement);
//                    }
//                }
//                if (ourPropNode != null && !ourPropNode.HasChildNodes)
//                    ourPropNode.ParentNode.RemoveChild(ourPropNode);
//                if (theirPropNode != null && !theirPropNode.HasChildNodes)
//                    theirPropNode.ParentNode.RemoveChild(theirPropNode);
//#endif
//            } // End of main property loop.
//        }

		//private static NodeMergeResult TryUsingMainMerger(MergeSituation mergeSituation, XmlNode commonPropNode, XmlNode ourPropNode, XmlNode theirPropNode)
		//{
		//    var extantPropNode = commonPropNode ?? ourPropNode ?? theirPropNode;
		//    var commonRootNode = CreateRootNode(commonPropNode, extantPropNode.Name);
		//    var ourRootNode = CreateRootNode(ourPropNode, extantPropNode.Name);
		//    var theirRootNode = CreateRootNode(theirPropNode, extantPropNode.Name);

		//    var merger = new XmlMerger(mergeSituation);
		//    merger.MergeStrategies.ElementStrategies.Add(extantPropNode.Name, ElementStrategy.CreateSingletonElement());
		//    merger.MergeStrategies.ElementStrategies.Add("obj", ElementStrategy.CreateForKeyedElement("guid", true));
		//    var result = merger.Merge(ourRootNode, theirRootNode, commonRootNode);
		//    return result;
		//}

		//private static XmlElement CreateRootNode(XmlNode commonPropNode, string rootNodeName)
		//{
		//    var commonDoc = new XmlDocument();
		//    var commonRootNode = commonDoc.CreateElement(rootNodeName);
		//    commonDoc.AppendChild(commonRootNode);
		//    if (commonPropNode != null)
		//    {
		//        foreach (XmlNode childNode in commonPropNode.ChildNodes)
		//        {
		//            var objNode = commonDoc.CreateElement("obj");
		//            commonRootNode.AppendChild(objNode);
		//            var guidAttr = commonDoc.CreateAttribute("guid");
		//            guidAttr.Value = childNode.GetStringAttribute("guid");
		//            objNode.Attributes.Append(guidAttr);
		//        }
		//    }
		//    return commonRootNode;
		//}

		//private static void PreMerge(IMergeEventListener eventListener, MergeSituation mergeSituation, MetadataCache mdc, XmlNode commonPropNode, XmlNode ourPropNode, XmlNode theirPropNode, HashSet<string> theirValues)
		//{
		//    if (ourPropNode == null)
		//        return;

		//    foreach (XmlNode ourChild in ourPropNode.ChildNodes)
		//    {
		//        var ourChildGuid = XmlUtilities.GetStringAttribute(ourChild, SharedConstants.GuidStr).ToLowerInvariant();
		//        if (!theirValues.Contains(ourChildGuid))
		//            continue;

		//        var query = string.Format(@"{0}[@guid=""{1}""]", ourChild.Name, ourChildGuid);
		//        PreMerge(true, eventListener, mergeSituation, mdc,
		//                 ourChild,
		//                 theirPropNode.SelectSingleNode(query),
		//                 commonPropNode == null ? null : commonPropNode.SelectSingleNode(query));
		//    }
		//}

		//private static void PreMergeAtomicOwningProperties(bool isNewStyle, IMergeEventListener eventListener, MergeSituation mergeSituation, MetadataCache mdc, FdoClassInfo classInfo, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		//{

		//    if (!isNewStyle)
		//        return;

		//    foreach (var ownAtomicProp in classInfo.AllProperties.Where(prop => prop.DataType == DataType.OwningAtomic))
		//    {
		//        var propName = ownAtomicProp.PropertyName;
		//        var ourAtomicOwningPropNode = GetPropertyNode(ourEntry, propName);
		//        var theirAtomicOwningPropNode = GetPropertyNode(theirEntry, propName);
		//        var commonAtomicOwningPropNode = GetPropertyNode(commonEntry, propName);
		//        // If all three are the same object, then use the regular recursive call back to this method.
		//        // If they are not the same, there is nothing that can be done to pre-merge them.
		//        var ourOwnedItem = ourAtomicOwningPropNode == null ? null : ourAtomicOwningPropNode.FirstChild;
		//        var theirOwnedItem = theirAtomicOwningPropNode == null ? null : theirAtomicOwningPropNode.FirstChild;
		//        var commonOwnedItem = commonAtomicOwningPropNode == null ? null : commonAtomicOwningPropNode.FirstChild;
		//        if (ourOwnedItem != null && ourOwnedItem.Name != SharedConstants.Objsur
		//            && theirOwnedItem != null && theirOwnedItem.Name != SharedConstants.Objsur
		//            && commonOwnedItem != null && commonOwnedItem.Name != SharedConstants.Objsur
		//            && ourOwnedItem.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant() == theirOwnedItem.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant()) // Allows for common to be different.
		//        {
		//            PreMerge(true, eventListener, mergeSituation, mdc, ourOwnedItem, theirOwnedItem, commonOwnedItem);
		//        }
		//    }
		//}

		//private static void AddNode(XmlDocument srcDoc, XmlNode parentNode, XElement newElementData)
		//{
		//    var newNode = srcDoc.CreateNode(XmlNodeType.Element, newElementData.Name.LocalName, null);
		//    parentNode.AppendChild(newNode);
		//    if (newElementData.HasAttributes)
		//    {
		//        foreach (var attr in newElementData.Attributes())
		//        {
		//            var newAttr = srcDoc.CreateAttribute(attr.Name.LocalName);
		//            newAttr.Value = attr.Value;
		//            newNode.Attributes.Append(newAttr);
		//        }
		//    }
		//    if (newElementData.HasElements)
		//    {
		//        foreach (XElement childElement in newElementData.Nodes())
		//            AddNode(srcDoc, newNode, childElement);
		//        return;
		//    }

		//    // Text in an element.
		//    newNode.AppendChild(srcDoc.CreateTextNode(newElementData.Value));
		//}
	}
}