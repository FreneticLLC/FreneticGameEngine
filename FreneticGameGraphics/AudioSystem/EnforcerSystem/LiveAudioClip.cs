using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameGraphics.AudioSystem.EnforcerSystem
{
    /// <summary>
    /// Represents a live audio clip.
    /// The data for a loaded piece of audio.
    /// </summary>
    public class LiveAudioClip
    {
        /// <summary>
        /// The data for this clip.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// The number of channels in this clip.
        /// </summary>
        public byte Channels;
    }
}
