using HUD;

namespace CommunicationModule.Patch
{
    public static class SubregionTrackerPatch
    {
        public static void Patch()
        {
            On.HUD.SubregionTracker.Update += new On.HUD.SubregionTracker.hook_Update(UpdatePatch);
        }

        public static void UpdatePatch(On.HUD.SubregionTracker.orig_Update orig, SubregionTracker instance)
        {
            Player player = instance.textPrompt.hud.owner as Player;
            int num = 0;
            if (player.room != null)
            {
                num = player.room.abstractRoom.subRegion;
            }
            if (!instance.DEVBOOL && num != 0 && player.room.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev)
            {
                instance.lastShownRegion = num;
                instance.DEVBOOL = true;
            }
            if (num != instance.lastShownRegion && player.room != null && num != 0 && instance.lastRegion == num && instance.textPrompt.show == 0f)
            {
                instance.counter++;
                if (instance.counter > 80)
                {
                    if ((num > 1 || instance.lastShownRegion == 0 || player.room.world.region.name != "SS") && num < player.room.world.region.subRegions.Count)
                    {
                        if (instance.showCycleNumber && player.room.game.IsStorySession && player.room.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Load)
                        {
                            int num2 = player.room.game.GetStorySession.saveState.cycleNumber;
                            if ((player.room.game.session as StoryGameSession).saveState.saveStateNumber == 2)
                            {
                                num2 = RedsIllness.RedsCycles(player.room.game.GetStorySession.saveState.redExtraCycles) - num2;
                            }
                            instance.textPrompt.AddMessage(string.Concat(
                            instance.textPrompt.hud.rainWorld.inGameTranslator.Translate("Cycle"),
                            " ",
                            num2,
                            " ~ ",
                            InGameTranslatorPatch.TranslateRegion(player.room.world.region.subRegions[num])),
                            0, 160, false, true);
                        }
                        else
                        {
                            instance.textPrompt.AddMessage(InGameTranslatorPatch.TranslateRegion(player.room.world.region.subRegions[num]), 0, 160, false, true);
                        }
                    }
                    instance.showCycleNumber = false;
                    instance.lastShownRegion = num;
                }
            }
            else
            {
                instance.counter = 0;
            }
            instance.lastRegion = num;
        }
    }
}
