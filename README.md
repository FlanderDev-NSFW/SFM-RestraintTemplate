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

The full guide, the hub's API and the list of taken ring values:
[SFM-RestraintHub/docs/RestraintHub.md](https://github.com/FlanderDev-NSFW/SFM-RestraintHub/blob/main/docs/RestraintHub.md);
more on this template: [docs/RestraintTemplate.md](docs/RestraintTemplate.md). On GitHub, "Use this template" starts your own repository from it.

Code: MIT licence (`LICENSE.txt`).

## Building from source
- .NET SDK 8, and a copy of the game with BepInEx 6 IL2CPP started once: copy `Directory.Build.local.props.example` to
  `Directory.Build.local.props` and set `GameDir`. Then `dotnet build src/RestraintTemplate/RestraintTemplate.csproj -c Release`.
- It compiles against RestraintHub: the [SFM-RestraintHub](https://github.com/FlanderDev-NSFW/SFM-RestraintHub) repository next to this one, `lib/RestraintHub/RestraintHub.dll`, or the hub installed in the game.
- The source of the mod is `src/RestraintTemplate`; `lib/SfmKit` is a copy of the shared [SfmKit](https://github.com/FlanderDev-NSFW/SfmKit)
  library; `docs/` holds the developer notes.
- Every push to `main` builds, tests and releases it (`.github/workflows/release.yml`, `build/ci.py`).
