using HarmonyLib;

namespace MoveToHotkeyDebrisToo.Patches;

[HarmonyPatch(typeof(MainMenu), "CheckDoubleBoundKeys")]
public static class MainMenu_CheckDoubleBoundKeys_Patch
{
    /// <summary>
    /// Hides this mod's bindings from the game's duplicate key check (which would unassign one of them)
    /// by temporarily marking them as not rebindable. The original entries are restored afterwards.
    /// </summary>
    public static void Prefix(out BindingEntry[] __state)
    {
        var bindings = GameInputMapping.KeyBindings;
        __state = (BindingEntry[])bindings.Clone();
        for (var i = 0; i < bindings.Length; i++)
        {
            if (!ModActions.IsModAction(bindings[i].mAction))
                continue;
            var entry = bindings[i];
            entry.mRebindable = false;
            bindings[i] = entry;
        }
    }

    public static void Postfix(BindingEntry[] __state)
    {
        var bindings = GameInputMapping.KeyBindings;
        for (var i = 0; i < bindings.Length && i < __state.Length; i++)
        {
            if (ModActions.IsModAction(bindings[i].mAction))
                bindings[i] = __state[i];
        }
    }
}
