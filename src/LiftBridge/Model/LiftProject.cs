using System;

namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Class that represents a Lift project.
	/// </summary>
	internal class LiftProject
	{
		private readonly Guid _id;

		internal LiftProject(string liftProjectName)
		{
			LiftProjectName = liftProjectName;
			_id = Guid.Empty;
		}

		internal LiftProject(string liftProjectName, Guid langProjId)
		{
			LiftProjectName = liftProjectName;
			_id = langProjId;
			LiftProjectServices.StoreIdentifiers(langProjId, null);
		}

		internal string LiftProjectName { get; private set; }

		internal string RepositoryIdentifier
		{
			get
			{
				return LiftProjectServices.GetRepositoryIdentifier(_id);
			}
			set
			{
				LiftProjectServices.StoreIdentifiers(_id, value);
			}
		}

		internal string LiftPathname
		{
			get { return LiftProjectServices.PathToFirstLiftFile(this); }
		}
	}
}
