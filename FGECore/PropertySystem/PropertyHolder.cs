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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.PropertySystem;

/// <summary>Holds an uncapped set of properties.</summary>
public class PropertyHolder
{
    /// <summary>Internal data useful to <see cref="PropertyHolder"/> instances.</summary>
    public struct Internal
    {
        /// <summary>All currently held properties on this object.</summary>
        public Dictionary<Type, Property> HeldProperties;

        /// <summary>All currently held interfaces on this object.</summary>
        public Dictionary<Type, List<object>> HeldInterfaces;

        /// <summary>Special helper: Default empty list for some returns.</summary>
        public static readonly IReadOnlyList<object> DefaultReturnEmptyList = new List<object>();

        /// <summary>Notice a property (called when a property is added).</summary>
        /// <param name="holder">The holder instance.</param>
        /// <param name="type">The property class type.</param>
        /// <param name="propObj">The property object instance.</param>
        public readonly void NoticeProperty(PropertyHolder holder, Type type, Property propObj)
        {
            propObj.Holder = holder;
            propObj.Helper = PropertyHelper.EnsureHandled(type);
            foreach (Type iface in type.GetInterfaces())
            {
                if (HeldInterfaces.TryGetValue(iface, out List<Object> objs))
                {
                    objs.Add(propObj);
                }
                else
                {
                    HeldInterfaces[iface] = [propObj];
                }
            }
            propObj.OnAdded();
            holder.OnAdded(propObj);
        }

        /// <summary>forget a property from this property holder.</summary>
        /// <param name="holder">The holder instance.</param>
        /// <param name="type">The property class type.</param>
        /// <param name="prop">The property of relevance.</param>
        public readonly void ForgetProperty(PropertyHolder holder, Type type, Property prop)
        {
            HeldProperties.Remove(type);
            foreach (Type iface in type.GetInterfaces())
            {
                if (HeldInterfaces.TryGetValue(iface, out List<Object> objs))
                {
                    objs.Remove(prop);
                }
            }
            prop.OnRemoved();
            holder.OnRemoved(prop);
            prop.Holder = null;
        }
    }

    /// <summary>Internal data useful to <see cref="PropertyHolder"/> instances.</summary>
    public Internal PropertyInternals = new()
    {
        HeldProperties = [],
        HeldInterfaces = [],
    };

    /// <summary>
    /// Gets the first property with a specific interface.
    /// <para>Returns null when nothing is found.</para>
    /// </summary>
    /// <param name="type">The type of the interface.</param>
    /// <returns>The first found object, or null.</returns>
    public object GetFirstInterfacedProperty(Type type)
    {
        if (PropertyInternals.HeldInterfaces.TryGetValue(type, out List<object> objs) && objs.Count > 0)
        {
            return objs[0];
        }
        return null;
    }

    /// <summary>
    /// Gets the first property with a specific interface.
    /// <para>Returns null when nothing is found.</para>
    /// </summary>
    /// <typeparam name="T">The type of the interface.</typeparam>
    /// <returns>The first found object, or null.</returns>
    public T GetFirstInterfacedProperty<T>() where T: class
    {
        if (PropertyInternals.HeldInterfaces.TryGetValue(typeof(T), out List<object> objs) && objs.Count > 0)
        {
            return objs[0] as T;
        }
        return null;
    }

    /// <summary>
    /// Gets all properties with a specific interface.
    /// <para>Note that this is faster but less clean than <see cref="GetAllInterfacedProperties{T}"/>.</para>
    /// <para>Good for foreach loops. Bad for when you need a typed list.</para>
    /// <para>Returns an empty list when nothing is found.</para>
    /// </summary>
    /// <param name="t">The type of the interface.</param>
    /// <returns>All the objects.</returns>
    public IReadOnlyList<object> GetAllInterfacedProperties(Type t)
    {
        if (PropertyInternals.HeldInterfaces.TryGetValue(t, out List<object> objs))
        {
            return objs;
        }
        return Internal.DefaultReturnEmptyList;
    }

    /// <summary>
    /// Gets all properties with a specific interface.
    /// <para>Note that this is slower but cleaner than <see cref="GetAllInterfacedProperties(Type)"/>.</para>
    /// <para>Good for when you need a typed list. Bad for foreach loops.</para>
    /// <para>Returns an empty list when nothing is found.</para>
    /// </summary>
    /// <typeparam name="T">The type of the interface.</typeparam>
    /// <returns>All the objects.</returns>
    public IReadOnlyList<T> GetAllInterfacedProperties<T>()
    {
        if (PropertyInternals.HeldInterfaces.TryGetValue(typeof(T), out List<object> objs))
        {
            return objs.Cast<T>().ToList();
        }
        return new List<T>();
    }

    /// <summary>Sends a signal to all properties with a specific interface.</summary>
    /// <typeparam name="T">The type of the interface.</typeparam>
    /// <param name="signal">The signal to send.</param>
    public void SignalAllInterfacedProperties<T>(Action<T> signal) where T : class
    {
        if (!PropertyInternals.HeldInterfaces.TryGetValue(typeof(T), out List<object> objs))
        {
            return;
        }
        foreach (object obj in objs)
        {
            signal(obj as T);
        }
    }

    /// <summary>Gets all currently held property types.</summary>
    /// <returns>The set of property types.</returns>
    public IEnumerable<Type> EnumerateAllPropertyTypes()
    {
        return PropertyInternals.HeldProperties.Keys;
    }

    /// <summary>Gets all currently held properties.</summary>
    /// <returns>The set of properties.</returns>
    public IEnumerable<Property> EnumerateAllProperties()
    {
        return PropertyInternals.HeldProperties.Values;
    }

    /// <summary>
    /// Gets all currently held property types in a safe copied container.
    /// <para>Generally, prefer <see cref="EnumerateAllPropertyTypes"/>.</para>
    /// </summary>
    /// <returns>The set of property types.</returns>
    public List<Type> GetAllPropertyTypes()
    {
        return new List<Type>(PropertyInternals.HeldProperties.Keys);
    }

    /// <summary>
    /// Gets all currently held properties in a safe copied container.
    /// <para>Generally, prefer <see cref="EnumerateAllProperties"/>.</para>
    /// </summary>
    /// <returns>The set of properties.</returns>
    public List<Property> GetAllProperties()
    {
        return new List<Property>(PropertyInternals.HeldProperties.Values);
    }

    /// <summary>Returns the number of properties held by this holder.</summary>
    public int PropertyCount
    {
        get
        {
            return PropertyInternals.HeldProperties.Count;
        }
    }

    /// <summary>
    /// Gets the first property that is a sub-type of the given property type.
    /// <para>This method is likely slower than its generic version! Prefer to use <see cref="GetFirstSubType{T}"/> when possible.</para>
    /// <para>Returns null if none found.</para>
    /// </summary>
    /// <param name="type">The property class type.</param>
    /// <returns>The property, or null.</returns>
    public Property GetFirstSubType(Type type)
    {
        foreach (KeyValuePair<Type, Property> prop in PropertyInternals.HeldProperties)
        {
            if (type.IsAssignableFrom(prop.Key))
            {
                return prop.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the first property that is a sub-type of the given property type.
    /// <para>Returns null if none found.</para>
    /// </summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <returns>The property, or null.</returns>
    public T GetFirstSubType<T>() where T : Property
    {
        foreach (Property prop in PropertyInternals.HeldProperties.Values)
        {
            if (prop is T typedProp)
            {
                return typedProp;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all properties that are a sub-type of the given property type.
    /// <para>This method is slower than its generic version!</para>
    /// </summary>
    /// <param name="type">The property class type.</param>
    /// <returns>The set of properties.</returns>
    public IEnumerable<Property> GetAllSubTypes(Type type)
    {
        foreach (KeyValuePair<Type, Property> typedPropPair in PropertyInternals.HeldProperties)
        {
            if (type.IsAssignableFrom(typedPropPair.Key))
            {
                yield return typedPropPair.Value;
            }
        }
    }

    /// <summary>Gets all properties that are a sub-type of the given property type.</summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <returns>The property set.</returns>
    public IEnumerable<T> GetAllSubTypes<T>() where T: Property
    {
        foreach (Property prop in PropertyInternals.HeldProperties.Values)
        {
            if (prop is T typedProp)
            {
                yield return typedProp;
            }
        }
    }

    /// <summary>Checks whether a property of a specified type is held.</summary>
    /// <param name="type">The property class type.</param>
    /// <returns>Whether it was removed.</returns>
    public bool HasProperty(Type type)
    {
        return PropertyInternals.HeldProperties.ContainsKey(type);
    }

    /// <summary>Checks whether a property of a specified type is held.</summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <returns>Whether it is held.</returns>
    public bool HasProperty<T>() where T : Property
    {
        return PropertyInternals.HeldProperties.ContainsKey(typeof(T));
    }

    /// <summary>Removes the property by type, returning whether it was removed.</summary>
    /// <param name="type">The property class type.</param>
    /// <returns>Whether it was removed.</returns>
    public bool RemoveProperty(Type type)
    {
        if (PropertyInternals.HeldProperties.TryGetValue(type, out Property prop))
        {
            PropertyInternals.ForgetProperty(this, type, prop);
            return true;
        }
        return false;
    }

    /// <summary>Removes the property by type, returning whether it was removed.</summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <returns>Whether it was removed.</returns>
    public bool RemoveProperty<T>() where T : Property
    {
        if (PropertyInternals.HeldProperties.TryGetValue(typeof(T), out Property prop))
        {
            PropertyInternals.ForgetProperty(this, typeof(T), prop);
            return true;
        }
        return false;
    }

    /// <summary>Gets the property by type, or returns false.</summary>
    /// <param name="type">The property class type.</param>
    /// <param name="outputProperty">The property result.</param>
    /// <returns>The property.</returns>
    public bool TryGetProperty(Type type, out Property outputProperty)
    {
        return PropertyInternals.HeldProperties.TryGetValue(type, out outputProperty);
    }

    /// <summary>Gets the property (with a typeparam-specified type), or returns false.</summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <param name="outputProperty">The property result.</param>
    /// <returns>The property.</returns>
    public bool TryGetProperty<T>(out T outputProperty) where T : Property
    {
        if (PropertyInternals.HeldProperties.TryGetValue(typeof(T), out Property prop))
        {
            outputProperty = prop as T;
            return true;
        }
        outputProperty = null;
        return false;
    }

    /// <summary>Runs a set of code on a property, if the property is present.</summary>
    /// <param name="type">The property class type.</param>
    /// <param name="logic">The logic to run.</param>
    public void InvokeIfPresent(Type type, Action<Property> logic)
    {
        if (PropertyInternals.HeldProperties.TryGetValue(type, out Property p))
        {
            logic(p);
        }
    }

    /// <summary>Runs a set of code on a property, if the property is present.</summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <param name="logic">The logic to run.</param>
    public void InvokeIfPresent<T>(Action<T> logic) where T: Property
    {
        if (PropertyInternals.HeldProperties.TryGetValue(typeof(T), out Property p))
        {
            logic(p as T);
        }
    }

    /// <summary>Gets the property by type, or gives an exception.</summary>
    /// <param name="type">The property class type.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the property does not exist on the object.</exception>
    /// <returns>The property.</returns>
    public Property GetProperty(Type type)
    {
        if (PropertyInternals.HeldProperties.TryGetValue(type, out Property prop))
        {
            return prop;
        }
        throw new ArgumentOutOfRangeException("Cannot find property of type: " + type.Name + ", but was required for object " + this);
    }

    /// <summary>Gets the property (with a typeparam-specified type), or gives an exception.</summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">If the property does not exist on the object.</exception>
    /// <returns>The property.</returns>
    public T GetProperty<T>() where T : Property
    {
        if (PropertyInternals.HeldProperties.TryGetValue(typeof(T), out Property prop))
        {
            return prop as T;
        }
        throw new ArgumentOutOfRangeException("Cannot find property of type: " + typeof(T).Name + ", but was required for object " + this);
    }

    /// <summary>
    /// Adds several properties in one go.
    /// <para>Does no special sequencing, just adds them all in given order.</para>
    /// </summary>
    /// <param name="propertiesToAdd">The properties.</param>
    public void AddProperties(params Property[] propertiesToAdd)
    {
        foreach (Property prop in propertiesToAdd)
        {
            AddProperty(prop);
        }
    }

    /// <summary>Adds the property, or gives an exception if a property of matching type already exists.</summary>
    /// <param name="prop">The property itself.</param>
    /// <exception cref="InvalidOperationException">If the property is already held by a different object.</exception>
    /// <returns>The property.</returns>
    public void AddProperty(Property prop)
    {
        if (prop.Holder != null)
        {
            throw new InvalidOperationException("That property is already held by something!");
        }
        Type t = prop.GetType();
        PropertyInternals.HeldProperties.Add(t, prop);
        PropertyInternals.NoticeProperty(this, t, prop);
    }

    // Note: Intentionally discard this signature:
    // --> public void AddProperty<T>(T prop) where T : Property
    // Because it can cause wrong type to be used!

    /// <summary>
    /// Gets the property (with a generic type), or adds the property with the specified property constructor.
    /// <para>May still throw an exception, if the property is held elsewhere!</para>
    /// </summary>
    /// <param name="propType">The property class type.</param>
    /// <param name="constructor">The property constructor.</param>
    /// <exception cref="InvalidOperationException">If the property is already held by a different object.</exception>
    /// <returns>The property.</returns>
    public Property GetOrAddProperty(Type propType, Func<Property> constructor)
    {
        if (PropertyInternals.HeldProperties.TryGetValue(propType, out Property p))
        {
            return p;
        }
        Property constructedProperty = constructor();
        if (constructedProperty.Holder != null)
        {
            throw new InvalidOperationException("That property is already held by something!");
        }
        PropertyInternals.HeldProperties[propType] = constructedProperty;
        PropertyInternals.NoticeProperty(this, propType, constructedProperty);
        return constructedProperty;
    }

    /// <summary>
    /// Gets the property (with a generic type), or adds the property with the specified property constructor.
    /// <para>May still throw an exception, if the property is held elsewhere!</para>
    /// <para>Be careful with this, as it can lead to incorrect typing if the Func input has an incorrect type!</para>
    /// </summary>
    /// <typeparam name="T">The property class type.</typeparam>
    /// <exception cref="InvalidOperationException">If the property is already held by a different object.</exception>
    /// <returns>The property.</returns>
    public T GetOrAddProperty<T>(Func<T> constructor) where T : Property
    {
        if (PropertyInternals.HeldProperties.TryGetValue(typeof(T), out Property p))
        {
            return p as T;
        }
        T res = constructor();
        if (res.Holder != null)
        {
            throw new InvalidOperationException("That property is already held by something!");
        }
        PropertyInternals.HeldProperties[typeof(T)] = res;
        PropertyInternals.NoticeProperty(this, typeof(T), res);
        return res;
    }

    /// <summary>Called when a property is added.</summary>
    /// <param name="prop">The property.</param>
    public virtual void OnAdded(Property prop)
    {
    }

    /// <summary>Called when a property is removed.</summary>
    /// <param name="prop">The property.</param>
    public virtual void OnRemoved(Property prop)
    {
    }
}
