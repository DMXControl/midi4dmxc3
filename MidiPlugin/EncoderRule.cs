using org.dmxc.lumos.Kernel.Input;
using org.dmxc.lumos.Kernel.Resource;
using System;
using System.Xml.Linq;
using System.Globalization;
namespace MidiPlugin
{
    [FriendlyName("Encoder")]
	public class EncoderRule : DeviceRule
	{
        private static NumberFormatInfo nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
		private const string nolearn = "LearnMode disabled.";
		private const string learn1 = "Turn the encoder clockwise.";
		private const string learn2 = "Turn the encoder counter clockwise.";
		private double value;
		private bool first = true;
		private MidiInputChannel c = null;
		public override event EventHandler LearningFinished;
		public MidiMessage CWMessage
		{
			get;
			set;
		}
		public MidiMessage MinimumBacktrack
		{
			get;
			set;
		}
		public MidiMessage CCWMessage
		{
			get;
			set;
		}
		public MidiMessage MaximumBacktrack
		{
			get;
			set;
		}
		public double Increment
		{
			get;
			set;
		}
		public override string ControlType
		{
			get
			{
				return "Encoder";
			}
		}
		public override double Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
				if (this.value > 1.0)
				{
					this.value = 1.0;
				}
				if (this.value < 0.0)
				{
					this.value = 0.0;
				}
				this.UpdateBacktrack();
			}
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
		public EncoderRule()
		{
			this.LearnStatus = nolearn;
		}
		public override void Init(ManagedTreeItem i)
		{
			base.Init(i);
			if (i.hasValue<double>("Increment"))
			{
				this.Increment = i.getValue<double>("Increment");
			}
			if (i.hasValue<int>("CWMessage"))
			{
				this.CWMessage = new MidiMessage
				{
					Data = i.getValue<int>("CWMessage")
				};
			}
			if (i.hasValue<int>("MinimumBacktrack"))
			{
				this.MinimumBacktrack = new MidiMessage
				{
					Data = i.getValue<int>("MinimumBacktrack")
				};
			}
			if (i.hasValue<int>("CCWMessage"))
			{
				this.CCWMessage = new MidiMessage
				{
					Data = i.getValue<int>("CCWMessage")
				};
			}
			if (i.hasValue<int>("MaximumBacktrack"))
			{
				this.MaximumBacktrack = new MidiMessage
				{
					Data = i.getValue<int>("MaximumBacktrack")
				};
			}
			if (i.hasValue<double>("Value"))
			{
				this.Value = i.getValue<double>("Value");
			}
		}
		public override void Save(ManagedTreeItem i)
		{

            MidiPlugin.log.Debug("Saving EncoderRule {0}, {1},  {2}", CWMessage.Data, CCWMessage.Data, Increment);
			base.Save(i);
			i.setValue<double>("Value", this.Value);
			i.setValue<double>("Increment", this.Increment);
			i.setValue<int>("CWMessage", this.CWMessage.Data);
			i.setValue<int>("MinimumBacktrack", this.MinimumBacktrack.Data);
			i.setValue<int>("CCWMessage", this.CCWMessage.Data);
			i.setValue<int>("MaximumBacktrack", this.MaximumBacktrack.Data);
		}

        protected override void Serialize(XElement item)
        {
			item.Add(new XAttribute("Increment", this.Increment.ToString(nfi)));
			item.Add(new XAttribute("CWMessage", this.CWMessage.Data));
			item.Add(new XAttribute("MinimumBacktrack", this.MinimumBacktrack.Data));
			item.Add(new XAttribute("CCWMessage", this.CCWMessage.Data));
            item.Add(new XAttribute("MaximumBacktrack", this.MaximumBacktrack.Data));
        }
        protected override void Deserialize(XElement item)
        {
            this.Increment = double.Parse(item.Attribute("Increment").Value, nfi);
            this.CWMessage = new MidiMessage { Data = int.Parse(item.Attribute("CWMessage").Value) };
            this.MinimumBacktrack = new MidiMessage { Data = int.Parse(item.Attribute("MinimumBacktrack").Value) };
            this.CCWMessage = new MidiMessage { Data = int.Parse(item.Attribute("CCWMessage").Value) };
            this.MaximumBacktrack = new MidiMessage { Data = int.Parse(item.Attribute("MaximumBacktrack").Value) };
        }
		public override void Process(MidiMessage m)
		{
			if (m.Equals(this.CWMessage))
			{
				this.Value += this.Increment;
				base.OnValueChanged();
			}
			if (m.Equals(this.CCWMessage))
			{
				this.Value -= this.Increment;
				base.OnValueChanged();
			}
		}
		public override void UpdateBacktrack()
		{
			int delta = (int)(this.MaximumBacktrack.data2 - this.MinimumBacktrack.data2);
			MidiMessage msg = this.MaximumBacktrack;
			msg.data2 = (byte)((double)this.MinimumBacktrack.data2 + (double)delta * this.Value);
			base.OnSendMessage(msg);
		}
		public override void BeginLearn()
		{
			this.LearnMode = true;
			this.first = true;
			this.LearnStatus = learn1;
		}
		public override void CancelLearn()
		{
			this.EndLearn();
		}
		private void EndLearn()
		{
			this.LearnMode = false;
			this.LearnStatus = nolearn;
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
					this.CWMessage = m;
					this.first = false;
					this.LearnStatus = learn2;
					if (base.UseBacktrack)
					{
						MidiMessage m2 = m;
						m2.data2 = 0;
						this.MinimumBacktrack = m2;
					}
				}
				else
				{
					if (m == this.CWMessage)
					{
						result = false;
						return result;
					}
					MidiMessage m2 = m;
					m2.data2 = 127;
					this.CCWMessage = m;
					if (base.UseBacktrack)
					{
						this.MaximumBacktrack = m2;
					}
					this.EndLearn();
				}
				result = true;
			}
			return result;
		}
		public override MidiInputChannel GetInputChannel(IInputLayer parent)
		{
			if (this.c == null)
			{
				this.c = new MidiRangeInputChannel(parent, this);
			}
			return this.c;
		}
	}
}
