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
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Represents one channel of audio (eg left or right ear) within the audio engine.</summary>
/// <remarks>Construct the audio channel instance and prep it for OpenAL usage.</remarks>
public unsafe class AudioChannel(string name, FGE3DAudioEngine engine, Quaternion rotation)
{
    /// <summary>Human-readable name of this audio channel, for debugging.</summary>
    public string Name = name;

    /// <summary>The backing audio engine instance.</summary>
    public FGE3DAudioEngine Engine = engine;

    /// <summary>The rotation from the listener's forward direction to this channel's ear. Build a quaternion presuming Y-Forward Z-Up for simple results. For example, the right ear channel would be a 90 degree rotation to the right.</summary>
    public Quaternion RotationFromForward = rotation;

    /// <summary>The current position of this channel's input, eg the location of an ear.</summary>
    public Location CurrentPosition = Location.Zero;

    /// <summary>The prior position of this channel's input in the previous audio frame.</summary>
    public Location PriorPosition = Location.Zero;

    /// <summary>The global time of the prior frame.</summary>
    public double PriorFrameTime = 0;

    /// <summary>The global time of this frame.</summary>
    public double FrameTime = 0;

    /// <summary>The delta time elapsed between the previous frame and this one.</summary>
    public double FrameDelta = 0;

    /// <summary>When this channel is being processed for new audio to add, this is the current buffer it's targeting.</summary>
    public short* InternalCurrentBuffer;

    /// <summary>Volume modifier for this channel.</summary>
    public float Volume = 1;

    /// <summary>The minimum volume applied from channel directionalism.</summary>
    public float DirectionalMinimum = 0.2f;
    
    /// <summary>Offset for stereo source reading (0 for left, 2 for right).</summary>
    public int StereoIndex = 0;

    /// <summary>Frequency, in Hz, as the maximum frequency to play (ie a low pass filter).
    /// Set for example to 1000 to only play low pitched bass.
    /// Set to <see cref="int.MaxValue"/> to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public int LowPassFrequency = int.MaxValue;

    /// <summary>Frequency, in Hz, as the minimum frequency to play (ie a high pass filter).
    /// Set for example to 2000 to exclude low pitched bass.
    /// Set to 0 to disable.
    /// You can combine both high and low pass to constrain to a range of frequencies.</summary>
    public int HighPassFrequency = 0;

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
            PriorPosition = newPosition;
        }
        else
        {
            PriorPosition = CurrentPosition;
        }
        CurrentPosition = newPosition;
    }

    /// <summary>Contains data about how audio sounds relative to a specific ear.</summary>
    /// <param name="Volume">The volume to this ear.</param>
    /// <param name="TimeOffset">The time offset to this ear, in samples. This is usually a negative number, to indicate the position in the clip should be earlier, ie the playback of real audio should come later.</param>
    /// <param name="PriorTimeOffset">The previous frame's time offset to this ear, in samples.</param>
    public record struct AudioPositionalData(float Volume, int TimeOffset, int PriorTimeOffset)
    {
    }

    /// <summary>Calculates the correct positional audio data for the ear for a position based on distance (using inverse-square-root) and direction (using trigonometry).</summary>
    public AudioPositionalData GetPositionalData(Location position, Location priorPosition)
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
        float priorDist = (float)priorPosition.Distance(PriorPosition);
        float priorTimeOffsetSeconds = -priorDist / Engine.SpeedOfSound;
        data.PriorTimeOffset = (int)(priorTimeOffsetSeconds * FGE3DAudioEngine.InternalData.FREQUENCY);
        return data;
    }

    /// <summary>Result data from <see cref="AddClipToBuffer(LiveAudioInstance)"/>.</summary>
    /// <param name="IsDead">The sound has passed its end.</param>
    public record struct ClipAddingResult(bool IsDead);

    /// <summary>Adds a single audio instance to the raw playback buffer, without losing pre-existing audio data in the buffer.</summary>
    public ClipAddingResult AddClipToBuffer(LiveAudioInstance toAdd)
    {
        int currentSample = toAdd.CurrentSample;
        int clipChannels = toAdd.Clip.Channels;
        short[] clipData = toAdd.Clip.Data;
        int clipLen = clipData.Length;
        float lowPassFrequencyCap = Math.Min(Math.Min(toAdd.LowPassFrequency, LowPassFrequency), Engine.LowPassFrequency);
        float lowPassFactor = lowPassFrequencyCap / FGE3DAudioEngine.InternalData.FREQUENCY;
        float highPassFrequencyMin = Math.Max(Math.Max(toAdd.HighPassFrequency, HighPassFrequency), Engine.HighPassFrequency);
        float highPassFactor = highPassFrequencyMin / FGE3DAudioEngine.InternalData.FREQUENCY;
        float lowPassPrior = 0, highPassPrior = 0;
        bool mustDoLowPass = lowPassFrequencyCap < 999_999 || highPassFrequencyMin > 0;
        int preRead = 0;
        int maxSample = clipLen;
        if (mustDoLowPass)
        {
            preRead = -(int)(2 / Math.Min(highPassFactor > 0 ? highPassFactor : 1, lowPassFactor));
            currentSample += preRead * clipChannels;
            maxSample -= preRead;
        }
        int reverbCount = toAdd.ReverbCount;
        bool doReverb = reverbCount > 0;
        float reverbDelay = 0, reverbGain = 0;
        int reverbDelaySamples = 0;
        if (doReverb)
        {
            reverbDelay = toAdd.ReverbDelay;
            reverbDelaySamples = (int)(reverbDelay * FGE3DAudioEngine.InternalData.FREQUENCY) * clipChannels;
            reverbDelaySamples -= reverbDelaySamples % clipChannels;
            reverbGain = 1 - toAdd.ReverbDecay;
            reverbGain *= reverbGain;
            int requiredToBeSilent = (int)Math.Ceiling(Math.Log(0.001, reverbGain));
            float secondsExtra = (requiredToBeSilent + reverbCount) * reverbDelay;
            maxSample += reverbDelaySamples * (requiredToBeSilent + reverbCount);
        }
        int timeOffset = 0, priorTimeOffset = 0;
        float volume = 1;
        if (toAdd.UsePosition)
        {
            AudioPositionalData data = GetPositionalData(toAdd.Position, toAdd.PriorPosition);
            volume = data.Volume;
            timeOffset = data.TimeOffset;
            priorTimeOffset = data.PriorTimeOffset;
        }
        float gain = toAdd.Gain * Engine.Volume * Volume;
        float pitch = toAdd.Pitch;
        gain *= gain; // Exponential volume is how humans perceive volume (see eg decibel system)
        int volumeModifier = (int)((volume * gain) * ushort.MaxValue);
        short* outBuffer = InternalCurrentBuffer;
        GraphicsUtil.DebugAssert(outBuffer != null, "outBuffer must not be null");
        double step = clipChannels / (double)clipLen;
        double samplePos = currentSample / (double)clipLen;
        bool isDead = false;
        double stepPitched = step * pitch;
        fixed (short* clipDataPtr = clipData)
        for (int outBufPosition = preRead; outBufPosition < FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER; outBufPosition++)
        {
            float fractionThrough = outBufPosition < 0 ? 0 : outBufPosition / (float)FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER;
            float timeOffsetLocal = (timeOffset * fractionThrough + priorTimeOffset * (1 - fractionThrough)) * clipChannels;
            double approxSample = samplePos * clipLen + timeOffsetLocal;
            currentSample = (int)Math.Round(approxSample);
            currentSample -= currentSample % clipChannels;
            int priorSample = currentSample + StereoIndex;
            if (approxSample > currentSample)
            {
                currentSample += clipChannels;
            }
            else
            {
                priorSample -= clipChannels;
            }
            float fraction = (float)(approxSample - priorSample) / clipChannels;
            int sample = currentSample + StereoIndex;
            if (toAdd.Loop)
            {
                sample %= clipLen;
                if (sample < 0)
                {
                    sample += clipLen;
                }
            }
            if (sample >= maxSample)
            {
                isDead = true;
                break;
            }
            if (sample >= 0)
            {
                int rawSample = sample < clipLen ? clipDataPtr[sample] : 0;
                int outSample = (rawSample * volumeModifier) >> 16;
                if (priorSample >= 0 && priorSample < clipLen)
                {
                    int rawPriorSample = clipDataPtr[priorSample];
                    int outPriorSample = (rawPriorSample * volumeModifier) >> 16;
                    outSample = (int)(outPriorSample + (outSample - outPriorSample) * fraction);
                }
                if (doReverb)
                {
                    float localReverbGain = 1;
                    for (int i = 0; i < reverbCount; i++)
                    {
                        localReverbGain *= reverbGain;
                        int otherSample = sample - reverbDelaySamples * i;
                        if (toAdd.Loop)
                        {
                            otherSample %= clipLen;
                            if (otherSample < 0)
                            {
                                otherSample += clipLen;
                            }
                        }
                        if (otherSample >= 0 && otherSample < clipLen)
                        {
                            int rawOtherSample = clipDataPtr[otherSample];
                            int outOtherSample = (rawOtherSample * volumeModifier) >> 16;
                            outSample += (int)(outOtherSample * localReverbGain);
                        }
                    }
                    // if reverb alone makes this sound loud enough to clip the final buffer, quiet the audio. This is basically a safety/sanity check.
                    if (Math.Abs(outSample) > short.MaxValue)
                    {
                        float reduceVolumeBy = short.MaxValue / Math.Abs(outSample);
                        gain *= reduceVolumeBy;
                        volumeModifier = (int)((volume * gain) * ushort.MaxValue);
                        toAdd.Gain *= reduceVolumeBy;
                    }
                }
                if (mustDoLowPass)
                {
                    if (lowPassFrequencyCap < 999_999)
                    {
                        lowPassPrior += lowPassFactor * (outSample - lowPassPrior);
                        outSample = (int)lowPassPrior;
                    }
                    if (highPassFrequencyMin > 0)
                    {
                        highPassPrior += highPassFactor * (outSample - highPassPrior);
                        outSample -= (int)highPassPrior;
                    }
                }
                if (outBufPosition >= 0)
                {
                    int rawPreValue = outBuffer[outBufPosition];
                    outSample += rawPreValue; // TODO: Better scaled adder?
                    outSample = Math.Clamp(outSample, short.MinValue, short.MaxValue);
                    outBuffer[outBufPosition] = (short)outSample;
                }
            }
            samplePos += stepPitched;
        }
        return new(isDead);
    }
}
