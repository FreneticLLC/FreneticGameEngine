//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.AudioSystem.EnforcerSystem;
using FreneticGameCore.Files;

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

        /// <summary>
        /// The file that was used to load this sound. Can be null for manually-generated textures.
        /// </summary>
        public PakkedFile FileRef;
    }
}
