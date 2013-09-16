using System;
using TheTurtle.Model;

namespace TheTurtle.View
{
	internal sealed class ProjectEventArgs : EventArgs
	{
		internal LanguageProject Project { get; private set; }

		internal ProjectEventArgs(LanguageProject project)
		{
			Project = project;
		}
	}
}