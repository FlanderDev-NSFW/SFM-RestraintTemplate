using System;
using ExposureUnnoticed2.Scripts.Base;
using ExposureUnnoticed2.Scripts.InGame;
using RestraintHub;

namespace RestraintTemplate
{
    /// <summary>
    /// The restraint's own save data: whether it is on and the struggle chance, as two extra keys in the game's save slot
    /// (ignored by the game without the mod). The hub saves the number shortcuts (Restraint.ShortcutSaveKey); everything
    /// else is yours. RestraintHub calls it and catches exceptions.
    /// </summary>
    internal static class Patches
    {
        public const string SaveKeyLocked = "RestraintTemplate.Locked"; // your mod's prefix; never rename once released
        public const string SaveKeyChance = "RestraintTemplate.Chance";

        // Called by RestraintHub (Restraint.OnSave / OnLoad) right after the game wrote / read a save slot (never the system file).
        internal static void Save(SaveSlot slot)
        {
            try
            {
                var c = TemplateController.Instance;
                if (c == null) return;
                slot.Save(SaveKeyLocked, c.Machine.Worn);
                slot.Save(SaveKeyChance, (double)c.Machine.Chance);
                Plugin.Logger.LogInfo("[SFM-ACCEPT] save: locked=" + c.Machine.Worn + " path=" + slot.Path);
            }
            catch (Exception e) { TemplateController.Fail("save hook", e); }
        }

        internal static void Load(SaveSlot slot)
        {
            try
            {
                var c = TemplateController.Instance;
                if (c == null) return;
                bool locked = slot.Load(SaveKeyLocked, false); // default when the save has no key (older save)
                double chance = slot.Load(SaveKeyChance, -1.0);
                c.OnSaveLoaded(locked, (float)Math.Max(chance, 0.0));
                Plugin.Logger.LogInfo("[SFM-ACCEPT] load: locked=" + locked + " path=" + slot.Path);
            }
            catch (Exception e) { TemplateController.Fail("load hook", e); }
        }
    }
}
