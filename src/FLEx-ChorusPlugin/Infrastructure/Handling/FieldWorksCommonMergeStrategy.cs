using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class FieldWorksCommonMergeStrategy : IMergeStrategy
	{
		private readonly MetadataCache _mdc;
		private readonly XmlMerger _merger;

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FieldWorksCommonMergeStrategy(MergeOrder mergeOrder, MetadataCache mdc)
		{
			_mdc = mdc;
			_merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(mergeOrder, mdc);
		}

		/// <summary>
		/// Constructor ONLY used by FieldWorksHeaderedMergeStrategy.
		/// </summary>
		internal FieldWorksCommonMergeStrategy(XmlMerger merger, MetadataCache mdc)
		{
			_mdc = mdc;
			_merger = merger;
		}

		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			FieldWorksMergingServices.PreMerge(_mdc, ourEntry, theirEntry, commonEntry);

			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
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

		public MergeStrategies GetStrategies()
		{
			return _merger.MergeStrategies;
		}

		#endregion
	}
}