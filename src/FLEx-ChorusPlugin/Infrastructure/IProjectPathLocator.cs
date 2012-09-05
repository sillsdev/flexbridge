using System.Collections.Generic;

namespace FLEx_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// Interface that locates one, or more, base paths to FieldWorks projects.
	/// </summary>
	internal interface IProjectPathLocator
	{
		HashSet<string> BaseFolderPaths { get; }
	}
}