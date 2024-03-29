﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
[assembly: SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible")]
[assembly: SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
[assembly: SuppressMessage("Design", "CA1063:Implement IDisposable Correctly")]
[assembly: SuppressMessage("Design", "CA1060:Move pinvokes to native methods class")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Not owned code", Scope = "member", Target = "~M:FGECore.PhysicsSystem.BepuCharacters.CharacterControllers.AnalyzeContactsWorkerCache.#ctor(System.Int32,BepuUtilities.Memory.BufferPool)")]
