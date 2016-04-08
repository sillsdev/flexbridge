// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using Chorus.VcsDrivers;
using Palaso.Progress;

namespace LfMergeBridge
{
	public interface ILfMergeBridgeDataProvider
	{
		RepositoryAddress Repo { get; }
		string ProjectFolderPath { get; }
		IProgress Progress { get; }
		string ProjectCode { get; }
		ILogger Logger { get; }
		string ChorusUserName { get; }
	}
}

