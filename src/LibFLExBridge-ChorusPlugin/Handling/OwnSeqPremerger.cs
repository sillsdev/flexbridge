// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.TextCorpus;

namespace LibFLExBridgeChorusPlugin.Handling
{
	/// <summary>
	/// This is the pre-merger for OwnSeq elements, but for now, the only special case is owned elements that are StTxtParas.
	/// </summary>
	internal class OwnSeqPremerger : IPremerger
	{
		public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
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
