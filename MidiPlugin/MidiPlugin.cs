using Lumos.GUI;
using Lumos.GUI.AssemblyScan;
using Lumos.GUI.Connection;
using Lumos.GUI.Plugin;
using Lumos.GUI.Resource;
using Lumos.GUI.Run;
using Lumos.GUI.Settings;
using Lumos.GUI.Settings.PE;
using Lumos.GUI.Windows;
using LumosLIB.Kernel.Log;
using org.dmxc.lumos.Kernel.AssemblyScan;
using org.dmxc.lumos.Kernel.Messaging.Listener;
using org.dmxc.lumos.Kernel.Messaging.Message;
using org.dmxc.lumos.Kernel.Resource;
using org.dmxc.lumos.Kernel.Settings;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
namespace MidiPlugin
{
    public class MidiPlugin : GuiPluginBase, IMessageListener
    {
        internal static readonly ILumosLog log = LumosLogger.getInstance(typeof(MidiPluginLog));
        private static readonly LumosResourceMetadata myMetaData = new LumosResourceMetadata("MidiSettings.xml", ELumosResourceType.MANAGED_TREE);
        private static readonly LumosResourceMetadata metadata2 = new LumosResourceMetadata("MidiPlugin.Config.xml", ELumosResourceType.MANAGED_TREE);
        private MidiForm form;
        private DeviceInformation devices;
        private MidiInformation midi;
        private AssemblyHelper asmh;
        private Utilities.ExecutorWindowHelper ewHelper;
//      Commented because of a bug in the Input Assignment of DMXControl 3.1(ListenerID is not unique)
//      private Utilities.LinkChangedHandler lch;
        private bool projectLoaded = false;
        public MidiPlugin() : base("{2E9C6D8E-431E-4d24-965C-AC4080C25CBA}", "Midi Plugin")
        {
        }
        protected override void initializePlugin()
        {
            try
            {
                log.Debug("Initialize MidiPlugin!");
                this.ewHelper = new Utilities.ExecutorWindowHelper();
                this.asmh = new AssemblyHelper();
                this.devices = new DeviceInformation();
                this.midi = new MidiInformation();
                //              Commented because of a bug in the Input Assignment of DMXControl 3.1(ListenerID is not unique)
                //              this.lch = new Utilities.LinkChangedHandler(this.midi);

                //              Commented because DynExecutors will be created after connecting to the kernel
                //              ewHelper.RegisterSettings();

                SettingsManager.getInstance().registerSetting(
                new SettingsMetadata(ESettingsRegisterType.BOTH, "GUI", new string[]{"Executors"} ,"DynExecutor MinCount",
                    "MIDI Plugin", "DYN_EXECUTOR.MIN_DYNEXECUTORS_COUNT",
                    "Here you can define the minimal count of the DynamicExecutors", null), 8);
    }
            catch (Exception ex)
            {
                log.Error("Error initializing plugin...", ex);
            }
        }
        protected override void startupPlugin()
        {
            try
            {
                if (ConnectionManager.getInstance().Connected)
                    Load();
                log.Debug("Startup MidiPlugin!");
                ConnectionManager.getInstance().registerMessageListener(this, "KernelInputLayerManager", "LinkChanged");
                ConnectionManager.getInstance().registerMessageListener(this, "ExecutorManager", "OnExecutorChanged");
                this.form = new MidiForm();
                this.form.Import += HandleImport;
                this.form.Export += HandleExport;
                this.devices.Start();
                WindowManager.getInstance().AddWindow(this.form);
                try
                {
                    var res = ResourceManager.getInstance().loadResource(EResourceAccess.READ_WRITE, EResourceType.APPLICATION, metadata2);
                    var data = res.ManagedData;
                    ewHelper.LoadExecutor(data);
                }
                catch
                {
                    log.Info("No configuration found.");
                }
                SettingsManager.getInstance().SettingChanged += ewHelper.HandleSettingChanged;
            }
            catch (Exception ex)
            {
                log.Error("Error startup plugin...", ex);
            }
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
            try
            {
                if (projectLoaded) Close();
                log.Debug("Shutdown MidiPlugin!");
                this.devices.Stop();
                ConnectionManager.getInstance().deregisterMessageListener(this, "KernelInputLayerManager", "LinkChanged");
                ConnectionManager.getInstance().deregisterMessageListener(this, "ExecutorManager", "OnExecutorChanged");
                WindowManager.getInstance().RemoveWindow(this.form);
                SettingsManager.getInstance().SettingChanged -= ewHelper.HandleSettingChanged;
                if (this.form.InvokeRequired)
                {
                    this.form.Invoke(new Action(this.form.Close));
                }
                else
                {
                    this.form.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error shutting down plugin...", ex);
            }
        }
        public override void saveProject(LumosGUIIOContext context)
        {
            if (!ConnectionManager.getInstance().Connected) return;
            log.Debug("SaveProject in MidiPlugin");
            base.saveProject(context);
            ResourceManager.getInstance().saveResource(EResourceType.APPLICATION, new LumosResource("MPlugin", ewHelper.SaveExecutors()));
            this.Save();
        }
        public override void loadProject(LumosGUIIOContext context)
        {
            if (!ConnectionManager.getInstance().Connected) return;
            log.Debug("LoadProject in MidiPlugin");
            base.loadProject(context);
            this.Load();
            //lch.Update(ewHelper);
        }
        public override void closeProject(LumosGUIIOContext context)
        {
            if (!ConnectionManager.getInstance().Connected) return;
            log.Debug("CloseProject in MidiPlugin");
            base.closeProject(context);
            this.Close();
        }
        public override void connectionClosing()
        {
            log.Debug("Connection closing in MidiPlugin...");

            ewHelper.Cleanup();
            base.connectionClosing();
        }
        public override void connectionEstablished()
        {
            log.Debug("Connection established in MidiPlugin...");

            if (ConnectionManager.getInstance().Connected)
                this.Load();

            ewHelper.Establish();
            ewHelper.RegisterSettings();
            base.connectionEstablished();
        }
        private void Close()
        {
 //         Commented because of a bug in the Input Assignment of DMXControl 3.1(ListenerID is not unique)
 //         lch.Clear();
            List<RuleSet> rules = new List<RuleSet>(this.midi.RuleSets);
            foreach (RuleSet item in rules)
            {
                this.midi.RuleSets.Remove(item);
            }
            projectLoaded = false;
        }
        private void Load()

        {
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
                projectLoaded = true;
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
            // Commented because of a bug in the Input Assignment of DMXControl 3.1 (ListenerID is not unique)
            /*
                //log.Info("OnMessage {0}", message.GetType().Name);
                var msg = message as InputLinkChangedMessage;
                if (msg != null)
                    lch.Update(ewHelper, msg);
            */
        }

        #endregion
    }
}
