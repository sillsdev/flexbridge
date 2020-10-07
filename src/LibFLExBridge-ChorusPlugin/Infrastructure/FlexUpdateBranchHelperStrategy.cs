// Copyright (c) 2015-16 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Globalization;
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

		// Note that, after Flex9.0.7/FlexBridge3.1, this is actually a two part number, where the whole
		// number part is the FlexBridgeDataVersion, and the fraction is the actual FLEx
		// model number. For example, the current code returns 7500002.7000072, where the
		// number after the decimal is actually the FLEx model version.
		double IUpdateBranchHelperStrategy.GetModelVersionFromBranchName(string branchName)
		{
			return double.Parse(branchName, NumberFormatInfo.InvariantInfo);
		}

		string IUpdateBranchHelperStrategy.GetBranchNameFromModelVersion(string modelVersion)
		{
			return (System.String.CompareOrdinal(modelVersion, "7000072") < 0)
				? modelVersion
				: FlexBridgeConstants.FlexBridgeDataVersion + "." + modelVersion;
		}

		double IUpdateBranchHelperStrategy.GetModelVersionFromClone(string cloneLocation)
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

