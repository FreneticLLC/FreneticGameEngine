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

namespace FGECore.StackNoteSystem;

/// <summary>Represents a current set of stack notes.</summary>
public class StackNoteSet
{
    /// <summary>Current stack of notes.</summary>
    public StackNote[] Notes = new StackNote[128];

    /// <summary>How deep the the stack is currently.</summary>
    public int Depth = 0;

    /// <summary>How deep the the stack has gone.</summary>
    public int MaxDepth = 0;

    /// <summary>Gets a string output of the current stack notes.</summary>
    public override string ToString()
    {
        string main = string.Concat(Notes.Take(Depth).Reverse().Select(note => $" -> {note}\n"));
        if (MaxDepth > Depth)
        {
            return main + "\nOver depth:\n" + string.Concat(Notes.Skip(Depth).Take(Math.Min(2, MaxDepth - Depth)).Reverse().Select(note => $" -> {note}\n"));
        }
        return main;
    }

    /// <summary>Pushes a new entry to the note stack.</summary>
    /// <param name="note">The note.</param>
    /// <param name="relatedObj">A related object, if any.</param>
    public void Push(string note, object relatedObj = null)
    {
        if (Depth == Notes.Length)
        {
            StackNote[] expanded = new StackNote[Notes.Length * 2];
            Array.Copy(Notes, expanded, Notes.Length);
            Notes = expanded;
        }
        Notes[Depth++] = new StackNote() { Note = note, Related = relatedObj };
        MaxDepth = Math.Max(MaxDepth, Depth);
    }

    /// <summary>Pops the current entry from the note stack.</summary>
    public void Pop()
    {
        Depth--;
    }

    /// <summary>Cleans out any references from the stack note set.</summary>
    public void Clean()
    {
        for (int index = Depth + 1; index <= MaxDepth; index++)
        {
            Notes[index] = new StackNote();
        }
        MaxDepth = Depth;
    }
}
