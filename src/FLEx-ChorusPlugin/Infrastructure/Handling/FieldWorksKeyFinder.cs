using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using Palaso.Code;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal class FieldWorksKeyFinder : IKeyFinder
	{
		#region Implementation of IKeyFinder

		public string GetKeyFromElement(IEnumerable<string> keys, XmlNode element)
		{
			Guard.AgainstNull(keys, "The 'keys' parameter is null.");
			Guard.AgainstNull(element, "The 'element' parameter is null.");

			var key = element.Name;
			var oddElementNames = new HashSet<string>
										{
											SharedConstants.Refcol,
											SharedConstants.Ownseq,
											SharedConstants.OwnseqAtomic,
											SharedConstants.Refseq,
											SharedConstants.CmAnnotation,
											SharedConstants.DsChart,
											SharedConstants.curiosity
										};
			var keysList = new HashSet<string>(keys);
			switch (key)
			{
				default:
					// This really does stink, but I'm (RBR) not sure how to avoid it today!
					// This came in Chorus' 776 (6b202d27705c) but is FW stuff.
					if (keysList.Contains(key) || element.ParentNode == null)
						return key;
					// Combine parent name + element name as key (for new styled FW properties).
					var combinedKey = oddElementNames.Contains(element.ParentNode.Name)
						? element.ParentNode.Attributes["class"].Value + "_" + key
						: element.ParentNode.Name + "_" + key;
					if (keysList.Contains(combinedKey))
						return combinedKey;
					break;
				// Another special FW situation.
				case "Custom":
					var customPropName = element.Attributes["name"].Value;
					var className = element.ParentNode.Name;
					if (oddElementNames.Contains(className))
						className = element.ParentNode.Attributes["class"].Value;
					var combinedCustomKey = key + "_" + className + "_" + customPropName;
					if (keysList.Contains(combinedCustomKey))
						return combinedCustomKey;
					break;
			}

			return key;
		}

		#endregion
	}
}