using System.Collections.Generic;
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
		internal FieldWorksHeaderedMergeStrategy(MergeOrder mergeOrder, MetadataCache mdc)
		{
			_mdc = mdc;
			_merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(mergeOrder, mdc);
		}

		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
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
							theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name).FirstChild,
							commonEntry == null ? null : commonEntry.SelectSingleNode(headerChild.Name).FirstChild);
					}
					else
					{
						FieldWorksMergingServices.PreMerge(_mdc,
							headerChild,
							theirEntry == null ? null : theirEntry.SelectSingleNode(headerChild.Name),
							commonEntry == null ? null : commonEntry.SelectSingleNode(headerChild.Name));
					}
				}
			}

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

		/// <summary>
		/// FieldWorks never mixes elements and text in the same parent, so there is no need to suppress indenting.
		/// </summary>
		/// <returns></returns>
		public HashSet<string> SuppressIndentingChildren()
		{
			return new HashSet<string>();
		}

		#endregion
	}
}