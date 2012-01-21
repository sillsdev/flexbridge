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
			var extantNode = ourEntry ?? theirEntry ?? commonEntry;
			if (extantNode.Name == "ScrDraft")
			{
				// Immutable, so common, if different.
				if ((ourEntry != null && ourEntry.OuterXml != commonEntry.OuterXml) || (theirEntry != null && theirEntry.OuterXml != commonEntry.OuterXml))
				{
					return commonEntry.OuterXml;
				}
				// I (RBR) don't think both can be null, but....
				if (ourEntry == null && theirEntry == null)
					return commonEntry.OuterXml;
			}
			if (extantNode.Name == SharedConstants.Header)
			{
				if (ourEntry != null)
				{
					foreach (XmlNode headerChild in ourEntry.ChildNodes)
						FieldWorksMergingServices.PreMerge(true, eventListener, _mdc, headerChild, theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name), commonEntry.SelectSingleNode(headerChild.Name));
				}
				else
				{
					foreach (XmlNode headerChild in theirEntry.ChildNodes)
						FieldWorksMergingServices.PreMerge(true, eventListener, _mdc, null, headerChild, commonEntry.SelectSingleNode(headerChild.Name));
				}
			}
			else
			{
				FieldWorksMergingServices.PreMerge(true, eventListener, _mdc, ourEntry, theirEntry, commonEntry);
			}

			if (ourEntry == null)
				return theirEntry.OuterXml;
			if (theirEntry == null)
				return ourEntry.OuterXml;
			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		#endregion
	}
}