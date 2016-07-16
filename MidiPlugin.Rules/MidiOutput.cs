using NAudio.Midi;
using System;
namespace MidiPlugin
{
	public class MidiOutput : MidiDev
	{
		public MidiOut OutputDevice
		{
			get;
			private set;
		}
		public MidiOutput(int devId)
		{
			base.DeviceID = new DeviceId();
			base.DeviceID.id = devId;
			base.DeviceID.t = EDeviceType.Out;
			base.DeviceName = MidiOut.DeviceInfo(devId).ProductName;
			this.OutputDevice = new MidiOut(base.DeviceID.id);
		}
	}
}
