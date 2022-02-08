using HUD;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CommunicationModule.Patch
{
    public static class DialogBoxPatch
    {
        public static void Patch()
        {
            On.HUD.DialogBox.InitiateSprites += new On.HUD.DialogBox.hook_InitiateSprites(InitSprPatch);
            On.HUD.DialogBox.Draw += new On.HUD.DialogBox.hook_Draw(DrawPatch);
            On.HUD.DialogBox.Message.ctor += new On.HUD.DialogBox.Message.hook_ctor(MsgCtorPatch);
            On.HUD.DialogBox.Interrupt += new On.HUD.DialogBox.hook_Interrupt(InterruptPatch);
            On.HUD.DialogBox.Update += new On.HUD.DialogBox.hook_Update(UpdatePatch);
        }

        public static int delay = 0;

        /// <summary>
        /// Add delays to Asian languages
        /// </summary>
        public static void UpdatePatch(On.HUD.DialogBox.orig_Update orig, DialogBox instance)
        {
            bool speaking = false;
            if (instance.CurrentMessage != null && instance.showCharacter > 0
                && instance.showCharacter < instance.CurrentMessage.text.Length)
            { speaking = true; delay++; }
            else { delay = 0; }

            orig(instance);

            if (speaking)
            {
                if (delay < GetDelay())
                {
                    instance.showCharacter--;
                    instance.showText = instance.CurrentMessage.text.Substring(0, instance.showCharacter);
                }
                else { delay = 0; }
            }

            int GetDelay()
            {
                switch (ComMod.customLang.ToLower())
                {
                    case "chi": return 3;
                    case "kor":
                    case "jap": return 2;
                }
                return 1;
            }
        }

        public static void InitSprPatch(On.HUD.DialogBox.orig_InitiateSprites orig, DialogBox instance)
        {
            if (!ComMod.fontExist)
            {
                orig.Invoke(instance);
                return;
            }

            instance.sprites = new FSprite[17];
            for (int i = 0; i < 4; i++)
            {
                instance.sprites[instance.SideSprite(i)] = new FSprite("pixel", true)
                {
                    scaleY = 2f,// * multiplierHeight;
                    scaleX = 2f// * multiplierWidth;
                };
                instance.sprites[instance.CornerSprite(i)] = new FSprite("UIroundedCorner", true)
                {
                    scaleX = 1f,
                    scaleY = 1f
                };

                instance.sprites[instance.FillSideSprite(i)] = new FSprite("pixel", true)
                {
                    scaleY = 6f,// * multiplierHeight;
                    scaleX = 6f// * multiplierWidth;
                };
                instance.sprites[instance.FillCornerSprite(i)] = new FSprite("UIroundedCornerInside", true)
                {
                    scaleX = 1f,
                    scaleY = 1f
                };
            }
            instance.sprites[instance.SideSprite(0)].anchorY = 0f;
            instance.sprites[instance.SideSprite(2)].anchorY = 0f;
            instance.sprites[instance.SideSprite(1)].anchorX = 0f;
            instance.sprites[instance.SideSprite(3)].anchorX = 0f;
            instance.sprites[instance.CornerSprite(0)].scaleY = -1f;
            instance.sprites[instance.CornerSprite(2)].scaleX = -1f;
            instance.sprites[instance.CornerSprite(3)].scaleY = -1f;
            instance.sprites[instance.CornerSprite(3)].scaleX = -1f;
            instance.sprites[instance.MainFillSprite] = new FSprite("pixel", true)
            {
                anchorY = 0f,
                anchorX = 0f
            };
            instance.sprites[instance.FillSideSprite(0)].anchorY = 0f;
            instance.sprites[instance.FillSideSprite(2)].anchorY = 0f;
            instance.sprites[instance.FillSideSprite(1)].anchorX = 0f;
            instance.sprites[instance.FillSideSprite(3)].anchorX = 0f;
            instance.sprites[instance.FillCornerSprite(0)].scaleY = -1f;
            instance.sprites[instance.FillCornerSprite(2)].scaleX = -1f;
            instance.sprites[instance.FillCornerSprite(3)].scaleY = -1f;
            instance.sprites[instance.FillCornerSprite(3)].scaleX = -1f;
            for (int j = 0; j < 9; j++)
            {
                instance.sprites[j].color = new Color(0f, 0f, 0f);
                instance.sprites[j].alpha = 0.75f;
            }
            instance.label = new FLabel("font", string.Empty)
            {
                alignment = FLabelAlignment.Left,
                anchorX = 0f,
                anchorY = 1f
            };
            for (int k = 0; k < instance.sprites.Length; k++)
            {
                instance.hud.fContainers[1].AddChild(instance.sprites[k]);
            }
            instance.hud.fContainers[1].AddChild(instance.label);
        }

        public static void DrawPatch(On.HUD.DialogBox.orig_Draw orig, DialogBox instance, float timeStacker)
        {
            if (!ComMod.fontExist)
            {
                orig.Invoke(instance, timeStacker);
                return;
            }

            //base.Draw(timeStacker); //HUDpart Draw has nothing in it
            for (int i = 0; i < instance.sprites.Length; i++)
            {
                instance.sprites[i].isVisible = (instance.CurrentMessage != null);
            }
            instance.label.isVisible = (instance.CurrentMessage != null);
            if (instance.CurrentMessage == null)
            {
                return;
            }

            //Debug.Log(string.Concat("widthMargin: " + widthMargin));
            //Debug.Log(string.Concat("heightMargin: " + heightMargin));
            //Debug.Log(string.Concat("meanCharWidth: " + meanCharWidth));
            //Debug.Log(string.Concat("lineHeight: " + lineHeight));
            //Debug.Log(string.Concat("xW: " + multiplierWidth + " / xH: " + multiplierHeight));

            Vector2 drawPos = instance.DrawPos(timeStacker); //center bottom of the screen
            Vector2 rectSize = ComCtrler.GetSize(instance.CurrentMessage.text, false);
            Vector2 curRectSize = rectSize + new Vector2(widthMargin, heightMargin);
            // = new Vector2(0f, 0f)
            // {
            //     x = widthMargin + instance.CurrentMessage.longestLine * meanCharWidth, //width of box
            //     y = heightMargin + lineHeight * (float)instance.CurrentMessage.lines //height of box
            // };

            curRectSize.x = Mathf.Lerp(40f, curRectSize.x, Mathf.Pow(Mathf.Lerp(instance.lastSizeFac, instance.sizeFac, timeStacker), 0.5f));
            curRectSize.y *= 0.5f + 0.5f * Mathf.Lerp(instance.lastSizeFac, instance.sizeFac, timeStacker);

            //Debug.Log(string.Concat("v.x: " + vector.x + " / v.y: " + vector.y));
            //Debug.Log(string.Concat("v2.x: " + vector2.x + " / v2.y: " + vector2.y));

            //slightly moved to right / downward
            float sq = 0.333333343f;
            //drawPos.x -= sq;
            //drawPos.y -= sq;

            instance.label.x = Mathf.Floor(drawPos.x - (curRectSize.x - widthMargin) * 0.5f) + 0.5f; // + ComCtrler.GetSize("A", false).y
            instance.label.y = Mathf.Floor(drawPos.y + (curRectSize.y - heightMargin) * 0.5f) + 0.5f; // - lineHeight * 1.3333f + 10f; //lineHeight * 0.6666f;
            instance.label.text = instance.showText;

            //Debug.Log(string.Concat("lbl.x: " + label.x + " / lbl.y: " + label.y));

            drawPos.x -= curRectSize.x / 2f;
            drawPos.y -= curRectSize.y / 2f;
            //top left

            //Debug.Log(string.Concat("-v.x: " + vector.x + " / -v.y: " + vector.y));

            instance.sprites[instance.SideSprite(1)].scaleX = curRectSize.x - ((12f - sq) * multiplierWidth + sq);
            instance.sprites[instance.SideSprite(3)].scaleX = curRectSize.x - ((12f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillSideSprite(1)].scaleX = curRectSize.x - ((14f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillSideSprite(3)].scaleX = curRectSize.x - ((14f - sq) * multiplierWidth + sq);
            instance.sprites[instance.MainFillSprite].scaleX = curRectSize.x - ((14f - sq) * multiplierWidth + sq);

            instance.sprites[instance.SideSprite(0)].scaleY = curRectSize.y - ((12f - sq) * multiplierHeight + sq);
            instance.sprites[instance.SideSprite(2)].scaleY = curRectSize.y - ((12f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillSideSprite(0)].scaleY = curRectSize.y - ((14f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillSideSprite(2)].scaleY = curRectSize.y - ((14f - sq) * multiplierHeight + sq);
            instance.sprites[instance.MainFillSprite].scaleY = curRectSize.y - ((14f - sq) * multiplierHeight + sq);

            instance.sprites[instance.SideSprite(0)].x = drawPos.x + ((1f - sq) * multiplierWidth + sq);
            instance.sprites[instance.SideSprite(0)].y = drawPos.y + ((6f - sq) * multiplierHeight + sq);
            instance.sprites[instance.SideSprite(1)].x = drawPos.x + ((6f - sq) * multiplierWidth + sq);
            instance.sprites[instance.SideSprite(1)].y = drawPos.y + curRectSize.y - ((1f - sq) * multiplierHeight + sq);
            instance.sprites[instance.SideSprite(2)].x = drawPos.x + curRectSize.x - ((1f - sq) * multiplierWidth + sq);
            instance.sprites[instance.SideSprite(2)].y = drawPos.y + ((6f - sq) * multiplierHeight + sq);
            instance.sprites[instance.SideSprite(3)].x = drawPos.x + ((6f - sq) * multiplierWidth + sq);
            instance.sprites[instance.SideSprite(3)].y = drawPos.y + ((1f - sq) * multiplierHeight + sq);
            instance.sprites[instance.CornerSprite(0)].x = drawPos.x + ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.CornerSprite(0)].y = drawPos.y + ((3.5f - sq) * multiplierHeight + sq);
            instance.sprites[instance.CornerSprite(1)].x = drawPos.x + ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.CornerSprite(1)].y = drawPos.y + curRectSize.y - ((3.5f - sq) * multiplierHeight + sq);
            instance.sprites[instance.CornerSprite(2)].x = drawPos.x + curRectSize.x - ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.CornerSprite(2)].y = drawPos.y + curRectSize.y - ((3.5f - sq) * multiplierHeight + sq);
            instance.sprites[instance.CornerSprite(3)].x = drawPos.x + curRectSize.x - ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.CornerSprite(3)].y = drawPos.y + ((3.5f - sq) * multiplierHeight + sq);
            Color color = new Color(1f, 1f, 1f);
            for (int j = 0; j < 4; j++)
            {
                instance.sprites[instance.SideSprite(j)].color = color;
                instance.sprites[instance.CornerSprite(j)].color = color;
            }
            instance.sprites[instance.FillSideSprite(0)].x = drawPos.x + ((4f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillSideSprite(0)].y = drawPos.y + ((7f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillSideSprite(1)].x = drawPos.x + ((7f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillSideSprite(1)].y = drawPos.y + curRectSize.y - ((4f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillSideSprite(2)].x = drawPos.x + curRectSize.x - ((4f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillSideSprite(2)].y = drawPos.y + ((7f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillSideSprite(3)].x = drawPos.x + ((7f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillSideSprite(3)].y = drawPos.y + ((4f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillCornerSprite(0)].x = drawPos.x + ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillCornerSprite(0)].y = drawPos.y + ((3.5f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillCornerSprite(1)].x = drawPos.x + ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillCornerSprite(1)].y = drawPos.y + curRectSize.y - ((3.5f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillCornerSprite(2)].x = drawPos.x + curRectSize.x - ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillCornerSprite(2)].y = drawPos.y + curRectSize.y - ((3.5f - sq) * multiplierHeight + sq);
            instance.sprites[instance.FillCornerSprite(3)].x = drawPos.x + curRectSize.x - ((3.5f - sq) * multiplierWidth + sq);
            instance.sprites[instance.FillCornerSprite(3)].y = drawPos.y + ((3.5f) * multiplierHeight + sq);

            instance.sprites[instance.MainFillSprite].x = drawPos.x + ((7f - sq) * multiplierWidth + sq);
            instance.sprites[instance.MainFillSprite].y = drawPos.y + ((7f - sq) * multiplierHeight + sq);
        }

        public static float meanCharWidth
        {
            get
            {
                if (!ComMod.fontExist) { return 6f; }
                if (CommunicationModule.ComMod.pWidth > CommunicationModule.ComMod.pHeight * 0.65f)
                {
                    return CommunicationModule.ComMod.pWidth * 0.5f;
                }
                else
                {
                    return CommunicationModule.ComMod.pWidth * 0.8f;
                }
            }
        }

        public static float heightMargin
        {
            get
            {
                return 20f;
                //if (!ComMod.fontExist) { return 20f; }
                //return Mathf.Max(20f, Mathf.Round(ComMod.pHeight * 1.3333f / ComMod.pMulti));
            }
        }

        public static float widthMargin
        {
            get
            {
                return 30f;
                //if (!ComMod.fontExist) { return 30f; }
                //return Mathf.Max(30f, Mathf.Round(meanCharWidth * 5f / ComMod.pMulti));
            }
        }

        public static float multiplierWidth => 1f; // Mathf.Max(1f, meanCharWidth / 6f);

        public static float multiplierHeight => 1f; // Mathf.Max(1f, lineHeight / 15f);

        public static void InterruptPatch(On.HUD.DialogBox.orig_Interrupt orig, DialogBox instance, string text, int extraLinger)
        {
            string t = text;
            if (t.Length > 5 && t.Substring(0, 4) == "...t") { t = instance.hud.rainWorld.inGameTranslator.Translate(t); }
            orig.Invoke(instance, t, extraLinger);
        }

        public static void MsgCtorPatch(On.HUD.DialogBox.Message.orig_ctor orig, DialogBox.Message instance, string text, float xOrientation, float yPos, int extraLinger)
        {
            instance.text = Regex.Replace(text, "<LINE>", Environment.NewLine);
            instance.xOrientation = xOrientation;
            instance.linger = (int)Mathf.Lerp((float)text.Length * 2f, 80f, 0.5f) + extraLinger;
            string[] array = Regex.Split(text, "<LINE>");
            instance.longestLine = 2;
            for (int i = 0; i < array.Length; i++)
            { //3 byte char -> 2 sizes
                int n = 0;
                char[] line = array[i].ToCharArray();
                for (int j = 0; j < line.Length; j++)
                {
                    n += 2;
                }
                instance.longestLine = Math.Max(instance.longestLine, n);
            }
            instance.lines = array.Length;
            instance.yPos = yPos + (20f + 15f * (float)instance.lines);
            if (instance.longestLine > 30)
            {
                int k = instance.longestLine - 30;
                instance.longestLine = 30 + Mathf.FloorToInt(k * 1.1f);
            }
        }
    }
}
