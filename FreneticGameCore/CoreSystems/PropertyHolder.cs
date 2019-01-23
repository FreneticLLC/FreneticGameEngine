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
using BEPUutilities;
using FreneticGameCore.EntitySystem.PhysicsHelpers;
using FreneticGameCore.Files;
using FreneticGameCore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;
using FreneticGameCore.PhysicsSystem;
using FreneticGameCore.MathHelpers;

namespace FreneticGameCore.CoreSystems
{
    /// <summary>
    /// Represents a helper to save or load an object.
    /// </summary>
    public class PropertySaverLoader
    {
        /// <summary>
        /// The save name.
        /// </summary>
        public string SaveString;
        
        /// <summary>
        /// The save method.
        /// </summary>
        public Func<Object, byte[]> Saver;

        /// <summary>
        /// The load method.
        /// </summary>
        public Func<byte[], Object> Loader;
    }

    /// <summary>
    /// Holds an uncapped set of properties.
    /// </summary>
    public class PropertyHolder
    {
        /// <summary>
        /// All type saver methods.
        /// </summary>
        public static Dictionary<Type, PropertySaverLoader> TypeSavers = new Dictionary<Type, PropertySaverLoader>(1024);

        /// <summary>
        /// All type loader methods.
        /// </summary>
        public static Dictionary<string, PropertySaverLoader> TypeLoaders = new Dictionary<string, PropertySaverLoader>(1024);

        /// <summary>
        /// Ensures initialization.
        /// </summary>
        static PropertyHolder()
        {
            EnsureInit();
        }

        /// <summary>
        /// Whether the system is already inited.
        /// </summary>
        private static bool Initted = false;

        /// <summary>
        /// Configures the default set of savers and readers for the FGE core.
        /// </summary>
        public static void EnsureInit()
        {
            if (Initted)
            {
                return;
            }
            Initted = true;
            // Core Helpers
            TypeSavers.Add(typeof(bool), new PropertySaverLoader()
            {
                Saver = (o) => new byte[] { ((bool)o) ? (byte)1 : (byte)0 },
                Loader = (b) => b[0] != 0 ? true : false,
                SaveString = "C/bool"
            });
            TypeSavers.Add(typeof(byte), new PropertySaverLoader()
            {
                Saver = (o) => new byte[] { (byte)o },
                Loader = (b) => b[0],
                SaveString = "C/byte"
            });
            TypeSavers.Add(typeof(sbyte), new PropertySaverLoader()
            {
                Saver = (o) => new byte[] { unchecked((byte)((sbyte)o)) },
                Loader = (b) => unchecked((sbyte)(b[0])),
                SaveString = "C/sbyte"
            });
            TypeSavers.Add(typeof(ushort), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((ushort)o),
                Loader = (b) => BitConverter.ToUInt16(b, 0),
                SaveString = "C/ushort"
            });
            TypeSavers.Add(typeof(short), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((short)o),
                Loader = (b) => BitConverter.ToInt16(b, 0),
                SaveString = "C/short"
            });
            TypeSavers.Add(typeof(uint), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((uint)o),
                Loader = (b) => BitConverter.ToUInt32(b, 0),
                SaveString = "C/uint"
            });
            TypeSavers.Add(typeof(int), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((int)o),
                Loader = (b) => BitConverter.ToInt32(b, 0),
                SaveString = "C/int"
            });
            TypeSavers.Add(typeof(ulong), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((ulong)o),
                Loader = (b) => BitConverter.ToUInt64(b, 0),
                SaveString = "C/ulong"
            });
            TypeSavers.Add(typeof(long), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((long)o),
                Loader = (b) => BitConverter.ToInt64(b, 0),
                SaveString = "C/long"
            });
            TypeSavers.Add(typeof(float), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((float)o),
                Loader = (b) => BitConverter.ToSingle(b, 0),
                SaveString = "C/float"
            });
            TypeSavers.Add(typeof(double), new PropertySaverLoader()
            {
                Saver = (o) => BitConverter.GetBytes((double)o),
                Loader = (b) => BitConverter.ToDouble(b, 0),
                SaveString = "C/double"
            });
            TypeSavers.Add(typeof(string), new PropertySaverLoader()
            {
                Saver = (o) => StringConversionHelper.UTF8Encoding.GetBytes(o as string),
                Loader = (b) => StringConversionHelper.UTF8Encoding.GetString(b),
                SaveString = "C/string"
            });
            // FGE/Core Helpers
            TypeSavers.Add(typeof(Location), new PropertySaverLoader()
            {
                Saver = (o) => ((Location)o).ToDoubleBytes(),
                Loader = (b) => Location.FromDoubleBytes(b, 0),
                SaveString = "C/location"
            });
            TypeSavers.Add(typeof(Color3F), new PropertySaverLoader()
            {
                Saver = (o) => ((Color3F)o).ToBytes(),
                Loader = (b) => Color3F.FromBytes(b),
                SaveString = "C/color3f"
            });
            TypeSavers.Add(typeof(Color4F), new PropertySaverLoader()
            {
                Saver = (o) => ((Color4F)o).ToBytes(),
                Loader = (b) => Color4F.FromBytes(b),
                SaveString = "C/color4f"
            });
            TypeSavers.Add(typeof(MathHelpers.Quaternion), new PropertySaverLoader()
            {
                Saver = (o) => ((MathHelpers.Quaternion)o).ToDoubleBytes(),
                Loader = (b) => MathHelpers.Quaternion.FromDoubleBytes(b, 0),
                SaveString = "C/quaternion"
            });
            // BEPU Helpers
            TypeSavers.Add(typeof(Vector3), new PropertySaverLoader()
            {
                Saver = (o) => (new Location((Vector3)o)).ToDoubleBytes(),
                Loader = (b) => Location.FromDoubleBytes(b, 0).ToBVector(),
                SaveString = "C/B/vector3"
            });
            TypeSavers.Add(typeof(BEPUutilities.Quaternion), new PropertySaverLoader()
            {
                Saver = (o) => BepuUtilities.QuaternionToBytes((BEPUutilities.Quaternion)o),
                Loader = (b) => BepuUtilities.BytesToQuaternion(b, 0),
                SaveString = "C/B/quaternion"
            });
            // End default helpers
            foreach (PropertySaverLoader psl in TypeSavers.Values)
            {
                TypeLoaders.Add(psl.SaveString, psl);
            }
        }

        /// <summary>
        /// All currently held properties on this object.
        /// </summary>
        private Dictionary<Type, Property> HeldProperties = new Dictionary<Type, Property>();

        /// <summary>
        /// All currently held interfaces on this object.
        /// </summary>
        private Dictionary<Type, List<Object>> HeldInterfaces = new Dictionary<Type, List<Object>>();

        /// <summary>
        /// Special helper: Default empty list for some returns.
        /// </summary>
        private static readonly IReadOnlyList<Object> DefaultReturnEmptyList = new List<Object>();

        /// <summary>
        /// Gets the first property with a specific interface.
        /// Returns null when nothing is found.
        /// </summary>
        /// <param name="t">The type of the interface.</param>
        /// <returns>The first found object, or null.</returns>
        public Object GetFirstInterfacedProperty(Type t)
        {
            if (HeldInterfaces.TryGetValue(t, out List<Object> objs) && objs.Count > 0)
            {
                return objs[0];
            }
            return null;
        }

        /// <summary>
        /// Gets the first property with a specific interface.
        /// Returns null when nothing is found.
        /// </summary>
        /// <typeparam name="T">The type of the interface.</typeparam>
        /// <returns>The first found object, or null.</returns>
        public T GetFirstInterfacedProperty<T>() where T: class
        {
            if (HeldInterfaces.TryGetValue(typeof(T), out List<Object> objs) && objs.Count > 0)
            {
                return objs[0] as T;
            }
            return null;
        }

        /// <summary>
        /// Gets all properties with a specific interface.
        /// Note that this is faster but less clean than <see cref="GetAllInterfacedProperties{T}"/>.
        /// Good for foreach loops. Bad for when you need a typed list.
        /// Returns an empty list when nothing is found.
        /// </summary>
        /// <param name="t">The type of the interface.</param>
        /// <returns>All the objects.</returns>
        public IReadOnlyList<Object> GetAllInterfacedProperties(Type t)
        {
            if (HeldInterfaces.TryGetValue(t, out List<Object> objs))
            {
                return objs;
            }
            return DefaultReturnEmptyList;
        }

        /// <summary>
        /// Gets all properties with a specific interface.
        /// Note that this is slower but cleaner than <see cref="GetAllInterfacedProperties(Type)"/>.
        /// Good for when you need a typed list. Bad for foreach loops.
        /// Returns an empty list when nothing is found.
        /// </summary>
        /// <typeparam name="T">The type of the interface.</typeparam>
        /// <returns>All the objects.</returns>
        public IReadOnlyList<T> GetAllInterfacedProperties<T>()
        {
            if (HeldInterfaces.TryGetValue(typeof(T), out List<Object> objs))
            {
                return objs.Cast<T>().ToList();
            }
            return new List<T>();
        }

        /// <summary>
        /// Sends a signal to all properties with a specific interface.
        /// </summary>
        /// <typeparam name="T">The type of the interface.</typeparam>
        /// <param name="signal">The signal to send.</param>
        public void SignalAllInterfacedProperties<T>(Action<T> signal)
        {
            if (!HeldInterfaces.TryGetValue(typeof(T), out List<Object> objs))
            {
                return;
            }
            foreach (T obj in objs)
            {
                signal(obj);
            }
        }

        /// <summary>
        /// Gets all currently held property types.
        /// </summary>
        /// <returns>The set of property types.</returns>
        public IEnumerable<Type> EnumerateAllPropertyTypes()
        {
            return HeldProperties.Keys;
        }

        /// <summary>
        /// Gets all currently held properties.
        /// </summary>
        /// <returns>The set of properties.</returns>
        public IEnumerable<Property> EnumerateAllProperties()
        {
            return HeldProperties.Values;
        }
        
        /// <summary>
        /// Gets all currently held property types in a safe copied container.
        /// <para>Generally, prefer <see cref="EnumerateAllPropertyTypes"/>.</para>
        /// </summary>
        /// <returns>The set of property types.</returns>
        public List<Type> GetAllPropertyTypes()
        {
            return new List<Type>(HeldProperties.Keys);
        }

        /// <summary>
        /// Gets all currently held properties in a safe copied container.
        /// <para>Generally, prefer <see cref="EnumerateAllProperties"/>.</para>
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
        /// Gets the first property that is a sub-type of the given property type.
        /// <para>This method is likely slower than its generic version! Prefer to use <see cref="GetFirstSubType{T}"/> when possible.</para>
        /// Returns null if none found.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The property, or null.</returns>
        public Property GetFirstSubType(Type t)
        {
            foreach (KeyValuePair<Type, Property> p in HeldProperties)
            {
                if (t.IsAssignableFrom(p.Key))
                {
                    return p.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the first property that is a sub-type of the given property type.
        /// Returns null if none found.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <returns>The property, or null.</returns>
        public T GetFirstSubType<T>() where T : Property
        {
            foreach (Property p in HeldProperties.Values)
            {
                if (p is T a)
                {
                    return a;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets all properties that are a sub-type of the given property type.
        /// <para>This method is likely slower than its generic version!</para>
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The set of properties.</returns>
        public IEnumerable<Property> GetAllSubTypes(Type t)
        {
            foreach (KeyValuePair<Type, Property> p in HeldProperties)
            {
                if (t.IsAssignableFrom(p.Key))
                {
                    yield return p.Value;
                }
            }
        }

        /// <summary>
        /// Gets all properties that are a sub-type of the given property type.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <returns>The property set.</returns>
        public IEnumerable<T> GetAllSubTypes<T>() where T: Property
        {
            foreach (Property p in HeldProperties.Values)
            {
                if (p is T a)
                {
                    yield return a;
                }
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
        /// Internal: forget a property from this property holder.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="p">The property of relevance.</param>
        private void ForgetProperty(Type t, Property p)
        {
            HeldProperties.Remove(t);
            foreach (Type iface in t.GetInterfaces())
            {
                if (HeldInterfaces.TryGetValue(iface, out List<Object> objs))
                {
                    objs.Remove(p);
                }
            }
            p.OnRemoved();
            OnRemoved(p);
            p.Holder = null;
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
                ForgetProperty(t, p);
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
                ForgetProperty(typeof(T), p);
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
        /// Runs a set of code on a property, if the property is present.
        /// </summary>
        /// <param name="t">The property type.</param>
        /// <param name="logic">The logic to run.</param>
        public void InvokeIfPresent(Type t, Action<Property> logic)
        {
            if (HeldProperties.TryGetValue(t, out Property p))
            {
                logic(p);
            }
        }

        /// <summary>
        /// Runs a set of code on a property, if the property is present.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="logic">The logic to run.</param>
        public void InvokeIfPresent<T>(Action<T> logic) where T: Property
        {
            if (HeldProperties.TryGetValue(typeof(T), out Property p))
            {
                logic(p as T);
            }
        }

        /// <summary>
        /// Gets the property by type, or gives an exception.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The property.</returns>
        public Property GetProperty(Type t)
        {
            if (HeldProperties.TryGetValue(t, out Property prop))
            {
                return prop;
            }
            throw new ArgumentOutOfRangeException("Cannot find property of type: " + t.Name + ", but was required for object " + this);
        }

        /// <summary>
        /// Gets the property (with a typeparam-specified type), or gives an exception.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <returns>The property.</returns>
        public T GetProperty<T>() where T : Property
        {
            if (HeldProperties.TryGetValue(typeof(T), out Property prop))
            {
                return prop as T;
            }
            throw new ArgumentOutOfRangeException("Cannot find property of type: " + typeof(T).Name + ", but was required for object " + this);
        }

        /// <summary>
        /// Adds several properties in one go.
        /// <para>Does no special sequencing, just adds them all in given order.</para>
        /// </summary>
        /// <param name="props">The properties.</param>
        public void AddProperties(params Property[] props)
        {
            foreach (Property p in props)
            {
                AddProperty(p);
            }
        }

        /// <summary>
        /// Internal: Notice a property (called when a property is added).
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="p">The property.</param>
        private void NoticeProperty(Type t, Property p)
        {
            p.Holder = this;
            p.Helper = PropertyHelper.EnsureHandled(t);
            foreach (Type iface in t.GetInterfaces())
            {
                if (HeldInterfaces.TryGetValue(iface, out List<Object> objs))
                {
                    objs.Add(p);
                }
                else
                {
                    HeldInterfaces[iface] = new List<Object>() { p };
                }
            }
            p.OnAdded();
            OnAdded(p);
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
            HeldProperties.Add(t, prop);
            NoticeProperty(t, prop);
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
            HeldProperties[t] = res;
            NoticeProperty(t, res);
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
            HeldProperties[typeof(T)] = res;
            NoticeProperty(typeof(T), res);
            return res;
        }

        /// <summary>
        /// Called when a property is added.
        /// </summary>
        /// <param name="prop">The property.</param>
        public virtual void OnAdded(Property prop)
        {
        }

        /// <summary>
        /// Called when a property is removed.
        /// </summary>
        /// <param name="prop">The property.</param>
        public virtual void OnRemoved(Property prop)
        {
        }
    }

    /// <summary>
    /// Used to indicate that a property field is debuggable (if not marked, the property field is not debuggable).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyDebuggable : Attribute
    {
    }

    /// <summary>
    /// Used to indicate that a property field is auto-saveable (if not marked, the property field is not auto-saveable).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyAutoSavable : Attribute
    {
    }

    /// <summary>
    /// Used to indicate that a property's C# property must be tested before a property or object is included in a property save or debug (will expect a boolean C# property, not a field).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyRequiredBool : Attribute
    {
    }

    /// <summary>
    /// Used to indicate that the numerical priority (order of usage, lowest = first, highest = last) a property should be handled in.
    /// <para>Note that fields always come before property methods.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyPriority : Attribute
    {
        /// <summary>
        /// The priority.
        /// </summary>
        public double Priority;

        /// <summary>
        /// Construct the priority.
        /// </summary>
        /// <param name="_prio">The priority value.</param>
        public PropertyPriority(double _prio)
        {
            Priority = _prio;
        }
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
            Label next = default(Label);
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
        public static string StringifyDebuggableStruct<T>(T a) where T: struct
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
