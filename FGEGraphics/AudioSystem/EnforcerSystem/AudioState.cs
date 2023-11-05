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

/// <summary>Represents the various states an audio instance can be in.</summary>
public enum AudioState : byte
{
    /// <summary>The audio is waiting for start signal.</summary>
    WAITING = 0,
    /// <summary>The audio is actively playing.</summary>
    PLAYING = 1,
    /// <summary>The audio is done playing.</summary>
    DONE = 2,
    /// <summary>The audio has been told to stop.</summary>
    STOP = 3,
    /// <summary>The audio is paused and waiting a resume signal.</summary>
    PAUSED = 4
}
