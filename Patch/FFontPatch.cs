using CommunicationModule.FutileManager;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CommunicationModule.Patch
{
    public static class FFontPatch
    {
        public static void Patch()
        {
            On.FFont.LoadAndParseConfigFile += new On.FFont.hook_LoadAndParseConfigFile(LoadAndParseConfigFilePatch);
        }

        public static string digitext { get { return FManager.digitext; } }

        public static void LoadAndParseConfigFilePatch(On.FFont.orig_LoadAndParseConfigFile orig, FFont instance)
        {
            if (instance.element == null)
            {
                LoadAndParseExtConfigFile(ref instance);
                return;
            }

            try
            {
                TextAsset textAsset = (TextAsset)Resources.Load(instance._configPath, typeof(TextAsset));
                orig.Invoke(instance);
            }
            catch
            {
                LoadAndParseExtConfigFile(ref instance);
                return;
            }
        }

        public static void LoadAndParseExtConfigFile(ref FFont instance)
        {
            string path = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Atlas", Path.DirectorySeparatorChar, instance.name, ".txt");
            string txt = File.ReadAllText(path);

            if (txt == null) { throw new FutileException("Couldn't find font config file at " + txt); }

            string[] array = new string[] { "\n" };
            string[] array2 = txt.Split(array, StringSplitOptions.RemoveEmptyEntries);
            if (array2.Length <= 1)
            {
                array[0] = "\r\n";
                array2 = txt.Split(array, StringSplitOptions.RemoveEmptyEntries);
            }
            if (array2.Length <= 1)
            {
                array[0] = "\r";
                array2 = txt.Split(array, StringSplitOptions.RemoveEmptyEntries);
            }
            if (array2.Length <= 1) { throw new FutileException("Your font file is messed up"); }
            int num = 0;
            int num2 = 0;

            instance._charInfosByID = new Dictionary<uint, FCharInfo>(127);
            FCharInfo fcharInfo = new FCharInfo();
            instance._charInfosByID[0u] = fcharInfo;
            float resourceScaleInverse = Futile.resourceScaleInverse;
            Vector2 textureSize = instance._element.atlas.textureSize;
            Debug.Log("texture width " + textureSize.x);

            FAtlas atlas = instance._element.atlas;

            bool flag = false;
            int num3 = array2.Length;

            //calculate avgWidth/Height
            int sumWidth = 0;
            float avgWidth;
            int height = 15;

            for (int i = 0; i < num3; i++)
            {
                string text = array2[i];
                string[] array3 = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (array3[0] == "info")
                {
                    string[] arrayt = array2[i].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    height = int.Parse(arrayt[2].Split(new char[] { ' ' })[0]);
                }
                if (array3[0] == "common")
                {
                    instance._configWidth = int.Parse(array3[3].Split(new char[] { '=' })[1]);
                    instance._configRatio = instance._element.sourcePixelSize.x / (float)instance._configWidth;
                    instance._lineHeight = (float)int.Parse(array3[1].Split(new char[] { '=' })[1]) * instance._configRatio * resourceScaleInverse;
                }
                else if (array3[0] == "chars")
                {
                    int num4 = int.Parse(array3[1].Split(new char[] { '=' })[1]);
                    instance._charInfos = new FCharInfo[num4 + 1];
                }
                else if (array3[0] == "char")
                {
                    FCharInfo fcharInfo2 = new FCharInfo();
                    int num5 = array3.Length;
                    for (int j = 1; j < num5; j++)
                    {
                        string[] array4 = array3[j].Split(new char[] { '=' });
                        string text2 = array4[0];
                        if (text2 == "letter")
                        {
                            if (array4[1].Length >= 3)
                            { fcharInfo2.letter = array4[1].Substring(1, 1); }
                        }
                        else if (!(text2 == "\r"))
                        {
                            int num6 = int.Parse(array4[1]);
                            float num7 = (float)num6;
                            if (text2 == "id")
                            { fcharInfo2.charID = num6; }
                            else if (text2 == "x")
                            { fcharInfo2.x = num7 * instance._configRatio - instance._element.sourceRect.x * Futile.resourceScale; }
                            else if (text2 == "y")
                            { fcharInfo2.y = num7 * instance._configRatio - instance._element.sourceRect.y * Futile.resourceScale; }
                            else if (text2 == "width")
                            { fcharInfo2.width = num7 * instance._configRatio; sumWidth += num6; }
                            else if (text2 == "height")
                            { fcharInfo2.height = num7 * instance._configRatio; }
                            else if (text2 == "xoffset")
                            { fcharInfo2.offsetX = num7 * instance._configRatio; }
                            else if (text2 == "yoffset")
                            { fcharInfo2.offsetY = num7 * instance._configRatio; }
                            else if (text2 == "xadvance")
                            { fcharInfo2.xadvance = num7 * instance._configRatio; }
                            else if (text2 == "page")
                            {
                                fcharInfo2.page = num6;

                                instance._element = FManager.callElementByName(atlas, string.Concat(instance.name + "_" + num6.ToString(digitext)));
                            }
                        }
                    }
                    Rect uvRect = new Rect(instance._element.uvRect.x + fcharInfo2.x / textureSize.x, (textureSize.y - fcharInfo2.y - fcharInfo2.height) / textureSize.y - (1f - instance._element.uvRect.yMax), fcharInfo2.width / textureSize.x, fcharInfo2.height / textureSize.y);
                    fcharInfo2.uvRect = uvRect;
                    fcharInfo2.uvTopLeft.Set(uvRect.xMin, uvRect.yMax);
                    fcharInfo2.uvTopRight.Set(uvRect.xMax, uvRect.yMax);
                    fcharInfo2.uvBottomRight.Set(uvRect.xMax, uvRect.yMin);
                    fcharInfo2.uvBottomLeft.Set(uvRect.xMin, uvRect.yMin);
                    fcharInfo2.width *= resourceScaleInverse;
                    fcharInfo2.height *= resourceScaleInverse;
                    fcharInfo2.offsetX *= resourceScaleInverse;
                    fcharInfo2.offsetY *= resourceScaleInverse;
                    fcharInfo2.xadvance *= resourceScaleInverse;
                    instance._charInfosByID[(uint)fcharInfo2.charID] = fcharInfo2;
                    instance._charInfos[num] = fcharInfo2;
                    num++;
                }
                else if (array3[0] == "kernings")
                {
                    flag = true;
                    int num8 = int.Parse(array3[1].Split(new char[] { '=' })[1]);
                    instance._kerningInfos = new FKerningInfo[num8 + 100];
                }
                else if (array3[0] == "kerning")
                {
                    FKerningInfo fkerningInfo = new FKerningInfo { first = -1 };
                    int num5 = array3.Length;
                    for (int k = 1; k < num5; k++)
                    {
                        string[] array5 = array3[k].Split(new char[] { '=' });
                        if (array5.Length >= 2)
                        {
                            string text3 = array5[0];
                            int num9 = int.Parse(array5[1]);
                            if (text3 == "first")
                            { fkerningInfo.first = num9; }
                            else if (text3 == "second")
                            { fkerningInfo.second = num9; }
                            else if (text3 == "amount")
                            { fkerningInfo.amount = (float)num9 * instance._configRatio * resourceScaleInverse; }
                        }
                    }
                    if (fkerningInfo.first != -1)
                    { instance._kerningInfos[num2] = fkerningInfo; }
                    num2++;
                }
            }
            instance._kerningCount = num2;
            if (!flag) { instance._kerningInfos = new FKerningInfo[0]; }
            if (instance._charInfosByID.ContainsKey(32u))
            {
                instance._charInfosByID[32u].offsetX = 0f;
                instance._charInfosByID[32u].offsetY = 0f;
            }

            avgWidth = sumWidth / num * instance._configRatio;

            float fontScale;
            if (instance.name == "pFont")
            {
                ComMod.pWidth = Mathf.Floor(avgWidth * 100f) / 100f;
                ComMod.pHeight = Mathf.Floor(height * 100f) / 100f;
                ComMod.pMulti = Custom.IntClamp(Mathf.RoundToInt(ComMod.pHeight / 15f), 1, 2);
                fontScale = Mathf.Clamp01(1f / ComMod.pMulti + 0.05f);
                Debug.Log($"pFont avg width: {avgWidth:N2} height: {ComMod.pHeight:N2} mul: {ComMod.pMulti}");
            }
            else
            {
                ComMod.pdWidth = Mathf.Floor(avgWidth * 100f) / 100f;
                ComMod.pdHeight = Mathf.Floor(height * 100f) / 100f;
                ComMod.pdMulti = Custom.IntClamp(Mathf.RoundToInt(ComMod.pdHeight / 30f), 1, 2);
                fontScale = 1f / ComMod.pdMulti;
                Debug.Log($"pdFont avg width: {avgWidth:N2} height: {ComMod.pdHeight:N2} mul: {ComMod.pdMulti}");
            }

            foreach (KeyValuePair<uint, FCharInfo> p in instance._charInfosByID)
            {
                if (p.Value != null)
                {
                    p.Value.width *= fontScale;
                    p.Value.height *= fontScale;
                    p.Value.offsetX *= fontScale;
                    p.Value.offsetY *= fontScale;
                    p.Value.xadvance *= fontScale;
                }
            }
            /*
            for (int i = 0; i < instance._charInfos.Length; i++)
            {
                if (instance._charInfos[i] != null)
                {
                    instance._charInfos[i].width *= fontScale;
                    instance._charInfos[i].height *= fontScale;
                    instance._charInfos[i].offsetX *= fontScale;
                    instance._charInfos[i].offsetY *= fontScale;
                    instance._charInfos[i].xadvance *= fontScale;
                }
            } */
        }
    }
}
