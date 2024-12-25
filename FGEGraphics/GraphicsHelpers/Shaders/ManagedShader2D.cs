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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static FGEGraphics.GraphicsHelpers.Shaders.ShaderLocations;

namespace FGEGraphics.GraphicsHelpers.Shaders;

/// <summary>Base <see cref="ManagedShader"/> for common 2D shaders (eg ColorMult2D). Does not apply to postprocessing shaders (eg combine2d).</summary>
public class ManagedShader2D(Shader shader) : ManagedShader(shader)
{
    // Note: We must read the locations dynamically because old Intel GPUs compile at least ColorMult2D with inconsistent locations, despite them being hardcoded explicitly.

    /// <summary>The uniform of the Scaler value, a value that scales (multiplies) all rendered object locations. The Z value corresponds to <see cref="RenderContext2D.AspectHelper"/>.</summary>
    public ShaderUniformVec3 Scaler = new(GL.GetUniformLocation(shader.Internal_Program, "scaler")); // 1

    /// <summary>The uniform of the Adder value, a value that offsets (adds to) all rendered object locations.</summary>
    public ShaderUniformVec2 Adder = new(GL.GetUniformLocation(shader.Internal_Program, "adder")); // 2

    /// <summary>The uniform of the Color value, a <see cref="Color4F"/> (R,G,B,A) color to multiply the rendered object's colors with.</summary>
    public ShaderUniformVec4 Color = new(GL.GetUniformLocation(shader.Internal_Program, "v_color")); // 3

    /// <summary>The uniform of the Rotation value, a 3D vector of (Center X, Center Y, Rotation Angle Radians).</summary>
    public ShaderUniformVec3 Rotation = new(GL.GetUniformLocation(shader.Internal_Program, "rotation")); // 4

    /// <summary>Gets the <see cref="Scaler"/> for whatever is currently bound.</summary>
    public static ShaderUniformVec3 CurrentScaler => ShaderEngine.BoundNow.ManagedForm is ManagedShader2D managed ? managed.Scaler : Common2D.SCALER;

    /// <summary>Gets the <see cref="Adder"/> on whatever is currently bound.</summary>
    public static ShaderUniformVec2 CurrentAdder => ShaderEngine.BoundNow.ManagedForm is ManagedShader2D managed ? managed.Adder : Common2D.ADDER;

    /// <summary>Gets the <see cref="Color"/> on whatever is currently bound.</summary>
    public static ShaderUniformVec4 CurrentColor => ShaderEngine.BoundNow.ManagedForm is ManagedShader2D managed ? managed.Color : Common2D.COLOR;

    /// <summary>Gets the <see cref="Rotation"/> on whatever is currently bound.</summary>
    public static ShaderUniformVec3 CurrentRotation => ShaderEngine.BoundNow.ManagedForm is ManagedShader2D managed ? managed.Rotation : Common2D.ROTATION;
}
