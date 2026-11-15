using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;

namespace MoveToHotkeyDebrisToo;

public sealed class MoveToHotkeyDebrisTooMod : UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        PUtil.InitLibrary();
        new PLocalization().Register();
        ModActions.Register();
    }

    // PLib applies the translation in its own Localization.Initialize postfix; this one runs after it.
    [HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
    private static class Localization_Initialize_Patch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix() => ModActions.RefreshTitles();
    }
}
