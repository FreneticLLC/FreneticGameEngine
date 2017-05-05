using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticGameCore
{
    /// <summary>
    /// Holds an uncapped set of properties.
    /// </summary>
    public class PropertyHolder
    {
        private Dictionary<Type, Property> HeldProperties = new Dictionary<Type, Property>();

        /// <summary>
        /// Gets all currently held property types in a safe copied container.
        /// </summary>
        /// <returns>The set of property types.</returns>
        public List<Type> GetAllPropertyTypes()
        {
            return new List<Type>(HeldProperties.Keys);
        }

        /// <summary>
        /// Gets all currently held properties in a safe copied container.
        /// </summary>
        /// <returns>The set of properties.</returns>
        public List<Property> GetAllProperties()
        {
            return new List<Property>(HeldProperties.Values);
        }

        /// <summary>
        /// Returns the number of properties held by this holder.
        /// </summary>
        public int PropertyCount
        {
            get
            {
                return HeldProperties.Count;
            }
        }

        /// <summary>
        /// Checks whether a property of a specified type is held.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>Whether it was removed.</returns>
        public bool HasProperty(Type t)
        {
            return HeldProperties.ContainsKey(t);
        }

        /// <summary>
        /// Checks whether a property of a specified type is held.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>Whether it is held.</returns>
        public bool HasProperty<T>() where T : Property
        {
            return HeldProperties.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Removes the property by type, returning whether it was removed.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>Whether it was removed.</returns>
        public bool RemoveProperty(Type t)
        {
            if (HeldProperties.TryGetValue(t, out Property p))
            {
                p.Holder = null;
                HeldProperties.Remove(t);
                p.OnRemoved();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the property by type, returning whether it was removed.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>Whether it was removed.</returns>
        public bool RemoveProperty<T>() where T : Property
        {
            if (HeldProperties.TryGetValue(typeof(T), out Property p))
            {
                p.Holder = null;
                HeldProperties.Remove(typeof(T));
                p.OnRemoved();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the property by type, or returns false.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="p">The property result.</param>
        /// <returns>The property.</returns>
        public bool TryGetProperty(Type t, out Property p)
        {
            return HeldProperties.TryGetValue(t, out p);
        }

        /// <summary>
        /// Gets the property (with a typeparam-specified type), or returns false.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="p">The property result.</param>
        /// <returns>The property.</returns>
        public bool TryGetProperty<T>(out T p) where T : Property
        {
            if (HeldProperties.TryGetValue(typeof(T), out Property prop))
            {
                p = prop as T;
                return true;
            }
            p = null;
            return false;
        }

        /// <summary>
        /// Gets the property by type, or gives an exception.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The property.</returns>
        public Property GetProperty(Type t)
        {
            return HeldProperties[t];
        }

        /// <summary>
        /// Gets the property (with a typeparam-specified type), or gives an exception.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <returns>The property.</returns>
        public T GetProperty<T>() where T : Property
        {
            return HeldProperties[typeof(T)] as T;
        }

        /// <summary>
        /// Adds the property, or gives an exception if a property of matching type already exists.
        /// </summary>
        /// <param name="prop">The property itself.</param>
        /// <returns>The property.</returns>
        public void AddProperty(Property prop)
        {
            if (prop.Holder != null)
            {
                throw new InvalidOperationException("That property is already held by something!");
            }
            Type t = prop.GetType();
            prop.Holder = this;
            prop.Helper = PropertyHelper.EnsureHandled(t);
            HeldProperties.Add(t, prop);
            prop.OnAdded();
        }

        // Note: Intentionally discard this signature:
        // --> public void AddProperty<T>(T prop) where T : Property
        // Because it can cause wrong type to be used!

        /// <summary>
        /// Gets the property (with a generic type), or adds the property with the specified property constructor.
        /// May still throw an exception, if the property is held elsewhere!
        /// </summary>
        /// <param name="t">The property type.</param>
        /// <param name="constructor">The property constructor.</param>
        /// <returns>The property.</returns>
        public Property GetOrAddProperty(Type t, Func<Property> constructor)
        {
            if (HeldProperties.TryGetValue(t, out Property p))
            {
                return p;
            }
            Property res = constructor();
            if (res.Holder != null)
            {
                throw new InvalidOperationException("That property is already held by something!");
            }
            res.Holder = this;
            res.Helper = PropertyHelper.EnsureHandled(t);
            HeldProperties[t] = res;
            res.OnAdded();
            return res;
        }

        /// <summary>
        /// Gets the property (with a generic type), or adds the property with the specified property constructor.
        /// <para>May still throw an exception, if the property is held elsewhere!</para>
        /// <para>Be careful with this, as it can lead to incorrect typing if the Func input has an incorrect type!</para>
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <returns>The property.</returns>
        public T GetOrAddProperty<T>(Func<T> constructor) where T : Property
        {
            if (HeldProperties.TryGetValue(typeof(T), out Property p))
            {
                return p as T;
            }
            T res = constructor();
            if (res.Holder != null)
            {
                throw new InvalidOperationException("That property is already held by something!");
            }
            res.Holder = this;
            res.Helper = PropertyHelper.EnsureHandled(typeof(T));
            HeldProperties[typeof(T)] = res;
            res.OnAdded();
            return res;
        }
    }

    /// <summary>
    /// Used to indicate that a property field is debuggable (if not marked, the property field is not debuggable).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PropertyDebuggable : Attribute
    {
    }

    /// <summary>
    /// Used to indicate that a property field is auto-saveable (if not marked, the property field is not auto-saveable).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PropertyAutoSaveable : Attribute
    {
    }

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
            if (!typeof(Property).IsAssignableFrom(t))
            {
                throw new Exception("Trying to handle a type that isn't a property!");
            }
            CPropID++;
            List<FieldInfo> fdbg = new List<FieldInfo>();
            List<FieldInfo> fautosave = new List<FieldInfo>();
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                // Just in case!
                if (fields[i].IsStatic || !fields[i].IsPublic)
                {
                    continue;
                }
                PropertyDebuggable dbgable = fields[i].GetCustomAttribute<PropertyDebuggable>();
                if (dbgable != null)
                {
                    fdbg.Add(fields[i]);
                }
                PropertyAutoSaveable autosaveable = fields[i].GetCustomAttribute<PropertyAutoSaveable>();
                if (autosaveable != null)
                {
                    fautosave.Add(fields[i]);
                }
            }
            List<KeyValuePair<string, MethodInfo>> fdbgm = new List<KeyValuePair<string, MethodInfo>>();
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < props.Length; i++)
            {
                if (!props[i].CanRead)
                {
                    continue;
                }
                PropertyDebuggable dbgable = props[i].GetCustomAttribute<PropertyDebuggable>();
                if (dbgable != null)
                {
                    fdbgm.Add(new KeyValuePair<string, MethodInfo>(props[i].Name, props[i].GetMethod));
                }
            }
            string tid = "__FGE_Property_" + CPropID + "__" + t.Name + "__";
            AssemblyName asmn = new AssemblyName(tid);
            AssemblyBuilder asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(asmn, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder modb = asmb.DefineDynamicModule(tid);
            TypeBuilder typeb_c = modb.DefineType(tid + "__CENTRAL", TypeAttributes.Class | TypeAttributes.Public, typeof(PropertyHelper));
            MethodBuilder methodb_c = typeb_c.DefineMethod("GetDebuggableInfoOutput", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new Type[] { typeof(Property), typeof(Dictionary<string, string>) });
            ILGenerator ilgen = methodb_c.GetILGenerator();
            for (int i = 0; i < fdbg.Count; i++)
            {
                bool isClass = fdbg[i].FieldType.IsClass;
                ilgen.Emit(OpCodes.Ldarg_2); // Load the 'vals' Dictionary.
                ilgen.Emit(OpCodes.Ldstr, fdbg[i].FieldType.Name + "(" + fdbg[i].Name + ")"); // Load the field name as a string.
                ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
                ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?
                ilgen.Emit(OpCodes.Ldfld, fdbg[i]); // Load the field's value.
                if (isClass) // If a class
                {
                    // CODE: vals.Add("FType(fname)", Stringify(p.fname));
                    ilgen.Emit(OpCodes.Call, Method_PropertyHelper_Stringify); // Convert the field's value to a string.
                }
                else // if a struct
                {
                    // CODE: vals.Add("FType(fname)", StringifyStruct<FType>(p.fname));
                    MethodInfo structy = Method_PropertyHelper_StringifyStruct.MakeGenericMethod(fdbg[i].FieldType);
                    ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                }
                ilgen.Emit(OpCodes.Call, Method_DictionaryStringString_Add); // Call Dictionary<string, string>.Add(string, string).
            }
            for (int i = 0; i < fdbgm.Count; i++)
            {
                bool isClass = fdbgm[i].Value.ReturnType.IsClass;
                ilgen.Emit(OpCodes.Ldarg_2); // Load the 'vals' Dictionary.
                ilgen.Emit(OpCodes.Ldstr, fdbgm[i].Value.ReturnType.Name + "(" + fdbgm[i].Key + ")"); // Load the method name as a string.
                ilgen.Emit(OpCodes.Ldarg_1); // Load the 'p' Property.
                ilgen.Emit(OpCodes.Castclass, t); // Cast 'p' to the correct property type. // TODO: Necessity?)
                ilgen.Emit(OpCodes.Call, fdbgm[i].Value); // Call the method and load the method's return value.
                if (isClass) // If a class
                {
                    // CODE: vals.Add("FType(fname)", Stringify(p.mname()));
                    ilgen.Emit(OpCodes.Call, Method_PropertyHelper_Stringify); // Convert the field's value to a string.
                }
                else // if a struct
                {
                    // CODE: vals.Add("FType(fname)", StringifyStruct<FType>(p.mname()));
                    MethodInfo structy = Method_PropertyHelper_StringifyStruct.MakeGenericMethod(fdbgm[i].Value.ReturnType);
                    ilgen.Emit(OpCodes.Call, structy); // Convert the field's value to a string.
                }
                ilgen.Emit(OpCodes.Call, Method_DictionaryStringString_Add); // Call Dictionary<string, string>.Add(string, string).
            }
            ilgen.Emit(OpCodes.Ret);
            typeb_c.DefineMethodOverride(methodb_c, Method_PropertyHelper_GetDebuggableInfoOutput);
            Type res = typeb_c.CreateType();
            PropertyHelper ph = Activator.CreateInstance(res) as PropertyHelper;
            ph.PropertyType = t;
            ph.FieldsDebuggable.AddRange(fdbg);
            ph.FieldsAutoSaveable.AddRange(fautosave);
            ph.GetterMethodsDebuggable.AddRange(fdbgm);
            PropertiesHelper.Add(t, ph);
            return ph;
        }

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
        /// The <see cref="Stringify"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_Stringify = typeof(PropertyHelper).GetMethod("Stringify");

        /// <summary>
        /// The <see cref="StringifyStruct"/> method.
        /// </summary>
        public static readonly MethodInfo Method_PropertyHelper_StringifyStruct = typeof(PropertyHelper).GetMethod("StringifyStruct");

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
        /// </summary>
        /// <param name="p">The property.</param>
        /// <param name="vals">The string dictionary.</param>
        public abstract void GetDebuggableInfoOutput(Property p, Dictionary<string, string> vals);

        /// <summary>
        /// The type of the property to monitor.
        /// </summary>
        public Type PropertyType;

        /// <summary>
        /// A list of all getter methods that are debuggable.
        /// </summary>
        public readonly List<KeyValuePair<string, MethodInfo>> GetterMethodsDebuggable = new List<KeyValuePair<string, MethodInfo>>();

        /// <summary>
        /// A list of all fields that are debuggable.
        /// </summary>
        public readonly List<FieldInfo> FieldsDebuggable = new List<FieldInfo>();

        /// <summary>
        /// A list of all fields that are auto-saveable.
        /// </summary>
        public readonly List<FieldInfo> FieldsAutoSaveable = new List<FieldInfo>();
    }

    /// <summary>
    /// Represents a set of custom data attached to an object.
    /// </summary>
    public abstract class Property
    {
        /// <summary>
        /// The holder of this property. Modifying this value could lead to errors!
        /// </summary>
        public PropertyHolder Holder = null;

        /// <summary>
        /// The system that helps this property's field information.
        /// </summary>
        public PropertyHelper Helper = null;

        /// <summary>
        /// Gets the debug output for this property.
        /// </summary>
        /// <returns>The debuggable data.</returns>
        public Dictionary<string, string> GetDebuggable()
        {
            Dictionary<string, string> strs = new Dictionary<string, string>();
            Helper.GetDebuggableInfoOutput(this, strs);
            return strs;
        }

        /// <summary>
        /// Returns whether this property is currently held by something.
        /// </summary>
        /// <returns>Whether it is held.</returns>
        public bool IsHeld()
        {
            return Holder != null;
        }

        /// <summary>
        /// Returns whether this property is currently held by something, and outputs the holder if so (otherwise, outputs null).
        /// </summary>
        /// <param name="outholder">The holder output.</param>
        /// <returns>Whether it is held.</returns>
        public bool IsHeld(out PropertyHolder outholder)
        {
            return (outholder = Holder) != null;
        }

        /// <summary>
        /// This will return the best available type name for the current property: either the property classname, or a custom specified name given by the property definition.
        /// </summary>
        /// <returns>The property type name.</returns>
        public virtual string GetPropertyName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// This will return a clean (no 'holder' value) duplicate of the property.
        /// This is NOT guaranteed to be a deep copy (but should be where possible): defaults to a shallow copy!
        /// </summary>
        public virtual Property DuplicateClean()
        {
            Property p = MemberwiseClone() as Property;
            p.Holder = null;
            return p;
        }

        /// <summary>
        /// This is fired when the property is added to a system.
        /// </summary>
        public virtual void OnAdded()
        {
            // Do nothing by default.
        }

        /// <summary>
        /// This is fired when the property is removed from a system.
        /// </summary>
        public virtual void OnRemoved()
        {
            // Do nothing by default.
        }

        /// <summary>
        /// Gets a string-ified version of this property.
        /// </summary>
        /// <returns>The property string.</returns>
        public override string ToString()
        {
            return "Property<" + GetPropertyName() + ">";
        }
    }
}
