using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.dmxc.lumos.Kernel.Resource;
using org.dmxc.lumos.Kernel.Run;
using org.dmxc.lumos.Kernel.Master;
using LumosLIB.Tools;
using System.Collections.ObjectModel;
using org.dmxc.lumos.Kernel.Messaging;
using org.dmxc.lumos.Kernel.Messaging.Producer;
using org.dmxc.lumos.Kernel.Messaging.Message;

namespace MidiKernelHelper
{
    public sealed class MasterManagerPatch : AbstractManagerAndService, IMasterManager, IIntensityMaster, ILumosProjectManager, ILumosManager, IManager, IMessageProducer
    {
        private static readonly MasterManagerPatch instance = new MasterManagerPatch();
        private double _intensity = 1.0;
        public string Name
        {
            get
            {
                return "Grand Master Patch";
            }
            set
            {
            }
        }
        public event LumosMessageEvent IntensityChanged;
        public double MasterIntensity
        {
            get
            {
                return this._intensity;
            }
            set
            {
                this._intensity = value.Limit(0.0, 1.0);
                var message = new GenericMessage("MasterIntensityChanged",this._intensity);
                if (IntensityChanged != null) IntensityChanged(message);
            }
        }
        ELoadTime ILumosProjectManager.LoadTime
        {
            get
            {
                return ELoadTime.AFTER_CONTAINERS;
            }
        }
        bool ILumosProjectManager.Dirty
        {
            get
            {
                return false;
            }
        }
        ReadOnlyCollection<Type> IManager.ManagerDependencies
        {
            get
            {
                List<Type> t = new List<Type>();
                return t.AsReadOnly();
            }
        }
        public MasterManagerPatch()
        {
        }

        void ILumosProjectManager.closeProject(LumosIOContext context)
        {
        }
        void ILumosProjectManager.loadProject(LumosIOContext context)
        {
        }
        void ILumosProjectManager.saveProject(LumosIOContext context)
        {
        }
        void IManager.initialize()
        {
            this.initializeManager();
        }
        void IManager.shutdown()
        {
            this.shutdownManager();
        }
        bool IManager.IsInitialized
        {
            get
            {
                return base.IsInitialized;
            }
        }

        string IMessageProducer.ProducerID
        {
            get
            {
                return "GrandMaster";
            }
        }
    }

}
