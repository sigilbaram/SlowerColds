using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SlowerColds
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "sigilbaram.slowercolds";
        public const string NAME = "Slower Colds";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;

        public static ConfigEntry<int> coldFrequency;
        public static ConfigEntry<int> coldMinTime;
        public static ConfigEntry<float> coldChance;
        public static ConfigEntry<int> freezingFrequency;
        public static ConfigEntry<int> freezingMinTime;
        public static ConfigEntry<float> freezingChance;

        internal void Awake()
        {
            Log = this.Logger;

            coldChance = Config.Bind("Need: Cold", "Chance", 9f, new ConfigDescription("Chance during Cold", new AcceptableValueRange<float>(0f, 100f)));
            coldMinTime = Config.Bind("Need: Cold", "Min Time", 30, new ConfigDescription("Minimum time during Cold (sec)", new AcceptableValueRange<int>(0, 150)));
            coldFrequency = Config.Bind("Need: Cold", "Check Frequence", 30, new ConfigDescription("How often to check during Cold (sec)", new AcceptableValueRange<int>(1, 150)));

            freezingChance = Config.Bind("Need: Very Cold", "Chance", 18f, new ConfigDescription("Chance during Very Cold", new AcceptableValueRange<float>(0f, 100f)));
            freezingMinTime = Config.Bind("Need: Very Cold", "Min Time", 0, new ConfigDescription("Minimum time during Very Cold (sec)", new AcceptableValueRange<int>(0, 150)));
            freezingFrequency = Config.Bind("Need: Very Cold", "Check Frequence", 15, new ConfigDescription("How often to check during Very Cold (sec)", new AcceptableValueRange<int>(1, 150)));

            new Harmony(GUID).PatchAll();
        }

        [HarmonyPatch(typeof(PlayerCharacterStats), nameof(PlayerCharacterStats.UpdateDiseasesContraction))]
        public class PlayerCharacterStats_UpdateDiseasesContraction
        {
            static bool Prefix(PlayerCharacterStats __instance)
            {
                Character m_character = __instance.m_character;

                StatusEffectFamily diseaseFamily = DiseaseLibrary.Instance.GetDiseaseFamily(Diseases.Cold);
                if (m_character.StatusEffectMngr.HasStatusEffect(diseaseFamily))
                {
                    return false;
                }

                if (m_character.StatusEffectMngr.HasStatusEffect("ColdExposure"))
                {
                    StatusEffect coldExposure = m_character.StatusEffectMngr.GetStatusEffectOfName("ColdExposure");
                    if (coldExposure != null && coldExposure.Age >= coldMinTime.Value)
                    {
                        if (__instance.m_lastColdCheckGameTime > coldExposure.Age)
                        {
                            __instance.m_lastColdCheckGameTime = 0f;
                        }

                        bool firstCheck = __instance.m_lastColdCheckGameTime <= 0f && coldExposure.Age < coldFrequency.Value + coldMinTime.Value;
                        bool nextCheck = coldExposure.Age > __instance.m_lastColdCheckGameTime + coldFrequency.Value;
                        if ( firstCheck || nextCheck )
                        {
                            __instance.m_lastColdCheckGameTime = coldExposure.Age;
                            if (UnityEngine.Random.Range(0f, 100f) <= coldChance.Value)
                            {
                                DiseaseLibrary.Instance.AddDiseaseToCharacter(m_character, Diseases.Cold, null);
                                __instance.m_lastColdCheckGameTime = 0f;
                            }
                        }
                    }
                }
                else if (m_character.StatusEffectMngr.HasStatusEffect("Freezing"))
                {
                    StatusEffect freezing = m_character.StatusEffectMngr.GetStatusEffectOfName("Freezing");
                    if (freezing != null && freezing.Age >= freezingMinTime.Value)
                    {
                        if (__instance.m_lastColdCheckGameTime > freezing.Age)
                        {
                            __instance.m_lastColdCheckGameTime = 0f;
                        }

                        bool firstCheck = __instance.m_lastColdCheckGameTime <= 0f && freezing.Age < freezingFrequency.Value + freezingMinTime.Value;
                        bool nextCheck = freezing.Age > __instance.m_lastColdCheckGameTime + freezingFrequency.Value;
                        if (firstCheck || nextCheck)
                        {
                            __instance.m_lastColdCheckGameTime = freezing.Age;
                            if (UnityEngine.Random.Range(0f, 100f) <= freezingChance.Value)
                            {
                                DiseaseLibrary.Instance.AddDiseaseToCharacter(m_character, Diseases.Cold, null);
                                __instance.m_lastColdCheckGameTime = 0f;
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
