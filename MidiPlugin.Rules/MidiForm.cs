using Lumos.GUI.BaseWindow;
using LumosLIB.GUI.Windows;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
namespace MidiPlugin
{
	public class MidiForm : ToolWindow, IDisposable
	{
		private IContainer components = null;
		private MenuStrip menuStrip1;
		private SplitContainer splitContainer;
		private GroupBox devicesGrp;
		private DataGridView devicesGrid;
		private GroupBox rulesGrp;
		private DataGridView rulesGrid;
		private ToolStripMenuItem addRuleSetToolStripMenuItem;
        private ToolStripMenuItem deleteRuleSetToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem exportRuleSetsToolStripMenuItem;
        private ToolStripMenuItem importRuleSetsToolStripMenuItem;
		private ToolStripMenuItem infoToolStripMenuItem;
		public MidiForm()
		{
			this.InitializeComponent();
			this.Text = "MidiManager";
			ContextManager.MidiForm = this;
			this.rulesGrid.DataSource = null;
			base.Shown += new EventHandler(this.HandleFormShown);
		}
		private void HandleFormShown(object o, EventArgs e)
		{
			this.devicesGrid.DataSource = null;
			if (ContextManager.DeviceInformation != null)
			{
				this.devicesGrid.DataSource = ContextManager.DeviceInformation.Devices;
			}
			this.rulesGrid.DataSource = null;
			if (ContextManager.MidiInformation != null)
			{
				this.rulesGrid.DataSource = ContextManager.MidiInformation.RuleSets;
			}
			this.rulesGrid.KeyDown += new KeyEventHandler(this.HandleKeyDown);
			this.rulesGrid.CellDoubleClick += new DataGridViewCellEventHandler(this.HandleRulesDoubleClick);
			this.addRuleSetToolStripMenuItem.Click += new EventHandler(this.HandleAddRuleClick);
			this.deleteRuleSetToolStripMenuItem.Click += new EventHandler(this.HandleDeleteRuleClick);
			this.infoToolStripMenuItem.Click += new EventHandler(this.HandleInfoClick);
		}
		private void HandleKeyDown(object s, KeyEventArgs e)
		{
			Keys keyCode = e.KeyCode;
			if (keyCode != Keys.Delete)
			{
				if (keyCode == Keys.F5)
				{
					this.AddNew();
				}
			}
			else
			{
				this.DeleteSelected();
			}
		}
		private void HandleRulesDoubleClick(object s, DataGridViewCellEventArgs e)
		{
            if (e.RowIndex == -1)
                return;
			RuleSet rs = ContextManager.MidiInformation.RuleSets[e.RowIndex];
			rs.OpenEditWindow();
		}
		private void HandleAddRuleClick(object s, EventArgs e)
		{
			this.AddNew();
		}
		private void HandleDeleteRuleClick(object s, EventArgs e)
		{
			this.DeleteSelected();
		}
		private void HandleInfoClick(object s, EventArgs e)
		{
			MessageBox.Show("Keybindings:\nF5 - Add RuleSet\ndel - Delete selected RuleSet", "Keybindings", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		private void AddNew()
		{
			ContextManager.MidiInformation.RuleSets.Add(new RuleSet());
		}
		private void DeleteSelected()
		{
			if (this.rulesGrid.SelectedRows.Count == 1)
			{
				RuleSet rs = (RuleSet)this.rulesGrid.SelectedRows[0].DataBoundItem;
				ContextManager.MidiInformation.RuleSets.Remove(rs);
				rs.Dispose();
			}
		}
		public new void Dispose()
		{
			this.addRuleSetToolStripMenuItem.Click -= new EventHandler(this.HandleAddRuleClick);
			this.deleteRuleSetToolStripMenuItem.Click -= new EventHandler(this.HandleDeleteRuleClick);
			this.infoToolStripMenuItem.Click -= new EventHandler(this.HandleInfoClick);
			this.rulesGrid.KeyDown -= new KeyEventHandler(this.HandleKeyDown);
			this.rulesGrid.CellDoubleClick -= new DataGridViewCellEventHandler(this.HandleRulesDoubleClick);
			this.rulesGrid.DataSource = null;
			base.Dispose();
		}
		public void UpdateUi()
		{
			if (base.InvokeRequired)
			{
				base.Invoke(new Action(this.UpdateUi));
			}
			else
			{
				this.rulesGrid.Refresh();
				this.devicesGrid.Refresh();
			}
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		private void InitializeComponent()
		{
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.addRuleSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteRuleSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportRuleSetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importRuleSetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.devicesGrp = new System.Windows.Forms.GroupBox();
            this.devicesGrid = new System.Windows.Forms.DataGridView();
            this.rulesGrp = new System.Windows.Forms.GroupBox();
            this.rulesGrid = new System.Windows.Forms.DataGridView();
            this.menuStrip1.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.devicesGrp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.devicesGrid)).BeginInit();
            this.rulesGrp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rulesGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addRuleSetToolStripMenuItem,
            this.deleteRuleSetToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(667, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // addRuleSetToolStripMenuItem
            // 
            this.addRuleSetToolStripMenuItem.Name = "addRuleSetToolStripMenuItem";
            this.addRuleSetToolStripMenuItem.Size = new System.Drawing.Size(85, 20);
            this.addRuleSetToolStripMenuItem.Text = "Add Rule Set";
            // 
            // deleteRuleSetToolStripMenuItem
            // 
            this.deleteRuleSetToolStripMenuItem.Name = "deleteRuleSetToolStripMenuItem";
            this.deleteRuleSetToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
            this.deleteRuleSetToolStripMenuItem.Text = "Delete Rule Set";
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.infoToolStripMenuItem.Text = "Info";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportRuleSetsToolStripMenuItem,
            this.importRuleSetsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // exportRuleSetsToolStripMenuItem
            // 
            this.exportRuleSetsToolStripMenuItem.Name = "exportRuleSetsToolStripMenuItem";
            this.exportRuleSetsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.exportRuleSetsToolStripMenuItem.Text = "Export RuleSets";
            this.exportRuleSetsToolStripMenuItem.Click += new System.EventHandler(this.exportRuleSetsToolStripMenuItem_Click);
            // 
            // importRuleSetsToolStripMenuItem
            // 
            this.importRuleSetsToolStripMenuItem.Name = "importRuleSetsToolStripMenuItem";
            this.importRuleSetsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.importRuleSetsToolStripMenuItem.Text = "Import RuleSets";
            this.importRuleSetsToolStripMenuItem.Click += new System.EventHandler(this.importRuleSetsToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 24);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.devicesGrp);
            this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(5);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.rulesGrp);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5);
            this.splitContainer.Size = new System.Drawing.Size(667, 434);
            this.splitContainer.SplitterDistance = 196;
            this.splitContainer.TabIndex = 1;
            // 
            // devicesGrp
            // 
            this.devicesGrp.Controls.Add(this.devicesGrid);
            this.devicesGrp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesGrp.Location = new System.Drawing.Point(5, 5);
            this.devicesGrp.Name = "devicesGrp";
            this.devicesGrp.Padding = new System.Windows.Forms.Padding(7);
            this.devicesGrp.Size = new System.Drawing.Size(657, 186);
            this.devicesGrp.TabIndex = 0;
            this.devicesGrp.TabStop = false;
            this.devicesGrp.Text = "Devices";
            // 
            // devicesGrid
            // 
            this.devicesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.devicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicesGrid.Location = new System.Drawing.Point(7, 20);
            this.devicesGrid.Name = "devicesGrid";
            this.devicesGrid.Size = new System.Drawing.Size(643, 159);
            this.devicesGrid.TabIndex = 0;
            // 
            // rulesGrp
            // 
            this.rulesGrp.Controls.Add(this.rulesGrid);
            this.rulesGrp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rulesGrp.Location = new System.Drawing.Point(5, 5);
            this.rulesGrp.Name = "rulesGrp";
            this.rulesGrp.Padding = new System.Windows.Forms.Padding(7);
            this.rulesGrp.Size = new System.Drawing.Size(657, 224);
            this.rulesGrp.TabIndex = 0;
            this.rulesGrp.TabStop = false;
            this.rulesGrp.Text = "Loaded Rule Sets";
            // 
            // rulesGrid
            // 
            this.rulesGrid.AllowUserToAddRows = false;
            this.rulesGrid.AllowUserToDeleteRows = false;
            this.rulesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.rulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rulesGrid.Location = new System.Drawing.Point(7, 20);
            this.rulesGrid.Name = "rulesGrid";
            this.rulesGrid.Size = new System.Drawing.Size(643, 197);
            this.rulesGrid.TabIndex = 1;
            // 
            // MidiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(667, 458);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = new System.Drawing.Point(0, 0);
            this.MainFormMenu = LumosLIB.GUI.Windows.MenuType.Settings;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MidiForm";
            this.TabText = "MidiManager";
            this.Text = "MidiManager";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.devicesGrp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.devicesGrid)).EndInit();
            this.rulesGrp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rulesGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private void exportRuleSetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Export != null) Export(null, null);
        }


        public event EventHandler Export;
        public event EventHandler Import;

        private void importRuleSetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Import != null) Import(null, null);
        }

	}
}
