//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGECore.MathHelpers;
using System;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Represents one channel of audio (eg left or right ear) within the audio engine.</summary>
/// <remarks>Construct the audio channel instance and prep it for OpenAL usage.</remarks>
public class AudioChannel(string name, FGE3DAudioEngine engine, Quaternion rotation)
{
    /// <summary>Human-readable name of this audio channel, for debugging.</summary>
    public string Name = name;

    /// <summary>The backing audio engine instance.</summary>
    public FGE3DAudioEngine Engine = engine;

    /// <summary>The rotation from the listener's forward direction to this channel's ear. Build a quaternion presuming Y-Forward Z-Up for simple results. For example, the right ear channel would be a 90 degree rotation to the right.</summary>
    public Quaternion RotationFromForward = rotation;

    /// <summary>The current position of this channel's input, eg the location of an ear.</summary>
    public Location CurrentPosition = Location.Zero;

    /// <summary>How far the <see cref="CurrentPosition"/> changed in this frame from the previous.</summary>
    public Location PositionChange = Location.Zero;

    /// <summary>The current frame's velocity for this ear.</summary>
    public Location Velocity = Location.Zero;

    /// <summary>The global time of the prior frame.</summary>
    public double PriorFrameTime = 0;

    /// <summary>The global time of this frame.</summary>
    public double FrameTime = 0;

    /// <summary>The delta time elapsed between the previous frame and this one.</summary>
    public double FrameDelta = 0;

    /// <summary>When this channel is being processed for new audio to add, this is the current buffer it's targeting.</summary>
    public byte[] InternalCurrentBuffer;

    /// <summary>Volume modifier for this channel.</summary>
    public float Volume = 1;

    /// <summary>The minimum volume applied from channel directionalism.</summary>
    public float DirectionalMinimum = 0.2f;
    
    /// <summary>Offset for stereo source reading (0 for left, 2 for right).</summary>
    public int StereoIndex = 0;

    /// <summary>Performs a general frame update of current data on this channel.</summary>
    public void FrameUpdate()
    {
        PriorFrameTime = FrameTime;
        FrameTime = Engine.Internal.FrameTime;
        FrameDelta = Math.Clamp(FrameTime - PriorFrameTime, 0.0001, 10);
        Quaternion adaptedUp = Quaternion.GetQuaternionBetween(Location.UnitZ, Engine.Internal.UpDirection);
        Quaternion actualCurrentRot = RotationFromForward * adaptedUp;
        Location earDirection = actualCurrentRot.Transform(Engine.Internal.ForwardDirection);
        Location newPosition = Engine.Internal.Position + earDirection * (Engine.HeadWidth * 0.5);
        if (Engine.Internal.DidTeleport)
        {
            PositionChange = Location.Zero;
        }
        else
        {
            PositionChange = newPosition - CurrentPosition;
        }
        Velocity = PositionChange / FrameDelta;
        CurrentPosition = newPosition;
    }

    /// <summary>Contains data about how audio sounds relative to a specific ear.</summary>
    /// <param name="Volume">The volume to this ear.</param>
    /// <param name="TimeOffset">The time offset to this ear, in samples. This is usually a negative number, to indicate the position in the clip should be earlier, ie the playback of real audio should come later.</param>
    public record struct AudioPositionalData(float Volume, int TimeOffset)
    {
    }

    /// <summary>Calculates the correct positional audio data for the ear for a position based on distance (using inverse-square-root) and direction (using trigonometry).</summary>
    public AudioPositionalData GetPositionalData(Location position)
    {
        Location relativeDirectionVector = (Engine.Internal.Position - position).Normalize();
        relativeDirectionVector = RotationFromForward.Transform(relativeDirectionVector);
        Quaternion directionDifference = Quaternion.GetQuaternionBetween(Engine.Internal.ForwardDirection, relativeDirectionVector);
        float angle = (float)directionDifference.RepresentedAngle();
        float angleVolume = Math.Max(0, (float)Math.Cos(angle * 0.5)) * (1f - DirectionalMinimum) + DirectionalMinimum;
        AudioPositionalData data = new();
        float dist = (float)position.Distance(CurrentPosition);
        float modifiedDist = dist / Engine.LinearAudioDistance;
        float distanceGain = 1.0f / Math.Max(1.0f, modifiedDist * modifiedDist);
        data.Volume = angleVolume * distanceGain;
        float timeOffsetSeconds = -dist / Engine.SpeedOfSound;
        data.TimeOffset = (int)(timeOffsetSeconds * FGE3DAudioEngine.InternalData.FREQUENCY);
        return data;
    }

    /// <summary>Result data from <see cref="AddClipToBuffer(LiveAudioInstance)"/>.</summary>
    /// <param name="NewSample">The new sample index.</param>
    /// <param name="TimeOffset">The time offset for this clip in this channel.</param>
    /// <param name="IsDead">The sound has passed its end.</param>
    public record struct ClipAddingResult(int NewSample, int TimeOffset, bool IsDead);

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
        int bytesPerSample = 2 * toAdd.Clip.Channels;
        float gain = toAdd.Gain * Engine.Volume * Volume;
        float pitch = toAdd.Pitch; // TODO: Determine pitch by relative velocity
        bool procPitch = pitch != 1;
        gain *= gain; // Exponential volume is how humans perceive volume (see eg decibel system)
        int volumeModifier = (int)((volume * gain) * ushort.MaxValue);
        byte[] clipData = toAdd.Clip.Data;
        byte[] outBuffer = InternalCurrentBuffer;
        int clipLen = clipData.Length;
        int offset = timeOffset * bytesPerSample + StereoIndex;
        double step = bytesPerSample / (double)clipLen;
        double samplePos = currentSample / (double)clipLen;
        bool isDead = false;
        while (outBufPosition + 1 < FGE3DAudioEngine.InternalData.BYTES_PER_BUFFER)
        {
            double approxSample = samplePos * clipLen;
            currentSample = (int)Math.Round(approxSample);
            currentSample -= currentSample % bytesPerSample;
            int priorSample = currentSample;
            if (procPitch && approxSample > currentSample)
            {
                currentSample += bytesPerSample;
            }
            else
            {
                priorSample -= bytesPerSample;
            }
            float fraction = (float)(approxSample - priorSample) / bytesPerSample;
            int sample = currentSample + offset;
            if (toAdd.Loop)
            {
                sample %= clipLen;
                if (sample < 0)
                {
                    sample += clipLen;
                }
            }
            if (sample >= clipLen)
            {
                isDead = true;
                break;
            }
            if (sample >= 0 && sample + 1 < clipLen)
            {
                int rawPreValue = unchecked((short)((outBuffer[outBufPosition + 1] << 8) | outBuffer[outBufPosition]));
                int rawSample = unchecked((short)((clipData[sample + 1] << 8) | clipData[sample]));
                int outSample = (rawSample * volumeModifier) >> 16;
                if (procPitch && priorSample >= 0 && priorSample + 1 < clipLen)
                {
                    int rawPriorSample = unchecked((short)((clipData[priorSample + 1] << 8) | clipData[priorSample]));
                    int outPriorSample = (rawPriorSample * volumeModifier) >> 16;
                    outSample = (int)(outPriorSample + (outSample - outPriorSample) * fraction);
                }
                outSample += rawPreValue; // TODO: Better scaled adder?
                outSample = Math.Clamp(outSample, short.MinValue, short.MaxValue);
                outBuffer[outBufPosition] = (byte)outSample;
                outBuffer[outBufPosition + 1] = unchecked((byte)(outSample >> 8));
            }
            samplePos += step * pitch;
            outBufPosition += 2;
        }
        return new(currentSample, timeOffset, isDead);
    }
}
