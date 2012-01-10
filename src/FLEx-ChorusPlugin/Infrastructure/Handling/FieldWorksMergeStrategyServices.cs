using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Properties;

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
		private const string Custom = "Custom";

		internal static ElementStrategy AddSharedImmutableSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = AddSharedSingletonElementType(sharedElementStrategies, name, orderOfTheseIsRelevant);
			strategy.IsImmutable = true;
			return strategy;
		}

		internal static ElementStrategy AddSharedSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = CreateSingletonElementType(orderOfTheseIsRelevant);
			sharedElementStrategies.Add(name, strategy);
			return strategy;
		}

		internal static ElementStrategy CreateSingletonElementType(bool orderOfTheseIsRelevant)
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
			AddSharedKeyedByWsElementType(sharedElementStrategies, AUni, false, true);
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

		private static void CreateMergers(MetadataCache metadataCache, MergeSituation mergeSituation,
										  IDictionary<string, ElementStrategy> sharedElementStrategies, IDictionary<string, XmlMerger> mergers)
		{
			var mutableSingleton = sharedElementStrategies[MutableSingleton];
			var immSingleton = sharedElementStrategies[ImmutableSingleton];
			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				var merger = new XmlMerger(mergeSituation);
				var strategiesForMerger = merger.MergeStrategies;
				strategiesForMerger.SetStrategy(SharedConstants.RtTag, sharedElementStrategies[SharedConstants.RtTag]);

				// Add all of the property bits.
				// NB: Each of the child elements (except for custom properties)
				// will be singletons.
				foreach (var propInfo in classInfo.AllProperties)
				{
					if (propInfo.IsCustomProperty)
					{
						ProcessCustomProperty(sharedElementStrategies, strategiesForMerger, propInfo, mutableSingleton);
					}
					else
					{
						ProcessStandardProperty(sharedElementStrategies, strategiesForMerger, propInfo, mutableSingleton, immSingleton);
					}
				}
				mergers.Add(classInfo.ClassName, merger);

			}

		}

		private static void ProcessCustomProperty(IDictionary<string, ElementStrategy> sharedElementStrategies, MergeStrategies strategiesForMerger, FdoPropertyInfo propInfo, ElementStrategy mutableSingleton)
		{
			// Start all with basic keyed strategy.
			var strategyForCurrentProperty = ElementStrategy.CreateForKeyedElement(SharedConstants.Name, false);
			ElementStrategy extantStrategy;
			switch (propInfo.DataType)
			{
				case DataType.Time: // Fall through
				case DataType.OwningCollection:
				case DataType.ReferenceCollection:
					strategyForCurrentProperty.IsImmutable = true;
					break;

				case DataType.OwningSequence: // Fall through. // TODO: Sort out ownership issues for conflicts.
				case DataType.ReferenceSequence:
					// Use IsAtomic for whole property.
					strategyForCurrentProperty.IsAtomic = true;
					break;
				case DataType.OwningAtomic: // Fall through. // TODO: Think about implications of a conflict.
				case DataType.ReferenceAtomic:
					// If own seq/col & ref seq end up being atomic, then 'obsur' is a simple singleton.
					// Otherwise, things get more complicated, since this one is singleton, but the others would be keyed.
					if (!strategiesForMerger.ElementStrategies.TryGetValue(SharedConstants.Objsur, out extantStrategy))
						strategiesForMerger.SetStrategy(SharedConstants.Objsur, mutableSingleton);
					break;

					// Other data types
				case DataType.MultiUnicode:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AUni, out extantStrategy))
						strategiesForMerger.SetStrategy(AUni, sharedElementStrategies[AUni]);
					break;
				case DataType.MultiString:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AStr, out extantStrategy))
						strategiesForMerger.SetStrategy(AStr, sharedElementStrategies[AStr]);
					break;
				case DataType.Unicode: // Ordinary C# string
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Uni, out extantStrategy))
						strategiesForMerger.SetStrategy(Uni, sharedElementStrategies[Uni]);
					break;
				case DataType.String: // TsString
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Str, out extantStrategy))
						strategiesForMerger.SetStrategy(Str, sharedElementStrategies[Str]);
					break;
				case DataType.Binary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Binary, out extantStrategy))
						strategiesForMerger.SetStrategy(Binary, sharedElementStrategies[Binary]);
					break;
				case DataType.TextPropBinary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Prop, out extantStrategy))
						strategiesForMerger.SetStrategy(Prop, sharedElementStrategies[Prop]);
					break;
					// NB: Booleans can never be in conflict in a 3-way merge environment.
					// One or the other can toggle the bool, so the one changing it 'wins'.
					// If both change it then it's no big deal either.
				case DataType.Boolean: // Fall through.
				case DataType.GenDate: // Fall through.
				case DataType.Guid: // Fall through.
				case DataType.Integer: // Fall through.
					// Simple, mutable properties.
					break;
			}
			strategiesForMerger.SetStrategy(Custom + "_" + propInfo.PropertyName, strategyForCurrentProperty);
		}

		private static void ProcessStandardProperty(IDictionary<string, ElementStrategy> sharedElementStrategies, MergeStrategies strategiesForMerger, FdoPropertyInfo propInfo, ElementStrategy mutableSingleton, ElementStrategy immSingleton)
		{
			// Start all with basic mutable singleton. Switch to 'immSingleton' for properties that are immutable.
			var strategyForCurrentProperty = mutableSingleton;
			ElementStrategy extantStrategy;
			switch (propInfo.DataType)
			{
					// These three are immutable, in a manner of speaking.
					// DateCreated is honestly, and the other two are because 'ours' and 'theirs' have been made to be the same already.
				case DataType.Time: // Fall through // DateTime
				case DataType.OwningCollection:
				case DataType.ReferenceCollection:
					strategyForCurrentProperty = immSingleton;
					break;

				case DataType.OwningSequence: // Fall through. // TODO: Sort out ownership issues for conflicts.
				case DataType.ReferenceSequence:
					// Use IsAtomic for whole property.
					strategyForCurrentProperty = CreateSingletonElementType(false);
					strategyForCurrentProperty.IsAtomic = true;
					break;
				case DataType.OwningAtomic: // Fall through. // TODO: Think about implications of a conflict.
				case DataType.ReferenceAtomic:
					strategyForCurrentProperty = mutableSingleton;
					// If own seq/col & ref seq end up being atomic, then 'obsur' is a simple singleton.
					// Otherwise, things get more complicated, since this one is singleton, but the others would be keyed.
					if (!strategiesForMerger.ElementStrategies.TryGetValue(SharedConstants.Objsur, out extantStrategy))
						strategiesForMerger.SetStrategy(SharedConstants.Objsur, mutableSingleton);
					break;

					// Other data types
				case DataType.MultiUnicode:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AUni, out extantStrategy))
						strategiesForMerger.SetStrategy(AUni, sharedElementStrategies[AUni]);
					break;
				case DataType.MultiString:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AStr, out extantStrategy))
						strategiesForMerger.SetStrategy(AStr, sharedElementStrategies[AStr]);
					break;
				case DataType.Unicode: // Ordinary C# string
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Uni, out extantStrategy))
						strategiesForMerger.SetStrategy(Uni, sharedElementStrategies[Uni]);
					break;
				case DataType.String: // TsString
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Str, out extantStrategy))
						strategiesForMerger.SetStrategy(Str, sharedElementStrategies[Str]);
					break;
				case DataType.Binary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Binary, out extantStrategy))
						strategiesForMerger.SetStrategy(Binary, sharedElementStrategies[Binary]);
					break;
				case DataType.TextPropBinary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Prop, out extantStrategy))
						strategiesForMerger.SetStrategy(Prop, sharedElementStrategies[Prop]);
					break;
					// NB: Booleans can never be in conflict in a 3-way merge environment.
					// One or the other can toggle the bool, so the one changing it 'wins'.
					// If both change it then it's no big deal either.
				case DataType.Boolean: // Fall through.
				case DataType.GenDate: // Fall through.
				case DataType.Guid: // Fall through.
				case DataType.Integer: // Fall through.
					// Simple, mutable properties.
					break;
			}
			strategiesForMerger.SetStrategy(propInfo.PropertyName, strategyForCurrentProperty);
		}

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

			var headerStrategy = CreateSingletonElementType(true);
			headerStrategy.ContextDescriptorGenerator = ContextGen;
			strategiesForMerger.SetStrategy(SharedConstants.Header, headerStrategy);

			var classStrat = new ElementStrategy(false)
								{
									MergePartnerFinder = GuidKey,
									ContextDescriptorGenerator = ContextGen,
									IsAtomic = false
								};

			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);
				foreach (var propertyInfo in classInfo.AllProperties)
				{
					var isCustom = propertyInfo.IsCustomProperty;
					var propStrategy = isCustom
										? ElementStrategy.CreateForKeyedElement(SharedConstants.Name, false)
										: ElementStrategy.CreateSingletonElement();
					switch (propertyInfo.DataType)
					{
						case DataType.OwningCollection:
							propStrategy.IsImmutable = false;
							break;
							// These two are immutable, in a manner of speaking.
							// DateCreated is honestly, and the other two are because 'ours' and 'theirs' have been made to be the same already.
						case DataType.Time: // Fall through // DateTime
						case DataType.ReferenceCollection:
							propStrategy.IsImmutable = true;
							break;

						case DataType.OwningSequence: // Fall through. // TODO: Sort out ownership issues for conflicts.
						case DataType.ReferenceSequence:
							// Use IsAtomic for whole property.
							propStrategy.IsAtomic = true;
							break;

						case DataType.OwningAtomic: // Fall through. // TODO: Think about implications of a conflict.
						case DataType.ReferenceAtomic:

							// Other data types
						case DataType.MultiUnicode:
						case DataType.MultiString:
						case DataType.Unicode: // Ordinary C# string
						case DataType.String: // TsString
						case DataType.Binary:
						case DataType.TextPropBinary:
							// NB: Booleans can never be in conflict in a 3-way merge environment.
							// One or the other can toggle the bool, so the one changing it 'wins'.
							// If both change it then it's no big deal either.
						case DataType.Boolean: // Fall through.
						case DataType.GenDate: // Fall through.
						case DataType.Guid: // Fall through.
						case DataType.Integer: // Fall through.
							// Simple, mutable properties.
							break;
					}
					strategiesForMerger.SetStrategy(String.Format("{0}{1}_{2}", isCustom ? "Custom_" : "", classInfo.ClassName, propertyInfo.PropertyName), propStrategy);
				}
			}
		}

		internal static void BootstrapSystem(MetadataCache mdc, Dictionary<string, ElementStrategy> sharedElementStrategies, Dictionary<string, XmlMerger> mergers, MergeSituation mergeSituation)
		{
			var strategy = new ElementStrategy(false)
							{
								MergePartnerFinder = GuidKey
							};
			strategy.AttributesToIgnoreForMerging.Add(SharedConstants.Class); // Immutable
			strategy.AttributesToIgnoreForMerging.Add(SharedConstants.GuidStr); // Immutable
			sharedElementStrategies.Add(SharedConstants.RtTag, strategy);
			strategy.ContextDescriptorGenerator = ContextGen;

			CreateSharedElementStrategies(sharedElementStrategies);
			CreateMergers(mdc, mergeSituation, sharedElementStrategies, mergers);
		}

		internal static void AddCustomPropInfo(MetadataCache mdc, MergeOrder mergeOrder, string customPropTargetDir, ushort levelsAboveCustomPropTargetDir)
		{
			if (mdc == null) throw new ArgumentNullException("mdc");
			if (mergeOrder == null) throw new ArgumentNullException("mergeOrder");
			if (String.IsNullOrEmpty(customPropTargetDir)) throw new ArgumentException(AnnotationImages.kInvalidArgument, "customPropTargetDir");

			// Add optional custom property information to MDC.
			string mainCustomPropPathname;
			string altCustomPropPathname;
			switch (mergeOrder.MergeSituation.ConflictHandlingMode)
			{
				default:
					mainCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToOurs);
					altCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToTheirs);
					break;
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					mainCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToTheirs);
					altCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToOurs);
					break;
			}
			mdc.AddCustomPropInfo(mainCustomPropPathname, altCustomPropPathname, customPropTargetDir, levelsAboveCustomPropTargetDir);
		}
	}
}