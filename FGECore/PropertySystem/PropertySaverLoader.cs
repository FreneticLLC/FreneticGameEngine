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

namespace FGECore.PropertySystem
{
    /// <summary>
    /// Represents a helper to save or load an object.
    /// </summary>
    public class PropertySaverLoader
    {
        /// <summary>
        /// The save name.
        /// </summary>
        public string SaveString;

        /// <summary>
        /// The save method.
        /// </summary>
        public Func<object, byte[]> Saver;

        /// <summary>
        /// The load method.
        /// </summary>
        public Func<byte[], object> Loader;
    }

}
