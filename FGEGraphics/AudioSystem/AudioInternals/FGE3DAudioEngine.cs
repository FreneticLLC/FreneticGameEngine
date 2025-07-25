//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>The internal engine to crunch audio data and push it to the speakers.
/// Do not use this directly, use <see cref="SoundEngine"/>.</summary>
public class FGE3DAudioEngine
{
    /// <summary>Constant value of the approximate speed of sound in air on Earth, 343 meters per second.</summary>
    public const float SPEED_OF_SOUND = 343;

    /// <summary>If true, the audio engine should use WASAPI. If false, use OpenAL.</summary>
    public static readonly bool USE_WASAPI = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>How many instances of the audio engine have been created. This value starts at 1 and increments every time an audio engine is launched.</summary>
    public static long AudioID = 1;

    /// <summary>The thread for the enforcer.</summary>
    public Thread AudioThread;

    /// <summary>Whether the system is running.</summary>
    public volatile bool Run = false;

    /// <summary>The current general volume of the audio engine.</summary>
    public float Volume = 0.5f;

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to <see cref="int.MaxValue"/> to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public int LowPassFrequency = int.MaxValue;

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to 0 to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public int HighPassFrequency = 0;

    /// <summary>All currently playing audio.</summary>
    public List<LiveAudioInstance> Playing = [];

    /// <summary>Locker for interaction with the audio engine.</summary>
    public LockObject Locker = new();

    /// <summary>If the audio "camera" moves faster than this many units per second, presume a teleportation occured instead of natural movement. Defaults to mach 10.</summary>
    public double SpeedOfPresumeTeleport = SPEED_OF_SOUND * 10;

    /// <summary>How many units of distance below which audio has basically no dropoff.</summary>
    public float LinearAudioDistance = 10;

    /// <summary>All channels this audio engine plays into.</summary>
    public List<AudioChannel> Channels = [];

    /// <summary>The speed of sound, in units per second (on Earth in air this is 343 m/s).</summary>
    public float SpeedOfSound = SPEED_OF_SOUND;

    /// <summary>How far apart the ears are.</summary>
    public float HeadWidth = 0.2f; // TODO: SoundEngine control of this var

    /// <summary>The internal backend for hardware interaction.</summary>
    public GenericAudioBacker AudioBacker;

    /// <summary>Data for syncing updates to audio clips between the game engine thread and the audio engine thread.</summary>
    public record class SyncUpdate(LiveAudioInstance Instance, Location Position, Location Velocity, float Gain, float Pitch, AudioState State, int Seek, bool Loop, double Time, Location Forward, Location Up, bool DidTeleport, bool IsNew);

    /// <summary>Data for syncing updates to audio clips between the game engine thread and the audio engine thread.</summary>
    public ConcurrentQueue<SyncUpdate> UpdatesToSync = new();

    /// <summary>Add an audio instance to the audio engine.</summary>
    /// <param name="inst">The instance to add.</param>
    public void Add(LiveAudioInstance inst)
    {
        if (inst.State == AudioState.PLAYING)
        {
            return;
        }
        inst.State = AudioState.PLAYING;
        inst.PriorPosition = inst.Position;
        inst.UsePosition = !inst.Position.IsNaN();
        UpdatesToSync.Enqueue(new(inst, Location.Zero, Location.Zero, 1, 1, AudioState.PLAYING, 0, false, 0, Location.Zero, Location.Zero, false, true));
    }

    /// <summary>Initialize and load the audio engine.</summary>
    public unsafe void Init()
    {
        if (AudioBacker is null)
        {
            if (USE_WASAPI)
            {
                AudioBacker = new WasApiAudioProvider();
            }
            else
            {
                AudioBacker = new OpenALAudioProvider();
            }
        }
        else
        {
            AudioBacker.Shutdown();
        }
        AudioBacker.PreInit();
        /*List<AudioDevice> devices = ListAvailableOutputDevices();
        Logs.Debug($"Found {devices.Count} audio devices:");
        foreach (AudioDevice device in devices)
        {
            Logs.Debug(device.FullDescriptionText);
        }*/
        AudioBacker.SelectDeviceAndInit(null); // TODO: Device select
        Internal.Instance = this;
        Internal.ReusableBuffers = new short*[InternalData.REUSABLE_BUFFER_ARRAY_SIZE];
        for (int i = 0; i < InternalData.REUSABLE_BUFFER_ARRAY_SIZE; i++)
        {
            Internal.ReusableBuffers[i] = (short*)Marshal.AllocHGlobal(sizeof(short) * InternalData.SAMPLES_PER_BUFFER);
        }
        if (Channels.IsEmpty())
        {
            Channels.Add(new AudioChannel("Left", this, Quaternion.FromAxisAngle(Location.UnitZ, Math.PI * 0.5)));
            Channels.Add(new AudioChannel("Right", this, Quaternion.FromAxisAngle(Location.UnitZ, -Math.PI * 0.5)) { StereoIndex = 2 });
        }
        Run = true;
        AudioThread = new Thread(new ThreadStart(Internal.ForceAudioLoop))
        {
            Name = "Audio_" + Interlocked.Increment(ref AudioID)
        };
        AudioThread.Start();
    }

    /// <summary>Shuts down the audio engine. May take a moment before the engine thread stops.</summary>
    public unsafe void Shutdown()
    {
        Run = false;
    }

    /// <summary>Returns a list of all available audio output devices.</summary>
    public List<AudioDevice> ListAvailableOutputDevices()
    {
        return AudioBacker.ListAllAudioDevices();
    }

    /// <summary>Current audio levels.</summary>
    public volatile float CurrentLevel = 0.0f;

    /// <summary>Current number of playing sound effects.</summary>
    public volatile int SoundCount = 0;

    /// <summary>Internal data used by the audio engine.</summary>
    public unsafe struct InternalData()
    {
        /// <summary>The relevant backing audio engine instance.</summary>
        public FGE3DAudioEngine Instance;

        /// <summary>The audio frequency, in Hz (samples per second).</summary>
        public const int FREQUENCY = 44100;

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

        /// <summary>Number of audio samples in a buffer.</summary>
        public const int SAMPLES_PER_BUFFER = (int)((FREQUENCY * MS_LOAD) / 1000.0);

        /// <summary>3D Position of the audio "camera".</summary>
        public Location Position;

        /// <summary>If true, a frame update is pending. If false, there has been no update.</summary>
        public bool HadUpdateFrame = false;

        /// <summary>If true, the audio "camera" teleported in this frame.</summary>
        public bool DidTeleport = false;

        /// <summary>The global time (total delta seconds) of the frame.</summary>
        public double FrameTime;

        /// <summary>Forward direction (vector) of the audio "camera".</summary>
        public Location ForwardDirection;

        /// <summary>Up direction (vector) of the audio "camera".</summary>
        public Location UpDirection;

        /// <summary>A queue of byte arrays to reuse as audio buffers. Buffers are generated once and kept for the lifetime of the audio engine to prevent GC thrash.</summary>
        public short*[] ReusableBuffers;

        /// <summary>The index in <see cref="ReusableBuffers"/> to next use.</summary>
        public int ByteBufferID;

        /// <summary>Cached reusable list of dead audio instances.</summary>
        public List<LiveAudioInstance> DeadInstances;

        /// <summary>Gets and cleans the next byte buffer to use.</summary>
        public unsafe short* GetNextBuffer()
        {
            short* toReturn = ReusableBuffers[ByteBufferID++];
            ByteBufferID %= REUSABLE_BUFFER_ARRAY_SIZE;
            Unsafe.InitBlockUnaligned(toReturn, 0, SAMPLES_PER_BUFFER * sizeof(short));
            return toReturn;
        }

        /// <summary>Completely closes and stops the audio engine.</summary>
        public void CloseAndStop()
        {
            if (ReusableBuffers is not null)
            {
                foreach (short* ptr in ReusableBuffers)
                {
                    Marshal.FreeHGlobal((IntPtr)ptr);
                }
                ReusableBuffers = null;
            }
            Instance.Channels.Clear();
            Instance.AudioBacker.Shutdown();
            Instance.AudioBacker = null;
        }

        /// <summary>Calculates the audio level of a raw audio buffer.</summary>
        public unsafe readonly float GetLevelFor(short* buffer)
        {
            GraphicsUtil.DebugAssert(buffer is not null, "GetLevelFor called with null buffer.");
            int maxSample = 0;
            for (int i = 0; i < SAMPLES_PER_BUFFER; i++)
            {
                maxSample = Math.Max(maxSample, Math.Abs((int)buffer[i]));
            }
            return maxSample / (float)short.MaxValue;
        }

        /// <summary>Adds a single playing audio instance to all channels.</summary>
        public readonly void AddClipToAllChannels(LiveAudioInstance audio)
        {
            if (audio.State == AudioState.PLAYING)
            {
                bool isDead = true;
                foreach (AudioChannel channel in Instance.Channels)
                {
                    AudioChannel.ClipAddingResult result = channel.AddClipToBuffer(audio);
                    isDead = isDead && result.IsDead;
                }
                audio.CurrentSample += (int)Math.Round(audio.Pitch * SAMPLES_PER_BUFFER * audio.Clip.Channels);
                if (isDead)
                {
                    audio.CurrentSample = 0;
                    audio.State = AudioState.DONE;
                    DeadInstances.Add(audio);
                }
                audio.PriorPosition = audio.Position;
            }
            else if (audio.State == AudioState.STOP || audio.State == AudioState.DONE)
            {
                DeadInstances.Add(audio);
            }
        }

        /// <summary>Updates data for the current frame.</summary>
        /// <param name="newPosition">The new position the audio "camera" listener is in.</param>
        /// <param name="forward">The new forward direction.</param>
        /// <param name="up">The new up direction.</param>
        /// <param name="newDidTeleport">If true, the listener teleported. If false, they moved normally.</param>
        /// <param name="time">The global time (total delta seconds) of the frame.</param>
        public void FrameUpdate(Location newPosition, Location forward, Location up, bool newDidTeleport, double time)
        {
            ForwardDirection = forward;
            UpDirection = up;
            FrameTime = time;
            if (!newDidTeleport)
            {
                Location travelVector = newPosition - Position;
                Location currentVelocity = travelVector / time;
                if (currentVelocity.LengthSquared() > Instance.SpeedOfPresumeTeleport * Instance.SpeedOfPresumeTeleport)
                {
                    newDidTeleport = true;
                }
            }
            Position = newPosition;
            DidTeleport = newDidTeleport;
            HadUpdateFrame = true;
        }

        /// <summary>Trigger the internal audio engine loop.</summary>
        public readonly void ForceAudioLoop()
        {
            // This has to be re-called from the correct instance, the thread code loses the reference.
            Instance.Internal.ForceAudioLoopInternal();
        }

        /// <summary>The internal audio engine loop.</summary>
        public void ForceAudioLoopInternal()
        {
            try
            {
                long lastTime = Stopwatch.GetTimestamp();
                Instance.AudioBacker.MakeCurrent();
                DeadInstances = [];
                while (true)
                {
                    if (!Instance.Run)
                    {
                        CloseAndStop();
                        return;
                    }
                    long newTime = Stopwatch.GetTimestamp();
                    double elSec = (newTime - lastTime) / (double)Stopwatch.Frequency;
                    lastTime = newTime;
                    bool needsFill = Instance.AudioBacker.PreprocessStep();
                    if (needsFill)
                    {
                        foreach (AudioChannel channel in Instance.Channels)
                        {
                            channel.InternalCurrentBuffer = GetNextBuffer();
                        }
                        lock (Instance.Locker)
                        {
                            while (Instance.UpdatesToSync.TryDequeue(out SyncUpdate update))
                            {
                                if (update.IsNew)
                                {
                                    Instance.Playing.Add(update.Instance);
                                }
                                else if (update.Instance is null)
                                {
                                    FrameUpdate(update.Position, update.Forward, update.Up, update.DidTeleport, update.Time);
                                }
                                else
                                {
                                    update.Instance.Position = update.Position;
                                    if (!update.Instance.UsePosition)
                                    {
                                        update.Instance.PriorPosition = update.Position;
                                    }
                                    update.Instance.UsePosition = !update.Position.IsNaN();
                                    update.Instance.Velocity = update.Velocity;
                                    update.Instance.Gain = update.Gain;
                                    update.Instance.Pitch = update.Pitch;
                                    if (update.State != (AudioState)255)
                                    {
                                        update.Instance.State = update.State;
                                    }
                                    update.Instance.Loop = update.Loop;
                                    if (update.Seek != -1)
                                    {
                                        update.Instance.CurrentSample = update.Seek;
                                    }
                                }
                            }
                            foreach (AudioChannel channel in Instance.Channels)
                            {
                                channel.FrameUpdate();
                            }
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
                        }
                        Instance.AudioBacker.SendNextBuffer(Instance);
                        Instance.CurrentLevel = newCurrentLevel;
                        Instance.SoundCount = Instance.Playing.Count;
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
                SysConsole.Output("Handling audio engine", ex);
            }
        }
    }

    /// <summary>Internal data used by the audio engine.</summary>
    public InternalData Internal = new();
}
