using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace DependencyPropertyWeaver
{
    public class DependencyPropertyWeaverTask : AppDomainIsolatedTask
    {
        /// <summary>
        /// Gets or sets the regular expression to match on the type name.
        /// </summary>
        public string TypePatternMatch { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute class used to mark types or properties to be injected.
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the files to weave.
        /// </summary>
        [Required]
        public string[] Files { get; set; }

        protected new void Log(string message, params object[] messageArgs)
        {
            if (BuildEngine != null)
                base.Log.LogMessage(message, messageArgs);
            else
                Trace.WriteLine(string.Format(message, messageArgs));
        }

        public override bool Execute()
        {
            foreach (var file in Files)
            {
                Log("Processing " + file);
                try
                {
                    var bytes = File.ReadAllBytes(file);
                    var assembly = Assembly.Load(bytes);
                    using (var stream = new MemoryStream(bytes))
                    {
                        var definition = AssemblyDefinition.ReadAssembly(stream);

                        if (Weave(assembly, definition))
                        {
                            Log("Weaving changes into " + file);
                            definition.Write(file, new WriterParameters { WriteSymbols = true });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Unable to weave file: " + ex);
                }
            }

            return true;
        }

        private bool Weave(Assembly assembly, AssemblyDefinition definition)
        {
            var properties = from module in definition.Modules
                             from type in module.Types
                             where string.IsNullOrEmpty(TypePatternMatch) || Regex.IsMatch(type.Name, TypePatternMatch)
                             from p in type.Properties
                             select p;

            var types = properties.GroupBy(p => p.DeclaringType);

            var saw = new StandardAssemblyWeaver(assembly, definition);
            saw.Modify(types);

            return saw.HasChanges;
        }
    }
}