using Lumos.GUI.AssemblyScan;
using org.dmxc.lumos.Kernel.AssemblyScan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace MidiPlugin
{
	public class AssemblyHelper : IAssemblyListener
	{
		public List<Type> DeviceRuleTypes = new List<Type>();
		public AssemblyHelper()
        {
            AssemblyManager.getInstance().registerAssemblyListener(this);
			ContextManager.AssemblyHelper = this;
            foreach (var t in GetType().Assembly.GetTypes())
                scanNewType(t);
		}
		public void scanNewType(Type t)
		{
            ContextManager.log.Debug("Scanning type {0}", t.FullName);
			if (t.IsClass && !t.IsAbstract && typeof(DeviceRule).IsAssignableFrom(t))
			{
				this.DeviceRuleTypes.Add(t);
			}
		}
		public void typeRemoved(Type t)
		{
            DeviceRuleTypes.RemoveAll(j => j == t);
		}

        public string GetFriendlyName(Type t)
        {
            var attr = t.GetCustomAttributes(true);
            if(attr.Length > 0 && attr.Any(j => j is FriendlyNameAttribute))
            {
                return (attr.First(j => j is FriendlyNameAttribute) as FriendlyNameAttribute).Name;
            }
            return t.Name;
        }
	}


}
