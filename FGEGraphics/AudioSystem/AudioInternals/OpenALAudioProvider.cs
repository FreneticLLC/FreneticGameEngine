//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FreneticUtilities.FreneticExtensions;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Helper for audio playback using OpenTK's OpenAL library. Cross-platform and simple. Used when native libraries are unavailable.</summary>
public class OpenALAudioProvider
{
    /// <summary>All available OpenAL extensions.</summary>
    public HashSet<string> ALExtensions = [];

    /// <summary>The relevant OpenAL Device.</summary>
    public ALDevice Device;

    /// <summary>The relevant OpenAL Device Name.</summary>
    public string DeviceName;

    /// <summary>The audio context from OpenAL.</summary>
    public ALContext Context = new(IntPtr.Zero);

    /// <summary>For OpenAL usage, all audio comes from a single "source" (ie bypassing normal 3D audio in OpenAL to allow FGE's audio engine to do the work).</summary>
    public int ALSource;

    /// <summary>Queue of reusable buffer IDs.</summary>
    public Queue<int> UsableBufferIDs = [];

    /// <summary>Reusable internal raw binary buffers.</summary>
    public Queue<byte[]> RawDataBuffers = [];

    /// <summary>Initialize the OpenAL system.</summary>
    public void Init()
    {
        // Gather some metadata, mostly just for debugging usage
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
        // Build the global single source
        ALSource = AL.GenSource();
        Vector3 zero = Vector3.Zero;
        AL.Source(ALSource, ALSourceb.Looping, false);
        AL.Source(ALSource, ALSource3f.Direction, ref zero);
        AL.Source(ALSource, ALSource3f.Velocity, ref zero);
        AL.Source(ALSource, ALSource3f.Position, ref zero);
        AL.Source(ALSource, ALSourceb.SourceRelative, true);
        Logs.ClientInit($"Audio system initialized using OpenAL... vendor='{vendor}', renderer='{renderer}', version='{version}', using device '{DeviceName}'");
        Logs.Debug($"OpenAL available devices: '{devices.JoinString("','")}', ALExtensions='{ALExtensions.JoinString("','")}'");
    }

    /// <summary>Shut down and disable the audio system.</summary>
    public void Shutdown()
    {
        if (Context.Handle != IntPtr.Zero)
        {
            ALC.DestroyContext(Context);
        }
        Context = new ALContext(IntPtr.Zero);
    }

    /// <summary>Attaches this OpenAL context to the current thread.</summary>
    public void MakeCurrent()
    {
        ALC.MakeContextCurrent(Context);
    }

    /// <summary>Preprocesses the OpenAL backer for a single framestep. Returns true if there's room to add anything, or false if the instance is already full on buffers.</summary>
    public bool PreprocessStep()
    {
        AL.GetSource(ALSource, ALGetSourcei.BuffersProcessed, out int buffersDone);
        while (buffersDone > 0)
        {
            int bufferID = AL.SourceUnqueueBuffer(ALSource);
            UsableBufferIDs.Enqueue(bufferID);
            buffersDone--;
        }
        AL.GetSource(ALSource, ALGetSourcei.BuffersQueued, out int waiting);
        return waiting < FGE3DAudioEngine.InternalData.BUFFERS_AT_ONCE;
    }

    /// <summary>Gather data from the internal audio engine and send it forward to OpenAL to play.</summary>
    public void SendNextBuffer(FGE3DAudioEngine engine)
    {
        byte[] rawBuffer = RawDataBuffers.Count > 0 ? RawDataBuffers.Dequeue() : new byte[FGE3DAudioEngine.InternalData.BYTES_PER_BUFFER * 2];
        AudioChannel leftChannel = engine.Channels.FirstOrDefault(c => c.Name == "Left");
        AudioChannel rightChannel = engine.Channels.FirstOrDefault(c => c.Name == "Right");
        byte[] leftBuffer = leftChannel.InternalCurrentBuffer;
        byte[] rightBuffer = rightChannel.InternalCurrentBuffer;
        // TODO: Perf testing -- validate this loop is as raw as possible. This might invoke some nasty JIT range check spam in current state
        for (int i = 0; i < FGE3DAudioEngine.InternalData.BYTES_PER_BUFFER; i += 2)
        {
            rawBuffer[i * 2] = leftBuffer[i];
            rawBuffer[i * 2 + 1] = leftBuffer[i + 1];
            rawBuffer[i * 2 + 2] = rightBuffer[i];
            rawBuffer[i * 2 + 3] = rightBuffer[i + 1];
        }
        int bufferId = UsableBufferIDs.Count > 0 ? UsableBufferIDs.Dequeue() : AL.GenBuffer();
        AL.BufferData(bufferId, ALFormat.Stereo16, rawBuffer, FGE3DAudioEngine.InternalData.FREQUENCY);
        AL.SourceQueueBuffer(ALSource, bufferId);
        if ((ALSourceState)AL.GetSource(ALSource, ALGetSourcei.SourceState) != ALSourceState.Playing)
        {
            AL.SourcePlay(ALSource);
        }
        RawDataBuffers.Enqueue(rawBuffer);
    }
}
