using HUD;

namespace CommunicationModule.Patch
{
    public static class TextPromptPatch
    {
        public static void Patch()
        {
            On.HUD.TextPrompt.InitNextMessage += new On.HUD.TextPrompt.hook_InitNextMessage(InitNextMsgPatch);
        }

        public static void InitNextMsgPatch(On.HUD.TextPrompt.orig_InitNextMessage orig, TextPrompt instance)
        {
            string translated = instance.hud.rainWorld.inGameTranslator.Translate(instance.messages[0].text);
            instance.messageString = translated;
            for (int i = 0; i < instance.symbols.Count; i++)
            {
                instance.symbols[i].RemoveSprites();
            }
            instance.symbols.Clear();

            if (instance.messages[0] is TextPrompt.SymbolsMessage)
            {
                instance.symbolsX = (instance.messages[0] as TextPrompt.SymbolsMessage).iconsX;
                for (int j = 0; j < (instance.messages[0] as TextPrompt.SymbolsMessage).iconIDs.Count; j++)
                {
                    instance.symbols.Add(IconSymbol.CreateIconSymbol(MultiplayerUnlocks.SymbolDataForSandboxUnlock((instance.messages[0] as TextPrompt.SymbolsMessage).iconIDs[j]), instance.hud.fContainers[1]));
                }
                for (int k = 0; k < instance.symbols.Count; k++)
                {
                    instance.symbols[k].Show(false);
                }
            }
            else if (instance.messages[0] is TextPrompt.MusicMessage && instance.musicSprite == null)
            {
                instance.musicSprite = new FSprite("musicSymbol", true)
                {
                    x = -1000f
                };
                instance.hud.fContainers[1].AddChild(instance.musicSprite);
            }
            instance.hideHud = instance.messages[0].hideHud;
        }
    }
}
