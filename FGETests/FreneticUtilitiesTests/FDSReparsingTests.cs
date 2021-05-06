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
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using NUnit.Framework;

namespace FGETests.FreneticUtilitiesTests
{
    /// <summary>
    /// Tests expectations of the Frenetic Data Syntax.
    /// </summary>
    [TestFixture]
    public class FDSReparsingTests : FGETest
    {
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
        /// Prepares the basics.
        /// </summary>
        [OneTimeSetUp]
        public static void PreInit()
        {
            Setup();
            TestSection = new FDSSection(TEST_FILE);
        }

        /// <summary>
        /// <see cref="FDSSection"/> of <see cref="TEST_FILE"/> for the following tests to use.
        /// </summary>
        public static FDSSection TestSection;

        /// <summary>
        /// Confirm that the FDS section parsed has the correct set of keys.
        /// </summary>
        [Test]
        public static void TestKeyPresence()
        {
            Assert.That(TestSection.HasKey("my root section 1"), "Key exists!");
            Assert.That(!TestSection.HasKey("my root section"), "Key shouldn't exist!");
        }

        /// <summary>
        /// Confirm that the FDS section parsed has the proper object types where they should be.
        /// </summary>
        [Test]
        public static void TestExpectedObjects()
        {
            Assert.AreEqual(3, TestSection.GetInt("my root section 1.my_sub_section.my numeric key"), "Key == 3!");
            Assert.AreEqual(3.14159, TestSection.GetDouble("my root section 1.my_sub_section.my decimal key"), "Key == 3.14159!");
            Assert.AreEqual("alpha", TestSection.GetString("my root section 1.my other section.my string key"), "Key == alpha!");
            Assert.AreEqual(StringConversionHelper.UTF8Encoding.GetString(TestSection.GetData("my second root section.my binary key").Internal as byte[]),
                "Hello world, and all who inhabit it!", "Key string from binary check!");
        }

        /// <summary>
        /// Confirm that the FDS section parsed has the proper list values.
        /// </summary>
        [Test]
        public static void TestList()
        {
            List<string> list = TestSection.GetStringList("my second root section.my list key");
            Assert.AreEqual(2, list.Count, "Key->List count");
            Assert.AreEqual("1", list[0], "Key->List[0] yields 1!");
            Assert.AreEqual("two", list[1], "Key->List[1] yields two!");
        }

        /// <summary>
        /// Confirm that the FDS section parsed has the proper comments.
        /// </summary>
        [Test]
        public static void TestComments()
        {
            Assert.AreEqual("MyTestFile.fds", TestSection.GetData("my root section 1").PrecedingComments[0].Trim(), "Root comment!");
        }
    }
}
