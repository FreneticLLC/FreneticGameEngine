using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticGameCore;

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
