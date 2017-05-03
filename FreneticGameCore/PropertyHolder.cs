using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool RemoveProperty<T>() where T: Property
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
        public T GetProperty<T>() where T: Property
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
            prop.Holder = this;
            HeldProperties.Add(prop.GetType(), prop);
            prop.OnAdded();
        }

        /// <summary>
        /// Adds the property, or gives an exception if a property of matching type already exists.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="prop">The property itself.</param>
        /// <returns>The property.</returns>
        public void AddProperty<T>(T prop) where T : Property
        {
            if (prop.Holder != null)
            {
                throw new InvalidOperationException("That property is already held by something!");
            }
            prop.Holder = this;
            HeldProperties.Add(typeof(T), prop);
            prop.OnAdded();
        }

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
            HeldProperties[t] = res;
            res.OnAdded();
            return res;
        }

        /// <summary>
        /// Gets the property (with a generic type), or adds the property with the specified property constructor.
        /// May still throw an exception, if the property is held elsewhere!
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
            HeldProperties[typeof(T)] = res;
            res.OnAdded();
            return res;
        }
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
    }
}
