using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal class MultiLingualStringsContextGenerator : FieldWorkObjectContextGenerator
	{
		//This array is used to determine which strings to look for and the order to search for them
		//which will be returned in the contextLabel
		private String[] m_valuesToCheckFor;

		private string m_objectNameForLabel = "";

		public MultiLingualStringsContextGenerator(string startOfContextLabel, params String[] valuesToGet)
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
			return m_objectNameForLabel + " '" + objectNameOrAbbr + "'";
		}

		protected string GetNameOrAbbreviationOrOther(XmlNode parent)
		{
			var dataToReturn = "";
			//var index = 0;
			foreach (var nodeName in m_valuesToCheckFor)
			{
				var nodeToCheck = parent.SelectSingleNode(nodeName);
				if (nodeToCheck == null)
					continue;

				dataToReturn = FirstNonBlankChildsData(nodeToCheck);
				if (!String.IsNullOrEmpty(dataToReturn))
					break;
			}
			if (String.IsNullOrEmpty(dataToReturn))
				return UnidentifiableLabel;
			else
				return dataToReturn;
		}
	}
}
