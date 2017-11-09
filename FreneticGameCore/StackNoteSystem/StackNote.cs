using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore.StackNoteSystem
{
    /// <summary>
    /// Represents a note about current stack data.
    /// </summary>
    public class StackNote
    {
        /// <summary>
        /// The stack note.
        /// </summary>
        public string Note;

        /// <summary>
        /// A related object, if any.
        /// </summary>
        public Object Related;

        /// <summary>
        /// Gets a string output for this stack note.
        /// </summary>
        /// <returns>The string output.</returns>
        public override string ToString()
        {
            try
            {
                return Note + " (" + (Related ?? "None") + ")";
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                return Note + "(Exception parsing Related: " + ex.ToString() + ")";
            }
        }
    }
}
