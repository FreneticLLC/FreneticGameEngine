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

namespace FreneticGameCore.StackNoteSystem
{
    /// <summary>
    /// Represents a current set of stack notes.
    /// </summary>
    public class StackNoteSet
    {
        /// <summary>
        /// Current stack of notes.
        /// </summary>
        public Stack<StackNote> Notes = new Stack<StackNote>(128);

        /// <summary>
        /// Gets a string output of the current stack notes.
        /// </summary>
        /// <returns>The string output.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Notes.Count * 50);
            foreach (StackNote note in Notes)
            {
                sb.Append(" -> " + note + "\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Pushes a new entry to the note stack.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="rel">A related object, if any.</param>
        public void Push(string note, Object rel = null)
        {
            Notes.Push(new StackNote() { Note = note, Related = rel });
        }

        /// <summary>
        /// Pops the current entry from the note stack.
        /// </summary>
        public void Pop()
        {
            Notes.Pop();
        }
    }
}
