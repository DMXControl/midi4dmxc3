using Lumos.GUI.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
namespace MidiPlugin
{
	public class MidiInformation : IDisposable
	{
		public class RuleSetCollection : BindingList<RuleSet>
		{
			public event EventHandler<MidiInformation.RuleSetEventArgs> Added;
			public event EventHandler<MidiInformation.RuleSetEventArgs> Deleted;
			protected virtual void OnAdded(RuleSet rs)
			{
				if (this.Added != null)
				{
					this.Added(this, new MidiInformation.RuleSetEventArgs
					{
						rs = rs
					});
				}
			}
			protected virtual void OnDeleted(RuleSet rs)
			{
				if (this.Deleted != null)
				{
					this.Deleted(this, new MidiInformation.RuleSetEventArgs
					{
						rs = rs
					});
				}
			}
			public new void Add(RuleSet r)
			{
				base.Add(r);
				this.OnAdded(r);
			}
			public new void Remove(RuleSet r)
			{
				base.Remove(r);
				this.OnDeleted(r);
			}
			public new void Clear()
			{
				foreach (RuleSet item in this)
				{
					this.OnDeleted(item);
				}
				base.Clear();
			}
		}
		public class RuleSetEventArgs : EventArgs
		{
			public RuleSet rs
			{
				get;
				set;
			}
		}
		private bool disposed = false;
		public event EventHandler<MidiInformation.RuleSetEventArgs> RuleSetAdded;
		public event EventHandler<MidiInformation.RuleSetEventArgs> RuleSetDeleted;
		public MidiInformation.RuleSetCollection RuleSets
		{
			get;
			set;
		}
		public MidiInformation()
		{
			ContextManager.MidiInformation = this;
			this.RuleSets = new MidiInformation.RuleSetCollection();
			this.RuleSets.Added += new EventHandler<MidiInformation.RuleSetEventArgs>(this.OnRuleSetAdded);
			this.RuleSets.Deleted += new EventHandler<MidiInformation.RuleSetEventArgs>(this.OnRuleSetDeleted);
		}
		private void OnRuleSetAdded(object o, MidiInformation.RuleSetEventArgs rs)
		{
			if (rs.rs != null && rs.rs.InputLayer != null)
			{
				try
				{
					InputLayerManager.getInstance().registerInputLayer(rs.rs.InputLayer);
				}
				catch (Exception x)
				{
					Debug.WriteLine(x);
				}
				if (this.RuleSetAdded != null)
				{
					this.RuleSetAdded(this, rs);
				}
			}
		}
		private void OnRuleSetDeleted(object o, MidiInformation.RuleSetEventArgs rs)
		{
			if (this.RuleSetDeleted != null)
			{
				this.RuleSetDeleted(this, rs);
			}
			if (rs.rs != null && rs.rs.InputLayer != null)
			{
				InputLayerManager.getInstance().deregisterInputLayer(rs.rs.InputLayer);
				rs.rs.Dispose();
			}
		}
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.RuleSets.Clear();
				this.RuleSets.Added -= new EventHandler<MidiInformation.RuleSetEventArgs>(this.OnRuleSetAdded);
				this.RuleSets.Deleted -= new EventHandler<MidiInformation.RuleSetEventArgs>(this.OnRuleSetDeleted);
				this.RuleSets = null;
			}
		}
	}
}
