using Lumos.GUI;
using Lumos.GUI.Input;
using LumosLIB.Kernel.Log;
using LumosLIB.Tools;
using org.dmxc.lumos.Kernel.Resource;
using System;
using System.ComponentModel;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;
using System.Xml.Linq;
namespace MidiPlugin
{
	public class RuleSet : IDisposable
	{
		public class RuleEventArgs : EventArgs
		{
			public DeviceRule Rule
			{
				get;
				set;
			}
		}
		private const int StatusMask = -256;
		protected const int DataMask = 255;
		private const int Data1Mask = -65281;
		private const int Data2Mask = 65535;
		private static ILumosLog log = LumosLogger.getInstance(typeof(RuleSet));
		private string name;
		public MidiInput InputDevice;
		public MidiOutput OutputDevice;
		public bool InputUsed;
		public bool OutputUsed;
		public BindingList<DeviceRule> Rules;
		private RuleSetEditForm editWindow;
		public event EventHandler<MidiEventArgs> SendMessage;
		public event EventHandler NameChanged;
		public event EventHandler<RuleSet.RuleEventArgs> RuleAdded;
		public event EventHandler<RuleSet.RuleEventArgs> RuleDeleted;
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
				if (this.NameChanged != null)
				{
					this.NameChanged(this, null);
				}
			}
		}
		public int RuleCount
		{
			get
			{
				return this.Rules.Count;
			}
		}
		public string InputDeviceID
		{
			get
			{
				return (this.InputDevice != null) ? this.InputDevice.DeviceID.ToString() : string.Empty;
			}
			set
			{
				if (!this.InputUsed)
				{
					if (this.InputDevice != null)
					{
						this.InputDevice.MessageReceived -= new EventHandler<MidiEventArgs>(this.HandleMessageReceived);
					}
					DeviceId devId = DeviceId.Parse(value);
					if (devId != null && ContextManager.DeviceInformation.InputDevices.Any((MidiInput i) => i.DeviceID.Equals(devId)))
					{
						this.InputDevice = ContextManager.DeviceInformation.InputDevices.First((MidiInput i) => i.DeviceID.Equals(devId));
						this.InputDevice.MessageReceived += new EventHandler<MidiEventArgs>(this.HandleMessageReceived);
					}
					else
					{
						this.InputDevice = null;
					}
				}
			}
		}
		public string OutputDeviceID
		{
			get
			{
				return (this.OutputDevice != null) ? this.OutputDevice.DeviceID.ToString() : string.Empty;
			}
			set
			{
				if (!this.OutputUsed)
				{
					DeviceId devId = DeviceId.Parse(value);
					if (devId != null && ContextManager.DeviceInformation.OutputDevices.Any((MidiOutput i) => i.DeviceID.Equals(devId)))
					{
						this.OutputDevice = ContextManager.DeviceInformation.OutputDevices.First((MidiOutput i) => i.DeviceID.Equals(devId));
					}
					else
					{
						this.OutputDevice = null;
					}
				}
			}
		}
		public string GUID
		{
			get;
			private set;
		}
		[Browsable(false)]
		public MidiInputLayer InputLayer
		{
			get;
			private set;
		}
		public static RuleSet Load(ManagedTreeItem mti)
		{
			RuleSet ret = new RuleSet(false);
			Pair<string, object> nameAt = mti.Attributes.FirstOrDefault((Pair<string, object> i) => i.Left == "Name");
			ret.Name = ((nameAt != null) ? ((string)nameAt.Right) : "");
			Pair<string, object> inp = mti.Attributes.FirstOrDefault((Pair<string, object> i) => i.Left == "InputDeviceID");
			ret.InputDeviceID = ((inp != null) ? ((string)inp.Right) : null);
			Pair<string, object> outp = mti.Attributes.FirstOrDefault((Pair<string, object> i) => i.Left == "OutputDeviceID");
			ret.OutputDeviceID = ((outp != null) ? ((string)outp.Right) : null);
			if (mti.hasValue<string>("GUID"))
			{
				ret.GUID = mti.getValue<string>("GUID");
			}
			else
			{
				ret.GUID = Guid.NewGuid().ToString();
			}
			ret.genInputLayer();
			foreach (ManagedTreeItem item in mti.GetChildren("Rule"))
			{
                    if (DeviceRule.Load(ret, item) == null)
                    {
                        RuleSet.log.Warn("Failed to load DeviceRule in RuleSet {0}, Invalid Cast and/or constructor missing!", ret.Name);
                    }
			}
			foreach (DeviceRule item2 in ret.Rules)
			{
				item2.UpdateBacktrack();
			}
			return ret;
		}
		private void genInputLayer()
		{
			this.InputLayer = new MidiInputLayer(InputLayerManager.getInstance().SessionName, this);
		}
		public void Save(ManagedTreeItem mti)
		{
            ContextManager.log.Debug("Saving RuleSet {0}", Name);
			mti.setValue<string>("Name", this.Name);
			mti.setValue<string>("InputDeviceID", this.InputDeviceID);
			mti.setValue<string>("OutputDeviceID", this.OutputDeviceID);
			mti.setValue<string>("GUID", this.GUID);
			foreach (DeviceRule rule in this.Rules)
			{
				ManagedTreeItem mtir = new ManagedTreeItem("Rule");
				rule.Save(mtir);
				mti.AddChild(mtir);
			}
		}
		protected void OnSendMessage(object s, MidiEventArgs m)
		{
			if (this.OutputDevice != null)
			{
				this.OutputUsed = true;
                int msg = (m.m.channel + m.m.message) + (m.m.data1 << 8) + (m.m.data2 << 16) - 1;
                try
                {
                    this.OutputDevice.OutputDevice.Send(msg);
                }
                catch(Exception)
                {
                    ContextManager.log.Warn("Error sending Midi Message to device {0}, message: {1}.{2}, {3},{4}", OutputDevice.DeviceID, m.m.channel, m.m.message, m.m.data1, m.m.data2);
                }
				this.OutputUsed = false;
			}
			if (this.SendMessage != null)
			{
				this.SendMessage(this, m);
			}
		}
		public RuleSet() : this(true)
		{
		}
		private RuleSet(bool genGuid)
		{
			this.Rules = new BindingList<DeviceRule>();
			this.Name = "New RuleSet";
			this.editWindow = new RuleSetEditForm(this);
			if (genGuid)
			{
				this.GUID = Guid.NewGuid().ToString();
				this.genInputLayer();
			}
		}
		private void HandleMessageReceived(object s, MidiEventArgs e)
		{
			foreach (DeviceRule item in this.Rules)
			{
				item.Process(e.m);
			}
		}
		public DeviceRule createRule(string t)
		{
            var type = ContextManager.AssemblyHelper.DeviceRuleTypes.FirstOrDefault(j => j.FullName == t);
            if (type == null) return null;
            var obj = Activator.CreateInstance(type) as DeviceRule;

            return obj;
		}
		public void AddRule(DeviceRule r)
		{
			this.Rules.Add(r);
			r.MidiMessageSend += new EventHandler<MidiEventArgs>(this.OnSendMessage);
			this.OnRuleAdded(r);
		}
		public void DeleteRule(DeviceRule r)
		{
			this.Rules.Remove(r);
			r.MidiMessageSend -= new EventHandler<MidiEventArgs>(this.OnSendMessage);
			this.OnRuleDeleted(r);
		}
		public void OpenEditWindow()
		{
			WindowManager.getInstance().ShowWindow(this.editWindow, DockState.Float);
		}
		public void Dispose()
		{
			this.editWindow.Dispose();
		}
		protected void OnRuleAdded(DeviceRule r)
		{
			if (this.RuleAdded != null)
			{
				this.RuleAdded(this, new RuleSet.RuleEventArgs
				{
					Rule = r
				});
			}
		}
		protected void OnRuleDeleted(DeviceRule r)
		{
			if (this.RuleDeleted != null)
			{
				this.RuleDeleted(this, new RuleSet.RuleEventArgs
				{
					Rule = r
				});
			}
		}

        public XElement Serialize()
        {
            var xElement = new XElement("RuleSet");
            xElement.Add(new XAttribute("Name", this.name));
            foreach (var rule in this.Rules)
            {
                xElement.Add(rule.Serialize());
            }
            return xElement;
            throw new NotImplementedException();
        }

        public static RuleSet LoadFromXml(XElement element)
        {
            var ruleset = new RuleSet(true);
            ruleset.name = element.Attribute("Name").Value;
            ruleset.genInputLayer();
            foreach (var item in element.Elements("Rule"))
            {
                ruleset.AddRule(DeviceRule.LoadFromXml(item));
            }
            return ruleset;
        }
    }
}
