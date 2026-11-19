namespace MoveToHotkeyDebrisToo;

// Not readonly: the game replaces these fields by reflection when it applies a translation. A readonly static can be
// inlined by the JIT, which then reads the replaced (collected) object and returns garbage.
public static class STRINGS
{
    public static class ACTIONS
    {
        public static LocString MOVE_DUPLICANT = "Move To (Duplicant)";
        public static LocString MOVE_DEBRIS = "Move To (Debris)";
    }
}
