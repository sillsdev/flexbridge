// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// This interface definition is used for bridges that can create new FLEx language projects from an obtained repo.
	///
	/// Do not use this interface, if the implemnentattion cannot be used to create a new language project.
	/// </summary>
	public interface IObtainProjectStrategy
	{
		bool ProjectFilter(string repositoryLocation);
		string HubQuery { get; }
		bool IsRepositoryEmpty(string repositoryLocation);
		void FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository);
		void TellFlexAboutIt();
		ActionType SupportedActionType { get; }
	}
}