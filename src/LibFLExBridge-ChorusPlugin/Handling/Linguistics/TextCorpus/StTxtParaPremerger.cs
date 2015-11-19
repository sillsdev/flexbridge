// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Xml;
using Chorus.merge.xml.generic;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.TextCorpus
{
	/// <summary>
	/// A pre-merger for merging (atomic) StTxtPara elements.
	/// There are currently none of these in the model. But this will handle them if they happen.
	/// It also seemed a more logical place to put the logic for handling them, though the real way it gets invoked is
	/// through a case in OwnSeqPremerger.
	/// </summary>
	internal class StTxtParaPremerger : IPremerger
	{
		public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			PreMergeStTxtPara(listener, ref ours, theirs, ancestor);
		}

		internal static void PreMergeStTxtPara(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (!AnyChanges(ours, theirs, ancestor))
				return;

			MakeParseIsCurrentFalse(ours);
			MakeParseIsCurrentFalse(theirs);
			MakeParseIsCurrentFalse(ancestor);
		}

		private static void MakeParseIsCurrentFalse(XmlNode node)
		{
			if (node == null)
				return;
			var parseIsCurrent = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(child => child.Name == "ParseIsCurrent") as XmlElement;
			if (parseIsCurrent == null)
				return; // already implicitly false

			parseIsCurrent.SetAttribute(@"val", @"False");
		}

		private static bool AnyChanges(XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (ancestor == null)
			{
				if (ours == null)
					return false; // they added, merge won't mess with it.

				if (theirs == null)
					return false; // we added, merge won't mess with it

				return !XmlUtilities.AreXmlElementsEqual(ours, theirs); // somehow we both added, problem unless somehow identical
			}

			// ancestor is not null.
			if (ours == null)
				return theirs != null; // we deleted, if they didn't there's a difference.
			if (theirs == null)
				return true; // they deleted, we didn't, that's a difference.

			return !XmlUtilities.AreXmlElementsEqual(ours, theirs);
		}
	}
}
