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
using System.Threading.Tasks;

namespace FGECore.UtilitySystems;

/// <summary>Helper class to measure the amount of time a section of code contributes to the overall frame time.</summary>
public class PerformanceTimer(string _name)
{
    /// <summary>Can be used to globally disable performance timing if preferred.</summary>
    public static bool DisableTimers = false;

    /// <summary>The name of this <see cref="PerformanceTimer"/>, if any.</summary>
    public string Name = _name;

    /// <summary>Internal data for a <see cref="PerformanceTimer"/>.</summary>
    public struct InternalData()
    {
        /// <summary>Value of <see cref="Stopwatch.GetTimestamp"/> when the timer started.</summary>
        public long StopwatchStartTime = 0;
    }

    /// <summary>Internal data for this <see cref="PerformanceTimer"/>.</summary>
    public InternalData Internal = new();

    /// <summary>Last frame's millisecond time.</summary>
    public long LastMS;

    /// <summary>Last frame's time duration.</summary>
    public double LastTime => LastMS / 1000.0;

    /// <summary>Highest millisecond time in the current measuring period.</summary>
    public long SpikeMS;

    /// <summary>Highest time duration in the current measuring period.</summary>
    public double SpikeTime => SpikeMS / 1000.0;

    /// <summary>Starts measuring a new frame.</summary>
    public void Start()
    {
        if (DisableTimers)
        {
            return;
        }
        Internal.StopwatchStartTime = Stopwatch.GetTimestamp();
    }

    /// <summary>Finishes measuring the current frame.</summary>
    public void Stop()
    {
        if (DisableTimers)
        {
            return;
        }
        if (Internal.StopwatchStartTime == 0)
        {
            return;
        }
        long ticks = Stopwatch.GetTimestamp() - Internal.StopwatchStartTime;
        double secElapsed = ticks / (double)Stopwatch.Frequency;
        Internal.StopwatchStartTime = 0;
        LastMS = (long)(secElapsed * 1000);
        if (LastMS > SpikeMS)
        {
            SpikeMS = LastMS;
        }
    }

    /// <summary>Creates a simple string display of this timer.</summary>
    public override string ToString()
    {
        return $"{Name}: {LastMS:000}ms normal, {SpikeMS:000}ms spike";
    }
}
