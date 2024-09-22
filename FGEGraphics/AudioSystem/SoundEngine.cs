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
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGEGraphics.AudioSystem.EnforcerSystem;
using FGEGraphics.ClientSystem;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using NVorbis;

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

    /// <summary>
    /// Maximum number of sound effects playing simultaneously before the next one gets fed to the enforcer instead of directly into OpenAL.
    /// If set to 0, the enforcer is always used.
    /// </summary>
    public int MaxBeforeEnforce = 50;

    //public MicrophoneHandler Microphone = null;

    /// <summary>The backing game client.</summary>
    public GameEngineBase Client;

    /// <summary>Current global volume.</summary>
    public float GlobalVolume = 0.5f;

    /// <summary>Current global pitch.</summary>
    public float GlobalPitch = 1.0f;

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
        string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
        ALDevice device = ALC.OpenDevice(deviceName);
        Context = ALC.CreateContext(device, (int[])null);
        if (Client.EnforceAudio)
        {
            EnforcerInternal = new AudioEnforcer();
            EnforcerInternal.Init(Context);
            Context = new ALContext(IntPtr.Zero);
        }
        else
        {
            ALC.MakeContextCurrent(Context);
        }
        /*try
        {
            if (Microphone is not null)
            {
                Microphone.StopEcho();
            }
            Microphone = new MicrophoneHandler(this);
        }
        catch (Exception ex)
        {
            SysConsole.Output("Loading microphone handling", ex);
        }*/
        if (Effects is not null)
        {
            foreach (SoundEffect sfx in Effects.Values)
            {
                sfx.Internal = -2;
            }
        }
        Effects = [];
        PlayingNow = [];
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

    /// <summary>Checks for audio errors.</summary>
    /// <param name="inp">The location.</param>
    [Conditional("AUDIO_ERROR_CHECK")]
    public void CheckError(string inp)
    {
        if (EnforcerInternal is null || MaxBeforeEnforce != 0)
        {
            ALError err = AL.GetError();
            if (err != ALError.NoError)
            {
                Logs.Warning($"Found audio error {err} for {inp}");
                //init(Client, CVars);
                return;
            }
        }
    }

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
        if (EnforcerInternal is null || MaxBeforeEnforce != 0)
        {
            ALError err = AL.GetError();
            if (err != ALError.NoError)
            {
                Logs.Warning($"Found audio error {err}!");
                //init(Client, CVars);
                return;
            }
        }
        bool sel = !Client.QuietOnDeselect || selected;
        Selected = sel;
        for (int i = 0; i < PlayingNow.Count; i++)
        {
            ActiveSound sound = PlayingNow[i];
            if (!sound.Exists || (sound.AudioInternal is null && sound.Src < 0) || (sound.AudioInternal is null ? (ALSourceState)AL.GetSource(sound.Src, ALGetSourcei.SourceState) == ALSourceState.Stopped : sound.AudioInternal.State == AudioState.DONE))
            {
                sound.Destroy();
                if (sound.AudioInternal is null)
                {
                    CheckError($"Destroy:{sound.Effect.Name}");
                }
                PlayingNow.RemoveAt(i);
                i--;
                continue;
            }
            sound.Effect.LastUse = Client.GlobalTickTime;
            if (sel && !sound.IsBackground)
            {
                if (sound.AudioInternal is null)
                {
                    AL.Source(sound.Src, ALSourcef.Gain, sound.Gain);
                }
                else
                {

                }
                sound.IsDeafened = false;
            }
            if (!sel && sound.IsBackground && !sound.Backgrounded)
            {
                if (sound.AudioInternal is null)
                {
                    AL.Source(sound.Src, ALSourcef.Gain, 0.0001f);
                }
                else
                {
                    sound.AudioInternal.Gain = 0.0001f;
                }
                sound.Backgrounded = true;
            }
            else if (sel && sound.Backgrounded)
            {
                if (sound.AudioInternal is null)
                {
                    AL.Source(sound.Src, ALSourcef.Gain, sound.Gain);
                }
                else
                {
                    sound.AudioInternal.Gain = sound.Gain;
                }
                sound.Backgrounded = false;
                sound.IsDeafened = false;
            }
        }
        CheckError("Setup");
        /*if (Microphone != null)
        {
            Microphone.Tick();
        }*/
        CheckError("Microphone");
        float globvol = GlobalVolume;
        globvol = globvol <= 0 ? 0.001f : (globvol > 1 ? 1 : globvol);
        if (EnforcerInternal == null || MaxBeforeEnforce != 0)
        {
            Vector3 pos = position.ToOpenTK();
            Vector3 forw = forward.ToOpenTK();
            Vector3 upvec = up.ToOpenTK();
            Vector3 vel = velocity.ToOpenTK();
            AL.Listener(ALListener3f.Position, ref pos);
            AL.Listener(ALListenerfv.Orientation, ref forw, ref upvec);
            AL.Listener(ALListener3f.Velocity, ref vel);
            CheckError("Positioning");
            AL.Listener(ALListenerf.Gain, globvol);
            CheckError("Gain");
        }
        if (EnforcerInternal is not null)
        {
            // TODO: vel
            //AudioInternal.Left = CVars.a_left.ValueB;
            //AudioInternal.Right = CVars.a_right.ValueB;
            EnforcerInternal.Position = position;
            EnforcerInternal.ForwardDirection = forward;
            EnforcerInternal.UpDirection = up;
            EnforcerInternal.Volume = globvol;
        }
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
                if (effect.Value.Internal > -1)
                {
                    AL.DeleteBuffer(effect.Value.Internal);
                }
                effect.Value.Internal = -2;
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
            //OutputType.DEBUG.Output("Audio / null");
            return;
        }
        if (PlayingNow.Count > 200 && EnforcerInternal is null)
        {
            if (!CanClean())
            {
                //OutputType.DEBUG.Output("Audio / count");
                return;
            }
        }
        if (sfx.Internal == -2)
        {
            Play(GetSound(sfx.Name), loop, pos, pitch, volume, seek, callback);
            return;
        }
        if (pitch <= 0 || pitch > 2)
        {
            throw new ArgumentException("Must be between 0 and 2", nameof(pitch));
        }
        if (volume == 0)
        {
            return;
        }
        if (volume <= 0 || volume > 1)
        {
            throw new ArgumentException("Must be between 0 and 1", nameof(volume));
        }
        void playSound()
        {
            if (sfx.Clip is null && sfx.Internal < 0)
            {
                //OutputType.DEBUG.Output("Audio / clip");
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
            if (actsfx.AudioInternal is null && actsfx.Src < 0)
            {
                //OutputType.DEBUG.Output("Audio / src");
                return;
            }
            CheckError("Create:" + sfx.Name);
            if (seek != 0)
            {
                actsfx.Seek(seek);
            }
            CheckError("Preconfig:" + sfx.Name);
            actsfx.Play();
            CheckError("Play:" + sfx.Name);
            //OutputType.DEBUG.Output("Audio / sucess");
            PlayingNow.Add(actsfx);
            callback?.Invoke(actsfx);
        }
        lock (sfx)
        {
            if (sfx.Clip is null && sfx.Internal == -1)
            {
                //OutputType.DEBUG.Output("Audio / delay");
                sfx.Loaded += (o, e) =>
                {
                    playSound();
                };
                return;
            }
        }
        playSound();
    }

    /// <summary>Play a sound (simple internal option).</summary>
    /// <param name="sfx">The effect.</param>
    /// <param name="loop">Whether to loop.</param>
    /// <returns>The sound played.</returns>
    public ActiveSound PlaySimpleInternal(SoundEffect sfx, bool loop)
    {
        ActiveSound playSound()
        {
            ActiveSound actsfx = new(sfx)
            {
                Engine = this,
                Position = Location.NaN,
                Pitch = 1.0f,
                Gain = 1.0f,
                Loop = loop
            };
            actsfx.Create();
            actsfx.Play();
            return actsfx;
        }
        lock (sfx)
        {
            if (sfx.Internal == -1)
            {
                return null; // TODO: Enforce load-NOW?
            }
        }
        return playSound();
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
        sfx = new SoundEffect()
        {
            Name = namelow,
            Internal = -1,
            LastUse = Client.GlobalTickTime
        };
        Effects.Add(namelow, sfx);
        return sfx;
    }

    /// <summary>Gets the OpenAL sound format for some data.</summary>
    /// <param name="channels">Channel count.</param>
    /// <param name="bits">Bit count.</param>
    /// <returns>AL format.</returns>
    static ALFormat GetSoundFormat(int channels, int bits)
    {
        if (channels == 1)
        {
            return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
        }
        else if (channels == 2)
        {
            return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
        }
        else
        {
            throw new NotSupportedException($"The specified sound format ({channels} channels) is not supported.");
        }
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
                Internal = -1,
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
                        tsfx.Internal = ts.Internal;
                        tsfx.Clip = ts.Clip;
                    }
                    //OutputType.DEBUG.Output("Audio / valid1: " + tsfx.Internal + ", " + tsfx.Clip);
                    if (tsfx.Loaded != null)
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
            //OutputType.DEBUG.Output("Audio / valid: " + tsfx);
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
        if (EnforcerInternal == null || MaxBeforeEnforce != 0)
        {
            sfx.Internal = AL.GenBuffer();
            AL.BufferData(sfx.Internal, GetSoundFormat(oggReader.Channels, 16), data, oggReader.SampleRate);
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
        byte[] data = ProcessWAVEData(stream, out int channels, out int bits, out int rate);
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
        if (EnforcerInternal == null || MaxBeforeEnforce != 0)
        {
            sfx.Internal = AL.GenBuffer();
            AL.BufferData(sfx.Internal, GetSoundFormat(channels, bits), data, rate);
        }
        //OutputType.DEBUG.Output("Audio / prepped: " + AudioInternal);
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
        if (EnforcerInternal is not null)
        {
            lock (EnforcerInternal.CLelLock)
            {
                return EnforcerInternal.CurrentLevel;
            }
        }
        else
        {
            return 0.5f; // TODO: ???
        }
    }
}
