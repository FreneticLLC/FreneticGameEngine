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
using BEPUutilities;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionRuleManagement;
using FreneticGameCore.MathHelpers;

namespace FreneticGameCore.PhysicsSystem
{
    /// <summary>
    /// Represents the results of a collision trace.
    /// </summary>
    public class CollisionResult
    {
        /// <summary>
        /// Whether it hit anything.
        /// </summary>
        public bool Hit;

        /// <summary>
        /// The impact normal. Warning: not normalized!
        /// </summary>
        public Location Normal;

        /// <summary>
        /// The end location.
        /// </summary>
        public Location Position;

        /// <summary>
        /// The hit entity, if any.
        /// </summary>
        public Entity HitEnt;
    }

    /// <summary>
    /// Helper code for tracing collision.
    /// </summary>
    public class CollisionUtil
    {
        /// <summary>
        /// The space associated with this utility.
        /// </summary>
        public Space World;

        /// <summary>
        /// The non-solid group.
        /// </summary>
        public static CollisionGroup NonSolid = new CollisionGroup();

        /// <summary>
        /// The solid group.
        /// </summary>
        public static CollisionGroup Solid = new CollisionGroup();

        /// <summary>
        /// The player group.
        /// </summary>
        public static CollisionGroup Player = new CollisionGroup();

        /// <summary>
        /// The item group.
        /// </summary>
        public static CollisionGroup Item = new CollisionGroup();

        /// <summary>
        /// The water group.
        /// </summary>
        public static CollisionGroup Water = new CollisionGroup();

        /// <summary>
        /// The world-solid group.
        /// </summary>
        public static CollisionGroup WorldSolid = new CollisionGroup();

        /// <summary>
        /// The character group.
        /// </summary>
        public static CollisionGroup Character = new CollisionGroup();

        static CollisionUtil()
        {
            // NonSolid Vs. Solid,NonSolid,WorldSolid (All)
            CollisionGroup.DefineCollisionRule(NonSolid, WorldSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(WorldSolid, NonSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(NonSolid, NonSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(NonSolid, Solid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Solid, NonSolid, CollisionRule.NoBroadPhase);
            // Player Vs. NonSolid,Player
            CollisionGroup.DefineCollisionRule(Player, NonSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(NonSolid, Player, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Player, Player, CollisionRule.NoBroadPhase);
            // Item Vs. NonSolid (All)
            CollisionGroup.DefineCollisionRule(Item, NonSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(NonSolid, Item, CollisionRule.NoBroadPhase);
            // Water Vs. NonSolid,Solid,Player,Item (All)
            CollisionGroup.DefineCollisionRule(Water, NonSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(NonSolid, Water, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Water, Solid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Solid, Water, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Water, Player, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Player, Water, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Water, Item, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Item, Water, CollisionRule.NoBroadPhase);
            // Non-player Character Vs. NonSolid,Item,Water
            CollisionGroup.DefineCollisionRule(Character, NonSolid, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(NonSolid, Character, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Character, Water, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Water, Character, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Character, Item, CollisionRule.NoBroadPhase);
            CollisionGroup.DefineCollisionRule(Item, Character, CollisionRule.NoBroadPhase);
        }

        /// <summary>
        /// Checks if an entry should collide at all ever.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Whether it should collide.</returns>
        public static bool ShouldCollide(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == NonSolid || entry.CollisionRules.Group == Water)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Constructs the utility.
        /// </summary>
        /// <param name="world">The physics world.</param>
        public CollisionUtil(Space world)
        {
            World = world;
        }

        /// <summary>
        /// Performs a cuboid line trace.
        /// </summary>
        /// <param name="halfsize">Half the size of the box.</param>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <param name="filter">The collision filter, if any.</param>
        /// <returns>Results.</returns>
        public CollisionResult CuboidLineTrace(Location halfsize, Location start, Location end, Func<BroadPhaseEntry, bool> filter = null)
        {
            BoxShape shape = new BoxShape((double)halfsize.X * 2f, (double)halfsize.Y * 2f, (double)halfsize.Z * 2f);
            return CuboidLineTrace(shape, start, end, filter);
        }

        /// <summary>
        /// Returns information on what a cuboid-shaped line trace would collide with, if anything.
        /// </summary>
        /// <param name="shape">The shape to trace with.</param>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="filter">The collision filter, input a BEPU BroadPhaseEntry and output whether collision should be allowed.</param>
        /// <returns>The collision details.</returns>
        public CollisionResult CuboidLineTrace(ConvexShape shape, Location start, Location end, Func<BroadPhaseEntry, bool> filter = null)
        {
            Vector3 e = new Vector3((double)(end.X - start.X), (double)(end.Y - start.Y), (double)(end.Z - start.Z));
            RigidTransform rt = new RigidTransform(new Vector3((double)start.X, (double)start.Y, (double)start.Z));
            RayCastResult rcr;
            bool hit;
            if (filter == null)
            {
                hit = World.ConvexCast(shape, ref rt, ref e, out rcr);
            }
            else
            {
                hit = World.ConvexCast(shape, ref rt, ref e, filter, out rcr);
            }
            CollisionResult cr = new CollisionResult()
            {
                Hit = hit
            };
            if (hit)
            {
                cr.Normal = new Location(rcr.HitData.Normal);
                cr.Position = new Location(rcr.HitData.Location);
                if (rcr.HitObject is EntityCollidable)
                {
                    cr.HitEnt = ((EntityCollidable)rcr.HitObject).Entity;
                }
                else
                {
                    cr.HitEnt = null; // Impacted static world
                }
            }
            else
            {
                cr.Normal = Location.Zero;
                cr.Position = end;
                cr.HitEnt = null;
            }
            return cr;
        }

        /// <summary>
        /// Returns information on what a line trace would collide with, if anything.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="filter">The collision filter, input a BEPU BroadPhaseEntry and output whether collision should be allowed.</param>
        /// <returns>The collision details.</returns>
        public CollisionResult RayTrace(Location start, Location end, Func<BroadPhaseEntry, bool> filter = null)
        {
            double len = (end - start).Length();
            Ray ray = new Ray(start.ToBVector(), ((end - start) / len).ToBVector());
            RayCastResult rcr;
            bool hit;
            if (filter == null)
            {
                hit = World.RayCast(ray, (double)len, out rcr);
            }
            else
            {
                hit = World.RayCast(ray, (double)len, filter, out rcr);
            }
            CollisionResult cr = new CollisionResult() { Hit = hit };
            if (hit)
            {
                cr.Normal = new Location(rcr.HitData.Normal);
                cr.Position = new Location(rcr.HitData.Location);
                if (rcr.HitObject is EntityCollidable)
                {
                    cr.HitEnt = ((EntityCollidable)rcr.HitObject).Entity;
                }
                else
                {
                    cr.HitEnt = null; // Impacted static world
                }
            }
            else
            {
                cr.Normal = Location.Zero;
                cr.Position = end;
                cr.HitEnt = null;
            }
            return cr;
        }

        /// <summary>
        /// Returns whether a box contains (intersects with) another box.
        /// </summary>
        /// <param name="elow">The low point for box 1.</param>
        /// <param name="ehigh">The high point for box 1.</param>
        /// <param name="Low">The low point for box 2.</param>
        /// <param name="High">The high point for box 2.</param>
        /// <returns>whether there is intersection.</returns>
        public static bool BoxContainsBox(Location elow, Location ehigh, Location Low, Location High)
        {
            return Low.X <= ehigh.X && Low.Y <= ehigh.Y && Low.Z <= ehigh.Z &&
            High.X >= elow.X && High.Y >= elow.Y && High.Z >= elow.Z;
        }

        /// <summary>
        /// Returns whether a box contains a point.
        /// </summary>
        /// <param name="elow">The low point for the box.</param>
        /// <param name="ehigh">The high point for the box.</param>
        /// <param name="point">The point to check.</param>
        /// <returns>whether there is intersection.</returns>
        public static bool BoxContainsPoint(Location elow, Location ehigh, Location point)
        {
            return point.X <= ehigh.X && point.Y <= ehigh.Y && point.Z <= ehigh.Z &&
            point.X >= elow.X && point.Y >= elow.Y && point.Z >= elow.Z;
        }

        /// <summary>
        /// Runs a collision check between two AABB objects.
        /// </summary>
        /// <param name="Position">The block's position.</param>
        /// <param name="Mins">The block's mins.</param>
        /// <param name="Maxs">The block's maxs.</param>
        /// <param name="Mins2">The moving object's mins.</param>
        /// <param name="Maxs2">The moving object's maxs.</param>
        /// <param name="start">The starting location of the moving object.</param>
        /// <param name="end">The ending location of the moving object.</param>
        /// <param name="normal">The normal of the hit, or NaN if none.</param>
        /// <returns>The location of the hit, or NaN if none.</returns>
        public static Location AABBClosestBox(Location Position, Location Mins, Location Maxs, Location Mins2, Location Maxs2, Location start, Location end, out Location normal)
        {
            Location velocity = end - start;
            Location RealMins = Position + Mins;
            Location RealMaxs = Position + Maxs;
            Location RealMins2 = start + Mins2;
            Location RealMaxs2 = start + Maxs2;
            double xInvEntry, yInvEntry, zInvEntry;
            double xInvExit, yInvExit, zInvExit;
            if (end.X >= start.X)
            {
                xInvEntry = RealMins.X - RealMaxs2.X;
                xInvExit = RealMaxs.X - RealMins2.X;
            }
            else
            {
                xInvEntry = RealMaxs.X - RealMins2.X;
                xInvExit = RealMins.X - RealMaxs2.X;
            }
            if (end.Y >= start.Y)
            {
                yInvEntry = RealMins.Y - RealMaxs2.Y;
                yInvExit = RealMaxs.Y - RealMins2.Y;
            }
            else
            {
                yInvEntry = RealMaxs.Y - RealMins2.Y;
                yInvExit = RealMins.Y - RealMaxs2.Y;
            }
            if (end.Z >= start.Z)
            {
                zInvEntry = RealMins.Z - RealMaxs2.Z;
                zInvExit = RealMaxs.Z - RealMins2.Z;
            }
            else
            {
                zInvEntry = RealMaxs.Z - RealMins2.Z;
                zInvExit = RealMins.Z - RealMaxs2.Z;
            }
            double xEntry, yEntry, zEntry;
            double xExit, yExit, zExit;
            if (velocity.X == 0f)
            {
                xEntry = xInvEntry / 0.00000000000000000000000000000001f;
                xExit = xInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                xEntry = xInvEntry / velocity.X;
                xExit = xInvExit / velocity.X;
            }
            if (velocity.Y == 0f)
            {
                yEntry = yInvEntry / 0.00000000000000000000000000000001f;
                yExit = yInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                yEntry = yInvEntry / velocity.Y;
                yExit = yInvExit / velocity.Y;
            }
            if (velocity.Z == 0f)
            {
                zEntry = zInvEntry / 0.00000000000000000000000000000001f;
                zExit = zInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                zEntry = zInvEntry / velocity.Z;
                zExit = zInvExit / velocity.Z;
            }
            double entryTime = Math.Max(Math.Max(xEntry, yEntry), zEntry);
            double exitTime = Math.Min(Math.Min(xExit, yExit), zExit);
            if (entryTime > exitTime || (xEntry < 0.0f && yEntry < 0.0f && zEntry < 0.0f) || xEntry > 1.0f || yEntry > 1.0f || zEntry > 1.0f)
            {
                normal = Location.NaN;
                return Location.NaN;
            }
            else
            {
                if (zEntry >= xEntry && zEntry >= yEntry)
                {
                    if (zInvEntry < 0)
                    {
                        normal = new Location(0, 0, 1);
                    }
                    else
                    {
                        normal = new Location(0, 0, -1);
                    }
                }
                else if (xEntry >= zEntry && xEntry >= yEntry)
                {
                    if (xInvEntry < 0)
                    {
                        normal = new Location(1, 0, 0);
                    }
                    else
                    {
                        normal = new Location(-1, 0, 0);
                    }
                }
                else
                {
                    if (yInvEntry < 0)
                    {
                        normal = new Location(0, 1, 0);
                    }
                    else
                    {
                        normal = new Location(0, -1, 0);
                    }
                }
                Location res = start + (end - start) * entryTime;
                return new Location(res.X, res.Y, res.Z);
            }
        }



        /// <summary>
        /// Runs a collision check between an AABB and a ray.
        /// </summary>
        /// <param name="Position">The block's position.</param>
        /// <param name="Mins">The block's mins.</param>
        /// <param name="Maxs">The block's maxs.</param>
        /// <param name="start">The starting location of the ray.</param>
        /// <param name="end">The ending location of the ray.</param>
        /// <param name="normal">The normal of the hit, or NaN if none.</param>
        /// <returns>The location of the hit, or NaN if none.</returns>
        public static Location RayTraceBox(Location Position, Location Mins, Location Maxs, Location start, Location end, out Location normal)
        {
            Location velocity = end - start;
            Location RealMins = Position + Mins;
            Location RealMaxs = Position + Maxs;
            double xInvEntry, yInvEntry, zInvEntry;
            double xInvExit, yInvExit, zInvExit;
            if (end.X >= start.X)
            {
                xInvEntry = RealMins.X - start.X;
                xInvExit = RealMaxs.X - start.X;
            }
            else
            {
                xInvEntry = RealMaxs.X - start.X;
                xInvExit = RealMins.X - start.X;
            }
            if (end.Y >= start.Y)
            {
                yInvEntry = RealMins.Y - start.Y;
                yInvExit = RealMaxs.Y - start.Y;
            }
            else
            {
                yInvEntry = RealMaxs.Y - start.Y;
                yInvExit = RealMins.Y - start.Y;
            }
            if (end.Z >= start.Z)
            {
                zInvEntry = RealMins.Z - start.Z;
                zInvExit = RealMaxs.Z - start.Z;
            }
            else
            {
                zInvEntry = RealMaxs.Z - start.Z;
                zInvExit = RealMins.Z - start.Z;
            }
            double xEntry, yEntry, zEntry;
            double xExit, yExit, zExit;
            if (velocity.X == 0f)
            {
                xEntry = xInvEntry / 0.00000000000000000000000000000001f;
                xExit = xInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                xEntry = xInvEntry / velocity.X;
                xExit = xInvExit / velocity.X;
            }
            if (velocity.Y == 0f)
            {
                yEntry = yInvEntry / 0.00000000000000000000000000000001f;
                yExit = yInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                yEntry = yInvEntry / velocity.Y;
                yExit = yInvExit / velocity.Y;
            }
            if (velocity.Z == 0f)
            {
                zEntry = zInvEntry / 0.00000000000000000000000000000001f;
                zExit = zInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                zEntry = zInvEntry / velocity.Z;
                zExit = zInvExit / velocity.Z;
            }
            double entryTime = Math.Max(Math.Max(xEntry, yEntry), zEntry);
            double exitTime = Math.Min(Math.Min(xExit, yExit), zExit);
            if (entryTime > exitTime || (xEntry < 0.0f && yEntry < 0.0f && zEntry < 0.0f) || xEntry > 1.0f || yEntry > 1.0f || zEntry > 1.0f)
            {
                normal = Location.NaN;
                return Location.NaN;
            }
            else
            {
                if (zEntry >= xEntry && zEntry >= yEntry)
                {
                    if (zInvEntry < 0)
                    {
                        normal = new Location(0, 0, 1);
                    }
                    else
                    {
                        normal = new Location(0, 0, -1);
                    }
                }
                else if (xEntry >= zEntry && xEntry >= yEntry)
                {
                    if (xInvEntry < 0)
                    {
                        normal = new Location(1, 0, 0);
                    }
                    else
                    {
                        normal = new Location(-1, 0, 0);
                    }
                }
                else
                {
                    if (yInvEntry < 0)
                    {
                        normal = new Location(0, 1, 0);
                    }
                    else
                    {
                        normal = new Location(0, -1, 0);
                    }
                }
                Location res = start + (end - start) * entryTime;
                return new Location(res.X, res.Y, res.Z);
            }
        }

        /// <summary>
        /// Gets the lowest point of two points.
        /// </summary>
        /// <param name="one">The first point.</param>
        /// <param name="two">The second point.</param>
        /// <returns>The lowest point.</returns>
        public static Location GetLow(Location one, Location two)
        {
            return new Location(one.X < two.X ? one.X : two.X,
            one.Y < two.Y ? one.Y : two.Y,
            one.Z < two.Z ? one.Z : two.Z);
        }

        /// <summary>
        /// Gets the highest point of two points.
        /// </summary>
        /// <param name="one">The first point.</param>
        /// <param name="two">The second point.</param>
        /// <returns>The highest point.</returns>
        public static Location GetHigh(Location one, Location two)
        {
            return new Location(one.X > two.X ? one.X : two.X,
            one.Y > two.Y ? one.Y : two.Y,
            one.Z > two.Z ? one.Z : two.Z);
        }
    }
}
