// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Globalization;
using SIL.LiftBridge.Services;
using SIL.Xml;
using System.Xml;
using LibTriboroughBridgeChorusPlugin.Infrastructure;

namespace SIL.LiftBridge.Infrastructure
{
	public class UpdateBranchHelperLift: UpdateBranchHelper
	{
		protected override float GetModelVersionFromBranchName(string branchName)
		{
			return float.Parse(branchName.Replace("LIFT", null).Substring(0,4), NumberFormatInfo.InvariantInfo);
		}

		protected override float GetModelVersionFromClone(string cloneLocation)
		{
			return GetLiftVersionNumber(cloneLocation);
		}

		protected override string GetFullModelVersion(string cloneLocation)
		{
			var modelVersion = GetModelVersionFromClone(cloneLocation);
			return "LIFT" + modelVersion + "_ldml3";
		}

		private static float GetLiftVersionNumber(string repoLocation)
		{
			// Return 0.13 if there is no lift file or it has no 'version' attr on the main 'lift' element.
			var firstLiftFile = FileAndDirectoryServices.GetPathToFirstLiftFile(repoLocation);
			if (firstLiftFile == null)
				return float.MaxValue;

			using (var reader = XmlReader.Create(firstLiftFile, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return float.Parse(reader.Value, NumberFormatInfo.InvariantInfo);
			}
		}

	}
}

