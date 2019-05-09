using BlazorSourceMangler.Processors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlazorSourceMangler.Locals
{
    internal static class LocalFunctions
    {
        private static Random rnd = new Random();

        internal static bool HasParameterAttribute(ICollection<CustomAttribute> attr)
        {
            foreach (var item in attr)
            {

                if (item.AttributeType.FullName.Contains("ParameterAttribute"))
                {
                    //Console.WriteLine("?????   " + item.AttributeType);
                    return true;
                }
            }


            return false;

        }

        internal static bool HasDoNotMangleAttribute(ICollection<CustomAttribute> attr)
        {
            foreach (var item in attr)
            {
                if (item.AttributeType.FullName.Contains("DoNotMangleAttribute"))
                {
                    //Console.WriteLine("?????????????   " + item.AttributeType);
                    return true;
                }
            }


            return false;

        }

        internal static bool IsDerrivedFrom(TypeDefinition t, string ClassName, bool DrillDown)
        {

            //Console.WriteLine(t.Name);

            if (t.BaseType is null)
            {
                return false;

            }
            else
            {
                //Console.WriteLine(t.Name + "        " + t.BaseType.FullName);

                if (t.BaseType.FullName.EndsWith("." + ClassName))
                {
                    return true;
                }
                else
                {
                    if (DrillDown)
                    {
                        if (t.BaseType.Namespace.Equals(t.Namespace))
                        {
                            TypeDefinition bt = t.BaseType.Resolve();


                            if (bt.FullName.EndsWith("." + ClassName))
                            {
                                return true;
                            }
                            else
                            {
                                IsDerrivedFrom(bt, ClassName, DrillDown);
                            }

                        }
                    }
                }


                return t.BaseType.FullName.EndsWith("." + ClassName);
            }


        }

        internal static List<string> GetInheritanceForSameModule(TypeDefinition t, List<string> list)
        {

            if (t.BaseType is null)
            {
                return list;

            }
            else
            {
                if (t.BaseType.Namespace.Equals(t.Namespace))
                {
                    list.Add(t.BaseType.Name);

                    GetInheritanceForSameModule(t.BaseType.Resolve(), list);
                }

            }

            return list;
        }

        internal static void EnsureOutputDirectory()
        {
            if (LocalData.outputDir.Exists)
            {
                LocalData.outputDir.GetFiles().ToList().ForEach(f => f.Delete());
                LocalData.outputDir.GetDirectories().ToList().ForEach(d => d.Delete(true));
            }
            else
            {
                LocalData.outputDir.Create();
            }
        }

        internal static void AnalyzeReferences()
        {
            ModuleDefinition md;
            foreach (var bf in LocalData.BlazorFilesList)
            {

                md = ModuleDefinition.ReadModule(bf.FI.FullName);

                if (md.HasAssemblyReferences)
                {
                    foreach (var item in md.AssemblyReferences)
                    {
                        if (File.Exists(Path.Combine(bf.FI.Directory.FullName, item.Name + ".dll")))
                        {
                            if (LocalData.BlazorFilesList.Any(x => x.FI.Name.Equals(item.Name + ".dll")))
                            {

                                BlazorFile tmp = LocalData.BlazorFilesList.Single(x => x.FI.Name.Equals(item.Name + ".dll"));

                                bf.References.Add(tmp);

                                if (!tmp.Usings.Any(x => x == bf))
                                {
                                    tmp.Usings.Add(bf);
                                }

                            }
                        }
                    }
                }

                md.Dispose();
            }

            foreach (var item in LocalData.BlazorFilesList)
            {
                foreach (var i in item.Usings)
                {
                    Console.WriteLine(item.FI.Name + " is used by " + i.FI.Name);
                }
            }


            if (LocalData.BlazorFilesList.Count(x => x.Usings.Count == 0) == 1)
            {
                LocalData.BlazorFilesList.First(x => x.Usings.Count == 0).IsPrimary = true;

                Console.WriteLine(Environment.NewLine + "Primary file is " + LocalData.BlazorFilesList.First(x => x.Usings.Count == 0).FI.Name);
            }
            else
            {
                throw new Exception("Can't determine primary file");
            }
        }

        public static bool HasReference(BlazorFile bf, bool LookingInsideParentModule, string Name, string TypeName="")
        {

            string tmpName = Name;

            //Console.WriteLine(Environment.NewLine + "Looking in " + bf.Name +  " name " + Name +" " +LookingInsideParentModule);


            foreach (var t in bf.module.GetTypes())
            {
                //

                if (LookingInsideParentModule)
                {


                    if (string.IsNullOrEmpty(TypeName))
                    {
                        tmpName = Name;
                    }
                    else
                    {
                        if (t.Name.Equals(TypeName))
                        {
                            tmpName = Name;
                        }
                        else
                        {

                            List<string> tmp_list = GetInheritanceForSameModule(t, new List<string>());

                            //if (tmp_list.Any())
                            //{
                            //    Console.WriteLine(Environment.NewLine + "abc " + Name +" "+ TypeName);
                            //    Console.WriteLine("Class inheritance for " + t.Name);
                            //    foreach (var item in tmp_list)
                            //    {
                            //        Console.WriteLine("===" + item);
                            //    }
                            //    Console.WriteLine(Environment.NewLine);
                            //}

                            if (tmp_list.Any(x => x.Equals(TypeName)))
                            {

                                tmpName = Name;
                            }
                            else
                            {
                                tmpName = TypeName + "::" + Name;
                            }
                        }
                    }
                }
                else
                {
                    tmpName = Name;
                }


                //foreach (var item in t.Methods.Where(p => p.HasBody))
                //{
                //    foreach (var it in item.Body.Instructions.Where(x=>x.ToString().Contains(Name)))
                //    {
                //        Console.WriteLine(it.ToString());
                //    }

                //}



                //if (Name == "test11")
                //{
                //    IEnumerable<Instruction> aaa = t.Methods.Where(p => p.HasBody)
                //                               .SelectMany(p => p.Body.Instructions.Where(x => x.ToString().Contains("test11")));

                //    foreach (var item in aaa)
                //    {
                //        Console.WriteLine(item.ToString());
                //        // Console.WriteLine(item.Operand.ToString());

                //        //Console.WriteLine(item.Operand. .ToString() + "         " + Name);
                //    }
                //}


                IEnumerable<Instruction> a = t.Methods.Where(p => p.HasBody)
                                               .SelectMany(p => p.Body.Instructions.Where(x => x.ToString().Contains(tmpName)));

                //IEnumerable<Instruction> a = t.Methods.Where(p => p.HasBody)
                //                               .SelectMany(p => p.Body.Instructions.Where(i => i.OpCode.Code == Code.Call &&
                //                               ((MethodReference)i.Operand).DeclaringType.FullName.Contains(Name)));


                //foreach (var item in a)
                //{
                //    Console.WriteLine(item.ToString());
                //    // Console.WriteLine(item.Operand.ToString());

                //    //Console.WriteLine(item.Operand. .ToString() + "         " + Name);
                //}

                if (a.Count() > 0)
                {
                    return true;
                }
                        
            }
            return false;
        }


        private static CalledItem ParseCalledItem(Instruction item, string Name)
        {

            string s = item.Operand.ToString();

            CalledItem ci = new CalledItem()
            {
                OpCode = item.OpCode.Code.ToString(),
            };


            if (ci.OpCode.Contains("call"))
            {
                ci.type = "Method";
            };

            if (ci.OpCode.Contains("newobj"))
            {
                ci.type = "Constructor";
            };

            if (ci.OpCode.Contains("fld"))
            {
                ci.type = "field";
            };

            int k = s.IndexOf(Name);


            if (k > -1)
            {
                s = s.Substring(k + Name.Length + 1, s.Length - k - Name.Length - 1);
            }

            k = s.IndexOf(":");

            if (k > -1)
            {
                ci.Module = s.Substring(0, k);
                Console.WriteLine(ci.Module);
                s = s.Substring(k + 2, s.Length - k - 2);
            }

            k = s.IndexOf("(");


            if (k > -1)
            {
                ci.Name = s.Substring(0, k);
                Console.WriteLine(ci.Name);
            }
            else
            {
                ci.Name = s;
                Console.WriteLine(ci.Name);
            }



            return ci;
        }



        public static void AnalyzeHardCodedPropertyNames()
        {
            ModuleDefinition md;

            foreach (var bf in LocalData.BlazorFilesList)
            {

                md = ModuleDefinition.ReadModule(bf.FI.FullName);

                foreach (var t in md.GetTypes())
                {

                    foreach (var item in t.Methods.Where(p => p.HasBody))
                    {
                        foreach (var it in item.Body.Instructions.Where(x => x.ToString().Contains("OpenComponent")))
                        {
                            //Console.WriteLine(it.ToString());
                            DrillDown(it);
                        }

                    }
                }

                md.Dispose();
            }
        }


        public static void DrillDown(Instruction i)
        {
            Console.WriteLine("============================");
            Console.WriteLine(Environment.NewLine + i.OpCode.Code.ToString() + " " + i.Operand.ToString());

            if (i.Next is null)
            {
                return;
            }
            Instruction current = i.Next;

            while (!current.ToString().Contains("CloseComponent"))
            {
                if (current.Operand != null)
                {
                    Console.WriteLine(current.OpCode.Code.ToString() + " " + current.Operand.ToString());
                }

                if (current.Next != null)
                {
                    current = current.Next;
                }
                else
                {
                    return;
                }
            }

            Console.WriteLine(current.OpCode.Code.ToString() + " " + current.Operand.ToString());
        }


        public static void RunMangler()
        {
            Mangler m;
            foreach (var item in LocalData.BlazorFilesList.Where(x=>!x.IsPrimary))
            {
                m = new Mangler(item);
            }

            m = new Mangler(LocalData.BlazorFilesList.Single(x => x.IsPrimary));

        }

        public static void RunDCCleaner()
        {
            RunCleaner();
        }

        public static void RunDCReporter()
        {
            RunCleaner();
        }

        private static void RunCleaner()
        {
            Cleaner c;

            for (int i = 1; i <= LocalData.CleanerRepeatCount; i++)
            {
                //int i = 1;

                ReorderList();

                foreach (var item in LocalData.BlazorFilesList.OrderBy(x => x.SequenceNumber))
                {
                    c = new Cleaner(item, i);
                }
            }
        }


        public static void SaveResult()
        {
            foreach (var item in LocalData.BlazorFilesList.OrderByDescending(x=>x.Usings.Count))
            {

                

                if (LocalData.verbose)
                {
                    Console.WriteLine(Environment.NewLine +  "======saving result========");
                    Console.WriteLine(Environment.NewLine + "======saving file========");
                    Console.WriteLine(item.OutputFile.FullName);
                }


                item.module.Write(item.OutputFile.FullName);
            }

           
        }


        public static void ReorderList()
        {
            if (LocalData.BlazorFilesList.Count > 1)
            {
                foreach (var item in LocalData.BlazorFilesList)
                {
                    item.SequenceNumber = rnd.NextDouble();
                }

                LocalData.BlazorFilesList.Single(x => x.IsPrimary).SequenceNumber = 0;

            }
        }

    }
}
