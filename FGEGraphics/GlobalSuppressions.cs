//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
[assembly: SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible")]
[assembly: SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0")]
[assembly: SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Scope = "type", Target = "~T:Valve.VR.EVRButtonId")]
[assembly: SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
[assembly: SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
[assembly: SuppressMessage("Design", "CA1063:Implement IDisposable Correctly")]
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "TODO: System.Drawing invalid on Linux?")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "This cannot be applied so generally. Not every class owns what it references.")]
[assembly: SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "wtf")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "<Pending>", Scope = "namespace", Target = "~N:Valve.VR")]
