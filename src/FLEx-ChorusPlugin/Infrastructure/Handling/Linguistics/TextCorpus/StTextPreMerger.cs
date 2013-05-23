using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus
{
	/// <summary>
	/// This class handles problems of TextTags that are made invalid because the other user changed the text.
	/// Note that this strategy assumes WeWin (that is, if both modified something, ours will be chosen).
	/// If this stops being true, we will have to enhance Chorus to pass in the MergeSituation or at least
	/// the ConflictHandlingMode.
	/// </summary>
	internal class StTextPreMerger : IPremergerEx
	{
		Dictionary<XmlNode, int> _beginOffsetCorrections = new Dictionary<XmlNode, int>();

		public void Premerge(MergeSituation situation, IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			_beginOffsetCorrections.Clear();
			if (ours == null || theirs == null)
				return; // can't have conflicting changes.
			string baseText = null;
			if (ancestor != null)
				baseText = GetContent(ancestor);
			string ourText = GetContent(ours);
			string theirText = GetContent(theirs);
			if (theirText == ourText)
				return; // all annotations are consistent with the new (or unchanged) version of the text.
			var theirTags = GetTags(theirs);
			var ourTags = GetTags(ours);
			bool ourTextWins;
			if (situation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
			{
				// Given a WeWin strategy, and we already know ours and theirs are different,
				// the only way the end result is not ourText is if we made no change (and hence their change happens).
				ourTextWins = ourText != baseText;
			}
			else
			{
				Debug.Assert(situation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.TheyWin,
					"Don't know how to handle other conflict modes");
				// They win unless they made no change.
				ourTextWins = theirText == baseText;
			}
			if (ourTextWins)
			{
				AdjustCorrespondingSegs(ours, theirs);
				FixTags(situation, listener, theirTags, ourTags, theirText, ourText);
			}
			else
			{
				AdjustCorrespondingSegs(theirs, ours);
				FixTags(situation, listener, ourTags, theirTags, ourText, theirText);
			}
			// Todo: also need to deal with ConstChartWordGroup.
		}

		/// <summary>
		/// Any segment which exists in both should have the offset in losingText changed to the
		/// one in winningText. That is, we keep the adjustment of the segment which corresponds
		/// to the winning baseline. This allows things to survive in segments where an edit occurred
		/// in the other branch but only before the start of the segment.
		/// </summary>
		/// <param name="winningText"></param>
		/// <param name="losingText"></param>
		private void AdjustCorrespondingSegs(XmlNode winningText, XmlNode losingText)
		{
			var winningSegs = winningText.SelectNodes("Paragraphs/ownseq/Segments/ownseq").Cast<XmlNode>().ToList();
			var losingSegs = losingText.SelectNodes("Paragraphs/ownseq/Segments/ownseq").Cast<XmlNode>().ToList();
			foreach (var seg in losingSegs)
			{
				var guid = XmlUtilities.GetStringAttribute(seg, "guid");
				var winningSeg =
					(from s in winningSegs where XmlUtilities.GetStringAttribute(s, "guid") == guid select s).FirstOrDefault();
				if (winningSeg == null)
					continue; // can't adjust
				int losingOffset = GetOffset(seg);
				int winningOffset = GetOffset(winningSeg);
				if (losingOffset == winningOffset)
					continue;
				_beginOffsetCorrections[seg] = losingOffset;
				var beginOffsetElt = NamedChild(seg, "BeginOffset");
				((XmlElement) beginOffsetElt).SetAttribute("val", winningOffset.ToString());
			}
		}

		/// <summary>
		/// Does nothing (we need to premerge after we have a context, and with a MergeSituation).
		/// </summary>
		/// <param name="situation"></param>
		/// <param name="listener"></param>
		/// <param name="ours"></param>
		/// <param name="theirs"></param>
		/// <param name="ancestor"></param>
		public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{

		}

		private void FixTags(MergeSituation situation, IMergeEventListener listener,
			List<XmlNode> losingTags, List<XmlNode> winningTags, string losingText, string winningText)
		{
			if (!losingTags.Any())
				return; // none to fix
			int ctagsDiscarded = 0;
			foreach (var tag in losingTags.ToArray())
			{
				if (GetText(tag, "Begin", losingText, true) != GetText(tag, "Begin", winningText, false)
					|| GetText(tag, "End", losingText, true) != GetText(tag, "End", winningText, false))
				{
					// This tag cannot be allowed to win; it is invalid with respect to what the
					// text is being changed to.
					ctagsDiscarded++;
					var guid = XmlUtilities.GetStringAttribute(tag, "guid");
					var partner =
						(from x in winningTags where XmlUtilities.GetStringAttribute(x, "guid") == guid select x).FirstOrDefault();
					if (partner == null)
					{
						// Tag not in the other branch: either deleted there, or added here.
						// In either case we can just remove it.
						tag.ParentNode.RemoveChild(tag);
					}
					else
					{
						// The problem node has a partner in the other branch. If we just remove it, we may create
						// a case that looks to the merger as if it was deleted in this branch, unchanged in the other,
						// and it will simply be deleted. Replace it with a copy of the other, so that wins.
						tag.ParentNode.ReplaceChild(tag.OwnerDocument.ImportNode(partner, true), tag);
					}
				}
			}
			if (ctagsDiscarded > 0)
			{
				var conflict = new TaggingDiscardedConflict(situation);
				listener.RecordContextInConflict(conflict);
				var sb = new StringBuilder();
				sb.Append("<body>");
				AppendUserVersionOfText(winningText, sb, situation.AlphaUserId);
				AppendUserVersionOfText(losingText, sb, situation.BetaUserId);
				sb.AppendFormat("<div>Tagging may have been lost in any of the changed sentences. {0} tags were discarded. </div>",
					ctagsDiscarded);
				sb.Append("</body>");
				conflict.HtmlDetails = sb.ToString();
				listener.ConflictOccurred(conflict);
			}
		}

		private static void AppendUserVersionOfText(string winningText, StringBuilder sb, string userId)
		{
			sb.AppendFormat("<div>The paragraph contents in {0}'s version of the paragraph was </div>", userId);
			sb.Append("<div>");
			sb.Append(winningText);
			sb.Append("</div>");
		}

		/// <summary>
		/// Get the text that the specified tag would have, from the start of its segment to
		/// the end of its 'word', if the baseline text of its paragaph were the input baseline.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="baseline"></param>
		/// <param name="useOldOffset"></param>
		/// <returns></returns>
		private string GetText(XmlNode tag, string beginOrEnd, string baseline, bool useOldOffset)
		{
			var segChild = NamedChild(tag, beginOrEnd + "Segment"); // e.g., the BeginSegment element
			if (segChild == null)
				return ""; // maybe one end is not specified?
			var segGuid = GetAttrOfChild(segChild, "objsur", "guid");
			var stText = tag.ParentNode.ParentNode;
			var seg = stText.SelectSingleNode("Paragraphs/ownseq/Segments/ownseq[@guid='" + segGuid + "']");
			if (seg == null)
				return ""; // maybe do something to force it to be deleted?
			int offset = GetOffset(seg);
			if (useOldOffset)
			{
				int oldOffset;
				if (_beginOffsetCorrections.TryGetValue(seg, out oldOffset))
					offset = oldOffset;
			}
			string segText = baseline.Substring(offset); // actually the segment may now be shorter than this.
			int index = GetIndex(tag, beginOrEnd, "AnalysisIndex");
			return TextToEndOfNthAnalysis(segText, index);
		}

		private int GetIndex(XmlNode tag, string beginOrEnd, string attrName)
		{
			var val = GetAttrOfChild(tag, beginOrEnd + attrName, "val");
			if (val == null)
				return 0;
			int result;
			int.TryParse(val, out result);
			return result;
		}

		/// <summary>
		/// This is an approximation of what Flex's ParagraphAnalysisFinder's behavior does to break a string into analyses.
		/// It doesn't know about not treating verse and chapter numbers as word-forming.
		/// It doesn't know about any non-standard character properties.
		/// This is an approximation. If it fails, it will almost certainly fail in the
		/// direction of not letting us keep a tag that we could have, in a segment that
		/// someone else edited. The best we can reasonably achieve is to keep many of them,
		/// at least until we merge FlexBridge into FLEx and can share the code. Even then, to use
		/// the FDO code we'd have to reconstitute the TsString.
		/// </summary>
		/// <param name="segText"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		string TextToEndOfNthAnalysis(string segText, int index)
		{
			int ich = 0;
			ich = AdvanceToWord(segText, ich);
			// now at start of word 0.
			int word = 0;
			ich = AdvancePastWord(segText, ich);
			// now at end of word 0
			while (word < index && ich < segText.Length)
			{
				ich = AdvanceToWord(segText, ich);
				ich = AdvancePastWord(segText, ich);
				word++;
			}
			return segText.Substring(0, ich);
		}

		private int AdvanceToWord(string segText, int ich)
		{
			while (ich < segText.Length && Char.IsWhiteSpace(segText[ich]))
				ich++;
			return ich;
		}

		private int AdvancePastWord(string segText, int ich)
		{
			if (ich >= segText.Length)
				return ich; // can't advance, caller better notice!
			if (IsWordForming(segText, ich))
			{
				// Make a token that is all wordforming characters
				while (ich < segText.Length && IsWordForming(segText, ich))
					ich++;
				return ich;
			}
			// Make a token that has no wordforming characters, that is, up to the next
			// whitespace or wordforming character.
			while (ich < segText.Length && !Char.IsWhiteSpace(segText, ich) && !IsWordForming(segText, ich))
				ich++;
			return ich;
		}

		/// <summary>
		/// This combines the logic of LgIcuCharPropEngine.get_IsWordForming() with that of WordMaker.IsWordforming()
		/// </summary>
		/// <param name="segText"></param>
		/// <param name="ich"></param>
		/// <returns></returns>
		bool IsWordForming(string segText, int ich)
		{
			var ch = segText[ich];
			if(Char.IsLetter(ch) || Char.IsNumber(ch))
			return true;
			var cat = Char.GetUnicodeCategory(ch);
			return cat == UnicodeCategory.NonSpacingMark
				|| cat == UnicodeCategory.SpacingCombiningMark
				|| cat == UnicodeCategory.ModifierSymbol;
		}

		int GetOffset(XmlNode seg)
		{
			string val = GetAttrOfChild(seg, "BeginOffset", "val");
			if (val == null)
				return 0; // should not happen?
			int result;
			int.TryParse(val, out result);
			return result;
		}

		string GetAttrOfChild(XmlNode input, string childName, string attr)
		{
			var node = NamedChild(input, childName);
			if (node == null)
				return null;
			return XmlUtilities.GetStringAttribute(node, attr);
		}

		/// <summary>
		/// Get a child of the input with the specified name.
		/// Returns null if there isn't one (or if the input is null, to facilitate chaining)
		/// </summary>
		/// <param name="input"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		XmlNode NamedChild(XmlNode input, string name)
		{
			if (input == null)
				return null;
			return (from XmlNode n in input.ChildNodes where n.Name == name select n).FirstOrDefault();
		}


		/// <summary>
		/// from an StText, get the concatenated content of all the runs.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		string GetContent(XmlNode stText)
		{
			var runs = stText.SelectNodes("Paragraphs/ownseq/Contents/Str/Run");
			StringBuilder sb = new StringBuilder();
			foreach (XmlNode run in runs)
				sb.Append(run.InnerText);
			return sb.ToString();
		}

		List<XmlNode> GetTags(XmlNode stText)
		{
			return stText.SelectNodes("Tags/TextTag").Cast<XmlNode>().ToList();
		}
	}
}
