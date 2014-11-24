using NAudio.Midi;
using System;
using System.Diagnostics;
namespace MidiPlugin
{
	public class MidiInput : MidiDev, IDisposable
	{
		private bool LearnMode;
		public event EventHandler<MidiEventArgs> MessageReceived;
		public event EventHandler<MidiEventArgs> LearnMessage;
		public MidiIn InputDevice
		{
			get;
			private set;
		}
		public MidiInput(int devId)
		{
			base.DeviceID = new DeviceId();
			base.DeviceID.id = devId;
			base.DeviceID.t = EDeviceType.In;
			base.DeviceName = MidiIn.DeviceInfo(devId).ProductName;
			this.InputDevice = new MidiIn(devId);
			this.InputDevice.MessageReceived += new EventHandler<MidiInMessageEventArgs>(this.HandleMsgReceived);
		}
		public void Start()
		{
			this.InputDevice.Start();
		}
		public void Stop()
		{
			this.InputDevice.Stop();
			this.InputDevice.Reset();
		}
		private void HandleMsgReceived(object s, MidiInMessageEventArgs e)
		{
			MidiMessage msg = default(MidiMessage);
			msg.channel = (byte)(e.MidiEvent.Channel);
			msg.message = (byte)(e.RawMessage - (msg.channel - 1));
			msg.data1 = (byte)(e.RawMessage >> 8);
			msg.data2 = (byte)(e.RawMessage >> 16);
			this.OnMessageReceived(msg);
			Debug.WriteLine("in:" + msg.Data);
		}
		protected void OnMessageReceived(MidiMessage m)
		{
			if (this.LearnMode)
			{
				if (this.LearnMessage != null)
				{
					this.LearnMessage(this, new MidiEventArgs
					{
						m = m
					});
				}
			}
			else
			{
				if (this.MessageReceived != null)
				{
					this.MessageReceived(this, new MidiEventArgs
					{
						m = m
					});
				}
			}
		}
		public void EnterLearnMode()
		{
			this.LearnMode = true;
		}
		public void LeaveLearnMode()
		{
			this.LearnMode = false;
		}
		public void Dispose()
		{
			this.InputDevice.Stop();
			this.InputDevice.Dispose();
		}
	}
}
