using BlazorSourceMangler.Helpers;
using BlazorSourceMangler.Locals;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BlazorSourceMangler.Processors
{
    internal class Mangler
    {
       
        BlazorFile blazorFile;

        internal Mangler(BlazorFile _blazorFile)
        {
            blazorFile = _blazorFile;
            ProcessFile();
        }


        private void ProcessFile()
        {




            if (LocalData.verbose)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("======mangling file========");
                Console.WriteLine(blazorFile.FI.FullName);
            }


            //Under Construction
            //RemoveDeadMethods(md);

            if (blazorFile.IsPrimary)
            {
                RenameNamespaceNames();
            }

            RenameEnumNames();
            RenameParameters();
            RenameFieldNames();


            //!!! can't be used for now because builder has hard coded prop names and mangling is giving error!!!
            // builder should instead use properties for example not "a" but someclass.a
            RenamePropertyNames();

            RenameMethodNames();
            RenameTypeNames();

           
        }

        private void RenameParameters()
        {
            Stat.Reset();
            int counter = 0;

            var toInspect = blazorFile.module.GetTypes().Where(x=>!x.Name.Contains("<"))
                  .SelectMany(t => t.Methods.Select(m => new { t, m }))
                  .Where(x => x.m.HasParameters && !x.m.Name.Contains("<") && !x.m.Name.StartsWith("set_"));


            foreach (var item in toInspect)
            {
                //Console.WriteLine(item.m.Name);
                foreach (var i in item.m.Parameters)
                {
                    counter++;

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("=====parameter " + i.Name + ", method " + item.m.Name + ", type " + item.t.Name);
                    }

                    i.Name = CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }
                }

                counter = 0;
            }

            if (LocalData.verbose)
            {
                Console.WriteLine("renamed parameters count = " + Stat.counter);
            }
        }

        private void RenameMethodNames()
        {
            Stat.Reset();

            int counter = 0;

            IEnumerable<MethodDefinition> methods;

            foreach (var t in blazorFile.module.GetTypes().Where(x => !x.Name.Contains("<") && !LocalData.ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower()))))
            {


                if (LocalData.manglePublic)
                {
                    methods = t.Methods.Where(x => !x.IsConstructor && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("get_") && !x.Name.Contains("set_"));
                }
                else
                {
                    methods = t.Methods.Where(x => x.IsPublic == false && !x.IsConstructor && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("get_") && !x.Name.Contains("set_"));
                }

                foreach (var i in methods)
                {

                    // ovverided methods should be skipped
                    if (i.IsVirtual && !i.IsNewSlot)
                    {
                        continue;
                    }


                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("=====method " + i.Name + ", type " + t.Name);
                    }

                    counter++;
                    if (i.Name.Contains("."))
                    {
                        i.Name = i.Name.Substring(0, i.Name.LastIndexOf(".") + 1) + "M" + CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());
                    }
                    else
                    {
                        i.Name = "M" + CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());
                    }

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }

                    
                }

                counter = 0;
            }

            if (LocalData.verbose)
            {
                Console.WriteLine("renamed methods count = " + Stat.counter);
            }
        }

        private void RenameFieldNames()
        {
            Stat.Reset();

            int counter = 0;

            IEnumerable<FieldDefinition> fields;

            foreach (var t in blazorFile.module.GetTypes())
            {

                if (LocalData.manglePublic)
                {

                    fields = t.Fields.Where(x => !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
                }
                else
                {
                    fields = t.Fields.Where(x => !x.IsPublic && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
                }

                foreach (var i in fields)
                {
                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("=====field " + i.Name + ", type " + t.Name);
                    }

                    counter++;

                    i.Name = "F" + CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }
                    

                }

                counter = 0;
            }

            if (LocalData.verbose)
            {
                Console.WriteLine("renamed fields count = " + Stat.counter);
            }
        }

        private void RenamePropertyNames()
        {
            Stat.Reset();
            int counter = 0;


            foreach (var t in blazorFile.module.GetTypes().Where(x=> !LocalFunctions.HasDoNotMangleAttribute(x.CustomAttributes)))
            {
                //Console.WriteLine(t.Name);

                
               
                 var properties = t.Properties.Where(x => !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."))
                    .Where(x=> !LocalFunctions.HasParameterAttribute(x.CustomAttributes));
               


                foreach (var i in properties)
                {
                    

                    if (!LocalData.manglePublic)
                    {
                        if (i.GetMethod != null)
                        {
                            if (i.GetMethod.IsPublic)
                            {
                                continue;
                            }
                        }

                        if (i.SetMethod != null)
                        {
                            if (i.SetMethod.IsPublic)
                            {
                                continue;
                            }
                        }
                    }


                    Console.WriteLine(i.Name);

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("=====property " + i.Name + ", type " + t.Name);
                    }

                    counter++;
                    i.Name = "P" + CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }

                    
                }

                counter = 0;
            }

            if (LocalData.verbose)
            {
                Console.WriteLine("renamed properties count = " + Stat.counter);
            }
        }

        private void RenameTypeNames()
        {
            Stat.Reset();
            int counter = 0;

            IEnumerable<TypeDefinition> types;

            bool b = LocalData.manglePublic;
            if (blazorFile.IsPrimary)
            {
                b = false;
            }

            if (b)
            {
                types = blazorFile.module.GetTypes().Where(x=>!LocalFunctions.IsDerrivedFrom(x, "Attribute", false) && !LocalFunctions.IsDerrivedFrom(x, "ComponentBase", true)).Where(x => !LocalData.ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
            }
            else
            {
                types = blazorFile.module.GetTypes().Where(x => !LocalFunctions.IsDerrivedFrom(x, "Attribute", false) && !LocalFunctions.IsDerrivedFrom(x, "ComponentBase", true)).Where(x => !LocalData.ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && x.IsNotPublic && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
            }
           
            foreach (var item in types)
            {
                

                if (LocalData.verboseDeep)
                {
                    Console.WriteLine("=====type " + item.Name);
                }

                counter++;
                item.Name = "T" + CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());

                if (LocalData.verboseDeep)
                {
                    Console.WriteLine("    =====new value " + item.Name);
                }

                
            }

            if (LocalData.verbose)
            {
                Console.WriteLine("renamed types count = " + counter, MethodBase.GetCurrentMethod());
            }
        }

        private void RenameNamespaceNames()
        {
            Stat.Reset();

            IEnumerable<TypeDefinition> types = blazorFile.module.GetTypes().Where(x => !string.IsNullOrEmpty(x.Namespace)).Where(x=>x.Namespace.Contains(".") && !x.Namespace.Contains("Shared"));

            List<string> nss = types.Select(x => x.Namespace).Distinct().ToList();

            foreach (var item in types)
            {
                if (LocalData.verboseDeep)
                {
                    Console.WriteLine("=====namespace " + item.Namespace);
                }

                item.Namespace = "N" + CodeHelper.GetCode(nss.IndexOf(item.Namespace), MethodBase.GetCurrentMethod());

                if (LocalData.verboseDeep)
                {
                    Console.WriteLine("    =====new value " + item.Namespace);
                }
            }

            if (LocalData.verbose)
            {
                Console.WriteLine("renamed namespaces count = " + nss.Count, MethodBase.GetCurrentMethod());
            }
        }

        private void RenameEnumNames()
        {
            Stat.Reset();

            int counter = 0;

            IEnumerable<TypeDefinition> enums;

            if (LocalData.manglePublic)
            {
                enums = blazorFile.module.GetTypes().Where(x =>!x.IsRuntimeSpecialName && x.IsEnum);
            }
            else
            {
                enums = blazorFile.module.GetTypes().Where(x => !x.IsPublic && !x.IsNestedPublic && !x.IsRuntimeSpecialName && x.IsEnum);
            }
            

            foreach (var e in enums)
            {

                var fields = e.Fields.Where(x => !x.IsRuntimeSpecialName);

                foreach (var i in fields)
                {
                 
                    if (i.Name == "value__")
                    {
                        continue;
                    }

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("=====enumField " + i.Name + ", enum " + e.Name);
                    }

                    counter++;
                    i.Name = CodeHelper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (LocalData.verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }

                    
                }

                counter = 0;

            }


            if (LocalData.verbose)
            {
                Console.WriteLine("renamed enums count = " + Stat.counter);
            }
        }

    }
}
