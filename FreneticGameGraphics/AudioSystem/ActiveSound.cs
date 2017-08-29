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
using FreneticGameGraphics.AudioSystem.EnforcerSystem;
using FreneticGameCore;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace FreneticGameGraphics.AudioSystem
{
    /// <summary>
    /// Represents a currently playing sound effect.
    /// </summary>
    public class ActiveSound
    {
        /// <summary>
        /// The backing sound engine.
        /// </summary>
        public SoundEngine Engine;

        /// <summary>
        /// The represented sound effect.
        /// </summary>
        public SoundEffect Effect;

        /// <summary>
        /// The 3D space position of the sound effect.
        /// </summary>
        public Location Position;

        /// <summary>
        /// Constructs the active sound.
        /// </summary>
        /// <param name="sfx">The backing sound effect.</param>
        public ActiveSound(SoundEffect sfx)
        {
            Effect = sfx;
        }

        /// <summary>
        /// Whether to loop the sound.
        /// </summary>
        public bool Loop = false;

        /// <summary>
        /// The pitch of the sound.
        /// </summary>
        public float Pitch = 1f;

        /// <summary>
        /// The gain of the sound.
        /// </summary>
        public float Gain = 1f;

        /// <summary>
        /// The OpenAL source value, if relevant.
        /// </summary>
        public int Src = -1;

        /// <summary>
        /// The internal enforcer instance, if relevant.
        /// </summary>
        public LiveAudioInstance AudioInternal = null;

        /// <summary>
        /// Whether the effect exists already in the backing systems.
        /// </summary>
        public bool Exists = false;

        /// <summary>
        /// Whether the sound effect is considered a background effect.
        /// </summary>
        public bool IsBackground = false;

        /// <summary>
        /// Whether the sound effect has been forced into the background and quieted due to user focus need.
        /// </summary>
        public bool Backgrounded = false;

        /// <summary>
        /// Creates the sound into the backing engines.
        /// </summary>
        public void Create()
        {
            if (!Exists)
            {
                if (Engine.AudioInternal != null)
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
                }
                else
                {
                    Engine.CheckError("PRECREATE:" + Effect.Name);
                    Src = AL.GenSource();
                    if (Src < 0 || AL.GetError() != ALError.NoError)
                    {
                        Src = -1;
                        return;
                    }
                    AL.Source(Src, ALSourcei.Buffer, Effect.Internal);
                    AL.Source(Src, ALSourceb.Looping, Loop);
                    Engine.CheckError("Preconfig:" + Effect.Name);
                    if (Pitch != 1f)
                    {
                        UpdatePitch();
                    }
                    if (Gain != 1f)
                    {
                        UpdateGain();
                    }
                    Engine.CheckError("GP:" + Effect.Name);
                    if (!Position.IsNaN())
                    {
                        Vector3 zero = Vector3.Zero;
                        Vector3 vec = Position.ToOpenTK();
                        AL.Source(Src, ALSource3f.Direction, ref zero);
                        AL.Source(Src, ALSource3f.Velocity, ref zero);
                        AL.Source(Src, ALSource3f.Position, ref vec);
                        AL.Source(Src, ALSourceb.SourceRelative, false);
                        AL.Source(Src, ALSourcef.EfxAirAbsorptionFactor, 1f);
                        Engine.CheckError("Positioning:" + Effect.Name);
                    }
                    else
                    {
                        AL.Source(Src, ALSourceb.SourceRelative, true);
                        Engine.CheckError("Relative:" + Effect.Name);
                    }
                }
                Exists = true;
            }
        }

        /// <summary>
        /// Updates the pitch to the backing engine.
        /// </summary>
        public void UpdatePitch()
        {
            if (Engine.AudioInternal != null)
            {
                AudioInternal.Pitch = Pitch;
            }
            else
            {
                AL.Source(Src, ALSourcef.Pitch, Pitch);
            }
        }

        /// <summary>
        /// Whether the audio has been deafened.
        /// </summary>
        public bool IsDeafened = false;

        /// <summary>
        /// Updates the gain to the backing systems.
        /// </summary>
        public void UpdateGain()
        {
            bool sel = Engine.Selected;
            if (Engine.AudioInternal != null)
            {
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
            else
            {
                if (sel)
                {
                    AL.Source(Src, ALSourcef.Gain, Gain);
                    Backgrounded = false;
                }
                else
                {
                    AL.Source(Src, ALSourcef.Gain, 0.0001f);
                    Backgrounded = true;
                }
            }
        }

        /// <summary>
        /// Plays the audio.
        /// </summary>
        public void Play()
        {
            if (Engine.AudioInternal != null)
            {
                Engine.AudioInternal.Add(AudioInternal);
            }
            else
            {
                if (Src < 0)
                {
                    return;
                }
                AL.SourcePlay(Src);
            }
        }

        /// <summary>
        /// Seeks to a location in the clip (From 0.0 to 1.0).
        /// </summary>
        /// <param name="f">The location.</param>
        public void Seek(float f)
        {
            if (Engine.AudioInternal != null)
            {
                int samp = (int)(AudioInternal.Clip.Data.Length * f);
                AudioInternal.CurrentSample = samp - samp % 4;
                return;
            }
            // TODO: Enforcer!
            if (Src < 0)
            {
                return;
            }
            AL.Source(Src, ALSourcef.SecOffset, f);
        }

        /// <summary>
        /// Pauses the audio.
        /// </summary>
        public void Pause()
        {
            if (Engine.AudioInternal != null)
            {
                AudioInternal.State = AudioState.PAUSED;
            }
            else
            {
                if (Src < 0)
                {
                    return;
                }
                AL.SourcePause(Src);
            }
        }

        /// <summary>
        /// Stops the audio from playing further.
        /// </summary>
        public void Stop()
        {
            if (Engine.AudioInternal != null)
            {
                AudioInternal.State = AudioState.STOP;
            }
            else
            {
                if (Src < 0)
                {
                    return;
                }
                AL.SourceStop(Src);
            }
        }

        /// <summary>
        /// Returns whether the audio is currently playing.
        /// </summary>
        /// <returns>Whether it is playing.</returns>
        public bool IsPlaying()
        {
            if (Engine.AudioInternal != null)
            {
                return AudioInternal.State == AudioState.PLAYING;
            }
            else
            {
                if (Src < 0)
                {
                    return false;
                }
                return (AL.GetSourceState(Src) == ALSourceState.Playing);
            }
        }

        /// <summary>
        /// Destroys the audio instance.
        /// </summary>
        public void Destroy()
        {
            if (Engine.AudioInternal != null)
            {
                AudioInternal.State = AudioState.STOP;
                return;
            }
            if (Src < 0)
            {
                return;
            }
            if (Exists)
            {
                AL.DeleteSource(Src);
                Exists = false;
            }
        }
    }
}
