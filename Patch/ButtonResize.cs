using Menu;
using UnityEngine;

namespace CommunicationModule.Patch
{
    internal static class ButtonResize
    {
        public static void Patch()
        {
            On.Menu.MainMenu.ctor += new On.Menu.MainMenu.hook_ctor(MainMenuPatch);
            On.Menu.OptionsMenu.ctor += new On.Menu.OptionsMenu.hook_ctor(OptionsMenuPatch);
        }

        public static void MainMenuPatch(On.Menu.MainMenu.orig_ctor orig, MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig.Invoke(self, manager, showRegionSpecificBkg);
            if (ComMod.customLang.ToLower() != "jap") { return; }
            for (int i = 0; i < self.pages[0].subObjects.Count; i++)
            {
                if (self.pages[0].subObjects[i] is SimpleButton s)
                {
                    s.pos = new Vector2(s.pos.x - 30f, s.pos.y);
                    s.SetSize(new Vector2(s.size.x + 60f, s.size.y));
                    //Debug.Log($"Resize {s.signalText}: {s.size.x}");
                }
            }
        }

        public static void OptionsMenuPatch(On.Menu.OptionsMenu.orig_ctor orig, OptionsMenu self, ProcessManager manager)
        {
            orig.Invoke(self, manager);
            float num;
            switch (ComMod.customLang.ToLower())
            {
                default: return;
                case "jap": num = 50f; break;
                case "rus": num = 30f; break;
            }
            self.wipeSaveButton.pos.x += num / 2f;
            self.resetWarningText.pos.x = self.wipeSaveButton.pos.x - 100f;

            for (int k = 0; k < self.saveSlotButtons.Length; k++)
            {
                self.saveSlotButtons[k].pos.x -= num / 2f;
                self.saveSlotButtons[k].SetSize(new Vector2(self.saveSlotButtons[k].size.x + num, self.saveSlotButtons[k].size.y));
                self.saveSlotButtons[k].outerRect.size = self.saveSlotButtons[k].size;
            }
        }
    }
}
