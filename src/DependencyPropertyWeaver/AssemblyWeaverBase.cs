using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;

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

        public abstract void Weave(string typePatternMatch);

        protected MethodDefinition GetStaticCtor(TypeDefinition type)
        {
            var staticCtor = type.Methods.SingleOrDefault(m => m.Name == ".cctor");
            if (staticCtor == null)
            {
                staticCtor = new MethodDefinition(".cctor",
                                                  MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                  Definition.ImportType(Type.GetType("System.Void")));
                type.Methods.Add(staticCtor);
                staticCtor.Body.GetILProcessor().Emit(OpCodes.Ret);
            }
            return staticCtor;
        }

        protected static FieldReference GetStaticDependencyPropertyField(TypeDefinition type, string propertyName)
        {
            var field = type.Fields.SingleOrDefault(f => f.Name == propertyName + "DependencyProperty");
            if (field == null)
            {
                field = new FieldDefinition(propertyName + "DependencyProperty",
                                            FieldAttributes.Static | FieldAttributes.Public | FieldAttributes.InitOnly,
                                            type.Module.Import(typeof(DependencyProperty)));
                type.Fields.Add(field);
            }

            return field;
        }
    }
}