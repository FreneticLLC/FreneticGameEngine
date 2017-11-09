using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore.StackNoteSystem
{
    /// <summary>
    /// Static helper for getting the current stack note set for any thread.
    /// </summary>
    public static class StackNoteHelper
    {
        /// <summary>
        /// Gets the current note set for the current thread.
        /// </summary>
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

        /// <summary>
        /// The current internal set of current notes (thread-static).
        /// </summary>
        [ThreadStatic]
        public static StackNoteSet InternalCurrentNotes;

        /// <summary>
        /// Pushes a new entry to the current note stack.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="rel">A related object, if any.</param>
        public static void Push(string note, Object rel)
        {
            Notes.Push(note, rel);
        }

        /// <summary>
        /// Pops the current entry from the current note stack.
        /// </summary>
        public static void Pop()
        {
            Notes.Pop();
        }
    }
}
