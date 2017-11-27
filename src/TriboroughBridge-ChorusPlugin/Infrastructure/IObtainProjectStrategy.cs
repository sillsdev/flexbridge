// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using LibTriboroughBridgeChorusPlugin;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// This interface definition is used for bridges that can create new FLEx language projects from an obtained repo.
	///
	/// Do not use this interface, if the implementation cannot be used to create a new language project.
	/// </summary>
	internal interface IObtainProjectStrategy
	{
		bool ProjectFilter(string repositoryLocation);
		string HubQuery { get; }
		bool IsRepositoryEmpty(string repositoryLocation);
		void FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository);
		void TellFlexAboutIt();
		ActionType SupportedActionType { get; }
	}
}