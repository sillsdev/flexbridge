// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel.Composition;
using Chorus;

namespace FLEx_ChorusPlugin.Infrastructure
{
	[Export(typeof(IChorusSystem))]
	public class FLExBridgeChorusSystem: ChorusSystem
	{
	}
}

