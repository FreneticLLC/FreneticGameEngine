using FGEGraphics.ClientSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

/// <summary>
/// Indicates that a property or field should be displayed in debug mode (when <see cref="ViewUI2D.IsDebug"/> is <c>true</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UIDebugAttribute : Attribute
{
}
