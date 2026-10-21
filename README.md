# Move To Hotkey Debris Too

An Oxygen Not Included mod that adds two "Move To" actions to the game's hotkey settings screen:

- Move To (Duplicant)
- Move To (Debris)

## Features

- Both hotkeys can be assigned from the in-game hotkey settings screen (via PLib).
- The mouse wheel click (Mouse 2) can be assigned in the hotkey settings screen.
- Pressing Esc while waiting for a key press sets the binding to None.
- The mod's two hotkeys may share the same key. Neither the duplicate check in the settings screen nor the one on the title screen unassigns the other.

## Notes

- Both actions are bound to the wheel click (Mouse 2) by default.
- Mouse 2 normally conflicts with the vanilla `MouseMiddle` action, but this mod's actions are exempt, so they are not unassigned.

## Dependencies

- [PLib](https://github.com/peterhaneve/ONIMods) (merged into the mod DLL with ILRepack)
- Harmony

## Building

Set the game's `Managed` folder in `Local.Build.props`, then build with Rider or the .NET SDK 8.0.
After a build, the DLL is copied to `mods\local` under `ModInstallPath`. The DLL is locked while the game is running, so close the game before building.
