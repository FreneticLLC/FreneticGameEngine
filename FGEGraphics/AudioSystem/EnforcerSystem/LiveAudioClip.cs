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

namespace FGEGraphics.AudioSystem.EnforcerSystem;

/// <summary>
/// Represents a live audio clip.
/// The data for a loaded piece of audio.
/// </summary>
public class LiveAudioClip
{
    /// <summary>The data for this clip.</summary>
    public byte[] Data;

    /// <summary>The number of channels in this clip.</summary>
    public byte Channels;
}
