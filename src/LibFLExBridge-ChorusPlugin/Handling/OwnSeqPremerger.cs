// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.TextCorpus;

namespace LibFLExBridgeChorusPlugin.Handling
{
	/// <summary>
	/// This is the pre-merger for OwnSeq elements, but for now, the only special case is owned elements that are StTxtParas.
	/// </summary>
	internal sealed class OwnSeqPremerger : IPremerger
	{
		void IPremerger.Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			var keyElt = ours ?? theirs ?? ancestor;
			if (keyElt == null)
				return;
			var className = XmlUtilities.GetStringAttribute(keyElt, "class");
			if (className == "StTxtPara" || className == "ScrTxtPara")
			{
				StTxtParaPremerger.PreMergeStTxtPara(listener, ref ours, theirs, ancestor);
			}
		}
	}
}
