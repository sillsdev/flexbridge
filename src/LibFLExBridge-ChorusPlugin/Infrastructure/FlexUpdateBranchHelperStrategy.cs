// Copyright (c) 2015-16 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using LibTriboroughBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	internal sealed class FlexUpdateBranchHelperStrategy : IUpdateBranchHelperStrategy
	{
		private IUpdateBranchHelperStrategy AsIUpdateBranchHelperStrategy
		{
			get { return this; }
		}

		#region IUpdateBranchHelperStrategy impl

		// Note that this model version, after 700072, may have a decimal part that is the FlexBridgeVersion,
		// in addition to the integer part that is the model version.
		float IUpdateBranchHelperStrategy.GetModelVersionFromBranchName(string branchName)
		{
			return float.Parse(branchName);
		}

		float IUpdateBranchHelperStrategy.GetModelVersionFromClone(string cloneLocation)
		{
			var modelVersion = AsIUpdateBranchHelperStrategy.GetFullModelVersion(cloneLocation);
			return string.IsNullOrEmpty(modelVersion)
				? uint.MaxValue // Get rid of the initial default commit by making it max for uint. It had no model version file.
				: uint.Parse(modelVersion);
		}

		string IUpdateBranchHelperStrategy.GetFullModelVersion(string cloneLocation)
		{
			return LibFLExBridgeUtilities.GetFlexModelVersion(cloneLocation);
		}

		#endregion IUpdateBranchHelperStrategy impl
	}
}

