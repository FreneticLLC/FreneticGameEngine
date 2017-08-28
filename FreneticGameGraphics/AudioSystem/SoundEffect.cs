using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.AudioSystem.EnforcerSystem;

namespace FreneticGameGraphics.AudioSystem
{
    /// <summary>
    /// Represents a general sound effect.
    /// </summary>
    public class SoundEffect
    {
        /// <summary>
        /// The name of the effect.
        /// </summary>
        public string Name;

        /// <summary>
        /// The internal OpenAL ID, if one is relevant.
        /// </summary>
        public int Internal;

        /// <summary>
        /// A quick-and-dirty event for when the sound effect is loaded.
        /// </summary>
        public EventHandler<EventArgs> Loaded;

        /// <summary>
        /// The time this sound effect was last used.
        /// </summary>
        public double LastUse = 0.0;

        /// <summary>
        /// The internal enforcer clip, if one is relevant.
        /// </summary>
        public LiveAudioClip Clip;
    }
}
