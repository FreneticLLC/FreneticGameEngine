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
using NUnit.Framework;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;

namespace FGETests
{
    /// <summary>
    /// Tests general expectations of the C# language and the system running it.
    /// </summary>
    [TestFixture]
    public class GeneralSystemTests : FGETest
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
        /// Confirm that number translation locale is correct.
        /// </summary>
        [Test]
        public static void TestFloatStrings()
        {
            Assert.That((3.2).ToString().Equals("3.2"), "Numbers (double) 3.2 != " + (3.2).ToString() + ", possibly a locale issue?");
            Assert.That((1.9f).ToString().Equals("1.9"), "Numbers (float) 1.9 != " + (1.9f).ToString() + ", possibly a locale issue?");
        }

        /// <summary>
        /// Confirms that bit endianness is correct.
        /// </summary>
        [Test]
        public static void TestEndianness()
        {
            if (!BitConverter.IsLittleEndian)
            {
                Assert.Fail("BitConverter identifies this system as big endian, which is not currently supported by the engine!");
            }
        }

        /// <summary>
        /// Confirms that integer bit encoding is correct.
        /// </summary>
        [Test]
        public static void TestIntBits()
        {
            byte[] bI = PrimitiveConversionHelper.Int32ToBytes(1 + 512);
            Assert.That(bI.Length == 4, "Bit length (int->bytes)");
            Assert.That(bI[0] == 1, "Bit contents (int->bytes)[0]");
            Assert.That(bI[1] == 2, "Bit contents (int->bytes)[1]");
            Assert.That(bI[2] == 0, "Bit contents (int->bytes)[2]");
            Assert.That(bI[3] == 0, "Bit contents (int->bytes)[3]");
        }

        /// <summary>
        /// Confirms that floating point bit encoding is correct.
        /// </summary>
        [Test]
        public static void TestFloatBits()
        {
            byte[] bF = PrimitiveConversionHelper.Float32ToBytes(127.125f);
            Assert.That(bF.Length == 4, "Bit length (float->bytes)");
            Assert.That(bF[0] == 0, "Bit contents (float->bytes)[0]");
            Assert.That(bF[1] == 64, "Bit contents (float->bytes)[1]");
            Assert.That(bF[2] == 254, "Bit contents (float->bytes)[2]");
            Assert.That(bF[3] == 66, "Bit contents (float->bytes)[3]");
        }

        /// <summary>
        /// Confirms that double parseback is correct.
        /// </summary>
        [Test]
        public static void TestDoubleParseback()
        {
            Assert.That(PrimitiveConversionHelper.BytesToDouble64(PrimitiveConversionHelper.Double64ToBytes(1.73e5)) == 1.73e5, "Double parseback validity");
        }
    }
}
