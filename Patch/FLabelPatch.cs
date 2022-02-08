using System.Text.RegularExpressions;

namespace CommunicationModule.Patch
{
    public static class FLabelPatch
    {
        public static void Patch()
        {
            //On.FLabel.CreateTextQuads += CreateTextQuadsPatch;
            On.FLabel.ctor_1 += new On.FLabel.hook_ctor_1(CtorPatch);
        }

        public static bool HasNonASCIIChars(string str) => (System.Text.Encoding.UTF8.GetByteCount(str) != str.Length);

        /*
        private static void CreateTextQuadsPatch(On.FLabel.orig_CreateTextQuads orig, FLabel instance)
        {
            bool disp;
            if (instance.text.Length > 0)
            {
                switch (instance._fontName)
                {
                    case "pDisFont":
                        disp = true;
                        if (!HasNonASCIIChars(instance.text)) { ChangeToPFont(); }
                        break;

                    case "DisplayFont":
                    case "jpnDisplayFont":
                        disp = true;
                        if (HasNonASCIIChars(instance.text)) { ChangeToVFont(); }
                        break;

                    case "font":
                    case "jpnFont":
                    default:
                        disp = false;
                        if (HasNonASCIIChars(instance.text)) { ChangeToVFont(); }
                        break;

                    case "pFont":
                        disp = false;
                        if (!HasNonASCIIChars(instance.text)) { ChangeToPFont(); }
                        break;
                }
            }

            void ChangeToPFont()
            {
                instance._fontName = disp ? "pDisFont" : "pFont";
                instance._font = Futile.atlasManager.GetFontWithName(instance._fontName);
                instance.Init(FFacetType.Quad, instance._font.element, 0);
            }

            void ChangeToVFont()
            {
                instance._fontName = disp ? "DisplayFont" : "font";
                instance._font = Futile.atlasManager.GetFontWithName(instance._fontName);
                instance.Init(FFacetType.Quad, instance._font.element, 0);
            }

            orig(instance);
        }
        */

        public static void CtorPatch(On.FLabel.orig_ctor_1 orig, FLabel instance, string fontName, string text, FTextParams textParams)
        {
            orig.Invoke(instance, fontName, text, textParams);

            string cleanText = Regex.Replace(text, @"\s+", "");
            if (cleanText.Length > 0 && !HasNonASCIIChars(cleanText) && ComMod.DataEnabled) { return; } // Only ASCII => No changing font

            if (ComMod.fontExist)
            {
                switch (fontName)
                {
                    case "DisplayFont":
                    case "jpnDisplayFont":
                        instance._fontName = "pDisFont";
                        break;

                    case "font":
                    case "jpnFont":
                    default:
                        instance._fontName = "pFont";
                        break;
                }
            }
            else
            {
                instance._fontName = fontName;
            }

            instance._text = text;
            instance._font = Futile.atlasManager.GetFontWithName(instance._fontName);
            instance._textParams = textParams;

            instance.Init(FFacetType.Quad, instance._font.element, 0);

            instance.CreateTextQuads();
        }
    }
}
