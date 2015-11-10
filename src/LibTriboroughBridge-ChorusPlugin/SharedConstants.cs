// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2015 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace LibTriboroughBridgeChorusPlugin
{
	public static class SharedConstants
	{
		public static readonly Encoding Utf8 = Encoding.UTF8;
		public const string dupid = "dupid";
		public const string OtherRepositories = "OtherRepositories";
		public const string FwXmlExtension = "." + FwXmlExtensionNoPeriod;
		public const string FwXmlExtensionNoPeriod = "fwdata";
		public const string FwLockExtension = ".lock";
		public const string FwXmlLockExtension = FwXmlExtension + FwLockExtension;
		public const string FwDb4oExtension = "." + FwDb4oExtensionNoPeriod;
		public const string FwDb4oExtensionNoPeriod = "fwdb";
	}
}