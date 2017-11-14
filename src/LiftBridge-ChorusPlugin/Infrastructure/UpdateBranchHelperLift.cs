// Copyright (c) 2015-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Xml;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Xml;
using SIL.LiftBridge.Services;

namespace SIL.LiftBridge.Infrastructure
{
	internal sealed class UpdateBranchHelperLift : IUpdateBranchHelperStrategy
	{
		private IUpdateBranchHelperStrategy AsIUpdateBranchHelperStrategy
		{
			get { return this; }
		}

		private static float GetLiftVersionNumber(string repoLocation)
		{
			// Return 0.13 if there is no lift file or it has no 'version' attr on the main 'lift' element.
			var firstLiftFile = FileAndDirectoryServices.GetPathToFirstLiftFile(repoLocation);
			if (string.IsNullOrEmpty(firstLiftFile))
				return float.MaxValue;

			using (var reader = XmlReader.Create(firstLiftFile, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return float.Parse(reader.Value);
			}
		}

		#region IUpdateBranchHelperStrategy impl

		float IUpdateBranchHelperStrategy.GetModelVersionFromBranchName(string branchName)
		{
			return float.Parse(branchName.Replace("LIFT", null));
		}

		float IUpdateBranchHelperStrategy.GetModelVersionFromClone(string cloneLocation)
		{
			return GetLiftVersionNumber(cloneLocation);
		}

		string IUpdateBranchHelperStrategy.GetFullModelVersion(string cloneLocation)
		{
			var modelVersion = AsIUpdateBranchHelperStrategy.GetModelVersionFromClone(cloneLocation);
			return "LIFT" + modelVersion;
		}

		#endregion IUpdateBranchHelperStrategy impl
	}
}

