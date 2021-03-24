//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.JointSystems
{
    /// <summary>The lowest level base class for all joints.</summary>
    public abstract class GenericBaseJoint : IEquatable<GenericBaseJoint>
    {
        /// <summary>Get the first entity in the joint.</summary>
        public abstract BasicEntity EntityOne { get; }

        /// <summary>Get the second entity in the joint.</summary>
        public abstract BasicEntity EntityTwo { get; }

        /// <summary>Get the generic engine backing this joint.</summary>
        public BasicEngine EngineGeneric => EntityOne.EngineGeneric;

        /// <summary>A unique ID for this specific joint.</summary>
        public long JointID;

        /// <summary>Called to enable the joint however necessary.</summary>
        public abstract void Enable();

        /// <summary>Called to disable the joint however necessary.</summary>
        public abstract void Disable();

        /// <summary>Implements <see cref="Object.GetHashCode"/>.</summary>
        public override int GetHashCode()
        {
            return JointID.GetHashCode();
        }

        /// <summary>Implements <see cref="Object.Equals(object?)"/>.</summary>
        public override bool Equals(object obj)
        {
            return obj is GenericBaseJoint joint && Equals(joint);
        }

        /// <summary>Returns whether this joint is the same as other.</summary>
        public bool Equals(GenericBaseJoint other)
        {
            if (JointID == 0)
            {
                return ReferenceEquals(this, other);
            }
            return JointID == other.JointID;
        }
    }
}
