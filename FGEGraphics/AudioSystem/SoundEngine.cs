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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGEGraphics.AudioSystem.EnforcerSystem;
using FGEGraphics.ClientSystem;
using OpenTK.Audio.OpenAL;
using NVorbis;
using System.Threading;

namespace FGEGraphics.AudioSystem;

/// <summary>An audio sound system and engine for playing audio.</summary>
public class SoundEngine : IDisposable
{
    /// <summary>Dumb MS logic dispose method.</summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Context.Handle != IntPtr.Zero)
            {
                ALC.DestroyContext(Context);
            }
            Context = new ALContext(IntPtr.Zero);
        }
    }

    /// <summary>Disposes the window client.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>A "noise" sound effect.</summary>
    public SoundEffect Noise;

    /// <summary>The audio context from OpenAL.</summary>
    public ALContext Context = new(IntPtr.Zero);

    /// <summary>The internal audio enforcer, if used.</summary>
    public AudioEnforcer EnforcerInternal;

    /// <summary>The backing game client.</summary>
    public GameEngineBase Client;

    /// <summary>Current global volume.</summary>
    public float GlobalVolume = 0.5f;

    /// <summary>Current global pitch.</summary>
    public float GlobalPitch = 1.0f;

    /// <summary>The speed of sound, in units per second, defaults to <see cref="AudioEnforcer.SPEED_OF_SOUND"/>.</summary>
    public float SpeedOfSound = AudioEnforcer.SPEED_OF_SOUND;

    /// <summary>The max volume/gain that can be applied to a sound effect.</summary>
    public float MaxSoundVolume = 2;

    /// <summary>All available OpenAL extensions.</summary>
    public HashSet<string> ALExtensions = [];

    /// <summary>The relevant OpenAL Device.</summary>
    public ALDevice Device;

    /// <summary>The relevant OpenAL Device Name.</summary>
    public string DeviceName;

    /// <summary>Initialize the sound engine.</summary>
    /// <param name="tclient">The backing client.</param>
    public void Init(GameEngineBase tclient)
    {
        EnforcerInternal?.Shutdown();
        if (Context.Handle != IntPtr.Zero)
        {
            ALC.DestroyContext(Context);
        }
        Client = tclient;
        string[] devices = [.. ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier)];
        string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
        Device = ALC.OpenDevice(deviceName);
        Context = ALC.CreateContext(Device, (int[])null);
        ALC.MakeContextCurrent(Context);
        DeviceName = ALC.GetString(Device, AlcGetString.DeviceSpecifier);
        string[] devExtensions = (ALC.GetString(Device, AlcGetString.Extensions) ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] coreExtensions = (AL.Get(ALGetString.Extensions) ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        ALExtensions.UnionWith(coreExtensions);
        ALExtensions.UnionWith(devExtensions);
        string vendor = AL.Get(ALGetString.Vendor);
        string renderer = AL.Get(ALGetString.Renderer);
        string version = AL.Get(ALGetString.Version);
        EnforcerInternal = new AudioEnforcer() { Engine = this };
        EnforcerInternal.Init(Context);
        Effects = [];
        PlayingNow = [];
        Logs.ClientInit($"Audio system initialized, OpenAL vendor='{vendor}', renderer='{renderer}', version='{version}', using device '{DeviceName}', available devices: '{devices.JoinString("','")}', ALExtensions='{ALExtensions.JoinString("','")}'");
    }

    /// <summary>Stop all sounds.</summary>
    public void StopAll()
    {
        for (int i = 0; i < PlayingNow.Count; i++)
        {
            PlayingNow[i].Stop();
        }
        PlayingNow.Clear();
    }

    /// <summary>Shutdown the engine.</summary>
    public void Shutdown()
    {
        StopAll();
        if (EnforcerInternal != null)
        {
            EnforcerInternal.Shutdown();
            EnforcerInternal = null;
        }
        if (Context.Handle != IntPtr.Zero && (EnforcerInternal is null || !EnforcerInternal.Run))
        {
            ALC.DestroyContext(Context);
        }
        Context = new ALContext(IntPtr.Zero);
    }

    /// <summary>Whether the engine is 'selected' currently, and should play audio.</summary>
    public bool Selected = true;

    /// <summary>The current position.</summary>
    public Location CPosition = Location.Zero;

    /// <summary>Time until the next clean up pass.</summary>
    public double TimeTowardsNextClean = 0.0;

    /// <summary>Updates the sound engine.</summary>
    /// <param name="position">Current position.</param>
    /// <param name="forward">Forward vector.</param>
    /// <param name="up">Up vcector.</param>
    /// <param name="velocity">Velocity.</param>
    /// <param name="selected">Whether the system is selected.</param>
    public void Update(Location position, Location forward, Location up, Location velocity, bool selected)
    {
        CPosition = position;
        bool sel = !Client.QuietOnDeselect || selected;
        Selected = sel;
        for (int i = 0; i < PlayingNow.Count; i++)
        {
            ActiveSound sound = PlayingNow[i];
            if (!sound.Exists || sound.AudioInternal.State == AudioState.DONE)
            {
                sound.Destroy();
                PlayingNow.RemoveAt(i);
                i--;
                continue;
            }
            sound.Effect.LastUse = Client.GlobalTickTime;
            if (sel && !sound.IsBackground)
            {
                sound.IsDeafened = false;
            }
            if (!sel && sound.IsBackground && !sound.Backgrounded)
            {
                sound.AudioInternal.Gain = 0.0001f;
                sound.Backgrounded = true;
            }
            else if (sel && sound.Backgrounded)
            {
                sound.AudioInternal.Gain = sound.Gain;
                sound.Backgrounded = false;
                sound.IsDeafened = false;
            }
        }
        float globvol = GlobalVolume;
        globvol = globvol <= 0 ? 0.001f : (globvol > 1 ? 1 : globvol);
        EnforcerInternal.FrameUpdate(position, forward, up, false, Client.Delta);
        EnforcerInternal.Volume = globvol;
        EnforcerInternal.SpeedOfSound = SpeedOfSound;
        TimeTowardsNextClean += Client.Delta;
        if (TimeTowardsNextClean > 10.0)
        {
            CleanTick();
            TimeTowardsNextClean = 0.0;
        }
    }

    /// <summary>Effect names to remove.</summary>
    readonly List<string> ToRemove = [];

    /// <summary>Runs a full clean-up pass.</summary>
    public void CleanTick()
    {
        foreach (KeyValuePair<string, SoundEffect> effect in Effects)
        {
            if (effect.Value.LastUse + 30.0 < Client.GlobalTickTime)
            {
                ToRemove.Add(effect.Key);
            }
        }
        foreach (string rem in ToRemove)
        {
            Effects.Remove(rem);
        }
        ToRemove.Clear();
    }

    /// <summary>Current effects.</summary>
    public Dictionary<string, SoundEffect> Effects;

    /// <summary>Currently playing audio.</summary>
    public List<ActiveSound> PlayingNow;

    /// <summary>Try to clean things. Return whether anything was cleaned.</summary>
    /// <returns>Whether successful.</returns>
    public bool CanClean()
    {
        bool cleaned = false;
        for (int i = 0; i < PlayingNow.Count; i++)
        {
            ActiveSound sound = PlayingNow[i];
            if (sound.Gain < 0.05 || (sound.Position.DistanceSquared(CPosition) > 30 * 30))
            {
                sound.Destroy();
                PlayingNow.RemoveAt(i);
                i--;
                cleaned = true;
            }
        }
        return cleaned;
    }

    /// <summary>
    /// Plays a sound effect.
    /// NOTE: *NOT* guaranteed to play a sound effect immediately, regardless of input! Some sound effects will be delayed! If too many audible sounds are already playing, this will refuse to play.
    /// </summary>
    /// <param name="sfx">The sound effect.</param>
    /// <param name="loop">Whether to loop.</param>
    /// <param name="pos">The position.</param>
    /// <param name="pitch">The pitch.</param>
    /// <param name="volume">The volume.</param>
    /// <param name="seek">The seek location.</param>
    /// <param name="callback">The callback upon playing start.</param>
    public void Play(SoundEffect sfx, bool loop, Location pos, float pitch = 1, float volume = 1, float seek = 0, Action<ActiveSound> callback = null)
    {
        if (sfx is null)
        {
            return;
        }
        if (PlayingNow.Count > 200 && EnforcerInternal is null)
        {
            if (!CanClean())
            {
                return;
            }
        }
        if (pitch <= 0 || pitch > 2)
        {
            throw new ArgumentException("Must be between 0 and 2", nameof(pitch));
        }
        if (volume == 0)
        {
            return;
        }
        if (volume < 0)
        {
            throw new ArgumentException("volume cannot be less than zero", nameof(volume));
        }
        volume = Math.Min(volume, MaxSoundVolume);
        void playSound()
        {
            if (sfx.Clip is null)
            {
                return;
            }
            ActiveSound actsfx = new(sfx)
            {
                Engine = this,
                Position = pos,
                Pitch = pitch * GlobalPitch,
                Gain = volume,
                Loop = loop
            };
            actsfx.Create();
            if (actsfx.AudioInternal is null)
            {
                return;
            }
            if (seek != 0)
            {
                actsfx.Seek(seek);
            }
            actsfx.Play();
            PlayingNow.Add(actsfx);
            callback?.Invoke(actsfx);
        }
        lock (sfx)
        {
            if (sfx.Clip is null)
            {
                sfx.Loaded += (o, e) =>
                {
                    playSound();
                };
                return;
            }
        }
        playSound();
    }

    /// <summary>Gets a sound by name.</summary>
    /// <param name="name">The name.</param>
    /// <returns>The sound.</returns>
    public SoundEffect GetSound(string name)
    {
        string namelow = name.ToLowerFast();
        if (Effects.TryGetValue(namelow, out SoundEffect sfx))
        {
            return sfx;
        }
        sfx = LoadSound(namelow);
        if (sfx is not null)
        {
            Effects.Add(namelow, sfx);
            return sfx;
        }
        Logs.Warning($"Unable to find sound named '{TextStyle.Standout}{namelow}{TextStyle.Base}'");
        sfx = new SoundEffect()
        {
            Name = namelow,
            LastUse = Client.GlobalTickTime
        };
        Effects.Add(namelow, sfx);
        return sfx;
    }

    /// <summary>Lock object to guarantee no simultaneous file reads.</summary>
    public LockObject SoundFileLocker = new();

    /// <summary>Load a sound effect.</summary>
    /// <param name="name">The name of the effect.</param>
    /// <returns>The sound effect.</returns>
    public SoundEffect LoadSound(string name)
    {
        try
        {
            string newname = $"sounds/{name}.ogg";
            if (!Client.Client.Files.FileExists(newname))
            {
                Logs.Warning($"Cannot load audio '{name}': file does not exist.");
                return null;
            }
            SoundEffect tsfx = new()
            {
                Name = name,
                LastUse = Client.GlobalTickTime
            };
            Client.Schedule.StartAsyncTask(() =>
            {
                try
                {
                    byte[] rawData;
                    lock (SoundFileLocker)
                    {
                        rawData = Client.Client.Files.ReadFileData(newname);
                    }
                    SoundEffect ts = LoadVorbisSound(new MemoryStream(rawData), name);
                    lock (tsfx)
                    {
                        tsfx.Clip = ts.Clip;
                    }
                    if (tsfx.Loaded is not null)
                    {
                        Client.Schedule.ScheduleSyncTask(() =>
                        {
                            tsfx.Loaded?.Invoke(tsfx, null);
                        });
                    }
                }
                catch (Exception ex)
                {
                    SysConsole.Output($"loading audio file '{name}'", ex);
                }
            });
            return tsfx;
        }
        catch (Exception ex)
        {
            Logs.Error($"Reading sound file '{name}': {ex}");
            return null;
        }
    }

    /// <summary>Loads a sound effect from a .OGG stream.</summary>
    /// <param name="stream">The data stream.</param>
    /// <param name="name">The name.</param>
    /// <returns>The sound effect.</returns>
    public SoundEffect LoadVorbisSound(Stream stream, string name)
    {
        using VorbisReader oggReader = new(stream);
        SoundEffect sfx = new()
        {
            Name = name,
            LastUse = Client.GlobalTickTime
        };
        int sampleCount = (int)oggReader.TotalSamples * oggReader.Channels;
        // TODO: re-usable buffer for opti reasons?
        float[] sampleBuffer = new float[sampleCount];
        oggReader.ReadSamples(sampleBuffer, 0, sampleCount);
        byte[] data = new byte[sampleCount * 2];
        for (int i = 0; i < sampleCount; i++)
        {
            PrimitiveConversionHelper.Short16ToBytes((short)(sampleBuffer[i] * short.MaxValue), data, i * 2);
        }
        if (EnforcerInternal != null)
        {
            LiveAudioClip clip = new()
            {
                Data = data,
                Channels = (byte)oggReader.Channels
            };
            sfx.Clip = clip;
        }
        return sfx;
    }

    /// <summary>Loads a sound effect from a .WAV stream.</summary>
    /// <param name="stream">The data stream.</param>
    /// <param name="name">The name.</param>
    /// <returns>The sound effect.</returns>
    public SoundEffect LoadWaveSound(DataStream stream, string name)
    {
        SoundEffect sfx = new()
        {
            Name = name,
            LastUse = Client.GlobalTickTime
        };
        byte[] data = ProcessWAVEData(stream, out int channels, out int bits, out _);
        if (EnforcerInternal != null)
        {
            LiveAudioClip clip = new()
            {
                Data = data
            };
            if (bits == 8)
            {
                clip.Data = new byte[data.Length * 2];
                for (int i = 0; i < data.Length; i++)
                {
                    // TODO: Sanity?
                    clip.Data[i] = data[i + 1];
                    clip.Data[i + 1] = 0;
                }
                //data = clip.Data;
            }
            /*long pblast = 0;
            for (int i = 0; i < clip.Data.Length; i++)
            {
                pblast += clip.Data[i];
            }*/
            // TODO: clip.Rate = rate;
            clip.Channels = (byte)channels;
            sfx.Clip = clip;
            // OutputType.DEBUG.Output("Clip: " + sfx.Clip.Data.Length + ", " + channels + ", " + bits + ", " + rate + ", " + pblast);
        }
        return sfx;
    }

    /// <summary>Loads a WAVE (.wav) audio effect.</summary>
    /// <param name="stream">The stream.</param>
    /// <param name="channels">Channel count output.</param>
    /// <param name="bits">Bit count output.</param>
    /// <param name="rate">Rate output.</param>
    /// <returns>The wave data.</returns>
    public static byte[] ProcessWAVEData(DataStream stream, out int channels, out int bits, out int rate)
    {
        DataReader dr = new(stream);
        string signature = dr.ReadString(4);
        if (signature != "RIFF")
        {
            throw new NotSupportedException("Not a RIFF .wav file: " + signature);
        }
        /*int riff_chunk_size = */
        dr.ReadInt();
        string format = dr.ReadString(4);
        if (format != "WAVE")
        {
            throw new NotSupportedException("Not a WAVE .wav file: " + format);
        }
        string format_signature = dr.ReadString(4);
        if (format_signature != "fmt ")
        {
            throw new NotSupportedException("Not a 'fmt ' .wav file: " + format_signature);
        }
        /*int format_chunk_size = */
        dr.ReadInt();
        /*int audio_format = */
        dr.ReadShort();
        int num_channels = dr.ReadShort();
        if (num_channels != 1 && num_channels != 2)
        {
            throw new NotSupportedException("Invalid number of channels: " + num_channels);
        }
        int sample_rate = dr.ReadInt();
        /*int byte_rate = */
        dr.ReadInt();
        /*int block_align = */
        dr.ReadShort();
        int bits_per_sample = dr.ReadShort();
        string data_signature = dr.ReadString(4);
        if (data_signature != "data")
        {
            throw new NotSupportedException("Not a DATA .wav file: " + data_signature);
        }
        int data_chunk_size = dr.ReadInt();
        channels = num_channels;
        bits = bits_per_sample;
        rate = sample_rate;
        int extra = data_chunk_size - dr.Available;
        if (extra == 0)
        {
            return dr.ReadBytes(data_chunk_size);
        }
        else
        {
            byte[] b = new byte[data_chunk_size];
            byte[] t = dr.ReadBytes(dr.Available);
            t.CopyTo(b, 0);
            return b;
        }
    }

    /// <summary>Estimates current audio levels (If enforcer enabled).</summary>
    /// <returns>The audio level.</returns>
    public float EstimateAudioLevel()
    {
        return Volatile.Read(ref EnforcerInternal.CurrentLevel);
    }
}
