using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SlowerColds
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "sigilbaram.slowercolds";
        public const string NAME = "Slower Colds";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;

        public static ConfigEntry<float> checkFrequency;
        public static ConfigEntry<float> coldChance;
        public static ConfigEntry<float> coldMinTime;
        public static ConfigEntry<float> freezingChance;
        public static ConfigEntry<float> freezingMinTime;

        internal void Awake()
        {
            checkFrequency = Config.Bind("Time", "Check Frequence", 0.2f, new ConfigDescription("How often to check (in game hours)", new AcceptableValueRange<float>(1f / 150f, 1f)));
            coldChance = Config.Bind("Need: Cold", "Chance", 0.085f, new ConfigDescription("Chance during Cold", new AcceptableValueRange<float>(0f, 1f)));
            coldMinTime = Config.Bind("Need: Cold", "Min Time", 1f, new ConfigDescription("Minimum time during Cold", new AcceptableValueRange<float>(1f / 150f, 1f)));
            freezingChance = Config.Bind("Need: Very Cold", "Chance", 0.17f, new ConfigDescription("Chance during Very Cold", new AcceptableValueRange<float>(0f, 1f)));
            freezingMinTime = Config.Bind("Need: Very Cold", "Min Time", 0f, new ConfigDescription("Minimum time during Very Cold", new AcceptableValueRange<float>(1f / 150f, 1f)));

            new Harmony(GUID).PatchAll();
        }

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
