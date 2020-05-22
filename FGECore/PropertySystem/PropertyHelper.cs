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
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;
using FGECore.FileSystems;

namespace FGECore.PropertySystem
{
    /// <summary>
    /// Helper for the systems on a property.
    /// </summary>
    public abstract class PropertyHelper
    {
        /// <summary>
        /// A mapping of types to their property maps. Do note: if a type object is lost (Assembly is collected and dropped), the properties on that type are also lost.
        /// </summary>
        public static readonly ConditionalWeakTable<Type, PropertyHelper> PropertiesHelper = new ConditionalWeakTable<Type, PropertyHelper>();

        private static long CPropID = 1;

        /// <summary>
        /// Ensures a type is handled by the system, and returns the helper for the type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The helper for the type.</returns>
        public static PropertyHelper EnsureHandled(Type t)
        {
            if (PropertiesHelper.TryGetValue(t, out PropertyHelper helper))
            {
                return helper;
            }
            if (t.IsValueType) // TODO: Remove need for this check!!!
            {
                return EnsureHandled(typeof(object));
            }
            CPropID++;
            List<KeyValuePair<double, FieldInfo>> fdbg = new List<KeyValuePair<double, FieldInfo>>();
            List<KeyValuePair<double, FieldInfo>> fautosave = new List<KeyValuePair<double, FieldInfo>>();
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
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
                    fdbg.Add(new KeyValuePair<double, FieldInfo>(prio, fields[i]));
                }
                PropertyAutoSavable autosaveable = fields[i].GetCustomAttribute<PropertyAutoSavable>();
                if (autosaveable != null)
                {
                    fautosave.Add(new KeyValuePair<double, FieldInfo>(prio, fields[i]));
                }
            }
            List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>> fdbgm = new List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>>();
            List<KeyValuePair<double, KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>>> fsavm = new List<KeyValuePair<double, KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>>>();
            List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>> fvalm = new List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>>();
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < props.Length; i++)
            {
                if (!props[i].CanRead)
                {
                    continue;
                }
                PropertyPriority propPrio = props[i].GetCustomAttribute<PropertyPriority>();
                double prio = 0;
                if (propPrio != null)
                {
                    prio = propPrio.Priority;
                }
                PropertyDebuggable dbgable = props[i].GetCustomAttribute<PropertyDebuggable>();
                if (dbgable != null)
                {
                    fdbgm.Add(new KeyValuePair<double, KeyValuePair<string, MethodInfo>>(prio, new KeyValuePair<string, MethodInfo>(props[i].Name, props[i].GetMethod)));
                }
                PropertyAutoSavable savable = props[i].GetCustomAttribute<PropertyAutoSavable>();
                if (props[i].CanWrite && savable != null)
                {
                    fsavm.Add(new KeyValuePair<double, KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>>(prio, new KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>(props[i].Name, new KeyValuePair<MethodInfo, MethodInfo>(props[i].GetMethod, props[i].SetMethod))));
                }
                PropertyRequiredBool validifier = props[i].GetCustomAttribute<PropertyRequiredBool>();
                if (validifier != null && props[i].GetMethod.ReturnType == typeof(bool))
                {
                    fvalm.Add(new KeyValuePair<double, KeyValuePair<string, MethodInfo>>(prio, new KeyValuePair<string, MethodInfo>(props[i].Name, props[i].GetMethod)));
                }
            }
            fdbg = fdbg.OrderBy((k) => k.Key).ToList();
            fautosave = fautosave.OrderBy((k) => k.Key).ToList();
            fdbgm = fdbgm.OrderBy((k) => k.Key).ToList();
            fsavm = fsavm.OrderBy((k) => k.Key).ToList();
            fvalm = fvalm.OrderBy((k) => k.Key).ToList();
            string tid = "__FGE_Property_" + CPropID + "__" + t.Name + "__";
            AssemblyName asmn = new AssemblyName(tid);
            AssemblyBuilder asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(asmn, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder modb = asmb.DefineDynamicModule(tid);
            TypeBuilder typeb_c = modb.DefineType(tid + "__CENTRAL", TypeAttributes.Class | TypeAttributes.Public, typeof(PropertyHelper));
            MethodBuilder methodb_c = typeb_c.DefineMethod("GetDebuggableInfoOutputTyped", MethodAttributes.Public | MethodAttributes.Virtual);
            GenericTypeParameterBuilder mbc_a = methodb_c.DefineGenericParameters("T")[0];
            mbc_a.SetGenericParameterAttributes(GenericParameterAttributes.None);
            methodb_c.SetParameters(mbc_a, typeof(Dictionary<string, string>));
            methodb_c.SetReturnType(typeof(void));
            ILGenerator ilgen = methodb_c.GetILGenerator();
            Label next = default;
            for (int i = 0; i < fvalm.Count; i++)
            {
                if (i > 0)
                {
                    ilgen.MarkLabel(next);
                }
                next = ilgen.DefineLabel();
                ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
                //ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?
                ilgen.Emit(OpCodes.Call, fdbgm[i].Value.Value); // Call the method and load the method's return value (always a bool).
                ilgen.Emit(OpCodes.Brtrue, next); // if b is false:
                ilgen.Emit(OpCodes.Ret); // Return NOW!
            }
            if (fvalm.Count > 0)
            {
                ilgen.MarkLabel(next);
            }
            for (int i = 0; i < fdbg.Count; i++)
            {
                bool isClass = fdbg[i].Value.FieldType.IsClass;
                PropertyHelper pht = EnsureHandled(fdbg[i].Value.FieldType);
                bool is_handlable = pht.FieldsAutoSaveable.Count > 0 || pht.FieldsDebuggable.Count > 0 || pht.GetterMethodsDebuggable.Count > 0 || pht.GetterSetterSaveable.Count > 0;
                ilgen.Emit(OpCodes.Ldarg_2); // Load the 'vals' Dictionary.
                if (is_handlable)
                {
                    ilgen.Emit(OpCodes.Ldstr, fdbg[i].Value.FieldType.FullName + "(" + fdbg[i].Value.Name + ")"); // Load the field name and full type as a string.
                }
                else
                {
                    ilgen.Emit(OpCodes.Ldstr, fdbg[i].Value.FieldType.Name + "(" + fdbg[i].Value.Name + ")"); // Load the field name and type as a string.
                }
                ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
                //ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?
                ilgen.Emit(OpCodes.Ldfld, fdbg[i].Value); // Load the field's value.
                if (isClass) // If a class
                {
                    if (is_handlable)
                    {
                        // CODE: vals.Add("FType(fname)", StringifyDebuggable(p.fname));
                        ilgen.Emit(OpCodes.Call, Method_PropertyHelper_StringifyDebuggable); // Convert the field's value to a string.
                    }
                    else
                    {
                        // CODE: vals.Add("FType(fname)", Stringify(p.fname));
                        ilgen.Emit(OpCodes.Call, Method_PropertyHelper_Stringify); // Convert the field's value to a string.
                    }
                }
                else // if a struct
                {
                    if (is_handlable)
                    {
                        // CODE: vals.Add("FType(fname)", StringifyDebuggableStruct<FType>(p.fname));
                        MethodInfo structy = Method_PropertyHelper_StringifyDebuggableStruct.MakeGenericMethod(fdbg[i].Value.FieldType);
                        ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                    }
                    else
                    {
                        // CODE: vals.Add("FType(fname)", StringifyStruct<FType>(p.fname));
                        MethodInfo structy = Method_PropertyHelper_StringifyStruct.MakeGenericMethod(fdbg[i].Value.FieldType);
                        ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                    }
                }
                ilgen.Emit(OpCodes.Call, Method_DictionaryStringString_Add); // Call Dictionary<string, string>.Add(string, string).
            }
            for (int i = 0; i < fdbgm.Count; i++)
            {
                bool isClass = fdbgm[i].Value.Value.ReturnType.IsClass;
                PropertyHelper pht = EnsureHandled(fdbgm[i].Value.Value.ReturnType);
                bool is_handlable = pht.FieldsAutoSaveable.Count > 0 || pht.FieldsDebuggable.Count > 0 || pht.GetterMethodsDebuggable.Count > 0 || pht.GetterSetterSaveable.Count > 0;
                ilgen.Emit(OpCodes.Ldarg_2); // Load the 'vals' Dictionary.
                if (is_handlable)
                {
                    ilgen.Emit(OpCodes.Ldstr, fdbgm[i].Value.Value.ReturnType.FullName + "(" + fdbgm[i].Value.Key + ")"); // Load the field name and full return type as a string.
                }
                else
                {
                    ilgen.Emit(OpCodes.Ldstr, fdbgm[i].Value.Value.ReturnType.Name + "(" + fdbgm[i].Value.Key + ")"); // Load the method name and return type as a string.
                }
                ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
                //ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?
                ilgen.Emit(OpCodes.Call, fdbgm[i].Value.Value); // Call the method and load the method's return value.
                if (isClass) // If a class
                {
                    if (is_handlable)
                    {
                        // CODE: vals.Add("FType(fname)", StringifyDebuggable(p.fname));
                        ilgen.Emit(OpCodes.Call, Method_PropertyHelper_StringifyDebuggable); // Convert the field's value to a string.
                    }
                    else
                    {
                        // CODE: vals.Add("FType(fname)", Stringify(p.fname));
                        ilgen.Emit(OpCodes.Call, Method_PropertyHelper_Stringify); // Convert the field's value to a string.
                    }
                }
                else // if a struct
                {
                    if (is_handlable)
                    {
                        // CODE: vals.Add("FType(fname)", StringifyDebuggableStruct<FType>(p.fname));
                        MethodInfo structy = Method_PropertyHelper_StringifyDebuggableStruct.MakeGenericMethod(fdbgm[i].Value.Value.ReturnType);
                        ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                    }
                    else
                    {
                        // CODE: vals.Add("FType(fname)", StringifyStruct<FType>(p.fname));
                        MethodInfo structy = Method_PropertyHelper_StringifyStruct.MakeGenericMethod(fdbgm[i].Value.Value.ReturnType);
                        ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                    }
                }
                ilgen.Emit(OpCodes.Call, Method_DictionaryStringString_Add); // Call Dictionary<string, string>.Add(string, string).
            }
            ilgen.Emit(OpCodes.Ret);
            typeb_c.DefineMethodOverride(methodb_c, Method_PropertyHelper_GetDebuggableInfoOutputTyped);
            // Create a helper method that automatically type-casts.
            MethodBuilder methodb_c2 = typeb_c.DefineMethod("GetDebuggableInfoOutput", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new Type[] { typeof(Object), typeof(Dictionary<string, string>) });
            ILGenerator ilgen2 = methodb_c2.GetILGenerator();
            ilgen2.Emit(OpCodes.Ldarg_0);
            ilgen2.Emit(OpCodes.Ldarg_1);
            ilgen2.Emit(OpCodes.Ldarg_2);
            MethodInfo mxi = methodb_c.MakeGenericMethod(t);
            ilgen2.Emit(OpCodes.Call, mxi);
            ilgen2.Emit(OpCodes.Ret);
            typeb_c.DefineMethodOverride(methodb_c2, Method_PropertyHelper_GetDebuggableInfoOutput);
            Type res = typeb_c.CreateType();
            PropertyHelper ph = Activator.CreateInstance(res) as PropertyHelper;
            ph.PropertyType = t;
            ph.FieldsDebuggable.AddRange(fdbg);
            ph.FieldsAutoSaveable.AddRange(fautosave);
            ph.GetterMethodsDebuggable.AddRange(fdbgm);
            ph.GetterSetterSaveable.AddRange(fsavm);
            PropertiesHelper.Add(t, ph);
            return ph;
        }

        /// <summary>
        /// Saves the property's data to a DataWriter, appending its generated strings to a string list and lookup table.
        /// <para>Is not a compiled method (Meaning, this method is reflection-driven)!</para>
        /// </summary>
        /// <param name="p">The object to get the data from.</param>
        /// <param name="dw">Data writer to use.</param>
        /// <param name="strs">Strings to reference.</param>
        /// <param name="strMap">The string lookup table.</param>
        public void SaveNC(Object p, DataWriter dw, List<string> strs, Dictionary<string, int> strMap)
        {
            dw.WriteVarInt(FieldsAutoSaveable.Count);
            foreach (KeyValuePair<double, FieldInfo> saveme in FieldsAutoSaveable)
            {
                if (!strMap.TryGetValue(saveme.Value.Name, out int id))
                {
                    id = strs.Count;
                    strs.Add(saveme.Value.Name);
                    strMap[saveme.Value.Name] = id;
                }
                dw.WriteVarInt(id);
                PropertyHelper ph = EnsureHandled(saveme.Value.FieldType);
                if (ph != null && (ph.FieldsAutoSaveable.Count > 0 || ph.FieldsDebuggable.Count > 0 || ph.GetterMethodsDebuggable.Count > 0 || ph.GetterSetterSaveable.Count > 0))
                {
                    ph.SaveNC(saveme.Value.GetValue(p), dw, strs, strMap);
                }
                else if (PropertyHolder.TypeSavers.TryGetValue(saveme.Value.FieldType, out PropertySaverLoader psl))
                {
                    dw.WriteFullBytesVar(psl.Saver(saveme.Value.GetValue(p)));
                }
            }
            dw.WriteVarInt(GetterSetterSaveable.Count);
            foreach (KeyValuePair<double, KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>> saveme in GetterSetterSaveable)
            {
                if (!strMap.TryGetValue(saveme.Value.Key, out int id))
                {
                    id = strs.Count;
                    strs.Add(saveme.Value.Key);
                    strMap[saveme.Value.Key] = id;
                }
                dw.WriteVarInt(id);
                PropertyHelper ph = EnsureHandled(saveme.Value.Value.Key.ReturnType);
                if (ph != null && (ph.FieldsAutoSaveable.Count > 0 || ph.FieldsDebuggable.Count > 0 || ph.GetterMethodsDebuggable.Count > 0 || ph.GetterSetterSaveable.Count > 0))
                {
                    ph.SaveNC(saveme.Value.Value.Key.Invoke(p, NoObjects), dw, strs, strMap);
                }
                else if (PropertyHolder.TypeSavers.TryGetValue(saveme.Value.Value.Key.ReturnType, out PropertySaverLoader psl))
                {
                    dw.WriteFullBytesVar(psl.Saver(saveme.Value.Value.Key.Invoke(p, NoObjects)));
                }
            }
        }

        private static readonly Object[] NoObjects = new object[0];

        /// <summary>
        /// The <see cref="Object.ToString"/> method.
        /// </summary>
        public static readonly MethodInfo Method_Object_ToString = typeof(Object).GetMethod("ToString", new Type[0]);

        /// <summary>
        /// The <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/> method.
        /// </summary>
        public static readonly MethodInfo Method_DictionaryStringString_Add = typeof(Dictionary<string, string>).GetMethod("Add", new Type[] { typeof(string), typeof(string) });

        /// <summary>
        /// The <see cref="GetDebuggableInfoOutput"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_GetDebuggableInfoOutput = typeof(PropertyHelper).GetMethod("GetDebuggableInfoOutput");

        /// <summary>
        /// The <see cref="GetDebuggableInfoOutputTyped"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_GetDebuggableInfoOutputTyped = typeof(PropertyHelper).GetMethod("GetDebuggableInfoOutputTyped");

        /// <summary>
        /// The <see cref="StringifyDebuggable"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_StringifyDebuggable = typeof(PropertyHelper).GetMethod("StringifyDebuggable");

        /// <summary>
        /// The <see cref="StringifyDebuggableStruct"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_StringifyDebuggableStruct = typeof(PropertyHelper).GetMethod("StringifyDebuggableStruct");

        /// <summary>
        /// The <see cref="Stringify"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_Stringify = typeof(PropertyHelper).GetMethod("Stringify");

        /// <summary>
        /// The <see cref="StringifyStruct"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_StringifyStruct = typeof(PropertyHelper).GetMethod("StringifyStruct");

        /// <summary>
        /// Safely converts a debuggable object to a string.
        /// </summary>
        /// <param name="a">The object.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringifyDebuggable(Object a)
        {
            if (a == null)
            {
                return "null";
            }
            PropertyHelper ph = EnsureHandled(a.GetType());
            Dictionary<string, string> outp = new Dictionary<string, string>();
            ph.GetDebuggableInfoOutputTyped(a, outp);
            StringBuilder outpstr = new StringBuilder();
            outpstr.Append("{");
            foreach (KeyValuePair<string, string> oss in outp)
            {
                outpstr.Append(oss.Key + ": " + oss.Value + ", ");
            }
            if (outpstr.Length > 1)
            {
                outpstr.Length -= 2;
            }
            outpstr.Append("}");
            return outpstr.ToString();
        }

        /// <summary>
        /// Safely converts a debuggable struct to a string.
        /// </summary>
        /// <param name="a">The object.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringifyDebuggableStruct<T>(T a) where T : struct
        {
            PropertyHelper ph = EnsureHandled(typeof(T));
            Dictionary<string, string> outp = new Dictionary<string, string>();
            ph.GetDebuggableInfoOutputTyped(a, outp);
            StringBuilder outpstr = new StringBuilder();
            outpstr.Append("{");
            foreach (KeyValuePair<string, string> oss in outp)
            {
                outpstr.Append(oss.Key + ": " + oss.Value + ", ");
            }
            if (outpstr.Length > 1)
            {
                outpstr.Length -= 2;
            }
            outpstr.Append("}");
            return outpstr.ToString();
        }

        /// <summary>
        /// Safely converts a struct to a string.
        /// </summary>
        /// <param name="a">The struct.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringifyStruct<T>(T a) where T : struct
        {
            return a.ToString();
        }

        /// <summary>
        /// Safely converts an object to a string.
        /// </summary>
        /// <param name="a">The object.</param>
        /// <returns>The string, or "null".</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Stringify(Object a)
        {
            return a?.ToString() ?? "null";
        }

        // TODO: Auto read-to and assign-from a MapTag in FS wherever possible!

        /// <summary>
        /// Call this method to get debuggable information output added to a string dictionary.
        /// This method's implementation is dynamically generated.
        /// </summary>
        /// <param name="p">The property.</param>
        /// <param name="vals">The string dictionary.</param>
        public abstract void GetDebuggableInfoOutput(Object p, Dictionary<string, string> vals);

        /// <summary>
        /// Call this method to get debuggable information output added to a string dictionary.
        /// This method's implementation is dynamically generated.
        /// </summary>
        /// <param name="p">The property.</param>
        /// <param name="vals">The string dictionary.</param>
        public abstract void GetDebuggableInfoOutputTyped<T>(T p, Dictionary<string, string> vals);

        /// <summary>
        /// The type of the property to monitor.
        /// </summary>
        public Type PropertyType;

        /// <summary>
        /// A list of all getter methods that are debuggable.
        /// </summary>
        public readonly List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>> GetterMethodsDebuggable = new List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>>();

        /// <summary>
        /// A list of all getter/setter method pairs that are autao-saveable.
        /// </summary>
        public readonly List<KeyValuePair<double, KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>>> GetterSetterSaveable = new List<KeyValuePair<double, KeyValuePair<string, KeyValuePair<MethodInfo, MethodInfo>>>>();

        /// <summary>
        /// A list of all fields that are debuggable.
        /// </summary>
        public readonly List<KeyValuePair<double, FieldInfo>> FieldsDebuggable = new List<KeyValuePair<double, FieldInfo>>();

        /// <summary>
        /// A list of all fields that are auto-saveable.
        /// </summary>
        public readonly List<KeyValuePair<double, FieldInfo>> FieldsAutoSaveable = new List<KeyValuePair<double, FieldInfo>>();

        /// <summary>
        /// A list of all "validity check" getter methods.
        /// </summary>
        public readonly List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>> GetterMethodValidity = new List<KeyValuePair<double, KeyValuePair<string, MethodInfo>>>();
    }
}
