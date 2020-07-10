// Copyright (c) 2015-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Globalization;
using System.Xml;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Xml;
using SIL.LiftBridge.Services;

namespace SIL.LiftBridge.Infrastructure
{
	internal sealed class UpdateBranchHelperLift : IUpdateBranchHelperStrategy
	{
		internal static double GetLiftVersionNumber(string repoLocation)
		{
			var firstLiftFile = FileAndDirectoryServices.GetPathToFirstLiftFile(repoLocation);
			if (string.IsNullOrEmpty(firstLiftFile))
				return double.MaxValue;

			using (var reader = XmlReader.Create(firstLiftFile, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return double.Parse(reader.Value, NumberFormatInfo.InvariantInfo);
			}
		}

		public double GetModelVersionFromBranchName(string branchName)
		{
			return double.Parse(branchName.Replace("LIFT", null).Split('_')[0], NumberFormatInfo.InvariantInfo);
		}

		string IUpdateBranchHelperStrategy.GetBranchNameFromModelVersion(string modelVersion)
		{
			return "LIFT" + modelVersion + "_ldml3";
		}

		public double GetModelVersionFromClone(string cloneLocation)
		{
			return GetLiftVersionNumber(cloneLocation);
		}

		public string GetFullModelVersion(string cloneLocation)
		{
			var modelVersion = GetModelVersionFromClone(cloneLocation);
			return "LIFT" + modelVersion + "_ldml3";
		}
	}
}

