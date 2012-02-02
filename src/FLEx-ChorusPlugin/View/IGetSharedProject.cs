using System.Windows.Forms;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// Interface that handles getting a teammate's shared FieldWorks project.
	/// </summary>
	internal interface IGetSharedProject
	{
		/// <summary>
		/// Get a teammate's shared FieldWorks project from the specified source.
		/// </summary>
		/// <returns>
		/// 'true' if the shared project was cloned, otherwise 'false'.
		/// </returns>
		bool GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, string flexProjectDir);
	}
}