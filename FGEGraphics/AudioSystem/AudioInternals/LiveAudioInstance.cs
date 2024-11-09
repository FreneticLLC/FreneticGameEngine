//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using FGECore.MathHelpers;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Represents currently playing audio.</summary>
public class LiveAudioInstance
{
    /// <summary>The clip for this instance.</summary>
    public LiveAudioClip Clip = null;

    /// <summary>Which audio sample this instance is on, as an index in the clip.</summary>
    public int CurrentSample = 0;

    /// <summary>Whether this instance should loop.</summary>
    public bool Loop = false;

    /// <summary>The position of this instance in 3D space.</summary>
    public Location Position = Location.Zero;

    /// <summary>The velocity of this instance in 3D space.</summary>
    public Location Velocity = Location.Zero;

    /// <summary>The gain of this audio instance (volume).</summary>
    public float Gain = 1f;

    /// <summary>The pitch of this audio instance (frequency).</summary>
    public float Pitch = 1f;

    /// <summary>Whether to use the position and velocity value.</summary>
    public bool UsePosition = false;

    /// <summary>The current state of this audio.</summary>
    public AudioState State = AudioState.WAITING;
}
