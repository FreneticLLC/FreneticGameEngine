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
using NUnit.Framework.Legacy;

namespace FGETests.FreneticUtilitiesTests.FreneticExtensionsTests;

/// <summary>Tests expectations of <see cref="StringExtensions"/>.</summary>
[TestFixture]
public class StringExtensionTests : FGETest
{
    /// <summary>Prepares the basics.</summary>
    [OneTimeSetUp]
    public static void PreInit()
    {
        Setup();
    }

    /// <summary>Tests "ToLowerFast", "IsAllLowerFast", "ToUpperFast", "IsAllUpperFast", and "EqualsIgnoreCaseFast".</summary>
    [Test]
    public static void CapitalizationTests()
    {
        ClassicAssert.AreEqual("wow", "WOW".ToLowerFast(), "ToLowerFast 'WOW' isn't right");
        ClassicAssert.AreEqual("big long text!", "BIG long TEXT!".ToLowerFast(), "ToLowerFast 'BIG long TEXT!' isn't right");
        Assert.That("lots of lower characters 123!".IsAllLowerFast(), "IsAllLowerFast isn't right");
        Assert.That(!"a few UPPER characters 123!".IsAllLowerFast(), "!IsAllLowerFast isn't right");
        ClassicAssert.AreEqual("WOW", "wow".ToUpperFast(), "ToUpperFast 'wow' isn't right");
        ClassicAssert.AreEqual("BIG LONG TEXT!", "big long text!".ToUpperFast(), "ToUpperFast 'big long text!' isn't right");
        Assert.That("lots of lower characters 123!".IsAllLowerFast(), "IsAllLowerFast isn't right");
        Assert.That(!"a few UPPER characters 123!".IsAllLowerFast(), "!IsAllLowerFast isn't right");
        Assert.That("LOTS OF UPPER CHARACTERS 123!".IsAllUpperFast(), "IsAllUpperFast isn't right");
        Assert.That(!"A FEW lower CHARACTERS 123!".IsAllUpperFast(), "!IsAllUpperFast isn't right");
        Assert.That("wow".EqualsIgnoreCaseFast("wow"), "EqualsIgnoreCaseFast isn't right");
        Assert.That("wow".EqualsIgnoreCaseFast("WOW"), "EqualsIgnoreCaseFast isn't right");
        Assert.That("wow123hello".EqualsIgnoreCaseFast("WOW123hello"), "EqualsIgnoreCaseFast isn't right");
        Assert.That(!"wow123hello".EqualsIgnoreCaseFast("lol123nopeo"), "!EqualsIgnoreCaseFast isn't right");
        Assert.That(!"wow123hello".EqualsIgnoreCaseFast("WOW123hellothere"), "!EqualsIgnoreCaseFast isn't right");
    }

    /// <summary>Tests "Before" and "BeforeLast" string versions.</summary>
    [Test]
    public static void BeforeStringTests()
    {
        ClassicAssert.AreEqual("One", "OneTwoThree".Before("Two"), "Before 'OneTwoThree' isn't right");
        ClassicAssert.AreEqual("One", "OneTwoThreeTwo".Before("Two"), "Before 'OneTwoThreeTwo' isn't right");
        ClassicAssert.AreEqual("One", "OneTwoThree".BeforeLast("Two"), "BeforeLast 'OneTwoThree' isn't right");
        ClassicAssert.AreEqual("OneTwoThree", "OneTwoThreeTwo".BeforeLast("Two"), "BeforeLast 'OneTwoThreeTwo' isn't right");
    }

    /// <summary>Tests "Before" and "BeforeLast" character versions.</summary>
    [Test]
    public static void BeforeCharTests()
    {
        ClassicAssert.AreEqual("One", "One2Three".Before('2'), "Before char 'One2Three' isn't right");
        ClassicAssert.AreEqual("One", "One2Three2".Before('2'), "Before char 'One2ThreeTwo' isn't right");
        ClassicAssert.AreEqual("One", "One2Three".BeforeLast('2'), "BeforeLast char 'One2Three' isn't right");
        ClassicAssert.AreEqual("One2Three", "One2Three2".BeforeLast('2'), "BeforeLast char 'OneTwoThree2' isn't right");
    }

    /// <summary>Tests "After" and "AfterLast" string versions.</summary>
    [Test]
    public static void AfterStringTests()
    {
        ClassicAssert.AreEqual("Three", "OneTwoThree".After("Two"), "After 'OneTwoThree' isn't right");
        ClassicAssert.AreEqual("ThreeTwo", "OneTwoThreeTwo".After("Two"), "After 'OneTwoThreeTwo' isn't right");
        ClassicAssert.AreEqual("Three", "OneTwoThree".AfterLast("Two"), "AfterLast 'OneTwoThree' isn't right");
        ClassicAssert.AreEqual("Four", "OneTwoThreeTwoFour".AfterLast("Two"), "AfterLast 'OneTwoThreeTwoFour' isn't right");
    }

    /// <summary>Tests "After" and "AfterLast" character versions.</summary>
    [Test]
    public static void AfterCharTests()
    {
        ClassicAssert.AreEqual("Three", "One2Three".After('2'), "After char 'One2Three' isn't right");
        ClassicAssert.AreEqual("Three2", "One2Three2".After('2'), "After char 'One2Three2' isn't right");
        ClassicAssert.AreEqual("Three", "One2Three".AfterLast('2'), "AfterLast char 'One2Three' isn't right");
        ClassicAssert.AreEqual("Four", "One2Three2Four".AfterLast('2'), "AfterLast char 'One2Three2Four' isn't right");
    }

    /// <summary>Tests "BeforeAndAfter" and "BeforeAndAfterLast" string versions.</summary>
    [Test]
    public static void BeforeAndAfterStringTests()
    {
        Assert.That("OneTwoThree".BeforeAndAfter("Two", out string out1) == "One" && out1 == "Three", "BeforeAndAfter 'OneTwoThree' isn't right");
        Assert.That("OneTwoThreeTwo".BeforeAndAfter("Two", out string out2) == "One" && out2 == "ThreeTwo", "BeforeAndAfter 'OneTwoThreeTwo' isn't right");
        Assert.That("OneTwoThree".BeforeAndAfterLast("Two", out string out3) == "One" && out3 == "Three", "BeforeAndAfterLast 'OneTwoThree' isn't right");
        Assert.That("OneTwoThreeTwoFour".BeforeAndAfterLast("Two", out string out4) == "OneTwoThree" && out4 == "Four", "BeforeAndAfterLast 'OneTwoThreeTwoFour' isn't right");
        Assert.That("OneTwoThree".BeforeAndAfter("Two") == ("One", "Three"), "BeforeAndAfter pair 'OneTwoThree' isn't right");
        Assert.That("OneTwoThreeTwo".BeforeAndAfter("Two") == ("One", "ThreeTwo"), "BeforeAndAfter pair 'OneTwoThreeTwo' isn't right");
        Assert.That("OneTwoThree".BeforeAndAfterLast("Two") == ("One", "Three"), "BeforeAndAfterLast pair 'OneTwoThree' isn't right");
        Assert.That("OneTwoThreeTwoFour".BeforeAndAfterLast("Two") == ("OneTwoThree", "Four"), "BeforeAndAfterLast pair 'OneTwoThreeTwoFour' isn't right");
    }

    /// <summary>Tests "BeforeAndAfter" and "BeforeAndAfterLast" character versions.</summary>
    [Test]
    public static void BeforeAndAfterCharTests()
    {
        Assert.That("One2Three".BeforeAndAfter('2', out string out1) == "One" && out1 == "Three", "BeforeAndAfter 'One2Three' isn't right");
        Assert.That("One2Three2".BeforeAndAfter('2', out string out2) == "One" && out2 == "Three2", "BeforeAndAfter 'One2Three2' isn't right");
        Assert.That("One2Three".BeforeAndAfterLast('2', out string out3) == "One" && out3 == "Three", "BeforeAndAfterLast 'One2Three' isn't right");
        Assert.That("One2Three2Four".BeforeAndAfterLast('2', out string out4) == "One2Three" && out4 == "Four", "BeforeAndAfterLast 'One2Three2Four' isn't right");
        Assert.That("One2Three".BeforeAndAfter('2') == ("One", "Three"), "BeforeAndAfter pair 'One2Three' isn't right");
        Assert.That("One2Three2".BeforeAndAfter('2') == ("One", "Three2"), "BeforeAndAfter pair 'One2Three2' isn't right");
        Assert.That("One2Three".BeforeAndAfterLast('2') == ("One", "Three"), "BeforeAndAfterLast pair 'One2Three' isn't right");
        Assert.That("One2Three2Four".BeforeAndAfterLast('2') == ("One2Three", "Four"), "BeforeAndAfterLast pair 'One2Three2Four' isn't right");
    }

    /// <summary>Tests "IndexEquals", "StartsWillNull", "StartsWithFast", "EndsWithNull", and "EndsWithFast".</summary>
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

    /// <summary>Tests "CountCharacter".</summary>
    [Test]
    public static void CountTest()
    {
        ClassicAssert.AreEqual(4, "CA BC132C 23C".CountCharacter('C'), "CountCharacter broke");
    }

    /// <summary>Tests "SplitFast".</summary>
    [Test]
    public static void SplitTest()
    {
        string[] SplitTestOne = "A_Set_Of_Words".SplitFast('_');
        Assert.That(SplitTestOne.Length == 4 && SplitTestOne[0] == "A" && SplitTestOne[1] == "Set" && SplitTestOne[2] == "Of" && SplitTestOne[3] == "Words", $"SplitFast broke: {string.Join(" / ", SplitTestOne)}");
        string[] SplitTestTwo = "A_Set_Of_Words".SplitFast('_', 2);
        Assert.That(SplitTestTwo.Length == 3 && SplitTestTwo[0] == "A" && SplitTestTwo[1] == "Set" && SplitTestTwo[2] == "Of_Words", $"SplitFast(max) broke: {string.Join(" / ", SplitTestTwo)}");
    }
}
