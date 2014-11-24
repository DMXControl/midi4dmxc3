using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lumos.GUI.Input;
using org.dmxc.lumos.Kernel.Input;
using org.dmxc.lumos.Kernel.Messaging.Message;

namespace MidiPlugin.Utilities
{
    public class LinkChangedHandler
    {
        private MidiInformation midiInf;
        public LinkChangedHandler(MidiInformation midiInf)
        {
            this.midiInf = midiInf;
        }
        private Dictionary<string, MidiBacktrackHelper> helperList = new Dictionary<string, MidiBacktrackHelper>();
        static InputLayerManager ilm = Lumos.GUI.Input.InputLayerManager.getInstance();



        public void Update(ExecutorWindowHelper ewHelper)
        {
            int removed = 0, created = 0;
            helperList.ForEach(j => j.Value.current = false); /* Setze alle current auf false für differentielles Update */
            var links = InputLayerManager.getInstance().LinkedChannels;
            foreach (var item in links)
            {
                var listener = item.Key;
                foreach (var item2 in item.Value)
                {
                    /* Channel updates */
                    var layer = ilm.getInputLayerByID(item2.ID.ParentLayer);
                    var layerObj = midiInf.RuleSets.Select(j => j.InputLayer).FirstOrDefault(j => j.Metadata.Equals(layer));
                    if (layerObj == null) continue;
                    var channel = layerObj.Channels.FirstOrDefault(j => j.Metadata.Equals(item2));
                    if (channel == null) continue;
                    
                    if(listener.ID.ListenerID.ID == "ExecutorManager")
                    {
                        // ExecutorManager detected
                        
                        if (listener.Name == "Fader")
                        {
                            MidiPlugin.log.Debug("Link to Fader detected.");
                            var executor = Lumos.GUI.Connection.ConnectionManager.getInstance().GuiSession.Executors.First(j => j.ID == listener.Parent.ID.MetadataID);
                            MidiBacktrackHelper helper;
                            if(!helperList.TryGetValue(channel.ID.ChannelID, out helper))
                            {
                                helper = new MidiBacktrackHelper(channel as MidiRangeInputChannel, executor);
                                helperList.Add(channel.ID.ChannelID, helper);
                                created++;
                                MidiPlugin.log.Info("Link to executor {0} established.", listener.Parent.Name);
                            }
                            helper.current = true;
                            if(!helper.registered)
                            helper.Register();
                        }
                    }

                    if (listener.ID.ListenerID.ID == "DynamicExecutor")
                    {
                        MidiPlugin.log.Info("Dynamic Executors detected...");
                        var mtd = listener;
                        if (mtd.Name == "Fader")
                        {
                            MidiPlugin.log.Debug("Link to dynamic Fader detected.");
                            var dynExc = ewHelper.GetDynamicExecutorByMetadata(mtd);
                            if (dynExc != null)
                            {
                                MidiBacktrackHelper helper;
                                if (!helperList.TryGetValue(channel.ID.ChannelID, out helper))
                                {
                                    helper = new MidiBacktrackHelper(channel as MidiRangeInputChannel, dynExc);
                                    helperList.Add(channel.ID.ChannelID, helper);
                                    created++;
                                    MidiPlugin.log.Info("Link to dynamic executor {0} established.", mtd.Parent.Name);
                                }
                                helper.current = true;
                                if (!helper.registered)
                                    helper.Register();
                            }
                        }
                    }
                }
            }

           
            var x = helperList.Where(j => !j.Value.current);
            x.ForEach(j => { if (j.Value.registered)j.Value.Unregister(); });
            var y = x.ToArray();
            foreach (var item in y)
            {
                helperList.Remove(item.Key);
                removed++;
            }
            MidiPlugin.log.Info("LinkChangedHandler finished, {0} added, {1} removed", created, removed);
        }

        private class MidiBacktrackHelper
        {
            private MidiRangeInputChannel channel;
            private Lumos.GUI.Facade.Executor.IExecutorFacade executor;
            private ExecutorWindowHelper.DynamicExecutor dynExec;
            private bool isDynamic = false;
            public MidiBacktrackHelper(MidiRangeInputChannel chan, Lumos.GUI.Facade.Executor.IExecutorFacade executor)
            {
                this.channel = chan;
                this.executor = executor;
            }

            public MidiBacktrackHelper(MidiRangeInputChannel chan, ExecutorWindowHelper.DynamicExecutor dynExc)
            {
                dynExec = dynExc;
                isDynamic = true;
                channel = chan;
            }
            public void Register()
            {
                if (!isDynamic)
                    executor.FaderValueChanged += HandlerFunc;
                else
                    dynExec.FaderValueChanged += HandlerFunc;

                registered = true;
            }

            public void Unregister()
            {
                if (!isDynamic)
                    executor.FaderValueChanged -= HandlerFunc;
                else
                    dynExec.FaderValueChanged -= HandlerFunc;

                registered = false;
            }

            private void HandlerFunc(object sender, double val)
            {
                channel.UpdateBacktrackValue(val);
            }

            public bool registered;
            public bool current = false;
        }
    }
}
