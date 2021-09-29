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

namespace FGECore.UtilitySystems
{
    /// <summary>Helper class to measure the amount of time a section of code contributes to the overall frame time.</summary>
    public class PerformanceTimer
    {
        /// <summary>Can be used to globally disable performance timing if preferred.</summary>
        public static bool DISABLE = false;

        /// <summary>The name of this <see cref="PerformanceTimer"/>, if any.</summary>
        public string Name;

        /// <summary>Constructs a new <see cref="PerformanceTimer"/> with a given name.</summary>
        public PerformanceTimer(string _name)
        {
            Name = _name;
        }

        /// <summary>The actual internal <see cref="Stopwatch"/> instance used for timing things.</summary>
        public Stopwatch Internal = new Stopwatch();

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
            if (DISABLE)
            {
                return;
            }
            Internal.Restart();
        }

        /// <summary>Finishes measuring the current frame.</summary>
        public void Stop()
        {
            if (DISABLE)
            {
                return;
            }
            Internal.Stop();
            LastMS = Internal.ElapsedMilliseconds;
            if (LastMS > SpikeMS)
            {
                SpikeMS = LastMS;
            }
        }

        /// <summary>Creates a simple string display of this timer.</summary>
        public override string ToString()
        {
            return $"{Name}: {LastMS}ms normal, {SpikeMS}ms spike";
        }
    }
}
