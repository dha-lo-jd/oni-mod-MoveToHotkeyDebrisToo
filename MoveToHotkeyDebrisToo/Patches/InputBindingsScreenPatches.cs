using HarmonyLib;
using UnityEngine;

namespace MoveToHotkeyDebrisToo.Patches;

/// <summary>Thin wrapper over the private state of the game's key configuration screen.</summary>
internal readonly struct RebindScreen
{
    private readonly Traverse screen;

    public RebindScreen(InputBindingsScreen instance) => screen = Traverse.Create(instance);

    public bool IsWaitingForKey => screen.Field("waitingForKeyPress").GetValue<bool>();
    public Action ActionToRebind => screen.Field("actionToRebind").GetValue<Action>();

    /// <summary>Leaves the "press a key" state and redraws the screen.</summary>
    public void EndWaiting()
    {
        screen.Field("waitingForKeyPress").SetValue(false);
        screen.Field("actionToRebind").SetValue(Action.NumActions);
        screen.Field("activeButton").SetValue(null);
        screen.Method("BuildDisplay").GetValue();
    }

    public void Unbind(Action action) => screen.Method("Unbind", action).GetValue();

    /// <summary>The game's own binding routine (including its duplicate check and dialog).</summary>
    public void Bind(KKeyCode key, Modifier modifier) =>
        screen.Method("Bind", new[] { typeof(KKeyCode), typeof(Modifier) }, new object[] { key, modifier })
            .GetValue();
}

/// <summary>
/// Adds middle mouse button (Mouse 2) and Esc (unassign) support while the key configuration screen
/// waits for a key press. The game does not list Mouse 2 among its valid keys.
/// </summary>
[HarmonyPatch(typeof(InputBindingsScreen), "Update")]
public static class InputBindingsScreen_Update_Patch
{
    private const float ClickSuppressSeconds = 0.3f;

    private static float suppressUntil;

    public static bool Prefix(InputBindingsScreen __instance)
    {
        var screen = new RebindScreen(__instance);
        var now = Time.unscaledTime;

        // Releasing the wheel button clicks the row button, which would re-enter the waiting state.
        // Keep cancelling that while the button is held and shortly after.
        if (Input.GetMouseButton(2) && now < suppressUntil)
            suppressUntil = now + ClickSuppressSeconds;
        if (screen.IsWaitingForKey && now < suppressUntil && !Input.GetKeyDown(KeyCode.Mouse2))
        {
            screen.EndWaiting();
            return false;
        }

        if (!screen.IsWaitingForKey)
            return true;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            screen.Unbind(screen.ActionToRebind);
            Global.GetInputManager().RebindControls();
            screen.EndWaiting();
            return false;
        }

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            suppressUntil = now + ClickSuppressSeconds;
            var modifier = ReadModifier();
            var action = screen.ActionToRebind;
            if (ModActions.IsModAction(action))
            {
                SetBinding(action, KKeyCode.Mouse2, modifier);
                Global.GetInputManager().RebindControls();
                screen.EndWaiting();
            }
            else
            {
                screen.Bind(KKeyCode.Mouse2, modifier);
            }
            return false;
        }

        return true;
    }

    /// <summary>Updates the key of an existing rebindable entry in place, keeping its group and button.</summary>
    private static void SetBinding(Action action, KKeyCode key, Modifier modifier)
    {
        var bindings = GameInputMapping.KeyBindings;
        for (var i = 0; i < bindings.Length; i++)
        {
            var entry = bindings[i];
            if (!entry.mRebindable || entry.mAction != action)
                continue;
            entry.mKeyCode = key;
            entry.mModifier = modifier;
            bindings[i] = entry;
            return;
        }
    }

    private static Modifier ReadModifier()
    {
        var modifier = Modifier.None;
        if (IsHeld(KeyCode.LeftAlt) || IsHeld(KeyCode.RightAlt)) modifier |= Modifier.Alt;
        if (IsHeld(KeyCode.LeftControl) || IsHeld(KeyCode.RightControl)) modifier |= Modifier.Ctrl;
        if (IsHeld(KeyCode.LeftShift) || IsHeld(KeyCode.RightShift)) modifier |= Modifier.Shift;
        if (IsHeld(KeyCode.CapsLock)) modifier |= Modifier.CapsLock;
        if (IsHeld(KeyCode.BackQuote)) modifier |= Modifier.Backtick;
        return modifier;
    }

    private static bool IsHeld(KeyCode key) => Input.GetKey(key) || Input.GetKeyDown(key);
}

/// <summary>
/// Same logic as the game, except that this mod's two actions may share a key with each other and may
/// use Mouse 2 even though the vanilla <see cref="Action.MouseMiddle"/> action is bound to it.
/// </summary>
[HarmonyPatch(typeof(InputBindingsScreen), "GetDuplicatedBinding")]
public static class InputBindingsScreen_GetDuplicatedBinding_Patch
{
    public static bool Prefix(string activeScreen, BindingEntry new_binding, ref BindingEntry __result)
    {
        __result = default;
        var newIsMod = ModActions.IsModAction(new_binding.mAction);
        var bindings = GameInputMapping.KeyBindings;
        for (var i = 0; i < bindings.Length; i++)
        {
            var existing = bindings[i];
            if (newIsMod && ModActions.IsModAction(existing.mAction))
                continue;
            if (newIsMod && new_binding.mKeyCode == KKeyCode.Mouse2 && existing.mAction == Action.MouseMiddle)
                continue;
            if (IsConflict(activeScreen, new_binding, existing))
            {
                __result = existing;
                break;
            }
        }
        return false;
    }

    // Vanilla conflict rule.
    private static bool IsConflict(string activeScreen, BindingEntry newBinding, BindingEntry existing) =>
        newBinding.IsBindingEqual(existing) &&
        (existing.mGroup == null || existing.mGroup == activeScreen || existing.mGroup == "Root" ||
         activeScreen == "Root") &&
        (activeScreen != "Root" || !existing.mIgnoreRootConflics) &&
        (existing.mGroup != "Root" || !newBinding.mIgnoreRootConflics);
}
