# RestraintTemplate 1.0.0

For modders: a complete, minimal restraint built on **RestraintHub**, to copy when you write your own. It has no model and
poses nothing. It shows what every restraint needs: registering a ring entry, putting it on, struggling free with `F`,
changing a value with `+` / `-`, and saving its state.

Not meant for players: in game it adds a "Template Restraint" that only blocks running.

## Files
- `Plugin.cs`: settings, the dependency on the hub, the `Restraint` registration.
- `TemplateController.cs`: putting it on, struggling free, `+` / `-`.
- `Patches.cs`: saving and loading its state through the hub.

## Make your own
1. Copy the folder and rename the project, namespace, plugin GUID (`<You>.SFM.<Mod>`), `Id`, `RingValue` (a free value
   9000-9998) and save keys.
2. Replace the example value with what `+` / `-` should do, and add your model and pose.
3. Reference the installed `BepInEx/plugins/RestraintHub/RestraintHub.dll` (not copied), and list RestraintHub as a dependency
   of your release.

The full guide, the hub's API and the list of taken ring values: `docs/mods/RestraintHub.md` and
`docs/mods/RestraintTemplate.md` in the source repository.

Code: MIT licence (`LICENSE.txt`).
