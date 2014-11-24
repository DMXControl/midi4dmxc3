using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lumos.GUI.Input;
using LumosLIB.Kernel.Log;
using org.dmxc.lumos.Kernel.Input;

namespace MidiPlugin.Utilities
{
    public static class InputLayerManagementHelper
    {
        private static ILumosLog log = MidiPlugin.log;
        private static InputLayerManager ilm = InputLayerManager.getInstance();
        private static Type ilmtype = typeof(org.dmxc.lumos.Kernel.Input.AbstractInputLayerManager);
        private static PropertyInfo indexer;
        private static Dictionary<InputID, IInputListener> dictionary;
        public static void GetFooForId(InputID inp, out IInputListener f)
        {
            try
            {
                f = dictionary[inp];
            }
            catch(Exception ex)
            {
                f = null;
            }
        }
        static InputLayerManagementHelper()
        {
            var field = ilmtype.GetField("registeredInputListeners", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = field.GetValue(ilm);
            dictionary = value as Dictionary<InputID, IInputListener>;
        }
    }
}
