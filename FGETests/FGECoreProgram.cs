using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FreneticGameCore;

namespace FGETests
{
    /// <summary>
    /// Tests expectations of the FGE Core program class.
    /// </summary>
    [TestFixture]
    public class FGECoreProgram : FGETest
    {
        /// <summary>
        /// Prepares the basics.
        /// </summary>
        [OneTimeSetUp]
        public static void PreInit()
        {
            Setup();
        }

        /// <summary>
        /// Confirm that the program provides the right string values.
        /// </summary>
        [Test]
        public static void TestIDs()
        {
            Assert.That(Program.GameName == FGETestProgram.NAME, "Game name bounces wrong!");
            Assert.That(Program.GameVersion == FGETestProgram.VERSION, "Game version bounces wrong!");
            Assert.That(Program.GameVersionDescription == FGETestProgram.VERSDESC, "Game version-description bounces wrong!");
        }
    }
}