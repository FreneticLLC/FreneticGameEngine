//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using NUnit.Framework;

namespace FGETests.FreneticUtilitiesTests.FreneticExtensionsTests
{
    /// <summary>Tests expectations of <see cref="EnumerableExtensions"/>.</summary>
    [TestFixture]
    public class EnumerableExtensionTests : FGETest
    {
        /// <summary>Prepares the basics.</summary>
        [OneTimeSetUp]
        public static void PreInit()
        {
            Setup();
        }

        /// <summary>Tests "SwapKeyValue".</summary>
        [Test]
        public static void SwapKeyValueTest()
        {
            Dictionary<string, int> testDict = new()
            {
                { "hello", 5 }, { "wow", 3 }, { "test", -5 }
            };
            Dictionary<int, string> resultDict = testDict.SwapKeyValue();
            Assert.AreEqual(3, resultDict.Count, "SwapKeyValueTest resultDict Count wrong");
            Assert.AreEqual("hello", resultDict[5], "SwapKeyValueTest resultDict[5] wrong");
            Assert.AreEqual("wow", resultDict[3], "SwapKeyValueTest resultDict[3] wrong");
            Assert.AreEqual("test", resultDict[-5], "SwapKeyValueTest resultDict[-5] wrong");
        }

        /// <summary>Tests "PairsToDictionary".</summary>
        [Test]
        public static void PairsToDictionary()
        {
            Dictionary<string, int> resultDict = new[] { ("a", 1), ("b", 2), ("c", 3) }.PairsToDictionary();
            Assert.AreEqual(3, resultDict.Count, "PairsToDictionary resultDict Count wrong");
            Assert.AreEqual(1, resultDict["a"], "PairsToDictionary resultDict['a'] wrong");
            Assert.AreEqual(2, resultDict["b"], "PairsToDictionary resultDict['b'] wrong");
            Assert.AreEqual(3, resultDict["c"], "PairsToDictionary resultDict['c'] wrong");
            Assert.Throws<ArgumentException>(() => new[] { ("a", 1), ("b", 2), ("c", 3), ("b", 4), ("d", 5) }.PairsToDictionary(), "PairsToDictionary allowed a dup");
        }

        /// <summary>Tests "ToDictionaryWithNoDup".</summary>
        [Test]
        public static void ToDictNoDupTest()
        {
            Dictionary<string, int> resultDict = new[] { "a", "b", "c" }.ToDictionaryWithNoDup(new[] { 1, 2, 3 });
            Assert.AreEqual(3, resultDict.Count, "ToDictNoDupTest resultDict Count wrong");
            Assert.AreEqual(1, resultDict["a"], "ToDictNoDupTest resultDict['a'] wrong");
            Assert.AreEqual(2, resultDict["b"], "ToDictNoDupTest resultDict['b'] wrong");
            Assert.AreEqual(3, resultDict["c"], "ToDictNoDupTest resultDict['c'] wrong");
            Assert.Throws<ArgumentException>(() => new[] { "a", "b", "c", "b", "d" }.ToDictionaryWithNoDup(new[] { 1, 2, 3, 4, 5 }), "ToDictNoDupTest allowed a dup");
        }

        /// <summary>Tests "ToDictionaryWith".</summary>
        [Test]
        public static void ToDictTest()
        {
            Dictionary<string, int> resultDict = new[] { "a", "b", "c", "b" }.ToDictionaryWith(new[] { 1, 2, 3, 4 });
            Assert.AreEqual(3, resultDict.Count, "ToDictTest resultDict Count wrong");
            Assert.AreEqual(1, resultDict["a"], "ToDictTest resultDict['a'] wrong");
            Assert.AreEqual(4, resultDict["b"], "ToDictTest resultDict['b'] wrong");
            Assert.AreEqual(3, resultDict["c"], "ToDictTest resultDict['c'] wrong");
        }

        /// <summary>Tests "AddAll".</summary>
        [Test]
        public static void AddAllTest()
        {
            Dictionary<string, int> resultDict = new() { { "a", 1 }, { "b", 2 } };
            resultDict.AddAll(new Dictionary<string, int>() { { "c", 3 }, { "d", 4 } });
            Assert.AreEqual(4, resultDict.Count, "AddAllTest resultDict Count wrong");
            Assert.AreEqual(1, resultDict["a"], "AddAllTest resultDict['a'] wrong");
            Assert.AreEqual(2, resultDict["b"], "AddAllTest resultDict['b'] wrong");
            Assert.AreEqual(3, resultDict["c"], "AddAllTest resultDict['c'] wrong");
            Assert.AreEqual(4, resultDict["d"], "AddAllTest resultDict['d'] wrong");
            Assert.Throws<ArgumentException>(() => resultDict.AddAll(new Dictionary<string, int>() { { "b", 3 } }), "AddAllTest allowed a dup");
        }

        /// <summary>Tests "UnionWith".</summary>
        [Test]
        public static void UnionWithTest()
        {
            Dictionary<string, int> resultDict = new() { { "a", 1 }, { "b", 2 } };
            resultDict.UnionWith(new Dictionary<string, int>() { { "c", 3 }, { "b", 4 } });
            Assert.AreEqual(3, resultDict.Count, "UnionWithTest resultDict Count wrong");
            Assert.AreEqual(1, resultDict["a"], "UnionWithTest resultDict['a'] wrong");
            Assert.AreEqual(4, resultDict["b"], "UnionWithTest resultDict['b'] wrong");
            Assert.AreEqual(3, resultDict["c"], "UnionWithTest resultDict['c'] wrong");
        }

        /// <summary>Tests "StopWhen".</summary>
        [Test]
        public static void StopWhenTest()
        {
            string[] strs = new[] { "alpha", "bravo", "charlie", "delta" };
            IEnumerable<string> stoppedEarly = strs.StopWhen(s => s.StartsWith("c"));
            Assert.AreEqual(2, stoppedEarly.Count(), "StopWhenTest stopped at wrong spot");
        }

        /// <summary>Tests "GetOrCreate".</summary>
        [Test]
        public static void GetOrCreateTest()
        {
            Dictionary<string, int> resultDict = new() { { "a", 1 }, { "b", 2} };
            int resultThree = resultDict.GetOrCreate("c", () => 3);
            Assert.AreEqual(3, resultDict.Count, "GetOrCreateTest resultDict Count wrong");
            Assert.AreEqual(3, resultThree, "GetOrCreateTest resultThree wrong");
            Assert.AreEqual(1, resultDict["a"], "GetOrCreateTest resultDict['a'] wrong");
            Assert.AreEqual(2, resultDict["b"], "GetOrCreateTest resultDict['b'] wrong");
            Assert.AreEqual(3, resultDict["c"], "GetOrCreateTest resultDict['c'] wrong");
            Assert.DoesNotThrow(() => resultDict.GetOrCreate("c", () => throw new ArgumentException("bork")), "GetOrCreateTest ran func at wrong time");
        }

        /// <summary>Tests "JoinWith".</summary>
        [Test]
        public static void JoinWithTest()
        {
            string[] alpha = new[] { "alpha", "bravo" };
            string[] bravo = new[] { "charlie", "delta" };
            string[] resultArray = alpha.JoinWith(bravo);
            Assert.AreEqual(4, resultArray.Length, "JoinWithTest resultArray length wrong");
            Assert.AreEqual("alpha", resultArray[0], "JoinWithTest resultArray[0] wrong");
            Assert.AreEqual("bravo", resultArray[1], "JoinWithTest resultArray[1] wrong");
            Assert.AreEqual("charlie", resultArray[2], "JoinWithTest resultArray[2] wrong");
            Assert.AreEqual("delta", resultArray[3], "JoinWithTest resultArray[3] wrong");
        }

        /// <summary>Tests "IsEmpty".</summary>
        [Test]
        public static void IsEmptyTest()
        {
            Assert.That(Array.Empty<string>().IsEmpty(), "IsEmptyTest wrong");
            Assert.That(!(new string[] { "1", "2" }.IsEmpty()), "!IsEmptyTest wrong");
        }
    }
}
