using org.dmxc.lumos.Kernel.Input;
using System;
namespace MidiPlugin
{
	public abstract class MidiInputChannel : AbstractInputChannel
	{
		private DeviceRule rule;
		public MidiInputChannel(IInputLayer parent, DeviceRule rule, bool registerValueChange = true, bool registerFB = true) : base(rule.GUID, parent)
		{
            base.AutofireChangedEvent = true;
			rule.NameChanged += new EventHandler(this.HandleNameChanged);
			base.Name = rule.Name;
			if (registerValueChange)
			{
				rule.ValueChanged += new EventHandler<ValueChangedEventArgs>(this.HandleValueChanged);
			}
			if (registerFB)
			{
				this.FeedbackCB = new InputLayerChangedCallback(this.HandleFeedback);
			}
			this.rule = rule;
            this.Changed += HandleChanged;
		}

        private void HandleChanged(IInputChannel sender, object newValue)
        {
            if(newValue is double)
            {
                this.rule.Value = (double)newValue;
            }
        }
		protected virtual void HandleNameChanged(object s, EventArgs e)
		{
			base.Name = this.rule.Name;
		}
		protected virtual void HandleValueChanged(object sender, ValueChangedEventArgs e)
		{
            if(!bBacktrack)
			this.ChannelValue = e.newValue;
		}
		protected virtual bool HandleFeedback(InputChannelID id, object newValue)
		{
			bool result;
			if (newValue is double)
			{
				this.rule.Value = (double)newValue;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

        public void UpdateBacktrackValue(double val)
        {
            this.AutofireChangedEvent = false;
            bBacktrack = true;
            this.rule.Value = val;
            bBacktrack = false;
            this.AutofireChangedEvent = true;
        }
        private bool bBacktrack = false;
	}
}
