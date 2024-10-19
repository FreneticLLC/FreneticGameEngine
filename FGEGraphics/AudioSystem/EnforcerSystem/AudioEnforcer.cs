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
    public Thread AudioThread;

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

    /// <summary>The speed of sound, in units per second (on Earth in air this is 343 m/s).</summary>
    public float SpeedOfSound = 343;

    /// <summary>How far apart the ears are.</summary>
    public float HeadWidth = 0.2f; // TODO: SoundEngine control of this var

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
            Internal.ReusableBuffers[i] = new byte[InternalData.BYTES_PER_BUFFER];
        }
        Internal.Context = acontext;
        Run = true;
        AudioThread = new Thread(new ThreadStart(Internal.ForceAudioLoop))
        {
            Name = "Audio_" + Interlocked.Increment(ref AudioID)
        };
        AudioThread.Start();
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

        /// <summary>The audio frequency, in Hz (samples per second).</summary>
        public const int FREQUENCY = 44100;

        /// <summary>Audio channels to use.</summary>
        public const int CHANNELS = 2;

        /// <summary>Audio byte-rate to use.</summary>
        public const int BYTERATE = 2;

        /// <summary>Multiplying an audio sample by this lowers its volume by 3 dB.</summary>
        public const float MINUS_THREE_DB = 0.707106781f;

        /// <summary>The maximum pause, in milliseconds, between audio crunching passes.</summary>
        public const int PAUSE = 10;

        /// <summary>How many milliseconds to load at once in any one buffer.</summary>
        public const int MS_LOAD = 33;

        /// <summary>Nubmer of audio buffers to use at the same time.</summary>
        public const int BUFFERS_AT_ONCE = 2;

        /// <summary>How many buffers are in <see cref="ReusableBuffers"/>.</summary>
        public const int REUSABLE_BUFFER_ARRAY_SIZE = 10;

        /// <summary>Actual byte space to load at once.</summary>
        public const int BYTES_PER_BUFFER = (int)((FREQUENCY * MS_LOAD) / 1000.0) * CHANNELS * BYTERATE;

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
                toReturn[i] = 0;
            }
            return toReturn;
        }

        /// <summary>Contains data about how audio sounds relative to a specific ear.</summary>
        /// <param name="Volume">The volume to this ear.</param>
        /// <param name="TimeOffset">The time offset to this ear, in samples. This is usually a negative number, to indicate the position in the clip should be earlier, ie the playback of real audio should come later.</param>
        public record struct AudioPositionalEarData(float Volume, int TimeOffset)
        {
            /// <summary>Fills the data for this ear based on relevant audio data.</summary>
            public void Fill(Location soundPosition, Location earPosition, float angleVolume, AudioEnforcer instance)
            {
                float distSq = (float)soundPosition.DistanceSquared(earPosition);
                float dist = MathF.Sqrt(distSq);
                float distanceGain = 1.0f / Math.Max(1.0f, distSq);
                Volume = angleVolume * distanceGain;
                float timeOffsetSeconds = -dist / instance.SpeedOfSound;
                TimeOffset = (int)(timeOffsetSeconds * FREQUENCY);
            }
        }

        /// <summary>Calculates the correct positional audio data for each ear for a position based on distance (using inverse-square-root) and direction (using trigonometry). Returns as (left, right).</summary>
        public readonly (AudioPositionalEarData, AudioPositionalEarData) GetPositionalData(Location position)
        {
            Location relativeDirectionVector = (position - Instance.Position).Normalize();
            Quaternion direction = Quaternion.GetQuaternionBetween(relativeDirectionVector, Instance.ForwardDirection);
            float angle = (float)direction.AxisAngleForRadians(Instance.UpDirection);
            Location rightDirection = Instance.ForwardDirection.CrossProduct(Instance.UpDirection);
            AudioPositionalEarData left = new(), right = new();
            if (Instance.Left)
            {
                Location leftEarPos = Instance.Position - rightDirection * (Instance.HeadWidth * 0.5);
                float angleVolume = Math.Max(0, (float)Math.Cos(angle + QUARTER_PI)) * 0.5f + 0.5f;
                left.Fill(position, leftEarPos, angleVolume, Instance);
            }
            if (Instance.Right)
            {
                Location rightEarPos = Instance.Position + rightDirection * (Instance.HeadWidth * 0.5);
                float angleVolume = Math.Max(0, (float)Math.Sin(angle + QUARTER_PI)) * 0.5f + 0.5f;
                right.Fill(position, rightEarPos, angleVolume, Instance);
            }
            return (left, right);
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
            // TODO: Need to track the actual change in position for each ear between frames, divided by frametime, and apply a shift effect to match.
            // TODO: So eg if a player whips their head 180 degrees in one frame, the audio should have a natural effect from that rather than glitch jumping.
            // TODO: Note to make sure that accounts reasonably for teleports (ie don't go wild at the frame of teleportation).
            // TODO: Note as well the current ear velocity should be additive with the sound velocity.
            AudioPositionalEarData leftEar = new(1, 0), rightEar = new(1, 0);
            if (toAdd.UsePosition)
            {
                (leftEar, rightEar) = GetPositionalData(toAdd.Position);
            }
            float gain = toAdd.Gain * Instance.Volume;
            gain *= gain;
            int volumeModifierRight = (int)((rightEar.Volume * gain) * ushort.MaxValue);
            int volumeModifierLeft = (int)((leftEar.Volume * gain) * ushort.MaxValue);
            byte[] clipData = toAdd.Clip.Data;
            int maxBytePositionLeft = BYTES_PER_BUFFER;
            int maxBytePositionRight = BYTES_PER_BUFFER;
            if (!toAdd.Loop)
            {
                maxBytePositionLeft = Math.Min(clipData.Length - (toAdd.CurrentSample + leftEar.TimeOffset * 2), BYTES_PER_BUFFER);
                maxBytePositionRight = Math.Min(clipData.Length - (toAdd.CurrentSample + rightEar.TimeOffset * 2), BYTES_PER_BUFFER);
            }
            int maxBytePosition = Math.Max(maxBytePositionLeft, maxBytePositionRight);
            while (outBufPosition < maxBytePosition && outBufPosition + 3 < BYTES_PER_BUFFER)
            {
                // TODO: pitch, velocity, etc.?
                int rawSampleInLeft = 0;
                if (outBufPosition < maxBytePositionLeft)
                {
                    int leftSample = toAdd.CurrentSample + leftEar.TimeOffset * 2;
                    if (toAdd.Loop)
                    {
                        leftSample %= clipData.Length;
                        if (leftSample < 0)
                        {
                            leftSample += clipData.Length;
                        }
                    }
                    if (leftSample >= 0 && leftSample + 1 < clipData.Length)
                    {
                        rawSampleInLeft = (short)((clipData[leftSample + 1] << 8) | clipData[leftSample]);
                        int outSampleLeft = (rawSampleInLeft * volumeModifierLeft) >> 16;
                        int rawPreValueLeft = (short)((outBuffer[outBufPosition + 1] << 8) | outBuffer[outBufPosition]);
                        outSampleLeft += rawPreValueLeft; // TODO: Better scaled adder?
                        outSampleLeft = Math.Max(short.MinValue, Math.Min(short.MaxValue, outSampleLeft));
                        outBuffer[outBufPosition] = (byte)outSampleLeft;
                        outBuffer[outBufPosition + 1] = (byte)(outSampleLeft >> 8);
                    }
                }
                outBufPosition += 2;
                if (outBufPosition < maxBytePositionRight)
                {
                    int outSampleRight = 0;
                    if (toAdd.Clip.Channels == 2)
                    {
                        toAdd.CurrentSample += 2;
                        int rightSample = toAdd.CurrentSample + rightEar.TimeOffset * 2;
                        if (toAdd.Loop)
                        {
                            rightSample %= clipData.Length;
                            if (rightSample < 0)
                            {
                                rightSample += clipData.Length;
                            }
                        }
                        if (rightSample >= 0 && rightSample + 1 < clipData.Length)
                        {
                            int rawSampleInRight = (short)((clipData[rightSample + 1] << 8) | clipData[rightSample]);
                            outSampleRight = (rawSampleInRight * volumeModifierRight) >> 16;
                        }
                    }
                    else
                    {
                        outSampleRight = (rawSampleInLeft * volumeModifierRight) >> 16;
                    }
                    int rawPreValueRight = (short)((outBuffer[outBufPosition + 1] << 8) | outBuffer[outBufPosition]);
                    outSampleRight += rawPreValueRight; // TODO: Better scaled adder?
                    outBuffer[outBufPosition] = (byte)outSampleRight;
                    outBuffer[outBufPosition + 1] = (byte)(outSampleRight >> 8);
                }
                toAdd.CurrentSample += 2;
                if (toAdd.Loop)
                {
                    toAdd.CurrentSample %= clipData.Length;
                }
                outBufPosition += 2;
            }
            if (!toAdd.Loop && toAdd.CurrentSample + Math.Max(leftEar.TimeOffset, rightEar.TimeOffset) * 2 >= clipData.Length)
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
