// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Chorus.VcsDrivers.Mercurial;
using LfMergeBridge.Infrastructure;

namespace LfMergeBridge
{
	public class LfMergeBridge: IDisposable
	{
		private AggregateCatalog _catalog;
		private CompositionContainer _container;

		public LfMergeBridge(ILfMergeBridgeDataProvider dataProvider)
		{
			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
				throw new ApplicationException("Mercurial is not set up properly");

			_catalog = CreateCatalog();

			// Create the CompositionContainer with the parts in the catalog
			_container = new CompositionContainer(_catalog, CompositionOptions.DisableSilentRejection);
			_container.ComposeExportedValue<ILfMergeBridgeDataProvider>(dataProvider);
		}

		#region Disposable stuff

		#if DEBUG
		/// <summary/>
		~LfMergeBridge()
		{
			Dispose(false);
		}
		#endif

		/// <summary/>
		public bool IsDisposed { get; private set; }

		/// <summary/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary/>
		protected virtual void Dispose(bool fDisposing)
		{
			Debug.WriteLineIf(!fDisposing, "****** Missing Dispose() call for " + GetType() + ". *******");

			if (fDisposing && !IsDisposed)
			{
				// dispose managed and unmanaged objects
				if (_container != null)
					_container.Dispose();
				if (_catalog != null)
					_catalog.Dispose();
			}
			_container = null;
			_catalog = null;
			IsDisposed = true;
		}

		#endregion

		private static AggregateCatalog CreateCatalog()
		{
			// An aggregate catalog that combines multiple catalogs
			var catalog = new AggregateCatalog();
			catalog.Catalogs.Add(new DirectoryCatalog(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				"*-ChorusPlugin.dll"));
			catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
			return catalog;
		}

		public void SendReceive()
		{
			var handlerRepository = _container.GetExportedValue<ActionTypeHandlerRepository>();
			var currentHandler = handlerRepository.GetHandler(ActionType.SendReceive);
			currentHandler.Execute();
		}
	}
}

