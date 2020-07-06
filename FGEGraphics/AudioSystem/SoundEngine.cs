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
using System.IO;
using System.Threading.Tasks;
using FGEGraphics.AudioSystem.EnforcerSystem;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using FGEGraphics.ClientSystem;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.FileSystems;
using NVorbis;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FGEGraphics.AudioSystem
{
    /// <summary>
    /// An audio sound system and engine for playing audio.
    /// </summary>
    public class SoundEngine : IDisposable
    {
        /// <summary>
        /// Dumb MS logic dispose method.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context.Dispose();
            }
        }

        /// <summary>
        /// Disposes the window client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// A "noise" sound effect.
        /// </summary>
        public SoundEffect Noise;

        /// <summary>
        /// Theudio context from OpenAL.
        /// </summary>
        public AudioContext Context;

        /// <summary>
        /// The internal audio enforcer, if used.
        /// </summary>
        public AudioEnforcer AudioInternal;

        //public MicrophoneHandler Microphone = null;

        /// <summary>
        /// The backing game client.
        /// </summary>
        public GameEngineBase Client;

        /// <summary>
        /// Current global volume.
        /// </summary>
        public float GlobalVolume = 0.5f;

        /// <summary>
        /// Current global pitch.
        /// </summary>
        public float GlobalPitch = 1.0f;
        
        /// <summary>
        /// Initialize the sound engine.
        /// </summary>
        /// <param name="tclient">The backing client.</param>
        public void Init(GameEngineBase tclient)
        {
            if (AudioInternal != null)
            {
                AudioInternal.Shutdown();
            }
            if (Context != null)
            {
                Context.Dispose();
            }
            Client = tclient;
            Context = new AudioContext(AudioContext.DefaultDevice, 0, 0, false, true);
            if (Client.EnforceAudio)
            {
                AudioInternal = new AudioEnforcer();
                AudioInternal.Init(Context);
                Context = null;
            }
            else
            {
                Context.MakeCurrent();
            }
            /*try
            {
                if (Microphone != null)
                {
                    Microphone.StopEcho();
                }
                Microphone = new MicrophoneHandler(this);
            }
            catch (Exception ex)
            {
                SysConsole.Output("Loading microphone handling", ex);
            }*/
            if (Effects != null)
            {
                foreach (SoundEffect sfx in Effects.Values)
                {
                    sfx.Internal = -2;
                }
            }
            Effects = new Dictionary<string, SoundEffect>();
            PlayingNow = new List<ActiveSound>();
            //DeafLoop = GetSound("sfx/ringing/earring_loop");
        }

        /// <summary>
        /// Stop all sounds.
        /// </summary>
        public void StopAll()
        {
            for (int i = 0; i < PlayingNow.Count; i++)
            {
                PlayingNow[i].Stop();
            }
            PlayingNow.Clear();
        }

        /// <summary>
        /// Shutdown the engine.
        /// </summary>
        public void Shutdown()
        {
            StopAll();
            if (Context != null)
            {
                Context.Dispose();
            }
            Context = null;
        }

        /// <summary>
        /// Whether the engine is 'selected' currently, and should play audio.
        /// </summary>
        public bool Selected = true;

        //SoundEffect DeafLoop;

        //public ActiveSound DeafNoise = null;

        /// <summary>
        /// Checks for audio errors.
        /// </summary>
        /// <param name="inp">The location.</param>
        public void CheckError(string inp)
        {
#if AUDIO_ERROR_CHECK
            if (AudioInternal == null)
            {
                ALError err = AL.GetError();
                if (err != ALError.NoError)
                {
                    SysConsole.Output(OutputType.WARNING, "Found audio error " + err + " for " + inp);
                    //init(TheClient, CVars);
                    return;
                }
            }
#endif
        }

        /// <summary>
        /// The current position.
        /// </summary>
        public Location CPosition = Location.Zero;

        /// <summary>
        /// Time until the next clean up pass.
        /// </summary>
        public double TimeTowardsNextClean = 0.0;

        /// <summary>
        /// Updates the sound engine.
        /// </summary>
        /// <param name="position">Current position.</param>
        /// <param name="forward">Forward vector.</param>
        /// <param name="up">Up vcector.</param>
        /// <param name="velocity">Velocity.</param>
        /// <param name="selected">Whether the system is selected.</param>
        public void Update(Location position, Location forward, Location up, Location velocity, bool selected)
        {
            CPosition = position;
            if (AudioInternal == null)
            {
                ALError err = AL.GetError();
                if (err != ALError.NoError)
                {
                    SysConsole.Output(OutputType.WARNING, "Found audio error " + err + "!");
                    //init(TheClient, CVars);
                    return;
                }
            }
            bool sel = Client.QuietOnDeselect ? selected : true;
            Selected = sel;
            /*if (DeafenTime > 0.0)
            {
                TimeDeaf += Engine.Delta;
                DeafenTime -= Engine.Delta;
                if (DeafNoise == null)
                {
                    DeafNoise = PlaySimpleInternal(DeafLoop, true);
                    if (DeafNoise == null)
                    {
                        DeafenTime = 0;
                        TimeDeaf = 0;
                    }
                }
                if (DeafenTime < 0)
                {
                    TimeDeaf = 0;
                    DeafenTime = 0;
                    DeafNoise.Stop();
                    DeafNoise.Destroy();
                    DeafNoise = null;
                }
            }
            if (TimeDeaf > 0.001 && DeafenTime > 0.001)
            {
                float weaken = (float)Math.Min(DeafenTime, TimeDeaf);
                if (weaken < 1.0)
                {
                    DeafNoise.Gain = (float)weaken * 0.5f;
                    DeafNoise.UpdateGain();
                }
                else
                {
                    DeafNoise.Gain = 0.5f;
                    DeafNoise.UpdateGain();
                }
            }
            DeafLoop.LastUse = Engine.GlobalTickTime;*/
            for (int i = 0; i < PlayingNow.Count; i++)
            {
                if (!PlayingNow[i].Exists || (AudioInternal == null && PlayingNow[i].Src < 0) || (AudioInternal == null ? AL.GetSourceState(PlayingNow[i].Src) == ALSourceState.Stopped : PlayingNow[i].AudioInternal.State == AudioState.DONE))
                {
                    PlayingNow[i].Destroy();
                    if (AudioInternal == null)
                    {
                        CheckError("Destroy:" + PlayingNow[i].Effect.Name);
                    }
                    PlayingNow.RemoveAt(i);
                    i--;
                    continue;
                }
                PlayingNow[i].Effect.LastUse = Client.GlobalTickTime;
                /*if ((TimeDeaf > 0.0) && sel && !PlayingNow[i].IsBackground)
                {
                    PlayingNow[i].IsDeafened = true;
                    float lesser = (float)Math.Min(DeafenTime, TimeDeaf);
                    if (lesser < 0.999)
                    {
                        if (AudioInternal == null)
                        {
                            AL.Source(PlayingNow[i].Src, ALSourcef.Gain, PlayingNow[i].Gain * (1.0f - lesser));
                        }
                        else
                        {
                            PlayingNow[i].AudioInternal.Gain = PlayingNow[i].Gain * (1.0f - lesser);
                        }
                    }
                    else
                    {
                        if (AudioInternal == null)
                        {
                            AL.Source(PlayingNow[i].Src, ALSourcef.Gain, 0.0001f);
                        }
                        else
                        {
                            PlayingNow[i].AudioInternal.Gain = 0.0001f;
                        }
                    }
                }
                else */if (/*(TimeDeaf <= 0.0) && */sel && !PlayingNow[i].IsBackground)
                {
                    if (AudioInternal == null)
                    {
                        AL.Source(PlayingNow[i].Src, ALSourcef.Gain, PlayingNow[i].Gain);
                    }
                    else
                    {

                    }
                    PlayingNow[i].IsDeafened = false;
                }
                if (/*(TimeDeaf <= 0.0) && */!sel && PlayingNow[i].IsBackground && !PlayingNow[i].Backgrounded)
                {
                    if (AudioInternal == null)
                    {
                        AL.Source(PlayingNow[i].Src, ALSourcef.Gain, 0.0001f);
                    }
                    else
                    {
                        PlayingNow[i].AudioInternal.Gain = 0.0001f;
                    }
                    PlayingNow[i].Backgrounded = true;
                }
                else if (/*(TimeDeaf <= 0.0) && */sel && PlayingNow[i].Backgrounded)
                {
                    if (AudioInternal == null)
                    {
                        AL.Source(PlayingNow[i].Src, ALSourcef.Gain, PlayingNow[i].Gain);
                    }
                    else
                    {
                        PlayingNow[i].AudioInternal.Gain = PlayingNow[i].Gain;
                    }
                    PlayingNow[i].Backgrounded = false;
                    PlayingNow[i].IsDeafened = false;
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
            if (AudioInternal == null)
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
            else
            {
                // TODO: vel
                //AudioInternal.Left = CVars.a_left.ValueB;
                //AudioInternal.Right = CVars.a_right.ValueB;
                AudioInternal.Position = position;
                AudioInternal.ForwardDirection = forward;
                AudioInternal.UpDirection = up;
                AudioInternal.Volume = globvol;
            }
            TimeTowardsNextClean += Client.Delta;
            if (TimeTowardsNextClean > 10.0)
            {
                CleanTick();
                TimeTowardsNextClean = 0.0;
            }
        }

        /// <summary>
        /// Effect names to remove.
        /// </summary>
        readonly List<string> ToRemove = new List<string>();

        /// <summary>
        /// Runs a full clean-up pass.
        /// </summary>
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

        /// <summary>
        /// Current effects.
        /// </summary>
        public Dictionary<string, SoundEffect> Effects;

        /// <summary>
        /// Currently playing audio.
        /// </summary>
        public List<ActiveSound> PlayingNow;

        /// <summary>
        /// Try to clean things. Return whether anything was cleaned.
        /// </summary>
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
            if (sfx == null)
            {
                //SysConsole.Output(OutputType.DEBUG, "Audio / null");
                return;
            }
            if (PlayingNow.Count > 200 && AudioInternal == null)
            {
                if (!CanClean())
                {
                    //SysConsole.Output(OutputType.DEBUG, "Audio / count");
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
                throw new ArgumentException("Must be between 0 and 2", "pitch");
            }
            if (volume == 0)
            {
                return;
            }
            if (volume <= 0 || volume > 1)
            {
                throw new ArgumentException("Must be between 0 and 1", "volume");
            }
            void playSound()
            {
                if (sfx.Clip == null && sfx.Internal < 0)
                {
                    //SysConsole.Output(OutputType.DEBUG, "Audio / clip");
                    return;
                }
                ActiveSound actsfx = new ActiveSound(sfx)
                {
                    Engine = this,
                    Position = pos,
                    Pitch = pitch * GlobalPitch,
                    Gain = volume,
                    Loop = loop
                };
                actsfx.Create();
                if (actsfx.AudioInternal == null && actsfx.Src < 0)
                {
                    //SysConsole.Output(OutputType.DEBUG, "Audio / src");
                    return;
                }
                CheckError("Create:" + sfx.Name);
                /*if (TimeDeaf > 0.0)
                {
                    actsfx.IsDeafened = true;
                    if (AudioInternal == null)
                    {
                        AL.Source(actsfx.Src, ALSourcef.Gain, 0.0001f);
                    }
                    else
                    {
                        actsfx.AudioInternal.Gain = 0.0001f;
                    }
                }*/
                if (seek != 0)
                {
                    actsfx.Seek(seek);
                }
                CheckError("Preconfig:" + sfx.Name);
                actsfx.Play();
                CheckError("Play:" + sfx.Name);
                //SysConsole.Output(OutputType.DEBUG, "Audio / sucess");
                PlayingNow.Add(actsfx);
                callback?.Invoke(actsfx);
            }
            lock (sfx)
            {
                if (sfx.Clip == null && sfx.Internal == -1)
                {
                    //SysConsole.Output(OutputType.DEBUG, "Audio / delay");
                    sfx.Loaded += (o, e) =>
                    {
                        playSound();
                    };
                    return;
                }
            }
            playSound();
        }

        /// <summary>
        /// Play a sound (simple internal option).
        /// </summary>
        /// <param name="sfx">The effect.</param>
        /// <param name="loop">Whether to loop.</param>
        /// <returns>The sound played.</returns>
        public ActiveSound PlaySimpleInternal(SoundEffect sfx, bool loop)
        {
            ActiveSound playSound()
            {
                ActiveSound actsfx = new ActiveSound(sfx)
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

        /// <summary>
        /// Gets a sound by name.
        /// </summary>
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
            if (sfx != null)
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

        /// <summary>
        /// Gets the OpenAL sound format for some data.
        /// </summary>
        /// <param name="channels">Channel count.</param>
        /// <param name="bits">Bit count.</param>
        /// <returns>AL format.</returns>
        ALFormat GetSoundFormat(int channels, int bits)
        {
            return channels switch
            {
                1 => bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16,
                // 2
                _ => bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16,
            };
        }

        /// <summary>
        /// Load a sound effect.
        /// </summary>
        /// <param name="name">The name of the effect.</param>
        /// <returns>The sound effect.</returns>
        public SoundEffect LoadSound(string name)
        {
            try
            {
                string newname = $"sounds/{name}.ogg";
                if (!Client.Client.Files.FileExists(newname))
                {
                    //SysConsole.Output(OutputType.DEBUG, "Audio / nullsource");
                    return null;
                }
                SoundEffect tsfx = new SoundEffect()
                {
                    Name = name,
                    Internal = -1,
                    LastUse = Client.GlobalTickTime
                };
                Client.Schedule.StartAsyncTask(() =>
                {
                    try
                    {
                        SoundEffect ts = LoadVorbisSound(new MemoryStream(Client.Client.Files.ReadFileData(newname)), name);
                        lock (tsfx)
                        {
                            tsfx.Internal = ts.Internal;
                            tsfx.Clip = ts.Clip;
                        }
                        //SysConsole.Output(OutputType.DEBUG, "Audio / valid1: " + tsfx.Internal + ", " + tsfx.Clip);
                        if (tsfx.Loaded != null)
                        {
                            Client.Schedule.ScheduleSyncTask(() =>
                            {
                                if (tsfx.Loaded != null)
                                {
                                    tsfx.Loaded.Invoke(tsfx, null);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        SysConsole.Output($"loading audio file '{name}'", ex);
                    }
                });
                //SysConsole.Output(OutputType.DEBUG, "Audio / valid: " + tsfx);
                return tsfx;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, $"Reading sound file '{name}': {ex}");
                return null;
            }
        }

        /*
        public void Deafen(double time)
        {
            if (DeafenTime == 0.0 && time < 2.0)
            {
                time = 2.0;
            }
            DeafenTime = time;
        }

        public double DeafenTime = 0.0;

        public double TimeDeaf = 0.0;
        */

        /// <summary>
        /// Loads a sound effect from a .OGG stream.
        /// </summary>
        /// <param name="stream">The data stream.</param>
        /// <param name="name">The name.</param>
        /// <returns>The sound effect.</returns>
        public SoundEffect LoadVorbisSound(Stream stream, string name)
        {
            using VorbisReader oggReader = new VorbisReader(stream);
            SoundEffect sfx = new SoundEffect()
            {
                Name = name,
                LastUse = Client.GlobalTickTime
            };
            int sampleCount = (int)oggReader.TotalSamples;
            // TODO: re-usable buffer for opti reasons?
            float[] sampleBuffer = new float[sampleCount];
            int readSamples = oggReader.ReadSamples(sampleBuffer, 0, sampleCount);
            byte[] data = new byte[sampleCount * 2];
            PrimitiveConversionHelper.Int32ByteUnion intConverter = default;
            for (int i = 0; i < sampleCount; i++)
            {
                intConverter.Int32Value = (int)(sampleBuffer[i] * short.MaxValue);
                data[i * 2] = intConverter.Bytes.Byte0Value;
                data[i * 2 + 1] = intConverter.Bytes.Byte1Value;
            }
            if (AudioInternal != null)
            {
                LiveAudioClip clip = new LiveAudioClip()
                {
                    Data = data
                };
                clip.Channels = (byte)oggReader.Channels;
                sfx.Clip = clip;
            }
            else
            {
                sfx.Internal = AL.GenBuffer();
                AL.BufferData(sfx.Internal, GetSoundFormat(oggReader.Channels, 16), data, data.Length, oggReader.SampleRate);
            }
            return sfx;
        }

        /// <summary>
        /// Loads a sound effect from a stream.
        /// </summary>
        /// <param name="stream">The data stream.</param>
        /// <param name="name">The name.</param>
        /// <returns>The sound effect.</returns>
        public SoundEffect LoadSound(DataStream stream, string name)
        {
            SoundEffect sfx = new SoundEffect()
            {
                Name = name,
                LastUse = Client.GlobalTickTime
            };
            byte[] data = LoadWAVE(stream, out int channels, out int bits, out int rate);
            if (AudioInternal != null)
            {
                LiveAudioClip clip = new LiveAudioClip()
                {
                    Data = data
                };
                if (bits == 1)
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
                // SysConsole.Output(OutputType.DEBUG, "Clip: " + sfx.Clip.Data.Length + ", " + channels + ", " + bits + ", " + rate + ", " + pblast);
            }
            else
            {
                sfx.Internal = AL.GenBuffer();
                AL.BufferData(sfx.Internal, GetSoundFormat(channels, bits), data, data.Length, rate);
            }
            //SysConsole.Output(OutputType.DEBUG, "Audio / prepped: " + AudioInternal);
            return sfx;
        }

        /// <summary>
        /// Loads a WAVE (.wav) audio effect.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="channels">Channel count output.</param>
        /// <param name="bits">Bit count output.</param>
        /// <param name="rate">Rate output.</param>
        /// <returns>The wave data.</returns>
        public byte[] LoadWAVE(DataStream stream, out int channels, out int bits, out int rate)
        {
            DataReader dr = new DataReader(stream);
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

        /// <summary>
        /// Estimates current audio levels (If enforcer enabled).
        /// </summary>
        /// <returns>The audio level.</returns>
        public float EstimateAudioLevel()
        {
            if (AudioInternal != null)
            {
                lock (AudioInternal.CLelLock)
                {
                    return AudioInternal.CurrentLevel;
                }
            }
            else
            {
                return 0.5f; // TODO???
            }
        }
    }
}
