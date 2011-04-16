using System.Reflection;
using Mono.Cecil;

namespace DependencyPropertyWeaver
{
    public abstract class AssemblyWeaverBase
    {
        /// <summary>
        /// Gets whether this weaver has modified the assembly.
        /// </summary>
        public bool HasChanges { get; protected set; }

        protected AssemblyDefinition Definition { get; private set; }

        protected Assembly Assembly { get; private set; }

        protected AssemblyWeaverBase(Assembly assembly, AssemblyDefinition definition)
        {
            Assembly = assembly;
            Definition = definition;
        }
    }
}