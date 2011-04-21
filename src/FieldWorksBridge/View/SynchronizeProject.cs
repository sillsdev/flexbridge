#define USEMULTIPLEFILES
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;
using FieldWorksBridge.Model;
#if USEMULTIPLEFILES
using FieldWorksBridge.Infrastructure;
#else
using System.Xml;
using Palaso.Xml;
#endif

namespace FieldWorksBridge.View
{
	internal class SynchronizeProject : ISynchronizeProject
	{
		#region Implementation of ISynchronizeProject

		public void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject)
		{
			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".lock");
			File.WriteAllText(lockPathname, "");

			var origPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata");
#if !USEMULTIPLEFILES
			// Make the xml better formed.
			var tempPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".temp");
			using (var reader = XmlReader.Create(origPathname,
				new XmlReaderSettings { IgnoreWhitespace = true }))
			using (var writer = XmlWriter.Create(tempPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				// 1. Write (copy) the root element, including its attributes.
				reader.MoveToContent();
				writer.WriteStartElement("languageproject");
				reader.MoveToAttribute("version");
				writer.WriteAttributeString("version", reader.Value);
				reader.MoveToElement();
				reader.Read();

				// 2. Write (copy) all root element child elements.
				while (!reader.EOF)
					writer.WriteNode(reader, true);
			}

			// 3. Rename/copy/move the new file to fwdata.
			File.Copy(origPathname, Path.Combine(langProject.DirectoryName, langProject.Name + ".bak"), true);
			File.Copy(tempPathname, origPathname, true);
			File.Delete(tempPathname);
#else
			// Break up into smaller files.
			MultipleFileServices.BreakupMainFile(origPathname);
#endif

			// Do the Chorus business.
			try
			{
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog())
				{
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.ShowDialog(parent);

#if USEMULTIPLEFILES
					if (syncDlg.SyncResult.DidGetChangesFromOthers)
					{
						// Put Humpty together again.
						MultipleFileServices.RestoreMainFile(origPathname);
					}
#endif
				}
			}
			finally
			{
				if (File.Exists(lockPathname))
					File.Delete(lockPathname);
			}
		}

		#endregion
	}
}