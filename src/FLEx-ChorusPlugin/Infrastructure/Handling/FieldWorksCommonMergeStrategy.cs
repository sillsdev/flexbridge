using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class FieldWorksCommonMergeStrategy : IMergeStrategy
	{
		private readonly MergeSituation _mergeSituation;
		private readonly MetadataCache _mdc;
		private readonly XmlMerger _merger;

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FieldWorksCommonMergeStrategy(MergeSituation mergeSituation, MetadataCache mdc)
		{
			_mergeSituation = mergeSituation;
			_mdc = mdc;
			_merger = new XmlMerger(mergeSituation);
			FieldWorksMergeStrategyServices.BootstrapSystem(_mdc, _merger);
		}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			var extantNode = ourEntry ?? theirEntry ?? commonEntry;

			switch (extantNode.Name)
			{
				default:
					FieldWorksMergingServices.MergeTimestamps(ourEntry, theirEntry);
					break;
				case "ScrDraft":
					// Immutable, so common, if different.
					if ((ourEntry != null && ourEntry.OuterXml != commonEntry.OuterXml) || (theirEntry != null && theirEntry.OuterXml != commonEntry.OuterXml))
					{
						return commonEntry.OuterXml;
					}
					// I (RBR) don't think both can be null, but....
					if (ourEntry == null && theirEntry == null)
						return commonEntry.OuterXml;
					break;
				case SharedConstants.Header:
					if (ourEntry != null)
					{
						foreach (XmlNode headerChild in ourEntry.ChildNodes)
						{
							if (_mdc.GetClassInfo(headerChild.Name) == null)
							{
								// Not a class, as what is found in the Anthro file. Go another level deeper to the class data.
								// This node only has one child, and it is class data.
								var dataCarryingChild = headerChild.FirstChild;
								FieldWorksMergingServices.MergeTimestamps(dataCarryingChild, theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name).FirstChild);
							}
							else
							{
								FieldWorksMergingServices.MergeTimestamps(headerChild, theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name));
							}
						}
					}
					break;
			}

			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		#endregion
	}
}