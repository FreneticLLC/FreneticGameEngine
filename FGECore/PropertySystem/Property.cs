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

namespace FGECore.PropertySystem
{

    /// <summary>Represents a set of custom data attached to an object.</summary>
    public abstract class Property
    {
        /// <summary>The holder of this property. Modifying this value could lead to errors!</summary>
        public PropertyHolder Holder = null;

        /// <summary>The system that helps this property's field information.</summary>
        public PropertyHelper Helper = null;

        /// <summary>Gets the debug output for this property.</summary>
        /// <returns>The debuggable data.</returns>
        public Dictionary<string, string> GetDebuggable()
        {
            Dictionary<string, string> strs = new Dictionary<string, string>();
            Helper.GetDebuggableInfoOutput(this, strs);
            return strs;
        }

        /// <summary>Returns whether this property is currently held by something.</summary>
        /// <returns>Whether it is held.</returns>
        public bool IsHeld()
        {
            return Holder != null;
        }

        /// <summary>Returns whether this property is currently held by something, and outputs the holder if so (otherwise, outputs null).</summary>
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

        /// <summary>This is fired when the property is added to a system.</summary>
        public virtual void OnAdded()
        {
            // Do nothing by default.
        }

        /// <summary>This is fired when the property is removed from a system.</summary>
        public virtual void OnRemoved()
        {
            // Do nothing by default.
        }

        /// <summary>Gets a string-ified version of this property.</summary>
        /// <returns>The property string.</returns>
        public override string ToString()
        {
            return $"Property<{GetPropertyName()}>";
        }
    }
}
