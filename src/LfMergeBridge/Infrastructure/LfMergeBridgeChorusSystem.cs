// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Autofac;
using Chorus;
using Chorus.sync;
using Palaso.Code;
using Palaso.Progress;

namespace LfMergeBridge.Infrastructure
{
	[Export(typeof(IChorusSystem))]
	internal class LfMergeBridgeChorusSystem: ChorusSystemSimple
	{
		[Import(AllowRecomposition = true)]
		private ILfMergeBridgeDataProvider DataProvider { get; set; }

		public override void Init(string dataFolderPath, string userNameForHistoryAndNotes)
		{
			base.Init(dataFolderPath, userNameForHistoryAndNotes);

			Require.That(DataProvider != null);
			Require.That(Container != null);

			var builder = new ContainerBuilder();
			builder.Register<ProjectFolderConfiguration>(
				c => new ProjectFolderConfiguration(DataProvider.ProjectFolderPath)).InstancePerLifetimeScope();
			builder.Register<IProgress>(
				c => DataProvider.Progress).InstancePerLifetimeScope();
			builder.Update(Container);

			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			string applicationName = assemblyName.Name;
			string applicationVersion = assemblyName.Version.ToString();

			var controlModel = Container.Resolve<SyncControlModelSimple>();
			var syncOptions = controlModel.SyncOptions;
			syncOptions.CheckinDescription = string.Format("[{0}: {1}] sync",
				applicationName, applicationVersion);
			syncOptions.RepositorySourcesToTry.Add(DataProvider.Repo);
		}
	}
}

