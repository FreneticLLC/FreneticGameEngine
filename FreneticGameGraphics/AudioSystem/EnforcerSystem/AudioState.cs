using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameGraphics.AudioSystem.EnforcerSystem
{
    /// <summary>
    /// Represents the various states an audio instance can be in.
    /// </summary>
    public enum AudioState : byte
    {
        /// <summary>
        /// The audio is waiting for start signal.
        /// </summary>
        WAITING = 0,
        /// <summary>
        /// The audio is actively playing.
        /// </summary>
        PLAYING = 1,
        /// <summary>
        /// The audio is done playing.
        /// </summary>
        DONE = 2,
        /// <summary>
        /// The audio has been told to stop.
        /// </summary>
        STOP = 3,
        /// <summary>
        /// The audio is paused and waiting a resume signal.
        /// </summary>
        PAUSED = 4
    }
}
