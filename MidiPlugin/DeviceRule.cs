using org.dmxc.lumos.Kernel.Input;
using org.dmxc.lumos.Kernel.Resource;
using System;
using System.Xml.Linq;
using System.Linq;
namespace MidiPlugin
{
    public abstract class DeviceRule : ILearnable, IProcessable, ISave, IBacktrack
    {
        private string name = ""; //set name to "" to ensure it won't get null
        public event EventHandler NameChanged;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;
        public event EventHandler<MidiEventArgs> MidiMessageSend;
        public abstract event EventHandler LearningFinished;
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
        public abstract string ControlType
        {
            get;
        }
        public string GUID
        {
            get;
            set;
        }
        public bool UseBacktrack
        {
            get;
            set;
        }
        public abstract double Value
        {
            get;
            set;
        }
        public abstract string LearnStatus
        {
            get;
            protected set;
        }
        public DeviceRule()
        {
            this.GUID = Guid.NewGuid().ToString();
        }
        protected void OnSendMessage(MidiMessage m)
        {
            if (this.MidiMessageSend != null)
            {
                this.MidiMessageSend(this, new MidiEventArgs
                {
                    m = m
                });
            }
        }
        protected void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, new ValueChangedEventArgs
                {
                    newValue = this.Value
                });
            }
        }
        public static DeviceRule Load(RuleSet r, ManagedTreeItem item)
        {
            DeviceRule result;
            if (!item.hasValue<Type>("Type"))
            {
                result = null;
            }
            else
            {
                Type type = item.getValue<Type>("Type");
                DeviceRule o = r.createRule(type);
                if (o == null)
                {
                    result = null;
                }
                else
                {
                    o.LoadGUID(item);
                    r.AddRule(o);
                    o.Init(item);
                    result = o;
                }
            }
            return result;
        }
        public abstract void UpdateBacktrack();
        public abstract void Process(MidiMessage m);
        public abstract void BeginLearn();
        public abstract void CancelLearn();
        public abstract bool TryLearnMessage(MidiMessage m);
        private void LoadGUID(ManagedTreeItem i)
        {
            if (i.hasValue<string>("GUID"))
            {
                this.GUID = i.getValue<string>("GUID");
            }
        }
        public virtual void Init(ManagedTreeItem i)
        {
            if (i.hasValue<string>("Name"))
            {
                this.Name = i.getValue<string>("Name");
            }
            if (i.hasValue<bool>("UseBacktrack"))
            {
                this.UseBacktrack = i.getValue<bool>("UseBacktrack");
            }
        }
        public virtual void Save(ManagedTreeItem i)
        {
            Type type = base.GetType();
            i.setValue<Type>("Type", type);
            i.setValue<string>("Name", this.Name);
            i.setValue<string>("GUID", this.GUID);
            i.setValue<bool>("UseBacktrack", this.UseBacktrack);
        }
        public abstract MidiInputChannel GetInputChannel(IInputLayer parent);

        public XElement Serialize()
        {
            var xElement = new XElement("Rule");
            var type = GetType();
            xElement.Add(new XElement("Type", type.FullName), new XAttribute("Name", this.Name), new XAttribute("UseBacktrack", this.UseBacktrack));
            Serialize(xElement);
            return xElement;
        }

        protected abstract void Serialize(XElement item);

        protected abstract void Deserialize(XElement item);
        public static DeviceRule LoadFromXml(XElement item)
        {
            var type = ContextManager.AssemblyHelper.DeviceRuleTypes.FirstOrDefault(j => j.FullName == item.Element("Type").Value);
            if (type == null) return null;
            var obj = Activator.CreateInstance(type) as DeviceRule;
            obj.Deserialize(item);
            obj.name = item.Attribute("Name").Value;
            obj.UseBacktrack = bool.Parse(item.Attribute("UseBacktrack").Value);
            return obj;
        }
    }
}
