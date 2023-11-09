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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FGECore.EntitySystem;
using FGECore.EntitySystem.PhysicsHelpers;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace FGECore.PhysicsSystem;


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

    /// <summary>
    /// (FROM BEPU SOURCE)
    /// Gets whether the integrator should use substepping for unconstrained bodies when using a substepping solver.
    /// If true, unconstrained bodies will be integrated with the same number of substeps as the constrained bodies in the solver.
    /// If false, unconstrained bodies use a single step of length equal to the dt provided to Simulation.Timestep. 
    /// </summary>
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;

    /// <summary>
    /// (FROM BEPU SOURCE)
    /// Gets whether the velocity integration callback should be called for kinematic bodies.
    /// If true, IntegrateVelocity will be called for bundles including kinematic bodies.
    /// If false, kinematic bodies will just continue using whatever velocity they have set.
    /// Most use cases should set this to false.
    /// </summary>
    public readonly bool IntegrateVelocityForKinematics => false;

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

    /// <summary>Callback called for each active body within the simulation during body integration.</summary>
    /// <param name="bodyIndices">Indices of the bodies being integrated in this bundle.</param>
    /// <param name="position">Current body positions.</param>
    /// <param name="orientation">Current body orientations.</param>
    /// <param name="localInertia">Body's current local inertia.</param>
    /// <param name="integrationMask">Mask indicating which lanes are active in the bundle. Active lanes will contain 0xFFFFFFFF, inactive lanes will contain 0.</param>
    /// <param name="workerIndex">Index of the worker thread processing this bundle.</param>
    /// <param name="dt">Durations to integrate the velocity over. Can vary over lanes.</param>
    /// <param name="velocity">Velocity of bodies in the bundle. Any changes to lanes which are not active by the integrationMask will be discarded.</param>
    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        // Note: this is terrifying because the input is SIMD vectors that need to be decomposed and recomposed. Blame BEPU.
        for (int i = 0; i < Vector<int>.Count; i++)
        {
            if (integrationMask[i] == 0)
            {
                continue;
            }
            if (localInertia.InverseMass[i] > 0)
            {
                int bodyIndex = bodyIndices[i];
                EntityPhysicsProperty physicsEntity = Space.Internal.EntitiesByPhysicsID[Space.Internal.CoreSimulation.Bodies.ActiveSet.IndexToHandle[bodyIndex].Value];
                if (physicsEntity != null)
                {
                    Vector3Wide.ReadSlot(ref velocity.Linear, i, out Vector3 velLinear);
                    Vector3Wide.ReadSlot(ref velocity.Angular, i, out Vector3 velAngular);
                    velLinear += physicsEntity.ActualGravity.ToNumerics() * Delta;
                    float linearDampingDt = MathF.Pow(1 - physicsEntity.LinearDamping, Delta);
                    float angularDampingDt = MathF.Pow(1 - physicsEntity.AngularDamping, Delta);
                    velLinear *= linearDampingDt;
                    velAngular *= angularDampingDt;
                    Vector3Wide.WriteSlot(velLinear, i, ref velocity.Linear);
                    Vector3Wide.WriteSlot(velAngular, i, ref velocity.Angular);
                }
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

    /// <summary>Helper to get a <see cref="EntityPhysicsProperty"/> from a <see cref="CollidableReference"/>, or null.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityPhysicsProperty PhysPropForCollidable(CollidableReference collidable)
    {
        return collidable.Mobility == CollidableMobility.Dynamic ? Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value] : null;
    }

    /// <summary>Chooses whether to allow contact generation to proceed for two overlapping collidables.</summary>
    /// <param name="workerIndex">Index of the worker that identified the overlap.</param>
    /// <param name="a">Reference to the first collidable in the pair.</param>
    /// <param name="b">Reference to the second collidable in the pair.</param>
    /// <param name="speculativeMargin">Reference to the speculative margin used by the pair.</param>
    /// <returns>True if collision detection should proceed, false otherwise.</returns>
    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        if (a.Mobility != CollidableMobility.Dynamic && b.Mobility != CollidableMobility.Dynamic)
        {
            return false;
        }
        EntityPhysicsProperty aEntity = PhysPropForCollidable(a);
        EntityPhysicsProperty bEntity = PhysPropForCollidable(b);
        if (aEntity is null || bEntity is null)
        {
            EntityPhysicsProperty validOne = aEntity ?? bEntity;
            if (validOne is not null)
            {
                return validOne.CGroup.DoesCollide(CollisionUtil.WorldSolid);
            }
            return false;
        }
        HashSet<long> noCollide = aEntity.Internal.NoCollideIDs;
        if (noCollide is not null && noCollide.Contains(bEntity.Entity.EID))
        {
            return false;
        }
        return aEntity.CGroup.DoesCollide(bEntity.CGroup);
    }

    /// <summary>Chooses whether to allow contact generation to proceed for the children of two overlapping collidables in a compound-including pair.</summary>
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

    /// <summary>Provides a notification that a manifold has been created for a pair. Offers an opportunity to change the manifold's details.</summary>
    /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
    /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
    /// <param name="manifold">Set of contacts detected between the collidables.</param>
    /// <param name="pairMaterial">Material properties of the manifold.</param>
    /// <returns>True if a constraint should be created for the manifold, false otherwise.</returns>
    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        EntityPhysicsProperty aEntity = PhysPropForCollidable(pair.A);
        EntityPhysicsProperty bEntity = PhysPropForCollidable(pair.B);
        if (aEntity is null || bEntity is null)
        {
            EntityPhysicsProperty validOne = aEntity ?? bEntity;
            if (validOne is not null)
            {
                pairMaterial.FrictionCoefficient = validOne.Friction * validOne.Friction;
                pairMaterial.MaximumRecoveryVelocity = validOne.Bounciness * 4;
            }
            else
            {
                pairMaterial.FrictionCoefficient = 1;
                pairMaterial.MaximumRecoveryVelocity = 2;
            }
        }
        else
        {
            pairMaterial.FrictionCoefficient = aEntity.Friction * bEntity.Friction;
            pairMaterial.MaximumRecoveryVelocity = aEntity.Bounciness + bEntity.Bounciness;
        }
        pairMaterial.SpringSettings = ContactSpringiness;
        if (aEntity?.CollisionHandler is not null || bEntity?.CollisionHandler is not null)
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
