using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static BlazorSourceMangler.Locals.LocalEnums;

namespace BlazorSourceMangler.Locals
{
    public static class LocalData
    {

        public static AppMode appMode = AppMode.CleanDeadCodeAndMangle;


        public static List<BlazorFile> BlazorFilesList = new List<BlazorFile>();

        public static DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();

        public static DirectoryInfo inputDir;
        public static DirectoryInfo outputDir;
        public static bool verbose;
        public static bool verboseDeep;
        public static bool manglePublic;



        private static int _CleanerRepeatCount { get; set; }



        public static int CleanerRepeatCount {

            set
            {
                if (value > 3)
                {
                    _CleanerRepeatCount = value;
                }
                else
                {
                    if (_CleanerRepeatCount!=3)
                    {
                        _CleanerRepeatCount = 3;
                    }
                }
            }
            get
            {
                return _CleanerRepeatCount;
            }
        }

        public static List<string> ShouldIgnoreClases = new List<string>()
        {
            "app", "program", "startup", "_imports", "mainlayout"
        };
    }
}
