using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;

namespace FOVFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        public static ConfigEntry<float> globalOpticFOVMulti { get; set; }

        public static ConfigEntry<float> globalCameraPOSMulti { get; set; }

        public static ConfigEntry<float> globalADSMulti { get; set; }
        public static ConfigEntry<float> oneADSMulti { get; set; }
        public static ConfigEntry<float> twoADSMulti { get; set; }
        public static ConfigEntry<float> threeADSMulti { get; set; }
        public static ConfigEntry<float> fourADSMulti { get; set; }
        public static ConfigEntry<float> sixADSMulti { get; set; }
        public static ConfigEntry<float> twelveADSMulti { get; set; }
        public static ConfigEntry<float> highADSMulti { get; set; }



        private void Awake()
        {
            string scopeFOV = "1. Scope Zoom";
            string adsFOV = "2. ADS FOV";
            string cameraPostiion = "3. ADS Camera Position";

            globalOpticFOVMulti = Config.Bind<float>(scopeFOV, "Global Optic Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 1.25f), new ConfigurationManagerAttributes { Order = 8 }));

            globalCameraPOSMulti = Config.Bind<float>(cameraPostiion, "Global Distance to Optic Multi", 1f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Lower = closer to Optic. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 8 }));

            globalADSMulti = Config.Bind<float>(adsFOV, "Global ADS FOV Multi", 1f, new ConfigDescription("Applies on top of all other ADS FOV Change Multies. Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 8 }));
            oneADSMulti = Config.Bind<float>(adsFOV, "1x ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.41f, 1.3f), new ConfigurationManagerAttributes { Order = 7 }));
            twoADSMulti = Config.Bind<float>(adsFOV, "2x ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 6 }));
            threeADSMulti = Config.Bind<float>(adsFOV, "3x ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 5 }));
            fourADSMulti = Config.Bind<float>(adsFOV, "4x ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 4 }));
            sixADSMulti = Config.Bind<float>(adsFOV, "6x ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
            twelveADSMulti = Config.Bind<float>(adsFOV, "12x ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 2 }));
            highADSMulti = Config.Bind<float>(adsFOV, "High Magnification ADS FOV Multi", 1f, new ConfigDescription("Multiplier for the FOV Change when ADSing. Lower Multi = lower FOV so more Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 1 }));

            new GetAnyOpticsDistanceToCameraPatch().Enable();
            new CalcDistancePatch().Enable();
            new OpticSightAwakePatch().Enable();
            new method_17Patch().Enable();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

    }
}
