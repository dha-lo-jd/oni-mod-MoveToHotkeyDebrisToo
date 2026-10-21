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
}
