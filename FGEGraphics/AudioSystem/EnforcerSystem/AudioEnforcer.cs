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
using FreneticUtilities.FreneticExtensions;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using OpenTK.Audio.OpenAL;

namespace FGEGraphics.AudioSystem.EnforcerSystem;

/// <summary>The internal engine to crunch audio data and push it to the speakers (when OpenAL's matching subsystem is not playing nice).</summary>
public class AudioEnforcer
{
    /// <summary>Constant value of the approximate speed of sound in air on Earth, 343 meters per second.</summary>
    public const float SPEED_OF_SOUND = 343;

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

    /// <summary>The previous frame's position of the audio "camera".</summary>
    public Location PreviousPosition;

    /// <summary>The current velocity vector of the audio "camera".</summary>
    public Location CurrentVelocity;

    /// <summary>The time between the previous update frame and now as a decimal number of seconds, ie the time that transpired between <see cref="PreviousPosition"/> and <see cref="Position"/>.</summary>
    public double FrameTime;

    /// <summary>If the audio "camera" moves faster than this many units per second, presume a teleportation occured instead of natural movement. Defaults to mach 10.</summary>
    public double SpeedOfPresumeTeleport = SPEED_OF_SOUND * 10;

    /// <summary>How many units of distance below which audio has basically no dropoff.</summary>
    public float LinearAudioDistance = 10;

    /// <summary>Forward direction (vector) of the audio "camera".</summary>
    public Location ForwardDirection;

    /// <summary>Up direction (vector) of the audio "camera".</summary>
    public Location UpDirection;

    /// <summary>All channels this enforcer plays into.</summary>
    public List<AudioChannel> Channels = [];

    /// <summary>The speed of sound, in units per second (on Earth in air this is 343 m/s).</summary>
    public float SpeedOfSound = SPEED_OF_SOUND;

    /// <summary>How far apart the ears are.</summary>
    public float HeadWidth = 0.2f; // TODO: SoundEngine control of this var

    /// <summary>The backing <see cref="SoundEngine"/> instance.</summary>
    public SoundEngine Engine;

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
        for (int i = 0; i < InternalData.REUSABLE_BUFFER_ARRAY_SIZE; i++)
        {
            Internal.ReusableBuffers[i] = new byte[InternalData.BYTES_PER_BUFFER];
        }
        Internal.Context = acontext;
        if (Channels.IsEmpty())
        {
            Channels.Add(new AudioChannel("Left", this, Quaternion.FromAxisAngle(Location.UnitZ, Math.PI * 0.5)));
            Channels.Add(new AudioChannel("Right", this, Quaternion.FromAxisAngle(Location.UnitZ, -Math.PI * 0.5)));
        }
        Run = true;
        AudioThread = new Thread(new ThreadStart(Internal.ForceAudioLoop))
        {
            Name = "Audio_" + Interlocked.Increment(ref AudioID)
        };
        AudioThread.Start();
    }

    /// <summary>Updates data for the current frame.</summary>
    /// <param name="newPosition">The new position the audio "camera" listener is in.</param>
    /// <param name="forward">The new forward direction.</param>
    /// <param name="up">The new up direction.</param>
    /// <param name="didTeleport">If true, the listener teleported. If false, they moved normally.</param>
    /// <param name="timeElapsed">How much time has elapsed since the previous frame, in seconds.</param>
    public void FrameUpdate(Location newPosition, Location forward, Location up, bool didTeleport, double timeElapsed)
    {
        lock (Locker)
        {
            ForwardDirection = forward;
            UpDirection = up;
            FrameTime = timeElapsed;
            if (!didTeleport)
            {
                Location travelVector = newPosition - Position;
                CurrentVelocity = travelVector / timeElapsed;
                if (CurrentVelocity.LengthSquared() > SpeedOfPresumeTeleport * SpeedOfPresumeTeleport)
                {
                    didTeleport = true;
                }
            }
            PreviousPosition = didTeleport ? newPosition : Position;
            Position = newPosition;
            if (didTeleport)
            {
                CurrentVelocity = Location.Zero;
            }
            foreach (AudioChannel channel in Channels)
            {
                channel.FrameUpdate();
            }
        }
    }

    /// <summary>Shuts down the enforcer. May take a moment before the enforcer thread stops.</summary>
    public void Shutdown()
    {
        Run = false;
    }

    /// <summary>Current audio levels. Use <see cref="Volatile"/>.</summary>
    public float CurrentLevel = 0.0f;

    /// <summary>Internal data used by the enforcer.</summary>
    public struct InternalData
    {
        /// <summary>The relevant backing enforcer instance.</summary>
        public AudioEnforcer Instance;

        /// <summary>The audio frequency, in Hz (samples per second).</summary>
        public const int FREQUENCY = 44100;

        /// <summary>Audio byte-rate to use.</summary>
        public const int BYTERATE = 2;

        /// <summary>Multiplying an audio sample by this lowers its volume by 3 dB.</summary>
        public const float MINUS_THREE_DB = 0.707106781f;

        /// <summary>The maximum pause, in milliseconds, between audio crunching passes. In practice pauses may end up longer due to operating system timing limitations, so keep this shorter than <see cref="MS_LOAD"/>.</summary>
        public const int PAUSE = 10;

        /// <summary>How many milliseconds to load at once in any one buffer.</summary>
        public const int MS_LOAD = 33;

        /// <summary>Nubmer of audio buffers to use at the same time per channel.</summary>
        public const int BUFFERS_AT_ONCE = 2;

        /// <summary>How many buffers are in <see cref="ReusableBuffers"/>.</summary>
        public const int REUSABLE_BUFFER_ARRAY_SIZE = 32;

        /// <summary>Actual byte space to load at once.</summary>
        public const int BYTES_PER_BUFFER = (int)((FREQUENCY * MS_LOAD) / 1000.0) * BYTERATE;

        /// <summary>Relevant OpenAL audio context.</summary>
        public ALContext Context;

        /// <summary>A queue of byte arrays to reuse as audio buffers. Buffers are generated once and kept for the lifetime of the enforcer to prevent GC thrash.</summary>
        public byte[][] ReusableBuffers;

        /// <summary>The index in <see cref="ReusableBuffers"/> to next use.</summary>
        public int ByteBufferID;

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

        /// <summary>Completely closes and stops the audio enforcer system.</summary>
        public void CloseAndStop()
        {
            foreach (AudioChannel channel in Instance.Channels)
            {
                channel.DisableSource();
            }
            if (Context.Handle != IntPtr.Zero)
            {
                ALC.DestroyContext(Context);
            }
            Context = new ALContext(IntPtr.Zero);
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

        /// <summary>Preprocess all channels, ensuring they have a valid audio source, and clearing used up buffers. Returns true if any channels are still full, or false if all have room for new buffers.</summary>
        public readonly bool PreprocessAllChannels()
        {
            bool anyFilled = false;
            foreach (AudioChannel channel in Instance.Channels)
            {
                if (channel.ALSource == -1)
                {
                    channel.BuildSource();
                }
                AL.GetSource(channel.ALSource, ALGetSourcei.BuffersProcessed, out int buffersDone);
                while (buffersDone > 0)
                {
                    int bufferID = AL.SourceUnqueueBuffer(channel.ALSource);
                    UsableBufferIDs.Enqueue(bufferID);
                    buffersDone--;
                }
                AL.GetSource(channel.ALSource, ALGetSourcei.BuffersQueued, out int waiting);
                if (waiting >= BUFFERS_AT_ONCE)
                {
                    anyFilled = true;
                }
            }
            return anyFilled;
        }

        /// <summary>Adds a single playing audio instance to all channels.</summary>
        public readonly void AddClipToAllChannels(LiveAudioInstance audio)
        {
            if (audio.State == AudioState.PLAYING)
            {
                int newSample = 0;
                int maxTimeOffset = 0;
                foreach (AudioChannel channel in Instance.Channels)
                {
                    AudioChannel.ClipAddingResult result = channel.AddClipToBuffer(audio);
                    newSample = result.NewSample;
                    maxTimeOffset = Math.Max(maxTimeOffset, result.TimeOffset);
                }
                audio.CurrentSample = newSample;
                if (!audio.Loop && audio.CurrentSample + maxTimeOffset * 2 >= audio.Clip.Data.Length)
                {
                    audio.CurrentSample = 0;
                    audio.State = AudioState.DONE;
                    DeadInstances.Add(audio);
                }
            }
            else if (audio.State == AudioState.STOP || audio.State == AudioState.DONE)
            {
                DeadInstances.Add(audio);
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
                    stopwatch.Restart();
                    bool anyFilled = PreprocessAllChannels();
                    if (!anyFilled)
                    {
                        foreach (AudioChannel channel in Instance.Channels)
                        {
                            channel.InternalCurrentBuffer = GetNextBuffer();
                        }
                        lock (Instance.Locker)
                        {
                            foreach (LiveAudioInstance audio in Instance.Playing)
                            {
                                AddClipToAllChannels(audio);
                            }
                            foreach (LiveAudioInstance inst in DeadInstances)
                            {
                                Instance.Playing.Remove(inst);
                            }
                            DeadInstances.Clear();
                        }
                        float newCurrentLevel = 0;
                        foreach (AudioChannel channel in Instance.Channels)
                        {
                            newCurrentLevel = Math.Max(newCurrentLevel, GetLevelFor(channel.InternalCurrentBuffer));
                            int bufferID = UsableBufferIDs.Count > 0 ? UsableBufferIDs.Dequeue() : AL.GenBuffer();
                            channel.PlayBuffer(bufferID);
                        }
                        Volatile.Write(ref Instance.CurrentLevel, newCurrentLevel);
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
