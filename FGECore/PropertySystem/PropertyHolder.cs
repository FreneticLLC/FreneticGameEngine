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
using BEPUutilities;
using FreneticUtilities.FreneticToolkit;
using FGECore.PhysicsSystem;
using FGECore.MathHelpers;

namespace FGECore.PropertySystem
{
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
        private readonly Dictionary<Type, Property> HeldProperties = new Dictionary<Type, Property>();

        /// <summary>
        /// All currently held interfaces on this object.
        /// </summary>
        private readonly Dictionary<Type, List<Object>> HeldInterfaces = new Dictionary<Type, List<Object>>();

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
}
