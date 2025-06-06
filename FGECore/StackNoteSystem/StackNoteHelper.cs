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
using System.Threading;
using FGECore.CoreSystems;

namespace FGECore.StackNoteSystem;

/// <summary>Static helper for getting the current stack note set for any thread.</summary>
public static class StackNoteHelper
{
    /// <summary>Gets the current note set for the current thread.</summary>
    public static StackNoteSet Notes
    {
        get
        {
            if (InternalCurrentNotes != null)
            {
                return InternalCurrentNotes;
            }
            InternalCurrentNotes = new StackNoteSet();
            return InternalCurrentNotes;
        }
    }

    /// <summary>The current internal set of current notes (thread-static).</summary>
    [ThreadStatic]
    public static StackNoteSet InternalCurrentNotes;

    /// <summary>Pushes a new entry to the current note stack.</summary>
    /// <param name="note">The note.</param>
    /// <param name="relatedObj">A related object, if any.</param>
    public static void Push(string note, object relatedObj = null)
    {
        //OutputType.DEBUG.Output($"Thread {Thread.CurrentThread.Name ?? Environment.CurrentManagedThreadId.ToString()} pushing note {note} with related object {relatedObj}.");
        Notes.Push(note, relatedObj);
    }

    /// <summary>Pops the current entry from the current note stack.</summary>
    public static void Pop()
    {
        Notes.Pop();
    }

    /// <summary>Pushes a new entry to the current note stack with a 'using' pattern.</summary>
    /// <param name="note">The note.</param>
    /// <param name="relatedObj">A related object, if any.</param>
    public static NotePush UsePush(string note, object relatedObj = null)
    {
        return new NotePush(note, relatedObj);
    }

    /// <summary>Helper struct for pushing a note and automatically popping it when disposed.</summary>
    public struct NotePush : IDisposable
    {
        /// <summary>Constructs a new note push.</summary>
        public NotePush(string note, object relatedObj)
        {
            Push(note, relatedObj);
        }

        /// <summary>Disposes the note push.</summary>
        public readonly void Dispose()
        {
            Pop();
        }
    }
}
