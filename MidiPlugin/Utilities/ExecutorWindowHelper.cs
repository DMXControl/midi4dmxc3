using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lumos.GUI;
using Lumos.GUI.Connection;
using Lumos.GUI.Facade.Executor;
using Lumos.GUI.Input;
using Lumos.GUI.Windows.ExecutorPanel;
using org.dmxc.lumos.Kernel.Input;
using Lumos.GUI.Facade;
using Lumos.GUI.Settings;
using org.dmxc.lumos.Kernel.Settings;
using Lumos.GUI.Windows.Master;
using org.dmxc.lumos.Kernel.Command;
using LumosLIB.GUI.Windows.Programmer;
using Lumos.GUI.Windows.Programmer;
using Lumos.GUI.Windows.SceneList;
using org.dmxc.lumos.Kernel.Executor;

namespace MidiPlugin.Utilities
{
    public class ExecutorWindowHelper : IInputListener
    {

        public class DynamicExecutor
        {
            public double Tolerance { get; set; }
            public string GUID { get; set; }

            internal Func<bool> IsModifierKeyPressed;
            internal ExecutorView executorWindow;
            private IExecutorFacade assignedExecutorInternal;
            
            
            public IExecutorFacade assignedExecutor { get { return assignedExecutorInternal; } set { Clear(); assignedExecutorInternal = value; Update(); } }

            private void Update()
            {
                if (assignedExecutorInternal != null) { assignedExecutorInternal.FaderValueChanged += OnFaderValueChanged; OnFaderValueChanged(this, assignedExecutorInternal.FaderValue); }
                else OnFaderValueChanged(this, 0);

            }

            public InputLayerChangedCallback AttachInputChannel(InputChannelMetadata c, InputListenerMetadata meta)
            {
                string metadataID = meta.ID.MetadataID;
                if (c.ChannelType == EInputChannelType.BUTTON)
                {
                    switch (metadataID)
                    {
                        case "Go":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null && object.Equals(newValue, 1.0))
                                {
                                    assignedExecutor.go();
                                    return true;
                                }
                                return false;
                            };

                        case "Stop":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null && object.Equals(newValue, 1.0))
                                {
                                    assignedExecutor.stop();
                                    return true;
                                }
                                return false;
                            };

                        case "GoStop":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null)
                                {
                                    if (object.Equals(newValue, 1.0))
                                    {
                                        assignedExecutor.go();
                                        return true;
                                    }
                                    else if (object.Equals(newValue, 0.0))
                                    {
                                        assignedExecutor.stop();
                                        return true;
                                    }
                                }
                                return false;
                            };

                        case "Back":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null && object.Equals(newValue, 1.0))
                                {
                                    assignedExecutor.goBack();
                                    return true;
                                }
                                return false;
                            };

                        case "Pause":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null && object.Equals(newValue, 1.0))
                                {
                                    assignedExecutor.go();
                                    return true;
                                }
                                return false;
                            };

                        case "PauseBack":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null && object.Equals(newValue, 1.0))
                                {
                                    assignedExecutor.pauseBack();
                                    return true;
                                }
                                return false;
                            };

                        case "Flash":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null)
                                {
                                    if (object.Equals(newValue, 1.0))
                                    {
                                        assignedExecutor.flash(true);
                                        return true;
                                    }
                                    else if (object.Equals(newValue, 0.0))
                                    {
                                        assignedExecutor.flash(false);
                                        return true;
                                    }
                                }
                                return false;
                            };
                        case "Select":
                            return (id, newValue) =>
                            {
                                if (assignedExecutor != null)
                                    if (object.Equals(newValue, 1.0))
                                    {
                                        if(WindowManager.getInstance().StoreActive)
                                        {
                                            if(IsModifierKeyPressed() || assignedExecutor.HasOption(EExecutorOptions.SHOW__PROGRAMMER__FILTER))
                                            {
                                                ProgrammerFilterPredicate predicate;
                                                if(ProgrammerFilter.ShowFilter(out predicate))
                                                    assignedExecutor.storeProgrammerAtExecutor(assignedExecutor.SceneCount, predicate);
                                                return false;
                                            }
                                            else
                                                assignedExecutor.storeProgrammerAtExecutor(assignedExecutor.SceneCount, null);
                                            WindowManager.getInstance().ElementStored();
                                        }
                                        else
                                        {
                                            SceneListWindowManager.getInstance().openSceneList(assignedExecutor.SceneList, IsModifierKeyPressed());
                                            return true;
                                        }
                                    }
                                return false;
                            };
                        default:
                            return null;
                    }
                }
                else if (c.ChannelType == EInputChannelType.RANGE)
                {
                    if (metadataID == "Fader")
                        return (id, newValue) =>
                        {
                            if (assignedExecutor != null)
                            {
                                if (Tolerance <= 0 || Math.Abs((double)assignedExecutor.FaderValue - (double)newValue) < Tolerance) //toleranz ggf. abschaltbar machen pro dynamic
                                    assignedExecutor.FaderValue = (double)newValue;
                                return true;
                            }
                            return false;
                        };
                    else if (metadataID == "Timing")
                        return (id, newValue) =>
                        {
                            if (assignedExecutor != null)
                            {
                                //FIXME
                                return true;
                            }
                            return false;
                        };
                }

                return null;
            }

            internal void Clear()
            {
                if (assignedExecutorInternal != null)
                    assignedExecutorInternal.FaderValueChanged -= OnFaderValueChanged;
                assignedExecutorInternal = null;
            }

            internal void RequestEvent()
            {
                if (assignedExecutorInternal == null) OnFaderValueChanged(this, 0);
                else OnFaderValueChanged(this, assignedExecutorInternal.FaderValue);
            }

            private void OnFaderValueChanged(object sender, double e)
            {
                if (FaderValueChanged != null) FaderValueChanged(sender, e);
            }
            public event FacadeChangedEvent<double> FaderValueChanged;
        }
        public const int ExecutorWindow_MaxExecutors = 8;
        public const string ExecutorWindow_ListenerId = "{CA528EAA-2768-498F-AF76-4F8D5F85E5D9}";
        static string[] executorIds = {
                                          "{85EB4AF4-32BF-4246-8CEF-C5CA66C6C90F}",
                                          "{DCF4CD04-061D-4DC5-96F1-932EAF9C1451}",
                                          "{28531BE2-159E-4529-A027-74782BC403C0}",
                                          "{4EFF8773-30C6-4D70-AD65-5C7825516627}",
                                          "{8C671F35-8974-41C2-9935-6F885D1FB2A9}",
                                          "{4CD39110-8F32-40B0-BA46-0121F8247DEE}",
                                          "{A0A1DFE4-9AFD-4DFB-A2DA-35DCA0B927FC}",
                                          "{037D0678-A050-478F-8667-9586F56BF8C5}",
                                      };

        DynamicExecutor[] dynExecutors;
        public ExecutorWindowHelper()
        {
            dynExecutors = executorIds.Select(j => new DynamicExecutor { GUID = j, Tolerance = 0.07, IsModifierKeyPressed = () => { return modifierKey; } }).ToArray();
        }
        private bool modifierKey;
        public const string ExecutorWindow_PGUp = "{6ABAAF63-E751-4A8C-BF8F-4C0CFE52DFAC}";
        public const string ExecutorWindow_PGDn = "{668FBFEC-266E-4F7D-BD3B-ABD2562F31F8}";

        public const string GrandMasterValue = "{6566CC0A-CD26-4892-B76A-62FDBA6C05DC}";
        public const string ModifierKeyValue = "{97E6F12F-CA8E-43F5-BD93-E3FB2D4DE8BE}";
        public Lumos.GUI.Windows.ExecutorPanel.ExecutorView ExecutorWindow { get; private set; }

        public System.Windows.Forms.ListBox ExecutorWindowListBox { get; private set; }

        public IExecutorPageFacade CurrentExecutorPage { get; private set; }

        public void Establish()
        {
            //Cleanup();
            /* Einklinken in ExecutorWindow */
            ExecutorWindow = WindowManager.getInstance().GuiWindows.First(j => j.GetType() == typeof(ExecutorView)) as ExecutorView;
            foreach (var item in dynExecutors)
            {
                item.executorWindow = ExecutorWindow;
            }
            var ewType = typeof(ExecutorView);

            ExecutorWindowListBox = ewType.GetField("lstPages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ExecutorWindow) as System.Windows.Forms.ListBox;
            ExecutorWindowListBox.SelectedValueChanged += HandleCurrentExecutorPageChanged;
            ExecutorWindowListBox.SelectedIndexChanged += HandleCurrentExecutorPageChanged;

            MidiPlugin.log.Info("ExecutorWindow hook established.");

            var currentItem = ExecutorWindowListBox.SelectedItem as ListBoxItem; /* Fetch current item */
            if (currentItem != null)
            {
                CurrentExecutorPage = ConnectionManager.getInstance().GuiSession.GetExecutorPageByID(currentItem.ID);
                HandleCurrentExecutorPageChanged(null, null); /* Update this. */
            }
            try
            {
                InputLayerManager.getInstance().registerInputListener(this);
            }
            catch (Exception)
            {
                MidiPlugin.log.Warn("Dynamic Executors already registered.");
            }
        }

        public void Cleanup()
        {
            if (ExecutorWindowListBox != null)
            {
                ExecutorWindowListBox.SelectedIndexChanged -= HandleCurrentExecutorPageChanged;
                ExecutorWindowListBox.SelectedValueChanged -=
                    HandleCurrentExecutorPageChanged;
            }
            /* Einklinken in ExecutorWindow */

            MidiPlugin.log.Info("ExecutorWindow hook broken.");
            try
            {
                InputLayerManager.getInstance().deregisterInputListener(this);
            }
            catch (Exception)
            {
                MidiPlugin.log.Info("ExecutorWindowHelper not registered.");
            }
            ExecutorWindowListBox = null;
            ExecutorWindow = null;
        }

        private void HandleCurrentExecutorPageChanged(object sender, EventArgs e)
        {
            var connMgr = ConnectionManager.getInstance().GuiSession;

            var item = ExecutorWindowListBox.SelectedItem as ListBoxItem;
            if (item != null)
            {
                CurrentExecutorPage = connMgr.GetExecutorPageByID(item.ID);
            }
            else
                CurrentExecutorPage = null;
            if (CurrentExecutorPage == null)
                Clear();
            else
            {
                for (int i = 0; i < ExecutorWindow_MaxExecutors; i++)
                {
                    dynExecutors[i].assignedExecutor = i < CurrentExecutorPage.GUIExecutors.Count ? connMgr.GetExecutorByID(CurrentExecutorPage.GUIExecutors[i].ID) : null;
                }
            }
        }

        private void Clear()
        {
            foreach (var item in dynExecutors)
            {
                item.Clear();
            }

            var settings = SettingsManager.getInstance();
            settings.GUISettingChanged -= HandleSettingChanged;
        }



        #region IInputListener Member

        void ExecutorWindowPgUp(bool up)
        {
            if (ExecutorWindowListBox.InvokeRequired)
                ExecutorWindow.Invoke(new Action<bool>(ExecutorWindowPgUp), up);
            else
            {
                if (up) ExecutorWindowListBox.SelectedIndex = Math.Max(ExecutorWindowListBox.SelectedIndex - 1, 0);
                else ExecutorWindowListBox.SelectedIndex = Math.Min(ExecutorWindowListBox.SelectedIndex + 1, ExecutorWindowListBox.Items.Count - 1);
            }
        }
        public InputLayerChangedCallback AttachInputChannel(InputChannelMetadata c, InputListenerMetadata meta)
        {

            if (meta.ID.MetadataID == ExecutorWindow_PGDn) return (j, newValue) => { if (object.Equals(newValue, 1.0)) { ExecutorWindowPgUp(false); return true; } return false; };
            if (meta.ID.MetadataID == ExecutorWindow_PGUp) return (j, newValue) => { if (object.Equals(newValue, 1.0)) { ExecutorWindowPgUp(true); return true; } return false; };
            if (meta.ID.MetadataID == ModifierKeyValue)
                return (j, newValue) =>
                {
                    if (object.Equals(newValue, 1.0))
                    {
                        modifierKey = true;
                        return true;
                    }
                    modifierKey = false;
                    return false;
                };
            if (meta.ID.MetadataID == GrandMasterValue)
                return (j, newValue) =>
                {
                    if (ConnectionManager.getInstance().Connected)
                    {
                        ICommand cmd = ConnectionManager.getInstance().GuiSession.getCommandInstance("Kernel", "setGrandMaster");
                        cmd.execute(new object[]
                        {
                                        newValue
                        });
                        return true;
                    }
                    return false;
                };
            var executor = meta.Parent;
            var dynExc = dynExecutors.FirstOrDefault(j => j.GUID == executor.ID.MetadataID);
            if (dynExc == null) return null;
            return dynExc.AttachInputChannel(c, meta);
        }

        public bool CanAttachInputChannel(InputChannelMetadata c, InputListenerMetadata meta)
        {
            if (!meta.ID.ListenerID.Equals(ListenerID)) return false; // check if metadata belongs to me
            var executor = meta.Parent; /* Meta should be one of Fader, Go,Stop,GoStop,Pause,Back,PauseBack,Select,Flash */
            if (meta.ID.MetadataID == ExecutorWindow_PGDn || meta.ID.MetadataID == ExecutorWindow_PGUp) return c.ChannelType == EInputChannelType.BUTTON;
            if (meta.ID.MetadataID == GrandMasterValue) return c.ChannelType == EInputChannelType.RANGE;
            if (!executorIds.Contains(executor.ID.MetadataID)) return false;
            switch (meta.Name)
            {
                case "Fader": return c.ChannelType == EInputChannelType.RANGE;
                default: return c.ChannelType == EInputChannelType.BUTTON;
            }
        }

        public void DetachInputChannel(InputChannelMetadata c, InputListenerMetadata meta)
        {
            /* Nothing to do here */
        }

        public InputID ListenerID
        {
            get { return new InputID("DynamicExecutor", ConnectionManager.getInstance().SessionName); }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<InputListenerMetadata> Metadata
        {
            get { return CreateDynamicExecutorInfo(); }
        }

        private System.Collections.ObjectModel.ReadOnlyCollection<InputListenerMetadata> CreateDynamicExecutorInfo()
        {
            var lId = ListenerID;
            InputListenerMetadata roote = new InputListenerMetadata(new InputListenerMetadataID(lId, ExecutorWindow_ListenerId), "Dynamic Executors", null, false);

            for (int i = 0; i < ExecutorWindow_MaxExecutors; i++)
            {
                var id = new InputListenerMetadataID(lId, executorIds[i]);
                InputListenerMetadata executorRoot = new InputListenerMetadata(id, "Current Executor " + i.ToString(), roote, false);
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Fader"), "Fader", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Go"), "Go", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Stop"), "Stop", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "GoStop"), "Go / Stop", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Pause"), "Pause", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Back"), "Back", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "PauseBack"), "Pause / Back", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Select"), "Select", executorRoot));
                executorRoot.AddChild(new InputListenerMetadata(new InputListenerMetadataID(id.ListenerID, "Flash"), "Flash", executorRoot));
                roote.AddChild(executorRoot);
            }

            roote.AddChild(new InputListenerMetadata(new InputListenerMetadataID(lId, ExecutorWindow_PGDn), "Page Down", roote, true));
            roote.AddChild(new InputListenerMetadata(new InputListenerMetadataID(lId, ExecutorWindow_PGUp), "Page Up", roote, true));
            roote.AddChild(new InputListenerMetadata(new InputListenerMetadataID(lId, GrandMasterValue), "Grandmaster", roote, true));
            roote.AddChild(new InputListenerMetadata(new InputListenerMetadataID(lId, ModifierKeyValue), "Modifier Key", roote, true));
            return new List<InputListenerMetadata> { roote }.AsReadOnly();
        }

        #endregion

        internal DynamicExecutor GetDynamicExecutorByMetadata(InputListenerMetadata mtd)
        {
            var executor = mtd.Parent;
            var guid = executor.ID.MetadataID;
            return dynExecutors.FirstOrDefault(j => j.GUID == guid);
        }

        internal void LoadExecutor(org.dmxc.lumos.Kernel.Resource.ManagedTreeItem exc)
        {
            foreach (var item in exc.GetChildren("ExecutorCfg"))
            {
                var id = item.getValue<string>("ID");
                var _exc = dynExecutors.FirstOrDefault(j => j.GUID == id);
                if (_exc == null) return;
                _exc.Tolerance = item.getValue<double>("Tolerance");
            }
        }

        internal org.dmxc.lumos.Kernel.Resource.ManagedTreeItem SaveExecutors()
        {
            org.dmxc.lumos.Kernel.Resource.ManagedTreeItem item = new org.dmxc.lumos.Kernel.Resource.ManagedTreeItem("Executors");
            foreach (var item2 in dynExecutors)
            {
                var child = new org.dmxc.lumos.Kernel.Resource.ManagedTreeItem("ExecutorCfg");
                child.setValue("ID", item2.GUID);
                child.setValue("Tolerance", item2.Tolerance);
                item.AddChild(child);
            }
            return item;
        }

        internal void RegisterSettings()

        {
            var settings = SettingsManager.getInstance();
            for (int i = 0; i < dynExecutors.Length; i++)
            {
                settings.registerGuiSetting(new SettingsMetadata(ESettingsRegisterType.APPLICATION, "Executor", null, "Dynamic Executor " + (i + 1) + ": Tolerance", "MPL.EXECFG" + i, "", null)
                {
                    MinDouble = 0,
                    MaxDouble = 1
                }, dynExecutors[i].Tolerance);
            }
            
        }

        //internal void DeregisterSettings()
        //{
        //    var settings = SettingsManager.getInstance();
        //    for (int i = 0; i < dynExecutors.Length; i++)
        //    {
        //        SettingsMetadata mt;
        //
        //        settings.registerGuiSetting(new SettingsMetadata(ESettingsRegisterType.APPLICATION, "Executor", null, "Dynamic Executor " + (i + 1) + ": Tolerance", "MidiPlugin", "MPL.EXECFG" + i, "", null)
        //        {
        //            MinDouble = 0,
        //            MaxDouble = 1,
        //            
        //        }, dynExecutors[i].Tolerance);
        //        
        //    }
        //    settings.GUISettingChanged -= HandleSettingChanged;
        //}
        public void HandleSettingChanged(object sender, SettingChangedEventArgs args)
        {
            MidiPlugin.log.Info("SettingChanged: {0}, NewValue: {1}, Type:{2}", args.PropertyName, args.NewValue, args.Type);
            for (int i = 0; i < dynExecutors.Length; i++)
            {
                dynExecutors[i].Tolerance = SettingsManager.getInstance().getGuiSetting<double>("MPL.EXECFG" + i);
            }
        }

    }
}
