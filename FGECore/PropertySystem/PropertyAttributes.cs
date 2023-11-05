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

namespace FGECore.PropertySystem;

/// <summary>Used to indicate that a property field is debuggable (if not marked, the property field is not debuggable).</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class PropertyDebuggable : Attribute
{
}

/// <summary>Used to indicate that a property field is auto-saveable (if not marked, the property field is not auto-saveable).</summary>
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
    /// <summary>The priority.</summary>
    public double Priority;

    /// <summary>Construct the priority.</summary>
    /// <param name="_prio">The priority value.</param>
    public PropertyPriority(double _prio)
    {
        Priority = _prio;
    }
}
