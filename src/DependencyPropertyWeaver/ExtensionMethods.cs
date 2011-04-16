using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace DependencyPropertyWeaver
{
    public static class ExtensionMethods
    {
        public static MethodReference ImportPropertyChangedEventArgsCtor(this AssemblyDefinition assembly)
        {
            return assembly.MainModule.Import(typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) }));
        }

        public static MethodReference ImportPropertyChangedEventHandlerInvoke(this AssemblyDefinition assembly)
        {
            return assembly.MainModule.Import(typeof(PropertyChangedEventHandler).GetMethod("Invoke"));
        }

        public static MethodReference ImportObjectEqualsMethod(this AssemblyDefinition assembly)
        {
            return assembly.MainModule.Import(typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public));
        }

        public static TypeReference ImportType<T>(this AssemblyDefinition assembly)
        {
            return assembly.MainModule.Import(typeof(T));
        }

        public static TypeReference ImportType(this AssemblyDefinition assembly, Type type)
        {
            return assembly.MainModule.Import(type);
        }

        public static MethodReference ImportMethod(this AssemblyDefinition assembly, MethodBase method)
        {
            return assembly.MainModule.Import(method);
        }

        public static MethodReference ImportMethod(this AssemblyDefinition assembly, MethodReference reference)
        {
            var type = Type.GetType(reference.DeclaringType.FullName);
            var method = type.GetMethod(reference.Name);
            return assembly.MainModule.Import(method);
        }

        public static bool IsAutoPropertySetter(this PropertyDefinition property)
        {
            if (property.SetMethod == null)
                return false;
            
            var body = property.SetMethod.Body;
            var backingFieldRef = body.Instructions.FirstOrDefault(x => x.Operand != null && x.Operand.ToString().Contains("BackingField"));
            return backingFieldRef != null;
        }

        public static bool IsAutoPropertyGetter(this PropertyDefinition property)
        {
            if (property.GetMethod == null)
                return false;

            var body = property.GetMethod.Body;
            var backingFieldRef = body.Instructions.FirstOrDefault(x => x.Operand != null && x.Operand.ToString().Contains("BackingField"));
            return backingFieldRef != null;
        }
    }
}