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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticToolkit;

namespace FGETests
{
    /// <summary>
    /// Tests expectations of the Frenetic Data Syntax.
    /// </summary>
    [TestFixture]
    class FDSCoreExpectations : FGETest
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
        /// The primary test file.
        /// </summary>
        public const string TEST_FILE =
            "# MyTestFile.fds\r\n"
            + "my root section 1:\r\n"
            + "  # This represents some data\r\n"
            + "  my_sub_section:\r\n"
            + "    my numeric key: 3\r\n"
            + "    my decimal key: 3.14159\r\n"
            + "  my other section:\r\n"
            + "    my string key: alpha\r\n"
            + "my second root section:\r\n"
            + "  # contains UTF-8 text: Hello world, and all who inhabit it!\r\n"
            + "  my binary key= SGVsbG8gd29ybGQsIGFuZCBhbGwgd2hvIGluaGFiaXQgaXQh\r\n"
            + "  my list key:\r\n"
            + "  # Integer test\r\n"
            + "  - 1\r\n"
            + "  # Text!\r\n"
            + "  - two\r\n"
            ;

        /// <summary>
        /// Confirm that FDS parses a file correctly.
        /// </summary>
        [Test]
        public static void TestReadInValid()
        {
            FDSSection test_section = new FDSSection(TEST_FILE);
            Assert.That(test_section.HasKey("my root section 1"), "Key exists!");
            Assert.That(!test_section.HasKey("my root section"), "Key shouldn't exist!");
            Assert.AreEqual(test_section.GetInt("my root section 1.my_sub_section.my numeric key"), 3, "Key == 3!");
            Assert.AreEqual(test_section.GetDouble("my root section 1.my_sub_section.my decimal key"), 3.14159, "Key == 3.14159!");
            Assert.AreEqual(test_section.GetString("my root section 1.my other section.my string key"), "alpha", "Key == alpha!");
            Assert.AreEqual(StringConversionHelper.UTF8Encoding.GetString(test_section.GetData("my second root section.my binary key").Internal as byte[]),
                "Hello world, and all who inhabit it!", "Key string from binary check!");
           List<string> list = test_section.GetStringList("my second root section.my list key");
            Assert.AreEqual(list[0], "1", "Key->List yields 1!");
            Assert.AreEqual(list[1], "two", "Key->List yields two!");
            Assert.That(test_section.GetData("my root section 1").PrecedingComments[0].Trim() == "MyTestFile.fds", "Root comment!");
        }
    }
}
