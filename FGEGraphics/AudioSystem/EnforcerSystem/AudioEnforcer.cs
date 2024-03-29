//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem;
using OpenTK.Audio.OpenAL;

namespace FGEGraphics.AudioSystem.EnforcerSystem;

/// <summary>The internal engine to crunch audio data and push it to the speakers (when OpenAL's matching subsystem is not playing nice).</summary>
public class AudioEnforcer
{
    /// <summary>How many instances of the enforcer have been created. This value starts at 1 and increments every time an enforcer is launched.</summary>
    public static long AudioID = 1;

    /// <summary>The thread for the enforcer.</summary>
    public Thread AudioForcer;

    /// <summary>Whether the system is running.</summary>
    public bool Run = false;

    /// <summary>The current general volume of the audio enforcer.</summary>
    public float Volume = 0.5f;

    /// <summary>All currently playing audio.</summary>
    public List<LiveAudioInstance> Playing = [];

    /// <summary>Locker for interaction with the enforcer.</summary>
    public LockObject Locker = new();

    /// <summary>3D Position of the audio "camera".</summary>
    public Location Position;

    /// <summary>Forward direction (vector) of the audio "camera".</summary>
    public Location ForwardDirection;

    /// <summary>Up direction (vector) of the audio "camera".</summary>
    public Location UpDirection;

    /// <summary>Whether the left channel is enabled.</summary>
    public bool Left = true;

    /// <summary>Whether the right channel is enabled.</summary>
    public bool Right = true;

    /// <summary>Add an audio instance to the enforcer.</summary>
    /// <param name="inst">The instance to add.</param>
    public void Add(ActiveSound inst)
    {
        lock (Locker)
        {
            if (inst.AudioInternal.State == AudioState.PLAYING)
            {
                return;
            }
            inst.AudioInternal.State = AudioState.PLAYING;
            Playing.Add(inst.AudioInternal);
        }
    }

    /// <summary>Initialize and load the enforcer.</summary>
    /// <param name="acontext">The backing OpenAL context.</param>
    public void Init(ALContext acontext)
    {
        Internal.Instance = this;
        Internal.ReusableBuffers = new byte[InternalData.REUSABLE_BUFFER_ARRAY_SIZE][];
        for (int i = 0; i < 10; i++)
        {
            Internal.ReusableBuffers[i] = new byte[InternalData.ACTUAL_SAMPLES];
        }
        Internal.Context = acontext;
        Run = true;
        AudioForcer = new Thread(new ThreadStart(Internal.ForceAudioLoop))
        {
            Name = "Audio_" + Interlocked.Increment(ref AudioID)
        };
        AudioForcer.Start();
    }

    /// <summary>Shuts down the enforcer. May take a moment before the enforcer thread stops.</summary>
    public void Shutdown()
    {
        Run = false;
    }

    /// <summary>Current level (of audio) locker.</summary>
    public LockObject CLelLock = new();

    /// <summary>Current audio levels. Use <see cref="CLelLock"/>.</summary>
    public float CurrentLevel = 0.0f;

    /// <summary>Internal data used by the enforcer.</summary>
    public struct InternalData
    {
        /// <summary>The relevant backing enforcer instance.</summary>
        public AudioEnforcer Instance;

        /// <summary>One quarter of PI. A constant.</summary>
        public const float QUARTER_PI = (float)Math.PI * 0.25f;

        /// <summary>The audio frequency, in Hz.</summary>
        public const int FREQUENCY = 44100;

        /// <summary>Audio channels to use.</summary>
        public const int CHANNELS = 2;

        /// <summary>Audio byte-rate to use.</summary>
        public const int BYTERATE = 2;

        /// <summary>Multiplying an audio sample by this lowers its volume by 3 dB.</summary>
        public const float MINUS_THREE_DB = 0.707106781f;

        /// <summary>The maximum pause, in milliseconds, between audio crunching passes.</summary>
        public const int PAUSE = 10;

        /// <summary>How many samples to load at once.</summary>
        public const int SAMPLE_LOAD = 33;

        /// <summary>Nubmer of audio buffers to use at the same time.</summary>
        public const int BUFFERS_AT_ONCE = 2;

        /// <summary>How many buffers are in <see cref="ReusableBuffers"/>.</summary>
        public const int REUSABLE_BUFFER_ARRAY_SIZE = 10;

        /// <summary>Actual byte space to load at once.</summary>
        public const int ACTUAL_SAMPLES = (int)((FREQUENCY * SAMPLE_LOAD) / 1000.0) * CHANNELS * BYTERATE;

        /// <summary>Relevant OpenAL audio context.</summary>
        public ALContext Context;

        /// <summary>A queue of byte arrays to reuse as audio buffers. Buffers are generated once and kept for the lifetime of the enforcer to prevent GC thrash.</summary>
        public byte[][] ReusableBuffers;

        /// <summary>The index in <see cref="ReusableBuffers"/> to next use.</summary>
        public int ByteBufferID;

        /// <summary>The OpenAL audio source ID.</summary>
        public int ALAudioSource;

        /// <summary>Cached reusable list of dead audio instances.</summary>
        public List<LiveAudioInstance> DeadInstances;

        /// <summary>Queue of reusable buffer IDs.</summary>
        public Queue<int> UsableBufferIDs;

        /// <summary>Gets and cleans the next byte buffer to use.</summary>
        public byte[] GetNextBuffer()
        {
            byte[] toReturn = ReusableBuffers[ByteBufferID++];
            ByteBufferID %= REUSABLE_BUFFER_ARRAY_SIZE;
            for (int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = 0; // TODO: Is 0 correct for a no-audio buffer?
            }
            return toReturn;
        }

        /// <summary>Calculates the correct positional audio volume for a position based on distance (using inverse-square-root) and direction (using trigonometry). Returns as (left, right).</summary>
        public readonly (float, float) GetPositionalVolume(Location position)
        {
            float distanceGain = 1.0f / Math.Max(1.0f, (float)position.DistanceSquared(Instance.Position));
            Location relativeDirectionVector = (position - Instance.Position).Normalize();
            Quaternion direction = Quaternion.GetQuaternionBetween(relativeDirectionVector, Instance.ForwardDirection);
            float angle = (float)direction.AxisAngleForRadians(Instance.UpDirection);
            float volumeRight = Instance.Right ? Math.Max(0f, (float)Math.Sin(angle + QUARTER_PI)) : 0f;
            float volumeLeft = Instance.Left ? Math.Max(0f, (float)Math.Cos(angle + QUARTER_PI)) : 0f;
            volumeRight *= volumeRight * distanceGain;
            volumeLeft *= volumeLeft * distanceGain;
            return (volumeLeft, volumeRight);
        }

        /// <summary>Completely closes and stops the audio enforcer system.</summary>
        public void CloseAndStop()
        {
            if ((ALSourceState)AL.GetSource(ALAudioSource, ALGetSourcei.SourceState) == ALSourceState.Playing)
            {
                AL.SourceStop(ALAudioSource);
            }
            if (Context.Handle != IntPtr.Zero)
            {
                ALC.DestroyContext(Context);
            }
            Context = new ALContext(IntPtr.Zero);
        }

        /// <summary>Adds a single audio instance to a raw playback buffer, without losing pre-existing audio data in the buffer.</summary>
        public readonly void AddClipToBuffer(byte[] outBuffer, LiveAudioInstance toAdd)
        {
            int outBufPosition = 0;
            float volumeLeft = 1f, volumeRight = 1f;
            if (toAdd.UsePosition)
            {
                (volumeLeft, volumeRight) = GetPositionalVolume(toAdd.Position);
            }
            float gain = toAdd.Gain * Instance.Volume;
            gain *= gain;
            int volumeModifierRight = (int)((volumeRight * gain) * ushort.MaxValue);
            int volumeModifierLeft = (int)((volumeLeft * gain) * ushort.MaxValue);
            byte[] clipData = toAdd.Clip.Data;
            int maxBytePosition = toAdd.Loop ? ACTUAL_SAMPLES : Math.Min(clipData.Length - toAdd.CurrentSample, ACTUAL_SAMPLES);
            while (outBufPosition < maxBytePosition && outBufPosition + 3 < ACTUAL_SAMPLES)
            {
                // TODO: pitch, velocity, etc.?
                int rawSampleInLeft = (short)((clipData[toAdd.CurrentSample + 1] << 8) | clipData[toAdd.CurrentSample]);
                int outSampleLeft = (rawSampleInLeft * volumeModifierLeft) >> 16;
                int rawPreValueLeft = (short)((outBuffer[outBufPosition + 1] << 8) | outBuffer[outBufPosition]);
                outSampleLeft += rawPreValueLeft; // TODO: Better scaled adder?
                outSampleLeft = Math.Max(short.MinValue, Math.Min(short.MaxValue, outSampleLeft));
                outBuffer[outBufPosition] = (byte)outSampleLeft;
                outBuffer[outBufPosition + 1] = (byte)(outSampleLeft >> 8);
                outBufPosition += 2;
                int outSampleRight;
                if (toAdd.Clip.Channels == 2)
                {
                    toAdd.CurrentSample += 2;
                    int rawSampleInRight = (short)((clipData[toAdd.CurrentSample + 1] << 8) | clipData[toAdd.CurrentSample]);
                    outSampleRight = (rawSampleInRight * volumeModifierRight) >> 16;
                }
                else
                {
                    outSampleRight = (rawSampleInLeft * volumeModifierRight) >> 16;
                }
                int rawPreValueRight = (short)((outBuffer[outBufPosition + 1] << 8) | outBuffer[outBufPosition]);
                outSampleRight += rawPreValueRight; // TODO: Better scaled adder?
                outBuffer[outBufPosition] = (byte)outSampleRight;
                outBuffer[outBufPosition + 1] = (byte)(outSampleRight >> 8);
                toAdd.CurrentSample += 2;
                if (toAdd.Loop)
                {
                    toAdd.CurrentSample %= clipData.Length;
                }
                outBufPosition += 2;
            }
            if (toAdd.CurrentSample >= clipData.Length)
            {
                toAdd.CurrentSample = 0;
                toAdd.State = AudioState.DONE;
                DeadInstances.Add(toAdd);
            }
        }

        /// <summary>Calculates the audio level of a raw audio buffer.</summary>
        public readonly float GetLevelFor(byte[] buffer)
        {
            float level = 0.0f;
            for (int i = 0; i < buffer.Length; i += BYTERATE)
            {
                int val = buffer[i] | (buffer[i + 1] << 8);
                float tval = val / (float)ushort.MaxValue;
                level += tval;
            }
            level /= (buffer.Length / 2) * Instance.Volume;
            return level;
        }

        /// <summary>Causes a single buffer of audio to be added to the live playing audio in OpenAL. Also ensures the enforcer is playing in OpenAL at all.</summary>
        public readonly void PlayBuffer(byte[] buffer)
        {
            int bufferID = UsableBufferIDs.Count > 0 ? UsableBufferIDs.Dequeue() : AL.GenBuffer();
            AL.BufferData(bufferID, ALFormat.Stereo16, buffer, FREQUENCY);
            AL.SourceQueueBuffer(ALAudioSource, bufferID);
            if ((ALSourceState)AL.GetSource(ALAudioSource, ALGetSourcei.SourceState) != ALSourceState.Playing)
            {
                AL.SourcePlay(ALAudioSource);
            }
        }

        /// <summary>The internal audio enforcer loop.</summary>
        public void ForceAudioLoop()
        {
            try
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                ALC.MakeContextCurrent(Context);
                ALAudioSource = AL.GenSource();
                AL.Source(ALAudioSource, ALSourceb.Looping, false);
                AL.Source(ALAudioSource, ALSourceb.SourceRelative, true);
                UsableBufferIDs = new Queue<int>();
                DeadInstances = [];
                while (true)
                {
                    if (!Instance.Run)
                    {
                        CloseAndStop();
                        return;
                    }
                    stopwatch.Stop();
                    double elSec = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
                    stopwatch.Reset();
                    stopwatch.Start();
                    AL.GetSource(ALAudioSource, ALGetSourcei.BuffersProcessed, out int buffersDone);
                    while (buffersDone > 0)
                    {
                        int bufferID = AL.SourceUnqueueBuffer(ALAudioSource);
                        UsableBufferIDs.Enqueue(bufferID);
                        buffersDone--;
                    }
                    AL.GetSource(ALAudioSource, ALGetSourcei.BuffersQueued, out int waiting);
                    if (waiting < BUFFERS_AT_ONCE)
                    {
                        byte[] buffer = GetNextBuffer();
                        lock (Instance.Locker)
                        {
                            foreach (LiveAudioInstance audio in Instance.Playing)
                            {
                                if (audio.State == AudioState.PLAYING)
                                {
                                    AddClipToBuffer(buffer, audio);
                                }
                                else if (audio.State == AudioState.STOP || audio.State == AudioState.DONE)
                                {
                                    DeadInstances.Add(audio);
                                }
                            }
                            foreach (LiveAudioInstance inst in DeadInstances)
                            {
                                Instance.Playing.Remove(inst);
                            }
                            DeadInstances.Clear();
                        }
                        float newCurrentLevel = GetLevelFor(buffer);
                        lock (Instance.CLelLock)
                        {
                            Instance.CurrentLevel = newCurrentLevel;
                        }
                        PlayBuffer(buffer);
                    }
                    int ms = PAUSE - (int)(elSec * 1000.0);
                    if (ms > 0)
                    {
                        Thread.Sleep(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output("Handling audio enforcer", ex);
            }
        }
    }

    /// <summary>Internal data used by the enforcer.</summary>
    public InternalData Internal;
}
