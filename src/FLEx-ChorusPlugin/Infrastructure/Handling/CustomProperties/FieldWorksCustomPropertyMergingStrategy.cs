using System.Collections.Generic;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.CustomProperties
{
	/// <summary>
	/// A merge strategy for FieldWorks 7.0 custom properties data.
	/// </summary>
	internal sealed class FieldWorksCustomPropertyMergingStrategy : IMergeStrategy
	{
		private const string CustomField = "CustomField";
		private readonly XmlMerger _merger;

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FieldWorksCustomPropertyMergingStrategy(MergeSituation mergeSituation)
		{
			_merger = new XmlMerger(mergeSituation);

			// Custom property declaration.
			var strategy = new ElementStrategy(false)
							{
								MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { SharedConstants.Name, SharedConstants.Class }),
								ContextDescriptorGenerator = new FieldWorksCustomPropertyContextGenerator(),
								IsAtomic = true,
								NumberOfChildren = NumberOfChildrenAllowed.Zero
							};
			_merger.MergeStrategies.SetStrategy(CustomField, strategy);
		}

		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return ourEntry == null
					? theirEntry.OuterXml
					: (theirEntry == null
						? ourEntry.OuterXml
						: _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml);
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			return _merger.MergeStrategies.GetElementStrategy(element);
		}

		#endregion
	}
}