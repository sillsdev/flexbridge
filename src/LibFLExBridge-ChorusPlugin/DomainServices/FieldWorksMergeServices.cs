// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Handling;
using LibFLExBridgeChorusPlugin.Handling.Anthropology;
using LibFLExBridgeChorusPlugin.Handling.Common;
using LibFLExBridgeChorusPlugin.Handling.CustomProperties;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Discourse;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Lexicon;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.MorphologyAndSyntax;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Phonology;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.Reversal;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.TextCorpus;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.WordformInventory;
using LibFLExBridgeChorusPlugin.Handling.Scripture;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.Code;
using SIL.Network;

namespace LibFLExBridgeChorusPlugin.DomainServices
{
	/// <summary>
	/// Services class used to support various merge tasks.
	///		1. Create an XmlMerger and the ElementStrategy instances it will use,
	///		2. Getting a Guid from an element,
	///		3. createing a context descriptor
	/// </summary>
	internal static class FieldWorksMergeServices
	{
		private static readonly FindByKeyAttribute WsKeyFinder = new FindByKeyAttribute(FlexBridgeConstants.Ws);
		private static readonly FindByKeyAttribute GuidKeyFinder = new FindByKeyAttribute(FlexBridgeConstants.GuidStr);
		private static readonly FindFirstElementWithSameName SameNameFinder = new FindFirstElementWithSameName();
		private const string MutableSingleton = "MutableSingleton";
		private const string ImmutableSingleton = "ImmutableSingleton";

		internal static XmlMerger CreateXmlMergerForFieldWorksData(MergeOrder mergeOrder, MetadataCache mdc)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation)
				{
					EventListener = mergeOrder.EventListener
				};
			BootstrapSystem(mdc, merger);
			return merger;
		}

		/// <summary>
		/// Bootstrap a merger for the new-styled (nested) files.
		/// </summary>
		/// <remarks>
		/// 1. A generic 'header' element will be handled, although it may not appear in the file.
		/// 2. All classes will be included.
		/// 3. Merge strategies for class properties (regular or custom) will have keys of "classname+propname" to make them unique, system-wide.
		/// </remarks>
		private static void BootstrapSystem(MetadataCache metadataCache, XmlMerger merger)
		{
			merger.MergeStrategies.ElementToMergeStrategyKeyMapper = new FieldWorksElementToMergeStrategyKeyMapper();

			// Each merger gets its own default context generator, since it holds this merger's strategies and metadata.
			var contextGen = new FieldWorkObjectContextGenerator(metadataCache);
			var sharedElementStrategies = new Dictionary<string, ElementStrategy>();
			CreateSharedElementStrategies(sharedElementStrategies, contextGen);

			var strategiesForMerger = merger.MergeStrategies;
			contextGen.MergeStrategies = strategiesForMerger;

			foreach (var sharedKvp in sharedElementStrategies)
				strategiesForMerger.SetStrategy(sharedKvp.Key, sharedKvp.Value);

			var customPropDefnStrat = new ElementStrategy(false)
							{
								MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { FlexBridgeConstants.Name, FlexBridgeConstants.Class }),
								ContextDescriptorGenerator = new FieldWorksCustomPropertyContextGenerator(),
								IsAtomic = true,
								NumberOfChildren = NumberOfChildrenAllowed.Zero
							};
			strategiesForMerger.SetStrategy(FlexBridgeConstants.CustomField, customPropDefnStrat);

			var headerStrategy = CreateSingletonElementType(false, contextGen);
			strategiesForMerger.SetStrategy(FlexBridgeConstants.Header, headerStrategy);

			// There are two abstract class names used: CmAnnotation and DsChart.
			// Chorus knows how to find the matching element for these, as they use <CmAnnotation class='concreteClassname'.
			// So, add a keyed strategy for each of them.
			var keyedStrat = ElementStrategy.CreateForKeyedElement(FlexBridgeConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(FlexBridgeConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(FlexBridgeConstants.GuidStr);
			strategiesForMerger.SetStrategy(FlexBridgeConstants.CmAnnotation, keyedStrat);

			keyedStrat = ElementStrategy.CreateForKeyedElement(FlexBridgeConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(FlexBridgeConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(FlexBridgeConstants.GuidStr);
			strategiesForMerger.SetStrategy(FlexBridgeConstants.DsChart, keyedStrat);

			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				MakeClassStrategy(metadataCache, strategiesForMerger, classInfo, contextGen);
				AddPropertyStrategiesForClass(metadataCache, strategiesForMerger, classInfo, contextGen);
			}
		}

		private static void MakeClassStrategy(MetadataCache mdc, MergeStrategies strategiesForMerger, FdoClassInfo classInfo, FieldWorkObjectContextGenerator defaultDescriptor)
		{
			Guard.AgainstNull(defaultDescriptor, "defaultDescriptor");

			// These values can be overridden or added to in the big switch, below.
			var classStrat = new ElementStrategy(false)
				{
					ContextDescriptorGenerator = defaultDescriptor,
					MergePartnerFinder = GuidKeyFinder,
					IsAtomic = false
				};

			strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);

			// Try to keep these in alphbetical order, and where there are 'blocks', then try to keep the blocks in order.
			// That will make them easier to find.
			switch (classInfo.ClassName)
			{
				case "CmPossibilityList":
					classStrat.ContextDescriptorGenerator = new PossibilityListContextGenerator(mdc);
					break;
				case "FsClosedFeature":
					classStrat.ContextDescriptorGenerator = new MultiLingualStringsContextGenerator(mdc, "Phonological Features", "Name", "Abbreviation");
					break;
				case "FsFeatStruc":
					classStrat.IsAtomic = true;
					break;
				case "LangProject":
					classStrat.ContextDescriptorGenerator = new LanguageProjectContextGenerator(mdc);
					break;
				case "LexEntry":
					classStrat.ContextDescriptorGenerator = new LexEntryContextGenerator(mdc);
					break;
				case "PhEnvironment":
					classStrat.ContextDescriptorGenerator = new EnvironmentContextGenerator(mdc);
					break;
				case "PhNCSegments":
					classStrat.ContextDescriptorGenerator = new MultiLingualStringsContextGenerator(mdc, "Natural Class", "Name", "Abbreviation");
					break;
				case "ReversalIndexEntry":
					classStrat.ContextDescriptorGenerator = new ReversalEntryContextGenerator(mdc);
					break;
				case "RnGenericRec":
					classStrat.ContextDescriptorGenerator = new RnGenericRecContextGenerator(mdc);
					break;
				case "ScrBook":
					classStrat.ContextDescriptorGenerator = new ScrBookContextGenerator(mdc);
					break;
				case "ScrDraft":
					// ScrDraft instances can only be added or removed, but not changed, according to John Wickberg (18 Jan 2012).
					classStrat.IsImmutable = true;
					break;
				case "ScrSection":
					classStrat.ContextDescriptorGenerator = new ScrSectionContextGenerator(mdc);
					break;
				case "ScrTxtPara": // Fall through.
				case "StTxtPara":
					// This will never be used, since StTxtParas & ScrTxtParas are actually in an 'ownseq' element.
					classStrat.Premerger = new StTxtParaPremerger();
					// Didn't work, since StTxtParas & ScrTxtParas are actually in an 'ownseq' element.
					// classStrat.IsAtomic = true;
					break;
				case "Text":
					classStrat.ContextDescriptorGenerator = new TextContextGenerator(mdc);
					break;
				case "WfiWordform":
					classStrat.ContextDescriptorGenerator = new WfiWordformContextGenerator(mdc);
					break;

				// These should be all the subclasses of CmPossiblity. It's unfortuate to have to list them here;
				// OTOH, if we ever want special handling for any of them, we can easily add a special generator.
				// Note that these will not usually be found as strategies, since they are owned in owning sequences
				// and ownseq has its own item. However, they can be found by the default object context generator code,
				// which has a special case for ownseq.
				case "ChkTerm":
				case "CmAnthroItem":
				case "CmAnnotationDefn":
				case "CmCustomItem":
				case "CmLocation":
				case "CmPerson":
				case "CmPossibility":
				case "CmSemanticDomain":
				case "LexEntryType":
				case "LexRefType":
				case "MoMorphType":
				case "PartOfSpeech":
				case "PhPhonRuleFeat":
					classStrat.ContextDescriptorGenerator = new PossibilityContextGenerator(mdc);
					break;

				case "ConstChartRow":
				case "ConstChartWordGroup":
				case "DsConstChart":
					classStrat.ContextDescriptorGenerator = new DiscourseChartContextGenerator(mdc);
					break;
			}

			((FieldWorkObjectContextGenerator)classStrat.ContextDescriptorGenerator).MergeStrategies = strategiesForMerger;
		}

		private static void AddPropertyStrategiesForClass(MetadataCache mdc, MergeStrategies strategiesForMerger, FdoClassInfo classInfo, FieldWorkObjectContextGenerator contextGen)
		{
			foreach (var propertyInfo in classInfo.AllProperties)
			{
				var isCustom = propertyInfo.IsCustomProperty;
				var propStrategy = isCustom
									   ? CreateStrategyForKeyedElement(FlexBridgeConstants.Name, false, contextGen)
									   : CreateSingletonElementStrategy(contextGen);
				switch (propertyInfo.DataType)
				{
					// Block of object properties

					case DataType.OwningAtomic:
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
						break;
					//case DataType.OwningCollection: // Nothing special done
					//	break;
					case DataType.OwningSequence:
						if ((classInfo.ClassName == "CmPossibilityList" && propertyInfo.PropertyName == "Possibilities")
							|| (propertyInfo.PropertyName == "SubPossibilities" && classInfo.IsOrInheritsFrom("CmPossibility")))
						{
							// Order may or may not be significant in possibility lists and sublists, depending on whether the list is sorted.
							propStrategy.ChildOrderPolicy = new PossibilityListOrderPolicy();
						}
						else
						{
							// Normally order is significant in owning sequences; no need to ask each child.
							propStrategy.ChildOrderPolicy = new SignificantOrderPolicy();
						}
						break;
					case DataType.ReferenceAtomic:
						if (classInfo.ClassName == "LexSense" && propertyInfo.PropertyName == "MorphoSyntaxAnalysis")
						{
							propStrategy.ContextDescriptorGenerator = new PosContextGenerator(mdc);
						}
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
						break;
					//case DataType.ReferenceCollection: // Nothing special done
					//	break;
					case DataType.ReferenceSequence:
						// Trying to merge the Analyses of a segment is problematic. Best to go all-or-nothing, and ensure
						// we get a conflict report if it fails.
						if (classInfo.ClassName == "Segment" && propertyInfo.PropertyName == "Analyses")
							propStrategy.IsAtomic = true;
						break;

					// Block of multi-somethings
					// In model, but nothing special done at the property element level
					//case DataType.MultiString:
					//    break;
					//case DataType.MultiUnicode:
					//    break;

					// Block of other property data types
					case DataType.Binary:
						propStrategy.IsAtomic = true;
						break;
					case DataType.Boolean:
						// LT-13320 "Date of Event is lost after send/receive (data loss)"
						// says these fields don't play nice as immutable.
						//if (classInfo.ClassName == "CmPerson" || classInfo.ClassName == "RnGenericRec")
						//	propStrategy.IsImmutable = true; // Surely DateOfBirth, DateOfDeath, and DateOfEvent are fixed. onced they happen. :-)
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
						break;
					//case DataType.Float: // Not used in model
					//	break;
					case DataType.GenDate:
						// LT-13320 "Date of Event is lost after send/receive (data loss)"
						// says these fields don't play nice as immutable.
						//if (classInfo.ClassName == "CmPerson" || classInfo.ClassName == "RnGenericRec")
						//	propStrategy.IsImmutable = true; // Surely DateOfBirth, DateOfDeath, and DateOfEvent are fixed. onced they happen. :-)
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
						break;
					case DataType.Guid:
						if (classInfo.ClassName == "CmFilter" || classInfo.ClassName == "CmResource")
							propStrategy.IsImmutable = true;
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
						break;
					case DataType.Integer:
						if (propertyInfo.PropertyName == "HomographNumber")
						{
							// Don't fret about conflicts in merging the homograph numbers.
							propStrategy.AttributesToIgnoreForMerging.Add("val");
						}
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
						break;
					//case DataType.Numeric: // Not used in model
					//	break;
					case DataType.String: // Contains one <Str> element
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
						break;
					case DataType.TextPropBinary:
						propStrategy.ContextDescriptorGenerator = new StyleContextGenerator(mdc);
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
						break;
					case DataType.Time:
						if (propertyInfo.PropertyName == "DateCreated")
						{
							propStrategy.IsImmutable = true;
						}
						else
						{
							// Suppress conflicts and change reports for other date time properties, which currently are all
							// some variation on modify time, or most recent run time.
							// For all of them, it is appropriate to just keep the most recent.
							propStrategy.Premerger = new PreferMostRecentTimePreMerger();
						}
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
						break;
					case DataType.Unicode: // Contains one <Uni> element
						propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
						break;
				}
				strategiesForMerger.SetStrategy(
					String.Format("{0}{1}_{2}", isCustom ? "Custom_" : "", classInfo.ClassName, propertyInfo.PropertyName), propStrategy);
			}
		}

		private static ElementStrategy CreateSingletonElementStrategy(FieldWorkObjectContextGenerator contextGen)
		{
			var result = ElementStrategy.CreateSingletonElement();
			result.ContextDescriptorGenerator = contextGen;
			return result;
		}

		private static void AddSharedImmutableSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant, FieldWorkObjectContextGenerator contextGen)
		{
			var strategy = AddSharedSingletonElementType(sharedElementStrategies, name, orderOfTheseIsRelevant, contextGen);
			strategy.IsImmutable = true;
		}

		private static ElementStrategy AddSharedSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant, FieldWorkObjectContextGenerator contextGen)
		{
			var strategy = CreateSingletonElementType(orderOfTheseIsRelevant, contextGen);
			sharedElementStrategies.Add(name, strategy);
			return strategy;
		}

		private static ElementStrategy CreateSingletonElementType(bool orderOfTheseIsRelevant, FieldWorkObjectContextGenerator contextGen)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = SameNameFinder,
								ContextDescriptorGenerator = contextGen
							};
			return strategy;
		}

		private static void CreateSharedElementStrategies(Dictionary<string, ElementStrategy> sharedElementStrategies, FieldWorkObjectContextGenerator contextGen)
		{
			// Set up immutable strategies.
			// Skip this one, since all DateTime props are, either legally (created), or are pre-processed for merging.
			//AddSharedImmutableSingletonElementType(sharedElementStrategies, DateCreated, false, contextGen);
			AddSharedImmutableSingletonElementType(sharedElementStrategies, ImmutableSingleton, false, contextGen);

			AddSharedSingletonElementType(sharedElementStrategies, MutableSingleton, false, contextGen);

			var elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, FlexBridgeConstants.Str, false, contextGen);
			elementStrategy.IsAtomic = true; // TsStrings are atomic
			elementStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;

			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, FlexBridgeConstants.Binary, false, contextGen);
			elementStrategy.IsAtomic = true; // Binary properties are atomic

			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, FlexBridgeConstants.Prop, false, contextGen);
			elementStrategy.IsAtomic = true; // Prop is atomic

			AddSharedSingletonElementType(sharedElementStrategies, FlexBridgeConstants.Uni, false, contextGen);
			AddSharedKeyedByWsElementType(sharedElementStrategies, FlexBridgeConstants.AStr, false, true, contextGen); // 4th parm is for IsAtomic, which in this case is atomic.
			AddSharedKeyedByWsElementType(sharedElementStrategies, FlexBridgeConstants.AUni, false, false, contextGen); // 4th parm is for IsAtomic, which in this case is not atomic.

			// Add element for "ownseq"
			elementStrategy = CreateStrategyForKeyedElement(FlexBridgeConstants.GuidStr, true, contextGen);
			elementStrategy.Premerger = new OwnSeqPremerger();
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { FlexBridgeConstants.GuidStr, FlexBridgeConstants.Class });
			sharedElementStrategies.Add(FlexBridgeConstants.Ownseq, elementStrategy);

			// Add element for SharedConstants.Objsur.
			// This is only good now for ref atomic.
			// No. atomic ref prop can't have multiples, so there is no need for a keyed lookup. CreateStrategyForKeyedElement(SharedConstants.GuidStr, false);
			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, FlexBridgeConstants.Objsur, false, contextGen);
			elementStrategy.IsAtomic = true; // Testing to see if atomic here, or at the prop level is better, as per https://www.pivotaltracker.com/story/show/25402673
			elementStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;

			// Add element for SharedConstants.Refseq
			elementStrategy = CreateStrategyForElementKeyedByGuidInList(contextGen); // JohnT's new Chorus widget that handles potentially repeating element guids for ref seq props.
			elementStrategy.AttributesToIgnoreForMerging.Add("t");
			sharedElementStrategies.Add(FlexBridgeConstants.Refseq, elementStrategy);

			// Add element for SharedConstants.Refcol
			// Order is not important in any kind of collection property, since they are mathematical sets with no ordering and no repeats.
			elementStrategy = CreateStrategyForKeyedElement(FlexBridgeConstants.GuidStr, false, contextGen);
			elementStrategy.AttributesToIgnoreForMerging.Add("t");
			sharedElementStrategies.Add(FlexBridgeConstants.Refcol, elementStrategy);
		}

		private static ElementStrategy CreateStrategyForElementKeyedByGuidInList(FieldWorkObjectContextGenerator contextGen)
		{
			var result = ElementStrategy.CreateForKeyedElementInList(FlexBridgeConstants.GuidStr);
			result.ContextDescriptorGenerator = contextGen;
			return result;
		}

		private static ElementStrategy CreateStrategyForKeyedElement(string guid, bool orderIsRelevant, FieldWorkObjectContextGenerator contextGen)
		{
			var result = ElementStrategy.CreateForKeyedElement(guid, orderIsRelevant);
			result.ContextDescriptorGenerator = contextGen;
			return result;
		}

		private static void AddSharedKeyedByWsElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, bool orderOfTheseIsRelevant, bool isAtomic, FieldWorkObjectContextGenerator contextGen)
		{
			AddKeyedElementType(sharedElementStrategies, elementName, WsKeyFinder, orderOfTheseIsRelevant, isAtomic, contextGen);
		}

		private static void AddKeyedElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, IFindNodeToMerge findBykeyAttribute, bool orderOfTheseIsRelevant, bool isAtomic, FieldWorkObjectContextGenerator contextGen)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = findBykeyAttribute,
								ContextDescriptorGenerator = contextGen,
								IsAtomic = isAtomic
							};
			sharedElementStrategies.Add(elementName, strategy);
		}

		/// <summary>
		/// This method will return the guid associated with the given element moving up the heirarchy if necessary.
		/// </summary>
		/// <param name="mdc">The metadata of the element's project, used to tell class elements from property elements.</param>
		/// <param name="element">The element to start from.</param>
		internal static string GetGuid(MetadataCache mdc, XmlNode element)
		{
			var elt = element;
			while (elt != null && mdc.GetClassInfo(GetClassName(elt)) == null
				   && elt.Name != FlexBridgeConstants.Ownseq)
				elt = elt.ParentNode;
			return elt.Attributes[FlexBridgeConstants.GuidStr] == null
				? GetGuid(mdc, element.ParentNode) // Oops. Its a property node that has the same name as a class (e.g., PartOfSppech, or Lexdb), so go higher.
				: elt.Attributes[FlexBridgeConstants.GuidStr].Value;
		}

		private static string GetClassName(XmlNode element)
		{
			// Owning collections do nothing special for the main element name;
			var name = element.Name;
			return (name == FlexBridgeConstants.Ownseq)
				? element.Attributes[FlexBridgeConstants.Class].Value
				: name;
		}

		/// <summary>
		/// Builds a context descriptor with the given parameters that will provide a url link to the correct area in FLEx.
		/// </summary>
		internal static ContextDescriptor GenerateContextDescriptor(string filePath, string guid, string label)
		{
			var appId = "FLEx";
			var directory = Path.GetDirectoryName(filePath);
			var lastDirectory = Path.GetFileName(directory);
			if (lastDirectory == "Scripture")
				appId = "TE";
			// figure out here which we need.
			var fwAppArgs = new FwAppArgs(appId, "current", "", "default", guid);
			// Add the "label" information which the Chorus Notes browser extracts to identify the object in the UI.
			// This is just for a label and we can't have & or = in the value. So replace them if they occur.
			fwAppArgs.AddProperty("label", label.Replace("&", " and ").Replace("=", " equals "));
			// The FwUrl has all the query part encoded.
			// Chorus needs it unencoded so it can extract the label.
			var fwUrl = fwAppArgs.ToString();
			var hostLength = fwUrl.IndexOf("?", StringComparison.Ordinal);
			var host = fwUrl.Substring(0, hostLength);
			var query = HttpUtility.UrlDecode(fwUrl.Substring(hostLength + 1));
			var url = host + "?" + query;
			return new ContextDescriptor(label, url);
		}
	}
}