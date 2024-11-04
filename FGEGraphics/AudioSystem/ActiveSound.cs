//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
using FGEGraphics.AudioSystem.EnforcerSystem;

namespace FGEGraphics.AudioSystem;

/// <summary>Represents a currently playing sound effect.</summary>
/// <param name="sfx">The backing sound effect.</param>
public class ActiveSound(SoundEffect sfx)
{
    /// <summary>The backing sound engine.</summary>
    public SoundEngine Engine;

    /// <summary>The represented sound effect.</summary>
    public SoundEffect Effect = sfx;

    /// <summary>The 3D space position of the sound effect.</summary>
    public Location Position;

    /// <summary>Whether to loop the sound.</summary>
    public bool Loop = false;

    /// <summary>The pitch of the sound.</summary>
    public float Pitch = 1f;

    /// <summary>The gain of the sound.</summary>
    public float Gain = 1f;

    /// <summary>The internal enforcer instance, if relevant.</summary>
    public LiveAudioInstance AudioInternal = null;

    /// <summary>Whether the effect exists already in the backing systems.</summary>
    public bool Exists = false;

    /// <summary>Whether the sound effect is considered a background effect.</summary>
    public bool IsBackground = false;

    /// <summary>Whether the sound effect has been forced into the background and quieted due to user focus need.</summary>
    public bool Backgrounded = false;

    /// <summary>Creates the sound into the backing engines.</summary>
    public void Create()
    {
        if (!Exists)
        {
            AudioInternal = new LiveAudioInstance()
            {
                Clip = Effect.Clip,
                Gain = Gain,
                Loop = Loop,
                Pitch = Pitch,
                Position = Position,
                Velocity = Location.Zero, // TODO: Velocity!
                                          // TODO: Direction?
                UsePosition = !Position.IsNaN()
            };
            Exists = true;
        }
    }

    /// <summary>Updates the pitch to the backing engine.</summary>
    public void UpdatePitch()
    {
        AudioInternal.Pitch = Pitch;
    }

    /// <summary>Whether the audio has been deafened.</summary>
    public bool IsDeafened = false;

    /// <summary>Updates the gain to the backing systems.</summary>
    public void UpdateGain()
    {
        bool sel = Engine.Selected;
        if (sel)
        {
            AudioInternal.Gain = Gain;
            Backgrounded = false;
        }
        else
        {
            AudioInternal.Gain = 0.0001f;
            Backgrounded = true;
        }
    }

    /// <summary>Plays the audio.</summary>
    public void Play()
    {
        Engine.EnforcerInternal.Add(this);
    }

    /// <summary>Seeks to a location in the clip (From 0.0 to 1.0).</summary>
    /// <param name="f">The location.</param>
    public void Seek(float f)
    {
        int samp = (int)(AudioInternal.Clip.Data.Length * f);
        AudioInternal.CurrentSample = samp - samp % 4;
    }

    /// <summary>Pauses the audio.</summary>
    public void Pause()
    {
        AudioInternal.State = AudioState.PAUSED;
    }

    /// <summary>Stops the audio from playing further.</summary>
    public void Stop()
    {
        AudioInternal.State = AudioState.STOP;
    }

    /// <summary>Returns whether the audio is currently playing.</summary>
    /// <returns>Whether it is playing.</returns>
    public bool IsPlaying()
    {
        return AudioInternal.State == AudioState.PLAYING;
    }

    /// <summary>Destroys the audio instance.</summary>
    public void Destroy()
    {
        AudioInternal.State = AudioState.STOP;
    }
}
