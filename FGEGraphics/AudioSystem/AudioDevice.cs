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

namespace FGEGraphics.AudioSystem;

/// <summary>
/// Represents a generic audio device. Contains key metadata to identify the device uniquely. Actual functional implementation and used is in the "AudioInternals" subpackage.
/// </summary>
public class AudioDevice
{
    /// <summary>The "friendly name" for this device.</summary>
    public string Name;

    /// <summary>A 'proper' unique-ID string for this device.</summary>
    public string UID;

    /// <summary>An 'improper' internal unique-ID string for this device.</summary>
    public string InternalID;

    /// <summary>A longer description of this device that may help identify it.</summary>
    public string FullDescriptionText;

    /// <summary>A list of ID strings, sorted such that the "most unique and reliable" is firstmost, and "least reliable" is lastmost. This is useful for recovering previous user device selection.</summary>
    public string[] IDs;
}
