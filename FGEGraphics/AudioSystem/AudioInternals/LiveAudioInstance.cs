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
using FGECore.MathHelpers;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Represents currently playing audio.</summary>
public class LiveAudioInstance(FGE3DAudioEngine engine)
{
    /// <summary>The clip for this instance.</summary>
    public SoundEffect Clip = null;

    /// <summary>Which audio sample this instance is on, as an index in the clip.</summary>
    public int CurrentSample = 0;

    /// <summary>Whether this instance should loop.</summary>
    public bool Loop = false;

    /// <summary>The position of this instance in 3D space during the previous sound engine internal frame.</summary>
    public Location PriorPosition = Location.Zero;

    /// <summary>The position of this instance in 3D space.</summary>
    public Location Position = Location.Zero;

    /// <summary>The velocity of this instance in 3D space.</summary>
    public Location Velocity = Location.Zero;

    /// <summary>The gain of this audio instance (volume).</summary>
    public float Gain = 1f;

    /// <summary>The pitch of this audio instance (frequency).</summary>
    public float Pitch = 1f;

    /// <summary>How long in seconds to delay between reverberations of this sound.</summary>
    public volatile float ReverbDelay = 0.1f;

    /// <summary>How many reverberations of this sound to play. Set to 0 to not play any reverb.</summary>
    public volatile int ReverbCount = 0;

    /// <summary>How much each reverberation's volume should drop by the time it reaches the next one. 0.25 means drop volume by a quarter after <see cref="ReverbDelay"/>.</summary>
    public volatile float ReverbDecay = 0.5f;

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to <see cref="int.MaxValue"/> to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public volatile int LowPassFrequency = int.MaxValue;

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to 0 to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public volatile int HighPassFrequency = 0;

    /// <summary>Whether to use the position and velocity value.</summary>
    public bool UsePosition = false;

    /// <summary>The current state of this audio.</summary>
    public AudioState State = AudioState.WAITING;

    /// <summary>Backing audio engine.</summary>
    public FGE3DAudioEngine Engine = engine;
}
