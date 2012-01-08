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

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			if (ourEntry.Name == SharedConstants.Header)
			{
				foreach (XmlNode headerChild in ourEntry.ChildNodes)
					FieldWorksMergingServices.PreMerge(true, eventListener, _mdc, headerChild, theirEntry.SelectSingleNode(headerChild.Name), commonEntry.SelectSingleNode(headerChild.Name));
			}
			else
			{
				FieldWorksMergingServices.PreMerge(true, eventListener, _mdc, ourEntry, theirEntry, commonEntry);
			}

			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		#endregion
	}
}