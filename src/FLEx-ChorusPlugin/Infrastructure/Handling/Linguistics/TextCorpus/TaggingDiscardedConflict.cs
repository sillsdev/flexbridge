using System.Xml;
using Chorus.VcsDrivers;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus
{
	[TypeGuid("4ef51287-2bc2-4be5-8e6d-1022d46d6d27")]
	public class TaggingDiscardedConflict : Conflict
	{
		public TaggingDiscardedConflict(MergeSituation situation) : base(situation)
		{
		}

		public TaggingDiscardedConflict(XmlNode xmlRepresentation) : base(xmlRepresentation)
		{

		}

		public override string GetFullHumanReadableDescription()
		{
			return
				string.Format(
					"{0} changed text tagging on this text, while {1} edited the base text. Some of the tagging changes could not be resolved and were discarded.",
					Situation.BetaUserId, Situation.AlphaUserId);
		}

		/// <summary>
		/// There's no useful information we can provide about this/that/ancestor context.
		/// </summary>
		/// <param name="fileRetriever"></param>
		/// <param name="mergeSource"></param>
		/// <returns></returns>
		public override string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			return "";
		}

		public override string Description
		{
			get { return "Tagging discarded conflict"; }
		}
	}
}
