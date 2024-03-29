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
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace FGETests;

/// <summary>Tests expectations of the FGE Core program class.</summary>
[TestFixture]
public class FGECoreProgramTests : FGETest
{
    /// <summary>Prepares the basics.</summary>
    [OneTimeSetUp]
    public static void PreInit()
    {
        Setup();
    }

    /// <summary>Confirm that the program provides the right string values.</summary>
    [Test]
    public static void TestIDs()
    {
        Assert.That(Program.GameName == FGETestProgram.NAME, "Game name bounces wrong!");
        Assert.That(Program.GameVersion == FGETestProgram.VERSION, "Game version bounces wrong!");
        Assert.That(Program.GameVersionDescription == FGETestProgram.VERSDESC, "Game version-description bounces wrong!");
    }
}
