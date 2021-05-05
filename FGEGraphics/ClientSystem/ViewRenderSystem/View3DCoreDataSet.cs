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

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>
    /// Core data tracked by all <see cref="View3D"/> instances.
    /// </summary>
    public class View3DCoreDataSet
    {
        /// <summary>
        /// The actual <see cref="View3D"/> object.
        /// </summary>
        public View3D View;

        /// <summary>
        /// The backing 3D engine.
        /// </summary>
        public GameEngine3D Engine;

        /// <summary>
        /// The backing shader set.
        /// </summary>
        public GE3DShaders Shaders;

        /// <summary>
        /// An internal data-generation helper for use by <see cref="View3D"/>.
        /// </summary>
        public View3DGenerationHelper GenerationHelper;

        /// <summary>
        /// Statistical data about this <see cref="View3D"/>.
        /// </summary>
        public View3DStats Statistics;

        /// <summary>
        /// Data related to the current state of this <see cref="View3D"/>.
        /// </summary>
        public View3DState State;

        /// <summary>
        /// Configuration of this <see cref="View3D"/>.
        /// </summary>
        public View3DConfiguration Config;

        /// <summary>
        /// Internal data for this <see cref="View3D"/>.
        /// </summary>
        public View3DInternalData Internal;

        /// <summary>
        /// Copies the <see cref="View3DCoreDataSet"/> data from a <see cref="View3D"/> instance to this object.
        /// </summary>
        /// <param name="_view">The <see cref="View3D"/> to copy from.</param>
        public void CopyDataFrom(View3D _view)
        {
            View = _view;
            Engine = View.Engine;
            Shaders = View.Shaders;
            GenerationHelper = View.GenerationHelper;
            Statistics = View.Statistics;
            State = View.State;
            Config = View.Config;
            Internal = View.Internal;
        }
    }
}
