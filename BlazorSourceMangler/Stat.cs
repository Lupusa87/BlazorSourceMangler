using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BlazorSourceMangler
{
    internal static class Stat
    {
        internal static string methodName = string.Empty;
        internal static int counter = 0;

        internal static void DoStat(MethodBase md)
        {
            if (methodName != md.Name)
            {
                methodName = md.Name;
                counter = 1;
            }
            else
            {
                counter++;
            }
        }

    }
}
