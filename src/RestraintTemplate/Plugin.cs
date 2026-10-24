using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using RestraintHub;

namespace RestraintTemplate
{
    /// <summary>
    /// A complete, minimal restraint built on RestraintHub - copy this mod to start your own (steps in README.md). It has no
    /// model and poses nothing; it shows the parts every restraint needs:
    /// <list type="bullet">
    /// <item>registering with the hub (this file): ring entry, list, conflicts, input blocking, F and + / - routing;</item>
    /// <item>the lock and struggling free, a value + / - change, putting it on (<see cref="TemplateController"/>);</item>
    /// <item>saving its own state with the game (<see cref="Patches"/>).</item>
    /// </list>
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // Hard dependency: BepInEx loads the hub first and refuses to load this mod without it. Add a minimum version when you use
    // newer hub API: [BepInDependency(Hub.Guid, "0.2.0")].
    [BepInDependency(Hub.Guid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class Plugin : BasePlugin
    {
        public const string PluginGuid = "com.local.restrainttemplate"; // yours: "com.<you>.<mod>"
        public const string PluginName = "RestraintTemplate";
        public const string PluginVersion = "0.1.0";

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

            Patches.Apply(new HarmonyLib.Harmony(PluginGuid));
            ClassInjector.RegisterTypeInIl2Cpp<TemplateController>(); // injected types need a unique full name: rename the namespace
            AddComponent<TemplateController>();

            Entry = new Restraint
            {
                Id = "RestraintTemplate",            // stable; orders the ring entries
                RingValue = 9900,                    // unique among restraint mods - see the registry in mods/RestraintHub/README.md
                Label = () => RingLabel.Value,
                ShortcutSaveKey = "RestraintTemplate.Shortcuts", // never rename once released (players' number keys)
                Holds = BodyParts.None,              // e.g. BodyParts.Neck for a collar: refused while something else holds the neck
                IsWorn = () => TemplateController.Instance != null && TemplateController.Instance.Machine.Worn,
                Blocks = () => RestraintHub.Blocks.Running,
                OnChosen = () => { var c = TemplateController.Instance; if (c != null) c.PutOn(); },
                Attempt = () => TemplateController.Instance == null ? 0f : TemplateController.Instance.Machine.Attempt,
                Controls = "+/- tightness",
            };
            Hub.Register(Entry);
            Logger.LogInfo("[SFM-ACCEPT] load: " + PluginName + " " + PluginVersion + " ready");
        }
    }
}
