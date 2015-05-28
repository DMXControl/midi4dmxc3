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


        public void Clear()
        {
            foreach (var item in helperList)
            {
                item.Value.Unregister();
            }
            helperList.Clear();
        }

        public void Update(ExecutorWindowHelper ewHelper, InputLinkChangedMessage ilcMsg)
        {
            if (ilcMsg.Type == AbstractChangedMessage.EChangeType.CHANGED) return; //nothing to do?

            //retrieve InputLayer object
            var layer = midiInf.RuleSets.Select(j => j.InputLayer).FirstOrDefault(j => j.Metadata.ID.Equals(ilcMsg.ID.ParentLayer));
            if (layer == null)
            {
                MidiPlugin.log.Debug("Input Layer {0} does not belong to me, ignoring", ilcMsg.ID.ParentLayer.ID);
                return;
            }

            //retrieve InputChannel object
            var channel = layer.Channels.FirstOrDefault(j => j.Metadata.ID.Equals(ilcMsg.ID));
            if(channel == null)
            {
                MidiPlugin.log.Warn("Could not retrieve channel {0}", ilcMsg.ID.ChannelID);
                return;
            }
            var listener = ilm.getInputListenerByID(ilcMsg.Listener.MetadataID);
            var channelId = channel.ID.ChannelID;
            switch(ilcMsg.Listener.ListenerID.ID)
            {
                case "ExecutorManager":
                    var executor = Lumos.GUI.Connection.ConnectionManager.getInstance().GuiSession.Executors.First(j => j.ID == listener.Parent.ID.MetadataID);
                    if(executor == null)
                    {
                        MidiPlugin.log.Warn("Executor {0} does not exist, but is linked.", listener.Parent.ID.MetadataID);
                        return;
                    }
                    if (listener.Name == "Fader")
                    {
                        //Link to executor fader
                        if(ilcMsg.Type == AbstractChangedMessage.EChangeType.ADDED)
                        {
                            try
                            {
                                var mbh = new MidiBacktrackHelper(channel as MidiInputChannel, executor);
                                helperList.Add(channelId, mbh);
                                mbh.Register();
                            }
                            catch(Exception e)
                            {
                                MidiPlugin.log.Warn("There is already a helper for channel {0} registered.", e, channelId);
                            }
                        }
                        else if(ilcMsg.Type == AbstractChangedMessage.EChangeType.REMOVED)
                        {
                            MidiBacktrackHelper mbh;
                            if(!helperList.TryGetValue(channelId, out mbh))
                            {
                                MidiPlugin.log.Warn("Could not fetch helper for ChannelID {0}", channelId);
                                return;
                            }
                            mbh.Unregister();
                            helperList.Remove(channelId);
                        }
                    }

                    break;
                case "DynamicExecutor":
                    var dynamicExecutor = ewHelper.GetDynamicExecutorByMetadata(listener);
                    if (dynamicExecutor == null)
                    {
                        MidiPlugin.log.Warn("DynamicExecutor {0} does not exist, but is linked.", listener.Parent.ID.MetadataID);
                        return;
                    }
                    if (listener.Name == "Fader")
                    {
                        //Link to executor fader
                        if (ilcMsg.Type == AbstractChangedMessage.EChangeType.ADDED)
                        {
                            try
                            {
                                var mbh = new MidiBacktrackHelper(channel as MidiInputChannel, dynamicExecutor);
                                helperList.Add(channelId, mbh);
                                mbh.Register();
                            }
                            catch (Exception e)
                            {
                                MidiPlugin.log.Warn("There is already a helper for channel {0} registered.", e, channelId);
                            }
                        }
                        else if (ilcMsg.Type == AbstractChangedMessage.EChangeType.REMOVED)
                        {
                            MidiBacktrackHelper mbh;
                            if (!helperList.TryGetValue(channelId, out mbh))
                            {
                                MidiPlugin.log.Warn("Could not fetch helper for ChannelID {0}", channelId);
                                return;
                            }
                            mbh.Unregister();
                            helperList.Remove(channelId);
                        }
                    }
                    break;
            }
        }

        private class MidiBacktrackHelper
        {
            private MidiInputChannel channel;
            private Lumos.GUI.Facade.Executor.IExecutorFacade executor;
            private ExecutorWindowHelper.DynamicExecutor dynExec;
            private bool isDynamic = false;
            public MidiBacktrackHelper(MidiInputChannel chan, Lumos.GUI.Facade.Executor.IExecutorFacade executor)
            {
                this.channel = chan;
                this.executor = executor;
            }

            public MidiBacktrackHelper(MidiInputChannel chan, ExecutorWindowHelper.DynamicExecutor dynExc)
            {
                dynExec = dynExc;
                isDynamic = true;
                channel = chan;
            }
            public void Register()
            {
                try {
                    if (!isDynamic)
                        executor.FaderValueChanged += HandlerFunc;
                    else
                        dynExec.FaderValueChanged += HandlerFunc;

                    registered = true;
                }
                catch(Exception ex)
                {
                    MidiPlugin.log.Error("Error registering feedback.", ex);
                }
            }

            public void Unregister()
            {
                try {
                    if (!isDynamic)
                        executor.FaderValueChanged -= HandlerFunc;
                    else
                        dynExec.FaderValueChanged -= HandlerFunc;

                    registered = false;
                }
                catch(Exception ex)
                {
                    MidiPlugin.log.Error("Error unregistering feedback. ", ex);
                }
            }

            private void HandlerFunc(object sender, double val)
            {
                try
                {
                    channel.UpdateBacktrackValue(val);
                }
                catch(Exception ex)
                {
                    MidiPlugin.log.Error("Error updating backtrack values", ex);
                }
            }

            public bool registered;
        }

    }
}
