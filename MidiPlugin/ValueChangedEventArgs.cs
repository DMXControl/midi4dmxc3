using System;
namespace MidiPlugin
{
	public class ValueChangedEventArgs : EventArgs
	{
		public double newValue
		{
			get;
			set;
		}
	}
}
