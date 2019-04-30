using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BlazorSourceMangler
{
    internal class Mangler
    {
       
        FileInfo inputFile;
        FileInfo outputFile;

        ModuleDefinition md;

        bool verbose;
        bool verboseDeep;
        bool manglePublic;

        DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();


        List<string> ShouldIgnoreClases = new List<string>()
        {
            "app", "program", "startup", "_imports", "mainlayout"
        };


        internal Mangler(FileInfo _inputFile, FileInfo _outputFile,bool _manglePublic, bool _verbose, bool _verboseDeep)
        {

            resolver.AddSearchDirectory(_outputFile.DirectoryName);
            md = ModuleDefinition.ReadModule(_inputFile.FullName, new ReaderParameters { AssemblyResolver = resolver });

            inputFile = _inputFile;
            outputFile = _outputFile;
            manglePublic = _manglePublic;
            verbose = _verbose;
            verboseDeep = _verboseDeep;
        }


        internal void ProcessFile()
        {
            if (verbose)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("======processing file========");
                Console.WriteLine(inputFile.FullName);
            }


            //Under Construction
            //RemoveDeadMethods(md);

            RenameEnumNames();
            RenameParameters();
            RenameFieldNames();


            //!!! can't be used for now because builder has hard coded prop names and manling is giving error!!!
            // builder shoul dinstead use properties for example not "abc" but someclass.abc
            // RenamePropertyNames();


            RenameMethodNames();
            RenameTypeNames();

            SaveResult();
        }


        private void SaveResult()
        {
            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("======saving file========");
                Console.WriteLine(outputFile.FullName);
            }
            md.Write(outputFile.FullName);
        }

        private void RenameParameters()
        {
            Stat.Reset();
            int counter = 0;

            var toInspect = md.GetTypes().Where(x=>!x.Name.Contains("<"))
                  .SelectMany(t => t.Methods.Select(m => new { t, m }))
                  .Where(x => x.m.HasParameters && !x.m.Name.Contains("<") && !x.m.Name.StartsWith("set_"));


            foreach (var item in toInspect)
            {
                //Console.WriteLine(item.m.Name);
                foreach (var i in item.m.Parameters)
                {
                    counter++;

                    if (verboseDeep)
                    {
                        Console.WriteLine("=====parameter " + i.Name + ", method " + item.m.Name + ", type " + item.t.Name);
                    }

                    i.Name = Helper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }
                }

                counter = 0;
            }

            if (verbose)
            {
                Console.WriteLine("renamed parameters count = " + Stat.counter);
            }
        }



        private void RenameMethodNames()
        {
            Stat.Reset();

            int counter = 0;

            IEnumerable<MethodDefinition> methods;

            foreach (var t in md.GetTypes().Where(x => !x.Name.Contains("<") && !ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower()))))
            {


                if (manglePublic)
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


                    if (verboseDeep)
                    {
                        Console.WriteLine("=====method " + i.Name + ", type " + t.Name);
                    }

                    counter++;
                    if (i.Name.Contains("."))
                    {
                        i.Name = i.Name.Substring(0, i.Name.LastIndexOf(".") + 1) + "M" + Helper.GetCode(counter, MethodBase.GetCurrentMethod());
                    }
                    else
                    {
                        i.Name = "M" + Helper.GetCode(counter, MethodBase.GetCurrentMethod());
                    }

                    if (verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }

                    
                }

                counter = 0;
            }

            if (verbose)
            {
                Console.WriteLine("renamed methods count = " + Stat.counter);
            }
        }

        private void RenameFieldNames()
        {
            Stat.Reset();

            int counter = 0;

            IEnumerable<FieldDefinition> fields;

            foreach (var t in md.GetTypes())
            {

                if (manglePublic)
                {

                    fields = t.Fields.Where(x => !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
                }
                else
                {
                    fields = t.Fields.Where(x => !x.IsPublic && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
                }

                foreach (var i in fields)
                {
                    if (verboseDeep)
                    {
                        Console.WriteLine("=====field " + i.Name + ", type " + t.Name);
                    }

                    counter++;

                    i.Name = "F" + Helper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }
                    

                }

                counter = 0;
            }

            if (verbose)
            {
                Console.WriteLine("renamed fields count = " + Stat.counter);
            }
        }


        private void RenamePropertyNames()
        {
            Stat.Reset();
            int counter = 0;


            

            foreach (var t in md.GetTypes())
            {
               
                 var properties = t.Properties.Where(x => !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
               

                foreach (var i in properties)
                {

                    if (!manglePublic)
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


                    if (verboseDeep)
                    {
                        Console.WriteLine("=====property " + i.Name + ", type " + t.Name);
                    }

                    counter++;
                    i.Name = "P" + Helper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }

                    
                }

                counter = 0;
            }

            if (verbose)
            {
                Console.WriteLine("renamed properties count = " + Stat.counter);
            }
        }


        private void RenameTypeNames()
        {
            Stat.Reset();
            int counter = 0;

            IEnumerable<TypeDefinition> types;

            if (manglePublic)
            {
                types = md.GetTypes().Where(x => !ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
            }
            else
            {
                types = md.GetTypes().Where(x => !ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && x.IsNotPublic && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
            }
           
            foreach (var item in types)
            {

                if (verboseDeep)
                {
                    Console.WriteLine("=====type " + item.Name);
                }

                counter++;
                item.Name = "T" + Helper.GetCode(counter, MethodBase.GetCurrentMethod());

                if (verboseDeep)
                {
                    Console.WriteLine("    =====new value " + item.Name);
                }

                
            }

            if (verbose)
            {
                Console.WriteLine("renamed types count = " + counter, MethodBase.GetCurrentMethod());
            }
        }


        private void RenameEnumNames()
        {
            Stat.Reset();

            int counter = 0;

            IEnumerable<TypeDefinition> enums;

            if (manglePublic)
            {
                enums = md.GetTypes().Where(x =>!x.IsRuntimeSpecialName && x.IsEnum);
            }
            else
            {
                enums = md.GetTypes().Where(x => !x.IsPublic && !x.IsNestedPublic && !x.IsRuntimeSpecialName && x.IsEnum);
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

                    if (verboseDeep)
                    {
                        Console.WriteLine("=====enumField " + i.Name + ", enum " + e.Name);
                    }

                    counter++;
                    i.Name = Helper.GetCode(counter, MethodBase.GetCurrentMethod());

                    if (verboseDeep)
                    {
                        Console.WriteLine("    =====new value " + i.Name);
                    }

                    
                }

                counter = 0;

            }


            if (verbose)
            {
                Console.WriteLine("renamed enums count = " + Stat.counter);
            }
        }


        private void RemoveDeadMethods()
        {
            Stat.Reset();
            List<string> mustdelete = new List<string>();

            int counter = 0;
            int counterDead = 0;
            foreach (var t in md.GetTypes())
            {

                var methods = t.Methods.Where(x => x.IsPublic == false && !x.IsConstructor && !x.IsRuntimeSpecialName);

                foreach (var i in methods)
                {


                    if (i.IsVirtual && !i.IsNewSlot)
                    {
                        continue;
                    }

                    if (i.Name.Contains("<"))
                    {
                        continue;
                    }


                    try
                    {
                        if (HasDependency(i) == false)
                        {
                            counterDead++;
                            //Console.WriteLine(methods[i].Name);

                            mustdelete.Add(i.Name);
                        }
                    }
                    catch (Exception)
                    {
                        return;
                        Console.WriteLine("erorr===========");
                    }


                    //int  c = GetReferencesCount(md, methods[i].Name);

                    //if (c > 0)
                    //{
                    //    Console.WriteLine(c + " " + methods[i].Name);
                    //}

                    counter++;

                }

                if (mustdelete.Count > 0)
                {
                    Console.WriteLine("======deleting========");
                }

                foreach (var item in mustdelete)
                {
                    Console.WriteLine(item);
                    if (t.Methods.Any(x => x.Name.Equals(item)))
                    {
                        try
                        {
                            t.Methods.Remove(t.Methods.FirstOrDefault(x => x.Name.Equals(item)));
                        }
                        catch (Exception)
                        {


                        }

                    }

                    Console.WriteLine("======00000000========");
                }

                mustdelete = new List<string>();

            }

            if (verbose)
            {
                Console.WriteLine("methods count = " + counter + " dead " + counterDead);
            }



        }


        private int GetReferencesCount(string methodName)
        {
            int referenceCount = 0;

            foreach (var t in md.GetTypes())
            {

                referenceCount += t.Methods.Where(p => p.HasBody)
                                               .SelectMany(p => p.Body.Instructions.Where(i => i.OpCode.Code == Code.Call &&
                                               ((MethodReference)i.Operand).DeclaringType.FullName.Equals(methodName))).Count();
            }
            return referenceCount;
        }


        bool HasDependency(MethodReference method)
        {
            //try
            //{


            foreach (var type in md.GetTypes())
            {
                foreach (var m in type.Methods.Where(x => x.HasBody))
                {
                    if (m.Body.Instructions.Any(il =>
                    {
                        //it can be without call, for example when subscribed on action, in this case method it is just assigned not called 
                        //if (il.OpCode == OpCodes.Call) 
                        //{
                        var mRef = il.Operand as MethodReference;
                        if (mRef != null && string.Equals(mRef.FullName, method.FullName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                        //}
                        return false;
                    }))
                    {
                        return true;
                    }
                }
            }


            //}
            //catch (Exception)
            //{

            //    Console.WriteLine("==================================================error");
            //    return true;
            //}

            return false;
        }


        private bool HasReference(string methodName)
        {
            int referenceCount = 0;

            foreach (var t in md.GetTypes())
            {

                referenceCount = t.Methods.Where(p => p.HasBody)
                                               .SelectMany(p => p.Body.Instructions.Where(i => i.OpCode.Code == Code.Call &&
                                               ((MethodReference)i.Operand).DeclaringType.FullName.Equals(methodName))).Count();
                if (referenceCount > 0)
                {
                    return true;
                }

            }

            return false;
        }

    }
}
