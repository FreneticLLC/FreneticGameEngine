//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using FGECore.EntitySystem;
using FGECore.EntitySystem.PhysicsHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.PhysicsSystem
{

    /// <summary>Implementation for <see cref="IPoseIntegratorCallbacks"/>. Some doc comments copied from BEPU source.</summary>
    public struct BepuPoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        /// <summary>The backing physics space.</summary>
        public PhysicsSpace Space;

        /// <summary>Gets how the pose integrator should handle angular velocity integration.</summary>
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.ConserveMomentum;

        /// <summary>Performs any required initialization logic after the Simulation instance has been constructed.</summary>
        public void Initialize(Simulation simulation)
        {
        }

        /// <summary>Current delta time value.</summary>
        public float Delta;

        /// <summary>
        /// Called prior to integrating the simulation's active bodies. When used with a substepping timestepper, this could be called multiple times per frame with different time step values.
        /// </summary>
        /// <param name="dt">Current time step duration.</param>
        public void PrepareForIntegration(float dt)
        {
            Delta = dt;
        }

        /// <summary>
        /// Callback called for each active body within the simulation during body integration.
        /// </summary>
        /// <param name="bodyIndex">Index of the body being visited.</param>
        /// <param name="pose">Body's current pose.</param>
        /// <param name="localInertia">Body's current local inertia.</param>
        /// <param name="workerIndex">Index of the worker thread processing this body.</param>
        /// <param name="velocity">Reference to the body's current velocity to integrate.</param>
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            if (localInertia.InverseMass > 0)
            {
                EntityPhysicsProperty physicsEntity = Space.Internal.EntitiesByPhysicsID[Space.Internal.CoreSimulation.Bodies.ActiveSet.IndexToHandle[bodyIndex].Value];
                if (physicsEntity != null)
                {
                    velocity.Linear += (physicsEntity.GravityIsSet ? physicsEntity.Gravity : Space.Gravity).ToNumerics() * Delta;
                    float linearDampingDt = MathF.Pow(MathHelper.Clamp(1 - physicsEntity.LinearDamping, 0, 1), Delta); // TODO: These clamps look wrong. Should be done in advance, or should surround the pow call?
                    float angularDampingDt = MathF.Pow(MathHelper.Clamp(1 - physicsEntity.AngularDamping, 0, 1), Delta);
                    velocity.Linear *= linearDampingDt;
                    velocity.Angular *= angularDampingDt;
                }
            }
        }
    }

    /// <summary>Implementation for <see cref="INarrowPhaseCallbacks"/>. Some doc comments copied from BEPU source.</summary>
    public struct BepuNarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        /// <summary>The backing physics space.</summary>
        public PhysicsSpace Space;

        /// <summary>Defines the default constraint's penetration recovery spring properties.</summary>
        public SpringSettings ContactSpringiness;

        /// <summary>Performs any required initialization logic after the Simulation instance has been constructed.</summary>
        public void Initialize(Simulation simulation)
        {
            if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
            {
                // TODO: ?
                ContactSpringiness = new SpringSettings(30, 0.5f);
            }
            Space.Internal.Characters.Initialize(simulation);
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for two overlapping collidables.
        /// </summary>
        /// <param name="workerIndex">Index of the worker that identified the overlap.</param>
        /// <param name="a">Reference to the first collidable in the pair.</param>
        /// <param name="b">Reference to the second collidable in the pair.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            if (a.Mobility != CollidableMobility.Dynamic && b.Mobility != CollidableMobility.Dynamic)
            {
                return false;
            }
            EntityPhysicsProperty aEntity = Space.Internal.EntitiesByPhysicsID[a.BodyHandle.Value];
            EntityPhysicsProperty bEntity = Space.Internal.EntitiesByPhysicsID[b.BodyHandle.Value];
            if (aEntity == null || bEntity == null)
            {
                EntityPhysicsProperty validOne = (aEntity ?? bEntity);
                if (validOne != null)
                {
                    return validOne.CGroup.DoesCollide(CollisionUtil.WorldSolid);
                }
                return false;
            }
            HashSet<long> noCollide = aEntity.Internal.NoCollideIDs;
            if (noCollide != null && noCollide.Contains(bEntity.Entity.EID))
            {
                return false;
            }
            return aEntity.CGroup.DoesCollide(bEntity.CGroup);
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for the children of two overlapping collidables in a compound-including pair.
        /// </summary>
        /// <param name="workerIndex">Index of the worker that identified the overlap.</param>
        /// <param name="pair">Parent pair of the two child collidables.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        /// <remarks>This is called for each sub-overlap in a collidable pair involving compound collidables. If neither collidable in a pair is compound, this will not be called.
        /// For compound-including pairs, if the earlier call to AllowContactGeneration returns false for owning pair, this will not be called. Note that it is possible
        /// for this function to be called twice for the same subpair if the pair has continuous collision detection enabled; 
        /// the CCD sweep test that runs before the contact generation test also asks before performing child pair tests.</remarks>
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        /// <summary>
        /// Provides a notification that a manifold has been created for a pair. Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <param name="pairMaterial">Material properties of the manifold.</param>
        /// <returns>True if a constraint should be created for the manifold, false otherwise.</returns>
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            EntityPhysicsProperty aEntity = Space.Internal.EntitiesByPhysicsID[pair.A.BodyHandle.Value];
            EntityPhysicsProperty bEntity = Space.Internal.EntitiesByPhysicsID[pair.B.BodyHandle.Value];
            if (aEntity == null || bEntity == null)
            {
                EntityPhysicsProperty validOne = (aEntity ?? bEntity);
                if (validOne != null)
                {
                    pairMaterial.FrictionCoefficient = validOne.Friction * validOne.Friction;
                    pairMaterial.MaximumRecoveryVelocity = validOne.Bounciness * 2;
                }
                else
                {
                    pairMaterial.FrictionCoefficient = 1f;
                    pairMaterial.MaximumRecoveryVelocity = 0.5f;
                }
            }
            else
            {
                pairMaterial.FrictionCoefficient = aEntity.Friction * bEntity.Friction;
                pairMaterial.MaximumRecoveryVelocity = aEntity.Bounciness + bEntity.Bounciness;
            }
            pairMaterial.SpringSettings = ContactSpringiness;
            if (aEntity?.CollisionHandler != null || bEntity?.CollisionHandler != null)
            {
                CollisionEvent evt = new CollisionEvent<TManifold>() { One = aEntity, Two = bEntity, Manifold = manifold };
                aEntity?.CollisionHandler?.Invoke(evt);
                bEntity?.CollisionHandler?.Invoke(evt);
                if (evt.Cancel)
                {
                    return false;
                }
            }
            Space.Internal.Characters.TryReportContacts(pair, ref manifold, workerIndex, ref pairMaterial);
            return true;
        }

        /// <summary>
        /// Provides a notification that a manifold has been created between the children of two collidables in a compound-including pair.
        /// Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <returns>True if this manifold should be considered for constraint generation, false otherwise.</returns>
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        /// <summary>Releases any resources held by the callbacks. Called by the owning narrow phase when it is being disposed.</summary>
        public void Dispose()
        {
        }
    }
}
