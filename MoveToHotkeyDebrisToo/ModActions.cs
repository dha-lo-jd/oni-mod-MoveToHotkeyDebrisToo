using HarmonyLib;
using System.Reflection;
using PeterHan.PLib.Actions;

namespace MoveToHotkeyDebrisToo;

/// <summary>
/// The two hotkey actions this mod adds to the game's key configuration screen.
/// </summary>
public static class ModActions
{
    public static PAction MoveDuplicant { get; private set; }
    public static PAction MoveDebris { get; private set; }

    // The LocString objects PLib holds as the action titles. A translation replaces the STRINGS fields with new objects,
    // so these old ones would keep the English text. Typed as object on purpose: the game scans every class in this
    // namespace for LocString fields when it registers the strings, and would trip over a LocString field that is still null.
    private static object duplicantTitle;
    private static object debrisTitle;

    private static readonly FieldInfo TextField = AccessTools.Field(typeof(LocString), "_text");

    public static void Register()
    {
        duplicantTitle = STRINGS.ACTIONS.MOVE_DUPLICANT;
        debrisTitle = STRINGS.ACTIONS.MOVE_DEBRIS;
        var manager = new PActionManager();
        MoveDuplicant = manager.CreateAction("MoveToHotkeyDebrisToo.MoveDuplicant",
            STRINGS.ACTIONS.MOVE_DUPLICANT, new PKeyBinding(KKeyCode.Mouse2));
        MoveDebris = manager.CreateAction("MoveToHotkeyDebrisToo.MoveDebris",
            STRINGS.ACTIONS.MOVE_DEBRIS, new PKeyBinding(KKeyCode.Mouse2));
    }

    /// <summary>Copies the translated action titles into what PLib shows on the key bindings screen.</summary>
    public static void RefreshTitles()
    {
        Refresh(duplicantTitle, STRINGS.ACTIONS.MOVE_DUPLICANT, MoveDuplicant);
        Refresh(debrisTitle, STRINGS.ACTIONS.MOVE_DEBRIS, MoveDebris);
    }

    private static void Refresh(object heldObject, LocString current, PAction action)
    {
        var held = heldObject as LocString;
        if (held == null || current == null || action == null)
            return;
        if (!ReferenceEquals(held, current) && TextField != null)
            TextField.SetValue(held, current.text);
        Strings.Add("STRINGS.INPUT_BINDINGS.PLIB." + action.GetKAction().ToString().ToUpperInvariant(), current.text);
    }

    /// <summary>True if <paramref name="action"/> is one of this mod's two actions.</summary>
    public static bool IsModAction(Action action) =>
        MoveDuplicant != null && MoveDebris != null &&
        (action == MoveDuplicant.GetKAction() || action == MoveDebris.GetKAction());
}
