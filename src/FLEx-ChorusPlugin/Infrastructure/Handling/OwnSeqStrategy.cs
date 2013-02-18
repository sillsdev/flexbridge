using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// This is the strategy for OwnSeq elements, but for now, the only special case is owned elements that are StTxtParas.
	/// </summary>
	class OwnSeqStrategy : ElementStrategy
	{
		public OwnSeqStrategy() : base(true)
		{
			MergePartnerFinder = new FindByKeyAttribute(@"guid");
		}
		public override void PreMerge(System.Xml.XmlNode ours, System.Xml.XmlNode theirs, System.Xml.XmlNode ancestor)
		{
			base.PreMerge(ours, theirs, ancestor);
			var keyElt = ours ?? theirs ?? ancestor;
			if (keyElt == null)
				return;
			var className = XmlUtilities.GetStringAttribute(keyElt, "class");
			if (className == "StTxtPara" || className == "ScrTxtPara")
			{
				StTxtParaStrategy.PreMergeStTxtPara(ours, theirs, ancestor);
			}
		}
	}
}
