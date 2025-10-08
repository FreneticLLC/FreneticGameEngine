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
using FreneticUtilities.FreneticExtensions;
using FGECore.MathHelpers;
using FGECore.UtilitySystems;
using FGEGraphics.GraphicsHelpers.Shaders;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>Handles pretty-rendered fonts.</summary>
/// <param name="fontEngine">The font engine.</param>
public class FontSetEngine(GLFontEngine fontEngine)
{
    /// <summary>Shader to revert to after rendering some text.</summary>
    public Shader FixToShader;

    /// <summary>Random helper object.</summary>
    public MTRandom RandomHelper = new();

    /// <summary>The lower-level font system.</summary>
    public GLFontEngine GLFonts = fontEngine;

    /// <summary>The general font used for all normal purposes.</summary>
    public FontSet Standard;

    /// <summary>The general font for slightly bigger text rendering.</summary>
    public FontSet SlightlyBigger;

    /// <summary>The general font for double sized text rendering.</summary>
    public FontSet DoubleSize;

    /// <summary>A list of all currently loaded font sets.</summary>
    public Dictionary<(string, int), FontSet> Fonts = [];

    /// <summary>Helper function to get a language data.</summary>
    public Func<string[], string> GetLanguageHelper;

    /// <summary>Helper function to get the current orthographic matrix.</summary>
    public Func<Matrix4> GetOrtho;

    /// <summary>Helper function to get the current global tick time.</summary>
    public Func<double> GetGlobalTickTime;

    /// <summary>
    /// Advanced text format functions, for use with the '^[' format code.
    /// Map of label to (input, font, currentPart, addedPart) function.
    /// For example, "^[color=#ff00ff]" style input will read key "color" and will run currentPart.TextColor = specifiedColor.Value; addedPart.Text = "";
    /// </summary>
    public Dictionary<string, Action<string, FontSet, RenderableTextPart, RenderableTextPart>> TextAdvancedFormatters = [];

    /// <summary>Prepares the FontSet system.</summary>
    /// <param name="getlanghelp">The helper function to get a language data.</param>
    /// <param name="orthobase">The helper function to get the current orthographic matrix.</param>
    /// <param name="ticktime">The helper function to get the current global tick time.</param>
    public void Init(Func<string[], string> getlanghelp, Func<Matrix4> orthobase, Func<double> ticktime)
    {
        GetLanguageHelper = getlanghelp;
        GetOrtho = orthobase;
        GetGlobalTickTime = ticktime;
        Standard = new FontSet("standard", this);
        Standard.Load(GLFonts.Standard.Name, GLFonts.Standard.Size);
        Fonts.Add((Standard.Name, Standard.FontDefault.Size), Standard);
        SlightlyBigger = new FontSet("slightlybigger", this);
        SlightlyBigger.Load(GLFonts.Standard.Name, GLFonts.Standard.Size + 5);
        Fonts.Add((SlightlyBigger.Name, SlightlyBigger.FontDefault.Size), SlightlyBigger);
        DoubleSize = new FontSet("doublesize", this);
        DoubleSize.Load(GLFonts.Standard.Name, GLFonts.Standard.Size * 2);
        Fonts.Add((DoubleSize.Name, DoubleSize.FontDefault.Size), DoubleSize);
        TextAdvancedFormatters["color"] = (input, font, currentPart, addedPart) =>
        {
            Color4F? specifiedColor = Color4F.FromString(input);
            if (specifiedColor.HasValue)
            {
                currentPart.TextColor = specifiedColor.Value;
                addedPart.Text = "";
            }
            else
            {
                addedPart.Text = $"^[color={input}";
                addedPart.Highlight = true;
                addedPart.HighlightColor = Color4F.Red;
            }
        };
        TextAdvancedFormatters["url"] = (input, font, currentPart, addedPart) =>
        {
            addedPart.ClickURL = input.BeforeAndAfter('|', out addedPart.Text);
        };
        TextAdvancedFormatters["hover"] = (input, font, currentPart, addedPart) =>
        {
            // TODO: Better newline method than this?
            addedPart.HoverText = font.ParseFancyText(input.Replace("\\n", "\n").BeforeAndAfter('|', out addedPart.Text), "^r^)");
        };
    }

    /// <summary>
    /// Gets a font by a specified name.
    /// If the relevant FontSet exists but is not yet loaded, will load it from file.
    /// </summary>
    /// <param name="fontname">The name of the font.</param>
    /// <param name="fontsize">The size of the font.</param>
    /// <returns>The specified font.</returns>
    public FontSet GetFont(string fontname, int fontsize)
    {
        string namelow = fontname.ToLowerFast();
        if (Fonts.TryGetValue((namelow, fontsize), out FontSet toret))
        {
            return toret;
        }
        toret = new FontSet(fontname, this);
        toret.Load(fontname, fontsize);
        Fonts.Add((toret.Name, fontsize), toret);
        return toret;
    }
}
