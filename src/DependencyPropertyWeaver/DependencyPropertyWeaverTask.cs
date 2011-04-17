using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace DependencyPropertyWeaver
{
    public class DependencyPropertyWeaverTask : Task
    {
        /// <summary>
        /// Gets or sets the regular expression to match on the type name.
        /// </summary>
        public string TypePatternMatch { get; set; }

        /// <summary>
        /// Gets or sets the regular expression to match on the attribute class used to mark types or properties.
        /// </summary>
        public string AttributePatternMatch { get; set; }

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
                            definition.Write(file, new WriterParameters
                                {
                                    SymbolWriterProvider = new PdbWriterProvider(),
                                    WriteSymbols = true,
                                });
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
            var saw = new DependencyPropertyWeaver(assembly, definition);
            saw.Weave(TypePatternMatch, AttributePatternMatch);

            return saw.HasChanges;
        }
    }
}