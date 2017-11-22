// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: License.htm file)

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	internal interface IUpdateBranchHelperStrategy
	{
		float GetModelVersionFromBranchName(string branchName);
		float GetModelVersionFromClone(string cloneLocation);
		string GetFullModelVersion(string cloneLocation);
	}
}