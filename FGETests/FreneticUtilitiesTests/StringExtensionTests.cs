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

namespace FGETests.FreneticUtilitiesTests
{
    /// <summary>
    /// Tests expectations of <see cref="StringExtensions"/>.
    /// </summary>
    [TestFixture]
    class StringExtensionTests : FGETest
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
        /// Tests "ToLowerFast", "IsAllLowerFast", "ToUpperFast", and "IsAllUpperFast"
        /// </summary>
        [Test]
        public static void CapitalizationTests()
        {
            Assert.AreEqual("wow", "WOW".ToLowerFast(), "ToLowerFast 'WOW' isn't right");
            Assert.AreEqual("big long text!", "BIG long TEXT!".ToLowerFast(), "ToLowerFast 'BIG long TEXT!' isn't right");
            Assert.That("lots of lower characters 123!".IsAllLowerFast(), "IsAllLowerFast isn't right");
            Assert.That(!"a few UPPER characters 123!".IsAllLowerFast(), "!IsAllLowerFast isn't right");
            Assert.AreEqual("WOW", "wow".ToUpperFast(), "ToUpperFast 'wow' isn't right");
            Assert.AreEqual("BIG LONG TEXT!", "big long text!".ToUpperFast(), "ToUpperFast 'big long text!' isn't right");
            Assert.That("lots of lower characters 123!".IsAllLowerFast(), "IsAllLowerFast isn't right");
            Assert.That(!"a few UPPER characters 123!".IsAllLowerFast(), "!IsAllLowerFast isn't right");
            Assert.That("LOTS OF UPPER CHARACTERS 123!".IsAllUpperFast(), "IsAllUpperFast isn't right");
            Assert.That(!"A FEW lower CHARACTERS 123!".IsAllUpperFast(), "!IsAllUpperFast isn't right");
        }

        /// <summary>
        /// Tests "Before" and "BeforeLast".
        /// </summary>
        [Test]
        public static void BeforeTests()
        {
            Assert.AreEqual("One", "OneTwoThree".Before("Two"), "Before 'OneTwoThree' isn't right");
            Assert.AreEqual("One", "OneTwoThreeTwo".Before("Two"), "Before 'OneTwoThreeTwo' isn't right");
            Assert.AreEqual("One", "OneTwoThree".BeforeLast("Two"), "BeforeLast 'OneTwoThree' isn't right");
            Assert.AreEqual("OneTwoThree", "OneTwoThreeTwo".BeforeLast("Two"), "BeforeLast 'OneTwoThreeTwo' isn't right");
        }

        /// <summary>
        /// Tests "After" and "AfterLast".
        /// </summary>
        [Test]
        public static void AfterTests()
        {
            Assert.AreEqual("Three", "OneTwoThree".After("Two"), "After 'OneTwoThree' isn't right");
            Assert.AreEqual("ThreeTwo", "OneTwoThreeTwo".After("Two"), "After 'OneTwoThreeTwo' isn't right");
            Assert.AreEqual("Three", "OneTwoThree".AfterLast("Two"), "AfterLast 'OneTwoThree' isn't right");
            Assert.AreEqual("Four", "OneTwoThreeTwoFour".AfterLast("Two"), "AfterLast 'OneTwoThreeTwoFour' isn't right");
        }

        /// <summary>
        /// Tests "BeforeAndAfter" and "BeforeAndAfterLast".
        /// </summary>
        [Test]
        public static void BeforeAndAfterTests()
        {
            Assert.That("OneTwoThree".BeforeAndAfter("Two", out string out1) == "One" && out1 == "Three", "BeforeAndAfter 'OneTwoThree' isn't right");
            Assert.That("OneTwoThreeTwo".BeforeAndAfter("Two", out string out2) == "One" && out2 == "ThreeTwo", "BeforeAndAfter 'OneTwoThreeTwo' isn't right");
            Assert.That("OneTwoThree".BeforeAndAfterLast("Two", out string out3) == "One" && out3 == "Three", "BeforeAndAfterLast 'OneTwoThree' isn't right");
            Assert.That("OneTwoThreeTwoFour".BeforeAndAfterLast("Two", out string out4) == "OneTwoThree" && out4 == "Four", "BeforeAndAfterLast 'OneTwoThreeTwoFour' isn't right");
        }

        /// <summary>
        /// Tests "IndexEquals", "StartsWillNull", "StartsWithFast", "EndsWithNull", and "EndsWithFast".
        /// </summary>
        [Test]
        public static void IndexBasedTests()
        {
            Assert.That("ABC123".IndexEquals(3, '1'), "IndexEquals broke");
            Assert.That(!("ABC123".IndexEquals(3, 'C')), "!IndexEquals broke");
            Assert.That("\0ABC123".StartsWithNull(), "StartsWithNull broke");
            Assert.That(!"ABC123".StartsWithNull(), "!StartsWithNull broke");
            Assert.That("ABC123\0".EndsWithNull(), "EndsWithNull broke");
            Assert.That(!"ABC123".EndsWithNull(), "!EndsWithNull broke");
            Assert.That("ABC123".StartsWithFast('A'), "StartsWithFast broke");
            Assert.That(!"ABC123".StartsWithFast('1'), "!StartsWithFast broke");
            Assert.That("ABC123".EndsWithFast('3'), "EndsWithFast broke");
            Assert.That(!"ABC123".EndsWithFast('C'), "!EndsWithFast broke");
        }

        /// <summary>
        /// Tests "CountCharacter".
        /// </summary>
        [Test]
        public static void CountTest()
        {
            Assert.AreEqual(4, "CA BC132C 23C".CountCharacter('C'), "CountCharacter broke");
        }

        /// <summary>
        /// Tests "SplitFast".
        /// </summary>
        [Test]
        public static void SplitTest()
        {
            string[] SplitTestOne = "A_Set_Of_Words".SplitFast('_');
            Assert.That(SplitTestOne.Length == 4 && SplitTestOne[0] == "A" && SplitTestOne[1] == "Set" && SplitTestOne[2] == "Of" && SplitTestOne[3] == "Words", $"SplitFast broke: {string.Join(" / ", SplitTestOne)}");
            string[] SplitTestTwo = "A_Set_Of_Words".SplitFast('_', 2);
            Assert.That(SplitTestTwo.Length == 3 && SplitTestTwo[0] == "A" && SplitTestTwo[1] == "Set" && SplitTestTwo[2] == "Of_Words", $"SplitFast(max) broke: {string.Join(" / ", SplitTestTwo)}");
        }
    }
}
