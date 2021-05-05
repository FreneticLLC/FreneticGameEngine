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
using System.Threading.Tasks;
using System.Diagnostics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>
    /// Timing statistics for a <see cref="View3D"/> (when enabled).
    /// </summary>
    public class View3DStats
    {
        /// <summary>
        /// Time statistic: Shadows.
        /// </summary>
        public double ShadowTime;

        /// <summary>
        /// Time statistic: FBO.
        /// </summary>
        public double FBOTime;

        /// <summary>
        /// Time statistic: Lights.
        /// </summary>
        public double LightsTime;

        /// <summary>
        /// Time statistic: Total.
        /// </summary>
        public double TotalTime;

        /// <summary>
        /// Time statistic (spike): Shadows.
        /// </summary>
        public double ShadowSpikeTime;

        /// <summary>
        /// Time statistic (spike): FBO.
        /// </summary>
        public double FBOSpikeTime;

        /// <summary>
        /// Time statistic (spike): Lights.
        /// </summary>
        public double LightsSpikeTime;

        /// <summary>
        /// Time statistic (spike): Total.
        /// </summary>
        public double TotalSpikeTime;

        /// <summary>
        /// Statistics timer for <see cref="TotalTime"/> and <see cref="TotalSpikeTime"/>.
        /// </summary>
        public readonly Stopwatch Render_Timer = new Stopwatch();

        /// <summary>
        /// Statistics timer for <see cref="ShadowTime"/> and <see cref="ShadowSpikeTime"/>.
        /// </summary>
        public readonly Stopwatch Shadows_Timer = new Stopwatch();

        /// <summary>
        /// Statistics timer for <see cref="FBOTime"/> and <see cref="FBOSpikeTime"/>.
        /// </summary>
        public readonly Stopwatch GBuffer_Timer = new Stopwatch();

        /// <summary>
        /// Statistics timer for <see cref="LightsTime"/> and <see cref="LightsSpikeTime"/>.
        /// </summary>
        public readonly Stopwatch Lights_Timer = new Stopwatch();
    }
}
