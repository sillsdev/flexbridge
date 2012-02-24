﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2012, SIL International. All Rights Reserved.
// <copyright from='2012' to='2012' company='SIL International'>
//		Copyright (c) 2012, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: EnvironmentContextGenerator.cs
// Responsibility: lastufka
// ---------------------------------------------------------------------------------------------

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Context generator for Phonological Environment elements.
	/// </summary>
	class EnvironmentContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(System.Xml.XmlNode start)
		{
			return GetLabelForEnvironment(start);
		}

		string EnvName
		{
			get { return "Environment"; } // Todo: internationalize
		}

		private string GetLabelForEnvironment(XmlNode entry)
		{
			var name = entry.SelectSingleNode("Name/AUni");
			if (name != null)
				return EnvName + " " + name.InnerText;
			var rep = entry.SelectSingleNode("StringRepresentation/Str");
			if (rep != null)
				return EnvName + " " + rep.InnerText;
			return EnvName + " with no name or representation";
		}
	}
}
