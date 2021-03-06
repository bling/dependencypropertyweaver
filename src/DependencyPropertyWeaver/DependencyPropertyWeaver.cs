﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DependencyPropertyWeaver
{
    public class DependencyPropertyWeaver : AssemblyWeaverBase
    {
        public DependencyPropertyWeaver(Assembly assembly, AssemblyDefinition definition)
            : base(assembly, definition)
        {
        }

        public override void Weave(string typePatternMatch, string attributePatternMatch)
        {
            var properties = from module in Definition.Modules
                             from type in module.Types
                             where string.IsNullOrEmpty(typePatternMatch) ||
                                   Regex.IsMatch(type.Name, typePatternMatch)
                             where string.IsNullOrEmpty(attributePatternMatch) ||
                                   type.CustomAttributes.Any(a => Regex.IsMatch(a.AttributeType.FullName, attributePatternMatch))
                             from p in type.Properties
                             let method = p.SetMethod ?? p.GetMethod
                             where !method.IsStatic
                             where string.IsNullOrEmpty(attributePatternMatch) ||
                                   p.CustomAttributes.Any(a => Regex.IsMatch(a.AttributeType.FullName, attributePatternMatch))
                             select p;

            var types = properties.GroupBy(p => p.DeclaringType);
            Modify(types);
        }

        private void Modify(IEnumerable<IGrouping<TypeDefinition, PropertyDefinition>> types)
        {
            foreach (var t in types)
            {
                WeaveDependencyObjectBaseClass(t.Key);
                WeaveProperties(t);
            }
        }

        private void WeaveDependencyObjectBaseClass(TypeDefinition type)
        {
            while (type.BaseType.FullName != "System.Object")
            {
                type = type.BaseType.Resolve();
                if (type.FullName == "System.Windows.DependencyObject")
                    return;
            }

            type.BaseType = Definition.ImportType<DependencyObject>();

            // replace base ctor call to dependency object
            var ctor = type.Methods.Single(m => m.Name == ".ctor");
            var instruction = ctor.Body.Instructions
                .Where(i => i.OpCode == OpCodes.Call)
                .Single(x => x.Operand.ToString() == "System.Void System.Object::.ctor()");

            instruction.Operand = Definition.ImportMethod(typeof(DependencyObject).GetConstructors()[0]);
        }

        private void WeaveProperties(IEnumerable<PropertyDefinition> properties)
        {
            foreach (var prop in properties)
            {
                if (!prop.IsAutoPropertySetter())
                    continue;

                if (!prop.IsAutoPropertyGetter())
                    continue;

                var staticCtor = GetStaticCtor(prop.DeclaringType);

                string propName = prop.Name;
                var ff = prop.DeclaringType.Fields.Single(f => f.Name.Contains("BackingField") && f.Name.Contains(propName));
                prop.DeclaringType.Fields.Remove(ff);

                var field = GetStaticDependencyPropertyField(prop.DeclaringType, propName);
                WeaveDependencyProperty(staticCtor.Body, field, prop);
                WeaveGetter(prop);
                WeaveSetter(prop);
            }
        }

        private void WeaveDependencyProperty(MethodBody staticCtorBody, FieldReference field, PropertyDefinition property)
        {
            var assembly = property.DeclaringType.Module.Assembly;
            var propertyType = assembly.ImportType(Type.GetType(property.PropertyType.FullName));
            var getTypeFromHandle = assembly.ImportMethod(typeof(Type).GetMethod("GetTypeFromHandle"));
            var register = assembly.ImportMethod(typeof(DependencyProperty).GetMethod("Register", new[] { typeof(string), typeof(Type), typeof(Type) }));

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

        private void WeaveGetter(PropertyDefinition property)
        {
            var body = property.GetMethod.Body;
            var proc = body.GetILProcessor();
            var field = property.DeclaringType.Fields.Single(f => f.Name == property.Name + "DependencyProperty");
            var getValue = Definition.ImportMethod(typeof(DependencyObject).GetMethod("GetValue"));

            body.Instructions.Clear();

            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldsfld, field);
            proc.Emit(OpCodes.Call, getValue);
            proc.Emit(property.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, property.PropertyType);
            proc.Emit(OpCodes.Ret);
        }

        private void WeaveSetter(PropertyDefinition property)
        {
            var body = property.SetMethod.Body;
            var proc = body.GetILProcessor();
            var field = property.DeclaringType.Fields.Single(f => f.Name == property.Name + "DependencyProperty");
            var setValue = Definition.ImportMethod(typeof(DependencyObject).GetMethod("SetValue", new[] { typeof(DependencyProperty), typeof(object) }));

            body.Instructions.Clear();

            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldsfld, field);
            proc.Emit(OpCodes.Ldarg_1);
            if (property.PropertyType.IsValueType)
            {
                proc.Emit(OpCodes.Box, property.PropertyType);
            }
            proc.Emit(OpCodes.Call, setValue);
            proc.Emit(OpCodes.Ret);
        }
    }
}