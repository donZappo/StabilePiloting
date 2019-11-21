using System;
using System.Collections;
using System.Reflection;
using Harmony;
using Newtonsoft.Json;
using static StabilePiloting.Logger;
using BattleTech;

namespace StabilePiloting
{
    public static class Core
    {
        #region Init

        public static void Init(string modDir, string settings)
        {
            var harmony = HarmonyInstance.Create("StabilePiloting");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // read settings
            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(settings);
                Settings.modDirectory = modDir;
            }
            catch (Exception)
            {
                Settings = new ModSettings();
            }

            // blank the logfile
            Clear();
            // PrintObjectFields(Settings, "Settings");
        }
        // logs out all the settings and their values at runtime
        internal static void PrintObjectFields(object obj, string name)
        {
            LogDebug($"[START {name}]");

            var settingsFields = typeof(ModSettings)
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in settingsFields)
            {
                if (field.GetValue(obj) is IEnumerable &&
                    !(field.GetValue(obj) is string))
                {
                    LogDebug(field.Name);
                    foreach (var item in (IEnumerable)field.GetValue(obj))
                    {
                        LogDebug("\t" + item);
                    }
                }
                else
                {
                    LogDebug($"{field.Name,-30}: {field.GetValue(obj)}");
                }
            }

            LogDebug($"[END {name}]");
        }

        #endregion

        internal static ModSettings Settings;
    }

    [HarmonyPatch(typeof(Mech), "AddInstability", null)]
    public static class Mech_AddInstability
    {
        private static void Prefix(Mech __instance, ref float amt)
        {
            if (amt > 0f)
            {
                float num2 = (float)__instance.pilot.Piloting;
                num2 *= Core.Settings.floatPerPilotSkill;
                amt *= 1f - num2;
            }
        }
    }
}

