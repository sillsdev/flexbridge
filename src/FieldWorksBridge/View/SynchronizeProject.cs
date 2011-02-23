using System.IO;
using System.Windows.Forms;
using System.Xml;
using Chorus;
using Chorus.UI.Sync;
using FieldWorksBridge.Model;
using Palaso.Xml;

namespace FieldWorksBridge.View
{
	internal class SynchronizeProject : ISynchronizeProject
	{
		#region Implementation of ISynchronizeProject

		public void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject)
		{
			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".lock");

			// Try to make the xml better formed.
			var readerSettings = new XmlReaderSettings
									{
										IgnoreWhitespace = true
									};
			var origPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata");
			var tmpBPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".tmpB");
			var writeSettings = CanonicalXmlSettings.CreateXmlWriterSettings();
			using (var reader = XmlReader.Create(origPathname, readerSettings))
			using (var writer = XmlWriter.Create(tmpBPathname, writeSettings))
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
				{
					writer.WriteNode(reader, true);
				}
			}

			// 3. Rename/copy/move the new file to fwdata.
			File.Delete(origPathname);
			File.Move(tmpBPathname, origPathname);

			try
			{
				File.WriteAllText(lockPathname, "");

				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog())
				{
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.ShowDialog(parent);
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