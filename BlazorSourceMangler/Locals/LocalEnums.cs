using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorSourceMangler.Locals
{
    public class LocalEnums
    {
        public enum AppMode
        {
            CleanDeadCodeAndMangle,
            Mangle,
            CleanDeadCode,
            ReportDeadCode,

        }

    }
}
