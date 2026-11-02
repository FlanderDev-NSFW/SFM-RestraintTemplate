using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using RestraintHub;

namespace RestraintTemplate
{
    /// <summary>
    /// A complete, minimal restraint built on RestraintHub - copy this mod to start your own (steps in docs/mods/RestraintTemplate.md). It has no
    /// model and poses nothing; it shows the parts every restraint needs:
    /// <list type="bullet">
    /// <item>registering with the hub (this file): ring entry, list, conflicts, input blocking, F and + / - routing;</item>
    /// <item>the lock and struggling free, a value + / - change, putting it on (<see cref="TemplateController"/>);</item>
    /// <item>saving its own state with the game (<see cref="Patches"/>).</item>
    /// </list>
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // Hard dependency with a minimum version: BepInEx loads the hub first and refuses to load this mod without it or with an
    // older one (0.3.0: OnSave / OnLoad / OnNewGame, OnTap, HeldStep, Hub.MenuOpen / InInteractArea; 1.1.0: PutOn, TakeOff,
    // Settings for missions).
    [BepInDependency(Hub.Guid, "^1.1.0")]
    public sealed class Plugin : BasePlugin
    {
        public const string PluginGuid = "FlanderDev.SFM.RestraintTemplate"; // yours: "<You>.SFM.<Mod>"
        public const string PluginName = "RestraintTemplate";
        public const string PluginVersion = "1.1.0";

        internal static ManualLogSource Logger;
        /// <summary>This restraint's registration; the controller reads <see cref="Restraint.StruggleHeld"/> and calls
        /// <see cref="Restraint.TakeStep"/> on it.</summary>
        internal static Restraint Entry;
        public static ConfigEntry<string> RingLabel;
        public static ConfigEntry<float> AttemptSeconds, SuccessChance;
        public static ConfigEntry<int> Tightness;

        public override void Load()
        {
            Logger = Log;
            RingLabel = Config.Bind("General", "RingLabel", "Template Restraint", "Name in the ring menu and the restraint list.");
            AttemptSeconds = Config.Bind("Unlock", "AttemptSeconds", 4.5f, new ConfigDescription(
                "Seconds of holding F per attempt to struggle free.", new AcceptableValueRange<float>(0.5f, 30f)));
            SuccessChance = Config.Bind("Unlock", "SuccessChance", 1f, new ConfigDescription(
                "Chance that an attempt frees her (1 = the first attempt; failures add 0.02, like the game's handcuffs).",
                new AcceptableValueRange<float>(0.01f, 1f)));
            Tightness = Config.Bind("General", "Tightness", 5, new ConfigDescription(
                "Example of a value + / - change while the restraint is selected (stored here).", new AcceptableValueRange<int>(1, 10)));

            ClassInjector.RegisterTypeInIl2Cpp<TemplateController>(); // injected types need a unique full name: rename the namespace
            AddComponent<TemplateController>();

            Entry = new Restraint
            {
                Id = "RestraintTemplate",            // stable; orders the ring entries
                RingValue = 9900,                    // unique among restraint mods - see the registry in docs/mods/RestraintHub.md
                Label = () => RingLabel.Value,
                ShortcutSaveKey = "RestraintTemplate.Shortcuts", // never rename once released (players' number keys)
                OnSave = Patches.Save,
                OnLoad = Patches.Load,
                OnNewGame = () => TemplateController.Instance?.OnNewGame(),
                Holds = BodyParts.None,              // e.g. BodyParts.Neck for a collar: refused while something else holds the neck
                IsWorn = () => TemplateController.Instance != null && TemplateController.Instance.Machine.Worn,
                Blocks = () => RestraintHub.Blocks.Running,
                OnChosen = () => { var c = TemplateController.Instance; if (c != null) c.OnRingChosen(); },
                Attempt = () => TemplateController.Instance == null ? 0f : TemplateController.Instance.Machine.Attempt,
                Controls = "+/- tightness",
                // For Custom Missions v2 scripts (RtHub_Apply / RtHub_Remove / RtHub_Get / RtHub_Set) and the escape key:
                PutOn = () => TemplateController.Instance == null ? "not ready" : TemplateController.Instance.PutOn(),
                TakeOff = () => TemplateController.Instance?.TakeOff(),
                Settings = new[]
                {
                    new RestraintSetting
                    {
                        Name = "tightness", Unit = "1-10",
                        Get = () => TemplateController.Tightness,
                        Set = TemplateController.SetTightness,
                    },
                },
            };
            Hub.Register(Entry);
            Logger.LogInfo("[SFM-ACCEPT] load: " + PluginName + " " + PluginVersion + " ready");
        }
    }
}
