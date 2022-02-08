using CommunicationModule.Encrypt;
using CommunicationModule.Patch;
using Menu;
using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: AssemblyVersion(CommunicationModule.ComMod.vers)]
[assembly: AssemblyFileVersion(CommunicationModule.ComMod.vers)]

namespace CommunicationModule
{
    public class ComMod : PartialityMod
    {
        public const string vers = "2.5.2.2";

        public ComMod()
        {
            ModID = "CommunicationModule";
            Version = vers; //Assembly.GetEntryAssembly().GetName().Version.ToString();
            author = "topicular";
        }

        public static GameObject go;
        public static ComCtrler cc;

        public static float pWidth = 6.4f;
        public static float pHeight = 15f;
        public static int pMulti = 1;

        public static float pdWidth = 11.1f;
        public static float pdHeight = 30f;
        public static int pdMulti = 1;

        // public const BindingFlags flagStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        // public static bool bepInEx = false;

        public static readonly string[] playerNameEng = { "archaeologist", "friend", "tormentor", "creature", "little", "Little", "true" };
        public static string[] playerName = (string[])playerNameEng.Clone();

        public static readonly string[] regionNameEng = {
            "Outskirts", "Drainage System", "Industrial Complex", "Garbage Wastes", "Shoreline", "Looks to the Moon",
            "Shaded Citadel", "Memory Crypts", "The Exterior", "The Leg", "The Wall", "Underhang", "Five Pebbles",
            "Five Pebbles (Memory Conflux)", "Five Pebbles (Unfortunate Development)", "Five Pebbles (Recursive Transform Array)",
            "Five Pebbles (General Systems Bus)", "Chimney Canopy", "Sky Islands", "Communications Array",
            "Farm Arrays", "Subterranean", "Filtration System", "Depths"
        };

        public static string[] regionName = (string[])regionNameEng.Clone();

        // Code for AutoUpdate support
        // Should be put in the main PartialityMod class.
        // Comments are optional.

        // Update URL - don't touch!
        // You can go to this in a browser (it's safe), but you might not understand the result.
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/2/1";

        // Version - increase this by 1 when you upload a new version of the mod.
        // The first upload should be with version 0, the next version 1, the next version 2, etc.
        // If you ever lose track of the version you're meant to be using, ask Pastebin.
        public int version = 18;

        // Public key in base64 - don't touch!
        public string keyE = "AQAB";

        public string keyN = "63oqj+JWQUUgPdZYGdMrZC1V8cNWBI6WYr04NxBKVPb1rrvDs8xRudZAuSdErhHyb9Qa6+ziG4GiXzfC8gElkSZ4uUfQgfomBgd4NACRCl+xHlIhyyYEFuexlj0pGK/OXKS8cX5zLqSXgGxnCfeqmHrs6pBvDrBnXHfWv1N6vOMBKmReRSioj2CBNJqYQxIY91Aoiyg+jf6AcJa/WOf9GmEr6OFcWQRkckf/GAcyq8EAV+BAQUOTfsZoYcFGqKGVmdFjMQokuuj0/Ut/zvd9SX/LwoIwCfZpfuGTBGM1+2+h9zd0US0eSG6en8QswahPukSKv2R+8uHfPBMCATzhgAoaf7xGM6zFwTgHlIODY4u8plB29OEOfelIYF0TSDVmuEPQFYXlNE2VsIUQLPuI4zjKxirEd2MQQUuek/P4X98SyPxmT53bLgoNPwfSXkUo4n2/b0gXgf7FCAU6XtFf1DZKwNo+3s6ESVidZUqYykTFRXQ5KLGHB6elC2oB/9J7m4AeysLDk+FU7AKnJ/VNwc6t+wq9d+8wolS6wBVFPbapprpJqRS5SYAdY+GpCSqO+IPIDZKYp1tQbx5J9egvr7zo5YiXDZmParFQk1Kc9ikLQgtIxk9D8qF4rYjcx96ZtMrDJM98vMuj9TdMzL5zqQRrfZpEAn7AYOlQs2uUP+E=";

        public static bool DataEnabled { get; set; }

        public static bool fontExist;

        public static string path;

        public static void ProcessManagerCtor(On.ProcessManager.orig_ctor orig, ProcessManager self, RainWorld rainWorld)
        {
            try { Initialize(); }
            finally { orig(self, rainWorld); }
        }

        public static void Initialize()
        {
            DataEnabled = false;
            fontExist = false;

            try
            {
                //Search for the patch file
                if (File.Exists(string.Concat(path, Path.DirectorySeparatorChar, "Rgn.txt")))
                {
                    //Load Pnm
                    string text = File.ReadAllText(string.Concat(string.Concat(path, Path.DirectorySeparatorChar, "Pnm.txt")), Encoding.UTF8);
                    string text2 = Crypto.DecryptStringAES(text, "aTopic");

                    string[] array = Regex.Split(text2, "/");

                    playerName = new string[7];
                    for (int i = 0; i < 7; i++) { playerName[i] = array[i]; }

                    //Load Rgn
                    text = File.ReadAllText(string.Concat(string.Concat(path, Path.DirectorySeparatorChar, "Rgn.txt")), Encoding.UTF8);

                    text2 = Crypto.DecryptStringAES(text, "YellowLizard");

                    array = Regex.Split(text2, "/");

                    regionName = new string[24];
                    for (int i = 0; i < 24; i++) { regionName[i] = array[i]; }

                    DataEnabled = true;
                }
                else
                {
                    //font = null;
                    Debug.Log("No Language Data for Communication MODule!");
                    DataEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            //Check Font Files
            try
            {
                if (File.Exists(string.Concat(path, Path.DirectorySeparatorChar, "Atlas", Path.DirectorySeparatorChar, "pAtlas.png")))
                { ComCtrler.LoadFont(); }
                else
                {
                    Debug.Log("Custom Font Not Found!");
                    fontExist = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            Debug.Log(string.Concat("ComMod Data: " + DataEnabled.ToString() + " / Font: " + fontExist.ToString()));

            InitTestLabel();
        }

        public static void InitTestLabel()
        {
            ComCtrler.lbl = new FLabel("font", "") { scale = 1f / ComMod.pMulti };
            ComCtrler.lblB = new FLabel("DisplayFont", "") { scale = 1f / ComMod.pdMulti };
            ComCtrler.lbl.RemoveFromContainer();
            ComCtrler.lblB.RemoveFromContainer();
        }

        public override void OnEnable()
        {
            DataEnabled = false;
            fontExist = false;
            Debug.Log("Activate ComCtrler.");
            go = new GameObject("Communication_Controller");
            cc = go.AddComponent<ComCtrler>();
            ComCtrler.mod = this;
            //path = string.Concat(Custom.RootFolderDirectory(), "Mods", Path.DirectorySeparatorChar, "Language");
            path = string.Concat(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Path.DirectorySeparatorChar, "Language");
            //Debug.Log(path);
            //Debug.Log(string.Concat(Custom.RootFolderDirectory(), "Mods", Path.DirectorySeparatorChar, "Language"));

            Patch();

            customLang = string.Empty;
            if (File.Exists(string.Concat(path, Path.DirectorySeparatorChar, "Lang.txt")))
            {
                string name = File.ReadAllText(string.Concat(path, Path.DirectorySeparatorChar, "Lang.txt"));
                if (name.Length >= 3)
                {
                    customLang = name.Trim().ToLower().Substring(0, 3);
                    Debug.Log(string.Concat("Custom Language: ", customLang));
                }
            }
            if (string.IsNullOrEmpty(customLang))
            {
                string key = File.ReadAllText(string.Concat(path, Path.DirectorySeparatorChar, "Atlas", Path.DirectorySeparatorChar, "pAtlas.txt"));
                Dictionary<string, string> langDic = new Dictionary<string, string>
                {
                    { "cca4263e0a015483dc154d775462b361", "kor" },
                    { "58bc32e0b39ea18f7829925c40f866b3", "rus" }
                };
                string mdsAtlas = Custom.Md5Sum(key);

                if (langDic.TryGetValue(mdsAtlas, out customLang))
                {
                    Debug.Log(string.Concat("Custom Language: ", customLang));
                }
                else
                {
                    Debug.Log(string.Concat("MD5Sum of pAtlas: ", mdsAtlas));
                    customLang = "eng";
                }
            }

            base.OnEnable();
        }

        public static string customLang;

        public static void Patch()
        {
            On.ProcessManager.ctor += ProcessManagerCtor;

            InGameTranslatorPatch.Patch();
            FLabelPatch.Patch();
            FFontPatch.Patch();
            SlugcatPageContinuePatch.Patch();
            ConversationPatch.Patch();
            DialogBoxPatch.Patch();
            InfoWindowPatch.Patch();
            MenuLabelPatch.Patch();
            SandboxEditorSelectorPatch.Patch();
            SLOracleBehaviorHasMarkPatch.Patch();
            SubregionTrackerPatch.Patch();
            TextPromptPatch.Patch();
            ButtonResize.Patch();

            On.Menu.MainMenu.ctor += UpdateNotice;
        }

        // temporary update notice for korean patch
        private static void UpdateNotice(On.Menu.MainMenu.orig_ctor orig, MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig.Invoke(self, manager, showRegionSpecificBkg);

            if (ComMod.customLang == "kor")
            {
                //Debug.Log(self.Translate("SINGLE PLAYER"));
                if (!self.Translate("SINGLE PLAYER").Contains("어")) { return; } // something that differenciates
                MenuLabel l = new MenuLabel(self, self.pages[0], "한글패치 번역이 구버전입니다. RainDB에서 다시 내려받아 주세요.", new Vector2(483f, 720f), new Vector2(400f, 15f), false);
                l.label.color = Color.red;
                self.pages[0].subObjects.Add(l);
            }
        }
    }
}
