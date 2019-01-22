//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticGameCore.Files;

namespace FreneticGameCore
{
    /// <summary>
    /// Handles internationalization of text (translation to other languages).
    /// <para>Text IDs are specified as arrays in the form: FileID, Key, Variables</para>
    /// <para>Key is an FDS key using dots '.' to represent a subkey separator.</para>
    /// </summary>
    public class LanguageEngine
    {
        /// <summary>
        /// Used for <see cref="DefaultLanguage"/> and <see cref="CurrentLanguage"/>.
        /// </summary>
        public const string AUTO_DEFAULT = "en_us";

        /// <summary>
        /// The default language.
        /// If unset, will be 'en_us' (English).
        /// <para>This is chosen as the default as most developers speak English, and it is commonly considered a default global language.</para>
        /// <para>It is not required to be kept at English, though any developer using FGE probably understands English (based off the fact that all main docs and code names are English).</para>
        /// </summary>
        public string DefaultLanguage = AUTO_DEFAULT;

        /// <summary>
        /// The default documents in English (or, in whatever language <see cref="DefaultLanguage"/> is set to).
        /// Considered the root-most document, should be the best-written document samples most likely to be updated by developers.
        /// </summary>
        public Dictionary<string, FDSSection> EnglishDocuments = new Dictionary<string, FDSSection>();

        /// <summary>
        /// All documents in the currently set language.
        /// </summary>
        public Dictionary<string, FDSSection> LanguageDocuments = new Dictionary<string, FDSSection>();

        /// <summary>
        /// The currently set language.
        /// </summary>
        public string CurrentLanguage = AUTO_DEFAULT;

        /// <summary>
        /// Sets the language to use. If the language specified is unavailable, text will fall back to <see cref="DefaultLanguage"/>.
        /// Should be in Language ID code, eg "en_us", but must match file names more than anything.
        /// </summary>
        /// <param name="language">The language to use.</param>
        public void SetLanguage(string language)
        {
            CurrentLanguage = language.ToLowerFast();
            LanguageDocuments.Clear();
        }

        /// <summary>
        /// Gets a language document for the specified parameters.
        /// </summary>
        /// <param name="id">The document ID.</param>
        /// <param name="Files">The file system.</param>
        /// <param name="lang">The language to enforce for this read, if any.</param>
        /// <param name="confs">The custom configuration set to use, if any.</param>
        /// <returns></returns>
        public FDSSection GetLangDoc(string id, FileHandler Files, string lang = null, Dictionary<string, FDSSection> confs = null)
        {
            if (lang == null)
            {
                lang = CurrentLanguage;
            }
            if (confs == null)
            {
                confs = LanguageDocuments;
            }
            string idlow = id.ToLowerFast();
            if (LanguageDocuments.TryGetValue(idlow, out FDSSection doc))
            {
                return doc;
            }
            string path = "info/text/" + idlow + "_" + lang + ".fds";
            if (Files.Exists(path))
            {
                try
                {
                    string dat = Files.ReadText(path);
                    doc = new FDSSection(dat);
                    LanguageDocuments[idlow] = doc;
                    return doc;
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    SysConsole.Output("Reading language documents", ex);
                }
            }
            LanguageDocuments[idlow] = null;
            return null;
        }

        /// <summary>
        /// The key that represents a missing key.
        /// </summary>
        public const string BADKEY = "common.languages.badkey";

        /// <summary>
        /// Helper to handle vars.
        /// Translates variables of form "{{1}}" to the var set by the requester.
        /// </summary>
        /// <param name="info">The text item.</param>
        /// <param name="pathAndVars">The path and its vars.</param>
        /// <returns>The var-cleaned string.</returns>
        public string Handle(string info, string[] pathAndVars)
        {
            info = info.Replace('\r', '\n').Replace("\n", "");
            for (int i = 2; i < pathAndVars.Length; i++)
            {
                info = info.Replace("{{" + (i - 1).ToString() + "}}", pathAndVars[i]);
            }
            return info;
        }

        /// <summary>
        /// Handles a list of texts against a var, similar to <see cref="Handle(string, string[])"/>.
        /// </summary>
        /// <param name="infolist">The list of infos.</param>
        /// <param name="pathAndVars">The path and its vars.</param>
        /// <returns>Handled lists.</returns>
        public List<string> HandleList(List<string> infolist, string[] pathAndVars)
        {
            for (int i = 0; i < infolist.Count; i++)
            {
                infolist[i] = Handle(infolist[i], pathAndVars);
            }
            return infolist;
        }

        /// <summary>
        /// Gets a list of texts, in the default language.
        /// </summary>
        /// <param name="Files">The file system.</param>
        /// <param name="pathAndVars">The path and its vars.</param>
        /// <returns>The text list.</returns>
        public List<string> GetTextListDefault(FileHandler Files, params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetTextListDefault(Files, "core", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            FDSSection langen = GetLangDoc(category, Files, DefaultLanguage, EnglishDocuments);
            List<string> str = null;
            if (langen != null)
            {
                str = langen.GetStringList(defPath);
                if (str != null)
                {
                    return HandleList(str, pathAndVars);
                }
            }
            if (defPath == BADKEY)
            {
                return new List<string>() { "((Invalid key!))" };
            }
            return GetTextListDefault(Files, "core", BADKEY);
        }

        /// <summary>
        /// Gets a text, in the default language.
        /// </summary>
        /// <param name="Files">The file system.</param>
        /// <param name="pathAndVars">The path and its vars.</param>
        /// <returns>The text.</returns>
        public string GetTextDefault(FileHandler Files, params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetTextDefault(Files, "core", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            FDSSection langen = GetLangDoc(category, Files, DefaultLanguage, EnglishDocuments);
            string str = null;
            if (langen != null)
            {
                str = langen.GetString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (defPath == BADKEY)
            {
                return "((Invalid key!))";
            }
            return GetTextDefault(Files, "core", BADKEY);
        }

        /// <summary>
        /// Gets a list of texts.
        /// </summary>
        /// <param name="Files">The file system.</param>
        /// <param name="pathAndVars">The path and its vars.</param>
        /// <returns>The text list.</returns>
        public List<string> GetTextList(FileHandler Files, params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetTextList(Files, "core", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            FDSSection lang = GetLangDoc(category, Files);
            FDSSection langen = GetLangDoc(category, Files, DefaultLanguage, EnglishDocuments);
            List<string> str = null;
            if (lang != null)
            {
                str = lang.GetStringList(defPath);
                if (str != null)
                {
                    return HandleList(str, pathAndVars);
                }
            }
            if (langen != null)
            {
                str = langen.GetStringList(defPath);
                if (str != null)
                {
                    return HandleList(str, pathAndVars);
                }
            }
            if (defPath == BADKEY)
            {
                return new List<string>() { "((Invalid key!))" };
            }
            return GetTextList(Files, "core", BADKEY);
        }

        /// <summary>
        /// Gets a text.
        /// </summary>
        /// <param name="Files">The file system.</param>
        /// <param name="pathAndVars">The path and its vars.</param>
        /// <returns>The text.</returns>
        public string GetText(FileHandler Files, params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetText(Files, "core", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            FDSSection lang = GetLangDoc(category, Files);
            FDSSection langen = GetLangDoc(category, Files, DefaultLanguage, EnglishDocuments);
            string str = null;
            if (lang != null)
            {
                str = lang.GetString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (langen != null)
            {
                str = langen.GetString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (defPath == BADKEY)
            {
                return "((Invalid key!))";
            }
            return GetText(Files, "core", BADKEY);
        }
    }
}
