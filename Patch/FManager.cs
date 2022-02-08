using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CommunicationModule.FutileManager
{
    public static class FManager
    {
        public static bool fontEnabled => CommunicationModule.ComMod.fontExist;

        public static FAtlas LoadExtAtlas(string atlasName)
        {
            //atlasName + Futile.resourceSuffix + "_png";
            string path = string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Atlas", Path.DirectorySeparatorChar, atlasName, ".png");

            Texture2D texture2D = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            byte[] fileData = File.ReadAllBytes(path);
            texture2D.LoadImage(fileData);

            return Futile.atlasManager.LoadAtlasFromTexture(atlasName, texture2D);
        }

        public static void LoadExtFont(string name, string atlasName, string configPath, float offsetX, float offsetY)
        {
            LoadExtFont(name, atlasName, configPath, offsetX, offsetY, new FTextParams());
        }

#pragma warning disable IDE0060

        public static void LoadExtFont(string name, string atlasName, string configPath, float offsetX, float offsetY, FTextParams textParams)
#pragma warning restore IDE0060
        {
            FAtlasElement element0 = Futile.atlasManager.GetElementWithName(string.Concat(name + "_" + ((int)0).ToString(digitext)));
            FFont ffont = new FFont(name, element0, configPath, offsetX, offsetY, textParams);
            Futile.atlasManager._fonts.Add(ffont);
            Futile.atlasManager._fontsByName.Add(name, ffont);
        }

        public static void LoadExtAtlasData(ref FAtlas atlas)
        {
            //TextAsset textAsset = Resources.Load(this._dataPath, typeof(TextAsset)) as TextAsset;
            string txt = File.ReadAllText(string.Concat(ComMod.path, Path.DirectorySeparatorChar, "Atlas", Path.DirectorySeparatorChar, atlas.name, ".txt"));

            Dictionary<string, object> dictionary = txt.dictionaryFromJson();
            if (dictionary == null)
            {
                throw new FutileException($"The atlas of {atlas.name} was not a proper JSON file. Make sure to select \"Unity3D\" in TexturePacker.");
            }
            Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary["frames"];
            float resourceScaleInverse = Futile.resourceScaleInverse;
            int num = 0;
            foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
            {
                FAtlasElement fatlasElement = new FAtlasElement { indexInAtlas = num++ };
                string text = keyValuePair.Key;
                if (Futile.shouldRemoveAtlasElementFileExtensions)
                {
                    int num2 = text.LastIndexOf(".");
                    if (num2 >= 0) { text = text.Substring(0, num2); }
                }
                fatlasElement.name = text;
                if (digit == 0)
                {
                    digit = fatlasElement.name.Split('_')[1].Length;
                }
                IDictionary dictionary3 = (IDictionary)keyValuePair.Value;
                fatlasElement.isTrimmed = (bool)dictionary3["trimmed"];
                if ((bool)dictionary3["rotated"])
                {
                    throw new NotSupportedException("Futile no longer supports TexturePacker's \"rotated\" flag. Please disable it when creating the " + atlas._dataPath + " atlas.");
                }
                IDictionary dictionary4 = (IDictionary)dictionary3["frame"];
                float num3 = float.Parse(dictionary4["x"].ToString());
                float num4 = float.Parse(dictionary4["y"].ToString());
                float num5 = float.Parse(dictionary4["w"].ToString());
                float num6 = float.Parse(dictionary4["h"].ToString());
                Rect uvRect = new Rect(num3 / atlas._textureSize.x, (atlas._textureSize.y - num4 - num6) / atlas._textureSize.y, num5 / atlas._textureSize.x, num6 / atlas._textureSize.y);
                fatlasElement.uvRect = uvRect;
                fatlasElement.uvTopLeft.Set(uvRect.xMin, uvRect.yMax);
                fatlasElement.uvTopRight.Set(uvRect.xMax, uvRect.yMax);
                fatlasElement.uvBottomRight.Set(uvRect.xMax, uvRect.yMin);
                fatlasElement.uvBottomLeft.Set(uvRect.xMin, uvRect.yMin);
                IDictionary dictionary5 = (IDictionary)dictionary3["sourceSize"];
                fatlasElement.sourcePixelSize.x = float.Parse(dictionary5["w"].ToString());
                fatlasElement.sourcePixelSize.y = float.Parse(dictionary5["h"].ToString());
                fatlasElement.sourceSize.x = fatlasElement.sourcePixelSize.x * resourceScaleInverse;
                fatlasElement.sourceSize.y = fatlasElement.sourcePixelSize.y * resourceScaleInverse;
                IDictionary dictionary6 = (IDictionary)dictionary3["spriteSourceSize"];
                float left = float.Parse(dictionary6["x"].ToString()) * resourceScaleInverse;
                float top = float.Parse(dictionary6["y"].ToString()) * resourceScaleInverse;
                float width = float.Parse(dictionary6["w"].ToString()) * resourceScaleInverse;
                float height = float.Parse(dictionary6["h"].ToString()) * resourceScaleInverse;
                fatlasElement.sourceRect = new Rect(left, top, width, height);
                atlas._elements.Add(fatlasElement);
                atlas._elementsByName.Add(fatlasElement.name, fatlasElement);

                fatlasElement.atlas = atlas;

                if (fatlasElement.name.Substring(fatlasElement.name.Length - digit) == ((int)0).ToString(digitext))
                { Futile.atlasManager.AddElement(fatlasElement); }
            }
        }

        public static int digit = 0;
        public static string digitext { get { return $"D{digit}"; } }

        public static FAtlasElement callElementByName(FAtlas atlas, string name)
        {
            if (atlas._elementsByName.ContainsKey(name))
            { return atlas._elementsByName[name]; }
            throw new FutileException($"Couldn't find element named '{name}'");
        }
    }
}
