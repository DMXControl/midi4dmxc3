using org.dmxc.lumos.Kernel.Input;
using org.dmxc.lumos.Kernel.Resource;
using System;
using System.Xml.Linq;
namespace MidiPlugin
{
    [FriendlyName("Button")]
	public class ButtonRule : DeviceRule
	{
		private const string nolearn = "LearnMode disabled.";
		private const string learn1 = "Press the button now.";
		private const string learn2 = "Release the button now.";
		private bool state;
		private MidiInputChannel c = null;
		private bool first = true;
		public override event EventHandler LearningFinished;
		public override string ControlType
		{
			get
			{
				return "Button";
			}
		}
		public bool State
		{
			get
			{
				return this.state;
			}
			private set
			{
				this.state = value;
				this.UpdateBacktrack();
			}
		}
		public MidiMessage EnableMessage
		{
			get;
			set;
		}
		public MidiMessage EnabledBacktrack
		{
			get;
			set;
		}
		public MidiMessage DisableMessage
		{
			get;
			set;
		}
		public MidiMessage DisabledBacktrack
		{
			get;
			set;
		}
		public byte Treshold
		{
			get;
			set;
		}
		public bool IsToggle
		{
			get;
			set;
		}
		public override string LearnStatus
		{
			get;
			protected set;
		}
		public bool LearnMode
		{
			get;
			private set;
		}
		public override double Value
		{
			get
			{
				return (double)(this.State ? 1 : 0);
			}
			set
			{
				this.State = (value >= 0.5);
			}
		}
		public ButtonRule()
		{
			this.LearnStatus = "LearnMode disabled.";
		}
		public override MidiInputChannel GetInputChannel(IInputLayer parent)
		{
			if (this.c == null)
			{
				this.c = new ButtonInputChannel(this, parent);
			}
			return this.c;
		}
		public override void BeginLearn()
		{
			this.LearnMode = true;
			this.first = true;
			this.LearnStatus = "Press the button now.";
		}
		public override void CancelLearn()
		{
			this.EndLearn();
		}
		private void EndLearn()
		{
			this.LearnMode = false;
			this.LearnStatus = "LearnMode disabled.";
			if (this.LearningFinished != null)
			{
				this.LearningFinished(this, EventArgs.Empty);
			}
		}
		public override bool TryLearnMessage(MidiMessage m)
		{
			bool result;
			if (!this.LearnMode)
			{
				result = false;
			}
			else
			{
				if (this.first)
				{
					this.EnableMessage = m;
					this.first = false;
					this.LearnStatus = "Release the button now.";
					this.Treshold = (byte)(m.data2 - 1);
					if (base.UseBacktrack)
					{
						this.EnabledBacktrack = m;
					}
					if (this.IsToggle)
					{
						this.DisableMessage = m;
						if (base.UseBacktrack)
						{
							MidiMessage m2 = m;
							m2.data2 = 0;
							this.DisabledBacktrack = m2;
						}
						this.EndLearn();
					}
				}
				else
				{
					if (m == this.EnableMessage)
					{
						result = false;
						return result;
					}
					this.DisableMessage = m;
					if (base.UseBacktrack)
					{
						this.DisabledBacktrack = m;
					}
					this.EndLearn();
				}
				result = true;
			}
			return result;
		}
		public override void Process(MidiMessage m)
		{
			if (this.State)
			{
				if (this.IsToggle)
				{
					if (m.EqualsSimple(this.EnableMessage))
					{
						this.State = false;
					}
					base.OnValueChanged();
				}
				else
				{
					if (m.EqualsSimple(this.DisableMessage))
					{
						this.State = false;
					}
					base.OnValueChanged();
				}
			}
			else
			{
				if (m.EqualsSimple(this.EnableMessage) && m.data2 > this.Treshold)
				{
					this.State = true;
				}
				base.OnValueChanged();
			}
		}
		public override void Init(ManagedTreeItem i)
		{
			base.Init(i);
			if (i.hasValue<byte>("Treshold"))
			{
				this.Treshold = i.getValue<byte>("Treshold");
			}
			if (i.hasValue<bool>("IsToggle"))
			{
				this.IsToggle = i.getValue<bool>("IsToggle");
			}
			if (i.hasValue<int>("EnableMessage"))
			{
				this.EnableMessage = new MidiMessage
				{
					Data = i.getValue<int>("EnableMessage")
				};
			}
			if (i.hasValue<int>("EnabledBacktrack"))
			{
				this.EnabledBacktrack = new MidiMessage
				{
					Data = i.getValue<int>("EnabledBacktrack")
				};
			}
			if (i.hasValue<int>("DisableMessage"))
			{
				this.DisableMessage = new MidiMessage
				{
					Data = i.getValue<int>("DisableMessage")
				};
			}
			if (i.hasValue<int>("DisabledBacktrack"))
			{
				this.DisabledBacktrack = new MidiMessage
				{
					Data = i.getValue<int>("DisabledBacktrack")
				};
			}
			if (i.hasValue<bool>("State"))
			{
				this.State = i.getValue<bool>("State");
			}
		}
		public override void Save(ManagedTreeItem i)
		{
            MidiPlugin.log.Debug("Saving ButtonRule {0}, {1},  {2}", EnableMessage.Data, DisableMessage.Data, Treshold);
			base.Save(i);
			i.setValue<bool>("State", this.State);
			i.setValue<byte>("Treshold", this.Treshold);
			i.setValue<bool>("IsToggle", this.IsToggle);
			i.setValue<int>("EnableMessage", this.EnableMessage.Data);
			i.setValue<int>("EnabledBacktrack", this.EnabledBacktrack.Data);
			i.setValue<int>("DisableMessage", this.DisableMessage.Data);
			i.setValue<int>("DisabledBacktrack", this.DisabledBacktrack.Data);
		}
        protected override void Serialize(XElement item)
        {
            item.Add(new XAttribute("Treshold", this.Treshold));
            item.Add(new XAttribute("IsToggle", this.IsToggle));
            item.Add(new XAttribute("EnableMessage", this.EnableMessage.Data));
            item.Add(new XAttribute("EnabledBacktrack", this.EnabledBacktrack.Data));
            item.Add(new XAttribute("DisableMessage", this.DisableMessage.Data));
            item.Add(new XAttribute("DisabledBacktrack", this.DisabledBacktrack.Data));
        }

        protected override void Deserialize(XElement item)
        {
            this.Treshold = byte.Parse(item.Attribute("Treshold").Value);
            this.IsToggle = bool.Parse(item.Attribute("IsToggle").Value);
            this.EnableMessage = new MidiMessage { Data = int.Parse(item.Attribute("EnableMessage").Value) };
            this.EnabledBacktrack = new MidiMessage { Data = int.Parse(item.Attribute("EnableMessage").Value) };
            this.DisableMessage = new MidiMessage { Data = int.Parse(item.Attribute("DisableMessage").Value) };
            this.DisabledBacktrack = new MidiMessage { Data = int.Parse(item.Attribute("DisabledBacktrack").Value) };
        }
		public override void UpdateBacktrack()
		{
			if (base.UseBacktrack)
			{
				if (this.State)
				{
					base.OnSendMessage(this.EnabledBacktrack);
				}
				else
				{
					base.OnSendMessage(this.DisabledBacktrack);
				}
			}
		}
	}
}
