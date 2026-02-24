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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.CoreSystems;
using FGECore.MathHelpers;

namespace FGECore.UtilitySystems;

/// <summary>Helpful utilities for general usage.</summary>
public static class CommonUtilities
{
    /// <summary>
    /// A thread-static random object for all non-deterministic objects to use.
    /// When possible, this should be avoided in favor of contextually available random objects.
    /// </summary>
    public static MTRandom UtilRandom
    {
        get
        {
            intRandom ??= new MTRandom();
            return intRandom;
        }
    }

    /// <summary>A thread-static random provider.</summary>
    [ThreadStatic]
    private static MTRandom intRandom;

    /// <summary>Grabs a sub section of a byte array.</summary>
    /// <param name="full">The original byte array.</param>
    /// <param name="start">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>The subset.</returns>
    public static byte[] BytesPartial(byte[] full, int start, int length)
    {
        byte[] data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = full[i + start];
        }
        return data;
    }

    /// <summary>
    /// Checks an exception for rethrow necessity.
    /// <para>This in theory should not be needed as <see cref="ThreadAbortException"/> shouldn't be miscaught, but in practice it seems to sometimes happen (might no longer apply for NET 5 update?).</para>
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    public static void CheckException(Exception ex)
    {
        if (ex is ThreadAbortException)
        {
            throw ex;
        }
    }

    /// <summary>Returns a peice of text copied a specified number of times.</summary>
    /// <param name="text">What text to copy.</param>
    /// <param name="times">How many times to copy it.</param>
    /// <returns>The repeated text.</returns>
    public static string CopyText(string text, int times)
    {
        StringBuilder toret = new(text.Length * times);
        for (int i = 0; i < times; i++)
        {
            toret.Append(text);
        }
        return toret.ToString();
    }

    /// <summary>Formats a <see cref="long"/> with comma-separated thousands ("123,456" style notation).</summary>
    /// <param name="input">The number.</param>
    /// <returns>The formatted string.</returns>
    public static string FormatThousands(long input)
    {
        // TODO: Better method here.
        string basinp = input.ToString();
        string creation = "";
        int c = 0;
        for (int i = basinp.Length - 1; i >= 0; i--)
        {
            if ((c % 3) == 0 && c != 0)
            {
                creation = basinp[i] + "," + creation;
            }
            else
            {
                creation = basinp[i] + creation;
            }
            c++;
        }
        return creation;
    }

    /// <summary><see cref="Stopwatch.Frequency"/>, per millisecond instead of per second.</summary>
    private static readonly double StopwatchFreqPerMs = Stopwatch.Frequency / 1000.0;

    /// <summary>Inverse of <see cref="StopwatchFreqPerMs"/>.</summary>
    private static readonly double StopwatchMsPerFreq = 1000.0 / Stopwatch.Frequency;

    /// <summary>Like <see cref="Thread.Sleep(int)"/>, but designed to try to hit the target duration very closely.</summary>
    /// <param name="ms">Time, in milliseconds, to wait.</param>
    /// <param name="startStamp">Optional start timestamp (<see cref="Stopwatch.GetTimestamp"/>) for the sleep period, leave as -1 to autocalculate.</param>
    public static void StableSleep(int ms, long startStamp = -1)
    {
        if (ms <= 0) // If we've overshot a target, fast return.
        {
            return;
        }
        long startTime = startStamp == -1 ? Stopwatch.GetTimestamp() : startStamp;
        long targetTime = startTime + (long)(ms * StopwatchFreqPerMs);
        long timeNow = targetTime - Stopwatch.GetTimestamp();
        while (timeNow > 0)
        {
            double timeNowMs = timeNow * StopwatchMsPerFreq;
            if (timeNowMs > 10)
            {
                // For big delays, try to sleep for most of the targeted wait - very imprecise, but aim to return early and get more precise afterward.
                // OS can target anywhere from 5ms to 15ms or do weird big values, so we're just rough estimating and hoping for the best.
                // TODO: Maybe some OS-specific awareness here to improve precision?
                Thread.Sleep((int)(timeNowMs - 10));
            }
            else if (timeNowMs > 5)
            {
                // This also risks OS precision issues.
                // Also if yield returns false this is just a busy-wait, but that's okay.
                Thread.Yield();
            }
            // At 5 and below, just busy wait (to try to hit the target as close as possible).
            timeNow = targetTime - Stopwatch.GetTimestamp();
        }
    }
}
