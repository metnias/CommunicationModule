using Menu;
using System.Collections.Generic;
using UnityEngine;

namespace CommunicationModule.Patch
{
    public static class SlugcatPageContinuePatch
    {
        public static void Patch()
        {
            On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += new On.Menu.SlugcatSelectMenu.SlugcatPageContinue.hook_ctor(CtorPatch);
        }

        public static void CtorPatch(On.Menu.SlugcatSelectMenu.SlugcatPageContinue.orig_ctor orig, SlugcatSelectMenu.SlugcatPageContinue instance, Menu.Menu menu, MenuObject owner, int pageIndex, int slugcatNumber)
        {
            orig.Invoke(instance, menu, owner, pageIndex, slugcatNumber);

            string text = string.Empty;
            if (instance.saveGameData.shelterName != null && instance.saveGameData.shelterName.Length > 2)
            {
                string text2 = instance.saveGameData.shelterName.Substring(0, 2);
                if (text2 != null)
                {
                    if (tuch74 == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(12)
                        {
                            { "CC", 0 },
                            { "DS", 1 },
                            { "HI", 2 },
                            { "GW", 3 },
                            { "SI", 4 },
                            { "SU", 5 },
                            { "SH", 6 },
                            { "SL", 7 },
                            { "LF", 8 },
                            { "UW", 9 },
                            { "SB", 10 },
                            { "SS", 11 }
                        };
                        tuch74 = dictionary;
                    }
                    if (tuch74.TryGetValue(text2, out int num))
                    {
                        switch (num)
                        {
                            case 0:
                                text = "Chimney Canopy";
                                break;

                            case 1:
                                text = "Drainage System";
                                break;

                            case 2:
                                text = "Industrial Complex";
                                break;

                            case 3:
                                text = "Garbage Wastes";
                                break;

                            case 4:
                                text = "Sky Islands";
                                break;

                            case 5:
                                text = "Outskirts";
                                break;

                            case 6:
                                text = "Shaded Citadel";
                                break;

                            case 7:
                                text = "Shoreline";
                                break;

                            case 8:
                                text = "Farm Arrays";
                                break;

                            case 9:
                                text = "The Exterior";
                                break;

                            case 10:
                                text = "Subterranean";
                                break;

                            case 11:
                                text = "Five Pebbles";
                                break;
                        }
                    }

                    text = InGameTranslatorPatch.TranslateRegion(text);
                }
                if (text.Length > 0)
                {
                    text2 = text;
                    text = string.Concat(
                            text2,
                            " - ",
                            menu.Translate("Cycle"),
                            " ",
                            (slugcatNumber != 2) ? instance.saveGameData.cycle : (RedsIllness.RedsCycles(instance.saveGameData.redsExtraCycles) - instance.saveGameData.cycle)
                    );
                }
            }
            instance.RemoveSubObject(instance.regionLabel);

            instance.regionLabel = new MenuLabel(menu, instance, text, new Vector2(-1000f, instance.imagePos.y - 249f), new Vector2(200f, 30f), true);
            instance.regionLabel.label.alignment = FLabelAlignment.Center;
            instance.subObjects.Add(instance.regionLabel);
        }

        public static Dictionary<string, int> tuch74;
    }
}
