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

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Abstract base class for an internal audio engine backer.</summary>
public abstract class GenericAudioBacker
{
    /// <summary>Any pre-initialization required.</summary>
    public abstract void PreInit();

    /// <summary>Initialize the handler for a given device.</summary>
    /// <param name="device">The device to use. Use null to choose default.</param>
    public abstract void SelectDeviceAndInit(AudioDevice device);

    /// <summary>Build and return a list of all available audio devices.</summary>
    public abstract List<AudioDevice> ListAllAudioDevices();

    /// <summary>Attaches this context to the current thread, if needed.</summary>
    public abstract void MakeCurrent();

    /// <summary>Shut down all backings, disable all systems, de-allocate all memory.</summary>
    public abstract void Shutdown();

    /// <summary>Preprocess the backer for a single framestep. Returns true if there's room to add anything, or false if the instance is already full on buffers.</summary>
    public abstract bool PreprocessStep();

    /// <summary>Gather data from the internal audio engine and send it forward to the backing system to play.</summary>
    public abstract void SendNextBuffer(FGE3DAudioEngine engine);
}
