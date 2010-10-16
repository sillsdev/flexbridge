using System;
using System.IO;
using Autofac;
using Autofac.Builder;
using Chorus;
using Chorus.FileTypeHanders;
using Chorus.notes;
using Chorus.retrieval;
using Chorus.sync;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Notes.Html;
using Chorus.UI.Review;
using Chorus.UI.Review.ChangedReport;
using Chorus.UI.Review.ChangesInRevision;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.UI.Settings;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace SIL.LiftBridge
{
	internal class LiftBridgeBootstrapper : IDisposable
	{
		internal string BaseDir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"LiftBridge");

		private IContainer _container;

		internal ExistingSystem Bootstrap(string projectFolder)
		{

			var pathname = Path.Combine(BaseDir, projectFolder);
			var builder = new ContainerBuilder();
			builder.Register<IProgress>(new NullProgress());
			// This is a sad hack... I (JohnH) don't know how to simply override the default using the container,
			// which I'd rather do, and just leave this to pushing in the "normal"
			builder.Register(SyncUIFeatures.Advanced).SingletonScoped();
			builder.Register(new EmbeddedMessageContentHandlerFactory());
			builder.Register(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
			builder.Register<ExistingSystem>();
			builder.Register(new ChorusSystem(pathname));
			builder.Register<ProjectFolderConfiguration>(new LiftBridgeProjectFolderConfiguration(pathname));
			builder.Register<NavigateToRecordEvent>();
			builder.Register(container => Synchronizer.FromProjectConfiguration(
					container.Resolve<ProjectFolderConfiguration>(),
					container.Resolve<IProgress>()));
			builder.Register(container => HgRepository.CreateOrLocate(
					pathname,
					container.Resolve<IProgress>()));
			RegisterSyncStuff(builder);
			RegisterReviewStuff(builder);
			RegisterSettingsStuff(builder);
			RegisterNotesStuff(builder);

			// For now, we like the idea of just using the login name.  But
			// this allows someone to override that in the ini (which would be for all users of this machine, then)
			var loginName = Environment.UserName;
			builder.Register<IChorusUser>(container =>
			{
				var repos = container.Resolve<HgRepository>();
				var hgUsername = repos.GetUserNameFromIni(
					container.Resolve<IProgress>(),
					loginName);
				if (string.IsNullOrEmpty(hgUsername))
					hgUsername = loginName;
				return new ChorusUser(hgUsername);
			});
			builder.Register(container => new SyncDialog(container.Resolve<ProjectFolderConfiguration>(),
													  SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended));

			_container = builder.Build();

			return _container.Resolve<ExistingSystem>();
		}

		/// <summary>
		/// Only call this directly if you're not using the synching stuff (e.g., testing the notes UI)
		/// </summary>
		private static void RegisterNotesStuff(ContainerBuilder builder)
		{
			builder.Register<MessageSelectedEvent>();
			builder.Register<EmbeddedMessageContentHandlerFactory>();
			builder.Register<NotesInProjectViewModel>();
			builder.Register<NotesInProjectView>();
			builder.Register<AnnotationEditorView>();
			builder.Register<AnnotationEditorModel>().FactoryScoped();
			builder.Register<NotesBrowserPage>();
			builder.Register(StyleSheet.CreateFromDisk());
			builder.RegisterGeneratedFactory<AnnotationEditorModel.Factory>();
			builder.Register<NotesBarModel>();
			builder.RegisterGeneratedFactory<NotesBarModel.Factory>();
			builder.Register<NotesBarView>();
			builder.RegisterGeneratedFactory<NotesBarView.Factory>();
			builder.RegisterGeneratedFactory<NotesInProjectView.Factory>().ContainerScoped();
			builder.RegisterGeneratedFactory<NotesInProjectViewModel.Factory>().ContainerScoped();
		}

		private static void RegisterSettingsStuff(ContainerBuilder builder)
		{
			builder.Register<SettingsModel>();
			builder.Register<SettingsView>();
		}

		private static void RegisterSyncStuff(ContainerBuilder builder)
		{
			builder.Register<SyncPanel>();
			builder.Register<SyncControlModel>();
			//innerBuilder.Register<SyncStartControl>();
			builder.Register<BridgeSyncControl>();
		}

		internal static void RegisterReviewStuff(ContainerBuilder builder)
		{
			builder.Register<IProgress>(new ConsoleProgress());
			builder.Register<RevisionInspector>();
			builder.Register<ChangesInRevisionModel>();
			builder.Register<HistoryPage>();
			builder.Register<ChangesInRevisionView>();
			builder.Register<ChangeReportView>();

			//review-related events
			builder.Register<RevisionSelectedEvent>();
			builder.Register<ChangedRecordSelectedEvent>();

			builder.Register<RevisionInRepositoryModel>();
			builder.Register<RevisionsInRepositoryView>();
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (_container != null)
				_container.Dispose();
		}

		#endregion
	}
}
