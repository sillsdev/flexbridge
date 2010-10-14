using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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

namespace FieldWorksBridge
{
	public class FieldWorksBridgeBootstrapper : IDisposable
	{
		public const string BaseDir = @"C:\ProgramData\SIL\FieldWorks 7\Projects";
		private readonly Dictionary<string, IContainer> _containers = new Dictionary<string, IContainer>();

		public Form Bootstrap()
		{
			var outerBuilder = new ContainerBuilder();
			outerBuilder.Register<IProgress>(new NullProgress());
			// This is a sad hack... I (JohnH) don't know how to simply override the default using the container,
			// which I'd rather do, and just leave this to pushing in the "normal"
			outerBuilder.Register(SyncUIFeatures.Advanced).SingletonScoped();
			outerBuilder.Register(new EmbeddedMessageContentHandlerFactory());
			outerBuilder.Register(ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
			outerBuilder.Register(new FieldWorksBridge(_containers));
			//outerBuilder.Register<FieldWorksBridge>();
			_containers.Add("MainContainer", outerBuilder.Build());
			var mainContainer = _containers["MainContainer"];

			foreach (var folder in Directory.GetDirectories(BaseDir))
			{
				var pathname = Path.GetFileName(folder);
				var innerBuilder = new ContainerBuilder();
				var innerContainer = mainContainer.CreateInnerContainer();
				_containers.Add(pathname, innerContainer);

				innerBuilder.Register(new ChorusSystem(folder));
				innerBuilder.Register<ProjectFolderConfiguration>(new FieldWorksProjectFolderConfiguration(folder));
				innerBuilder.Register<NavigateToRecordEvent>();
				innerBuilder.Register(innerC => Synchronizer.FromProjectConfiguration(
					innerC.Resolve<ProjectFolderConfiguration>(),
					mainContainer.Resolve<IProgress>()));
				innerBuilder.Register(HgRepository.CreateOrLocate(
					folder,
					mainContainer.Resolve<IProgress>()));
				//innerBuilder.Register<TroubleshootingView>();

				RegisterSyncStuff(innerBuilder);
				RegisterReviewStuff(innerBuilder);
				RegisterSettingsStuff(innerBuilder);
				RegisterNotesStuff(innerBuilder);

				// For now, we like the idea of just using the login name.  But
				// this allows someone to override that in the ini (which would be for all users of this machine, then)
				var loginName = Environment.UserName;
				innerBuilder.Register<IChorusUser>(x =>
													{
														var repos = innerContainer.Resolve<HgRepository>();
														var hgUsername = repos.GetUserNameFromIni(
															mainContainer.Resolve<IProgress>(),
															loginName);
														if (string.IsNullOrEmpty(hgUsername))
															hgUsername = loginName;
														return new ChorusUser(hgUsername);
													});
				innerBuilder.Register(x => new SyncDialog(innerContainer.Resolve<ProjectFolderConfiguration>(),
														  SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended));

				innerBuilder.Build(innerContainer);
			}
			return mainContainer.Resolve<FieldWorksBridge>();
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

		private static void RegisterSyncStuff(ContainerBuilder innerBuilder)
		{
			innerBuilder.Register<SyncPanel>();
			innerBuilder.Register<SyncControlModel>();
			//innerBuilder.Register<SyncStartControl>();
			innerBuilder.Register<BridgeSyncControl>();
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
			_containers["MainContainer"].Dispose();
		}

		#endregion
	}
}