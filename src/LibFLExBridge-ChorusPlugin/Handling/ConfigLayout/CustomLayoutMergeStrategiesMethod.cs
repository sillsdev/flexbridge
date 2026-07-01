// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using Chorus.merge.xml.generic;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	internal static class CustomLayoutMergeStrategiesMethod
	{
		internal static void AddElementStrategies(MergeStrategies mergeStrategies)
		{
			// Document root.
			mergeStrategies.SetStrategy("LayoutInventory", ElementStrategy.CreateSingletonElement());

			#region 'layoutType' and children.
			mergeStrategies.SetStrategy("layoutType", ElementStrategy.CreateSingletonElement());

			var elStrat = new ElementStrategy(false)
				{
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "class", "layout" }),
					NumberOfChildren = NumberOfChildrenAllowed.Zero,
					IsAtomic = true,
					ContextDescriptorGenerator = new FieldWorkCustomLayoutContextGenerator()
				};
			mergeStrategies.SetStrategy("configure", elStrat);
			#endregion 'layoutType' and children.

			#region 'layout' and children.
			elStrat = new ElementStrategy(false)
				{
					// LT-19237: The Data Notebook emits one <layout> per record type, all sharing the
					// same class/type/name and differing only by choiceGuid. Without choiceGuid in the
					// key the merger cannot tell record-type layouts apart and cross-matches them,
					// losing fields and duplicating layouts. Layouts without a choiceGuid all key on a
					// null value, so they continue to match each other exactly as before.
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "class", "type", "name", "choiceGuid" }),
					ContextDescriptorGenerator = new FieldWorkCustomLayoutContextGenerator()
				};
			mergeStrategies.SetStrategy("layout", elStrat);

			elStrat = new ElementStrategy(true)
				{
					MergePartnerFinder = new FindByKeyAttributeInList("ref"),
					IsAtomic = true
				};
			mergeStrategies.SetStrategy("part", elStrat);

			elStrat = new ElementStrategy(true)
				{
					//MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "class", "fieldType", "restrictions" }),
					MergePartnerFinder = new FindByKeyAttributeInList("combinedkey"),
					IsAtomic = true
				};
			mergeStrategies.SetStrategy("generate", elStrat);

			elStrat = new ElementStrategy(true)
				{
					MergePartnerFinder = new FindByKeyAttributeInList("name"),
					IsAtomic = true,
					NumberOfChildren = NumberOfChildrenAllowed.Zero
				};
			mergeStrategies.SetStrategy("sublayout", elStrat);
			#endregion 'layout' and children.
		}
	}
}