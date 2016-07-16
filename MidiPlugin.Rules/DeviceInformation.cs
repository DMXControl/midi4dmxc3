using LumosLIB.Kernel.Log;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
namespace MidiPlugin
{
	public class DeviceInformation : IDisposable
	{
		private static ILumosLog log = LumosLogger.getInstance(typeof(DeviceInformation));
		private bool disposed;
		public BindingList<MidiInput> InputDevices
		{
			get;
			private set;
		}
		public BindingList<MidiOutput> OutputDevices
		{
			get;
			private set;
		}
		public List<MidiDev> Devices
		{
			get
			{
				return this.InputDevices.Cast<MidiDev>().Concat(this.OutputDevices.Cast<MidiDev>()).ToList<MidiDev>();
			}
		}
		public void Start()
		{
			foreach (MidiInput d in this.InputDevices)
			{
				d.Start();
			}
		}
		public void Stop()
		{
			foreach (MidiInput d in this.InputDevices)
			{
				d.Stop();
			}
		}
		public DeviceInformation()
		{
			ContextManager.DeviceInformation = this;
			this.InputDevices = new BindingList<MidiInput>();
			this.OutputDevices = new BindingList<MidiOutput>();
			for (int i = 0; i < MidiIn.NumberOfDevices; i++)
			{
				try
				{
					this.InputDevices.Add(new MidiInput(i));
				}
				catch (Exception e)
				{
					DeviceInformation.log.Warn("Error initializing Midi-In Device", e, new object[0]);
				}
			}
			for (int i = 0; i < MidiOut.NumberOfDevices; i++)
			{
				try
				{
					this.OutputDevices.Add(new MidiOutput(i));
				}
				catch (Exception e)
				{
					DeviceInformation.log.Warn("Error initializing Midi-Out Device", e, new object[0]);
				}
			}
		}
		public void Dispose()
		{
			Monitor.Enter(this);
			try
			{
				if (!this.disposed)
				{
					this.disposed = true;
					foreach (MidiInput item in this.InputDevices)
					{
						item.Dispose();
					}
					foreach (MidiOutput item2 in this.OutputDevices)
					{
						item2.OutputDevice.Dispose();
					}
					this.InputDevices = null;
					this.OutputDevices = null;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
	}
}
