using System;
using System.Collections.Generic;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Services class used by FieldWorksMergingStrategy to create ElementStrategy instances
	/// (some shared and some not shared).
	/// </summary>
	internal static class FieldWorksMergeStrategyServices
	{
		private static readonly FindByKeyAttribute WsKey = new FindByKeyAttribute(Ws);
		private static readonly FindByKeyAttribute GuidKey = new FindByKeyAttribute(SharedConstants.GuidStr);
		private static readonly FindFirstElementWithSameName SameName = new FindFirstElementWithSameName();
		private static readonly FieldWorkObjectContextGenerator ContextGen = new FieldWorkObjectContextGenerator();
		private const string MutableSingleton = "MutableSingleton";
		private const string ImmutableSingleton = "ImmutableSingleton";
		private const string Str = "Str";
		private const string AStr = "AStr";
		private const string Uni = "Uni";
		private const string AUni = "AUni";
		private const string Ws = "ws";
		private const string Binary = "Binary";
		private const string Prop = "Prop";

		/// <summary>
		/// Bootstrap a merger for the new-styled (nested) files.
		/// </summary>
		/// <remarks>
		/// 1. A generic 'header' element will be handled, although it may not appear in the file.
		/// 2. All classes  will be included.
		/// 3. Merge strategies for class properties (regular or custom) will have keys of "classname+propname" to make them unique, system-wide.
		/// </remarks>
		internal static void BootstrapSystem(MetadataCache metadataCache, XmlMerger merger)
		{
			var sharedElementStrategies = new Dictionary<string, ElementStrategy>();
			CreateSharedElementStrategies(sharedElementStrategies);

			var strategiesForMerger = merger.MergeStrategies;
			ContextGen.MergeStrategies = strategiesForMerger;

			foreach (var sharedKvp in sharedElementStrategies)
				strategiesForMerger.SetStrategy(sharedKvp.Key, sharedKvp.Value);

			BootstrapHeaderElementNonClassStrategies(strategiesForMerger);

			var classStrat = MakeClassStrategy(ContextGen);

			// There are two abstract class names used: CmAnnotation and DsChart.
			// Chorus knows how to find the matching element for these, as they use <CmAnnotation class='concreteClassname'.
			// So, add two keyed strategies for each of them.
			var keyedStrat = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.GuidStr);
			strategiesForMerger.SetStrategy("CmAnnotation", keyedStrat);

			keyedStrat = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.GuidStr);
			strategiesForMerger.SetStrategy("DsChart", keyedStrat);

			// The lint file has a collection of odd stuff.
			keyedStrat = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, false);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.Class);
			keyedStrat.AttributesToIgnoreForMerging.Add(SharedConstants.GuidStr);
			keyedStrat.AttributesToIgnoreForMerging.Add("curiositytype");
			keyedStrat.AttributesToIgnoreForMerging.Add("tempownerguid");
			strategiesForMerger.SetStrategy("curiosity", keyedStrat);

			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				// ScrDraft instances can only be added or removed, but not changed, according to John Wickberg (18 Jan 2012).
				classStrat.IsImmutable = classInfo.ClassName == "ScrDraft";
				// Didn't work, since the paras are actually in an 'ownseq' element.
				// So, use a new ownseatomic element tag.
				// classStrat.IsAtomic = classInfo.ClassName == "StTxtPara" || classInfo.ClassName == "ScrTxtPara";

				switch (classInfo.ClassName)
				{
					case "LexEntry":
						strategiesForMerger.SetStrategy(classInfo.ClassName, MakeClassStrategy(new LexEntryContextGenerator()));
						break;
					case "WfiWordform":
						strategiesForMerger.SetStrategy(classInfo.ClassName, MakeClassStrategy(new WfiWordformContextGenerator()));
						break;
					case "CmPossibilityList":
						strategiesForMerger.SetStrategy(classInfo.ClassName, MakeClassStrategy(new PossibilityListContextGenerator()));
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
						strategiesForMerger.SetStrategy(classInfo.ClassName, MakeClassStrategy(new PossibilityContextGenerator()));
						break;
					case "PhEnvironment":
						strategiesForMerger.SetStrategy(classInfo.ClassName, MakeClassStrategy(new EnvironmentContextGenerator()));
						break;
					case "DsConstChart":
					case "ConstChartRow":
					case "ConstChartWordGroup":
						strategiesForMerger.SetStrategy(classInfo.ClassName, MakeClassStrategy(new DiscourseChartContextGenerator()));
						break;
					default:
						strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);
						break;
				}
				foreach (var propertyInfo in classInfo.AllProperties)
				{
					var isCustom = propertyInfo.IsCustomProperty;
					var propStrategy = isCustom
										? CreateStrategyForKeyedElement(SharedConstants.Name, false)
										: CreateSingletonElementStrategy();
					if (propertyInfo.DataType == DataType.Time)
					{
						propStrategy.IsImmutable = true; // Immutable, because we have pre-merged them to be so.
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
			var elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, Str, false);
			elementStrategy.IsAtomic = true;
			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, Binary, false);
			elementStrategy.IsAtomic = true;
			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, Prop, false);
			elementStrategy.IsAtomic = true;
			AddSharedSingletonElementType(sharedElementStrategies, Uni, false);
			AddSharedKeyedByWsElementType(sharedElementStrategies, AStr, false, true);
			AddSharedKeyedByWsElementType(sharedElementStrategies, AUni, true, false);

			// Add element for SharedConstants.Refseq
			elementStrategy = CreateStrategyForElementKeyedByGuidInList();
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			sharedElementStrategies.Add(SharedConstants.Refseq, elementStrategy);

			// Add element for "ownseq"
			elementStrategy = CreateStrategyForKeyedElement(SharedConstants.GuidStr, true);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			sharedElementStrategies.Add(SharedConstants.Ownseq, elementStrategy);

			// Add element for "ownseqatomic" // Atomic here means the whole elment is treated as effectively as if it were binary data.
			elementStrategy = CreateStrategyForKeyedElement(SharedConstants.GuidStr, true);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			elementStrategy.IsAtomic = true;
			sharedElementStrategies.Add(SharedConstants.OwnseqAtomic, elementStrategy);

			// Add element for "objsur".
			elementStrategy = CreateStrategyForKeyedElement(SharedConstants.GuidStr, false);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, "t" });
			sharedElementStrategies.Add(SharedConstants.Objsur, elementStrategy);
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

		private static void BootstrapHeaderElementNonClassStrategies(MergeStrategies strategiesForMerger)
		{
			var strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy(SharedConstants.Header, strategy);

			// As of 26 Jan 2012, no context has non-class wrapper elements in the header.
		}
	}
}