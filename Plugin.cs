using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EFT;
using UnityEngine;

namespace FOVFix
{
    [BepInPlugin("com.fontaine.fovfix", "Fontaine-FOVFix", "4.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private bool _detectedMods = false;
        public static bool RealismIsPresent = false;

        public static ConfigEntry<float> test1 { get; set; }
        public static ConfigEntry<float> test2 { get; set; }
        public static ConfigEntry<float> test3 { get; set; }
        public static ConfigEntry<float> test4 { get; set; }

        /*public static ConfigEntry<bool> TrueOneX { get; set; }
          public static ConfigEntry<float> GlobalOpticFOVMulti { get; set; }
          public static ConfigEntry<float> RangeFinderFOV { get; set; }
          public static ConfigEntry<float> BaseScopeFOV { get; set; }
          public static ConfigEntry<float> MagPowerFactor { get; set; }*/

        public static ConfigEntry<KeyboardShortcut> CameraIncreaseOffset { get; set; }
        public static ConfigEntry<KeyboardShortcut> CameraDecreaseOffset { get; set; }
        public static ConfigEntry<float> OpticPosOffset { get; set; }
        public static ConfigEntry<float> NonOpticOffset { get; set; }
        public static ConfigEntry<float> PistolOffset { get; set; }
        public static ConfigEntry<float> RifleLeftShoulderOffset { get; set; }
        public static ConfigEntry<float> PistolLeftShoulderOffset { get; set; }

        public static ConfigEntry<float> GlobalADSMulti { get; set; }
        public static ConfigEntry<float> NonOpticFOVMulti { get; set; }
/*        public static ConfigEntry<float> OneADSMulti { get; set; }
        public static ConfigEntry<float> TwoADSMulti { get; set; }
        public static ConfigEntry<float> ThreeADSMulti { get; set; }
        public static ConfigEntry<float> FourADSMulti { get; set; }
        public static ConfigEntry<float> FiveADSMulti { get; set; }
        public static ConfigEntry<float> SixADSMulti { get; set; }
        public static ConfigEntry<float> EightADSMulti { get; set; }
        public static ConfigEntry<float> TwelveADSMulti { get; set; }
        public static ConfigEntry<float> FourteenADSMulti { get; set; }
        public static ConfigEntry<float> HighADSMulti { get; set; }
        public static ConfigEntry<float> RangeFinderADSMulti { get; set; }*/

        public static ConfigEntry<float> RifleAimSpeedX { get; set; }
        public static ConfigEntry<float> PistolAimSpeedX { get; set; }
        public static ConfigEntry<float> UnAimSpeedX { get; set; }
        public static ConfigEntry<float> RifleAimSpeedY { get; set; }
        public static ConfigEntry<float> PistolAimSpeedY { get; set; }
        public static ConfigEntry<float> UnAimSpeedY { get; set; }
        public static ConfigEntry<float> RifleAimSpeedZ { get; set; }
        public static ConfigEntry<float> PistolAimSpeedZ { get; set; }
        public static ConfigEntry<float> UnAimSpeedZ { get; set; }

        public static ConfigEntry<float> CameraAimSpeed { get; set; }
        public static ConfigEntry<float> PistolAimSpeed { get; set; }
        public static ConfigEntry<float> OpticAimSpeed { get; set; }
        public static ConfigEntry<float> CameraSmoothOut { get; set; }

        public static ConfigEntry<KeyboardShortcut> ToggleZoomKeybind { get; set; }
        public static ConfigEntry<bool> CancelToggleOnUnADS { get; set; }
        public static ConfigEntry<bool> ToggleZoomOnHoldBreath { get; set; }
        public static ConfigEntry<bool> HoldToggleZoom { get; set; }
        public static ConfigEntry<float> OpticToggleZoomMulti { get; set; }
        public static ConfigEntry<float> NonOpticToggleZoomMulti { get; set; }
        public static ConfigEntry<float> UnaimedToggleZoomMulti { get; set; }

        public static ConfigEntry<int> MaxBaseFOV { get; set; }
        public static ConfigEntry<int> MinBaseFOV { get; set; }

        public static ConfigEntry<float> ToggleZoomOpticSensMulti { get; set; }
        public static ConfigEntry<float> ToggleZoomAimSensMulti { get; set; }
        public static ConfigEntry<float> ToggleZoomUnAimSensMulti { get; set; }

        public static ConfigEntry<bool> ChangeMouseSens { get; set; }

        public static ConfigEntry<float> NonOpticSensMulti { get; set; }
        public static ConfigEntry<float> OneSensMulti { get; set; }
        public static ConfigEntry<float> TwoSensMulti { get; set; }
        public static ConfigEntry<float> ThreeSensMulti { get; set; }
        public static ConfigEntry<float> FourSensMulti { get; set; }
        public static ConfigEntry<float> FiveSensMulti { get; set; }
        public static ConfigEntry<float> SixSensMulti { get; set; }
        public static ConfigEntry<float> EightSensMulti { get; set; }
        public static ConfigEntry<float> TenSensMulti { get; set; }
        public static ConfigEntry<float> TwelveSensMulti { get; set; }
        public static ConfigEntry<float> HighSensMulti { get; set; }

        public static ConfigEntry<float> FovScale { get; set; }
        public static ConfigEntry<bool> EnableFovScaleFix { get; set; }

        public static ConfigEntry<float> RifleCameraXOffset { get; set; }
        public static ConfigEntry<float> RifleCameraYOffset { get; set; }
        public static ConfigEntry<float> RifleCameraZOffset { get; set; }
        public static ConfigEntry<float> PistolCameraXOffset { get; set; }
        public static ConfigEntry<float> PistolCameraYOffset { get; set; }
        public static ConfigEntry<float> PistolCameraZOffset { get; set; }

        public static FovController FovController { get; set; }
        public static RealismCompat RealCompat { get; set; } 

        private void Awake()
        {
            string adsFOV = "1. Player Camera ADS FOV";
            string cameraPostiion = "2. ADS Player Camera Position";
            string toggleZoom = "3. Toggleable Zoom";
            string sens = "4. Mouse Sensitivity.";
            string cameraSpeed = "5. Camera Speed";
            string cameraSettings = "6. Camera Settings";
            string testing = ".0. Testing";

            test1 = Config.Bind<float>(testing, "test 1", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 600, IsAdvanced = true }));
            test2 = Config.Bind<float>(testing, "test 2", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 500, IsAdvanced = true }));
            test3 = Config.Bind<float>(testing, "test 3", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 400, IsAdvanced = true }));
            test4 = Config.Bind<float>(testing, "test 4", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-5000f, 5000f), new ConfigurationManagerAttributes { Order = 300, IsAdvanced = true }));

            GlobalADSMulti = Config.Bind<float>(adsFOV, "Optic FOV Multi", 1f, new ConfigDescription("Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.6f, 1.25f), new ConfigurationManagerAttributes { Order = 11 }));
            NonOpticFOVMulti = Config.Bind<float>(adsFOV, "Unmagnified Sight FOV Multi", 1f, new ConfigDescription("Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.6f, 1.25f), new ConfigurationManagerAttributes { Order = 10 }));
            /*         OneADSMulti = Config.Bind<float>(adsFOV, "1x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.41f, 1.3f), new ConfigurationManagerAttributes { Order = 9 }));
                     TwoADSMulti = Config.Bind<float>(adsFOV, "2x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 8 }));
                     ThreeADSMulti = Config.Bind<float>(adsFOV, "3x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 7 }));
                     FourADSMulti = Config.Bind<float>(adsFOV, "4x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 6 }));
                     FiveADSMulti = Config.Bind<float>(adsFOV, "5x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 6 }));
                     SixADSMulti = Config.Bind<float>(adsFOV, "6x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 5 }));
                     EightADSMulti = Config.Bind<float>(adsFOV, "8x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 4 }));
                     TwelveADSMulti = Config.Bind<float>(adsFOV, "12x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
                     FourteenADSMulti = Config.Bind<float>(adsFOV, "14x ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 3 }));
                     HighADSMulti = Config.Bind<float>(adsFOV, "High Magnification ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 2 }));
                     RangeFinderADSMulti = Config.Bind<float>(adsFOV, "Range Finder ADS FOV Multi", 1f, new ConfigDescription("Multiplier For The FOV Change When ADSing. Lower Multi = Lower FOV So More Zoom.", new AcceptableValueRange<float>(0.4f, 1.3f), new ConfigurationManagerAttributes { Order = 1 }));
         */
            CameraIncreaseOffset = Config.Bind(cameraPostiion, "Increase Camera Offset Key", new KeyboardShortcut(KeyCode.KeypadMultiply), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 60 }));
            CameraDecreaseOffset = Config.Bind(cameraPostiion, "Decrease Camera Offset Key", new KeyboardShortcut(KeyCode.KeypadDivide), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 60 }));
            OpticPosOffset = Config.Bind<float>(cameraPostiion, "Optic Camera Distance Offset", -0.03f, new ConfigDescription("Distance Of The Camera To Optics When ADSed. Lower = Closer To Optic.", new AcceptableValueRange<float>(-0.2f, 0.2f), new ConfigurationManagerAttributes { Order = 1 }));
            NonOpticOffset = Config.Bind<float>(cameraPostiion, "Non-Optic Camera Distance Offset", -0.01f, new ConfigDescription("Distance Of The Camera To Sights When ADSed. Lower = Closer To Optic.", new AcceptableValueRange<float>(-0.2f, 0.2f), new ConfigurationManagerAttributes { Order = 2 }));
            PistolOffset = Config.Bind<float>(cameraPostiion, "Pistol Camera Distance Offset", 0f, new ConfigDescription("Distance Of The Camera To Sights When ADSed. Lower = Closer To Optic.", new AcceptableValueRange<float>(-0.2f, 0.2f), new ConfigurationManagerAttributes { Order = 3 }));
            RifleLeftShoulderOffset = Config.Bind<float>(cameraPostiion, "Rifle Left Shoulder Offset", 0f, new ConfigDescription("Distance Of The Camera To Sights When ADSed. Lower = Closer To Optic. Set Till Left Shoulder Offset Matches Right Shoulder, Will Depend On Your Set Up. Does Not Apply If Realism Stances Are Enabled.", new AcceptableValueRange<float>(-1.0f, 1.0f), new ConfigurationManagerAttributes { Order = 4 }));
            PistolLeftShoulderOffset = Config.Bind<float>(cameraPostiion, "Pistol Left Shoulder Offset", 0f, new ConfigDescription("Distance Of The Camera To Sights When ADSed. Lower = Closer To Optic. Set Till Left Shoulder Offset Matches Right Shoulder, Will Depend On Your Set Up. Does Not Apply If Realism Stances Are Enabled.", new AcceptableValueRange<float>(-1.0f, 1.0f), new ConfigurationManagerAttributes { Order = 4 }));

            ToggleZoomKeybind = Config.Bind(toggleZoom, "Zoom Toggle", new KeyboardShortcut(KeyCode.M), new ConfigDescription("Toggle To Zoom.", null, new ConfigurationManagerAttributes { Order = 60 }));
            HoldToggleZoom = Config.Bind<bool>(toggleZoom, "Hold To Zoom", true, new ConfigDescription("Change Zoom To A Hold Keybind.", null, new ConfigurationManagerAttributes { Order = 50 }));
            CancelToggleOnUnADS = Config.Bind<bool>(toggleZoom, "Cancel Zoom On Un-ADS", false, new ConfigDescription("Cancels Toggle Zoom When Un-ADSing If Not Using Hold To Zoom", null, new ConfigurationManagerAttributes { Order = 31 }));
            ToggleZoomOnHoldBreath = Config.Bind<bool>(toggleZoom, "Enable Toggle Zoom On Hold-Breath", false, new ConfigDescription("Toggle Zoom Will Activated When Holding Breath Only.", null, new ConfigurationManagerAttributes { Order = 30 }));
            OpticToggleZoomMulti = Config.Bind<float>(toggleZoom, "Optics Toggle FOV Multi", 0.9f, new ConfigDescription("FOV Multiplier When Aiming With Optic.", new AcceptableValueRange<float>(0.5f, 1.25f), new ConfigurationManagerAttributes { Order = 20 }));
            NonOpticToggleZoomMulti = Config.Bind<float>(toggleZoom, "Non-Optics Toggle FOV Multi", 0.8f, new ConfigDescription("FOV Multiplier When Aiming with Non-Optic Or Not Aiming.", new AcceptableValueRange<float>(0.51f, 1.25f), new ConfigurationManagerAttributes { Order = 10 }));
            UnaimedToggleZoomMulti = Config.Bind<float>(toggleZoom, "Un-Aimed Toggle FOV Multi", 0.8f, new ConfigDescription("FOV Multiplier When Aiming with Non-Optic Or Not Aiming.", new AcceptableValueRange<float>(0.5f, 1.25f), new ConfigurationManagerAttributes { Order = 9 }));
            ToggleZoomAimSensMulti = Config.Bind<float>(toggleZoom, "Non-Optics Toggle Zoom Sens Multi", 0.7f, new ConfigDescription("Sens Modifier When Zoom Is Toggled.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 3 }));
            ToggleZoomUnAimSensMulti = Config.Bind<float>(toggleZoom, "Un-Aimed Toggle Zoom Sens Multi", 0.7f, new ConfigDescription("Sens Modifier When Zoom Is Toggled.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 2 }));
            ToggleZoomOpticSensMulti = Config.Bind<float>(toggleZoom, "Optics Toggle Zoom Sens Multi", 0.8f, new ConfigDescription("Sens Modifier When Zoom Toggled.", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 1 }));

            ChangeMouseSens = Config.Bind<bool>(sens, "Change Mouse Sensitivity", true, new ConfigDescription("Sets Mouse Sensitivity Based On The Scope's Current Magnificaiton. Non-Optical Sights Are Treated The Same As 1x.", null, new ConfigurationManagerAttributes { Order = 100 }));
            NonOpticSensMulti = Config.Bind<float>(sens, "Unmagnified Sight Sens Multi", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 11 }));
            OneSensMulti = Config.Bind<float>(sens, "1x Sens Multi", 0.75f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 10 }));
            TwoSensMulti = Config.Bind<float>(sens, "2x Sens Multi", 0.45f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 9 }));
            ThreeSensMulti = Config.Bind<float>(sens, "3x Sens Multi", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 8 }));
            FourSensMulti = Config.Bind<float>(sens, "4x Sens Multi", 0.2f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 7 }));
            FiveSensMulti = Config.Bind<float>(sens, "5x Sens Multi", 0.15f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 6 }));
            SixSensMulti = Config.Bind<float>(sens, "6x Sens Multi", 0.125f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 5 }));
            EightSensMulti = Config.Bind<float>(sens, "8x Sens Multi", 0.08f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 4 }));
            TenSensMulti = Config.Bind<float>(sens, "10x Sens Multi", 0.04f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 3 }));
            TwelveSensMulti = Config.Bind<float>(sens, "12x Sens Multi", 0.03f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 2 }));
            HighSensMulti = Config.Bind<float>(sens, "High Sens Multi", 0.01f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 2f), new ConfigurationManagerAttributes { Order = 1 }));

            RifleCameraXOffset = Config.Bind<float>(cameraSettings, "Rifle Camera X Offset", 0.04f, new ConfigDescription("Moves the player camera relative to the player's hands. Don't recommend changing.", new AcceptableValueRange<float>(-0.3f, 0.3f), new ConfigurationManagerAttributes { Order = 20 }));
            RifleCameraYOffset = Config.Bind<float>(cameraSettings, "Rifle Camera Y Offset", 0.04f, new ConfigDescription("Moves the player camera relative to the player's hands. Don't recommend changing.", new AcceptableValueRange<float>(-0.3f, 0.3f), new ConfigurationManagerAttributes { Order = 20 }));
            RifleCameraZOffset = Config.Bind<float>(cameraSettings, "Rifle Camera Z Offset (Formerly Hud FOV)", 0.025f, new ConfigDescription("How Far Away The Player Camera Is From The Player's Arms And Weapon, Making Them Appear Closer/Larger Or Further Away/Smaller", new AcceptableValueRange<float>(-0.3f, 0.3f), new ConfigurationManagerAttributes { Order = 20 }));

            PistolCameraXOffset = Config.Bind<float>(cameraSettings, "Pistol Camera X Offset", 0.04f, new ConfigDescription("Moves the player camera relative to the player's hands. Don't recommend changing.", new AcceptableValueRange<float>(-0.3f, 0.3f), new ConfigurationManagerAttributes { Order = 20 }));
            PistolCameraYOffset = Config.Bind<float>(cameraSettings, "Pistol Camera Y Offset", 0.04f, new ConfigDescription("Moves the player camera relative to the player's hands. Don't recommend changing.", new AcceptableValueRange<float>(-0.3f, 0.3f), new ConfigurationManagerAttributes { Order = 20 }));
            PistolCameraZOffset = Config.Bind<float>(cameraSettings, "Pistol Camera Z Offset (Formerly Hud FOV)", 0.025f, new ConfigDescription("How Far Away The Player Camera Is From The Player's Arms And Weapon, Making Them Appear Closer/Larger Or Further Away/Smaller", new AcceptableValueRange<float>(-0.3f, 0.3f), new ConfigurationManagerAttributes { Order = 20 }));


            EnableFovScaleFix = Config.Bind<bool>(cameraSettings, "Enable FOV Scale Fix", false, new ConfigDescription("Lower Value = More Viewmodel Distortion.", null, new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true }));
            FovScale = Config.Bind<float>(cameraSettings, "FOV Scale", 1f, new ConfigDescription("Viewmodel FOV. A Value Of One Reduces The Distortion Caused By Higher FOV Settings, Significantly Reducing Issues With Laser Misallignment And Optics Recoil. Does Make Weapon Postion And Scale Look Different.", new AcceptableValueRange<float>(0f, 2f), new ConfigurationManagerAttributes { Order = 4, IsAdvanced = true }));
            MaxBaseFOV = Config.Bind<int>(cameraSettings, "Max Base FOV", 110, new ConfigDescription("Max Selectable Main Camera FOV In Game Settings.", new AcceptableValueRange<int>(1, 200), new ConfigurationManagerAttributes { Order = 2 }));
            MinBaseFOV = Config.Bind<int>(cameraSettings, "Min Base FOV", 30, new ConfigDescription("Min Selectable Main Camera FOVIn Game Settings.", new AcceptableValueRange<int>(1, 200), new ConfigurationManagerAttributes { Order = 1 }));

            CameraAimSpeed = Config.Bind<float>(cameraSpeed, "Rfile Camera Speed", 1f, new ConfigDescription("Global Multi For The Speed Of ADS Camera Transitions For Rifles Without Optics. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 40 }));
            PistolAimSpeed = Config.Bind<float>(cameraSpeed, "Pistol Camera Speed", 1f, new ConfigDescription("Global Multi For The Speed Of ADS Camera Transitions For Pistols. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Weapons Can Have At High FOV.", new AcceptableValueRange<float>(0, 10f), new ConfigurationManagerAttributes { Order = 30 }));
            OpticAimSpeed = Config.Bind<float>(cameraSpeed, "Optic Camera Speed", 1f, new ConfigDescription("Global Multi For The Speed Of ADS Camera Transitions For Rifels With Optics. A Low Value Can Be Used To Smoothen Out The Overly Snappy Transitions Some Scope And Weapon Combinations Can Have At High FOV.", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 20 }));

            RifleAimSpeedX = Config.Bind<float>(cameraSpeed, "Rfile Camera Aim Speed X-Axis", 1f, new ConfigDescription("The Speed Of The Player Camera When Aiming For The X-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 11 }));
            PistolAimSpeedX = Config.Bind<float>(cameraSpeed, "Pistol Camera Aim Speed X-Axis", 1f, new ConfigDescription("The Speed Of The Player Camera When Aiming For The X-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 11 }));

            UnAimSpeedX = Config.Bind<float>(cameraSpeed, "Camera Un-Aim Speed X-Axis", 5.5f, new ConfigDescription("The Speed Of The Player Camera When Un-Aiming For The X-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 2 }));
            RifleAimSpeedY = Config.Bind<float>(cameraSpeed, "Rifle Camera Aim Speed Y-Axis", 1f, new ConfigDescription("The Speed Of The Player Camera When Aiming For The Y-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 11 }));
            PistolAimSpeedY = Config.Bind<float>(cameraSpeed, "Pistol Camera Aim Speed Y-Axis", 1f, new ConfigDescription("The Speed Of The Player Camera When Aiming For The Y-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 11 }));
            UnAimSpeedY = Config.Bind<float>(cameraSpeed, "Camera Un-Aim Speed Y-Axis", 4.5f, new ConfigDescription("The Speed Of The Player Camera When Un-Aiming For The Y-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 2 }));
            RifleAimSpeedZ = Config.Bind<float>(cameraSpeed, "Rifle Camera Aim Speed Z-Axis", 3f, new ConfigDescription("The Speed Of The Player Camera When Aiming For The Z-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 11 }));
            PistolAimSpeedZ = Config.Bind<float>(cameraSpeed, "Pistol Camera Aim Speed Z-Axis", 1f, new ConfigDescription("The Speed Of The Player Camera When Aiming For The Z-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 11 }));
            UnAimSpeedZ = Config.Bind<float>(cameraSpeed, "Camera Un-Aim Speed Z-Axis", 4.5f, new ConfigDescription("The Speed Of The Player Camera When Un-Aiming For The Z-Axis Specifically", new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 2 }));

            EnableFovScaleFix.SettingChanged += (obj, args) =>
            {
                if (EnableFovScaleFix.Value)
                {
                    CalculateScaleValueByFovPatch.UpdateRibcageScale(FovScale.Value);
                }
                else
                {
                    CalculateScaleValueByFovPatch.RestoreScale();
                }
            };
            
            FovScale.SettingChanged += (obj, args) =>
            {
                if (EnableFovScaleFix.Value)
                {
                    CalculateScaleValueByFovPatch.UpdateRibcageScale(FovScale.Value);
                }
            };
            
            Utils.Logger = Logger;  
            FovController = new FovController();

            new PwaWeaponParamsPatch().Enable();
            new FreeLookPatch().Enable();
            new LerpCameraPatch().Enable();
            new FovRangePatch().Enable();
            new FovValuePatch().Enable();
            new AimingSensitivityPatch().Enable();
            new ScopeSensitivityPatch().Enable();
            new CloneItemPatch().Enable();
            new SetPlayerAimingPatch().Enable();
            new CalculateScaleValueByFovPatch().Enable();
        }

        private void CheckForMods()
        {
            if (!_detectedMods && (int)Time.time % 5 == 0)
            {
                _detectedMods = true;
                if (Chainloader.PluginInfos.ContainsKey("RealismMod"))
                {
                    Logger.LogWarning("=============================== Fov Fix: Realism Mod is loaded ===============================");
                    RealismIsPresent = true;
                    Plugin.RealCompat = new RealismCompat();
                }
            }
        }

        void Update()
        {
            if (RealismIsPresent) Plugin.RealCompat.Update();
            CheckForMods();
            FovController.ControllerUpdate();
        }
    }
}
