using System;
namespace MidiPlugin
{
	public class DeviceId : IEquatable<DeviceId>
	{
		public EDeviceType t;
		public int id;
		public override string ToString()
		{
			return this.t.ToString() + "/" + this.id;
		}
		public static DeviceId Parse(string v)
		{
			DeviceId result;
			if (v == null)
			{
				result = null;
			}
			else
			{
				string[] sp = v.Split("/\\".ToCharArray());
				if (sp.Length != 2)
				{
					result = null;
				}
				else
				{
					DeviceId ret = new DeviceId();
					try
					{
						ret.t = (EDeviceType)Enum.Parse(typeof(EDeviceType), sp[0]);
						ret.id = int.Parse(sp[1]);
					}
					catch
					{
						result = null;
						return result;
					}
					result = ret;
				}
			}
			return result;
		}
		public bool Equals(DeviceId other)
		{
			return this.id == other.id && this.t == other.t;
		}
	}
}
