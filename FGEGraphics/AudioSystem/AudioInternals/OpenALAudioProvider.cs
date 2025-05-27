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
using FreneticUtilities.FreneticExtensions;
using FGECore.CoreSystems;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using FGEGraphics.GraphicsHelpers;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Helper for audio playback using OpenTK's OpenAL library. Cross-platform and simple. Used when native libraries are unavailable.</summary>
public class OpenALAudioProvider : GenericAudioBacker
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

    /// <inheritdoc/>
    public override void PreInit()
    {
        // Nothing needed.
    }

    /// <inheritdoc/>
    public override void SelectDeviceAndInit(AudioDevice device)
    {
        // Gather some metadata, mostly just for debugging usage
        string[] devices = [.. ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier)];
        string deviceName = device is not null ? device.InternalID : ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
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

    /// <summary>(Attempts to) return a list of all available audio devices.</summary>
    public override List<AudioDevice> ListAllAudioDevices()
    {
        List<AudioDevice> devices = [];
        string[] deviceNames = [.. ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier)];
        foreach (string deviceName in deviceNames)
        {
            // TODO: Can we even read any metadata here? Like... the *name* of a device even? Argh.
            AudioDevice device = new()
            {
                Name = deviceName,
                UID = deviceName,
                InternalID = deviceName,
                FullDescriptionText = $"OpenAL Device: {deviceName}",
                IDs = [deviceName],
            };
            devices.Add(device);
        }
        return devices;
    }

    /// <inheritdoc/>
    public override void Shutdown()
    {
        if (Context.Handle != IntPtr.Zero)
        {
            ALC.DestroyContext(Context);
        }
        Context = new ALContext(IntPtr.Zero);
    }

    /// <inheritdoc/>
    public override void MakeCurrent()
    {
        ALC.MakeContextCurrent(Context);
    }

    /// <inheritdoc/>
    public override bool PreprocessStep()
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

    /// <inheritdoc/>
    public override unsafe void SendNextBuffer(FGE3DAudioEngine engine)
    {
        // 4 = 2 bytes per sample, 2 channels
        byte[] rawBuffer = RawDataBuffers.Count > 0 ? RawDataBuffers.Dequeue() : new byte[FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER * 4];
        AudioChannel leftChannel = engine.Channels.FirstOrDefault(c => c.Name == "Left");
        AudioChannel rightChannel = engine.Channels.FirstOrDefault(c => c.Name == "Right");
        short* leftBuffer = leftChannel.InternalCurrentBuffer;
        short* rightBuffer = rightChannel.InternalCurrentBuffer;
        GraphicsUtil.DebugAssert(leftBuffer != null && rightBuffer != null, "Left and right buffers must be non-null.");
        fixed (byte* rawBufferPtr = rawBuffer)
        {
            for (int i = 0; i < FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER; i++)
            {
                short left = leftBuffer[i], right = rightBuffer[i];
                byte* subPtr = rawBufferPtr + (i * 4);
                subPtr[0] = unchecked((byte)(left & 0xff));
                subPtr[1] = unchecked((byte)((left >> 8) & 0xff));
                subPtr[2] = unchecked((byte)(right & 0xff));
                subPtr[3] = unchecked((byte)((right >> 8) & 0xff));
            }
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
