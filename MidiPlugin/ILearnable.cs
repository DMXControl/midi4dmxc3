using System;
namespace MidiPlugin
{
	public interface ILearnable
	{
		event EventHandler LearningFinished;
		string LearnStatus
		{
			get;
		}
		void BeginLearn();
		void CancelLearn();
		bool TryLearnMessage(MidiMessage m);
	}
}
