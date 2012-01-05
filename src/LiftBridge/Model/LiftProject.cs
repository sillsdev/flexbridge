namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Class that represents a Lift project.
	/// </summary>
	internal class LiftProject
	{
		internal string LiftProjectName { get; private set; }
		internal string RepositoryIdentifier { get; set; }

		public string LiftPathname
		{
			get { return LiftProjectServices.PathToFirstLiftFile(this); }
		}

		internal LiftProject(string liftProjectName, string repositoryIdentifier)
			: this(liftProjectName)
		{
			RepositoryIdentifier = repositoryIdentifier;
		}

		internal LiftProject(string liftProjectName)
		{
			LiftProjectName = liftProjectName;
		}
	}
}
