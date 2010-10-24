using System;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
{
	internal class ProjectEventArgs : EventArgs
	{
		public LanguageProject Project { get; private set; }

		internal ProjectEventArgs(LanguageProject project)
		{
			Project = project;
		}
	}
}