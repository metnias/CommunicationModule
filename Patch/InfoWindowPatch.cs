using Menu;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CommunicationModule.Patch
{
    public static class InfoWindowPatch
    {
        public static void Patch()
        {
            On.Menu.InfoWindow.ctor += new On.Menu.InfoWindow.hook_ctor(CtorPatch);
        }

        public static void CtorPatch(On.Menu.InfoWindow.orig_ctor orig, InfoWindow instance, Menu.Menu menu, MenuObject owner, Vector2 pos)
        {
            orig.Invoke(instance, menu, owner, pos);

            instance.RemoveSubObject(instance.label);

            string text = string.Empty;
            ArenaSetup.GameTypeID gameType = (menu as MultiplayerMenu).GetGameTypeSetup.gameType;
            if (gameType != ArenaSetup.GameTypeID.Competitive)
            {
                if (gameType == ArenaSetup.GameTypeID.Sandbox)
                {
                    text = Regex.Replace(menu.Translate("Create custom scenarios using levels, items and creatures<LINE>that have been unlocked in the single player campaigns.<LINE>Defeating creatures or other actions can be set to award points,<LINE>but Sandbox is mostly about customization and fun!"), "<LINE>", Environment.NewLine);
                }
            }
            else
            {
                text = Regex.Replace(menu.Translate("Compete in a battle against each other and the elements.<LINE>In this mode points are awarded for food items consumed,<LINE>with surviving players always ranking above dead players.<LINE>New levels, items and creatures can be unlocked in the<LINE>single player campaigns."), "<LINE>", Environment.NewLine);
            }
            /* string[] array = Regex.Split(text, Environment.NewLine);
             int num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                num = Math.Max(num, array[i].Length);
            } */
            // Debug.Log($"InfoSize: {ComCtrler.GetSize(text, true).x}, {ComCtrler.GetSize(text, true).y}:\n{text}");
            instance.goalSize = new Vector2(40f, 20f) + ComCtrler.GetSize(text, true); //+ new Vector2((float)num * ComMod.pdWidth * 1.4f, (float)array.Length * ComMod.pdHeight * 3f) ;
            instance.label = new MenuLabel(menu, instance, text, new Vector2(20f, 20f), instance.goalSize, true);
            instance.label.label.alignment = FLabelAlignment.Left;
            instance.subObjects.Add(instance.label);
        }
    }
}
