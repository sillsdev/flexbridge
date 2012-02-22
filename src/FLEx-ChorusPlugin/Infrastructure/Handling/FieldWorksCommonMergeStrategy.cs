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
		internal FieldWorksCommonMergeStrategy(MergeSituation mergeSituation, MetadataCache mdc)
		{
			_mdc = mdc;
			_merger = new XmlMerger(mergeSituation);
			FieldWorksMergeStrategyServices.BootstrapSystem(_mdc, _merger);
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

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			FieldWorksMergingServices.PreMerge(_mdc, ourEntry, theirEntry);

			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		#endregion
	}
}