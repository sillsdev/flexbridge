using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class FieldWorksHeaderedMergeStrategy : IMergeStrategy
	{
		private readonly MetadataCache _mdc;
		private readonly XmlMerger _merger;

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FieldWorksHeaderedMergeStrategy(MergeSituation mergeSituation, MetadataCache mdc)
		{
			_mdc = mdc;
			_merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(mergeSituation, mdc);
		}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			var extantNode = ourEntry ?? theirEntry ?? commonEntry;
			if (extantNode.Name != SharedConstants.Header)
			{
				var commonStrategy = new FieldWorksCommonMergeStrategy(_merger, _mdc);
				return commonStrategy.MakeMergedEntry(eventListener, ourEntry, theirEntry, commonEntry);
			}

			if (ourEntry != null)
			{
				foreach (XmlNode headerChild in ourEntry.ChildNodes)
				{
					if (_mdc.GetClassInfo(headerChild.Name) == null)
					{
						// Not a class, as what is found in the Anthro file. Go another level deeper to the class data.
						// This node only has one child, and it is class data.
						var dataCarryingChild = headerChild.FirstChild;
						FieldWorksMergingServices.PreMerge(_mdc,
							dataCarryingChild,
							theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name).FirstChild);
					}
					else
					{
						FieldWorksMergingServices.PreMerge(_mdc,
							headerChild,
							theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name));
					}
				}
			}

			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		#endregion
	}
}