using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;

namespace FOVFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        public static HashSet<int> opticIdHash = new HashSet<int>();
        public static bool isAiming;


        public static ConfigEntry<bool> trueOneX { get; set; }

        public static ConfigEntry<float> globalOpticFOVMulti { get; set; }
        /*        public static ConfigEntry<float> oneOpticFOVMulti { get; set; }
                public static ConfigEntry<float> twoOpticFOVMulti { get; set; }
                public static ConfigEntry<float> threeOpticFOVMulti { get; set; }
                public static ConfigEntry<float> fourOpticFOVMulti { get; set; }
                public static ConfigEntry<float> sixOpticFOVMulti { get; set; }
                public static ConfigEntry<float> twelveOpticFOVMulti { get; set; }
                public static ConfigEntry<float> highOpticFOVMulti { get; set; }*/

        public static ConfigEntry<float> globalCameraPOSMulti { get; set; }
        /*        public static ConfigEntry<float> oneCameraPOSMulti { get; set; }
                public static ConfigEntry<float> twoCameraPOSMulti { get; set; }
                public static ConfigEntry<float> threeCameraPOSMulti { get; set; }
                public static ConfigEntry<float> fourCameraPOSMulti { get; set; }
                public static ConfigEntry<float> sixCameraPOSMulti { get; set; }
                public static ConfigEntry<float> twelveCameraPOSMulti { get; set; }
                public static ConfigEntry<float> highCameraPOSMulti { get; set; }*/



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

            trueOneX = Config.Bind<bool>(scopeFOV, "True 1x Magnification", true, new ConfigDescription("1x Scopes will override `Global Optic Zoom Multi` and stay at a true 1x Magnification. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", null, new ConfigurationManagerAttributes { Order = 9 }));
            globalOpticFOVMulti = Config.Bind<float>(scopeFOV, "Global Optic Zoom Multi", 0.65f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 1.25f), new ConfigurationManagerAttributes { Order = 8 }));
            /*oneOpticFOVMulti = Config.Bind<float>(scopeFOV, "1x Optic FOV/Zoom Multi", 1f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 7 }));
            twoOpticFOVMulti = Config.Bind<float>(scopeFOV, "2x Optic FOV/Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 6 }));
            threeOpticFOVMulti = Config.Bind<float>(scopeFOV, "3x Optic FOV/Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 5 }));
            fourOpticFOVMulti = Config.Bind<float>(scopeFOV, "4x Optic FOV/Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 4 }));
            sixOpticFOVMulti = Config.Bind<float>(scopeFOV, "6x Optic FOV/Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 3 }));
            twelveOpticFOVMulti = Config.Bind<float>(scopeFOV, "12x Optic FOV/Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 2 }));
            highOpticFOVMulti = Config.Bind<float>(scopeFOV, "High Magnification Optic FOV/Zoom Multi", 0.5f, new ConfigDescription("Increases/Decreases the FOV/Zoom within Optics. Lower Multi = lower FOV so more Zoom. Requires Restart or going into a new Raid to update FOV. If in Hideout, load into a Raid but cancel out of loading immediately, this will update the FOV.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 1 }));
*/

            globalCameraPOSMulti = Config.Bind<float>(cameraPostiion, "Global Distance to Optic Multi", 1f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Lower = closer to Optic. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 8 }));
            /*            oneCameraPOSMulti = Config.Bind<float>(cameraPostiion, "1x Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 7 }));
                        twoCameraPOSMulti = Config.Bind<float>(cameraPostiion, "2x Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 6 }));
                        threeCameraPOSMulti = Config.Bind<float>(cameraPostiion, "3x Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 5 }));
                        fourCameraPOSMulti = Config.Bind<float>(cameraPostiion, "4x Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 4 }));
                        sixCameraPOSMulti = Config.Bind<float>(cameraPostiion, "6x Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 3 }));
                        twelveCameraPOSMulti = Config.Bind<float>(cameraPostiion, "12x Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 2 }));
                        highCameraPOSMulti = Config.Bind<float>(cameraPostiion, "High Magnification Distance To Optic Multi", 0.5f, new ConfigDescription("Distance of the Camera to Optics when ADSed. Have to unequip and re-equip Weapon to Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 1 }));
            */
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

        void Update() 
        {

        }
    }
}
