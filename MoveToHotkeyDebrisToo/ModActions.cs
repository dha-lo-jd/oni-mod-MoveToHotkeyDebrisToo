using HarmonyLib;
using PeterHan.PLib.Actions;
using UnityEngine;

namespace MoveToHotkeyDebrisToo;

public static class ModActions
{
    public static PAction MoveDuplicant;
    public static PAction MoveDebris;

    public static void Register()
    {
        var manager = new PActionManager();
        MoveDuplicant = manager.CreateAction("MoveToHotkeyDebrisToo.MoveDuplicant",
            STRINGS.ACTIONS.MOVE_DUPLICANT, new PKeyBinding(KKeyCode.Mouse2));
        MoveDebris = manager.CreateAction("MoveToHotkeyDebrisToo.MoveDebris",
            STRINGS.ACTIONS.MOVE_DEBRIS, new PKeyBinding(KKeyCode.Mouse2));
    }
}

[HarmonyPatch(typeof(UserMenu), nameof(UserMenu.AddButton))]
public static class UserMenu_AddButton_Patch
{
    public static void Prefix(KIconButtonMenu.ButtonInfo button)
    {
        if (button == null || button.shortcutKey != Action.NumActions)
            return;

        PAction action;
        if (button.text == global::STRINGS.UI.USERMENUACTIONS.MOVETOLOCATION.NAME)
            action = ModActions.MoveDuplicant;
        else if (button.text == global::STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME)
            action = ModActions.MoveDebris;
        else
            return;

        if (action == null)
            return;
        button.shortcutKey = action.GetKAction();
        button.text = GameUtil.ReplaceHotkeyString(button.text, button.shortcutKey);
    }
}

[HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetKeycodeLocalized))]
public static class GameUtil_GetKeycodeLocalized_Patch
{
    public static bool Prefix(KKeyCode key_code, ref string __result)
    {
        if (key_code != KKeyCode.Mouse2)
            return true;
        __result = STRINGS.INPUT_BINDINGS.ROOT.MOUSE2;
        return false;
    }
}

[HarmonyPatch(typeof(InputBindingsScreen), "Update")]
public static class InputBindingsScreen_Update_Patch
{
    private static bool Down(KeyCode a, KeyCode b = KeyCode.None) =>
        Input.GetKey(a) || Input.GetKeyDown(a) || (b != KeyCode.None && (Input.GetKey(b) || Input.GetKeyDown(b)));

    private static float suppressUntil;

    // Adds Esc (unassign) and middle mouse button support while waiting for a key press.
    public static bool Prefix(InputBindingsScreen __instance)
    {
        var traverse = Traverse.Create(__instance);
        var waiting = traverse.Field("waitingForKeyPress").GetValue<bool>();

        // Releasing the wheel button clicks the row button, which would re-enter the waiting state.
        if (Input.GetMouseButton(2) && Time.unscaledTime < suppressUntil)
            suppressUntil = Time.unscaledTime + 0.3f;
        if (waiting && Time.unscaledTime < suppressUntil && !Input.GetKeyDown(KeyCode.Mouse2))
        {
            traverse.Field("waitingForKeyPress").SetValue(false);
            traverse.Field("actionToRebind").SetValue(Action.NumActions);
            traverse.Field("activeButton").SetValue(null);
            traverse.Method("BuildDisplay").GetValue();
            return false;
        }

        if (!waiting)
            return true;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var action = traverse.Field("actionToRebind").GetValue<Action>();
            traverse.Method("Unbind", action).GetValue();
            Global.GetInputManager().RebindControls();
            traverse.Field("waitingForKeyPress").SetValue(false);
            traverse.Field("actionToRebind").SetValue(Action.NumActions);
            traverse.Field("activeButton").SetValue(null);
            traverse.Method("BuildDisplay").GetValue();
            return false;
        }

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            var modifier = Modifier.None;
            if (Down(KeyCode.LeftAlt, KeyCode.RightAlt)) modifier |= Modifier.Alt;
            if (Down(KeyCode.LeftControl, KeyCode.RightControl)) modifier |= Modifier.Ctrl;
            if (Down(KeyCode.LeftShift, KeyCode.RightShift)) modifier |= Modifier.Shift;
            if (Down(KeyCode.CapsLock)) modifier |= Modifier.CapsLock;
            if (Down(KeyCode.BackQuote)) modifier |= Modifier.Backtick;
            suppressUntil = Time.unscaledTime + 0.3f;
            var action = traverse.Field("actionToRebind").GetValue<Action>();
            if (ModBindingUtil.IsModAction(action))
            {
                var bindings = GameInputMapping.KeyBindings;
                for (var i = 0; i < bindings.Length; i++)
                {
                    var e = bindings[i];
                    if (e.mRebindable && e.mAction == action)
                    {
                        e.mKeyCode = KKeyCode.Mouse2;
                        e.mModifier = modifier;
                        bindings[i] = e;
                        break;
                    }
                }
                Global.GetInputManager().RebindControls();
                traverse.Field("waitingForKeyPress").SetValue(false);
                traverse.Field("actionToRebind").SetValue(Action.NumActions);
                traverse.Field("activeButton").SetValue(null);
                traverse.Method("BuildDisplay").GetValue();
            }
            else
            {
                traverse.Method("Bind", new[] { typeof(KKeyCode), typeof(Modifier) },
                    new object[] { KKeyCode.Mouse2, modifier }).GetValue();
            }
            return false;
        }

        return true;
    }
}

public static class ModBindingUtil
{
    public static bool IsModAction(Action action) =>
        ModActions.MoveDuplicant != null && ModActions.MoveDebris != null &&
        (action == ModActions.MoveDuplicant.GetKAction() || action == ModActions.MoveDebris.GetKAction());
}

[HarmonyPatch(typeof(InputBindingsScreen), "GetDuplicatedBinding")]
public static class InputBindingsScreen_GetDuplicatedBinding_Patch
{
    // Same logic as the game, but the two Move To actions of this mod may share a key.
    public static bool Prefix(string activeScreen, BindingEntry new_binding, ref BindingEntry __result)
    {
        __result = default;
        var bindings = GameInputMapping.KeyBindings;
        for (var i = 0; i < bindings.Length; i++)
        {
            var e = bindings[i];
            if (ModBindingUtil.IsModAction(new_binding.mAction) && ModBindingUtil.IsModAction(e.mAction))
                continue;
            if (ModBindingUtil.IsModAction(new_binding.mAction) &&
                new_binding.mKeyCode == KKeyCode.Mouse2 && e.mAction == Action.MouseMiddle)
                continue;
            if (new_binding.IsBindingEqual(e) &&
                (e.mGroup == null || e.mGroup == activeScreen || e.mGroup == "Root" || activeScreen == "Root") &&
                (activeScreen != "Root" || !e.mIgnoreRootConflics) &&
                (e.mGroup != "Root" || !new_binding.mIgnoreRootConflics))
            {
                __result = e;
                break;
            }
        }
        return false;
    }
}

[HarmonyPatch(typeof(MainMenu), "CheckDoubleBoundKeys")]
public static class MainMenu_CheckDoubleBoundKeys_Patch
{
    // Hide this mod's bindings from the game's duplicate key check while it runs.
    public static void Prefix(out BindingEntry[] __state)
    {
        var bindings = GameInputMapping.KeyBindings;
        __state = (BindingEntry[])bindings.Clone();
        for (var i = 0; i < bindings.Length; i++)
        {
            if (!ModBindingUtil.IsModAction(bindings[i].mAction))
                continue;
            var e = bindings[i];
            e.mRebindable = false;
            bindings[i] = e;
        }
    }

    public static void Postfix(BindingEntry[] __state)
    {
        var bindings = GameInputMapping.KeyBindings;
        for (var i = 0; i < bindings.Length && i < __state.Length; i++)
        {
            if (ModBindingUtil.IsModAction(bindings[i].mAction))
                bindings[i] = __state[i];
        }
    }
}
