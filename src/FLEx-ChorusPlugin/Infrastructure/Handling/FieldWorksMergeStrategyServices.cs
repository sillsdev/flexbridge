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

			foreach (var sharedKvp in sharedElementStrategies)
				strategiesForMerger.SetStrategy(sharedKvp.Key, sharedKvp.Value);

			BootstrapHeaderElementNonClassStrategies(strategiesForMerger);

			var classStrat = new ElementStrategy(false)
			{
				MergePartnerFinder = GuidKey,
				ContextDescriptorGenerator = ContextGen,
				IsAtomic = false
			};

			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				// ScrDraft instances can only be added or removed, but not changed, according to John Wickberg (18 Jan 2012).
				classStrat.IsImmutable = classInfo.ClassName == "ScrDraft";
				// Didn't work, since the paras are actually in an 'ownseq' element.
				// So, use a new ownseatomic element tag.
				// classStrat.IsAtomic = classInfo.ClassName == "StTxtPara" || classInfo.ClassName == "ScrTxtPara";

				strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);
				foreach (var propertyInfo in classInfo.AllProperties)
				{
					var isCustom = propertyInfo.IsCustomProperty;
					var propStrategy = isCustom
										? ElementStrategy.CreateForKeyedElement(SharedConstants.Name, false)
										: ElementStrategy.CreateSingletonElement();
					if (propertyInfo.DataType == DataType.Time)
					{
						propStrategy.IsImmutable = true; // Immutable, because we have pre-merged them to be so.
					}
					strategiesForMerger.SetStrategy(String.Format("{0}{1}_{2}", isCustom ? "Custom_" : "", classInfo.ClassName, propertyInfo.PropertyName), propStrategy);
				}
			}
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
								//ContextDescriptorGenerator = _contextGen
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
			elementStrategy = ElementStrategy.CreateForKeyedElementInList(SharedConstants.GuidStr);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			sharedElementStrategies.Add(SharedConstants.Refseq, elementStrategy);

			// Add element for "ownseq"
			elementStrategy = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, true);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			sharedElementStrategies.Add(SharedConstants.Ownseq, elementStrategy);

			// Add element for "ownseqatomic" // Atomic here means the whole elment is treated as effectively as if it were binary data.
			elementStrategy = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, true);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, SharedConstants.Class });
			elementStrategy.IsAtomic = true;
			sharedElementStrategies.Add(SharedConstants.OwnseqAtomic, elementStrategy);

			// Add element for "objsur".
			elementStrategy = ElementStrategy.CreateForKeyedElement(SharedConstants.GuidStr, false);
			elementStrategy.AttributesToIgnoreForMerging.AddRange(new[] { SharedConstants.GuidStr, "t" });
			sharedElementStrategies.Add(SharedConstants.Objsur, elementStrategy);
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
								IsAtomic = isAtomic
							};
			//strategy.ContextDescriptorGenerator
			sharedElementStrategies.Add(elementName, strategy);
		}

		private static void BootstrapHeaderElementNonClassStrategies(MergeStrategies strategiesForMerger)
		{
			var strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy(SharedConstants.Header, strategy);

			// Add all anthro pos list elements.
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("AnthroList", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("ConfidenceLevels", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("Education", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("Locations", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("People", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("Positions", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("Restrictions", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("Roles", strategy);
			strategy = CreateSingletonElementType(true);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("Status", strategy);
			strategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy("TimeOfDay", strategy);

			// As of 26 Jan 2012, no other context has non-class wrapper elements.
		}
	}
}