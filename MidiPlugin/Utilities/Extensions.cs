using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlugin.Utilities
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}
