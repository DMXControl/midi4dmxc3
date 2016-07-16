using System;
namespace MidiPlugin
{
	public abstract class MidiDev
	{
		public DeviceId DeviceID
		{
			get;
			protected set;
		}
		public string DeviceName
		{
			get;
			protected set;
		}
	}
}
