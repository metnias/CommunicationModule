using CommunicationModule.Encrypt;
using CommunicationModule.FutileManager;
using RWCustom;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// When there is no translation data
/// </summary>

namespace CommunicationModule
{
    public class ComCtrler : MonoBehaviour
    {
        public static ComMod mod;

        // public static bool runtime = false;

        public static FLabel lbl, lblB;

        public static Vector2 GetSize(string text, bool bigText = false)
        {
            if (lbl != null) { ComMod.InitTestLabel(); }
            //if (lbl != null) { return new Vector2((bigText ? ComMod.pdWidth / ComMod.pdMulti : ComMod.pWidth / ComMod.pMulti) * text.Length, bigText ? ComMod.pdHeight / ComMod.pdMulti : ComMod.pHeight / ComMod.pMulti); }
            FLabel l = bigText ? lblB : lbl;
            l.text = text;
            return new Vector2(l.textRect.width, l.textRect.height);
        }

#pragma warning disable CA1822 // Mark members as static

        public void Update()
        {
            /*
            if (Patch.InGameTranslatorPatch.patchEnabled) { return; }

            if (!runtime)
            {
                RainWorld rw = FindObjectOfType<RainWorld>();
                if (rw.processManager.upcomingProcess == ProcessManager.ProcessID.MainMenu)
                {
                    runtime = true;
                    ComMod.Initialize();
                    // if (ComMod.DataEnabled) { patch_InGameTranslator.patchEnabled = true; }
                }
                else { return; }
            } */

            if (Input.GetKeyDown(KeyCode.R)) //Reload Font = Ctrl+R
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Futile.atlasManager.DoesContainElementWithName("pAtlas")) { Futile.atlasManager.UnloadAtlas("pAtlas"); }
                    if (Futile.atlasManager.DoesContainElementWithName("pdAtlas")) { Futile.atlasManager.UnloadAtlas("pdAtlas"); }
                    ComMod.Initialize();
                }
            }

            if (Input.GetKeyDown(KeyCode.X)) //eXtract
            {
                //check if the trigger file exists
                if (!File.Exists(string.Concat(Custom.RootFolderDirectory(), "enableextract.txt"))) { return; }

                //file exist -> return
                if (!File.Exists(string.Concat(Custom.RootFolderDirectory(), "Extract.txt")))
                {
                    Patch.ConversationPatch.ExtractAllDialogue();

                    return;
                }
                else
                {
                    Debug.Log("CommunicationController: Already eXtracted.");
                    return;
                }
            }
            if (Input.GetKeyDown(KeyCode.E)) //Encode & Reload
            {
                //check if the trigger file exists
                if (!File.Exists(string.Concat(Custom.RootFolderDirectory(), "enableextract.txt"))) { return; }
                // if (ComMod.DataEnabled) { return; }
                if (!File.Exists(string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Rgn.txt")))
                {
                    if (File.Exists(string.Concat(Custom.RootFolderDirectory(), "Translation.txt")))
                    {
                        //create txt files from Translation.txt
                        Encode();

                        //Reload
                        ComMod.Initialize();
                        return;
                    }
                    else
                    {
                        Debug.Log("CommunicatonController: Nothing to Encode.");
                        return;
                    }
                }
                else
                {
                    Debug.Log("CommunicatonController: Encoded Data Exists.");
                    return;
                }
            }
        }

        public static void Encode() //Reimport!
        {
            //Path Check
            DirectoryInfo check = new DirectoryInfo(string.Concat(ComMod.path, Path.DirectorySeparatorChar));
            if (!check.Exists) { check.Create(); }
            check = new DirectoryInfo(string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar));
            if (!check.Exists) { check.Create(); }
            check = new DirectoryInfo(string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Text_Ctm", Path.DirectorySeparatorChar));
            if (!check.Exists) { check.Create(); }

            Debug.Log("Begin Encoding Translation.txt!");

            //load txt & create Short_Strings
            string[] dirtyLines = Regex.Split(File.ReadAllText(string.Concat(Custom.RootFolderDirectory(), "Translation.txt"), Encoding.UTF8), Environment.NewLine);

            //clean array
            string cleanText = string.Empty;
            for (int l = 0; l < dirtyLines.Length; l++)
            {
                if (dirtyLines[l].Length > 2)
                {
                    if (dirtyLines[l].Substring(0, 1) == "\"" && dirtyLines[l].Substring(dirtyLines[l].Length - 1) == "\"")
                    {
                        dirtyLines[l] = dirtyLines[l].Trim(new char[] { '\"' });
                    }
                }
                dirtyLines[l] = dirtyLines[l].Replace("\"\"", "\"");

                cleanText = cleanText + dirtyLines[l].ToString() + Environment.NewLine;
            }
            cleanText = cleanText.Substring(0, cleanText.Length - 1);
            //Debug.Log(cleanText);
            string[] array = Regex.Split(cleanText, "<SstA>");

            for (int s = 0; s < array.Length - 1; s++)
            {//ignore <SstB>##
                //SstD ~ SstC
                //string textd = Regex.Split(array[s], "<SstB>")[1];
                string textd = array[s].Split(new string[] { "<SstB>" }, StringSplitOptions.None)[1];

                int d = textd.IndexOf(Environment.NewLine);
                int num = int.Parse(textd.Substring(0, d)); //beginning ~ line break

                string[] arrays = Regex.Split(array[s], "<SstC>");
                string text = string.Empty;
                for (int t = 0; t < arrays.Length - 1; t++)
                {
                    string textc = Regex.Split(arrays[t], "<SstD>")[1];
                    textc = Regex.Split(textc, Environment.NewLine)[1];
                    text = text + textc + Environment.NewLine;
                }
                text = text.Remove(text.Length - 2);
                string place = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar, num, ".txt");

                //string text2 = Custom.xorEncrypt('1' + text, 12467 - num);

                //text = '1' + text;
                string text2 = Crypto.EncryptStringAES(text, (12467 - num).ToString("X6"));

                File.WriteAllText(place, text2, Encoding.UTF8);
            }

            //create Text_Ctm
            string[] array2 = Regex.Split(array[array.Length - 1], "<CtmA>");

            for (int c = 0; c < array2.Length - 1; c++)
            {
                string text = Regex.Split(array2[c], "<CtmB>")[1];
                int d = text.IndexOf(Environment.NewLine);
                int num = int.Parse(text.Substring(0, d)); //til line break.
                d = text.IndexOf('-');
                if (d > 1)
                {
                    text = text.Substring(d);
                    text = text.Substring(0, text.Length - 2);
                }
                else
                { text = '-' + text; }
                text.Trim(new char[] { '\n', '\r', ' ' });

                string place = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Text_Ctm", Path.DirectorySeparatorChar, num, ".txt");

                //text = '1' + text;
                string text2 = Crypto.EncryptStringAES(text, (61 + num).ToString("X6"));

                File.WriteAllText(place, text2, Encoding.UTF8);
            }

            //create Pnm.txt
            string path = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Pnm.txt");
            string name = Regex.Split(array2[array2.Length - 1], "<PnmB>")[1];
            int k = name.IndexOf(Environment.NewLine);
            name = name.Substring(k);

            string[] array3 = Regex.Split(name, Environment.NewLine);
            name = string.Empty;
            //a|a|a|a|a|a|b
            for (int p = 0; p < 7; p++)
            {
                name = name + array3[p + 1] + '/';
            }

            name = name.Remove(name.Length - 1); //Remove Last |

            //name = '1' + name;
            name = Crypto.EncryptStringAES(name, "aTopic");

            //add '1' before data to every txt files.

            File.WriteAllText(path, name, Encoding.UTF8);

            path = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Rgn.txt");
            name = string.Empty;
            bool flag = false;
            for (int p = 8; p < array3.Length; p++)
            {
                //Debug.Log(string.Concat(p + ": " + array3[p].Substring(0, 6) + "/ Len: " + array3[p].Length));
                if (!flag)
                {
                    if (array3[p].Length > 7 && array3[p].Substring(0, 6) == "<rgnB>") { flag = true; continue; }
                    continue;
                }
                else
                {
                    if (array3[p].Length > 7 && array3[p].Substring(0, 6) == "<rgnA>") { goto RGNfile; }
                    name = name + array3[p] + '/';
                }
            }
        RGNfile:
            name = name.Remove(name.Length - 1);
            name = Crypto.EncryptStringAES(name, "YellowLizard");
            File.WriteAllText(path, name, Encoding.UTF8);

            //Encrypt Sst and Ctm
            ComMod.DataEnabled = true;
            //patch_Conversation.EncryptAllDialogue();
        }

        public static void LoadFont()
        {
            try
            {
                Debug.Log("Loading External Atlases!");
                FAtlas pAtlas = FManager.LoadExtAtlas("pAtlas");
                FAtlas pdAtlas = FManager.LoadExtAtlas("pdAtlas");
                Debug.Log("Load External AtlasData.");
                FManager.LoadExtAtlasData(ref pAtlas);
                FManager.LoadExtAtlasData(ref pdAtlas);
                Debug.Log("Loading External Fonts!");
                FManager.LoadExtFont("pFont", "pAtlas", "Mods/Language/pFont", 0f, 0f);
                FManager.LoadExtFont("pDisFont", "pdAtlas", "Mods/Language/pDisFont", 0f, 0f);
                //pFont: 12px, pDisFont: 16px;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            try
            {
                Futile.atlasManager.GetFontWithName("pFont");
                Futile.atlasManager.GetFontWithName("pDisFont");
            }
            catch (Exception ex)
            {
                Debug.Log("Custom Fonts Failed to Load!");
                Debug.LogError(ex);
                ComMod.fontExist = false;
                return;
            }

            Debug.Log(string.Concat("Custom Fonts Loaded to Futile."));
            ComMod.fontExist = true;
        }
    }
}
