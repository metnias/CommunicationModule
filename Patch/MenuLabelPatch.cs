using Menu;
using UnityEngine;

namespace CommunicationModule.Patch
{
    public static class MenuLabelPatch
    {
        public static void Patch()
        {
            On.Menu.MenuLabel.GrafUpdate += new On.Menu.MenuLabel.hook_GrafUpdate(GrafUpdatePatch);
        }

        /// <summary>
        /// calibrates
        /// </summary>
        public static void GrafUpdatePatch(On.Menu.MenuLabel.orig_GrafUpdate orig, MenuLabel instance, float timeStacker)
        {
            orig.Invoke(instance, timeStacker);
            if (instance.label._fontName.Contains("Dis")) { if (ComMod.pdMulti > 1) { return; } }
            else { if (ComMod.pMulti > 1) { return; } }
            instance.label.x = instance.label.x - instance.ScreenPos.x + Mathf.Floor(instance.ScreenPos.x) + 0.01f;
            instance.label.y = instance.label.y - instance.ScreenPos.y + Mathf.Floor(instance.ScreenPos.y) + 0.01f;
        }
    }
}
