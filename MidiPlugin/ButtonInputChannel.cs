using org.dmxc.lumos.Kernel.Input;
using System;
namespace MidiPlugin
{
	public class ButtonInputChannel : MidiInputChannel
	{
		protected override EInputChannelType ChannelType
		{
			get
			{
				return EInputChannelType.BUTTON;
			}
		}
		public ButtonInputChannel(DeviceRule d, IInputLayer parent) : base(parent, d, true, true)
		{
		}
	}
}
