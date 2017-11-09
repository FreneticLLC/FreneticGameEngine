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
            return string.Join("\n -> ", Notes);
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
