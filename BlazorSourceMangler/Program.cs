using BlazorSourceMangler.Helpers;
using BlazorSourceMangler.Locals;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static BlazorSourceMangler.Locals.LocalEnums;

namespace BlazorSourceMangler
{

    class Program
    {
        

        static void Main(string[] args)
        {
            TimeAnalyzer.StartTimeCounter();
           
            LocalData.appMode = AppMode.CleanDeadCodeAndMangle;

            LocalData.inputDir = new DirectoryInfo("E:/monocecil/_bin");
            LocalData.outputDir = new DirectoryInfo("E:/monocecil/_bin2");

            LocalData.resolver.AddSearchDirectory(LocalData.outputDir.FullName);

            LocalData.CleanerRepeatCount = 3;

            LocalData.verbose = true;
            LocalData.verboseDeep = false;
            LocalData.manglePublic = false;

            LocalFunctions.EnsureOutputDirectory();

            FileInfo[] files = LocalData.inputDir.GetFiles("*.dll", SearchOption.AllDirectories);


            if (files.Count() > 0)
            {


                LocalData.BlazorFilesList = new List<BlazorFile>();
                foreach (var item in files)
                {
                    BlazorFile bf = new BlazorFile()
                    {
                        FI = item,
                        Name = item.Name.Replace(".dll", null),
                        module = ModuleDefinition.ReadModule(item.FullName, new ReaderParameters { AssemblyResolver =LocalData.resolver }),
                        OutputFile = new FileInfo(Path.Combine(LocalData.outputDir.FullName, item.Name)),
                        IsPrimary = false,
                        References = new List<BlazorFile>(),
                    };
                    LocalData.BlazorFilesList.Add(bf);
                }



                if (LocalData.BlazorFilesList.Count > 1)
                {
                    LocalFunctions.AnalyzeReferences();
                }
                else
                {
                    LocalData.BlazorFilesList.First().IsPrimary = true;
                }

               // LocalData.BlazorFilesList.Single(x => x.IsPrimary).Process = false;


                switch (LocalData.appMode)
                {
                    case AppMode.CleanDeadCodeAndMangle:
                        LocalFunctions.RunDCCleaner();
                        LocalFunctions.RunMangler();
                        LocalFunctions.SaveResult();
                        break;
                    case AppMode.Mangle:
                        LocalFunctions.RunMangler();
                        LocalFunctions.SaveResult();
                        break;
                    case AppMode.CleanDeadCode:
                        LocalFunctions.RunDCCleaner();
                        LocalFunctions.SaveResult();
                        break;
                    case AppMode.ReportDeadCode:
                        LocalFunctions.RunDCReporter();
                        break;
                    default:
                        break;
                }

                //    LocalFunctions.AnalyzeHardCodedPropertyNames();

                // LocalFunctions.AnalyzeOutsideCalls();



               
            }
            else
            {
                Console.WriteLine("Files not found in " + LocalData.inputDir.FullName);
            }




            TimeAnalyzer.FinishTimeCounter();
        }

    }
}
