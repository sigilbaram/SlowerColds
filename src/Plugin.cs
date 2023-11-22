using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

// RENAME 'OutwardModTemplate' TO SOMETHING ELSE
namespace OutwardModTemplate
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        // Choose a GUID for your project. Change "myname" and "mymod".
        public const string GUID = "sigilbaram.slowercolds";
        // Choose a NAME for your project, generally the same as your Assembly Name.
        public const string NAME = "Slower Colds";
        // Increment the VERSION when you release a new version of your mod.
        public const string VERSION = "1.0.0";

        // For accessing your BepInEx Logger from outside of this class (eg Plugin.Log.LogMessage("");)
        internal static ManualLogSource Log;

        // If you need settings, define them like so:
        public static ConfigEntry<float> checkFrequency;
        public static ConfigEntry<float> coldChance;
        public static ConfigEntry<float> coldMinTime;
        public static ConfigEntry<float> freezingChance;
        public static ConfigEntry<float> freezingMinTime;

        // Awake is called when your plugin is created. Use this to set up your mod.
        internal void Awake()
        {
            Log = this.Logger;
            Log.LogMessage($"Hello world from {NAME} {VERSION}!");

            // Any config settings you define should be set up like this:
            // ExampleConfig = Config.Bind("ExampleCategory", "ExampleSetting", false, "This is an example setting.");
            checkFrequency = Config.Bind("Time", "Check Frequence", 0.2f, new ConfigDescription("How often to check (in game hours)", new AcceptableValueRange<float>(1f / 150f, 1f)));
            coldChance = Config.Bind("Need: Cold", "Chance", 0.085f, new ConfigDescription("Chance during Cold", new AcceptableValueRange<float>(0f, 1f)));
            coldMinTime = Config.Bind("Need: Cold", "Min Time", 1f, new ConfigDescription("Minimum time during Cold", new AcceptableValueRange<float>(1f / 150f, 1f)));
            freezingChance = Config.Bind("Need: Very Cold", "Chance", 0.17f, new ConfigDescription("Chance during Very Cold", new AcceptableValueRange<float>(0f, 1f)));
            freezingMinTime = Config.Bind("Need: Very Cold", "Min Time", 0f, new ConfigDescription("Minimum time during Very Cold", new AcceptableValueRange<float>(1f / 150f, 1f)));

            // Harmony is for patching methods. If you're not patching anything, you can comment-out or delete this line.
            new Harmony(GUID).PatchAll();
        }

        // Update is called once per frame. Use this only if needed.
        // You also have all other MonoBehaviour methods available (OnGUI, etc)
        internal void Update()
        {

        }

        // This is an example of a Harmony patch.
        // If you're not using this, you should delete it.
        [HarmonyPatch(typeof(PlayerCharacterStats), nameof(PlayerCharacterStats.UpdateDiseasesContraction))]
        public class PlayerCharacterStats_UpdateDiseasesContraction
        {
            static bool Prefix(PlayerCharacterStats __instance)
            {
                StatusEffectFamily diseaseFamily = DiseaseLibrary.Instance.GetDiseaseFamily(Diseases.Cold);
                if (__instance.m_character.StatusEffectMngr.HasStatusEffect(diseaseFamily))
                {
                    return false;
                }

                if (EnvironmentConditions.GameTimeF - __instance.m_lastColdCheckGameTime >= checkFrequency.Value && __instance.m_character.StatusEffectMngr.HasStatusEffect("ColdExposure"))
                {
                    __instance.m_lastColdCheckGameTime = EnvironmentConditions.GameTimeF;

                    StatusEffect coldExposure = __instance.m_character.StatusEffectMngr.GetStatusEffectOfName("ColdExposure");
                    
                    if (coldExposure != null && coldExposure.Age >= coldMinTime.Value && UnityEngine.Random.Range(0f, 100f) <= coldChance.Value)
                    {
                        DiseaseLibrary.Instance.AddDiseaseToCharacter(__instance.m_character, Diseases.Cold, null);
                    }
                }

                if (EnvironmentConditions.GameTimeF - __instance.m_lastColdCheckGameTime >= checkFrequency.Value && __instance.m_character.StatusEffectMngr.HasStatusEffect("Freezing"))
                {
                    __instance.m_lastColdCheckGameTime = EnvironmentConditions.GameTimeF;

                    StatusEffect freezing = __instance.m_character.StatusEffectMngr.GetStatusEffectOfName("Freezing");
                    
                    if (freezing != null && freezing.Age >= freezingMinTime.Value && UnityEngine.Random.Range(0f, 100f) <= freezingChance.Value)
                    {
                        DiseaseLibrary.Instance.AddDiseaseToCharacter(__instance.m_character, Diseases.Cold, null);
                    }
                }
                
                return false;
            }
        }
    }
}
