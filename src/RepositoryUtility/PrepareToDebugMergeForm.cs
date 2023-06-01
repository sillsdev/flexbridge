using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RepositoryUtility
{
	internal interface IPrepareToDebugMergeView
	{
		void SetController(PrepareToDebugController controller);
		void UpdateOkEnabledValue(bool newValue);
		void UpdateParentList(IEnumerable<string> parents);

		void SetMergeCommit(string mergeCommitToDebug);
	}
	internal class PrepareToDebugMergeForm : Form, IPrepareToDebugMergeView
	{
		private TextBox textbox_mergecommit;
		private Button btn_ok;
		private Button btn_cancel;
		private Label label_mergecommit;
		private ListBox listbox_mergeparents;
		private TextBox tb_label_mergeparents;
		private PrepareToDebugController controller;
		public PrepareToDebugMergeForm()
		{
			InitializeComponent();
		}

		public void SetController(PrepareToDebugController controller)
		{
			this.controller = controller;
		}

		public void UpdateOkEnabledValue(bool newValue)
		{
			btn_ok.Enabled = newValue;
		}

		public void UpdateParentList(IEnumerable<string> parents)
		{
			listbox_mergeparents.SelectedIndexChanged -= listbox_mergeparents_SelectedIndexChanged;
			listbox_mergeparents.Items.Clear();
			listbox_mergeparents.Items.AddRange(parents.Select(p => p as object).ToArray());
			listbox_mergeparents.SelectedIndexChanged += listbox_mergeparents_SelectedIndexChanged;
			Refresh();
		}

		public void SetMergeCommit(string mergeCommitToDebug)
		{
			textbox_mergecommit.TextChanged -= textbox_mergecommit_TextChanged;
			textbox_mergecommit.Text = mergeCommitToDebug;
			textbox_mergecommit.TextChanged += textbox_mergecommit_TextChanged;
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrepareToDebugMergeForm));
			this.textbox_mergecommit = new System.Windows.Forms.TextBox();
			this.btn_ok = new System.Windows.Forms.Button();
			this.btn_cancel = new System.Windows.Forms.Button();
			this.label_mergecommit = new System.Windows.Forms.Label();
			this.listbox_mergeparents = new System.Windows.Forms.ListBox();
			this.tb_label_mergeparents = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// textbox_mergecommit
			// 
			this.textbox_mergecommit.Location = new System.Drawing.Point(12, 26);
			this.textbox_mergecommit.Name = "textbox_mergecommit";
			this.textbox_mergecommit.Size = new System.Drawing.Size(100, 20);
			this.textbox_mergecommit.TabIndex = 0;
			this.textbox_mergecommit.TextChanged += new System.EventHandler(this.textbox_mergecommit_TextChanged);
			// 
			// btn_ok
			// 
			this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btn_ok.Location = new System.Drawing.Point(112, 226);
			this.btn_ok.Name = "btn_ok";
			this.btn_ok.Size = new System.Drawing.Size(75, 23);
			this.btn_ok.TabIndex = 1;
			this.btn_ok.Text = "OK";
			this.btn_ok.UseVisualStyleBackColor = true;
			// 
			// btn_cancel
			// 
			this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_cancel.Location = new System.Drawing.Point(197, 226);
			this.btn_cancel.Name = "btn_cancel";
			this.btn_cancel.Size = new System.Drawing.Size(75, 23);
			this.btn_cancel.TabIndex = 2;
			this.btn_cancel.Text = "Cancel";
			this.btn_cancel.UseVisualStyleBackColor = true;
			// 
			// label_mergecommit
			// 
			this.label_mergecommit.AutoSize = true;
			this.label_mergecommit.Location = new System.Drawing.Point(118, 29);
			this.label_mergecommit.Name = "label_mergecommit";
			this.label_mergecommit.Size = new System.Drawing.Size(103, 13);
			this.label_mergecommit.TabIndex = 3;
			this.label_mergecommit.Text = "Bad merge to debug";
			// 
			// listbox_mergeparents
			// 
			this.listbox_mergeparents.FormattingEnabled = true;
			this.listbox_mergeparents.Location = new System.Drawing.Point(13, 53);
			this.listbox_mergeparents.Name = "listbox_mergeparents";
			this.listbox_mergeparents.Size = new System.Drawing.Size(120, 95);
			this.listbox_mergeparents.TabIndex = 4;
			this.listbox_mergeparents.SelectedIndexChanged += new System.EventHandler(this.listbox_mergeparents_SelectedIndexChanged);
			// 
			// tb_label_mergeparents
			// 
			this.tb_label_mergeparents.Location = new System.Drawing.Point(140, 53);
			this.tb_label_mergeparents.Multiline = true;
			this.tb_label_mergeparents.Name = "tb_label_mergeparents";
			this.tb_label_mergeparents.ReadOnly = true;
			this.tb_label_mergeparents.Size = new System.Drawing.Size(132, 95);
			this.tb_label_mergeparents.TabIndex = 5;
			this.tb_label_mergeparents.Text = "Pick the merge parent (user) to initiate the merge from";
			// 
			// PrepareToDebugMergeForm
			// 
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.tb_label_mergeparents);
			this.Controls.Add(this.listbox_mergeparents);
			this.Controls.Add(this.label_mergecommit);
			this.Controls.Add(this.btn_cancel);
			this.Controls.Add(this.btn_ok);
			this.Controls.Add(this.textbox_mergecommit);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PrepareToDebugMergeForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void listbox_mergeparents_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			var listBox = sender as ListBox;
			controller.SetParentToInitFrom(listBox?.SelectedItem?.ToString());
		}

		private void textbox_mergecommit_TextChanged(object sender, System.EventArgs e)
		{
			controller.SetMergeCommit(((TextBox)sender).Text);
		}
	}

	internal class PrepareToDebugModel
	{
		public string MergeCommitToDebug { get; set; }
		public List<string> MergeParents { get; set; }
		public string ParentToInitFrom { get; set; }
	}
}