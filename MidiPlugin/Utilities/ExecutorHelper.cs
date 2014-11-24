using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlugin.Utilities
{
    public class ExecutorHelper
    {
        private static ExecutorHelper _i;
        public static ExecutorHelper Instance { get { if (_i == null) return _i = new ExecutorHelper(); else return _i; } }

        public ExecutorHelper()
        {

        }

        public void Startup()
        {
            //ExecutorManager
        }

        public void Shutdown()
        {

        }
        void RegisterExecutor()
        {

        }

        void DeregisterExecutor()
        {
            
        }
    }
}
