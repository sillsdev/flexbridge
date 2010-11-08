namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Class that represents a Lift project.
	/// </summary>
	internal class LiftProject
	{
		internal string LiftProjectName { get; private set; }

		public string LiftPathname
		{
			get { return LiftProjectServices.PathToFirstLiftFile(this); }
		}

		internal LiftProject(string liftProjectName)
		{
			LiftProjectName = liftProjectName;
		}
	}
}
