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
using FGECore.UtilitySystems;

namespace FGECore.StackNoteSystem
{
    /// <summary>Represents a note about current stack data.</summary>
    public struct StackNote
    {
        /// <summary>The stack note.</summary>
        public string Note;

        /// <summary>A related object, if any.</summary>
        public object Related;

        /// <summary>Gets a string output for this stack note.</summary>
        public override string ToString()
        {
            try
            {
                string relatedString = Related == null ? "None" : Related.ToString().Replace("\n", "\\n");
                return $"{Note} ({relatedString})";
            }
            catch (Exception ex)
            {
                CommonUtilities.CheckException(ex);
                return Note + $"(Exception parsing {nameof(StackNote)}.{nameof(Related)}: {ex})";
            }
        }
    }
}
