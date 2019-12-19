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
using FreneticUtilities.FreneticToolkit;

namespace FGETests.FreneticUtilitiesTests.FreneticToolkitTests
{
    /// <summary>
    /// Tests expectations of <see cref="AsciiMatcher"/>.
    /// </summary>
    public class AsciiMatcherTests : FGETest
    {
        /// <summary>
        /// Prepares the basics.
        /// </summary>
        [OneTimeSetUp]
        public static void PreInit()
        {
            Setup();
            ReferenceMatcher = new AsciiMatcher(ReferenceMatchChars);
        }
        
        /// <summary>
        /// Matched characters reference.
        /// </summary>
        public const string ReferenceMatchChars = "AaBCcDDDDDDDDDDDdeeF12215490lMO!?@#$%";

        /// <summary>
        /// Non-matched characters reference.
        /// </summary>
        public const string ReferenceNonMatchedChars = "bEf3mZz()*&\0";

        /// <summary>
        /// Reference matcher constructed from <see cref="ReferenceMatchChars"/>.
        /// </summary>
        public static AsciiMatcher ReferenceMatcher;

        /// <summary>
        /// Tests "IsMatch".
        /// </summary>
        [Test]
        public static void IsMatchTest()
        {
            foreach (char c in ReferenceMatchChars)
            {
                Assert.That(ReferenceMatcher.IsMatch(c), $"IsMatch failed for {c}");
            }
            foreach (char c in ReferenceNonMatchedChars)
            {
                Assert.That(!ReferenceMatcher.IsMatch(c), $"!IsMatch failed for {c}");
            }
            Assert.That(!ReferenceMatcher.IsMatch((char)1000), "!IsMatch failed for char 1000");
        }

        /// <summary>
        /// Tests "IsOnlyMatches".
        /// </summary>
        [Test]
        public static void IsOnlyMatchesTest()
        {
            Assert.That(ReferenceMatcher.IsOnlyMatches(ReferenceMatchChars), $"IsOnlyMatches failed.");
            Assert.That(!ReferenceMatcher.IsOnlyMatches(ReferenceNonMatchedChars), $"!IsOnlyMatches failed.");
            Assert.That(!ReferenceMatcher.IsOnlyMatches(ReferenceMatchChars + ReferenceNonMatchedChars), $"!IsOnlyMatches failed.");
        }

        /// <summary>
        /// Tests "TrimToMatches".
        /// </summary>
        [Test]
        public static void TrimToMatchesTest()
        {
            Assert.AreEqual(ReferenceMatchChars, ReferenceMatcher.TrimToMatches(ReferenceMatchChars), $"TrimToMatches pure-match failed.");
            Assert.AreEqual("", ReferenceMatcher.TrimToMatches(ReferenceNonMatchedChars), $"TrimToMatches no-match failed.");
            Assert.AreEqual(ReferenceMatchChars, ReferenceMatcher.TrimToMatches(ReferenceNonMatchedChars + ReferenceMatchChars + ReferenceNonMatchedChars), $"TrimToMatches mixed-match failed.");
        }

        /// <summary>
        /// Tests "TrimToNonMatches".
        /// </summary>
        [Test]
        public static void TrimToNonMatchesTest()
        {
            Assert.AreEqual("", ReferenceMatcher.TrimToNonMatches(ReferenceMatchChars), $"TrimToNonMatches pure-match failed.");
            Assert.AreEqual(ReferenceNonMatchedChars, ReferenceMatcher.TrimToNonMatches(ReferenceNonMatchedChars), $"TrimToNonMatches no-match failed.");
            Assert.AreEqual(ReferenceNonMatchedChars, ReferenceMatcher.TrimToNonMatches(ReferenceMatchChars + ReferenceNonMatchedChars + ReferenceMatchChars), $"TrimToNonMatches mixed-match failed.");
        }
    }
}
