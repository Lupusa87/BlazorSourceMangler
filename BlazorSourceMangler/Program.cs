using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlazorSourceMangler
{

    class Program
    {
        

        static void Main(string[] args)
        {
            DirectoryInfo inputDir = new DirectoryInfo("E:/monocecil/_bin");
            DirectoryInfo outputDir = new DirectoryInfo("E:/monocecil/_bin2");
            bool verbose = true;
            bool verboseDeep = false;
            bool manglePublic = false;

            string BlazorAppDllName = "blazortodos.dll";
            

            if (outputDir.Exists)
            {
                outputDir.GetFiles().ToList().ForEach(f => f.Delete());
                outputDir.GetDirectories().ToList().ForEach(d => d.Delete(true));
            }
            else
            {
                outputDir.Create();
            }

            FileInfo[] files = inputDir.GetFiles("*.dll", SearchOption.AllDirectories);

            foreach (var item in files.OrderBy(x=>x.Length))
            {

                Mangler m = new Mangler(item,
                    new FileInfo(Path.Combine(outputDir.FullName, item.Name)),
                    manglePublic,
                    verbose,
                    verboseDeep, item.Name.Equals(BlazorAppDllName, StringComparison.InvariantCultureIgnoreCase));

                m.ProcessFile();
            }

        }

    }
}
