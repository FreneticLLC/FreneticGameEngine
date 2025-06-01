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
using FGECore.CoreSystems;
using FGECore.EntitySystem;
using FGECore.EntitySystem.PhysicsHelpers;
using FGECore.MathHelpers;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace FGECore.PhysicsSystem;


/// <summary>Implementation for <see cref="IPoseIntegratorCallbacks"/>.</summary>
public struct BepuPoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    /// <summary>The backing FGE physics space.</summary>
    public PhysicsSpace Space;

    /// <inheritdoc/>
    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.ConserveMomentum;

    /// <inheritdoc/>
    public readonly void Initialize(Simulation simulation)
    {
    }

    /// <inheritdoc/>
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;

    /// <inheritdoc/>
    public readonly bool IntegrateVelocityForKinematics => false;

    /// <summary>Current delta time value.</summary>
    public float Delta;

    /// <inheritdoc/>
    public void PrepareForIntegration(float dt)
    {
        Delta = dt;
        Space.GeneralPhysicsUpdate?.Invoke(dt);
    }

    /// <inheritdoc/>
    public readonly void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        // Note: this is terrifying because the input is SIMD vectors that need to be decomposed and recomposed. Blame BEPU.
        for (int i = 0; i < Vector<int>.Count; i++)
        {
            if (integrationMask[i] == 0 || localInertia.InverseMass[i] <= 0)
            {
                continue;
            }
            int bodyIndex = bodyIndices[i];
            EntityPhysicsProperty physicsEntity = Space.Internal.EntitiesByPhysicsID[Space.Internal.CoreSimulation.Bodies.ActiveSet.IndexToHandle[bodyIndex].Value];
            if (physicsEntity is null)
            {
                continue;
            }
            float delta = dt[i];
            physicsEntity.PhysicsUpdate?.Invoke(delta);
            Space.PhysicsUpdate?.Invoke(physicsEntity, delta);
            Vector3Wide.ReadSlot(ref velocity.Linear, i, out Vector3 velLinear);
            Vector3Wide.ReadSlot(ref velocity.Angular, i, out Vector3 velAngular);
            velLinear += physicsEntity.ActualGravity.ToNumerics() * delta;
            float linDamp = physicsEntity.LinearDamping + physicsEntity.LinearDampingBoost;
            float angDamp = physicsEntity.AngularDamping + physicsEntity.AngularDampingBoost;
            float linearDampingDt = MathF.Pow(1 - linDamp, delta);
            float angularDampingDt = MathF.Pow(1 - angDamp, delta);
            velLinear *= linearDampingDt;
            velAngular *= angularDampingDt;
            Vector3Wide.WriteSlot(velLinear, i, ref velocity.Linear);
            Vector3Wide.WriteSlot(velAngular, i, ref velocity.Angular);
            physicsEntity.LinearDampingBoost = 0;
            physicsEntity.AngularDampingBoost = 0;
        }
    }
}

/// <summary>Implementation for <see cref="INarrowPhaseCallbacks"/>.</summary>
public struct BepuNarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    /// <summary>The backing FGE physics space.</summary>
    public PhysicsSpace Space;

    /// <summary>Defines the default constraint's penetration recovery spring properties.</summary>
    public SpringSettings ContactSpringiness;

    /// <summary>Minimum recovery velocity when bouncing is insufficient.</summary>
    public float MinimumRecoveryVelocity;

    /// <inheritdoc/>
    public void Initialize(Simulation simulation)
    {
        if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
        {
            // TODO: ?
            ContactSpringiness = new SpringSettings(30, 0.5f);
        }
        if (MinimumRecoveryVelocity == 0)
        {
            MinimumRecoveryVelocity = 2;
        }
        Space.Internal.Characters.Initialize(simulation);
    }

    /// <summary>Helper to get a <see cref="EntityPhysicsProperty"/> from a <see cref="CollidableReference"/>, or null.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly EntityPhysicsProperty PhysPropForCollidable(CollidableReference collidable)
    {
        return collidable.Mobility == CollidableMobility.Dynamic ? Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value] : null;
    }

    /// <inheritdoc/>
    public readonly bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
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

    /// <inheritdoc/>
    public readonly bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    /// <inheritdoc/>
    public readonly bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial.SpringSettings = ContactSpringiness;
        EntityPhysicsProperty aEntity = PhysPropForCollidable(pair.A);
        EntityPhysicsProperty bEntity = PhysPropForCollidable(pair.B);
        Vector3 avgNorm = Vector3.Zero;
        for (int i = 0; i < manifold.Count; i++)
        {
            avgNorm += manifold.GetNormal(i);
        }
        avgNorm /= manifold.Count;
        if (aEntity is null || bEntity is null)
        {
            EntityPhysicsProperty validOne = aEntity ?? bEntity;
            if (validOne is not null)
            {
                pairMaterial.FrictionCoefficient = validOne.Friction * validOne.Friction;
                // TODO: Sustained contacts might not re-call this method, ie the values here will not be kept accurate
                float projectedVel = Math.Abs(Vector3.Dot(validOne.LinearVelocity.ToNumerics(), avgNorm));
                pairMaterial.MaximumRecoveryVelocity = Math.Max(MinimumRecoveryVelocity + validOne.Bounciness * 4, validOne.Bounciness * projectedVel);
                pairMaterial.SpringSettings.TwiceDampingRatio = validOne.RecoveryDamping;
            }
            else
            {
                pairMaterial.FrictionCoefficient = 1;
                pairMaterial.MaximumRecoveryVelocity = MinimumRecoveryVelocity;
            }
        }
        else
        {
            pairMaterial.FrictionCoefficient = aEntity.Friction * bEntity.Friction;
            float projectedVelA = Math.Abs(Vector3.Dot(aEntity.LinearVelocity.ToNumerics(), avgNorm));
            float projectedVelB = Math.Abs(Vector3.Dot(bEntity.LinearVelocity.ToNumerics(), avgNorm));
            float bounce = aEntity.Bounciness + bEntity.Bounciness;
            pairMaterial.MaximumRecoveryVelocity = Math.Max(MinimumRecoveryVelocity + bounce * 4, bounce * (projectedVelA + projectedVelB));
            pairMaterial.SpringSettings.TwiceDampingRatio = (aEntity.RecoveryDamping + bEntity.RecoveryDamping) * 0.5f;
        }
        if (aEntity?.CollisionHandler is not null || bEntity?.CollisionHandler is not null || Space.CollisionHandler is not null)
        {
            CollisionEvent evt = new CollisionEvent<TManifold>() { One = aEntity, Two = bEntity, Manifold = manifold };
            Space.CollisionHandler?.Invoke(evt);
            if (evt.Cancel)
            {
                return false;
            }
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

    /// <inheritdoc/>
    public readonly bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        return true;
    }

    /// <inheritdoc/>
    public readonly void Dispose()
    {
    }
}
