using System;
using System.Windows.Forms;
namespace MidiPlugin
{
	internal class ToolStripDeviceRuleButton : ToolStripMenuItem
	{
		internal Type Type;
		internal DeviceRule CreateDeviceRule()
		{
			DeviceRule result;
			if (this.Type == null)
			{
				result = null;
			}
			else
			{
				result = (DeviceRule)Activator.CreateInstance(this.Type);
			}
			return result;
		}
	}
}
