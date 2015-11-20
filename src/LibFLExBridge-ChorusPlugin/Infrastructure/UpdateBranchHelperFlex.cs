// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using LibTriboroughBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	public class UpdateBranchHelperFlex: UpdateBranchHelper
	{
		protected override float GetModelVersionFromBranchName(string branchName)
		{
			return uint.Parse(branchName);
		}

		protected override float GetModelVersionFromClone(string cloneLocation)
		{
			var modelVersion = GetFullModelVersion(cloneLocation);
			return (modelVersion == null)
				? uint.MaxValue // Get rid of the initial default commit by making it max for uint. It had no model version file.
				: uint.Parse(modelVersion);
		}

		protected override string GetFullModelVersion(string cloneLocation)
		{
			return Utilities.GetFlexModelVersion(cloneLocation);
		}
	}
}

