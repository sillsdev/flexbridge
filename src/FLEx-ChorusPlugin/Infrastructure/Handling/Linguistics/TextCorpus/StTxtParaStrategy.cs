using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus
{
	/// <summary>
	/// A strategy for merging (atomic) StTxtPara elements.
	/// There are currently none of these in the model. But this will handle them if they happen.
	/// It also seemed a more logical place to put the logic for handling them, though the real way it gets invoked is
	/// through a case in OwnSeqStrategy.
	/// </summary>
	internal class StTxtParaStrategy : ElementStrategy
	{
		internal StTxtParaStrategy(bool orderIsRelevant)
			: base(orderIsRelevant)
		{
		}

		public StTxtParaStrategy() : this(false)
		{

		}

		public override void PreMerge(XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			base.PreMerge(ours, theirs, ancestor); // currently does nothing, but may be safer to do it than not.
			PreMergeStTxtPara(ours, theirs, ancestor);
		}

		internal static void PreMergeStTxtPara(XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (AnyChanges(ours, theirs, ancestor))
			{
				MakeParseIsCurrentFalse(ours);
				MakeParseIsCurrentFalse(theirs);
				MakeParseIsCurrentFalse(ancestor);
			}
		}

		private static void MakeParseIsCurrentFalse(System.Xml.XmlNode node)
		{
			if (node == null)
				return;
			var parseIsCurrent = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(child => child.Name == "ParseIsCurrent") as XmlElement;
			if (parseIsCurrent == null)
				return; // already implicitly false
			parseIsCurrent.SetAttribute(@"val", @"False");
		}

		private static bool AnyChanges(System.Xml.XmlNode ours, System.Xml.XmlNode theirs, System.Xml.XmlNode ancestor)
		{
			if (ancestor == null)
			{
				if (ours == null)
					return false; // they added, merge won't mess with it.
				else if (theirs == null)
					return false; // we added, merge won't mess with it
				else
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
