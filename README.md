# RestraintTemplate

A complete, minimal restraint on **RestraintHub**, meant to be copied. It has no model and poses nothing; it shows what
every restraint needs and nothing else:

| File | What it shows |
|---|---|
| `Plugin.cs` | config, the hard dependency on the hub, registering a `Restraint` (ring entry, list text, body parts, blocks, callbacks) |
| `TemplateController.cs` | the lock and struggling free with SfmKit's `LockMachine` (F via `Entry.StruggleHeld`), a value changed with + / - (`Entry.TakeStep()`), putting it on (`OnChosen`), dropping it on a new game |
| `Patches.cs` | saving its own state (on / off, struggle chance) in the game's save slot; the hub saves the number shortcuts |

In game it appears as **Template Restraint** on the restraint page of the ring, shows in the list over the head preview when
on, takes away running, `+` / `-` change `General.Tightness` (1-10), holding **F** frees her. It is not deployed by default.

## Make your own from it

1. Copy the folder: `mods/RestraintTemplate` -> `mods/<YourMod>`; rename `RestraintTemplate.csproj` to `<YourMod>.csproj` and
   set `AssemblyName` / `RootNamespace` in it. (Outside this repo see "Building outside this repo" below.)
2. Rename the namespace `RestraintTemplate` in all files - injected MonoBehaviours need a unique full type name.
3. In `Plugin.cs`: `PluginGuid` (`com.<you>.<mod>`), `PluginName`, the ring label, and in the `Restraint`:
   - `Id` - stable, unique; the ring entries are ordered by it;
   - `RingValue` - a free value from the registry in `mods/RestraintHub/README.md` (9000-9998), and add yours there;
   - `ShortcutSaveKey` and the keys in `Patches.cs` - your mod's prefix; never rename them after a release;
   - `Holds` - the body parts it holds (conflicts), `Blocks` - what she may not do while it is on, `Controls` - a few words
     for + / - (keep label + controls under ~30 characters; longer rows are shrunk to fit).
4. Replace the tightness example with what + / - should do (or set `Controls = null` and ignore `TakeStep()`).
5. Add the visible part: a model placed in `Camera.onPreCull`, bones posed in `LateUpdate` (see the note at the end of
   `TemplateController.cs`; `mods/Yoke` is a complete example: model export, sizing to her body, arm pose).
6. `./tools/dev.ps1 build <YourMod>`, then `./tools/dev.ps1 cycle <YourMod> --allow-invoke` (deploys the hub along) and test
   (below). `./tools/dev.ps1 package <YourMod>` makes a zip with your mod and the hub.

## Test it (bridge)
- Log: `[SFM-ACCEPT] register: <Id>` (hub), `ring: N restraint entries from index ...`.
- Put it on through the real ring: hold `1`, `X` to the restraint page, select it, **click** (releasing the ring key instead
  assigns it to that number key). Diagnostics: `game call RestraintTemplate.TemplateController PutOnNow`,
  `game get RestraintTemplate.TemplateController::Status`, `game get RestraintHub.HubController::Status`.
- `key Equals --via input --expect "tightness:"`, `game keydown F` ... `wait-log "release: off"` ... `game keyup F`.
- Negative controls: with another restraint selected (`key B --via input`), + / - and F must not reach yours.

## Building outside this repo
Reference the installed hub instead of the project, without copying it, and declare the dependency:

```xml
<Reference Include="RestraintHub">
  <HintPath>$(GameDir)\BepInEx\plugins\RestraintHub\RestraintHub.dll</HintPath>
  <Private>false</Private>
</Reference>
```

`RestraintHub.xml` next to the DLL gives your IDE the API documentation. Ship your DLL in `BepInEx/plugins/<YourMod>/` and tell
players to install RestraintHub (or include its folder in your zip).

## Verification
- L0: builds with the repo (`./tools/dev.ps1 build`).
- L3 0.1.0 (2026-10-24, dev copy, first save; deployed for the test and removed again): "register: RestraintTemplate",
  "ring: 4 restraint entries from index 32"; chosen in the ring (page 5, first slot, click) -> Locked and selected, the list
  row "> Template Restraint (F, +/- tightness)" over the head preview (screenshot); `=` -> "tightness: 6"; a pose skill was not
  refused (it blocks only running); F held -> "release: off", list empty, key 1 kept its shortcut. Not checked in game: the
  save round trip (the same code as Yoke's, which is L3).
