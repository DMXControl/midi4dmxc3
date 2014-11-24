using org.dmxc.lumos.Kernel.Input;
using System;
namespace MidiPlugin
{
	public class MidiRangeInputChannel : MidiInputChannel
	{
		protected override EInputChannelType ChannelType
		{
			get
			{
				return EInputChannelType.RANGE;
			}
		}
		public MidiRangeInputChannel(IInputLayer parent, DeviceRule r) : base(parent, r, true, true)
		{
		}
	}
}
