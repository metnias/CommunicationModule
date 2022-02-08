using Menu;

namespace CommunicationModule.Patch
{
    public static class SandboxEditorSelectorPatch
    {
        public static void Patch()
        {
            On.Menu.SandboxEditorSelector.Update += new On.Menu.SandboxEditorSelector.hook_Update(UpdatePatch);
        }

        public static void UpdatePatch(On.Menu.SandboxEditorSelector.orig_Update orig, SandboxEditorSelector instance)
        {
            orig.Invoke(instance);

            if (instance.editor.performanceWarning > 0)
            {
                if (instance.editor.performanceWarning == 1)
                {
                    instance.infoLabel.text = instance.menu.Translate("Warning, too many creatures may result in poor game performance.");
                }
                else if (instance.editor.performanceWarning == 2)
                {
                    instance.infoLabel.text = instance.menu.Translate("WARNING! Too many creatures may result in bad game performance or crashes.");
                }
            }
        }
    }
}
