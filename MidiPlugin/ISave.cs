using org.dmxc.lumos.Kernel.Resource;
using System;
namespace MidiPlugin
{
	public interface ISave
	{
		void Init(ManagedTreeItem i);
		void Save(ManagedTreeItem i);
	}
}
