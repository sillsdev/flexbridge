// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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
					MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "class", "type", "name" }),
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