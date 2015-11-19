// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	///<summary>
	/// Class that holds some basic information about FDO classes.
	///</summary>
	internal sealed class FdoClassInfo
	{
		/// <summary>
		/// Get the class name.
		/// </summary>
		internal string ClassName { get; private set; }
		/// <summary>
		/// Check if class is abstract.
		/// </summary>
		internal bool IsAbstract { get; private set; }
		private readonly List<FdoPropertyInfo> _directProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoPropertyInfo> _allProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoPropertyInfo> _allReferenceSequenceProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoPropertyInfo> _allOwningSequenceProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoClassInfo> _allDirectSubclass = new List<FdoClassInfo>();
		private readonly List<FdoPropertyInfo> _allCollectionProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoPropertyInfo> _allReferenceCollectionProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoPropertyInfo> _allMultiAltProperties = new List<FdoPropertyInfo>();
		private readonly List<FdoPropertyInfo> _allOwningProperties = new List<FdoPropertyInfo>();

		internal FdoClassInfo(string className, string superclassName)
			: this(className, false, superclassName)
		{
		}

		internal FdoClassInfo(string className, bool isAbstract, string superclassName)
		{
			ClassName = className;
			IsAbstract = isAbstract;
			SuperclassName = superclassName;
		}

		public FdoClassInfo(FdoClassInfo superclass, string newClassName, bool isAbstract)
		{
			Superclass = superclass;
			ClassName = newClassName;
			IsAbstract = isAbstract;
		}

		internal void AddProperty(FdoPropertyInfo propertyinfo)
		{
			_directProperties.Add(propertyinfo);
		}

		internal void RemoveProperty(string propertyName)
		{
			_directProperties.Remove((from pi in _directProperties
										 where pi.PropertyName == propertyName
										 select pi).First());
		}

		internal FdoPropertyInfo GetProperty(string propertyName)
		{
			return (from propInfo in AllProperties
						  where propInfo.PropertyName == propertyName
						  select propInfo).FirstOrDefault();
		}

		/// <summary>
		/// Get a collection of all subclasses if this class (not including this class).
		/// </summary>
		internal IEnumerable<FdoClassInfo> AllSubclasses
		{
			get
			{
				var subclasses = new List<FdoClassInfo>(_allDirectSubclass);
				foreach (var subclass in _allDirectSubclass)
				{
					subclasses.AddRange(subclass.AllSubclasses);
				}
				return subclasses;
			}
		}

		internal bool IsOrInheritsFrom(string name)
		{
			for (var info = this; info != null; info = info.Superclass)
				if (info.ClassName == name)
					return true;
			return false;
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllProperties
		{
			get
			{
				return _allProperties;
			}
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllReferenceSequenceProperties
		{
			get
			{
				return _allReferenceSequenceProperties;
			}
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllOwningSequenceProperties
		{
			get
			{
				return _allOwningSequenceProperties;
			}
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllOwningProperties
		{
			get
			{
				return _allOwningProperties;
			}
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllCollectionProperties
		{
			get
			{
				return _allCollectionProperties;
			}
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllReferenceCollectionProperties
		{
			get
			{
				return _allReferenceCollectionProperties;
			}
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		internal IEnumerable<FdoPropertyInfo> AllMultiAltProperties
		{
			get
			{
				return _allMultiAltProperties;
			}
		}

		/// <summary>
		/// Get the superclass name.
		/// </summary>
		internal string SuperclassName { get; set; }

		/// <summary>
		/// Get the superclass.
		/// </summary>
		internal FdoClassInfo Superclass { get; set; }

		public void ResetCaches(Dictionary<string, FdoClassInfo> classes)
		{
			// No. _directProperties.Clear();
			_allProperties.Clear();
			_allDirectSubclass.Clear();
			_allCollectionProperties.Clear();
			_allReferenceCollectionProperties.Clear();
			_allMultiAltProperties.Clear();
			_allOwningProperties.Clear();
			_allReferenceSequenceProperties.Clear();
			_allOwningSequenceProperties.Clear();

			if (Superclass != null)
				_allProperties.AddRange(Superclass._allProperties);
			if (_directProperties.Count > 0)
			{
				_allProperties.AddRange(_directProperties);
			}

			if (_allProperties.Count > 0)
			{
				_allReferenceSequenceProperties.AddRange(from prop in _allProperties
														 where prop.DataType == DataType.ReferenceSequence
														 select prop);
				_allOwningSequenceProperties.AddRange(from prop in _allProperties
													  where prop.DataType == DataType.OwningSequence
													  select prop);
				_allCollectionProperties.AddRange(from prop in _allProperties
												  where prop.DataType == DataType.OwningCollection || prop.DataType == DataType.ReferenceCollection
												  select prop);
				_allReferenceCollectionProperties.AddRange(from prop in _allCollectionProperties
														   where prop.DataType == DataType.ReferenceCollection
														   select prop);
				_allMultiAltProperties.AddRange(from prop in _allProperties
												where prop.DataType == DataType.MultiString || prop.DataType == DataType.MultiUnicode
												select prop);
				_allOwningProperties.AddRange(from prop in _allProperties
											  where prop.DataType == DataType.OwningAtomic || prop.DataType == DataType.OwningCollection || prop.DataType == DataType.OwningSequence
											  select prop);
			}

			classes.Remove(ClassName);
			_allDirectSubclass.AddRange(from classInfo in classes.Values
									where classInfo.Superclass == this
									select classInfo);
			foreach (var directSubclass in _allDirectSubclass)
				directSubclass.ResetCaches(classes);
		}

		public override string ToString()
		{
			return ClassName + ": " + IsAbstract;
		}
	}
}