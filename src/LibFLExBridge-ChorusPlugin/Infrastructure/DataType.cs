// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// The types of data in the FDO properties.
	/// </summary>
	internal enum DataType
	{
		/// <summary></summary>
		OwningCollection,
		/// <summary></summary>
		ReferenceCollection,
		/// <summary></summary>
		OwningSequence,
		/// <summary></summary>
		ReferenceSequence,
		/// <summary></summary>
		OwningAtomic,
		/// <summary></summary>
		ReferenceAtomic,
		/// <summary></summary>
		MultiUnicode,
		/// <summary></summary>
		MultiString,
		/// <summary></summary>
		Unicode,
		/// <summary></summary>
		String, // TsString
		/// <summary></summary>
		Integer,
		/// <summary>Not used</summary>
		Float,
		/// <summary>Not used</summary>
		Numeric,
		/// <summary></summary>
		Boolean,
		/// <summary></summary>
		Time, // DateTime
		/// <summary></summary>
		GenDate,
		/// <summary></summary>
		Guid,
		/// <summary></summary>
		Binary,
		/// <summary></summary>
		TextPropBinary
	}
}