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

namespace FGECore.NetworkSystem
{
    /// <summary>
    /// Indicates that a field is to be handled by the network.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class NetworkableData : Attribute
    {
        /// <summary>
        /// When to update the data.
        /// </summary>
        public NetworkTime NetUsage;
    }

    /// <summary>
    /// Indicates that a property class is to be handled by the network.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NetworkableProperty : Attribute
    {
        /// <summary>
        /// How important it is to update this property.
        /// <para>This helps decide which order and what times to update a property with.</para>
        /// <para>Properties approaching their <see cref="MaximumUpdateDelay"/> are automatically considered higher priority.</para>
        /// </summary>
        public double UpdateImportance;

        /// <summary>
        /// The smallest amount of time (in seconds) before an update may be sent.
        /// </summary>
        public double MinimumUpdateDelay = 0.01;

        /// <summary>
        /// The largest amount of time (in seconds) before an update MUST be sent.
        /// <para>0 or negative values indicate disabling of this feature.</para>
        /// </summary>
        public double MaximumUpdateDelay = -1;

        /// <summary>
        /// The network channel to send this property on.
        /// <para>Must be a valid network channel ID (registered with the <see cref="GameNetwork"/>): otherwise no data will be sent!</para>
        /// </summary>
        public int NetChannel = 0;
    }

    /// <summary>
    /// Indicates that a bool-returning C# property is used to decide whether network existence transmission should be sent on any given network tick.
    /// <para>Network existence transmission is when the network tells a client that the property is present.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NetworkExistsActivator : Attribute
    {
    }

    /// <summary>
    /// Indicates that a bool-returning C# property is used to decide whether network update transmission should be sent on any given network tick.
    /// <para>Network update transmission is when the network tells a client of the current state of a property (that the client is already aware of).</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NetworkUpdateActivator : Attribute
    {
    }

    /// <summary>
    /// An enumeration of times to network a value.
    /// </summary>
    [Flags]
    public enum NetworkTime
    {
        /// <summary>
        /// Never network the value.
        /// </summary>
        NEVER = 0,
        /// <summary>
        /// Network the value during existence transmission. (Existence transmission generally include an update!)
        /// </summary>
        EXISTENCE = 1,
        /// <summary>
        /// network the value during update transmission. (Will generally also run from existence transmission!)
        /// </summary>
        UPDATE = 1 << 1
    }
}
