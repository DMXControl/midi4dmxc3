using System;
namespace MidiPlugin
{
	public interface IBacktrack
	{
		bool UseBacktrack
		{
			get;
			set;
		}
		void UpdateBacktrack();
	}
}
