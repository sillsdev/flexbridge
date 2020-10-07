// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	internal interface IUpdateBranchHelperStrategy
	{
		double GetModelVersionFromBranchName(string branchName);
		string GetBranchNameFromModelVersion(string modelVersion);
		double GetModelVersionFromClone(string cloneLocation);
		string GetFullModelVersion(string cloneLocation);
	}
}