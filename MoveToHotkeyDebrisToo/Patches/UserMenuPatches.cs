using HarmonyLib;
using PeterHan.PLib.Actions;

namespace MoveToHotkeyDebrisToo.Patches;

/// <summary>
/// Since U60 the vanilla "Move To" buttons are hard-wired to <see cref="Action.BuildMenuKeyQ"/> and
/// have no entry on the key configuration screen. Hand the buttons over to this mod's own actions so
/// that they can be rebound (to the wheel click, for example).
/// </summary>
[HarmonyPatch(typeof(UserMenu), nameof(UserMenu.AddButton))]
public static class UserMenu_AddButton_Patch
{
    public static void Prefix(KIconButtonMenu.ButtonInfo button)
    {
        if (button == null || button.shortcutKey != Action.NumActions)
            return;

        if (button.text == global::STRINGS.UI.USERMENUACTIONS.MOVETOLOCATION.NAME)
            Assign(button, ModActions.MoveDuplicant, global::STRINGS.UI.USERMENUACTIONS.MOVETOLOCATION.TOOLTIP);
        else if (button.text == global::STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME)
            Assign(button, ModActions.MoveDebris, global::STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.TOOLTIP);
    }

    private static void Assign(KIconButtonMenu.ButtonInfo button, PAction action, string tooltipTemplate)
    {
        if (action == null)
            return;
        button.shortcutKey = action.GetKAction();
        // The game bakes "Q" into the tooltip when it creates the button. Restore the template so that
        // ButtonInfo.GetTooltipText() substitutes the key actually bound to our action.
        button.tooltipText = tooltipTemplate;
    }
}
