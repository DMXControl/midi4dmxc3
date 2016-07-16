using LumosLIB.Kernel.Log;
using System;
namespace MidiPlugin
{
	internal static class ContextManager 
	{
		private static MidiInformation info;
		private static DeviceInformation dev;
		private static AssemblyHelper asm;
		public static MidiInformation MidiInformation
		{
			get
			{
				return ContextManager.info;
			}
			set
			{
				if (ContextManager.info != null)
				{
					throw new Exception("MidiInformation already set");
				}
				ContextManager.info = value;
			}
		}
		public static DeviceInformation DeviceInformation
		{
			get
			{
				return ContextManager.dev;
			}
			set
			{
				if (ContextManager.dev != null)
				{
					throw new Exception("DeviceInformation already set");
				}
				ContextManager.dev = value;
			}
		}
		public static AssemblyHelper AssemblyHelper
		{
			get
			{
				return ContextManager.asm;
			}
			set
			{
				if (ContextManager.asm != null)
				{
					throw new Exception("AssemblyHelper already set");
				}
				ContextManager.asm = value;
			}
		}
		public static MidiForm MidiForm
		{
			get;
			set;
		}

        public static ILumosLog log
        {
            get;
            set;
        }

        static ContextManager()
        {
            log = LumosLogger.getInstance(typeof(MidiPluginLog));
        }
	}

    public class MidiPluginLog
    {

    }
}
