// Copyright (c) 2015-2016  SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	internal enum FinalCloneResult
	{
		Cloned,
		ExistingCloneTargetFolder,
		FlexVersionIsTooOld,
        ChosenRepositoryIsEmpty
    }
}