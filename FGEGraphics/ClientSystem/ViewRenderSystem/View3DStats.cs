//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>Timing statistics for a <see cref="View3D"/> (when enabled).</summary>
    public class View3DStats
    {
        /// <summary>Time contributed by the named specific part of performance processing.</summary>
        public PerformanceTimer Shadows = new PerformanceTimer("Shadows"),
            FrameBuffer = new PerformanceTimer("FrameBuffer"),
            Lights = new PerformanceTimer("Lights"),
            Total = new PerformanceTimer("RenderTotal");

        /// <summary>How much delta time has accumulated since the last spike reset.</summary>
        public double TimeSinceSpikeReset = 0;

        /// <summary>How much delta time shall accumulate before spikes are reset.</summary>
        public double TimeToRetainSpikes = 5;

        /// <summary>Resets perf spike times when needed.</summary>
        public void Check(double delta)
        {
            if (PerformanceTimer.DISABLE)
            {
                return;
            }
            TimeSinceSpikeReset += delta;
            if (TimeSinceSpikeReset >= TimeToRetainSpikes)
            {
                TimeSinceSpikeReset = 0;
                Shadows.SpikeMS = 0;
                FrameBuffer.SpikeMS = 0;
                Lights.SpikeMS = 0;
                Total.SpikeMS = 0;
            }
        }
    }
}
