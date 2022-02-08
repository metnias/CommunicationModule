using CommunicationModule.Encrypt;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CommunicationModule.Patch
{
    public static class ConversationPatch
    {
        public static void Patch()
        {
            On.Conversation.LoadEventsFromFile += new On.Conversation.hook_LoadEventsFromFile(LoadEventsFromFile0Patch);
            On.Conversation.LoadEventsFromFile_1 += new On.Conversation.hook_LoadEventsFromFile_1(LoadEventsFromFile1Patch);
        }

        public static void LoadEventsFromFile0Patch(On.Conversation.orig_LoadEventsFromFile orig, Conversation instance, int fileName)
        {
            instance.LoadEventsFromFile(fileName, false, 0);
        }

        public static bool patchEnabled
        {
            get
            {
                return InGameTranslatorPatch.patchEnabled;
            }
        }

        public static void LoadEventsFromFile1Patch(On.Conversation.orig_LoadEventsFromFile_1 orig, Conversation instance, int fileName, bool oneRandomLine, int randomSeed)
        {
            if (!patchEnabled)
            {
                orig.Invoke(instance, fileName, oneRandomLine, randomSeed);
                return;
            }

            Debug.Log("~~~LOAD CONVO " + fileName);
            string text = string.Concat(InGameTranslatorPatch.SpecificTextFolderDirectory(), Path.DirectorySeparatorChar, fileName, ".txt");
            if (!File.Exists(text)) { Debug.Log("NOT FOUND " + text); return; }
            string text2 = File.ReadAllText(text, Encoding.UTF8);

            text2 = Crypto.DecryptStringAES(text2, (61 + fileName).ToString("X6"));
            text2.Trim(new char[] { '\n', '\r', ' ' });

            //Debug.Log(string.Concat(fileName, " Conversation: ", Environment.NewLine, text2));

            string[] array = Regex.Split(text2, Environment.NewLine);
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    if (oneRandomLine)
                    {
                        List<Conversation.TextEvent> list = new List<Conversation.TextEvent>();
                        for (int i = 1; i < array.Length; i++)
                        {
                            string[] array2 = Regex.Split(array[i], " : ");
                            if (array2.Length == 3)
                            {
                                list.Add(new Conversation.TextEvent(instance, int.Parse(array2[0]), array2[2], int.Parse(array2[1])));
                            }
                            else if (array2.Length == 1 && array2[0].Length > 0)
                            {
                                list.Add(new Conversation.TextEvent(instance, 0, array2[0], 0));
                            }
                        }
                        if (list.Count > 0)
                        {
                            int seed = UnityEngine.Random.seed;
                            UnityEngine.Random.seed = randomSeed;
                            Conversation.TextEvent textEvent = list[UnityEngine.Random.Range(0, list.Count)];
                            UnityEngine.Random.seed = seed;
                            instance.events.Add(textEvent);
                        }
                    }
                    else
                    {
                        for (int j = 1; j < array.Length; j++)
                        {
                            string[] array3 = Regex.Split(array[j], " : ");
                            if (array3.Length == 3)
                            {
                                instance.events.Add(new Conversation.TextEvent(instance, int.Parse(array3[0]), array3[2], int.Parse(array3[1])));
                            }
                            else if (array3.Length == 2)
                            {
                                if (array3[0] == "SPECEVENT")
                                {
                                    instance.events.Add(new Conversation.SpecialEvent(instance, 0, array3[1]));
                                }
                                else if (array3[0] == "PEBBLESWAIT")
                                {
                                    instance.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(instance, null, int.Parse(array3[1])));
                                }
                            }
                            else if (array3.Length == 1 && array3[0].Length > 0)
                            {
                                instance.events.Add(new Conversation.TextEvent(instance, 0, array3[0], 0));
                            }
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("TEXT ERROR");
                instance.events.Add(new Conversation.TextEvent(instance, 0, "TEXT ERROR", 100));
            }
        }

        public static void ExtractAllDialogue()
        {
            Debug.Log("Extract all texts");

            //Original route
            DirectoryInfo directoryInfo = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar));

            //If you cannot find the Path, Create one
            DirectoryInfo check = new DirectoryInfo(string.Concat(ComMod.path, Path.DirectorySeparatorChar));
            if (!check.Exists) { check.Create(); }
            check = new DirectoryInfo(string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar));
            if (!check.Exists) { check.Create(); }
            check = new DirectoryInfo(string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Text_Ctm", Path.DirectorySeparatorChar));
            if (!check.Exists) { check.Create(); }

            string[] data = new string[60000];
            int idx = 0;

            FileInfo[] files = directoryInfo.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.Substring(files[i].Name.Length - 4, 4) == ".txt")
                {
                    int num = int.Parse(files[i].Name.Substring(0, files[i].Name.Length - 4));
                    string text = string.Concat(Custom.RootFolderDirectory(), Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar, files[i].Name);
                    string text2 = File.ReadAllText(text, Encoding.Default);
                    _ = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar, files[i].Name);
                    data[idx] = string.Concat("<SstB>", num);
                    idx++;

                    string text3 = Custom.xorEncrypt(text2, 12467 - num);
                    text3 = text3.Remove(0, 1);
                    Debug.Log("Decrypting short string: " + num);
                    //File.WriteAllText(text, text3); //text3

                    string[] dialogue = Regex.Split(text3, Environment.NewLine);
                    if (dialogue.Length > 0)
                    {
                        for (int k = 0; k < dialogue.Length; k++)
                        {
                            int split = dialogue[k].IndexOf("|");
                            if (split > 0)
                            {
                                data[idx] = string.Concat("<SstD>", k);
                                data[idx + 1] = dialogue[k].Substring(0, split);
                                data[idx + 2] = string.Concat("<SstC>", k);
                                idx += 3;
                            }
                        }
                    }

                    data[idx] = string.Concat("<SstA>", num);
                    idx++;
                }
            }
            for (int k = 1; k <= 57; k++)
            {
                string text = string.Concat(Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Text_Eng", Path.DirectorySeparatorChar, k, ".txt");
                if (File.Exists(text))
                {
                    string text2 = File.ReadAllText(text, Encoding.Default);
                    _ = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Text_Ctm", Path.DirectorySeparatorChar, k, ".txt");
                    //if (Regex.Split(text2, Environment.NewLine).Length > 1) //text2[0] == '0' &&
                    //{
                    string text4 = Custom.xorEncrypt(text2, 54 + k);
                    // = Custom.xorEncrypt(text2, 54 + k + j * 7);
                    text4 = text4.Remove(0, 1);
                    Debug.Log("Decrypting _Eng string: " + k);
                    //File.WriteAllText(text, text4); //text4

                    data[idx] = string.Concat("<CtmB>", k);
                    data[idx + 1] = text4;
                    data[idx + 2] = string.Concat("<CtmA>", k);
                    idx += 3;
                    //}
                }
            }
            //public static string[] playerName = { "archaeologist", "friend", "tormentor", "creature", "little", "Little" };
            //PlayerName Data
            data[idx] = "<PnmB> How Looks to the Moon Calls Player. This replaces <PlayerName> / <CapPlayerName> in dialogues.";
            data[idx + 1] = "archaeologist";
            data[idx + 2] = "friend";
            data[idx + 3] = "tormentor";
            data[idx + 4] = "creature";
            data[idx + 5] = "little";
            data[idx + 6] = "Little";
            data[idx + 7] = "true";
            data[idx + 8] = "<PnmA> If ^ is true, 'Little creature'. If ^ is false, 'Creature little'.";
            idx += 10;

            int len = CommunicationModule.ComMod.regionName.Length;
            data[idx - 1] = "<rgnB> Region Names";
            for (int k = 0; k < len; k++)
            {
                data[idx + k] = CommunicationModule.ComMod.regionName[k];
            }
            data[idx + len] = "<rgnA> Region Names";
            idx += len + 2;

            //save data

            //Temp txt Gen
            string sum = string.Empty;
            try
            {
                for (int p = 0; p <= idx; p++)
                {
                    if (string.IsNullOrEmpty(data[p])) { continue; }
                    sum += data[p] + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            string path = string.Concat(Custom.RootFolderDirectory(), "Extract.txt");

            File.WriteAllText(path, sum);
        }

        /*
        public static void ExtractAllDialogue()
        {
            Debug.Log("Extract all texts");

            //Original route
            DirectoryInfo directoryInfo = new DirectoryInfo(string.Concat(Custom.RootFolderDirectory(), "Text", Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar));

            string[] data = new string[60000];
            int idx = 0;

            FileInfo[] files = directoryInfo.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.Substring(files[i].Name.Length - 4, 4) == ".txt")
                {
                    int num = int.Parse(files[i].Name.Substring(0, files[i].Name.Length - 4));
                    string text = string.Concat(Custom.RootFolderDirectory(), Path.DirectorySeparatorChar, "Text", Path.DirectorySeparatorChar, "Short_Strings", Path.DirectorySeparatorChar, files[i].Name);
                    string text2 = File.ReadAllText(text, Encoding.Default);
                    data[idx] = string.Concat("<SstB>", num);
                    idx++;

                    string text3 = text2.Remove(0, 1);
                    Debug.Log("Decrypting short string: " + num);
                    //File.WriteAllText(text, text3); //text3

                    string[] dialogue = Regex.Split(text3, Environment.NewLine);
                    if (dialogue.Length > 0)
                    {
                        for (int k = 0; k < dialogue.Length; k++)
                        {
                            string[] sp = dialogue[k].Split(new char[] { '|' });
                            //int split = dialogue[k].IndexOf("|");
                            if (sp.Length > 3)
                            {
                                data[idx] = string.Concat("<SstD>", k);
                                data[idx + 1] = sp[7];
                                data[idx + 2] = string.Concat("<SstC>", k);
                                idx += 3;
                            }
                        }
                    }

                    data[idx] = string.Concat("<SstA>", num);
                    idx++;
                }
            }
            for (int k = 1; k <= 57; k++)
            {
                string text = string.Concat(Custom.RootFolderDirectory(), "Text", Path.DirectorySeparatorChar, "Text_Jap", Path.DirectorySeparatorChar, k, ".txt");
                if (File.Exists(text))
                {
                    string text2 = File.ReadAllText(text, Encoding.Default);
                    //if (Regex.Split(text2, Environment.NewLine).Length > 1) //text2[0] == '0' &&
                    //{
                    string text4 = text2.Remove(0, 1);
                    Debug.Log("Decrypting _Jap string: " + k);
                    //File.WriteAllText(text, text4); //text4

                    data[idx] = string.Concat("<CtmB>", k);
                    data[idx + 1] = text4;
                    data[idx + 2] = string.Concat("<CtmA>", k);
                    idx += 3;
                    //}
                }
            }
            //public static string[] playerName = { "archaeologist", "friend", "tormentor", "creature", "little", "Little" };
            //PlayerName Data
            data[idx] = "<PnmB> How Looks to the Moon Calls Player. This replaces <PlayerName> / <CapPlayerName> in dialogues.";
            data[idx + 1] = "archaeologist";
            data[idx + 2] = "friend";
            data[idx + 3] = "tormentor";
            data[idx + 4] = "creature";
            data[idx + 5] = "little";
            data[idx + 6] = "Little";
            data[idx + 7] = "true";
            data[idx + 8] = "<PnmA> If ^ is true, 'Little creature'. If ^ is false, 'Creature little'.";
            idx += 10;

            int len = CommunicationModule.ComMod.regionName.Length;
            data[idx - 1] = "<rgnB> Region Names";
            for (int k = 0; k < len; k++)
            {
                data[idx + k] = CommunicationModule.ComMod.regionName[k];
            }
            data[idx + len] = "<rgnA> Region Names";
            idx += len + 2;

            //save data

            //Temp txt Gen
            string sum = string.Empty;
            try
            {
                for (int p = 0; p <= idx; p++)
                {
                    if (string.IsNullOrEmpty(data[p])) { continue; }
                    sum += data[p] + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            string path = string.Concat(Custom.RootFolderDirectory(), "Extract.txt");

            File.WriteAllText(path, sum);
        }
        */
    }
}
