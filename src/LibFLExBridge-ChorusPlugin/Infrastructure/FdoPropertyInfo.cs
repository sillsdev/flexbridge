// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// Property information for an FDO property.
	/// </summary>
	internal sealed class FdoPropertyInfo
	{
		private readonly Dictionary<string, string> _allPropertyValues = new Dictionary<string, string>();

		/// <summary>
		/// Get the name of the property.
		/// </summary>
		internal string PropertyName
		{
			get { return _allPropertyValues["name"]; }
		}

		/// <summary>
		/// Get the data type of the property.
		/// </summary>
		internal DataType DataType { get { return (DataType)Enum.Parse(typeof (DataType), _allPropertyValues["type"]); } }

		/// <summary>
		/// See if the property is custom or standard.
		/// </summary>
		internal bool IsCustomProperty { get; private set; }

		/// <summary>
		/// Get a listing of all known information for the property.
		/// The dictionary 'key' is the attribute name in the stried xml. The dictionary 'value' is the contents of the attribute value.
		/// </summary>
		internal Dictionary<string, string> AllPropertyValues
		{
			get { return new Dictionary<string, string>(_allPropertyValues); }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FdoPropertyInfo(string propertyName, DataType dataType)
			: this(propertyName, dataType, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FdoPropertyInfo(string propertyName, DataType dataType, bool isCustomProperty)
		{
			_allPropertyValues["name"] = propertyName;
			_allPropertyValues["type"] = dataType.ToString();
			IsCustomProperty = isCustomProperty;
		}

		/// <summary>
		/// Constructor for custom properties.
		/// </summary>
		internal FdoPropertyInfo(XElement customPropertyDefinition)
		{
			IsCustomProperty = true;
			foreach (var attr in customPropertyDefinition.Attributes().Where(attr => attr.Name.LocalName != "key"))
			{
				_allPropertyValues[attr.Name.LocalName] = attr.Value;
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FdoPropertyInfo(string propertyName, string dataType, bool isCustomProperty)
		{
			_allPropertyValues["name"] = propertyName;
			_allPropertyValues["type"] = dataType;
			IsCustomProperty = isCustomProperty;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1} user-defined: {2}", PropertyName, DataType, IsCustomProperty);
		}
	}
}