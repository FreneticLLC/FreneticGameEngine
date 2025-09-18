//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FGECore.FileSystems;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.PropertySystem;

/// <summary>Helper for the systems on a property.</summary>
public abstract class PropertyHelper
{
    /// <summary>
    /// A mapping of types to their property maps. Do note: if a type object is lost (Assembly is collected and dropped), the properties on that type are also lost.
    /// </summary>
    public static readonly ConditionalWeakTable<Type, PropertyHelper> PropertiesHelper = [];

    /// <summary>Lock to prevent a new helper from being initialized multiple times across multiple threads. Only the writeclaim is used, read claim is ignored for perf reasons.</summary>
    public static ManyReadOneWriteLock NewHelpersLock = new(1);

    /// <summary>Internal data useful to the <see cref="PropertyHelper"/> class - not meant for external access.</summary>
    public static class Internal
    {
        /// <summary>The ID of the last generated property ID.</summary>
        public static long CPropID = 1;

        /// <summary>A premade, reusable, empty array of <see cref="object"/>s.</summary>
        public static readonly object[] NoObjects = [];
    }

    /// <summary>Represents a field with a specific numeric priority.</summary>
    public class PrioritizedField(double _priority, FieldInfo _field)
    {
        /// <summary>The priority of the field.</summary>
        public double Priority = _priority;

        /// <summary>The field itself.</summary>
        public FieldInfo Field = _field;
    }

    /// <summary>Represents a C# property with a specific numeric priority.</summary>
    public class PrioritizedSharpProperty(double _priority, PropertyInfo _property)
    {
        /// <summary>The priority of the field.</summary>
        public double Priority = _priority;

        /// <summary>The C# property itself.</summary>
        public PropertyInfo SharpProperty = _property;
    }

    /// <summary>Ensures a type is handled by the system, and returns the helper for the type.</summary>
    /// <param name="propType">The type.</param>
    /// <returns>The helper for the type.</returns>
    public static PropertyHelper EnsureHandled(Type propType)
    {
        if (PropertiesHelper.TryGetValue(propType, out PropertyHelper helper))
        {
            return helper;
        }
        if (propType.IsValueType) // TODO: Remove need for this check!!!
        {
            return EnsureHandled(typeof(object));
        }
        using ManyReadOneWriteLock.WriteClaim claim = NewHelpersLock.LockWrite();
        if (PropertiesHelper.TryGetValue(propType, out helper)) // re-check inside lock
        {
            return helper;
        }
        Internal.CPropID++;
        List<PrioritizedField> fieldsDebuggable = [];
        List<PrioritizedField> fieldsAutoSaveable = [];
        FieldInfo[] fields = propType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            // Just in case!
            if (fields[i].IsStatic || !fields[i].IsPublic)
            {
                continue;
            }
            PropertyPriority propPrio = fields[i].GetCustomAttribute<PropertyPriority>();
            double prio = 0;
            if (propPrio != null)
            {
                prio = propPrio.Priority;
            }
            PropertyDebuggable dbgable = fields[i].GetCustomAttribute<PropertyDebuggable>();
            if (dbgable != null)
            {
                fieldsDebuggable.Add(new PrioritizedField(prio, fields[i]));
            }
            PropertyAutoSavable autosaveable = fields[i].GetCustomAttribute<PropertyAutoSavable>();
            if (autosaveable != null)
            {
                fieldsAutoSaveable.Add(new PrioritizedField(prio, fields[i]));
            }
        }
        List<PrioritizedSharpProperty> getterPropertiesDebuggable = [];
        List<PrioritizedSharpProperty> getterSetterPropertiesSaveable = [];
        List<PrioritizedSharpProperty> validityTestProperties = [];
        PropertyInfo[] sharpProperties = propType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo sharpProperty in sharpProperties)
        {
            if (!sharpProperty.CanRead)
            {
                continue;
            }
            PropertyPriority propPrio = sharpProperty.GetCustomAttribute<PropertyPriority>();
            double prio = 0;
            if (propPrio != null)
            {
                prio = propPrio.Priority;
            }
            PropertyDebuggable dbgable = sharpProperty.GetCustomAttribute<PropertyDebuggable>();
            if (dbgable != null)
            {
                getterPropertiesDebuggable.Add(new PrioritizedSharpProperty(prio, sharpProperty));
            }
            PropertyAutoSavable savable = sharpProperty.GetCustomAttribute<PropertyAutoSavable>();
            if (sharpProperty.CanWrite && savable != null)
            {
                getterSetterPropertiesSaveable.Add(new PrioritizedSharpProperty(prio, sharpProperty));
            }
            PropertyRequiredBool validifier = sharpProperty.GetCustomAttribute<PropertyRequiredBool>();
            if (validifier != null && sharpProperty.GetMethod.ReturnType == typeof(bool))
            {
                validityTestProperties.Add(new PrioritizedSharpProperty(prio, sharpProperty));
            }
        }
        fieldsDebuggable = [.. fieldsDebuggable.OrderBy((k) => k.Priority)];
        fieldsAutoSaveable = [.. fieldsAutoSaveable.OrderBy((k) => k.Priority)];
        getterPropertiesDebuggable = [.. getterPropertiesDebuggable.OrderBy((k) => k.Priority)];
        getterSetterPropertiesSaveable = [.. getterSetterPropertiesSaveable.OrderBy((k) => k.Priority)];
        validityTestProperties = [.. validityTestProperties.OrderBy((k) => k.Priority)];
        string newTypeID = $"__FGE_Property_{Internal.CPropID}__{propType.Name} __";
        AssemblyName asmName = new(newTypeID);
        AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(newTypeID);
        TypeBuilder generatedType = moduleBuilder.DefineType(newTypeID + "__CENTRAL", TypeAttributes.Class | TypeAttributes.Public, typeof(PropertyHelper));
        MethodBuilder debugTypedMethodBuilder = generatedType.DefineMethod("GetDebuggableInfoOutputTyped", MethodAttributes.Public | MethodAttributes.Virtual);
        GenericTypeParameterBuilder debugTypedGenericParam = debugTypedMethodBuilder.DefineGenericParameters("T")[0];
        debugTypedGenericParam.SetGenericParameterAttributes(GenericParameterAttributes.None);
        debugTypedMethodBuilder.SetParameters(debugTypedGenericParam, typeof(Dictionary<string, string>));
        debugTypedMethodBuilder.SetReturnType(typeof(void));
        ILGenerator ilgen = debugTypedMethodBuilder.GetILGenerator();
        Label nextLabel = default;
        for (int i = 0; i < validityTestProperties.Count; i++)
        {
            if (i > 0)
            {
                ilgen.MarkLabel(nextLabel);
            }
            nextLabel = ilgen.DefineLabel();
            ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
            //ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?
            ilgen.Emit(OpCodes.Call, validityTestProperties[i].SharpProperty.GetMethod); // Call the method and load the method's return value (always a bool).
            ilgen.Emit(OpCodes.Brtrue, nextLabel); // if b is false:
            ilgen.Emit(OpCodes.Ret); // Return NOW!
        }
        if (validityTestProperties.Count > 0)
        {
            ilgen.MarkLabel(nextLabel);
        }
        foreach (PrioritizedField prioFIeld in fieldsDebuggable)
        {
            FieldInfo field = prioFIeld.Field;
            bool isClass = field.FieldType.IsClass;
            PropertyHelper pht = EnsureHandled(field.FieldType);
            bool is_handlable = pht.FieldsAutoSaveable.Count > 0 || pht.FieldsDebuggable.Count > 0 || pht.GetterPropertiesDebuggable.Count > 0 || pht.GetterSetterSaveable.Count > 0;
            ilgen.Emit(OpCodes.Ldarg_2); // Load the 'vals' Dictionary.
            if (is_handlable)
            {
                ilgen.Emit(OpCodes.Ldstr, field.FieldType.FullName + "(" + field.Name + ")"); // Load the field name and full type as a string.
            }
            else
            {
                ilgen.Emit(OpCodes.Ldstr, field.FieldType.Name + "(" + field.Name + ")"); // Load the field name and type as a string.
            }
            ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
            //ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?
            ilgen.Emit(OpCodes.Ldfld, field); // Load the field's value.
            if (isClass) // If a class
            {
                if (is_handlable)
                {
                    // CODE: vals.Add("FType(fname)", StringifyDebuggable(p.fname));
                    ilgen.Emit(OpCodes.Call, ReflectedMethods.StringifyDebuggable); // Convert the field's value to a string.
                }
                else
                {
                    // CODE: vals.Add("FType(fname)", Stringify(p.fname));
                    ilgen.Emit(OpCodes.Call, ReflectedMethods.Stringify); // Convert the field's value to a string.
                }
            }
            else // if a struct
            {
                if (is_handlable)
                {
                    // CODE: vals.Add("FType(fname)", StringifyDebuggableStruct<FType>(p.fname));
                    MethodInfo structy = ReflectedMethods.StringifyDebuggableStruct.MakeGenericMethod(field.FieldType);
                    ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                }
                else
                {
                    // CODE: vals.Add("FType(fname)", StringifyStruct<FType>(p.fname));
                    MethodInfo structy = ReflectedMethods.StringifyStruct.MakeGenericMethod(field.FieldType);
                    ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                }
            }
            ilgen.Emit(OpCodes.Call, ReflectedMethods.DictionaryStringString_Add); // Call Dictionary<string, string>.Add(string, string).
        }
        foreach (PrioritizedSharpProperty prioSharpProperty in getterPropertiesDebuggable)
        {
            PropertyInfo sharpProperty = prioSharpProperty.SharpProperty;
            bool isClass = sharpProperty.GetMethod.ReturnType.IsClass;
            PropertyHelper pht = EnsureHandled(sharpProperty.GetMethod.ReturnType);
            bool is_handlable = pht.FieldsAutoSaveable.Count > 0 || pht.FieldsDebuggable.Count > 0 || pht.GetterPropertiesDebuggable.Count > 0 || pht.GetterSetterSaveable.Count > 0;
            ilgen.Emit(OpCodes.Ldarg_2); // Load the 'vals' Dictionary.
            if (is_handlable)
            {
                ilgen.Emit(OpCodes.Ldstr, sharpProperty.GetMethod.ReturnType.FullName + "(" + sharpProperty.Name + ")"); // Load the field name and full return type as a string.
            }
            else
            {
                ilgen.Emit(OpCodes.Ldstr, sharpProperty.GetMethod.ReturnType.Name + "(" + sharpProperty.Name + ")"); // Load the method name and return type as a string.
            }
            ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
            //ilgen.Emit(OpCodes.Castclass, propType); // Cast 'p' to the correct property type. // TODO: Necessity?
            ilgen.Emit(OpCodes.Call, sharpProperty.GetMethod); // Call the method and load the method's return value.
            if (isClass) // If a class
            {
                if (is_handlable)
                {
                    // CODE: vals.Add("FType(fname)", StringifyDebuggable(p.fname));
                    ilgen.Emit(OpCodes.Call, ReflectedMethods.StringifyDebuggable); // Convert the field's value to a string.
                }
                else
                {
                    // CODE: vals.Add("FType(fname)", Stringify(p.fname));
                    ilgen.Emit(OpCodes.Call, ReflectedMethods.Stringify); // Convert the field's value to a string.
                }
            }
            else // if a struct
            {
                if (is_handlable)
                {
                    // CODE: vals.Add("FType(fname)", StringifyDebuggableStruct<FType>(p.fname));
                    MethodInfo structy = ReflectedMethods.StringifyDebuggableStruct.MakeGenericMethod(sharpProperty.GetMethod.ReturnType);
                    ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                }
                else
                {
                    // CODE: vals.Add("FType(fname)", StringifyStruct<FType>(p.fname));
                    MethodInfo structy = ReflectedMethods.StringifyStruct.MakeGenericMethod(sharpProperty.GetMethod.ReturnType);
                    ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                }
            }
            ilgen.Emit(OpCodes.Call, ReflectedMethods.DictionaryStringString_Add); // Call Dictionary<string, string>.Add(string, string).
        }
        ilgen.Emit(OpCodes.Ret);
        generatedType.DefineMethodOverride(debugTypedMethodBuilder, ReflectedMethods.PropertyHelper_GetDebuggableInfoOutputTyped);
        // Create a helper method that automatically type-casts.
        MethodBuilder debugOutMethodBuilder = generatedType.DefineMethod("GetDebuggableInfoOutput", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), [typeof(object), typeof(Dictionary<string, string>)]);
        ILGenerator ilgen2 = debugOutMethodBuilder.GetILGenerator();
        ilgen2.Emit(OpCodes.Ldarg_0);
        ilgen2.Emit(OpCodes.Ldarg_1);
        ilgen2.Emit(OpCodes.Ldarg_2);
        MethodInfo mxi = debugTypedMethodBuilder.MakeGenericMethod(propType);
        ilgen2.Emit(OpCodes.Call, mxi);
        ilgen2.Emit(OpCodes.Ret);
        generatedType.DefineMethodOverride(debugOutMethodBuilder, ReflectedMethods.PropertyHelper_GetDebuggableInfoOutput);
        Type res = generatedType.CreateType();
        PropertyHelper propHolder = Activator.CreateInstance(res) as PropertyHelper;
        propHolder.PropertyType = propType;
        propHolder.FieldsDebuggable.AddRange(fieldsDebuggable);
        propHolder.FieldsAutoSaveable.AddRange(fieldsAutoSaveable);
        propHolder.GetterPropertiesDebuggable.AddRange(getterPropertiesDebuggable);
        propHolder.GetterSetterSaveable.AddRange(getterSetterPropertiesSaveable);
        propHolder.ValidityTestGetterProperties.AddRange(validityTestProperties);
        PropertiesHelper.Add(propType, propHolder);
        return propHolder;
    }

    /// <summary>
    /// Saves the property's data to a DataWriter, appending its generated strings to a string list and lookup table.
    /// <para>Is not a compiled method (Meaning, this method is reflection-driven)!</para>
    /// </summary>
    /// <param name="propertyObject">The object to get the data from.</param>
    /// <param name="outputWriter">Data writer to use.</param>
    /// <param name="strs">Strings to reference.</param>
    /// <param name="strMap">The string lookup table.</param>
    /// <returns>True if saved successfully, false if saving was not allowed.</returns>
    public bool SaveNC(object propertyObject, DataWriter outputWriter, List<string> strs, Dictionary<string, int> strMap)
    {
        foreach (PrioritizedSharpProperty testMe in ValidityTestGetterProperties)
        {
            if (!((bool)testMe.SharpProperty.GetMethod.Invoke(propertyObject, Internal.NoObjects)))
            {
                return false;
            }
        }
        outputWriter.WriteVarInt(FieldsAutoSaveable.Count);
        foreach (PrioritizedField saveme in FieldsAutoSaveable)
        {
            if (!strMap.TryGetValue(saveme.Field.Name, out int id))
            {
                id = strs.Count;
                strs.Add(saveme.Field.Name);
                strMap[saveme.Field.Name] = id;
            }
            outputWriter.WriteVarInt(id);
            PropertyHelper ph = EnsureHandled(saveme.Field.FieldType);
            if (ph != null && (ph.FieldsAutoSaveable.Count > 0 || ph.FieldsDebuggable.Count > 0 || ph.GetterPropertiesDebuggable.Count > 0 || ph.GetterSetterSaveable.Count > 0))
            {
                ph.SaveNC(saveme.Field.GetValue(propertyObject), outputWriter, strs, strMap);
            }
            else if (PropertySaveSystem.TypeSavers.TryGetValue(saveme.Field.FieldType, out PropertySaverLoader psl))
            {
                outputWriter.WriteFullBytesVar(psl.Saver(saveme.Field.GetValue(propertyObject)));
            }
        }
        outputWriter.WriteVarInt(GetterSetterSaveable.Count);
        foreach (PrioritizedSharpProperty saveme in GetterSetterSaveable)
        {
            if (!strMap.TryGetValue(saveme.SharpProperty.Name, out int id))
            {
                id = strs.Count;
                strs.Add(saveme.SharpProperty.Name);
                strMap[saveme.SharpProperty.Name] = id;
            }
            outputWriter.WriteVarInt(id);
            PropertyHelper ph = EnsureHandled(saveme.SharpProperty.GetMethod.ReturnType);
            if (ph != null && (ph.FieldsAutoSaveable.Count > 0 || ph.FieldsDebuggable.Count > 0 || ph.GetterPropertiesDebuggable.Count > 0 || ph.GetterSetterSaveable.Count > 0))
            {
                ph.SaveNC(saveme.SharpProperty.GetMethod.Invoke(propertyObject, Internal.NoObjects), outputWriter, strs, strMap);
            }
            else if (PropertySaveSystem.TypeSavers.TryGetValue(saveme.SharpProperty.GetMethod.ReturnType, out PropertySaverLoader psl))
            {
                outputWriter.WriteFullBytesVar(psl.Saver(saveme.SharpProperty.GetMethod.Invoke(propertyObject, Internal.NoObjects)));
            }
        }
        return true;
    }

    /// <summary>A class containing reflected method references, for code generation usage.</summary>
    public static class ReflectedMethods
    {
        /// <summary>The <see cref="object.ToString"/> method.</summary>
        public static readonly MethodInfo Object_ToString = typeof(object).GetMethod(nameof(object.ToString), []);

        /// <summary>The <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/> method.</summary>
        public static readonly MethodInfo DictionaryStringString_Add = typeof(Dictionary<string, string>).GetMethod(nameof(Dictionary<string, string>.Add), [typeof(string), typeof(string)]);

        /// <summary>The <see cref="GetDebuggableInfoOutput"/> method.</summary>
        public static readonly MethodInfo PropertyHelper_GetDebuggableInfoOutput = typeof(PropertyHelper).GetMethod(nameof(GetDebuggableInfoOutput));

        /// <summary>The <see cref="GetDebuggableInfoOutputTyped"/> method.</summary>
        public static readonly MethodInfo PropertyHelper_GetDebuggableInfoOutputTyped = typeof(PropertyHelper).GetMethod(nameof(GetDebuggableInfoOutputTyped));

        /// <summary>The <see cref="GeneratedCodeHelperMethods.StringifyDebuggable"/> method.</summary>
        public static readonly MethodInfo StringifyDebuggable = typeof(GeneratedCodeHelperMethods).GetMethod(nameof(GeneratedCodeHelperMethods.StringifyDebuggable));

        /// <summary>The <see cref="GeneratedCodeHelperMethods.StringifyDebuggableStruct"/> method.</summary>
        public static readonly MethodInfo StringifyDebuggableStruct = typeof(GeneratedCodeHelperMethods).GetMethod(nameof(GeneratedCodeHelperMethods.StringifyDebuggableStruct));

        /// <summary>The <see cref="GeneratedCodeHelperMethods.Stringify"/> method.</summary>
        public static readonly MethodInfo Stringify = typeof(GeneratedCodeHelperMethods).GetMethod(nameof(GeneratedCodeHelperMethods.Stringify));

        /// <summary>The <see cref="GeneratedCodeHelperMethods.StringifyStruct"/> method.</summary>
        public static readonly MethodInfo StringifyStruct = typeof(GeneratedCodeHelperMethods).GetMethod(nameof(GeneratedCodeHelperMethods.StringifyStruct));
    }

    /// <summary>A class with a few methods to help the generated code work.</summary>
    public static class GeneratedCodeHelperMethods
    {
        /// <summary>Safely converts a debuggable object to a string.</summary>
        /// <param name="propObj">The object.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringifyDebuggable(object propObj)
        {
            if (propObj == null)
            {
                return "null";
            }
            PropertyHelper propHelper = EnsureHandled(propObj.GetType());
            Dictionary<string, string> debugInfo = [];
            propHelper.GetDebuggableInfoOutputTyped(propObj, debugInfo);
            StringBuilder output = new();
            output.Append('{');
            foreach (KeyValuePair<string, string> infoVal in debugInfo)
            {
                output.Append(infoVal.Key).Append(": ").Append(infoVal.Value).Append(", ");
            }
            if (output.Length > 1)
            {
                output.Length -= 2;
            }
            output.Append('}');
            return output.ToString();
        }

        /// <summary>Safely converts a debuggable struct to a string.</summary>
        /// <param name="propObj">The object.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringifyDebuggableStruct<T>(T propObj) where T : struct
        {
            PropertyHelper propHelper = EnsureHandled(typeof(T));
            Dictionary<string, string> debugInfo = [];
            propHelper.GetDebuggableInfoOutputTyped(propObj, debugInfo);
            StringBuilder output = new();
            output.Append('{');
            foreach (KeyValuePair<string, string> infoVal in debugInfo)
            {
                output.Append(infoVal.Key).Append(": ").Append(infoVal.Value).Append(", ");
            }
            if (output.Length > 1)
            {
                output.Length -= 2;
            }
            output.Append('}');
            return output.ToString();
        }

        /// <summary>Safely converts a struct to a string.</summary>
        /// <param name="structInst">The struct.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringifyStruct<T>(T structInst) where T : struct
        {
            return structInst.ToString();
        }

        /// <summary>Safely converts an object to a string.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>The string, or "null".</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Stringify(object obj)
        {
            return obj?.ToString() ?? "null";
        }
    }

    // TODO: Auto read-to and assign-from a MapTag in FS wherever possible!

    /// <summary>
    /// Call this method to get debuggable information output added to a string dictionary.
    /// This method's implementation is dynamically generated.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="vals">The string dictionary.</param>
    public abstract void GetDebuggableInfoOutput(Object property, Dictionary<string, string> vals);

    /// <summary>
    /// Call this method to get debuggable information output added to a string dictionary.
    /// This method's implementation is dynamically generated.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="vals">The string dictionary.</param>
    public abstract void GetDebuggableInfoOutputTyped<T>(T property, Dictionary<string, string> vals);

    /// <summary>The type of the property to monitor.</summary>
    public Type PropertyType;

    /// <summary>A list of all getter methods that are debuggable.</summary>
    public readonly List<PrioritizedSharpProperty> GetterPropertiesDebuggable = [];

    /// <summary>A list of all getter/setter method pairs that are autao-saveable.</summary>
    public readonly List<PrioritizedSharpProperty> GetterSetterSaveable = [];

    /// <summary>A list of all fields that are debuggable.</summary>
    public readonly List<PrioritizedField> FieldsDebuggable = [];

    /// <summary>A list of all fields that are auto-saveable.</summary>
    public readonly List<PrioritizedField> FieldsAutoSaveable = [];

    /// <summary>A list of all "validity check" getter methods.</summary>
    public readonly List<PrioritizedSharpProperty> ValidityTestGetterProperties = [];
}
