using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// The order of the possibilities in CmPossibilityList.Possibilities or CmPossibility.SubPossibilities
	/// is significant only if the list is not sorted. This class implements that policy.
	/// </summary>
	class PossibilityListOrderPolicy : IChildOrderPolicy
	{
		public ChildOrder OrderSignificance(XmlNode parent)
		{
			var listNode = parent.ParentNode;
			while (listNode != null && listNode.Name != "CmPossibilityList")
				listNode = listNode.ParentNode;
			var listElt = listNode as XmlElement;
			if (listElt == null)
				return ChildOrder.Significant; // should never happen, but this is the safe default for a sequence.
			var sorted = listElt.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.Name == "IsSorted");
			if (sorted == null)
				return ChildOrder.Significant; // missing value is obsolete, but means false if it happens
			var val = XmlUtilities.GetStringAttribute(sorted, "val");
			return val == "True" ? ChildOrder.NotSignificant : ChildOrder.Significant;
		}
	}
}
