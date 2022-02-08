using CommunicationModule.Encrypt;
using RWCustom;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CommunicationModule.Patch
{
    /// <summary>
    /// Language : Custom(Ctm), id = 1
    /// Language 6th -> English
    /// else -> use Patch
    /// </summary>
    public static class InGameTranslatorPatch
    {
        public static bool patchEnabled
        {
            get
            {
                return pe && CommunicationModule.ComMod.DataEnabled;
            }
            set { pe = value; }
        }

        private static bool pe = true;

        public static void Patch()
        {
            On.InGameTranslator.Translate += new On.InGameTranslator.hook_Translate(TranslatePatch);
            On.InGameTranslator.LoadTable += new On.InGameTranslator.hook_LoadTable(LoadTablePatch);
        }

#pragma warning disable CA1819 // Properties should not return arrays

        public static string[] regionName => CommunicationModule.ComMod.regionName;

        public static string TranslateRegion(string region)
        {
            if (!patchEnabled) { return region; }
            if (region.Length < 4) { goto CRS; }

            for (int i = 0; i < ComMod.regionNameEng.Length; i++)
            {
                if (ComMod.regionNameEng[i].Equals(region, StringComparison.OrdinalIgnoreCase))
                { return regionName[i]; }
            }

        //RegionName Snapping
        /*
        switch (region.Substring(0, 4).ToUpper())
        {
            case "OUTS":
                return regionName[0];

            case "DRAI":
                return regionName[1];

            case "INDU":
                return regionName[2];

            case "GARB":
                return regionName[3];

            case "SHOR":
                return regionName[4];

            case "LOOK":
                if (region.ToUpper().EndsWith("MOON")) { return regionName[5]; }
                goto CRS;

            case "SHAD":
                return regionName[6];

            case "MEMO":
                return regionName[7];

            case "THE ":
                if (region.Length < 7) { goto CRS; }
                switch (region.Substring(4, 3).ToUpper())
                {
                    default:
                    case "EXT":
                        return regionName[8];

                    case "LEG":
                        return regionName[9];

                    case "WAL":
                        return regionName[10];
                }
            case "UNDE":
                return regionName[11];

            case "FIVE":
                switch (region.Substring(region.Length - 3).ToUpper())
                {
                    default: goto CRS;

                    case "LES":
                        return regionName[12];

                    case "UX)":
                        return regionName[13];

                    case "NT)":
                        return regionName[14];

                    case "AY)":
                        return regionName[15];

                    case "US)":
                        return regionName[16];
                }
            case "CHIM":
                return regionName[17];

            case "SKY ":
                if (region.ToUpper().EndsWith("NDS")) { return regionName[18]; }
                goto CRS;

            case "COMM":
                return regionName[19];

            case "FARM":
                return regionName[20];

            case "SUBT":
                return regionName[21];

            case "FILT":
                return regionName[22];

            case "DEPT":
                return regionName[23];

            default:
                goto CRS;
        }
        */

        CRS:
            UnityEngine.Debug.Log("NO REGION TRANSLATION FOUND: " + region + "(len:" + region.Length + ")");
            return region;
        }

        public static InGameTranslator.LanguageID currentLanguage(InGameTranslator instance)
        {
            pe = true;
            if (!patchEnabled || (InGameTranslator.LanguageID)instance.rainWorld.options.language == InGameTranslator.LanguageID.Portuguese)
            {
                pe = false;
                return InGameTranslator.LanguageID.English;
            }
            return (InGameTranslator.LanguageID)1;
        }

        public static string SpecificTextFolderDirectory()
        {
            if (pe)
            {
                return string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Text_Ctm");
            }
            else
            {
                return string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Text_Eng");
            }
        }

        public static string ShortStringsDirectory(int l)
        {
            if (pe)
            {
                return string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar, l, ".txt");
            }
            else
            {
                return string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar, l, ".txt");
            }
        }

        public static string TranslatePatch(On.InGameTranslator.orig_Translate orig, InGameTranslator instance, string s)
        {
            if (currentLanguage(instance) == InGameTranslator.LanguageID.English) { return s; }
            if (!patchEnabled) { return orig.Invoke(instance, s); }

            //return CommunicationModule.ComMod.playerName[(int)(Mathf.Floor(UnityEngine.Random.Range(0, 6)))];

            if (s.Length == 10) { if (s.Substring(0, 4).ToUpper() == "PORT") { return "USE ENGLISH"; } }

            if (!instance.tables.ContainsKey(s.Length))
            {
                instance.LoadTable(s.Length);
                if (!instance.tables.ContainsKey(s.Length))
                {
                    UnityEngine.Debug.Log($"NO TABLE (len: {s.Length}):\n{s}");
                    //CommunicationModule.ComMod.dataEnabled = false;
                    return s; //(instance.rainWorld.buildType != RainWorld.BuildType.Distribution) ? "NO TABLE" : s;
                }
            }

            for (int i = 0; i < instance.tables[s.Length].GetLength(0); i++)
            {
                if (s == instance.tables[s.Length][i, 0]) //s equals original
                {
                    //Debug.Log(string.Concat(s + " ==> " + this.tables[s.Length][i, 1]));
                    return instance.tables[s.Length][i, 1]; //return translated text
                }
            }
            if (instance.rainWorld.buildType == RainWorld.BuildType.Distribution)
            { return s; }
            //Use original text if there is a translation error
            if (s.Length > 0 && s[0] == '!') //get_Chars(0)
            {
                UnityEngine.Debug.Log($"DoubleTranslation ERROR (len: {s.Length}):\n{s}");
                return s;
            }
            UnityEngine.Debug.Log($"No Translation ERROR (len: {s.Length}):\n{s}");
            return s;
        }

        public static void LoadTablePatch(On.InGameTranslator.orig_LoadTable orig, InGameTranslator instance, int l)
        {
            string text;

            if (!pe)
            {
                if (!File.Exists(InGameTranslatorPatch.ShortStringsDirectory(l))) { return; }

                text = File.ReadAllText(InGameTranslatorPatch.ShortStringsDirectory(l), Encoding.UTF8);
                text = Crypto.DecryptStringAES(text, (12467 - l).ToString("X6"));

                text = text.Remove(0, 1); //original file
                string[] array = Regex.Split(text, Environment.NewLine);
                string[,] array2 = new string[array.Length, Enum.GetNames(typeof(InGameTranslator.LanguageID)).Length];
                for (int i = 0; i < array.Length; i++)
                {
                    string[] array3 = array[i].Split(new char[] { '|' });
                    for (int j = 0; j < array3.Length; j++)
                    {
                        if (i < array2.GetLength(0) && j < array2.GetLength(1))
                        { array2[i, j] = array3[j]; }
                    }
                }
                instance.tables.Add(l, array2);

                return;
            }
            else
            {
                if (!File.Exists(InGameTranslatorPatch.ShortStringsDirectory(l))) { return; }

                text = File.ReadAllText(InGameTranslatorPatch.ShortStringsDirectory(l), Encoding.UTF8);
                text = Crypto.DecryptStringAES(text, (12467 - l).ToString("X6")); //Custom.xorEncrypt(text, 12467 - l);

                string refPath = string.Concat(Custom.RootFolderDirectory(), Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Short_strings", Path.DirectorySeparatorChar, l, ".txt");

                string refText = File.ReadAllText(refPath, Encoding.UTF8);
                if (refText.Substring(0, 1) != "0") { refText = Custom.xorEncrypt(refText, 12467 - l); }
                refText = refText.Remove(0, 1);
                string[] arrayr = Regex.Split(refText, Environment.NewLine);
                string[] array2r = new string[arrayr.Length];
                for (int j = 0; j < arrayr.Length; j++)
                {//original English
                    array2r[j] = arrayr[j].Split(new char[] { '|' })[0];
                }

                string[] array = Regex.Split(text, Environment.NewLine);
                string[,] array2 = new string[array2r.Length, 2];

                for (int i = 0; i < array2r.Length; i++)
                {
                    array2[i, 0] = array2r[i];
                    if (i >= array.Length) { array2[i, 1] = array2r[i]; continue; }
                    array2[i, 1] = array[i];
                }
                instance.tables.Add(l, array2);
            }
        }
    }
}
