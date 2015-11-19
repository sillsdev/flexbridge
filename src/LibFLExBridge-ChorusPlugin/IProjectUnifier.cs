// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin
{
	public interface IProjectUnifier
	{
		void PutHumptyTogetherAgain(IProgress progress, bool verbose, string mainFilePathname);
	}
}

