using System.Windows.Forms;
using SIL.LiftBridge.Model;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// Interface that handles getting a teammate's shared Lift project.
	/// </summary>
	internal interface IGetSharedProject
	{
		/// <summary>
		/// Get a teammate's shared Lift project from the specified source.
		/// </summary>
		/// <returns>
		/// One of several of the enum values.
		/// </returns>
		CloneResult GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, LiftProject project);
	}
}