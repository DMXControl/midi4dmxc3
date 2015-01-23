using Lumos.GUI;
using Lumos.GUI.Plugin;
using Lumos.GUI.Resource;
using Lumos.GUI.Run;
using LumosLIB.Kernel.Log;
using org.dmxc.lumos.Kernel.Resource;
using System;
using System.Collections.Generic;
using org.dmxc.lumos.Kernel.Input;
using org.dmxc.lumos.Kernel.Messaging.Listener;
using Lumos.GUI.Connection;
using org.dmxc.lumos.Kernel.Messaging.Message;
using org.dmxc.lumos.Kernel.Executor;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
namespace MidiPlugin
{
	public class MidiPlugin : GuiPluginBase, IMessageListener
	{
        public org.dmxc.lumos.Kernel.Input.IInputLayerManager interfacedILM;
        public Type interfacedILMType;
		internal static readonly ILumosLog log = LumosLogger.getInstance(typeof(MidiPlugin));
		private static readonly LumosResourceMetadata myMetaData = new LumosResourceMetadata("MidiSettings.xml", ELumosResourceType.MANAGED_TREE);
        private static readonly LumosResourceMetadata metadata2 = new LumosResourceMetadata("MidiPlugin.Config.xml", ELumosResourceType.MANAGED_TREE);
		private MidiForm form;
		private DeviceInformation devices;
		private MidiInformation midi;
		private AssemblyHelper asmh;
        private Utilities.ExecutorWindowHelper ewHelper = new Utilities.ExecutorWindowHelper();
        private Utilities.LinkChangedHandler lch;
		public MidiPlugin() : base("{2E9C6D8E-431E-4d24-965C-AC4080C25CBA}", "Midi Plugin")
		{
		}
		protected override void initializePlugin()
		{
			this.asmh = new AssemblyHelper();
			this.devices = new DeviceInformation();
			this.midi = new MidiInformation();
            this.lch = new Utilities.LinkChangedHandler(this.midi);
            ConnectionManager.getInstance().registerMessageListener(this, "KernelInputLayerManager", "LinkChanged");
            ConnectionManager.getInstance().registerMessageListener(this, "ExecutorManager", "OnExecutorChanged");
            try
            {
                var res = ResourceManager.getInstance().loadResource(EResourceAccess.READ_WRITE, EResourceType.CONFIG, metadata2);
                var data = res.ManagedData;
                ewHelper.LoadExecutor(data);
            }
            catch
            {
                log.Info("No configuration found.");
            }
        }
		protected override void startupPlugin()
		{
            log.Debug("Startup MidiPlugin!");
			this.form = new MidiForm();
            this.form.Import += HandleImport;
            this.form.Export += HandleExport;
			this.devices.Start();
			WindowManager.getInstance().AddWindow(this.form);
            
		}

        private void HandleCurrentExecutorPageChanged(object sender, EventArgs e)
        {
            log.Warn("ExecutorPage changed.");
        }

        private void HandleExport(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.DefaultExt = ".xml";
            sfd.AddExtension = true;
            sfd.Filter = "Midi-Mapping|*.xml";
            var dr = sfd.ShowDialog();
            if (dr != DialogResult.OK) return;
            var xelement = Serialize();
            xelement.Save(sfd.FileName);
        }

        private void HandleImport(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Midi-Mapping|*.xml";
            ofd.Multiselect = false;
            var dr = ofd.ShowDialog();
            if (dr != DialogResult.OK) return;
            LoadFromXml(XElement.Load(ofd.FileName));
        }
		protected override void shutdownPlugin()
		{
            log.Debug("Shutdown MidiPlugin!");
			this.devices.Stop();
			WindowManager.getInstance().RemoveWindow(this.form);
			if (this.form.InvokeRequired)
			{
				this.form.Invoke(new Action(this.form.Close));
			}
			else
			{
				this.form.Close();
			}
		}
		public override void saveProject(LumosGUIIOContext context)
		{
            log.Debug("SaveProject in MidiPlugin");
			base.saveProject(context);
            ResourceManager.getInstance().saveResource(EResourceType.CONFIG, new LumosResource("MPlugin", ewHelper.SaveExecutors()));
			this.Save();
		}
		public override void loadProject(LumosGUIIOContext context)
		{
            log.Debug("LoadProject in MidiPlugin");
			base.loadProject(context);
			this.Load();
            lch.Update(ewHelper);
		}
		public override void closeProject(LumosGUIIOContext context)
		{
			base.closeProject(context);
			this.Close();
		}
		public override void connectionClosing()
		{
			this.Save();
			this.Close();

            ewHelper.Cleanup();
            lch.Update(ewHelper);
            base.connectionClosing();
		}
		public override void connectionEstablished()
		{
			this.Load();


            ewHelper.Establish();
            lch.Update(ewHelper);
			base.connectionEstablished();
		}
		private void Close()
		{
			List<RuleSet> rules = new List<RuleSet>(this.midi.RuleSets);
			foreach (RuleSet item in rules)
			{
				this.midi.RuleSets.Remove(item);
			}
           
		}
		private void Load()
		
        {
			this.Close();
			if (ResourceManager.getInstance().existsResource(EResourceType.PROJECT, MidiPlugin.myMetaData))
			{
				LumosResource r = ResourceManager.getInstance().loadResource(EResourceAccess.READ_WRITE, EResourceType.PROJECT, MidiPlugin.myMetaData);
				ManagedTreeItem item = r.ManagedData;
				foreach (ManagedTreeItem mti in item.GetChildren("RuleSet"))
				{
					RuleSet rs = RuleSet.Load(mti);
					if (rs != null)
					{
						this.midi.RuleSets.Add(rs);
					}
				}
			}
		}
		private void Save()
		{
            log.Debug("Save called!");
			ManagedTreeItem _midi = new ManagedTreeItem("MidiSettings");
			foreach (RuleSet item in this.midi.RuleSets)
			{
				ManagedTreeItem rs = new ManagedTreeItem("RuleSet");
				item.Save(rs);
				_midi.AddChild(rs);
			}
            log.Debug("Creating resource");
			LumosResource res = new LumosResource(MidiPlugin.myMetaData.Name, _midi);
            log.Debug("Resource created: {0}", res.ManagedData.Children.Count);
			ResourceManager.getInstance().saveResource(EResourceType.PROJECT, res);

            
            log.Debug("Resource saved");
		}

        public XElement Serialize()
        {
            var xElement = new XElement("MidiSettings");
            foreach (RuleSet item in this.midi.RuleSets)
            {
                xElement.Add(item.Serialize());
            }

            return xElement;
        }

        public void LoadFromXml(XElement element)
        {
            foreach (var item in element.Elements("RuleSet"))
            {
                var rs = RuleSet.LoadFromXml(item);
                this.midi.RuleSets.Add(rs);
            }
        }
		protected override void DisposePlugin(bool disposing)
		{
			if (this.midi != null)
			{
				this.midi.Dispose();
			}
			if (this.devices != null)
			{
				this.devices.Dispose();
			}
			base.DisposePlugin(disposing);
		}

        #region IMessageListener Member

        public void onMessage(org.dmxc.lumos.Kernel.Messaging.Message.IMessage message)
        {
            //log.Info("OnMessage {0}", message.GetType().Name);
            var msg = message as InputLinkChangedMessage;
            if(msg != null)
                lch.Update(ewHelper);
        }

        #endregion
    }
}
