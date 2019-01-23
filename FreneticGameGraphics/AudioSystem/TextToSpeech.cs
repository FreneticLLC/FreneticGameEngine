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
using System.Diagnostics;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticGameCore.UtilitySystems;
#if WINDOWS
using System.Speech.Synthesis;
#endif

namespace FreneticGameGraphics.AudioSystem
{
    /// <summary>
    /// Text-to-speech helper.
    /// </summary>
    public class TextToSpeech
    {
        /// <summary>
        /// Whether to try the Windows speech engine.
        /// </summary>
        public static bool TrySpeech = true;

        /// <summary>
        /// Speaks aloud some text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="male">Whether to be male (if not, female).</param>
        /// <param name="rate">The rate at which to speak.</param>
        public static void Speak(string text, bool male, int rate)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
#if WINDOWS
                    if (TrySpeech)
                    {
                        SpeechSynthesizer speech = new SpeechSynthesizer();
                        VoiceInfo vi = null;
                        foreach (InstalledVoice v in speech.GetInstalledVoices())
                        {
                            if (!v.Enabled)
                            {
                                continue;
                            }
                            if (vi == null)
                            {
                                vi = v.VoiceInfo;
                            }
                            else if ((male && v.VoiceInfo.Gender == VoiceGender.Male) || (!male && v.VoiceInfo.Gender == VoiceGender.Female))
                            {
                                vi = v.VoiceInfo;
                                break;
                            }
                        }
                        if (vi == null)
                        {
                            TrySpeech = false;
                        }
                        else
                        {
                            speech.SelectVoice(vi.Name);
                            speech.Rate = rate;
                            speech.Speak(text);
                        }
                    }
#endif
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    TrySpeech = false;
                }
                if (!TrySpeech)
                {
                    // TODO: Rate!
                    String addme = male ? " -p 40" : " -p 95";
                    Process p = Process.Start("espeak", "\"" + text.Replace("\"", " quote ") + "\"" + addme);
                    Console.WriteLine(p.MainModule.FileName);
                }
            });
        }
    }
}
