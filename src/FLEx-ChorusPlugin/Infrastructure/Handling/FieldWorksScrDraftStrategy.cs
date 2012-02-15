using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class FieldWorksScrDraftStrategy : IMergeStrategy
	{
		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			//// Immutable, so common, if different.
			return commonEntry != null
					? commonEntry.OuterXml // We don't really care what the other two may, or may not, have done.
					: (ourEntry == null && theirEntry != null
						? theirEntry.OuterXml // commonEntry and ourEntry are null, so they added it.
						: ourEntry.OuterXml); // Looks like we added it.
		}

		#endregion
	}
}