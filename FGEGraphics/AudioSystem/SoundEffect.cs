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

namespace FGEGraphics.AudioSystem;

/// <summary>Represents a general sound effect.</summary>
public class SoundEffect
{
    /// <summary>The name of the effect.</summary>
    public string Name;

    /// <summary>A quick-and-dirty event for when the sound effect is loaded.</summary>
    public Action Loaded;

    /// <summary>The time this sound effect was last used.</summary>
    public double LastUse = 0.0;

    /// <summary>The internal data for this clip.</summary>
    public short[] Data;

    /// <summary>The number of channels in this clip (if it has been loaded).</summary>
    public byte Channels;
}
