using System.Windows.Forms;
using FLEx_ChorusPlugin.Model;

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

		/// <summary>
		/// Access some current language project data.
		/// Implementations may throw an exception if called before GetSharedProjectUsing().
		/// </summary>
		LanguageProject CurrentProject { get; set; }
	}
}