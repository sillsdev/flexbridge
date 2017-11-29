// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Text;

namespace LibTriboroughBridgeChorusPlugin
{
	internal static class LibTriboroughBridgeSharedConstants
	{
		internal static readonly Encoding Utf8 = Encoding.UTF8;
		internal const string dupid = "dupid";
		internal const string OtherRepositories = "OtherRepositories";
		internal const string FwXmlExtension = "." + FwXmlExtensionNoPeriod;
		internal const string FwXmlExtensionNoPeriod = "fwdata";
		internal const string FwLockExtension = ".lock";
		internal const string FwXmlLockExtension = FwXmlExtension + FwLockExtension;
		internal const string FwDb4oExtension = "." + FwDb4oExtensionNoPeriod;
		internal const string FwDb4oExtensionNoPeriod = "fwdb";
		internal const string Default = "default";
	}
}