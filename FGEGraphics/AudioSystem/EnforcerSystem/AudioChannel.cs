//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using Quaternion = FGECore.MathHelpers.Quaternion;

namespace FGEGraphics.AudioSystem.EnforcerSystem;

/// <summary>Represents one channel of audio (eg left or right ear) within the audio enforcer engine.</summary>
/// <remarks>Construct the audio channel instance and prep it for OpenAL usage.</remarks>
public class AudioChannel(string name, AudioEnforcer enforcer, Quaternion rotation)
{
    /// <summary>Human-readable name of this audio channel, for debugging.</summary>
    public string Name = name;

    /// <summary>The backing enforcer instance.</summary>
    public AudioEnforcer Enforcer = enforcer;

    /// <summary>The rotation from the listener's forward direction to this channel's ear. Build a quaternion presuming Y-Forward Z-Up for simple results. For example, the right ear channel would be a 90 degree rotation to the right.</summary>
    public Quaternion RotationFromForward = rotation;

    /// <summary>The current position of this channel's input, eg the location of an ear.</summary>
    public Location CurrentPosition = Location.Zero;

    /// <summary>When this channel is being processed for new audio to add, this is the current buffer it's targeting.</summary>
    public byte[] InternalCurrentBuffer;

    /// <summary>The AL Audio Source for this channel.</summary>
    public int ALSource = -1;

    /// <summary>Volume modifier for this channel.</summary>
    public float Volume = 1;

    /// <summary>The minimum volume applied from channel directionalism.</summary>
    public float DirectionalMinimum = 0;//0.2f;

    /// <summary>Performs a general frame update of current data on this channel.</summary>
    public void FrameUpdate()
    {
        Quaternion adaptedUp = Quaternion.GetQuaternionBetween(Location.UnitZ, Enforcer.UpDirection);
        Quaternion actualCurrentRot = RotationFromForward * adaptedUp;
        Location earDirection = actualCurrentRot.Transform(Enforcer.ForwardDirection);
        CurrentPosition = Enforcer.Position + earDirection * (Enforcer.HeadWidth * 0.5);
        // TODO: Track and handle ear's velocity
    }

    /// <summary>Builds and configures the audio source for this channel.</summary>
    public void BuildSource()
    {
        ALSource = AL.GenSource();
        ConfigureSource();
    }

    /// <summary>Configures the OpenAL source to have current proper information for this channel.</summary>
    public void ConfigureSource()
    {
        // TODO: Audio: This is a jank hack, OpenAL is really bad at selecting which channels play what apparently.
        // Need to rewrite this. Maybe nuke OpenAL and use a more native library? Eck.
        Vector3 zero = Vector3.Zero;
        // OpenAL uses Y-Up with X-Sides and Z-Forward
        Quaternion adaptedUp = Quaternion.GetQuaternionBetween(Location.UnitY, Enforcer.UpDirection);
        Quaternion alRot = RotationFromForward * adaptedUp;
        Vector3 pos = alRot.Transform(Location.UnitZ).ToOpenTK();
        Vector3 dir = -pos;
        AL.Source(ALSource, ALSourceb.Looping, false);
        AL.Source(ALSource, ALSource3f.Direction, ref dir);
        AL.Source(ALSource, ALSource3f.Velocity, ref zero);
        AL.Source(ALSource, ALSource3f.Position, ref pos);
        AL.Source(ALSource, ALSourceb.SourceRelative, false);
        AL.Source(ALSource, ALSourcef.ConeInnerAngle, 20);
        AL.Source(ALSource, ALSourcef.ConeOuterAngle, 40);
    }

    /// <summary>Stops and removes the audio source.</summary>
    public void DisableSource()
    {
        if ((ALSourceState)AL.GetSource(ALSource, ALGetSourcei.SourceState) == ALSourceState.Playing)
        {
            AL.SourceStop(ALSource);
        }
        AL.DeleteSource(ALSource);
        ALSource = -1;
    }

    /// <summary>Contains data about how audio sounds relative to a specific ear.</summary>
    /// <param name="Volume">The volume to this ear.</param>
    /// <param name="TimeOffset">The time offset to this ear, in samples. This is usually a negative number, to indicate the position in the clip should be earlier, ie the playback of real audio should come later.</param>
    public record struct AudioPositionalData(float Volume, int TimeOffset)
    {
    }

    /// <summary>Calculates the correct positional audio data for each ear for a position based on distance (using inverse-square-root) and direction (using trigonometry). Returns as (left, right).</summary>
    public AudioPositionalData GetPositionalData(Location position)
    {
        Location relativeDirectionVector = (Enforcer.Position - position).Normalize();
        relativeDirectionVector = RotationFromForward.Transform(relativeDirectionVector);
        Quaternion directionDifference = Quaternion.GetQuaternionBetween(Enforcer.ForwardDirection, relativeDirectionVector);
        float angle = (float)directionDifference.RepresentedAngle();
        float angleVolume = Math.Max(0, (float)Math.Cos(angle * 0.5)) * (1f - DirectionalMinimum) + DirectionalMinimum;
        AudioPositionalData data = new();
        float dist = (float)position.Distance(CurrentPosition);
        float modifiedDist = dist / Enforcer.LinearAudioDistance;
        float distanceGain = 1.0f / Math.Max(1.0f, modifiedDist * modifiedDist);
        data.Volume = angleVolume * distanceGain;
        float timeOffsetSeconds = -dist / Enforcer.SpeedOfSound;
        data.TimeOffset = (int)(timeOffsetSeconds * AudioEnforcer.InternalData.FREQUENCY);
        return data;
    }

    /// <summary>Plays the current audio buffer into OpenAL.</summary>
    public void PlayBuffer(int bufferId)
    {
        AL.BufferData(bufferId, ALFormat.Mono16, InternalCurrentBuffer, AudioEnforcer.InternalData.FREQUENCY);
        AL.SourceQueueBuffer(ALSource, bufferId);
        if ((ALSourceState)AL.GetSource(ALSource, ALGetSourcei.SourceState) != ALSourceState.Playing)
        {
            AL.SourcePlay(ALSource);
        }
        InternalCurrentBuffer = null;
    }

    /// <summary>Result data from <see cref="AddClipToBuffer(LiveAudioInstance)"/>.</summary>
    /// <param name="NewSample">The new sample index.</param>
    /// <param name="TimeOffset">The time offset for this clip in this channel.</param>
    public record struct ClipAddingResult(int NewSample, int TimeOffset);

    /// <summary>Adds a single audio instance to the raw playback buffer, without losing pre-existing audio data in the buffer.</summary>
    public ClipAddingResult AddClipToBuffer(LiveAudioInstance toAdd)
    {
        int currentSample = toAdd.CurrentSample;
        int outBufPosition = 0;
        // TODO: Need to track the actual change in position for each ear between frames, divided by frametime, and apply a shift effect to match.
        // TODO: So eg if a player whips their head 180 degrees in one frame, the audio should have a natural effect from that rather than glitch jumping.
        // TODO: Note to make sure that accounts reasonably for teleports (ie don't go wild at the frame of teleportation).
        // TODO: Note as well the current ear velocity should be additive with the sound velocity.
        int timeOffset = 0;
        float volume = 1;
        if (toAdd.UsePosition)
        {
            AudioPositionalData data = GetPositionalData(toAdd.Position);
            timeOffset = data.TimeOffset;
            volume = data.Volume;
        }
        float gain = toAdd.Gain * Enforcer.Volume * Volume;
        gain *= gain;
        int volumeModifier = (int)((volume * gain) * ushort.MaxValue);
        byte[] clipData = toAdd.Clip.Data;
        byte[] outBuffer = InternalCurrentBuffer;
        int maxBytePosition = AudioEnforcer.InternalData.BYTES_PER_BUFFER;
        if (!toAdd.Loop)
        {
            maxBytePosition = Math.Min(clipData.Length - (currentSample + timeOffset * 2), AudioEnforcer.InternalData.BYTES_PER_BUFFER);
        }
        while (outBufPosition < maxBytePosition && outBufPosition + 3 < AudioEnforcer.InternalData.BYTES_PER_BUFFER)
        {
            // TODO: pitch, velocity, etc.?
            int sample = currentSample + timeOffset * 2;
            if (toAdd.Loop)
            {
                sample %= clipData.Length;
                if (sample < 0)
                {
                    sample += clipData.Length;
                }
            }
            if (sample >= 0 && sample + 1 < clipData.Length)
            {
                int rawSample = unchecked((short)((clipData[sample + 1] << 8) | clipData[sample]));
                int outSample = (rawSample * volumeModifier) >> 16;
                int rawPreValue = unchecked((short)((outBuffer[outBufPosition + 1] << 8) | outBuffer[outBufPosition]));
                outSample += rawPreValue; // TODO: Better scaled adder?
                outSample = Math.Clamp(outSample, short.MinValue, short.MaxValue);
                outBuffer[outBufPosition] = (byte)outSample;
                outBuffer[outBufPosition + 1] = unchecked((byte)(outSample >> 8));
            }
            currentSample += toAdd.Clip.Channels * 2;
            if (toAdd.Loop)
            {
                currentSample %= clipData.Length;
            }
            outBufPosition += 2;
        }
        return new(currentSample, timeOffset);
    }
}
