using Lumos.GUI.BaseWindow;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MidiPlugin
{
	public class RuleSetEditForm : ToolWindow
	{
		private IContainer components = null;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem rulesToolStripMenuItem;
		private ToolStripMenuItem neuToolStripMenuItem;
		private ToolStripMenuItem deleteToolStripMenuItem;
		private DataGridView ruleSetGrid;
		private ToolStripMenuItem beginLearnToolStrip;
		private SplitContainer splitContainer1;
		private PropertyGrid ruleInfo;
		private ToolStripMenuItem cancelLearnToolStripMenuItem;
		private RuleSet rules;
		private BindingSource bs;
		private bool learning;
		private ILearnable currentlearn;
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
            this.rulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.neuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beginLearnToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelLearnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ruleSetGrid = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ruleInfo = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ruleSetGrid)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rulesToolStripMenuItem,
            this.beginLearnToolStrip,
            this.cancelLearnToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(556, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // rulesToolStripMenuItem
            // 
            this.rulesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.neuToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.rulesToolStripMenuItem.Name = "rulesToolStripMenuItem";
            this.rulesToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.rulesToolStripMenuItem.Text = "Rules";
            // 
            // neuToolStripMenuItem
            // 
            this.neuToolStripMenuItem.Name = "neuToolStripMenuItem";
            this.neuToolStripMenuItem.ShowShortcutKeys = false;
            this.neuToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.neuToolStripMenuItem.Text = "New";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // beginLearnToolStrip
            // 
            this.beginLearnToolStrip.Name = "beginLearnToolStrip";
            this.beginLearnToolStrip.Size = new System.Drawing.Size(78, 20);
            this.beginLearnToolStrip.Text = "Begin learn";
            // 
            // cancelLearnToolStripMenuItem
            // 
            this.cancelLearnToolStripMenuItem.Name = "cancelLearnToolStripMenuItem";
            this.cancelLearnToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.cancelLearnToolStripMenuItem.Text = "Cancel learn";
            // 
            // ruleSetGrid
            // 
            this.ruleSetGrid.AllowUserToAddRows = false;
            this.ruleSetGrid.AllowUserToDeleteRows = false;
            this.ruleSetGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ruleSetGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ruleSetGrid.Location = new System.Drawing.Point(0, 0);
            this.ruleSetGrid.MultiSelect = false;
            this.ruleSetGrid.Name = "ruleSetGrid";
            this.ruleSetGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ruleSetGrid.Size = new System.Drawing.Size(316, 387);
            this.ruleSetGrid.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ruleSetGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ruleInfo);
            this.splitContainer1.Size = new System.Drawing.Size(556, 387);
            this.splitContainer1.SplitterDistance = 316;
            this.splitContainer1.TabIndex = 2;
            // 
            // ruleInfo
            // 
            this.ruleInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ruleInfo.Location = new System.Drawing.Point(0, 0);
            this.ruleInfo.Name = "ruleInfo";
            this.ruleInfo.Size = new System.Drawing.Size(236, 387);
            this.ruleInfo.TabIndex = 0;
            // 
            // RuleSetEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(556, 411);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RuleSetEditForm";
            this.Text = "RuleSetEditForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ruleSetGrid)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		public RuleSetEditForm(RuleSet s)
		{
			this.rules = s;
            this.Text = "Edit rule set: " + this.rules.Name;
            base.Name = "Edit rule set: " + this.rules.Name;
			this.bs = new BindingSource
			{
				DataSource = this.rules.Rules
			};

			this.InitializeComponent();
			this.ruleSetGrid.DataSource = this.bs;
			this.ruleSetGrid.SelectionChanged += new EventHandler(this.HandleSelectionChanged);
			this.ruleSetGrid.KeyDown += new KeyEventHandler(this.HandleKeyDown);
			
            //befüllen der Listbox
            AssemblyHelper h = ContextManager.AssemblyHelper;
			foreach (Type item in h.DeviceRuleTypes)
			{
				ToolStripDeviceRuleButton tsbtn = new ToolStripDeviceRuleButton();
				tsbtn.Name = item.Name;
                tsbtn.Text = h.GetFriendlyName(item);
				tsbtn.Click += new EventHandler(this.HandleAddRule);
				tsbtn.Type = item;
				this.neuToolStripMenuItem.DropDownItems.Add(tsbtn);
			}

			this.ruleInfo.SelectedObject = null;
			this.beginLearnToolStrip.Click += new EventHandler(this.HandleBeginLearnClick);
			this.deleteToolStripMenuItem.Click += new EventHandler(this.HandleDeleteClick);
			this.cancelLearnToolStripMenuItem.Click += new EventHandler(this.HandleCancelLearnClick);
			this.cancelLearnToolStripMenuItem.Enabled = false;
			this.ruleInfo.KeyDown += new KeyEventHandler(this.HandleKeyDown);
		}
		private void HandleKeyDown(object s, KeyEventArgs e)
		{
			Keys keyCode = e.KeyCode;
			if (keyCode != Keys.Delete)
			{
				if (keyCode != Keys.C)
				{
					if (keyCode != Keys.L)
					{
						int dec = (int)e.KeyCode;
						if (dec >= 96 && dec <= 105)
						{
							int id = dec - 96;
							if (e.Control)
							{
								e.Handled = true;
								this.HandleAddRule(id);
							}
						}
						if (dec >= 48 && dec <= 57)
						{
							int id = dec - 48;
							if (e.Control)
							{
								this.HandleAddRule(id);
								e.Handled = true;
							}
						}
					}
					else
					{
						if (e.Control && !this.learning)
						{
							this.BeginLearn();
							e.Handled = true;
						}
					}
				}
				else
				{
					if (e.Control && this.learning)
					{
						this.CancelLearn();
						e.Handled = true;
					}
				}
			}
			else
			{
				this.DeleteSelected();
			}
		}
		private void HandleDeleteClick(object s, EventArgs e)
		{
			this.DeleteSelected();
		}
		private void HandleCancelLearnClick(object s, EventArgs e)
		{
			this.CancelLearn();
		}
		private void HandleAddRule(object o, EventArgs e)
		{
			this.AddRule((o as ToolStripDeviceRuleButton).Type);
		}
		private void HandleAddRule(int id)
		{
			ToolStripItem type = (this.neuToolStripMenuItem.DropDownItems.Count > id) ? this.neuToolStripMenuItem.DropDownItems[id] : null;
			if (type != null)
			{
				this.AddRule((type as ToolStripDeviceRuleButton).Type);
			}
		}
		private void HandleBeginLearnClick(object s, EventArgs e)
		{
			this.BeginLearn();
		}
		private void HandleLearnMessage(object s, MidiEventArgs e)
		{
			this.currentlearn.TryLearnMessage(e.m);
			if (base.InvokeRequired)
			{
				base.Invoke(new Action(this.UpdateUiPostLearnMsg));
			}
			else
			{
				this.UpdateUiPostLearnMsg();
			}
		}
		private void UpdateUiPostLearnMsg()
		{
			this.ruleSetGrid.Refresh();
			this.ruleInfo.Refresh();
		}
		private void HandleLearnFinished(object s, EventArgs e)
		{
			((ILearnable)s).LearningFinished -= new EventHandler(this.HandleLearnFinished);
			this.learning = false;
			this.currentlearn = null;
			if (base.InvokeRequired)
			{
				base.Invoke(new Action(this.updateUIPostLearn));
			}
			else
			{
				this.updateUIPostLearn();
			}
			this.rules.InputUsed = false;
		}
		private void updateUIPostLearn()
		{
			this.beginLearnToolStrip.Enabled = true;
			this.cancelLearnToolStripMenuItem.Enabled = false;
			this.rules.InputDevice.LearnMessage -= new EventHandler<MidiEventArgs>(this.HandleLearnMessage);
			this.rules.InputDevice.LeaveLearnMode();
			this.ruleSetGrid.Refresh();
			this.ruleInfo.Refresh();
		}
		private void HandleSelectionChanged(object s, EventArgs e)
		{
			if (this.ruleSetGrid.SelectedRows.Count > 0)
			{
				DataGridViewRow selected = this.ruleSetGrid.SelectedRows[0];
				DeviceRule obj = this.rules.Rules[selected.Index];
				this.ruleInfo.SelectedObject = obj;
				this.ruleInfo.ExpandAllGridItems();
			}
			else
			{
				this.ruleInfo.SelectedObject = null;
			}
		}
		private void CancelLearn()
		{
			this.currentlearn.CancelLearn();
			this.currentlearn = null;
		}
		private void DeleteSelected()
		{
			if (this.ruleSetGrid.SelectedRows.Count == 1)
			{
				DeviceRule rs = (DeviceRule)this.ruleSetGrid.SelectedRows[0].DataBoundItem;
				this.rules.DeleteRule(rs);
				if (this.ruleInfo.SelectedObject == rs)
				{
					this.ruleInfo.SelectedObject = null;
				}
			}
		}
		private void AddRule(Type t)
		{
			DeviceRule r = this.rules.createRule(t);
			if (r != null)
			{
				this.rules.AddRule(r);
			}
		}
		private void BeginLearn()
		{
			if (this.ruleSetGrid.SelectedRows.Count > 0)
			{
				if (this.rules.InputDevice != null && !this.learning)
				{
					this.learning = true;
					this.beginLearnToolStrip.Enabled = false;
					this.cancelLearnToolStripMenuItem.Enabled = true;
					DataGridViewRow selected = this.ruleSetGrid.SelectedRows[0];
					DeviceRule obj = this.rules.Rules[selected.Index];
					obj.LearningFinished += new EventHandler(this.HandleLearnFinished);
					obj.BeginLearn();
					this.currentlearn = obj;
					this.ruleSetGrid.Refresh();
					this.ruleInfo.Refresh();
					this.rules.InputUsed = true;
					this.rules.InputDevice.LearnMessage += new EventHandler<MidiEventArgs>(this.HandleLearnMessage);
					this.rules.InputDevice.EnterLearnMode();
				}
			}
		}
	}
}
