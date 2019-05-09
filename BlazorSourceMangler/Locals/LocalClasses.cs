using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlazorSourceMangler.Locals
{
    public class BlazorFile
    {

        public FileInfo FI { get; set; }

        public string Name { get; set; }

        public ModuleDefinition module { get; set; }

        public  FileInfo OutputFile { get; set; }

        public bool IsPrimary { get; set; }

        public List<BlazorFile> References { get; set; } = new List<BlazorFile>();

        public List<BlazorFile> Usings { get; set; } = new List<BlazorFile>();

        public double SequenceNumber { get; set; }
    }


    public class CalledItem
    {
        public string OpCode { get; set; }

        public string Module { get; set; }

        public string Name { get; set; }

        public string type { get; set; }

    }

}
