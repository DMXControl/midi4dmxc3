using Lumos.GUI.Input;
using org.dmxc.lumos.Kernel.Input;
using System;
using System.Collections.Generic;
namespace MidiPlugin
{
	public class MidiInputLayer : AbstractGUIInputLayer, IDisposable
	{
		private RuleSet ruleset;
		private Dictionary<string, IInputChannel> inputchannels = new Dictionary<string, IInputChannel>();
		public MidiInputLayer(string sessionName, RuleSet rs) : base(new InputID(rs.GUID, sessionName), rs.Name)
		{
			this.ruleset = rs;
			this.ruleset.RuleAdded += new EventHandler<RuleSet.RuleEventArgs>(this.HandleItemAdd);
			this.ruleset.RuleDeleted += new EventHandler<RuleSet.RuleEventArgs>(this.HandleItemDeleted);
			this.ruleset.NameChanged += new EventHandler(this.HandleNameChanged);
		}
		private void HandleNameChanged(object s, EventArgs e)
		{
			this.ruleset.InputLayer.Name = this.ruleset.Name;
		}
		private void HandleItemDeleted(object s, RuleSet.RuleEventArgs e)
		{
			this.RemoveInputChannel(this.inputchannels[e.Rule.GUID]);
		}
		private void HandleItemAdd(object s, RuleSet.RuleEventArgs e)
		{
			DeviceRule item = e.Rule;
			MidiInputChannel ic = item.GetInputChannel(this);
			if (ic != null)
			{
				this.inputchannels.Add(e.Rule.GUID, ic);
				this.AddInputChannel(ic);
			}
		}
		public void Dispose()
		{
			List<IInputChannel> Channels = new List<IInputChannel>(base.Channels);
			foreach (IInputChannel item in Channels)
			{
				this.RemoveInputChannel(item);
			}
		}
	}
}
