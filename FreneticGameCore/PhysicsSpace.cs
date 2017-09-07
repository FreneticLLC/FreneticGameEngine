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
using BEPUphysics.BroadPhaseEntries;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a physical world (space).
    /// </summary>
    public class PhysicsSpace<T, T2> where T: BasicEntity<T2> where T2: BasicEngine<T, T2>
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
        public List<T> SpawnedEntities = new List<T>();

        /// <summary>
        /// Spawns a physical object into the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="ent">The controlling entity.</param>
        /// <param name="bepuent">The BEPU object.</param>
        public void Spawn(T ent, ISpaceObject bepuent)
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
        public void Despawn(T ent, ISpaceObject bepuent)
        {
            Internal.Remove(bepuent);
            SpawnedEntities.Remove(ent);
        }

        /// <summary>
        /// Sends a world ray trace, giving back the single found object, or null if none.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="dist">The distance.</param>
        /// <param name="filter">The filter, if any.</param>
        public T RayTraceSingle(Location start, Location dir, double dist, Func<BroadPhaseEntry, bool> filter = null)
        {
            RayCastResult rcr;
            if (filter != null)
            {
                if (!Internal.RayCast(new Ray(start.ToBVector(), dir.ToBVector()), dist, filter, out rcr))
                {
                    return null;
                }
            }
            else
            {
                if (!Internal.RayCast(new Ray(start.ToBVector(), dir.ToBVector()), dist, out rcr))
                {
                    return null;
                }
            }
            if (rcr.HitObject != null && rcr.HitObject.Tag != null)
            {
                return rcr.HitObject.Tag as T;
            }
            SysConsole.Output(OutputType.DEBUG, "FAILED : " + start + ", " + dir + ", " + dist + ": " +rcr.HitObject + " ? " + rcr.HitObject?.Tag);
            return null;
        }
    }
}
