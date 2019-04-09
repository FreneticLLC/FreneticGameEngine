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
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;

namespace FGETests
{
    /// <summary>
    /// Represents any test in Voxalia. Should be derived from.
    /// </summary>
    public abstract class FGETest
    {
        /// <summary>
        /// ALWAYS call this in a test's static OneTimeSetUp!
        /// </summary>
        public static void Setup()
        {
            Program.PreInit(new FGETestProgram());
        }
    }
	
    /// <summary>
    /// Represents a test program.
    /// </summary>
	public class FGETestProgram : Program
	{
        /// <summary>
        /// Name of the program.
        /// </summary>
        public const string NAME = "FGE Tests";

        /// <summary>
        /// Version of the program.
        /// </summary>
        public const string VERSION = "1.0.0.0";

        /// <summary>
        /// Version-Description of the program.
        /// </summary>
        public const string VERSDESC = "Test-Only";

        /// <summary>
        /// Construct the tester.
        /// </summary>
        public FGETestProgram()
            : base(NAME, VERSION, VERSDESC)
        {
        }
	}
}
