using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Compile
{
    public class ScriptEngine
    {
        public readonly string[] ExtraReferencedAssemblies;

        private readonly CodeDomProvider provider;
        private readonly string codeType = "csharp";

        public ScriptEngine() : this(null, null)
        {
        }

        public ScriptEngine(string[] extraReferencedAssemblies)
            : this(null, extraReferencedAssemblies)
        {
        }

        public ScriptEngine(string codeType, string[] extraReferencedAssemblies)
        {
            if (!String.IsNullOrEmpty(codeType))
                this.codeType = codeType;
            this.ExtraReferencedAssemblies = extraReferencedAssemblies;

            // There is currently no way to generically create a CodeDomProvider and have it work with .NET 3.5
            provider = CodeDomProvider.CreateProvider(this.codeType);
        }

        public Assembly Compile(string directory)
        {
            string[] files = GetFilesRecursive(directory);
            Console.Out.WriteLine("Compiling:");
            Array.ForEach(files, file => Console.Out.WriteLine(file));

            return Compile(files);
        }

        private string[] GetFilesRecursive(string directory)
        {
            FileInfo[] files = GetFilesRecursive(new DirectoryInfo(directory));
            string[] fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i ++)
            {
                fileNames[i] = files[i].FullName;
            }
            return fileNames;
        }

        private FileInfo[] GetFilesRecursive(DirectoryInfo d)
        {
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(d.GetFiles(String.Format("*.{0}", provider.FileExtension)));
            DirectoryInfo[] subDirs = d.GetDirectories();
            if (subDirs.Length > 0)
            {
                foreach (DirectoryInfo subDir in subDirs)
                {
                    files.AddRange(GetFilesRecursive(subDir));
                }
            }

            return files.ToArray();
        }

        public Assembly Compile(params string[] files)
        {
            CompilerParameters parms = SetupCompilerParams();

            CompilerResults compileResult = provider.CompileAssemblyFromFile(parms, files);
            if (compileResult.Errors.Count != 0)
            {
                foreach (CompilerError err in compileResult.Errors)
                {
                    Console.Error.WriteLine("{0} ({1}:{2})  {3}", err.FileName, err.Line, err.Column, err.ErrorText);
                }
            }
            return compileResult.CompiledAssembly;
        }

        private CompilerParameters SetupCompilerParams()
        {
            string migrationFrameworkPath = FrameworkAssemblyPath();
            CompilerParameters parms = new CompilerParameters();
            parms.CompilerOptions = "/t:library";
            parms.GenerateInMemory = true;
            parms.IncludeDebugInformation = true;
            parms.OutputAssembly = Path.Combine(Path.GetDirectoryName(migrationFrameworkPath), "MyMigrations.dll");

            Console.Out.WriteLine("Output assembly: " + parms.OutputAssembly);

            // Add Default referenced assemblies
            parms.ReferencedAssemblies.Add("mscorlib.dll");
            parms.ReferencedAssemblies.Add("System.dll");
            parms.ReferencedAssemblies.Add("System.Data.dll");
            parms.ReferencedAssemblies.Add(FrameworkAssemblyPath());
            if (null != ExtraReferencedAssemblies && ExtraReferencedAssemblies.Length > 0)
            {
                Array.ForEach(ExtraReferencedAssemblies,
                              delegate(String assemb) { parms.ReferencedAssemblies.Add(assemb); });
            }
            return parms;
        }
        
        private static string FrameworkAssemblyPath()
        {
            string path = typeof (MigrationAttribute).Module.FullyQualifiedName;
            Console.Out.WriteLine("Framework DLL: " + path);
            return path;
        }
    }
}