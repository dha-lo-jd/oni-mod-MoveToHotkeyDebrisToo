#if DEBUG
// In-game tests run by Gravitas.TestHarness:
//   python Gravitas.TestProbe/probe.py run --project <this repo> --tests '{"tags":["movetohotkeydebristoo"]}'
// Needs HarnessDllPath in Local.Build.props (Check comes from the harness assembly).
using System.Collections.Generic;
using System.Linq;
using Gravitas.TestHarness;
using HarmonyLib;
using MoveToHotkeyDebrisToo.Patches;

namespace MoveToHotkeyDebrisToo;

internal static class HarnessTests
{
    private const string Tag = "movetohotkeydebristoo";

    private static BindingEntry FindBinding(Action action) =>
        GameInputMapping.KeyBindings.First(b => b.mAction == action);

    [HarnessTest(Name = "movetohotkeydebristoo.actions_registered", Tags = Tag)]
    public static void ActionsRegistered()
    {
        Check.True(ModActions.MoveDuplicant != null && ModActions.MoveDebris != null, "actions were not registered");
        foreach (var pAction in new[] { ModActions.MoveDuplicant, ModActions.MoveDebris })
        {
            var entry = FindBinding(pAction.GetKAction());
            Check.True(entry.mRebindable, pAction.GetKAction() + " is not rebindable");
            Check.True(ModActions.IsModAction(entry.mAction), "IsModAction is false for " + pAction.GetKAction());
        }
        Check.True(!ModActions.IsModAction(Action.MouseMiddle), "vanilla action reported as a mod action");
    }

    [HarnessTest(Name = "movetohotkeydebristoo.title_strings", Tags = Tag)]
    public static void TitleStrings()
    {
        Check.Equal(STRINGS.ACTIONS.MOVE_DUPLICANT.text,
            Strings.Get("STRINGS.INPUT_BINDINGS.PLIB." + ModActions.MoveDuplicant.GetKAction().ToString().ToUpperInvariant()).String);
        Check.Equal(STRINGS.ACTIONS.MOVE_DEBRIS.text,
            Strings.Get("STRINGS.INPUT_BINDINGS.PLIB." + ModActions.MoveDebris.GetKAction().ToString().ToUpperInvariant()).String);
    }

    // Builds the buttons the way U60's Movable / MoveToLocationMonitor do and sends them through the real
    // UserMenu.AddButton; this mod's prefix must hand them our actions and restore the {Hotkey} template.
    [HarnessTest(Name = "movetohotkeydebristoo.user_menu_buttons_rebound", Tags = Tag)]
    public static void UserMenuButtonsRebound()
    {
        var go = Game.Instance.gameObject;
        var menu = Game.Instance.userMenu;
        var buttons = Traverse.Create(menu).Field("buttons").GetValue<List<KeyValuePair<KIconButtonMenu.ButtonInfo, float>>>();
        var before = buttons.Count;
        try
        {
            var move = new KIconButtonMenu.ButtonInfo("action_control", global::STRINGS.UI.USERMENUACTIONS.MOVETOLOCATION.NAME,
                () => { }, Action.NumActions, null, null, null,
                GameUtil.ReplaceHotkeyString(global::STRINGS.UI.USERMENUACTIONS.MOVETOLOCATION.TOOLTIP, Action.MoveRelocateTo));
            var pickup = new KIconButtonMenu.ButtonInfo("action_control", global::STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME,
                () => { }, Action.NumActions, null, null, null,
                GameUtil.ReplaceHotkeyString(global::STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.TOOLTIP, Action.MoveRelocateTo));
            var other = new KIconButtonMenu.ButtonInfo("action_control", "Unrelated", () => { }, Action.NumActions, null, null, null, "tip");
            menu.AddButton(go, move);
            menu.AddButton(go, pickup);
            menu.AddButton(go, other);

            Check.True(move.shortcutKey == ModActions.MoveDuplicant.GetKAction(), "Move To button not rebound: " + move.shortcutKey);
            Check.True(pickup.shortcutKey == ModActions.MoveDebris.GetKAction(), "Move debris button not rebound: " + pickup.shortcutKey);
            Check.True(other.shortcutKey == Action.NumActions, "unrelated button was changed");
            Check.True(move.tooltipText.Contains("{Hotkey}") && pickup.tooltipText.Contains("{Hotkey}"), "tooltip template not restored");

            var hotkey = GameUtil.GetHotkeyString(ModActions.MoveDuplicant.GetKAction());
            Check.True(move.GetTooltipText().Contains(hotkey), "tooltip does not show the bound key '" + hotkey + "': " + move.GetTooltipText());
            Check.True(!move.GetTooltipText().Contains("{Hotkey}"), "tooltip still has the placeholder");
        }
        finally
        {
            while (buttons.Count > before)
                buttons.RemoveAt(buttons.Count - 1);
        }
    }

    // This mod's two actions may share Mouse 2 with each other and with vanilla MouseMiddle; other actions may not.
    [HarnessTest(Name = "movetohotkeydebristoo.duplicate_check", Tags = Tag)]
    public static void DuplicateCheck()
    {
        var modEntry = FindBinding(ModActions.MoveDuplicant.GetKAction());
        modEntry.mKeyCode = KKeyCode.Mouse2;
        modEntry.mModifier = Modifier.None;
        BindingEntry result = default;
        InputBindingsScreen_GetDuplicatedBinding_Patch.Prefix("Root", modEntry, ref result);
        Check.True(result.mAction == Action.Invalid, "mod action on Mouse 2 reported a conflict with " + result.mAction);

        // The vanilla rule is kept for everything else: an existing rebindable vanilla binding still conflicts with itself.
        var vanilla = GameInputMapping.KeyBindings.First(x => x.mRebindable && !ModActions.IsModAction(x.mAction) &&
                                                              x.mKeyCode != KKeyCode.None && x.mGroup != null &&
                                                              x.mGroup != "Root" && !x.mIgnoreRootConflics);
        InputBindingsScreen_GetDuplicatedBinding_Patch.Prefix(vanilla.mGroup, vanilla, ref result);
        Check.True(result.mAction != Action.Invalid, "a vanilla binding should still conflict (" + vanilla.mAction + ")");
    }
}
#endif
