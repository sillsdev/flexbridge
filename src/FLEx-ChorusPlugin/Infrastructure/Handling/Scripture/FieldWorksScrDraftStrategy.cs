using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	internal sealed class FieldWorksScrDraftStrategy : IMergeStrategy
	{
		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			//// Immutable, so common, if different.
			return commonEntry != null
				? (ourEntry == null || theirEntry == null)
						? null // Somebody deleted it, and we don't care who.
						: commonEntry.OuterXml // We don't really care what the other two may, or may not, have done.
					: (ourEntry == null && theirEntry != null
						? theirEntry.OuterXml // commonEntry and ourEntry are null, so they added it.
						: ourEntry.OuterXml); // Looks like we added it.
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			var strategy = new ElementStrategy(false);

			return strategy;
		}

		#endregion
	}
}