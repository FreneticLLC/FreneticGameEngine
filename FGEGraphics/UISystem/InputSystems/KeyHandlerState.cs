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

namespace FGEGraphics.UISystem.InputSystems;

/// <summary>Full state of the keyboard from a point in time.</summary>
public class KeyHandlerState
{
    /// <summary>All text that was written since the information was last retrieved.</summary>
    public string KeyboardString = "";

    /// <summary>How many backspaces were pressed, excluding ones that modified the KeyboardString.</summary>
    public int InitBS = 0;

    /// <summary>How many deletes were pressed.</summary>
    public int EndDelete = 0;

    /// <summary>Whether the control key is currently down, primarily for internal purposes.</summary>
    public bool ControlDown = false;

    /// <summary>Whether COPY (CTRL+C) was pressed.</summary>
    public bool CopyPressed = false;

    /// <summary>Whether ALL (CTRL+A) was pressed.</summary>
    public bool AllPressed = false;

    /// <summary>The number of times PageUp was pressed minus the number of times PageDown was pressed.</summary>
    public int Pages = 0;

    /// <summary>The number of times the UP arrow was pressed minus the number of times the DOWN arrow was pressed.</summary>
    public int Scrolls = 0;

    /// <summary>The number of times the RIGHT arrow was pressed minus the number of times the LEFT arrow was pressed.
    /// <para>Note that the 'home' key will be read as 9000 lefts, and the 'end' key will be read as 9000 rights.</para></summary>
    public int LeftRights = 0;

    /// <summary>Whether the escape key was pressed at all.</summary>
    public bool Escaped = false;
}
