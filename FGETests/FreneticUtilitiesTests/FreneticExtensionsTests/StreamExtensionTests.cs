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
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticToolkit;

namespace FGETests.FreneticUtilitiesTests.FreneticExtensionsTests
{
    /// <summary>
    /// Tests expectations of <see cref="StreamExtensions"/>.
    /// </summary>
    [TestFixture]
    public class StreamExtensionTests : FGETest
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
        /// Tests "AllLinesOfText"
        /// </summary>
        [Test]
        public static void AllLinesOfTextTest()
        {
            string input = "Wow\nThis\nIs a big ol'\nlist of text\nyep";
            string[] splitInput = input.Split('\n');
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            IEnumerable<string> result = stream.AllLinesOfText();
            int i = 0;
            foreach (string str in result)
            {
                Assert.AreEqual(splitInput[i++], str, $"Stream AllLinesOfText broke at line {i}");
            }
            Assert.AreEqual(5, i, "Stream AllLinesOfText length wrong");
        }
    }
}
