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
        public static ConfigEntry<float> rangeFinderFOV { get; set; }
        public static ConfigEntry<float> globalOpticFOVMulti { get; set; }
        public static ConfigEntry<float> globalCameraPOSMulti { get; set; }
        public static ConfigEntry<float> globalADSMulti { get; set; }
        public static ConfigEntry<float> oneADSMulti { get; set; }
        public static ConfigEntry<float> twoADSMulti { get; set; }
        public static ConfigEntry<float> threeADSMulti { get; set; }
        public static ConfigEntry<float> fourADSMulti { get; set; }
        public static ConfigEntry<float> sixADSMulti { get; set; }
        public static ConfigEntry<float> eightADSMulti { get; set; }
        public static ConfigEntry<float> twelveADSMulti { get; set; }
        public static ConfigEntry<float> fourteenADSMulti { get; set; }
        public static ConfigEntry<float> highADSMulti { get; set; }
        public static ConfigEntry<float> rangeFinderADSMulti { get; set; }
        public static ConfigEntry<float> cameraSmoothTime { get; set; }
        public static ConfigEntry<bool> disableRangeF { get; set; }


        private void Awake()
        {
            string scopeFOV = "1. Scope Zoom";
            string adsFOV = "2. ADS FOV";
            string cameraPostiion = "3. ADS Camera Position";
            string misc = "4. Misc.";

            globalOpticFOVMulti = Config.Bind<float>(scopeFOV, "Global Optic Magnificaiton Multi", 0.75f, new ConfigDescription("Increases/Decreases The FOV/Magnification Within Optics. Lower Multi = Lower FOV So More Zoom. Requires Restart Or Going Into A New Raid To Update Magnification. If In Hideout, Load Into A Raid But Cancel Out Of Loading Immediately, This Will Update The FOV.", new AcceptableValueRange<float>(0.1f, 1.25f), new ConfigurationManagerAttributes { Order = 3 }));
            rangeFinderFOV = Config.Bind<float>(scopeFOV, "Range Finder Magnificaiton", 15, new ConfigDescription("Set The Magnification For The Range Finder Seperately From The Global Multi. If The Magnification Is Too High, The Rang Finder Text Will Break. Lower Value = Lower FOV So More Zoom.", new AcceptableValueRange<float>(1f, 30f), new ConfigurationManagerAttributes { Order = 2 }));
            trueOneX = Config.Bind<bool>(scopeFOV, "True 1x Magnification", true, new ConfigDescription("1x Scopes Will Override 'Global Optic Magnificaiton Multi' And Stay At A True 1x Magnification. Requires Restart Or Going Into A New Raid To Update FOV. If In Hideout, Load Into A Raid But Cancel Out Of Loading Immediately, This Will Update The FOV.", null, new ConfigurationManagerAttributes { Order = 1 }));

            globalCameraPOSMulti = Config.Bind<float>(cameraPostiion, "Global Distance to Optic Multi", 1f, new ConfigDescription("Distance Of The Camera To Optics When ADSed. Lower = Closer To Optic. Have To Unequip And Re-Equip Weapon To Update Distance.", new AcceptableValueRange<float>(0.01f, 1.5f), new ConfigurationManagerAttributes { Order = 1 }));

            globalADSMulti = Config.Bind<float>(adsFOV, "Global ADS FOV Multi", 1f, new ConfigDescription("Applies On Top Of All Other ADS FOV Change Multies. Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 10 }));
            oneADSMulti = Config.Bind<float>(adsFOV, "1x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.41f, 1.3f), new ConfigurationManagerAttributes { Order = 9 }));
            twoADSMulti = Config.Bind<float>(adsFOV, "2x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 8 }));
            threeADSMulti = Config.Bind<float>(adsFOV, "3x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 7 }));
            fourADSMulti = Config.Bind<float>(adsFOV, "4x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 6 }));
            sixADSMulti = Config.Bind<float>(adsFOV, "6x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 5 }));
            eightADSMulti = Config.Bind<float>(adsFOV, "8x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 4 }));
            twelveADSMulti = Config.Bind<float>(adsFOV, "12x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
            fourteenADSMulti = Config.Bind<float>(adsFOV, "14x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
            highADSMulti = Config.Bind<float>(adsFOV, "High Magnification ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 2 }));
            rangeFinderADSMulti = Config.Bind<float>(adsFOV, "Range Finder ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 1 }));

            cameraSmoothTime = Config.Bind<float>(misc, "Camera Smooth Time", 3f, new ConfigDescription("The Speed Of ADS Camera Transitions. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 1 }));
            disableRangeF = Config.Bind<bool>(misc, "Disable Range Finder Patch", true, new ConfigDescription("Disables Patching For Range Finders. This Prevents The RAPTAR Range Finder Breaking Scope FOV. Use This Option If Using That Device Instead Of The Hendheld RF.", null, new ConfigurationManagerAttributes { Order = 1 }));


            new GetAnyOpticsDistanceToCameraPatch().Enable();
            new OpticSightAwakePatch().Enable();
            new method_20Patch().Enable();
            new TacticalRangeFinderControllerPatch().Enable();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
