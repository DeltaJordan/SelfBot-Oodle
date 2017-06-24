using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SelfBot.Scripting
{
    public static class ScriptHelper
    {
        public static List<CompilerError> Errors = new List<CompilerError>();

        public static string DynamicMethod(string code)
        {
            string builder =
                @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    public class Script
    {
        public static object DynamicMethod()
        {
            ";

            builder += code;
            builder +=
                @"
        }
    }
}";

            return builder;
        }

        public static string AddNamespace(string code, params string[] namespaces)
        {
            string codeBuilder = namespaces.Aggregate(string.Empty, (current, ns) => current + $"{ns}\n");

            codeBuilder += code;

            return codeBuilder;
        }

        public static Assembly Compile(string code)
        {
            CompilerParameters options = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                IncludeDebugInformation = true
            };


            List<string> refs = new List<string>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly t in assemblies)
            {
                if (!t.FullName.Contains("System.") && !t.FullName.Contains("Microsoft."))
                {
                    //refs.Add(System.IO.Path.GetFileName(t.Location));
                }
            }
            refs.Add("System.dll");
            refs.Add("System.Data.dll");
            refs.Add("System.Drawing.dll");
            refs.Add("System.Xml.dll");
            refs.Add("System.Core.dll");
            options.ReferencedAssemblies.AddRange(refs.ToArray());

            CompilerResults results = CodeDomProvider.CreateProvider(CodeDomProvider.GetLanguageFromExtension("cs")).CompileAssemblyFromSource(options, code);

            Errors = results.Errors.Cast<CompilerError>().ToList();

            return results.CompiledAssembly;
        }
    }
}
