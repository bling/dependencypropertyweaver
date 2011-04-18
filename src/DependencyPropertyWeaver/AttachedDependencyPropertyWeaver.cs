using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

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
            var properties = from module in Definition.Modules
                             from type in module.Types
                             where string.IsNullOrEmpty(typePatternMatch) || Regex.IsMatch(type.Name, typePatternMatch)
                             from p in type.Properties
                             let set = p.SetMethod
                             let get = p.GetMethod
                             where (set != null && set.IsStatic) || (get != null && get.IsStatic)
                             where p.IsAutoPropertyGetter() || p.IsAutoPropertySetter()
                             select p;

            var types = properties.GroupBy(p => p.DeclaringType);
            Modify(types);
        }

        private void Modify(IEnumerable<IGrouping<TypeDefinition, PropertyDefinition>> types)
        {
            foreach (var t in types)
            {
                var staticCtor = GetStaticCtor(t.Key);
                foreach (var prop in t.ToArray())
                {
                    var field = GetStaticDependencyPropertyField(t.Key, prop.Name);
                    WeaveDependencyPropertyCtor(staticCtor.Body, field, prop);

                    AddGetterMethod(prop, field);
                    AddSetterMethod(prop, field);

                    var name = prop.Name;
                    var backingField = prop.DeclaringType.Fields.Single(f => f.Name.Contains("BackingField") && f.Name.Contains(name));
                    t.Key.Fields.Remove(backingField);

                    t.Key.Methods.Remove(prop.GetMethod);
                    t.Key.Methods.Remove(prop.SetMethod);
                    t.Key.Properties.Remove(prop);
                }
            }
        }

        private void WeaveDependencyPropertyCtor(MethodBody staticCtorBody, FieldReference field, PropertyDefinition property)
        {
            var assembly = property.DeclaringType.Module.Assembly;
            var propertyType = assembly.ImportType(Type.GetType(property.PropertyType.FullName));
            var getTypeFromHandle = assembly.ImportMethod(typeof(Type).GetMethod("GetTypeFromHandle"));
            var register = assembly.ImportMethod(typeof(DependencyProperty).GetMethod("RegisterAttached", new[] { typeof(string), typeof(Type), typeof(Type) }));

            // ignore previously weaved DPs
            if (staticCtorBody.Instructions.Any(i => i.Operand != null && i.Operand.ToString() == field.ToString()))
            {
                return;
            }

            var ret = staticCtorBody.Instructions.Last();
            if (ret.OpCode != OpCodes.Ret)
                throw new InvalidOperationException("The last instruction should be OpCode.Ret");

            HasChanges = true;

            var proc = staticCtorBody.GetILProcessor();
            proc.InsertBefore(ret, proc.Create(OpCodes.Ldstr, property.Name));
            proc.InsertBefore(ret, proc.Create(OpCodes.Ldtoken, propertyType));
            proc.InsertBefore(ret, proc.Create(OpCodes.Call, getTypeFromHandle));
            proc.InsertBefore(ret, proc.Create(OpCodes.Ldtoken, property.DeclaringType));
            proc.InsertBefore(ret, proc.Create(OpCodes.Call, getTypeFromHandle));
            proc.InsertBefore(ret, proc.Create(OpCodes.Call, register));
            proc.InsertBefore(ret, proc.Create(OpCodes.Stsfld, field));
        }

        private void AddGetterMethod(PropertyReference property, FieldReference field)
        {
            var method = new MethodDefinition("Get" + property.Name,
                                              MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig,
                                              property.PropertyType);

            method.Parameters.Add(new ParameterDefinition(Definition.ImportType<UIElement>()));

            var proc = method.Body.GetILProcessor();
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldsfld, field);
            proc.Emit(OpCodes.Callvirt, Definition.ImportMethod(typeof(DependencyObject).GetMethod("GetValue")));
            proc.Emit(property.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, property.PropertyType);
            proc.Emit(OpCodes.Ret);

            field.DeclaringType.Resolve().Methods.Add(method);
        }

        private void AddSetterMethod(PropertyReference property, FieldReference field)
        {
            var method = new MethodDefinition("Set" + property.Name,
                                              MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig,
                                              Definition.ImportType(Type.GetType("System.Void")));

            method.Parameters.Add(new ParameterDefinition(Definition.ImportType<UIElement>()));
            method.Parameters.Add(new ParameterDefinition(property.PropertyType));

            var proc = method.Body.GetILProcessor();
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldsfld, field);
            proc.Emit(OpCodes.Ldarg_1);
            if (property.PropertyType.IsValueType)
            {
                proc.Emit(OpCodes.Box, property.PropertyType);
            }
            proc.Emit(OpCodes.Callvirt, Definition.ImportMethod(typeof(DependencyObject).GetMethod("SetValue", new[] { typeof(DependencyProperty), typeof(object) })));
            proc.Emit(OpCodes.Ret);

            field.DeclaringType.Resolve().Methods.Add(method);
        }
    }
}