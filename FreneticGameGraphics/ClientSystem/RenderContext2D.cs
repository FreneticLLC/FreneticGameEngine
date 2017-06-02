using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a 2D rendering context.
    /// </summary>
    public class RenderContext2D
    {
        /// <summary>
        /// The backing engine.
        /// </summary>
        public GameEngine2D Engine;

        /// <summary>
        /// Width of the view.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the view.
        /// </summary>
        public int Height;

        /// <summary>
        /// The zoom of the view.
        /// </summary>
        public float Zoom;

        /// <summary>
        /// Whether the system is currently calculating shadows.
        /// </summary>
        public bool CalcShadows = false;

        /// <summary>
        /// The multiplier for zoom effects.
        /// </summary>
        public float ZoomMultiplier;

        /// <summary>
        /// The center of the 2D view.
        /// </summary>
        public Vector2 ViewCenter;

        /// <summary>
        /// The present Adder value.
        /// </summary>
        public Vector2 Adder;

        /// <summary>
        /// The present Scaler value.
        /// </summary>
        public Vector2 Scaler;
    }
}
