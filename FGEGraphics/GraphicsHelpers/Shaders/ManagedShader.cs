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

namespace FGEGraphics.GraphicsHelpers.Shaders;

/// <summary>Base abstract class for a managed shader: a shader that has a dedicated object to control it.
/// This mostly exists for compatibility for certain shaders, but may also serve as a nicer interface than raw GL calls.</summary>
public abstract class ManagedShader
{
    /// <summary>Construct the <see cref="ManagedShader"/> instance from the underlying shader data.</summary>
    public ManagedShader(Shader shader)
    {
        UnderlyingShader = shader;
        shader.ManagedForm = this;
    }

    /// <summary>The actual shader object that this <see cref="ManagedShader"/> represents.</summary>
    public Shader UnderlyingShader;

    /// <summary>Binds the shader to GL, all following GL calls will now use this shader until a different one is bound.</summary>
    public void Bind()
    {
        UnderlyingShader = UnderlyingShader.Bind();
    }

    /// <summary>If true, the shader is currently bound. If false, the shader is not currently bound.</summary>
    public bool IsBound => ShaderEngine.BoundNow?.Internal_Program == UnderlyingShader.Internal_Program;
}
