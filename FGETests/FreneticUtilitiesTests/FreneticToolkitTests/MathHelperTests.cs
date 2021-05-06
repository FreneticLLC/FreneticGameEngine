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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using NUnit.Framework;

namespace FGETests.FreneticUtilitiesTests.FreneticToolkitTests
{
    /// <summary>
    /// Tests expectations of <see cref="MathHelper"/>.
    /// </summary>
    public class MathHelperTests : FGETest
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
        /// Tests "NextPowerOfTwo".
        /// </summary>
        [Test]
        public static void NextPowerOfTwoTest()
        {
            SortedDictionary<int, int> testPairs = new SortedDictionary<int, int>() {
                { 1, 1 }, { 2, 2 }, { 3, 4 }, { 4, 4 }, { 5, 8 }, { 6, 8 }, { 7, 8 }, { 8, 8 }, { 9, 16 },
                { 10, 16 }, { 11, 16 }, { 12, 16 }, { 13, 16 }, { 14, 16 }, { 15, 16 }, { 16, 16 }, { 17, 32 }, { 18, 32 },
                { 20, 32 }, { 30, 32 }, { 40, 64 }, { 50, 64 }, { 60, 64 }, { 63, 64 }, { 64, 64 }, { 65, 128 },
                { 128, 128 }, { 256, 256 }, { 300, 512 }, { 800, 1024 }, { 10000, 16384 }, { 150000, 262144 }, { 9999999, 16777216 } };
            foreach (KeyValuePair<int, int> pair in testPairs)
            {
                Assert.AreEqual(pair.Value, MathHelper.NextPowerOfTwo(pair.Key), $"NextPowerOfTwo returned wrong value for {pair.Key}");
            }
        }

        /// <summary>
        /// Tests "StepTowards".
        /// </summary>
        [Test]
        public static void StepTowardsTest()
        {
            const double refEpsilon = 0.00001;
            Assert.AreEqual(1.0, MathHelper.StepTowards(0.5, 1.0, 0.5), refEpsilon, "StepTowards gave bad value for 0.5, 1.0, 0.5");
            Assert.AreEqual(1.0, MathHelper.StepTowards(0.5, 1.0, 0.8), refEpsilon, "StepTowards gave bad value for 0.5, 1.0, 0.8");
            Assert.AreEqual(1.0, MathHelper.StepTowards(0.8, 1.0, 0.5), refEpsilon, "StepTowards gave bad value for 0.8, 1.0, 0.5");
            Assert.AreEqual(1.0, MathHelper.StepTowards(1.5, 1.0, 0.5), refEpsilon, "StepTowards gave bad value for 1.5, 1.0, 0.5");
            Assert.AreEqual(1.0, MathHelper.StepTowards(1.5, 1.0, 0.8), refEpsilon, "StepTowards gave bad value for 1.5, 1.0, 0.8");
            Assert.AreEqual(1.0, MathHelper.StepTowards(1.8, 1.0, 1.0), refEpsilon, "StepTowards gave bad value for 1.8, 1.0, 1.0");
            Assert.AreEqual(2.0, MathHelper.StepTowards(1.5, 2.0, 0.5), refEpsilon, "StepTowards gave bad value for 1.5, 2.0, 0.5");
            Assert.AreEqual(1.5, MathHelper.StepTowards(2.0, 1.0, 0.5), refEpsilon, "StepTowards gave bad value for 2.0, 1.0, 0.5");
            Assert.AreEqual(1.5, MathHelper.StepTowards(1.0, 2.0, 0.5), refEpsilon, "StepTowards gave bad value for 1.0, 2.0, 0.5");
        }

        /// <summary>
        /// Tests "IsCloseTo".
        /// </summary>
        [Test]
        public static void IsCloseToTest()
        {
            Assert.That(MathHelper.IsCloseTo(0.6, 1.0, 0.5), "IsCloseTo failed for 0.6, 1.0, 0.5");
            Assert.That(MathHelper.IsCloseTo(0.9, 1.0, 0.5), "IsCloseTo failed for 0.9, 1.0, 0.5");
            Assert.That(!MathHelper.IsCloseTo(0.4, 1.0, 0.5), "!IsCloseTo failed for 0.4, 1.0, 0.5");
            Assert.That(!MathHelper.IsCloseTo(0.9, 1.0, 0.05), "!IsCloseTo failed for 0.9, 1.0, 0.05");
            Assert.That(MathHelper.IsCloseTo(0.9, 2.0, 3.0), "IsCloseTo failed for 0.9, 2.0, 3.0");
            Assert.That(MathHelper.IsCloseTo(0.9, 0.0, 3.0), "IsCloseTo failed for 0.9, 0.0, 3.0");
            Assert.That(!MathHelper.IsCloseTo(0.9, 0.0, 0.5), "!IsCloseTo failed for 0.9, 0.0, 0.5");
        }
    }
}
