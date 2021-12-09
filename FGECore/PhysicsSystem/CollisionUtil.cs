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
using FGECore.EntitySystem;
using FGECore.MathHelpers;
using BepuPhysics.Collidables;

namespace FGECore.PhysicsSystem
{
    /// <summary>Represents the results of a collision trace.</summary>
    public class CollisionResult
    {
        /// <summary>Whether it hit anything.</summary>
        public bool Hit;

        /// <summary>The impact normal. Warning: not normalized!</summary>
        public Location Normal;

        /// <summary>The end location.</summary>
        public Location Position;

        /// <summary>The hit entity, if any.</summary>
        public EntityPhysicsProperty HitEnt;

        /// <summary>The time of hit, where 0 is colliding-at-the-start, up to the distance from start to target.</summary>
        public float Time;
    }

    /// <summary>Helper to group together logic for whether two objects collide.</summary>
    public class CollisionGroup
    {
        /// <summary>Static current collision group ID to always give a unique ID to new group objects.</summary>
        public static uint CurrentID = 0;

        /// <summary>A unique integer identifying this collision group instance.</summary>
        public uint ID = CurrentID++;

        /// <summary>An array of booleans, with a true value for any indices that match a collision group ID that this group will NOT collide with.</summary>
        public bool[] NoCollideWith = Array.Empty<bool>();

        /// <summary>The clean name of this collision group.</summary>
        public string Name;

        /// <summary>Construct a new collision group instance.</summary>
        public CollisionGroup(string _name)
        {
            Name = _name;
        }

        /// <summary>Returns true if this group should collide with another.</summary>
        public bool DoesCollide(CollisionGroup two)
        {
            return two.ID >= NoCollideWith.Length || !NoCollideWith[two.ID];
        }

        /// <summary>Sets whether this group should collide with another.</summary>
        public void SetNoCollide(CollisionGroup two, bool noCollide)
        {
            if (two.ID >= NoCollideWith.Length)
            {
                bool[] newArr = new bool[two.ID + 5];
                Array.Copy(NoCollideWith, newArr, NoCollideWith.Length);
                NoCollideWith = newArr;
            }
            NoCollideWith[two.ID] = noCollide;
        }

        /// <summary>Implements <see cref="Object.ToString"/>.</summary>
        public override string ToString()
        {
            return $"CollisionGroup({Name})";
        }
    }

    /// <summary>Helper code for tracing collision.</summary>
    public class CollisionUtil
    {
        /// <summary>The space associated with this utility.</summary>
        public PhysicsSpace World;

        /// <summary>The non-solid group.</summary>
        public static CollisionGroup NonSolid = new("NonSolid");

        /// <summary>The solid entity group.</summary>
        public static CollisionGroup Solid = new("Solid");

        /// <summary>The player group.</summary>
        public static CollisionGroup Player = new("Player");

        /// <summary>The item-entity group.</summary>
        public static CollisionGroup Item = new("Item");

        /// <summary>The water (or other liquid) group.</summary>
        public static CollisionGroup Water = new("Water");

        /// <summary>The world-solid group.</summary>
        public static CollisionGroup WorldSolid = new("WorldSolid");

        /// <summary>The non-player character group.</summary>
        public static CollisionGroup Character = new("Character");

        static CollisionUtil()
        {
            // TODO: This maybe shouldn't be so rigidly predefined in FGE, leave it up to games instead.

            // NonSolid Vs. Solid,NonSolid,WorldSolid (All)
            NonSolid.SetNoCollide(WorldSolid, true);
            WorldSolid.SetNoCollide(NonSolid, true);
            NonSolid.SetNoCollide(NonSolid, true);
            NonSolid.SetNoCollide(Solid, true);
            Solid.SetNoCollide(NonSolid, true);
            // Player Vs. NonSolid,Player
            Player.SetNoCollide(NonSolid, true);
            NonSolid.SetNoCollide(Player, true);
            Player.SetNoCollide(Player, true);
            // Item Vs. NonSolid (All)
            Item.SetNoCollide(NonSolid, true);
            NonSolid.SetNoCollide(Item, true);
            // Water Vs. NonSolid,Solid,Player,Item (All)
            Water.SetNoCollide(NonSolid, true);
            NonSolid.SetNoCollide(Water, true);
            Water.SetNoCollide(Solid, true);
            Solid.SetNoCollide(Water, true);
            Water.SetNoCollide(Player, true);
            Player.SetNoCollide(Water, true);
            Water.SetNoCollide(Item, true);
            Item.SetNoCollide(Water, true);
            // Non-player Character Vs. NonSolid,Item,Water
            Character.SetNoCollide(NonSolid, true);
            NonSolid.SetNoCollide(Character, true);
            Character.SetNoCollide(Water, true);
            Water.SetNoCollide(Character, true);
            Character.SetNoCollide(Item, true);
            Item.SetNoCollide(Character, true);
        }

        /// <summary>Checks if an entry should collide at all ever.</summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Whether it should collide.</returns>
        public static bool ShouldCollide(EntityPhysicsProperty entry)
        {
            if (entry.CGroup == NonSolid || entry.CGroup == Water)
            {
                return false;
            }
            return true;
        }

        /// <summary>Constructs the utility.</summary>
        /// <param name="world">The physics world.</param>
        public CollisionUtil(PhysicsSpace world)
        {
            World = world;
        }

        /// <summary>Performs a cuboid line trace.</summary>
        /// <param name="halfsize">Half the size of the box.</param>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <param name="filter">The collision filter, if any.</param>
        /// <returns>Results.</returns>
        public CollisionResult CuboidLineTrace(in Location halfsize, in Location start, in Location end, Func<EntityPhysicsProperty, bool> filter = null)
        {
            Box shape = new((float)halfsize.X * 2f, (float)halfsize.Y * 2f, (float)halfsize.Z * 2f);
            return CuboidLineTrace(shape, start, end, filter);
        }

        /// <summary>Returns information on what a cuboid-shaped line trace would collide with, if anything.</summary>
        /// <param name="shape">The shape to trace with.</param>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="filter">The collision filter, input a BEPU BroadPhaseEntry and output whether collision should be allowed.</param>
        /// <returns>The collision details.</returns>
        public CollisionResult CuboidLineTrace<TShape>(TShape shape, in Location start, in Location end, Func<EntityPhysicsProperty, bool> filter = null) where TShape : unmanaged, IConvexShape
        {
            double len = (end - start).Length();
            return World.ConvexTraceSingle(shape, start, (end - start) / len, len, filter);
        }

        /// <summary>Returns information on what a line trace would collide with, if anything.</summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="filter">The collision filter, input a BEPU BroadPhaseEntry and output whether collision should be allowed.</param>
        /// <returns>The collision details.</returns>
        public CollisionResult RayTrace(in Location start, in Location end, Func<EntityPhysicsProperty, bool> filter = null)
        {
            double len = (end - start).Length();
            return World.RayTraceSingle(start, (end - start) / len, len, filter);
        }
    }
}
