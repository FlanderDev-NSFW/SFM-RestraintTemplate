using System;
using ExposureUnnoticed2.Object3D.Player.Scripts;
using ExposureUnnoticed2.Scripts.InGame;
using ExposureUnnoticed2.Scripts.Sound;
using Il2CppInterop.Runtime.Attributes;
using RestraintHub;
using SfmKit.Game;
using SfmKit.Pure;
using UnityEngine;

namespace RestraintTemplate
{
    /// <summary>
    /// The restraint's state, ticked every frame. Bridge diagnostics (session started with <c>--allow-invoke</c>):
    /// <c>game call RestraintTemplate.TemplateController PutOnNow</c>, <c>game get RestraintTemplate.TemplateController::Status</c>.
    /// </summary>
    public sealed class TemplateController : MonoBehaviour
    {
        internal static TemplateController Instance;
        public TemplateController(IntPtr pointer) : base(pointer) { } // required for injected types

        /// <summary>Lock / struggle rules like the game's handcuffs (SfmKit, unit-tested).</summary>
        internal readonly LockMachine Machine = new LockMachine();

        /// <summary>Live state for diagnostics.</summary>
        public static string Status = "off";

        private static int _failures;

        internal static void Fail(string where, Exception e)
        {
            if (_failures++ < 20) Plugin.Logger.LogWarning(where + " failed: " + e);
        }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Unity messages run every frame: never let an exception escape (it would repeat every frame).
        private void Update()
        {
            try { Tick(); }
            catch (Exception e) { Fail("Update", e); }
        }

        [HideFromIl2Cpp]
        private void Tick()
        {
            if (PlayerController.Instance == null || GameState.GameStateData == null) return; // title screen, loading

            // + / -: the hub hands out steps only while this restraint is the selected one.
            int step = Plugin.Entry.TakeStep();
            if (step != 0 && Machine.State == LockState.Locked)
            {
                int now = Tightness;
                int want = Math.Clamp(now + step, 1, 10);
                if (want == now) Hub.Message(step > 0 ? "It is as tight as it goes." : "It is as loose as it goes.");
                Plugin.Tightness.Value = want; // writes the config: only on the player's action
                MissionTightness = 0;
                Plugin.Logger.LogInfo("[SFM-ACCEPT] tightness: " + want);
            }

            // F: StruggleHeld is true only while this restraint is selected, outside menus and interaction areas.
            Machine.AttemptSeconds = Plugin.AttemptSeconds.Value; // live config
            Machine.BaseChance = Plugin.SuccessChance.Value;
            bool struggling = Plugin.Entry.StruggleHeld && Machine.State == LockState.Locked;
            switch (Machine.Tick(Time.deltaTime, struggling, UnityEngine.Random.value))
            {
                case LockMachine.Event.Locked:
                    GameUi.PlaySe(InGameSoundManager.SeType.AttachHandcuffs);
                    Plugin.Logger.LogInfo("[SFM-ACCEPT] lock: locked");
                    break;
                case LockMachine.Event.AttemptFailed:
                    Plugin.Logger.LogInfo("[SFM-ACCEPT] attempt: failed");
                    break;
                case LockMachine.Event.Unlocking:
                    GameUi.PlaySe(InGameSoundManager.SeType.Unlock);
                    Plugin.Logger.LogInfo("[SFM-ACCEPT] attempt: success");
                    break;
                case LockMachine.Event.Unlocked:
                    Plugin.Logger.LogInfo("[SFM-ACCEPT] release: off");
                    break;
            }
            Status = Machine.State + ", attempt " + Machine.Attempt.ToString("0.00") + ", selected " + Plugin.Entry.Selected + ", tightness " + Tightness;
        }


        /// <summary>Put it on - for the panel, and for missions (Wearable.PutOn, RestraintHub 1.1.0): null when it is closing,
        /// else why not in a few words. Check whatever your restraint needs here.</summary>
        [HideFromIl2Cpp]
        internal string PutOn()
        {
            if (!Machine.RequestLock()) return "it is already on";
            GameUi.PlaySe(InGameSoundManager.SeType.CommonEquip);
            Plugin.Logger.LogInfo("[SFM-ACCEPT] ring: put on, closing");
            return null;
        }

        /// <summary>Take it off at once, without a struggle (Wearable.TakeOff: a mission, its timer, or the player's escape key).
        /// Undo everything the restraint does to her here.</summary>
        [HideFromIl2Cpp]
        internal void TakeOff()
        {
            if (Machine.Worn) Plugin.Logger.LogInfo("[SFM-ACCEPT] release: taken off");
            Machine.Reset();
            MissionTightness = 0;
        }

        /// <summary>A tightness set by a mission (Wearable.Settings "tightness"), used instead of the config until + / - or the
        /// restraint comes off; 0 = none. Missions never write the player's config.</summary>
        internal static int MissionTightness;

        internal static int Tightness => MissionTightness > 0 ? MissionTightness : Plugin.Tightness.Value;

        /// <summary>Wearable.Settings "tightness": clamped to 1..10; null when done, else why not.</summary>
        internal static string SetTightness(float value)
        {
            if (Instance == null || !Instance.Machine.Worn) return "it is not on";
            MissionTightness = Math.Clamp((int)Math.Round(value), 1, 10);
            Plugin.Logger.LogInfo("[SFM-ACCEPT] tightness: " + MissionTightness + " (mission)");
            return null;
        }

        /// <summary>Diagnostics: put it on as the panel would (skips the hub's conflict check).</summary>
        public static string PutOnNow()
        {
            if (Instance == null) return "no controller";
            return Instance.PutOn() ?? Instance.Machine.State.ToString();
        }

        // --- save identity: a load restores the lock (Patches); a new game or the title drops it -----------------------------

        [HideFromIl2Cpp]
        internal void OnSaveLoaded(bool locked, float chance)
        {
            Machine.Restore(locked, chance);
        }

        [HideFromIl2Cpp]
        internal void OnNewGame()
        {
            if (Machine.Worn) Plugin.Logger.LogInfo("game data replaced without a load: off");
            Machine.Reset();
            MissionTightness = 0;
        }

        // --- where a real restraint does its visible work ----------------------------------------------------------------------
        // Pose bones in LateUpdate (after the animator, before skinning) and place the model in Camera.onPreCull (the game still
        // moves bones after LateUpdate) - see docs/knowledge/patterns.md "Bend the body on top of any animation" and Yoke's
        // YokePoser / YokeRig for a full example. Remember: with blocking switched off another restraint may pose the same bones.
    }
}
