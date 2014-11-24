using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.dmxc.lumos.Kernel.Input;

namespace MidiPlugin
{
    [FriendlyName("Fader/Rotary")]
    public class SliderRule : DeviceRule
    {

 
        private const string nolearn = "LearnMode disabled.";
        private const string learn1 = "Move the fader.";

        private MidiInputChannel c;

        public override event EventHandler LearningFinished;

        public override string ControlType
        {
            get { return "Fader"; }
        }

        private double value;
        public override double Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value != this.value)
                {
                    this.value = Math.Min(1, Math.Max(0, value));
                    UpdateBacktrack();
                }
            }
        }

        public MidiMessage SliderMessage
        {
            get;
            set;
        }

        public MidiMessage MinimumBacktrack
        {
            get;
            set;
        }

        public MidiMessage MaximumBacktrack
        {
            get;
            set;
        }

        public bool LearnMode
        {
            get;
            private set;
        }

        public override void UpdateBacktrack()
        {
            int delta = (int)(this.MaximumBacktrack.data2 - this.MinimumBacktrack.data2);
            MidiMessage msg = this.MaximumBacktrack;
            msg.data2 = (byte)((double)this.MinimumBacktrack.data2 + (double)delta * this.Value);
            base.OnSendMessage(msg);
        }

        public override void Process(MidiMessage m)
        {
            if (m.EqualsSimple(SliderMessage))
            {
                Value = m.data2 / 127d; //hardcoded velocity
                base.OnValueChanged();
            }
        }

        public override void BeginLearn()
        {
            LearnMode = true;
            LearnStatus = learn1;
        }

        public override string LearnStatus
        {
            get;
            protected set;
        }
        public override void CancelLearn()
        {
            LearnMode = false;
            LearnStatus = nolearn;
        }

        public override bool TryLearnMessage(MidiMessage m)
        {
            if(LearnMode)
            {
                SliderMessage = new MidiMessage { channel = m.channel, data1 = m.data1, data2 = 0, message = m.message, };
                MinimumBacktrack = new MidiMessage { channel = m.channel, data1 = m.data1, data2 = 0, message = m.message, };
                MaximumBacktrack = new MidiMessage { channel = m.channel, data1 = m.data1, data2 = 127, message = m.message, };
                LearnMode = false;
                LearnStatus = nolearn;
                if (this.LearningFinished != null)
                {
                    this.LearningFinished(this, EventArgs.Empty);
                }
                return true;
            }
            return false;
        }

        public override MidiInputChannel GetInputChannel(IInputLayer parent)
        {
            if (this.c == null)
            {
                this.c = new MidiRangeInputChannel(parent, this);
            }
            return this.c;
        }

        public override void Init(org.dmxc.lumos.Kernel.Resource.ManagedTreeItem i)
        {
            base.Init(i);
            if (i.hasValue<int>("MinimumBacktrack"))
            {
                this.MinimumBacktrack = new MidiMessage
                {
                    Data = i.getValue<int>("MinimumBacktrack")
                };
            }
            if (i.hasValue<int>("MaximumBacktrack"))
            {
                this.MaximumBacktrack = new MidiMessage
                {
                    Data = i.getValue<int>("MaximumBacktrack")
                };
            }
            if (i.hasValue<int>("Message"))
            {
                this.SliderMessage = new MidiMessage
                {
                    Data = i.getValue<int>("Message")
                };
            }
            if (i.hasValue<double>("Value"))
            {
                this.Value = i.getValue<double>("Value");
            }
        }

        public override void Save(org.dmxc.lumos.Kernel.Resource.ManagedTreeItem i)
        {
            MidiPlugin.log.Debug("Saving SliderRule {0}, {1},  {2}", SliderMessage.Data, MaximumBacktrack.Data, MinimumBacktrack.Data);
            base.Save(i);
            i.setValue<double>("Value", this.Value);
            i.setValue<int>("Message", this.SliderMessage.Data);
            i.setValue<int>("MinimumBacktrack", this.MinimumBacktrack.Data);
            i.setValue<int>("MaximumBacktrack", this.MaximumBacktrack.Data);
        }

        protected override void Serialize(System.Xml.Linq.XElement item)
        {
            item.Add(new XAttribute("SliderMessage", this.SliderMessage.Data), new XAttribute("MinimumBacktrack", this.MinimumBacktrack.Data), new XAttribute("MaximumBacktrack", this.MaximumBacktrack.Data));
        }
        protected override void Deserialize(XElement item)
        {
            this.SliderMessage = new MidiMessage { Data = int.Parse(item.Attribute("SliderMessage").Value) };
            this.MinimumBacktrack = new MidiMessage { Data = int.Parse(item.Attribute("MinimumBacktrack").Value) };
            this.MaximumBacktrack = new MidiMessage { Data = int.Parse(item.Attribute("MaximumBacktrack").Value) };
        }
    }
}
