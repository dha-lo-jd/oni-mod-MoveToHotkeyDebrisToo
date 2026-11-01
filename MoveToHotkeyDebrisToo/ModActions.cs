using PeterHan.PLib.Actions;

namespace MoveToHotkeyDebrisToo;

/// <summary>
/// The two hotkey actions this mod adds to the game's key configuration screen.
/// </summary>
public static class ModActions
{
    public static PAction MoveDuplicant { get; private set; }
    public static PAction MoveDebris { get; private set; }

    public static void Register()
    {
        var manager = new PActionManager();
        MoveDuplicant = manager.CreateAction("MoveToHotkeyDebrisToo.MoveDuplicant",
            STRINGS.ACTIONS.MOVE_DUPLICANT, new PKeyBinding(KKeyCode.Mouse2));
        MoveDebris = manager.CreateAction("MoveToHotkeyDebrisToo.MoveDebris",
            STRINGS.ACTIONS.MOVE_DEBRIS, new PKeyBinding(KKeyCode.Mouse2));
    }

    /// <summary>True if <paramref name="action"/> is one of this mod's two actions.</summary>
    public static bool IsModAction(Action action) =>
        MoveDuplicant != null && MoveDebris != null &&
        (action == MoveDuplicant.GetKAction() || action == MoveDebris.GetKAction());
}
