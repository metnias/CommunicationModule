using System.Collections.Generic;

namespace CommunicationModule.Patch
{
    public static class SLOracleBehaviorHasMarkPatch
    {
        public static void Patch()
        {
            On.SLOracleBehaviorHasMark.NameForPlayer += new On.SLOracleBehaviorHasMark.hook_NameForPlayer(NameForPlayerPatch);
            On.SLOracleBehaviorHasMark.ThirdAndUpGreeting += new On.SLOracleBehaviorHasMark.hook_ThirdAndUpGreeting(ThirdAndUpGreeting);
        }

        public static Dictionary<string, int> tuch7C;

        public static string[] GetpatchName()
        {
            return CommunicationModule.ComMod.playerName;
        }

        public static string NameForPlayerPatch(On.SLOracleBehaviorHasMark.orig_NameForPlayer orig, SLOracleBehaviorHasMark instance, bool capitalized)
        {
            string engName = "creature";
            bool flag = instance.DamagedMode && UnityEngine.Random.value < 0.5f;
            if (UnityEngine.Random.value > 0.3f)
            {
                switch (instance.State.GetOpinion)
                {
                    case SLOrcacleState.PlayerOpinion.Dislikes:
                        engName = "tormentor";
                        goto GotName;
                    case SLOrcacleState.PlayerOpinion.Likes:
                        if (instance.State.totalPearlsBrought > 5 && !instance.DamagedMode)
                        {
                            engName = "archaeologist";
                        }
                        else
                        {
                            engName = "friend";
                        }
                        goto GotName;
                }
                engName = "creature";
            }
        GotName:
            if (CommunicationModule.ComMod.DataEnabled)
            {
                string cusName;
                if (engName != null)
                {
                    if (tuch7C == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(3)
                        {
                            { "archaeologist", 0 },
                            { "friend", 1 },
                            { "tormentor", 2 }
                        };
                        tuch7C = dictionary;
                    }
                    if (tuch7C.TryGetValue(engName, out int num))
                    {
                        switch (num)
                        {
                            case 0:
                                cusName = GetpatchName()[0];
                                goto AddLittle;
                            case 1:
                                cusName = GetpatchName()[1];
                                goto AddLittle;
                            case 2:
                                cusName = GetpatchName()[2];
                                goto AddLittle;
                        }
                    }
                }
                cusName = GetpatchName()[3];
            AddLittle:
                if (string.IsNullOrEmpty(GetpatchName()[4].Trim()))
                { return (cusName + (!flag ? "" : "... ")); }
                if (ComMod.customLang.ToLower() == "chi")
                {
                    return ((!capitalized) ? GetpatchName()[4] : GetpatchName()[5]) + ((!flag) ? "" : "…") + cusName;
                }
                else if (GetpatchName()[6].StartsWith("t"))
                {
                    return ((!capitalized) ? GetpatchName()[4] : GetpatchName()[5]) + ((!flag) ? " " : "... ") + cusName;
                }
                else
                {
                    if (capitalized && GetpatchName()[4] != GetpatchName()[5])
                    {
                        string CapName = cusName.Substring(0, 1);
                        CapName = CapName.ToUpper();
                        cusName = CapName + cusName.Substring(1);
                        return cusName + ((!flag) ? " " : "... ") + GetpatchName()[4];
                    }
                    else
                    {
                        return cusName + ((!flag) ? " " : "... ") + GetpatchName()[4];
                    }
                }
            }
            else
            {
                return ((!capitalized) ? "little" : "Little") + ((!flag) ? " " : "... ") + engName;
            }
        }

        public static void ThirdAndUpGreeting(On.SLOracleBehaviorHasMark.orig_ThirdAndUpGreeting orig, SLOracleBehaviorHasMark instance)
        {
            switch (instance.State.neuronsLeft)
            {
                case 0:
                    break;

                case 1:
                    instance.dialogBox.Interrupt("...", 40);
                    break;

                case 2:
                    instance.dialogBox.Interrupt(instance.Translate("...leave..."), 20);
                    break;

                case 3:
                    instance.dialogBox.Interrupt(instance.Translate("...you."), 10);
                    instance.dialogBox.NewMessage(instance.Translate("...leave me alone..."), 10);
                    break;

                default:
                    if (instance.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                    {
                        switch (UnityEngine.Random.Range(0, 4))
                        {
                            case 0:
                                instance.dialogBox.Interrupt(instance.Translate("Here again."), 10);
                                break;

                            case 1:
                                instance.dialogBox.Interrupt(instance.Translate("You."), 10);
                                instance.dialogBox.NewMessage(instance.Translate("I wish you would stop coming here."), 10);
                                break;

                            case 2:
                                instance.dialogBox.Interrupt(instance.Translate("You again."), 10);
                                instance.dialogBox.NewMessage(instance.Translate("Please leave me alone."), 10);
                                break;

                            default:
                                instance.dialogBox.Interrupt(instance.Translate("Oh, it's you, <PlayerName>."), 10);
                                break;
                        }
                    }
                    else
                    {
                        bool flag = instance.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes;
                        switch (UnityEngine.Random.Range(0, 5))
                        {
                            case 0:
                                instance.dialogBox.Interrupt(instance.Translate("Hello again, <PlayerName>" + ((!flag) ? "." : "!")), 10);
                                break;

                            case 1:
                                instance.dialogBox.Interrupt(instance.Translate("Hello, <PlayerName>" + ((!flag) ? "." : "!")), 10);
                                instance.dialogBox.NewMessage(instance.Translate((!flag) ? "You're here again." : "How have you been?"), 10);
                                break;

                            case 2:
                                instance.dialogBox.Interrupt(instance.Translate("Oh, <PlayerName>. Hello" + ((!flag) ? "." : "!")), 10);
                                break;

                            case 3:
                                instance.dialogBox.Interrupt(instance.Translate((!flag) ? "It's you, <PlayerName>. Hello." : "It's you, <PlayerName>!  Hello!"), 10); // Matched typo in translator
                                break;

                            case 4:
                                instance.dialogBox.Interrupt(instance.Translate("Ah... <PlayerName>, you're here again" + ((!flag) ? "." : "!")), 10);
                                break;

                            default:
                                instance.dialogBox.Interrupt(instance.Translate("Ah... <PlayerName>, you're back" + ((!flag) ? "." : "!")), 10);
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
