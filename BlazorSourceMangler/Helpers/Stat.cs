using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BlazorSourceMangler.Helpers
{
    internal static class Stat
    {
        internal static string methodName = string.Empty;
        internal static int counter = 0;

        internal static void DoStat(MethodBase md)
        {
            if (methodName.Equals(md.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                counter++;
            }
            else
            {
                methodName = md.Name;
                counter = 1;
                
            }
        }


        internal static void Reset()
        {
            methodName = string.Empty;
            counter = 0;
            Console.WriteLine();
        }
  }
}
