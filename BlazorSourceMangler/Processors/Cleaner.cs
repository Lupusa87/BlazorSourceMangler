using BlazorSourceMangler.Helpers;
using BlazorSourceMangler.Locals;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static BlazorSourceMangler.Locals.LocalEnums;

namespace BlazorSourceMangler.Processors
{
    public class Cleaner
    {
        BlazorFile blazorFile;
        int phase;


        internal Cleaner(BlazorFile _blazorFile, int _phase)
        {
            blazorFile = _blazorFile;

            phase = _phase;

            ProcessFile();
        }


        private void ProcessFile()
        {


            if (LocalData.verbose)
            {
                Console.WriteLine(Environment.NewLine);


                switch (LocalData.appMode)
                {
                    case AppMode.CleanDeadCodeAndMangle:
                    case AppMode.CleanDeadCode:
                        Console.WriteLine("======DCCleaning file========");
                        break;
                    case AppMode.ReportDeadCode:
                        Console.WriteLine("======DCReporting file========");
                        break;
                    default:
                        Console.WriteLine("======processing file========");
                        break;

                }

                Console.WriteLine("=========phase "+ phase + "===========");
                Console.WriteLine( blazorFile.FI.FullName);
            }

            CleanDeadTypes();
            CleanDeadMethods();


            //CleanDeadParameters();
            //CleanDeadFields();


            //!!! can't be used for now because builder has hard coded prop names and mangling is giving error!!!
            // builder should instead use properties for example not "a" but someclass.a
            //CleanDeadProperties();


        }



        private void CleanDeadParameters()
        {
            Stat.Reset();
            int counter = 0;

            var toInspect = blazorFile.module.GetTypes().Where(x => !x.Name.Contains("<"))
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

        private void CleanDeadFields()
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


        private void CleanDeadProperties()
        {
            Stat.Reset();
            int counter = 0;


            foreach (var t in blazorFile.module.GetTypes().Where(x => !LocalFunctions.HasDoNotMangleAttribute(x.CustomAttributes)))
            {
                //Console.WriteLine(t.Name);



                var properties = t.Properties.Where(x => !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."))
                   .Where(x => !LocalFunctions.HasParameterAttribute(x.CustomAttributes));



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


        private void CleanDeadTypes(int counter = 0)
        {

            Console.WriteLine(Environment.NewLine);

            bool CallRecursion = false;
            List<string> mustdelete = new List<string>();

            IEnumerable<TypeDefinition> types;

            
            // types = blazorFile.module.GetTypes().Where(x => !LocalFunctions.IsDerrivedFrom(x, "Attribute") && !LocalFunctions.IsDerrivedFrom(x, "ComponentBase")).Where(x => !LocalData.ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
            
            types = blazorFile.module.Types.Where(x => !LocalFunctions.IsDerrivedFrom(x, "Attribute", false) && !LocalFunctions.IsDerrivedFrom(x, "ComponentBase", true))
                .Where(x => !LocalData.ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));
          

            foreach (var item in types)
            {
                //if (LocalData.verboseDeep)
                //{
                //    Console.WriteLine("=====type " + item.Name);
                //}


               

                if (item.IsNotPublic)
                {
                    if (!LocalFunctions.HasReference(blazorFile,true, item.Name))
                    {
                        counter++;
                        Console.WriteLine("=====type " + item.Name + " has no references");

                        mustdelete.Add(item.Name);

                    }
                }
                else
                {
                    bool HasReference = LocalFunctions.HasReference(blazorFile, true, item.Name);


                    if (!HasReference && blazorFile.Usings.Count>0)
                    {

                        foreach (var bf in blazorFile.Usings)
                        {
                            if (LocalFunctions.HasReference(bf, false, blazorFile.Name + "." + item.Name))
                            {
                                HasReference = true;
                                break;
                            }
                        }
   
                    }

                    if (!HasReference)
                    {
                        counter++;
                        Console.WriteLine("=====type " + item.Name + " has no references");

                        mustdelete.Add(item.Name);
                    }
                }

            }


            if (LocalData.appMode == AppMode.CleanDeadCode || LocalData.appMode == AppMode.CleanDeadCodeAndMangle)
            {
                foreach (var item in mustdelete)
                {
                    Console.WriteLine("========deleting " + item);
                    if (types.Any(x => x.Name.Equals(item)))
                    {
                        try
                        {
                            blazorFile.module.Types.Remove(types.FirstOrDefault(x => x.Name.Equals(item)));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }


            CallRecursion = mustdelete.Count > 0;
            mustdelete = new List<string>();


            if (CallRecursion)
            {
                CleanDeadTypes(counter);
            }
            else
            {
                if (LocalData.verbose)
                {

                    switch (LocalData.appMode)
                    {
                        case AppMode.CleanDeadCodeAndMangle:
                        case AppMode.CleanDeadCode:
                            Console.WriteLine("Cleared Dead types count = " + counter, MethodBase.GetCurrentMethod());
                            break;
                        case AppMode.ReportDeadCode:
                            Console.WriteLine("Reported Dead types count = " + counter, MethodBase.GetCurrentMethod());
                            break;
                        default:
                            Console.WriteLine("Dead types count = " + counter, MethodBase.GetCurrentMethod());
                            break;

                    }

                }
            }
        }

        private void CleanDeadMethods(int counter = 0)
        {

            Console.WriteLine(Environment.NewLine);

            bool CallRecursion = false;
            List<string> mustdelete = new List<string>();

            IEnumerable<TypeDefinition> types;


            types = blazorFile.module.Types.Where(x => !LocalFunctions.IsDerrivedFrom(x, "Attribute", false))
                .Where(x => !LocalData.ShouldIgnoreClases.Any(y => y.Equals(x.Name.ToLower())) && !x.IsRuntimeSpecialName && !x.Name.Contains("<") && !x.Name.Contains("."));


            foreach (var t in types)
            {

                //if (LocalData.verboseDeep)
                //{
                //    Console.WriteLine("=====Method " + item.Name);
                //}


                var methods = t.Methods.Where(x => !x.IsConstructor && !x.IsRuntimeSpecialName && !x.IsSetter && !x.IsGetter);

                mustdelete = new List<string>();
                foreach (var m in methods)
                {


                    if (m.IsVirtual && !m.IsNewSlot)
                    {
                        continue;
                    }

                    if (m.Name.Contains("<"))
                    {
                        continue;
                    }


                    if (!m.IsPublic)
                    {

                        //if (m.Name=="test11" && t.Name == "MyInternalStaticClasses")
                        //{
                        //    Console.WriteLine("??????????????????????????????????" + m.Name + " " + t.Name +" " + m.Attributes);
                        //}

                        if (!LocalFunctions.HasReference(blazorFile, true, m.Name, t.Name))
                        {

                           
                            counter++;
                            Console.WriteLine("=====Method " + t.Name + "." + m.Name + " has no references");

                            mustdelete.Add(m.Name);

                        }
                    }
                    else
                    {
                        bool HasReference = LocalFunctions.HasReference(blazorFile, true, m.Name, t.Name);


                        if (!HasReference && blazorFile.Usings.Count > 0)
                        {

                            foreach (var bf in blazorFile.Usings)
                            {
                                if (LocalFunctions.HasReference(bf, false, blazorFile.Name + "." + t.Name + "::" + m.Name))
                                {
                                    HasReference = true;
                                    break;
                                }
                            }

                        }

                        if (!HasReference)
                        {
                            counter++;
                           
                            Console.WriteLine("=====Method " + t.Name + "." + m.Name + " has no references");

                            mustdelete.Add(m.Name);
                        }
                    }



                   
                 
                }


                if (LocalData.appMode == AppMode.CleanDeadCode || LocalData.appMode == AppMode.CleanDeadCodeAndMangle)
                {
                    if (!CallRecursion)
                    {
                        CallRecursion = mustdelete.Count > 0;
                    }

                    foreach (var md in mustdelete)
                    {
                        Console.WriteLine("========deleting " + t.Name + "." + md);
                        if (t.Methods.Any(x => x.Name.Equals(md)))
                        {
                            try
                            {
                                t.Methods.Remove(t.Methods.FirstOrDefault(x => x.Name.Equals(md)));
                            }
                            catch (Exception)
                            {
                            }
                        }

                       // Console.WriteLine("========deleted " + t.Name + "." + md);
                    }

                    Console.WriteLine(Environment.NewLine);
                }


                mustdelete = new List<string>();

               
            }
           

            if (CallRecursion)
            {
                CleanDeadTypes(counter);
            }
            else
            {
                if (LocalData.verbose)
                {

                    switch (LocalData.appMode)
                    {
                        case AppMode.CleanDeadCodeAndMangle:
                        case AppMode.CleanDeadCode:
                            Console.WriteLine("Cleared Dead methods count = " + counter, MethodBase.GetCurrentMethod());
                            break;
                        case AppMode.ReportDeadCode:
                            Console.WriteLine("Reported Dead methods count = " + counter, MethodBase.GetCurrentMethod());
                            break;
                        default:
                            Console.WriteLine("Dead methods count = " + counter, MethodBase.GetCurrentMethod());
                            break;

                    }
                   
                }
            }
        }

        private void RemoveDeadMethods()
        {
            Stat.Reset();
            List<string> mustdelete = new List<string>();

            int counter = 0;
            int counterDead = 0;
            foreach (var t in blazorFile.module.GetTypes())
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

            if (LocalData.verbose)
            {
                Console.WriteLine("methods count = " + counter + " dead " + counterDead);
            }



        }


        private int GetReferencesCount(string methodName)
        {
            int referenceCount = 0;

            foreach (var t in blazorFile.module.GetTypes())
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


            foreach (var type in blazorFile.module.GetTypes())
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

            foreach (var t in blazorFile.module.GetTypes())
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
