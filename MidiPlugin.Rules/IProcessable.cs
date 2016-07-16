using System;
namespace MidiPlugin
{
	public interface IProcessable
	{
		event EventHandler<MidiEventArgs> MidiMessageSend;
		double Value
		{
			get;
		}
		void Process(MidiMessage m);
	}
}
