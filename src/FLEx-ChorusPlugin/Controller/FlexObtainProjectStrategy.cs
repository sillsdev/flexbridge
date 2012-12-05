using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IObtainProjectStrategy))]
	public class FlexObtainProjectStrategy : IObtainProjectStrategy
	{
		private string _newProjectPathname;
		[Import]
		public FLExConnectionHelper ConnectionHelper;

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*_custom_properties.i").Any();
		}

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !File.Exists(Path.Combine(repositoryLocation, SharedConstants.ModelVersionFilename));
		}

		public ActualCloneResult FinishCloning(string fwrootBaseDir, string cloneLocation)
		{
			// This may not be a really great name for the project, but it can't get any better,
			// since the real lang proj name is not available in the FW data.
			var langProjName = Path.GetDirectoryName(cloneLocation);
			_newProjectPathname = langProjName + ".fwdata";
			var newHomeDir = Path.Combine(fwrootBaseDir, _newProjectPathname);

			var retVal = new ActualCloneResult
				{
					// Be a bit pessimistic at first.
					CloneResult = null,
					ActualCloneFolder = null,
					FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
				};
			if (Directory.Exists(newHomeDir))
			{
				Directory.Delete(cloneLocation, true);
				return retVal;
			}

			// Move the repo from its temp home in cloneLocation into new home, before doing the call to 'PutHumptyTogetherAgain'.
			// The original location, may not be on the same device, so it may be a copy+delete, rather than a formal move.
			// At the end of the day, cloneLocation and its parent temp folder need to be deleted. MakeLocalCloneAndRemoveSourceParentFolder aims to do all of it.
			Utilities.MakeLocalCloneAndRemoveSourceParentFolder(cloneLocation, newHomeDir);

			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), newHomeDir);

			retVal.ActualCloneFolder = newHomeDir;
			retVal.FinalCloneResult = FinalCloneResult.Cloned;
			return retVal;
		}

		public void TellFlexAboutIt()
		{
			ConnectionHelper.CreateProjectFromFlex(_newProjectPathname);
		}

		#endregion
	}
}
