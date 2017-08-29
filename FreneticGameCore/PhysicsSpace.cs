//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using FreneticGameCore.EntitySystem;
using BEPUutilities;
using BEPUphysics.Entities;
using BEPUutilities.Threading;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a physical world (space).
    /// </summary>
    public class PhysicsSpace
    {
        /// <summary>
        /// The actual internal physics space.
        /// </summary>
        public Space Internal;

        /// <summary>
        /// The scale of all physics vs. rendered objects. Phys * Scale = Render.
        /// </summary>
        public double RelativeScale = 1f;

        /// <summary>
        /// Construct the physics space.
        /// </summary>
        public PhysicsSpace()
        {
            ParallelLooper pl = new ParallelLooper();
            for (int i = 0; i < Environment.ProcessorCount * 2; i++)
            {
                pl.AddThread();
            }
            Internal = new Space(pl);
            Internal.ForceUpdater.Gravity = new Vector3(0, 0, -9.8);
        }

        /// <summary>
        /// Gets or sets the internal default gravity value.
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

        /// <summary>
        /// All current entities in this physics world.
        /// </summary>
        public List<BasicEntity> SpawnedEntities = new List<BasicEntity>();

        /// <summary>
        /// Spawns a physical object into the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="ent">The controlling entity.</param>
        /// <param name="bepuent">The BEPU object.</param>
        public void Spawn(BasicEntity ent, ISpaceObject bepuent)
        {
            Internal.Add(bepuent);
            SpawnedEntities.Add(ent);
        }

        /// <summary>
        /// De-Spawns a physical object from the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="ent">The controlling entity.</param>
        /// <param name="bepuent">The BEPU object.</param>
        public void DeSpawn(BasicEntity ent, ISpaceObject bepuent)
        {
            Internal.Remove(bepuent);
            SpawnedEntities.Remove(ent);
        }
    }
}
