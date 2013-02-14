using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.Handling.Anthropology;
using FLEx_ChorusPlugin.Infrastructure.Handling.Common;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Discourse;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Lexicon;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.MorphologyAndSyntax;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Phonology;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.WordformInventory;
using FLEx_ChorusPlugin.Infrastructure.Handling.Scripture;
using Palaso.Network;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Services class used by FieldWorksMergingStrategy to create ElementStrategy instances
	/// (some shared and some not shared).
	/// </summary>
	internal static class FieldWorksMergeStrategyServices
	{
		private static readonly FindByKeyAttribute WsKey = new FindByKeyAttribute(SharedConstants.Ws);
		private static readonly FindByKeyAttribute GuidKey = new FindByKeyAttribute(SharedConstants.GuidStr);
		private static readonly FindFirstElementWithSameName SameName = new FindFirstElementWithSameName();
		private static readonly FieldWorkObjectContextGenerator ContextGen = new FieldWorkObjectContextGenerator();
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

			var sharedElementStrategies = new Dictionary<string, ElementStrategy>();
			CreateSharedElementStrategies(sharedElementStrategies);

			var strategiesForMerger = merger.MergeStrategies;
			ContextGen.MergeStrategies = strategiesForMerger;

			foreach (var sharedKvp in sharedElementStrategies)
				strategiesForMerger.SetStrategy(sharedKvp.Key, sharedKvp.Value);

			var headerStrategy = CreateSingletonElementType(false);
			headerStrategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy(SharedConstants.Header, headerStrategy);

			// There are two abstract class names used: CmAnnotation and DsChart.
			// Chorus knows how to find the matching element for these, as they use <CmAnnotation class='concreteClassname'.
			// So, add two keyed strategies for each of them.
			var keyedStrat = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.GuidStr);
			strategiesForMerger.SetStrategy(SharedConstants.CmAnnotation, keyedStrat);

			keyedStrat = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.GuidStr);
			strategiesForMerger.SetStrategy(SharedConstants.DsChart, keyedStrat);

			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				var classStrat = MakeClassStrategy(ContextGen);
				// ScrDraft instances can only be added or removed, but not changed, according to John Wickberg (18 Jan 2012).
				classStrat.IsImmutable = classInfo.ClassName == "ScrDraft";
				// Didn't work, since the paras are actually in an 'ownseq' element.
				// So, use a new ownseatomic element tag.
				// classStrat.IsAtomic = classInfo.ClassName == "StTxtPara" || classInfo.ClassName == "ScrTxtPara";
				strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);

				switch (classInfo.ClassName)
				{
					case "LangProject":
						classStrat.ContextDescriptorGenerator = new LanguageProjectContextGenerator();
						break;
					case "LexEntry":
						classStrat.ContextDescriptorGenerator = new LexEntryContextGenerator();
						break;
					case "WfiWordform":
						classStrat.ContextDescriptorGenerator = new WfiWordformContextGenerator();
						break;
					case "Text":
						classStrat.ContextDescriptorGenerator = new TextContextGenerator();
						break;
					case "RnGenericRec":
						classStrat.ContextDescriptorGenerator = new RnGenericRecContextGenerator();
						break;
					case "ScrBook":
						classStrat.ContextDescriptorGenerator = new ScrBookContextGenerator();
						break;
					case "ScrSection":
						classStrat.ContextDescriptorGenerator = new ScrSectionContextGenerator();
						break;
					case "CmPossibilityList":
						classStrat.ContextDescriptorGenerator = new PossibilityListContextGenerator();
						break;
						// These should be all the subclasses of CmPossiblity. It's unfortuate to have to list them here;
						// OTOH, if we ever want special handling for any of them, we can easily add a special generator.
						// Note that these will not usually be found as strategies, since they are owned in owning sequences
						// and ownseq has its own item. However, they can be found by the default object context generator code,
						// which has a special case for ownseq.
					case "MoMorphType":
					case "PartOfSpeech":
					case "ChkTerm":
					case "PhPhonRuleFeat":
					case "CmCustomItem":
					case "CmLocation":
					case "CmAnnotationDefn":
					case "CmPerson":
					case "CmAnthroItem":
					case "CmSemanticDomain":
					case "LexEntryType":
					case "LexRefType":
					case "CmPossibility":
						classStrat.ContextDescriptorGenerator = new PossibilityContextGenerator();
						break;
					case "PhEnvironment":
						classStrat.ContextDescriptorGenerator = new EnvironmentContextGenerator();
						break;
					case "DsConstChart":
					case "ConstChartRow":
					case "ConstChartWordGroup":
						classStrat.ContextDescriptorGenerator = new DiscourseChartContextGenerator();
						break;
					case "PhNCSegments":
						classStrat.ContextDescriptorGenerator = new MultiLingualStringsContextGenerator("Natural Class", "Name", "Abbreviation");
						break;
					case "FsClosedFeature":
						classStrat.ContextDescriptorGenerator = new MultiLingualStringsContextGenerator("Phonological Features", "Name", "Abbreviation");
						break;
				}
				foreach (var propertyInfo in classInfo.AllProperties)
				{
					var isCustom = propertyInfo.IsCustomProperty;
					var propStrategy = isCustom
										? CreateStrategyForKeyedElement(SharedConstants.Name, false)
										: CreateSingletonElementStrategy();
					switch (propertyInfo.DataType)
					{
						//default:
						//	break;
						case DataType.ReferenceSequence:
							// Trying to merge the Analyses of a segment is problematic. Best to go all-or-nothing, and ensure
							// we get a conflict report if it fails.
							if (classInfo.ClassName == "Segment" && propertyInfo.PropertyName == "Analyses")
								propStrategy.IsAtomic = true;
							break;
						case DataType.ReferenceAtomic:
							if(classInfo.ClassName ==  "LexSense" && propertyInfo.PropertyName == "MorphoSyntaxAnalysis")
							{
								propStrategy.ContextDescriptorGenerator = new PosContextGenerator();
							}
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
							break;
						case DataType.OwningAtomic:
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
							break;

						case DataType.TextPropBinary:
							propStrategy.ContextDescriptorGenerator = new StyleContextGenerator();
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
							break;
						case DataType.Unicode: // Fall through - Contains one <Uni> element
						case DataType.String: // Fall through (TsString) - Contains one <Str> element
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
							break;

						case DataType.Integer: // Fall through
						case DataType.Boolean: // Fall through
						case DataType.GenDate:
							// LT-13320 "Date of Event is lost after send/receive (data loss)"
							// says these fields don't play nice as immutable.
							//if (classInfo.ClassName == "CmPerson" || classInfo.ClassName == "RnGenericRec")
							//	propStrategy.IsImmutable = true; // Surely DateOfBirth, DateOfDeath, and DateOfEvent are fixed. onced they happen. :-)
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
							break;
						case DataType.Time:
							propStrategy.IsImmutable = true; // Immutable, because some of them are immutable leagally (date created), and we have pre-merged the rest to be so.
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
							break;
						case DataType.Guid:
							if (classInfo.ClassName == "CmFilter" || classInfo.ClassName == "CmResource")
								propStrategy.IsImmutable = true;
							propStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;
							break;
						case DataType.Binary:
							propStrategy.IsAtomic = true;
							break;
					}
					strategiesForMerger.SetStrategy(String.Format("{0}{1}_{2}", isCustom ? "Custom_" : "", classInfo.ClassName, propertyInfo.PropertyName), propStrategy);
				}
			}
		}

		private static ElementStrategy MakeClassStrategy(IGenerateContextDescriptor descriptor)
		{
			var classStrat = new ElementStrategy(false)
								{
									MergePartnerFinder = GuidKey,
									ContextDescriptorGenerator = descriptor,
									IsAtomic = false
								};
			if (ContextGen != null && descriptor is FieldWorkObjectContextGenerator)
				((FieldWorkObjectContextGenerator) descriptor).MergeStrategies = ContextGen.MergeStrategies;
			return classStrat;
		}

		private static ElementStrategy CreateSingletonElementStrategy()
		{
			var result = ElementStrategy.CreateSingletonElement();
			result.ContextDescriptorGenerator = ContextGen;
			return result;
		}

		private static void AddSharedImmutableSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = AddSharedSingletonElementType(sharedElementStrategies, name, orderOfTheseIsRelevant);
			strategy.IsImmutable = true;
		}

		private static ElementStrategy AddSharedSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = CreateSingletonElementType(orderOfTheseIsRelevant);
			sharedElementStrategies.Add(name, strategy);
			return strategy;
		}

		private static ElementStrategy CreateSingletonElementType(bool orderOfTheseIsRelevant)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = SameName,
								ContextDescriptorGenerator = ContextGen
							};
			return strategy;
		}

		private static void CreateSharedElementStrategies(Dictionary<string, ElementStrategy> sharedElementStrategies)
		{
			// Set up immutable strategies.
			// Skip this one, since all DateTime props are, either legally (created), or are pre-processed for merging.
			//AddSharedImmutableSingletonElementType(sharedElementStrategies, DateCreated, false);
			AddSharedImmutableSingletonElementType(sharedElementStrategies, ImmutableSingleton, false);

			AddSharedSingletonElementType(sharedElementStrategies, MutableSingleton, false);

			var elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, SharedConstants.Str, false);
			elementStrategy.IsAtomic = true; // TsStrings are atomic
			elementStrategy.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;

			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, SharedConstants.Binary, false);
			elementStrategy.IsAtomic = true; // Binary properties are atomic

			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, SharedConstants.Prop, false);
			elementStrategy.IsAtomic = true; // Prop is atomic

			AddSharedSingletonElementType(sharedElementStrategies, SharedConstants.Uni, false);
			AddSharedKeyedByWsElementType(sharedElementStrategies, SharedConstants.AStr, false, true); // final parm is for IsAtomic, which in this case is atomic.
			AddSharedKeyedByWsElementType(sharedElementStrategies, SharedConstants.AUni, false, false); // final parm is for IsAtomic, which in this case is not atomic.

			// Add element for "ownseq"
			elementStrategy = CreateStrategyForKeyedElement(SharedConstants.GuidStr, true);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			sharedElementStrategies.Add(SharedConstants.Ownseq, elementStrategy);

			// Add element for SharedConstants.Objsur.
			// This is only good now for ref atomic.
			// No. atomic ref prop can't have multiples, so there is no need for a keyed lookup. CreateStrategyForKeyedElement(SharedConstants.GuidStr, false);
			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, SharedConstants.Objsur, false);
			elementStrategy.IsAtomic = true; // Testing to see if atomic here, or at the prop level is better, as per https://www.pivotaltracker.com/story/show/25402673
			elementStrategy.NumberOfChildren = NumberOfChildrenAllowed.Zero;

			// Add element for SharedConstants.Refseq
			elementStrategy = CreateStrategyForElementKeyedByGuidInList(); // JohnT's new Chorus widget that handles potentially repeating element guids for ref seq props.
			elementStrategy.AttributesToIgnoreForMerging.Add("t");
			sharedElementStrategies.Add(SharedConstants.Refseq, elementStrategy);

			// Add element for SharedConstants.Refcol
			// Order is not important in any kind of collection property, since they are mathematical sets with no ordering and no repeats.
			elementStrategy = CreateStrategyForKeyedElement(SharedConstants.GuidStr, false);
			elementStrategy.AttributesToIgnoreForMerging.Add("t");
			sharedElementStrategies.Add(SharedConstants.Refcol, elementStrategy);
		}

		private static ElementStrategy CreateStrategyForElementKeyedByGuidInList()
		{
			var result = ElementStrategy.CreateForKeyedElementInList(SharedConstants.GuidStr);
			result.ContextDescriptorGenerator = ContextGen;
			return result;
		}

		private static ElementStrategy CreateStrategyForKeyedElement(string guid, bool orderIsRelevant)
		{
			var result = ElementStrategy.CreateForKeyedElement(guid, orderIsRelevant);
			result.ContextDescriptorGenerator = ContextGen;
			return result;
		}

		private static void AddSharedKeyedByWsElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, bool orderOfTheseIsRelevant, bool isAtomic)
		{
			AddKeyedElementType(sharedElementStrategies, elementName, WsKey, orderOfTheseIsRelevant, isAtomic);
		}

		private static void AddKeyedElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, IFindNodeToMerge findBykeyAttribute, bool orderOfTheseIsRelevant, bool isAtomic)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = findBykeyAttribute,
								ContextDescriptorGenerator = ContextGen,
								IsAtomic = isAtomic
							};
			sharedElementStrategies.Add(elementName, strategy);
		}

		/// <summary>
		/// This method will return the guid associated with the given element moving up the heirarchy if necessary.
		/// </summary>
		internal static string GetGuid(XmlNode element)
		{
			var elt = element;
			while (elt != null && MetadataCache.MdCache.GetClassInfo(FieldWorksMergingServices.GetClassName(elt)) == null
				   && elt.Name != SharedConstants.Ownseq)
				elt = elt.ParentNode;
			return elt.Attributes[SharedConstants.GuidStr] == null
				? GetGuid(element.ParentNode) // Oops. Its a property node that has the same name as a class (e.g., PartOfSppech, or Lexdb), so go higher.
				: elt.Attributes[SharedConstants.GuidStr].Value;
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
			var query = HttpUtilityFromMono.UrlDecode(fwUrl.Substring(hostLength + 1));
			var url = host + "?" + query;
			return new ContextDescriptor(label, url);
		}
	}
}