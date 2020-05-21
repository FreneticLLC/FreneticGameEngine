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
using OpenTK;
using FreneticUtilities.FreneticExtensions;
using FGECore.UtilitySystems;
using FGEGraphics.GraphicsHelpers.Shaders;

namespace FGEGraphics.GraphicsHelpers.FontSets
{
    /// <summary>
    /// Handles pretty-rendered fonts.
    /// </summary>
    public class FontSetEngine
    {
        /// <summary>
        /// Constructs the font set engine without initializing it.
        /// </summary>
        /// <param name="fontEngine">The font engine.</param>
        public FontSetEngine(GLFontEngine fontEngine)
        {
            GLFonts = fontEngine;
        }

        /// <summary>
        /// Shader to revert to after rendering some text.
        /// </summary>
        public Shader FixToShader;

        /// <summary>
        /// Random helper object.
        /// </summary>
        public MTRandom RandomHelper = new MTRandom();

        /// <summary>
        /// The lower font system.
        /// </summary>
        public GLFontEngine GLFonts;
        
        /// <summary>
        /// The general font used for all normal purposes.
        /// </summary>
        public FontSet Standard;

        /// <summary>
        /// The general font for slightly bigger text rendering.
        /// </summary>
        public FontSet SlightlyBigger;

        /// <summary>
        /// A list of all currently loaded font sets.
        /// </summary>
        public List<FontSet> Fonts = new List<FontSet>();

        /// <summary>
        /// Helper function to get a language data.
        /// </summary>
        public Func<string[], string> GetLanguageHelper;

        /// <summary>
        /// Helper function to get the current orthographic matrix.
        /// </summary>
        public Func<Matrix4> GetOrtho;

        /// <summary>
        /// Helper function to get the current global tick time.
        /// </summary>
        public Func<double> GetGlobalTickTime;

        /// <summary>
        /// Prepares the FontSet system.
        /// </summary>
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
            Fonts.Add(Standard);
            SlightlyBigger = new FontSet("slightlybigger", this);
            SlightlyBigger.Load(GLFonts.Standard.Name, GLFonts.Standard.Size + 5);
            Fonts.Add(SlightlyBigger);
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
            for (int i = 0; i < Fonts.Count; i++)
            {
                if (Fonts[i].FontDefault.Size == fontsize && Fonts[i].Name == namelow)
                {
                    return Fonts[i];
                }
            }
            FontSet toret = new FontSet(fontname, this);
            toret.Load(fontname, fontsize);
            Fonts.Add(toret);
            return toret;
        }
    }
}
