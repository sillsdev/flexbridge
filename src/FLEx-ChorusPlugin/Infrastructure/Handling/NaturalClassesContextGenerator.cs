using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal class NaturalClassesContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(System.Xml.XmlNode start)
		{
			return GetLabelForNaturalClass(start);
		}

		string NaturalClassLabel
		{
			get { return "Natural Class"; } // Todo: internationalize
		}


		private string GetLabelForNaturalClass(XmlNode naturalClass)
		{
			var naturalClassName = UnidentifiableLabel;

			if (naturalClass != null)
			{
				naturalClassName = GetNameOrAbbreviation(naturalClass);
			}
			return NaturalClassLabel + " '" + naturalClassName + "'";
		}
	}
}
