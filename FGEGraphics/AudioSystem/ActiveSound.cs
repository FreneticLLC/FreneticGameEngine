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
using FGEGraphics.AudioSystem.AudioInternals;

namespace FGEGraphics.AudioSystem;

/// <summary>Represents a currently playing sound effect.</summary>
public class ActiveSound
{
    /// <summary>The backing sound engine.</summary>
    public SoundEngine Engine;

    /// <summary>The represented sound effect.</summary>
    public SoundEffect Effect;

    /// <summary>Internal data for <see cref="ActiveSound"/>.</summary>
    public struct InternalData()
    {
        /// <summary>If true, this sound effect has unsynced modifications.</summary>
        public bool Modified = false;

        /// <summary>The 3D space position of the sound effect.</summary>
        public Location Position = Location.Zero;

        /// <summary>The 3D space position of the sound effect.</summary>
        public Location Velocity = Location.Zero;

        /// <summary>Whether to loop the sound.</summary>
        public bool Loop = false;

        /// <summary>The pitch of the sound.</summary>
        public float Pitch = 1;

        /// <summary>The gain of the sound.</summary>
        public float Gain = 1;

        /// <summary>Whether the sound effect has been forced into the background and quieted due to user focus need.</summary>
        public bool Backgrounded = false;

        /// <summary>The internal audio engine instance, if relevant.</summary>
        public LiveAudioInstance AudioInternal;

        /// <summary>The current state of the audio.</summary>
        public AudioState State = AudioState.WAITING;

        /// <summary>A wanted change to the audio state.</summary>
        public AudioState ForceState = (AudioState)255;

        /// <summary>A desired seek index.</summary>
        public int Seek = -1;

        /// <summary>Sync the data copy to the backing engine, if needed.</summary>
        public void Sync()
        {
            if (!Modified)
            {
                return;
            }
            Modified = false;
            float gain = Backgrounded ? 0.0001f : Gain;
            AudioInternal.Engine.UpdatesToSync.Enqueue(new(AudioInternal, Position, Velocity, gain, Pitch, ForceState, Seek, Loop, 0, Location.Zero, Location.Zero, false, false));
            Seek = -1;
            ForceState = (AudioState)255;
        }
    }

    /// <summary>Internal data for <see cref="ActiveSound"/>.</summary>
    public InternalData Internal = new();

    /// <summary>The 3D space position of the sound effect.</summary>
    public Location Position
    {
        get => Internal.Position;
        set => Internal.Position = value;
    }

    /// <summary>The 3D space position of the sound effect.</summary>
    public Location Velocity
    {
        get => Internal.Velocity;
        set { Internal.Velocity = value; Internal.Modified = true; }
    }

    /// <summary>Whether to loop the sound.</summary>
    public bool Loop
    {
        get => Internal.Loop;
        set { Internal.Loop = value; Internal.Modified = true; }
    }

    /// <summary>The pitch of the sound.</summary>
    public float Pitch
    {
        get => Internal.Pitch;
        set { Internal.Pitch = value; Internal.Modified = true; }
    }

    /// <summary>The gain of the sound.</summary>
    public float Gain
    {
        get => Internal.Gain;
        set { Internal.Gain = value; Internal.Modified = true; }
    }

    /// <summary>Whether the sound effect is considered a background effect.</summary>
    public bool IsBackground = false;

    /// <summary>Whether the sound effect has been forced into the background and quieted due to user focus need.</summary>
    public bool Backgrounded
    {
        get => Internal.Backgrounded;
        set { Internal.Backgrounded = value; Internal.Modified = true; }
    }

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to <see cref="int.MaxValue"/> to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public int LowPassFrequency
    {
        get => Internal.AudioInternal.LowPassFrequency;
        set => Internal.AudioInternal.LowPassFrequency = value;
    }

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to 0 to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public int HighPassFrequency
    {
        get => Internal.AudioInternal.HighPassFrequency;
        set => Internal.AudioInternal.HighPassFrequency = value;
    }

    /// <summary>How long to delay between reverberations of this sound.</summary>
    public float ReverbDelay
    {
        get => Internal.AudioInternal.ReverbDelay;
        set => Internal.AudioInternal.ReverbDelay = value;
    }

    /// <summary>How many reverberations of this sound to play. Set to 0 to not play any reverb.</summary>
    public int ReverbCount
    {
        get => Internal.AudioInternal.ReverbCount;
        set => Internal.AudioInternal.ReverbCount = value;
    }

    /// <summary>How much each reverberation's volume should drop by the time it reaches the next one. 0.25 means drop volume by a quarter after <see cref="ReverbDelay"/>.</summary>
    public float ReverbDecay
    {
        get => Internal.AudioInternal.ReverbDecay;
        set => Internal.AudioInternal.ReverbDecay = value;
    }

    /// <summary>The number of channels in the sound effect (if it has been loaded, otherwise 0).</summary>
    public byte Channels => Internal.AudioInternal.Clip.Channels;

    /// <summary>Constructs the sound instance.</summary>
    /// <param name="sfx">The backing sound effect.</param>
    /// <param name="engine">The backing sound engine.</param>
    public ActiveSound(SoundEffect sfx, SoundEngine engine)
    {
        Engine = engine;
        Effect = sfx;
        Internal.AudioInternal = new(engine.Internal.AudioEngine) { Clip = sfx };
    }

    /// <summary>Plays the audio.</summary>
    public void Play()
    {
        Internal.State = AudioState.PLAYING;
        Internal.Sync();
        Engine.Internal.AudioEngine.Add(Internal.AudioInternal);
    }

    /// <summary>Seeks to a location in the clip (From 0.0 to 1.0). Not valid if the clip hasn't been loaded yet.</summary>
    /// <param name="f">The location.</param>
    public void Seek(float f)
    {
        int samp = (int)(Internal.AudioInternal.Clip.Data.Length * f);
        Internal.Seek = samp - samp % Channels;
        Internal.Modified = true;
    }

    /// <summary>Pauses the audio.</summary>
    public void Pause()
    {
        Internal.ForceState = AudioState.PAUSED;
        Internal.Modified = true;
    }

    /// <summary>Stops the audio from playing further.</summary>
    public void Stop()
    {
        Internal.ForceState = AudioState.STOP;
        Internal.Modified = true;
    }

    /// <summary>Returns whether the audio is currently playing.</summary>
    /// <returns>Whether it is playing.</returns>
    public bool IsPlaying()
    {
        return Internal.ForceState == AudioState.PLAYING;
    }
}
