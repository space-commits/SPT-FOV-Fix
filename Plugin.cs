using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using HarmonyLib;
using EFT.Animations;
using EFT.InventoryLogic;

namespace FOVFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        public static bool IsOptic;
        public static bool HasRAPTAR = false;
        public static bool IsReady = false;
        public static bool WeaponReady = false;
        public static Player Player;
        public static bool CalledZoom = false;
        public static bool ZoomReset = false;
        public static bool DoZoom = false;
        public static ConfigEntry<bool> TrueOneX { get; set; }
        public static ConfigEntry<float> RangeFinderFOV { get; set; }
        public static ConfigEntry<float> GlobalOpticFOVMulti { get; set; }
        public static ConfigEntry<float> OpticPosOffset { get; set; }
        public static ConfigEntry<float> NonOpticOffset { get; set; }
        public static ConfigEntry<float> PistolOffset { get; set; }
        public static ConfigEntry<float> GlobalADSMulti { get; set; }
        public static ConfigEntry<float> OneADSMulti { get; set; }
        public static ConfigEntry<float> TwoADSMulti { get; set; }
        public static ConfigEntry<float> ThreeADSMulti { get; set; }
        public static ConfigEntry<float> FourADSMulti { get; set; }
        public static ConfigEntry<float> SixADSMulti { get; set; }
        public static ConfigEntry<float> EightADSMulti { get; set; }
        public static ConfigEntry<float> TwelveADSMulti { get; set; }
        public static ConfigEntry<float> FourteenADSMulti { get; set; }
        public static ConfigEntry<float> HighADSMulti { get; set; }
        public static ConfigEntry<float> RangeFinderADSMulti { get; set; }
        public static ConfigEntry<float> CameraSmoothTime { get; set; }
        public static ConfigEntry<float> PistolSmoothTime { get; set; }
        public static ConfigEntry<float> OpticSmoothTime { get; set; }
        public static ConfigEntry<float> CameraSmoothOut { get; set; }
        public static ConfigEntry<bool> DisableRangeF { get; set; }
        public static ConfigEntry<bool> HoldZoom { get; set; }
        public static ConfigEntry<bool> EnableExtraZoomOptic { get; set; }
        public static ConfigEntry<bool> EnableExtraZoomNonOptic { get; set; }
        public static ConfigEntry<float> OpticExtraZoom { get; set; }
        public static ConfigEntry<float> NonOpticExtraZoom { get; set; }
        public static ConfigEntry<KeyboardShortcut> ZoomKeybind { get; set; }



        private void Awake()
        {
            string scopeFOV = "1. Scope Zoom";
            string adsFOV = "2. ADS FOV";
            string cameraPostiion = "3. ADS Camera Position";
            string toggleZoom = "4. Toggleable Zoom";
            string misc = "5. Misc.";

            GlobalOpticFOVMulti = Config.Bind<float>(scopeFOV, "Global Optic Magnificaiton Multi", 0.75f, new ConfigDescription("Increases/Decreases The FOV/Magnification Within Optics. Lower Multi = Lower FOV So More Zoom. Requires Restart Or Going Into A New Raid To Update Magnification. If In Hideout, Load Into A Raid But Cancel Out Of Loading Immediately, This Will Update The FOV.", new AcceptableValueRange<float>(0.1f, 1.25f), new ConfigurationManagerAttributes { Order = 3 }));
/*            rangeFinderFOV = Config.Bind<float>(scopeFOV, "Range Finder Magnificaiton", 15, new ConfigDescription("Set The Magnification For The Range Finder Seperately From The Global Multi. If The Magnification Is Too High, The Rang Finder Text Will Break. Lower Value = Lower FOV So More Zoom.", new AcceptableValueRange<float>(1f, 30f), new ConfigurationManagerAttributes { Order = 2 }));
*/            TrueOneX = Config.Bind<bool>(scopeFOV, "True 1x Magnification", true, new ConfigDescription("1x Scopes Will Override 'Global Optic Magnificaiton Multi' And Stay At A True 1x Magnification. Requires Restart Or Going Into A New Raid To Update FOV. If In Hideout, Load Into A Raid But Cancel Out Of Loading Immediately, This Will Update The FOV.", null, new ConfigurationManagerAttributes { Order = 1 }));

            OpticPosOffset = Config.Bind<float>(cameraPostiion, "Optic Camera Distance Offset", 0.0f, new ConfigDescription("Distance Of The Camera To Optics When ADSed. Lower = Closer To Optic.", new AcceptableValueRange<float>(-1.0f, 1.0f), new ConfigurationManagerAttributes { Order = 1 }));
            NonOpticOffset = Config.Bind<float>(cameraPostiion, "Non-Optic Camera Distance Offset", 0.0f, new ConfigDescription("Distance Of The Camera To Sights When ADSed. Lower = Closer To Optic.", new AcceptableValueRange<float>(-1.0f, 1.0f), new ConfigurationManagerAttributes { Order = 2 }));
            PistolOffset = Config.Bind<float>(cameraPostiion, "Pistol Camera Distance Offset", 0.0f, new ConfigDescription("Distance Of The Camera To Sights When ADSed. Lower = Closer To Optic.", new AcceptableValueRange<float>(-1.0f, 1.0f), new ConfigurationManagerAttributes { Order = 3 }));

            GlobalADSMulti = Config.Bind<float>(adsFOV, "Global ADS FOV Multi", 1f, new ConfigDescription("Applies On Top Of All Other ADS FOV Change Multies. Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 10 }));
            OneADSMulti = Config.Bind<float>(adsFOV, "1x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.41f, 1.3f), new ConfigurationManagerAttributes { Order = 9 }));
            TwoADSMulti = Config.Bind<float>(adsFOV, "2x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 8 }));
            ThreeADSMulti = Config.Bind<float>(adsFOV, "3x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 7 }));
            FourADSMulti = Config.Bind<float>(adsFOV, "4x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 6 }));
            SixADSMulti = Config.Bind<float>(adsFOV, "6x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 5 }));
            EightADSMulti = Config.Bind<float>(adsFOV, "8x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 4 }));
            TwelveADSMulti = Config.Bind<float>(adsFOV, "12x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
            FourteenADSMulti = Config.Bind<float>(adsFOV, "14x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
            HighADSMulti = Config.Bind<float>(adsFOV, "High Magnification ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 2 }));
            RangeFinderADSMulti = Config.Bind<float>(adsFOV, "Range Finder ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 1 }));

            PistolSmoothTime = Config.Bind<float>(misc, "Pistol Camera Smooth Time", 8f, new ConfigDescription("If Using Realism Or Combat Stances, It Is Recommended To Set This To 0. The Speed Of ADS Camera Transitions. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(-10f, 10f), new ConfigurationManagerAttributes { Order = 10 }));
            OpticSmoothTime = Config.Bind<float>(misc, "Optic Camera Smooth Time", 8f, new ConfigDescription("The Speed Of ADS Camera Transitions. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(-10f, 10f), new ConfigurationManagerAttributes { Order = 20 }));
            CameraSmoothTime = Config.Bind<float>(misc, "Camera Smooth Time", 8f, new ConfigDescription("The Speed Of ADS Camera Transitions. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(-10f, 10f), new ConfigurationManagerAttributes { Order = 30 }));
            CameraSmoothOut = Config.Bind<float>(misc, "Camera Smooth Out", 6f, new ConfigDescription("The Speed Of ADS Camera Transitions. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(-10f, 10f), new ConfigurationManagerAttributes { Order = 40 }));

            /*            disableRangeF = Config.Bind<bool>(misc, "Disable Range Finder Patch", false, new ConfigDescription("Disables Patching For Range Finders. Use This Option If There Are Any Unforseen Issues With Range Finders.", null, new ConfigurationManagerAttributes { Order = 3 }));
            */
            ZoomKeybind = Config.Bind(toggleZoom, "Zoom Toggle", new KeyboardShortcut(KeyCode.M), new ConfigDescription("Toggle To Zoom.", null, new ConfigurationManagerAttributes { Order = 6 }));
            HoldZoom = Config.Bind<bool>(toggleZoom, "Hold To Zoom", false, new ConfigDescription("Change Zoom To A Hold Keybind.", null, new ConfigurationManagerAttributes { Order = 5 }));
            EnableExtraZoomOptic = Config.Bind<bool>(toggleZoom, "Enable Toggleable Zoom For Optics", false, new ConfigDescription("Using A Keybind, You Can Get Additional Zoom/Magnification When Aiming.", null, new ConfigurationManagerAttributes { Order = 4 }));
            EnableExtraZoomNonOptic = Config.Bind<bool>(toggleZoom, "Enable Toggleable Zoom For Non-Optics", false, new ConfigDescription("Using A Keybind, You Can Get Additional Zoom/Magnification When Aiming..", null, new ConfigurationManagerAttributes { Order = 3 }));
            OpticExtraZoom = Config.Bind<float>(toggleZoom, "Optics Toggle FOV Multi", 1f, new ConfigDescription("FOV Multiplier When Toggled.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 2 }));
            NonOpticExtraZoom = Config.Bind<float>(toggleZoom, "Non-Optics Toggle FOV Multi", 1f, new ConfigDescription("FOV Multiplier When Toggled.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 1 }));


            new OpticSightAwakePatch().Enable();
            new method_20Patch().Enable();
/*            new TacticalRangeFinderControllerPatch().Enable();
            new OnWeaponParametersChangedPatch().Enable();*/
            new FreeLookPatch().Enable();
            new LerpCameraPatch().Enable();


            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        void Update()
        {
            Helper.CheckIsReady();

            if (Plugin.IsReady && Plugin.WeaponReady && Player.HandsController != null && (EnableExtraZoomOptic.Value || EnableExtraZoomNonOptic.Value)) 
            {

                var method_20 = AccessTools.Method(typeof(ProceduralWeaponAnimation), "method_20");


                if (HoldZoom.Value)
                {

                    if (Input.GetKey(ZoomKeybind.Value.MainKey) && !Plugin.CalledZoom)
                    {
                        Plugin.DoZoom = true;
                        method_20.Invoke(Player.ProceduralWeaponAnimation, new object[] { });
                        Plugin.CalledZoom = true;
                        Plugin.DoZoom = false;

                    }
                    if (!Input.GetKey(ZoomKeybind.Value.MainKey) && Plugin.CalledZoom)
                    {
                        method_20.Invoke(Player.ProceduralWeaponAnimation, new object[] { });
                        Plugin.CalledZoom = false;
                    }
                }
                else
                {

                    if (Input.GetKeyDown(ZoomKeybind.Value.MainKey))
                    {
                        Plugin.DoZoom = !Plugin.DoZoom;
                        method_20.Invoke(Player.ProceduralWeaponAnimation, new object[] { });
                    }
                }
            }
        }
    }
}
