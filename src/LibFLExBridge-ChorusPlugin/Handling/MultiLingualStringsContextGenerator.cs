// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;

namespace LibFLExBridgeChorusPlugin.Handling
{
	internal sealed class MultiLingualStringsContextGenerator : FieldWorkObjectContextGenerator
	{
		//This array is used to determine which strings to look for and the order to search for them
		//which will be returned in the contextLabel
		private readonly string[] m_valuesToCheckFor;

		private readonly string m_objectNameForLabel = "";

		internal MultiLingualStringsContextGenerator(string startOfContextLabel, params string[] valuesToGet)
		{
			m_objectNameForLabel = startOfContextLabel;
			m_valuesToCheckFor = valuesToGet;
		}

		protected override string GetLabel(System.Xml.XmlNode startNode)
		{
			return GetContextLabelToDisplay(startNode);
		}

		private string GetContextLabelToDisplay(XmlNode startNode)
		{
			string objectNameOrAbbr = UnidentifiableLabel;

			if (startNode != null)
			{
				objectNameOrAbbr = GetNameOrAbbreviationOrOther(startNode);
			}
			return m_objectNameForLabel + Space + Quote + objectNameOrAbbr + Quote;
		}

		private string GetNameOrAbbreviationOrOther(XmlNode parent)
		{
			var dataToReturn = "";
			//var index = 0;
			foreach (var nodeName in m_valuesToCheckFor)
			{
				var nodeToCheck = parent.SelectSingleNode(nodeName);
				if (nodeToCheck == null)
					continue;

				dataToReturn = FirstNonBlankChildsData(nodeToCheck);
				if (!string.IsNullOrEmpty(dataToReturn))
					break;
			}
			return string.IsNullOrEmpty(dataToReturn) ? UnidentifiableLabel : dataToReturn;
		}
	}
}
