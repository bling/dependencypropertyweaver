using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace DependencyPropertyWeaver
{
    public class AttachedDependencyPropertyWeaver : AssemblyWeaverBase
    {
        public AttachedDependencyPropertyWeaver(Assembly assembly, AssemblyDefinition definition)
            : base(assembly, definition)
        {
        }

        public override void Weave(string typePatternMatch, string attributePatternMatch)
        {
            foreach (var type in from module in Definition.Modules
                                 from type in module.Types
                                 where string.IsNullOrEmpty(typePatternMatch) || Regex.IsMatch(type.Name, typePatternMatch)
                                 select type)
                Modify(type);
        }

        private void Modify(TypeDefinition type)
        {
            foreach (var field in FindAttachedPropertyFields(type))
            {
                HasChanges = true;
                AddGetterMethod(field);
                AddSetterMethod(field);
            }
        }

        private class AttachedPropertyField
        {
            public string PropertyName;
            public TypeReference PropertyType;
            public TypeReference DeclaringType;
            public FieldReference FieldReference;
        }

        private IEnumerable<AttachedPropertyField> FindAttachedPropertyFields(TypeDefinition typeDef)
        {
            var cctor = GetStaticCtor(typeDef);
            string propertyName = null;
            TypeReference type = null;
            TypeReference declaringType = null;
            for (int i = 0; i < cctor.Body.Instructions.Count; i++)
            {
                var ins = cctor.Body.Instructions[i];
                if (ins.OpCode != OpCodes.Ldstr)
                    continue;

                propertyName = ins.Operand.ToString();

                ins = cctor.Body.Instructions.ElementAtOrDefault(++i);
                if (ins == null) continue;

                if (ins.OpCode == OpCodes.Ldtoken)
                    type = (TypeReference)ins.Operand;

                ins = cctor.Body.Instructions.ElementAtOrDefault(++i);
                if (ins == null) continue;

                if (ins.OpCode == OpCodes.Call && ins.Operand.ToString().Contains("GetTypeFromHandle"))
                    ;

                ins = cctor.Body.Instructions.ElementAtOrDefault(++i);
                if (ins == null) continue;

                if (ins.OpCode == OpCodes.Ldtoken)
                    declaringType = (TypeReference)ins.Operand;

                ins = cctor.Body.Instructions.ElementAtOrDefault(++i);
                if (ins == null) continue;

                if (ins.OpCode == OpCodes.Call && ins.Operand.ToString().Contains("GetTypeFromHandle"))
                    ;

                ins = cctor.Body.Instructions.ElementAtOrDefault(++i);
                if (ins == null) continue;

                if (ins.OpCode == OpCodes.Call && ins.Operand.ToString().Contains("RegisterAttached"))
                    ;

                ins = cctor.Body.Instructions.ElementAtOrDefault(++i);
                if (ins == null) continue;

                if (ins.OpCode == OpCodes.Stsfld)
                    yield return new AttachedPropertyField
                                     {
                                         PropertyName = propertyName,
                                         DeclaringType = declaringType,
                                         PropertyType = type,
                                         FieldReference = (FieldReference)ins.Operand
                                     };
            }
        }

        private void AddGetterMethod(AttachedPropertyField field)
        {
            var method = new MethodDefinition("Get" + field.PropertyName,
                                              MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig,
                                              field.PropertyType);

            method.Parameters.Add(new ParameterDefinition("dependencyObject", ParameterAttributes.None, Definition.ImportType<DependencyObject>()));

            var proc = method.Body.GetILProcessor();
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldsfld, field.FieldReference);
            proc.Emit(OpCodes.Callvirt, Definition.ImportMethod(typeof(DependencyObject).GetMethod("GetValue")));
            proc.Emit(field.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, field.PropertyType);
            proc.Emit(OpCodes.Ret);

            field.DeclaringType.Resolve().Methods.Add(method);
        }

        private void AddSetterMethod(AttachedPropertyField field)
        {
            var method = new MethodDefinition("Set" + field.PropertyName,
                                              MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig,
                                              Definition.ImportType(Type.GetType("System.Void")));

            method.Parameters.Add(new ParameterDefinition("dependencyObject", ParameterAttributes.None, Definition.ImportType<DependencyObject>()));
            method.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, field.PropertyType));

            var proc = method.Body.GetILProcessor();
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldsfld, field.FieldReference);
            proc.Emit(OpCodes.Ldarg_1);
            if (field.PropertyType.IsValueType)
            {
                proc.Emit(OpCodes.Box, field.PropertyType);
            }
            proc.Emit(OpCodes.Callvirt, Definition.ImportMethod(typeof(DependencyObject).GetMethod("SetValue", new[] { typeof(DependencyProperty), typeof(object) })));
            proc.Emit(OpCodes.Ret);

            field.DeclaringType.Resolve().Methods.Add(method);
        }
    }
}