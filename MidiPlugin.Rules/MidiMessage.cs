using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
namespace MidiPlugin
{
	[TypeConverter(typeof(ValueTypeTypeConverter<MidiMessage>))]
	[StructLayout(LayoutKind.Explicit)]
	public struct MidiMessage
	{
		[FieldOffset(0)]
		public byte channel;
		[FieldOffset(1)]
		public byte message;
		[FieldOffset(2)]
		public byte data1;
		[FieldOffset(3)]
		public byte data2;
		[FieldOffset(0)]
		public int Data;
		public byte Channel
		{
			get
			{
				return this.channel;
			}
			set
			{
				this.channel = value;
			}
		}
		public byte Message
		{
			get
			{
				return this.message;
			}
			set
			{
				this.message = value;
			}
		}
		public byte Data1
		{
			get
			{
				return this.data1;
			}
			set
			{
				this.data1 = value;
			}
		}
		public byte Data2
		{
			get
			{
				return this.data2;
			}
			set
			{
				this.data2 = value;
			}
		}
		public bool EqualsSimple(MidiMessage other)
		{
			return this.channel == other.channel && this.message == other.message && this.data1 == other.data1;
		}
		public override string ToString()
		{
			return string.Format("{0}:{1} D1:{2} D2:{3}", new object[]
			{
				this.channel,
				this.message,
				this.data1,
				this.data2
			});
		}
		public static bool operator ==(MidiMessage a, MidiMessage b)
		{
			return a.Data == b.Data;
		}
		public static bool operator !=(MidiMessage a, MidiMessage b)
		{
			return a.Data != b.Data;
		}
		public override int GetHashCode()
		{
			return this.Data;
		}
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
	}
}
