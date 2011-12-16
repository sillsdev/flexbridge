using System;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
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