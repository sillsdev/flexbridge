using System;
using FLEx_ChorusPlugin.StandAlone.Model;

namespace FLEx_ChorusPlugin.StandAlone.View
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