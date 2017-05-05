using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a physical space.
    /// </summary>
    public class PhysicsSpace
    {
        /// <summary>
        /// The actual internal physics space.
        /// </summary>
        public Space Internal;

        /// <summary>
        /// Gets or sets the internal gravity value.
        /// </summary>
        public Location Gravity
        {
            get
            {
                return new Location(Internal.ForceUpdater.Gravity);
            }
            set
            {
                Internal.ForceUpdater.Gravity = value.ToBVector();
            }
        }
    }
}
