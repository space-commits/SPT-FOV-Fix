using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.CameraControl;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Settings;
using HarmonyLib;
using RealismMod;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;
using static EFT.Player;
using static GClass1085;
//using FCSubClass = EFT.Player.FirearmController.GClass1780;
// System.String EFT.Player/FirearmController/GClass????::SHELLPORT_TRANSFORM_NAME
//using InputClass1 = Class1604;
// EFT.IFirearmHandsController Class????::ifirearmHandsController_0
//using InputClass2 = Class1579;
using GameSettingsClass = GClass1085;

namespace FOVFix
{
    //don't remember its purpose...
    public class CloneItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass3380).GetMethod("CloneItem", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(typeof(Item));
            // IEnumerable<EFT.InventoryLogic.Item> GClass????::GetAllItemsFromGridItemCollectionNonAlloc(GClass2924, List<EFT.InventoryLogic.Item>)
            // very good distinct name to search for
        }

        [PatchPostfix]
        private static void PatchPostfix(Item originalItem, ref Item __result)
        {
            if (Utils.PlayerIsReady && Utils.IsInHideout && originalItem is Weapon)
            {
                WeaponClassExtension.SetCustomProperty(__result, originalItem.Id);
            }
        }
    }

    //can modifer Optic cam FOV here but doesnt' work for fixed optics, and causes bugs when swapping between variable and fixed optics on different guns
    public class CameraUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(OpticComponentUpdater).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(OpticComponentUpdater __instance, Camera ___camera_0, ScopeZoomHandler ___scopeZoomHandler_0)
        {
            if (___camera_0 != null && ___scopeZoomHandler_0 != null) ___camera_0.fieldOfView = ___scopeZoomHandler_0.FiledOfView;
            Logger.LogWarning($"cam is null {(___camera_0 == null)}, scopezoom is null {(___scopeZoomHandler_0 == null)}");
        }
    }

    //can use to display aproximate IRL magnificaiton
    public class OpticPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AnimatedTextPanel).GetMethod("Show", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(AnimatedTextPanel __instance)
        {
            if (__instance.gameObject.name == "OpticCratePanel") Plugin.FovController.OpticPanel = __instance;
        }
    }


    //can use to control scope zoom, affects both reticle and scope FOV. Not sure if it's worth it though considering how inconsistent FOV is between different scopes
    public class ScopeZoomPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ScopeZoomHandler).GetMethod("method_4");
        }

        [PatchPostfix]
        private static void Patch(ScopeZoomHandler __instance, SightComponent ___sightComponent_0, ref float ___float_1, ref float ___float_2)
        {
            /*
                        float scrollDelta = Input.mouseScrollDelta.y * Plugin.test4.Value;
                        if (scrollDelta != 0f)
                        {
                            HandleZoomInput(scrollDelta);
                        }
                        float zoom = Mathf.Clamp(Plugin.test1.Value, __instance.Single_1, __instance.Single_0); //
                        ___float_1 = zoom;
                        ___float_2 = zoom;
                        ___sightComponent_0.ScopeZoomValue = zoom;
                        __instance.method_9();
            */

            //Logger.LogWarning($"possible scope zoom: {Plugin.FovController.CurrentScopeFOV} , real scope zoom: {___sightComponent_0.ScopeZoomValue}");
        }
    }

    //allows bigger base FOV range in settings
    public class FovRangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameSettingsTab).GetMethod("Show");
        }

        [PatchPostfix]
        private static void PostFix(NumberSlider ____fov, GameSettingsClass gameSettings)
        {
            if (Plugin.MinBaseFOV.Value > Plugin.MaxBaseFOV.Value)
            {
                Plugin.MinBaseFOV.Value = 50;
                Plugin.MaxBaseFOV.Value = 75;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            SettingsTab.BindNumberSliderToSetting(____fov, gameSettings.FieldOfView, Plugin.MinBaseFOV.Value, Plugin.MaxBaseFOV.Value);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    //allows bigger base FOV range in settings
    public class FovValuePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Class1841).GetMethod("method_0");
            // subclass of this class: Bsg.GameSettings.GameSetting<Boolean> GClass????::StreamerModeEnabled
        }

        [PatchPostfix]
        private static void PostFix(ref int __result, int x)
        {
            if (Plugin.MinBaseFOV.Value > Plugin.MaxBaseFOV.Value)
            {
                Plugin.MinBaseFOV.Value = 50;
                Plugin.MaxBaseFOV.Value = 75;
            }
            __result = Mathf.Clamp(x, Plugin.MinBaseFOV.Value, Plugin.MaxBaseFOV.Value);
        }
    }

    //Required to prevent bugs and issues with my modifications to FOV when using freelook
    public class FreeLookPatch : ModulePatch
    {
        private static FieldInfo resetLookField;
        private static FieldInfo mouseLookControlField;
        private static FieldInfo isResettingLookField;
        private static FieldInfo setResetedLookNextFrameField;
        private static FieldInfo isLookingField;
        private static FieldInfo horizontalField;
        private static FieldInfo verticalField;

        protected override MethodBase GetTargetMethod()
        {
            resetLookField = AccessTools.Field(typeof(Player), "_resetLook");
            mouseLookControlField = AccessTools.Field(typeof(Player), "_mouseLookControl");
            isResettingLookField = AccessTools.Field(typeof(Player), "_isResettingLook");
            setResetedLookNextFrameField = AccessTools.Field(typeof(Player), "_setResetedLookNextFrame");
            isLookingField = AccessTools.Field(typeof(Player), "_isLooking");
            horizontalField = AccessTools.Field(typeof(Player), "_horizontal");
            verticalField = AccessTools.Field(typeof(Player), "_vertical");

            return typeof(Player).GetMethod("Look", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(Player __instance, float deltaLookY, float deltaLookX, bool withReturn = true)
        {
            Player.FirearmController fc = __instance.HandsController as Player.FirearmController;
            if (fc == null || !__instance.IsYourPlayer) return true;

            bool _resetLook = (bool)resetLookField.GetValue(__instance);
            bool mouseLookControl = (bool)mouseLookControlField.GetValue(__instance);
            bool isResettingLook = (bool)isResettingLookField.GetValue(__instance);
            bool _setResetedLookNextFrame = (bool)setResetedLookNextFrameField.GetValue(__instance);
            bool isLooking = (bool)isLookingField.GetValue(__instance);

            float _horizontal = (float)horizontalField.GetValue(__instance);
            float _vertical = (float)verticalField.GetValue(__instance);

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming && !__instance.IsAI;
            EFTHardSettings instance = EFTHardSettings.Instance;
            Vector2 horizontalLimit = new Vector2(-50f, 50f);
            Vector2 mouse_LOOK_VERTICAL_LIMIT = instance.MOUSE_LOOK_VERTICAL_LIMIT;
            if (isAiming)
            {
                horizontalLimit *= instance.MOUSE_LOOK_LIMIT_IN_AIMING_COEF;
            }
            Vector3 eulerAngles = __instance.ProceduralWeaponAnimation.HandsContainer.CameraTransform.eulerAngles;
            if (eulerAngles.x >= 50f && eulerAngles.x <= 90f && __instance.MovementContext.IsSprintEnabled)
            {
                mouse_LOOK_VERTICAL_LIMIT.y = 0f;
            }
            horizontalField.SetValue(__instance, Mathf.Clamp(_horizontal - deltaLookY, horizontalLimit.x, horizontalLimit.y));
            verticalField.SetValue(__instance, Mathf.Clamp(_vertical + deltaLookX, mouse_LOOK_VERTICAL_LIMIT.x, mouse_LOOK_VERTICAL_LIMIT.y));
            float x2 = (_vertical > 0f) ? (_vertical * (1f - _horizontal / horizontalLimit.y * (_horizontal / horizontalLimit.y))) : _vertical;
            if (_setResetedLookNextFrame)
            {
                isResettingLookField.SetValue(__instance, false);
                setResetedLookNextFrameField.SetValue(__instance, false);
            }
            if (_resetLook)
            {
                mouseLookControlField.SetValue(__instance, false);
                resetLookField.SetValue(__instance, false);
                isResettingLookField.SetValue(__instance, true);
                deltaLookY = 0f;
                deltaLookX = 0f;
            }
            if (Math.Abs(deltaLookY) >= 1E-45f && Math.Abs(deltaLookX) >= 1E-45f)
            {
                mouseLookControlField.SetValue(__instance, true);
            }
            if (!mouseLookControl && withReturn)
            {
                if (Mathf.Abs(_horizontal) > 0.01f)
                {
                    horizontalField.SetValue(__instance, Mathf.Lerp(_horizontal, 0f, Time.deltaTime * 15f));
                }
                else
                {
                    horizontalField.SetValue(__instance, 0f);
                }
                if (Mathf.Abs(_vertical) > 0.01f)
                {
                    verticalField.SetValue(__instance, Mathf.Lerp(_vertical, 0f, Time.deltaTime * 15f));
                }
                else
                {
                    verticalField.SetValue(__instance, 0f);
                }
            }
            if (!isResettingLook && _horizontal != 0f && _vertical != 0f)
            {
                isLookingField.SetValue(__instance, true);
            }
            else
            {
                isLookingField.SetValue(__instance, false);
            }
            if (_horizontal == 0f && _vertical == 0f)
            {
                setResetedLookNextFrameField.SetValue(__instance, true);
            }
            __instance.HeadRotation = new Vector3(x2, _horizontal, 0f);
            __instance.ProceduralWeaponAnimation.SetHeadRotation(__instance.HeadRotation);
            return false;
        }
    }

    //Everything related to camera to scope distance, smoothing, etc.
    public class LerpCameraPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        private static FieldInfo _fcField;

        private static float _yPos = 0f;
        private static float _xStanceCameraSpeedFactor = 1f;
        private static float _yStanceCameraSpeedFactor = 1f;
        private static float _zStanceCameraSpeedFactor = 1f;
        private static float _stanceTimer = 0f;
        private static float _collsionCameraSpeed = 1f;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("LerpCamera", BindingFlags.Instance | BindingFlags.Public);
        }

        //this is redundant if doing gun to camera ADS, but might be good for left shoulder and things like that
        private static void DoStanceSmoothing(bool isAltPistol)
        {
            if ((Plugin.RealCompat.StanceBlenderTarget <= 0f && Plugin.RealCompat.StanceBlenderValue > 0f) || Plugin.RealCompat.IsLeftShoulder || Plugin.RealCompat.IsResettingShoulder)
            {
                _xStanceCameraSpeedFactor = Mathf.MoveTowards(_xStanceCameraSpeedFactor, 0.7f, 1f);
                _yStanceCameraSpeedFactor = Mathf.MoveTowards(_xStanceCameraSpeedFactor, 0.75f, 1f); 
                _zStanceCameraSpeedFactor = Mathf.MoveTowards(_zStanceCameraSpeedFactor, 0.85f, 1f);
            }
            else
            {
                _stanceTimer += Time.deltaTime;
            }

            float timer = isAltPistol ? 0.15f : 0.85f;
            float yResetSpeed = isAltPistol ? 0.2f : 0.1f;
            if (_stanceTimer >= timer)
            {
                _xStanceCameraSpeedFactor = Mathf.MoveTowards(_xStanceCameraSpeedFactor, 1f, 0.05f);
                _yStanceCameraSpeedFactor = Mathf.MoveTowards(_yStanceCameraSpeedFactor, 1f, yResetSpeed);
                _zStanceCameraSpeedFactor = Mathf.MoveTowards(_zStanceCameraSpeedFactor, 1f, 0.15f);
                _stanceTimer = 0f;
            }
        }

        private static float SetBaseCamZOffset(ProceduralWeaponAnimation __instance, float camZ, bool treatAsPistol, bool isOptic) 
        {
            return 
                __instance.IsAiming && !isOptic && treatAsPistol ? camZ - Plugin.PistolOffset.Value :
                __instance.IsAiming && !isOptic ? camZ - Plugin.NonOpticOffset.Value :
                __instance.IsAiming && isOptic ? camZ - Plugin.OpticPosOffset.Value :
                camZ;
        }

        private static float GetLeftShoulderZoffset(bool shouldMoveGunToCamera, bool isPistol, bool isAltPistol, bool isDoingLeftShoulder) 
        {
            float offset = shouldMoveGunToCamera && isDoingLeftShoulder ? (isAltPistol ? Plugin.PistolLeftShoulderOffset.Value : Plugin.RifleLeftShoulderOffset.Value) : 0f;
            offset += isDoingLeftShoulder ? (isPistol ? Plugin.PistolLeftShoulderOffset.Value : Plugin.RifleLeftShoulderOffset.Value) : 0f;
            return offset;
        }

        [PatchPrefix]
        private static bool Prefix(EFT.Animations.ProceduralWeaponAnimation __instance, float dt, float ____overweightAimingMultiplier,
            float ____aimingSpeed, float ____aimSwayStrength, Player.ValueBlender
            ____aimSwayBlender, Vector3 ____aimSwayDirection, Vector3 ____headRotationVec,
            Vector3 ____vCameraTarget, Player.ValueBlenderDelay ____tacticalReload,
            Quaternion ____cameraIdenity, Quaternion ____rotationOffset, Vector2 ____cameraShiftToLineOfSight,
            float ____lineOfSightDeltaAngle, Vector3 ____shotDirection, bool ____adjustCollimatorsToTrajectory,
            Transform ____bone0, Transform ____bone1)
        {
            FirearmController firearmController = (FirearmController)_fcField.GetValue(__instance);
            if (firearmController == null) return true;
            Player player = (Player)_playerField.GetValue(firearmController);
            if (player != null && player.IsYourPlayer && firearmController.Weapon != null)
            {
                bool realismIsNull = Plugin.RealCompat == null || !Plugin.RealCompat.StancesAreEnabled || !Plugin.RealismIsPresent;
                bool smoothPatrolStanceADS = !realismIsNull && Plugin.RealCompat.DoPatrolStanceAdsSmoothing;
                bool isColliding = !realismIsNull && Plugin.RealCompat.StopCameraMovmentForCollision;
                bool isMachinePistol = (!realismIsNull && Plugin.RealCompat.IsMachinePistol);
                bool isPistol = isMachinePistol || Plugin.FovController.IsPistol;
                bool treatAsPistol = isPistol && (realismIsNull || (!realismIsNull && !Plugin.RealCompat.HasShoulderContact));
                bool isAltPistol = !realismIsNull && treatAsPistol && Plugin.RealCompat.DoAltPistol;
                bool isAltRifle = !realismIsNull && !treatAsPistol && Plugin.RealCompat.DoAltRifle;
                bool isOptic = __instance.CurrentScope.IsOptic;
                float collsionCameraSpeed = !realismIsNull ? Plugin.RealCompat.CameraMovmentForCollisionSpeed : 1f;
                bool isRealLeftShoulder = !realismIsNull && Plugin.RealCompat.IsLeftShoulder;
                bool isDoingLeftShoulder = isRealLeftShoulder || __instance.Boolean_0;  //boolean_0 detects if in BSG's left stance
                bool canMoveGunToCamera = !realismIsNull && (isAltPistol || isAltRifle); 
                float leftShoulderZOffset = GetLeftShoulderZoffset(canMoveGunToCamera, isPistol, isAltPistol, isDoingLeftShoulder);

                _collsionCameraSpeed = isColliding ? 0f : Mathf.Lerp(_collsionCameraSpeed, 1f, collsionCameraSpeed);
                if (!realismIsNull) DoStanceSmoothing(isAltPistol);

                float headBob = Singleton<SharedGameSettingsClass>.Instance.Game.Settings.HeadBobbing;
                Vector3 localPosition = __instance.HandsContainer.CameraTransform.localPosition;
                float localX = localPosition.x;
                float localY = localPosition.y;
                float localZ = localPosition.z;

                float camXOffset = treatAsPistol ? Plugin.PistolCameraXOffset.Value : Plugin.RifleCameraXOffset.Value; 
                float camYOffset = treatAsPistol ? Plugin.PistolCameraYOffset.Value : Plugin.RifleCameraYOffset.Value;
                float camZOffset = treatAsPistol ? Plugin.PistolCameraZOffset.Value : Plugin.RifleCameraZOffset.Value;

                float camX = !isDoingLeftShoulder && canMoveGunToCamera ? camXOffset : ____vCameraTarget.x;
                float camY = canMoveGunToCamera ? camYOffset : ____vCameraTarget.y;

                //should be an option to allow camera to move to Z target
                float camZ = canMoveGunToCamera ? 
                    SetBaseCamZOffset(__instance, camZOffset, treatAsPistol, isOptic) :  
                    SetBaseCamZOffset(__instance, ____vCameraTarget.z, treatAsPistol, isOptic);

                camZ = __instance.IsAiming ? camZ + leftShoulderZOffset : camZ;
                camZ = __instance.IsAiming && isMachinePistol ? camZ + (-0.1f) : camZ;
                camZ = __instance.IsAiming ? camZ + Plugin.FovController.ScrollCameraOffset : camZ;

                float rifleSpeed = smoothPatrolStanceADS ? 0.5f * Plugin.CameraAimSpeed.Value : Plugin.CameraAimSpeed.Value;
                float smoothTime = isOptic ? Plugin.OpticAimSpeed.Value * dt : treatAsPistol ? Plugin.PistolAimSpeed.Value * dt : rifleSpeed * dt;

                float xAimBaseMulti = treatAsPistol ? Plugin.PistolAimSpeedX.Value : Plugin.RifleAimSpeedX.Value;
                float yAimBaseMulti = treatAsPistol ? Plugin.PistolAimSpeedY.Value : Plugin.RifleAimSpeedY.Value;
                float zAimBaseMulti = treatAsPistol ? Plugin.PistolAimSpeedZ.Value : Plugin.RifleAimSpeedZ.Value;

                float aimFactorX = __instance.IsAiming ? (____aimingSpeed * __instance.CameraSmoothBlender.Value * ____overweightAimingMultiplier) * xAimBaseMulti : Plugin.UnAimSpeedX.Value;
                aimFactorX *= _xStanceCameraSpeedFactor;
                float aimFactorY = __instance.IsAiming ? (____aimingSpeed * __instance.CameraSmoothBlender.Value * ____overweightAimingMultiplier) * yAimBaseMulti : Plugin.UnAimSpeedY.Value;
                aimFactorY *= _yStanceCameraSpeedFactor;
                float aimFactorZ = __instance.IsAiming ? (1f + __instance.HandsContainer.HandsPosition.GetRelative().y * 100f + __instance.TurnAway.Position.y * 10f) * zAimBaseMulti : Plugin.UnAimSpeedZ.Value;
                aimFactorZ *= _zStanceCameraSpeedFactor;

                float targetX = Mathf.Lerp(localX, camX, smoothTime * aimFactorX * _collsionCameraSpeed);
                float targetY = Mathf.Lerp(localY, camY, smoothTime * aimFactorY * _collsionCameraSpeed);
                float targetZ = Mathf.Lerp(localZ, camZ, smoothTime * aimFactorZ * _collsionCameraSpeed);

                Vector3 newLocalPosition = new Vector3(targetX, targetY, targetZ) + __instance.HandsContainer.CameraPosition.GetRelative();

                if (____aimSwayStrength > 0f)
                {
                    float blendValue = ____aimSwayBlender.Value;
                    if (__instance.IsAiming && blendValue > 0f)
                    {
                        __instance.HandsContainer.SwaySpring.ApplyVelocity(____aimSwayDirection * blendValue);
                    }
                }

                _yPos = newLocalPosition.y;

                __instance.HandsContainer.CameraTransform.localPosition = new Vector3(newLocalPosition.x, _yPos, newLocalPosition.z);
                Quaternion animatedRotation = __instance.HandsContainer.CameraAnimatedFP.localRotation * __instance.HandsContainer.CameraAnimatedTP.localRotation;
                __instance.HandsContainer.CameraTransform.localRotation = Quaternion.Lerp(____cameraIdenity, animatedRotation, headBob * (1f - ____tacticalReload.Value)) * Quaternion.Euler(__instance.HandsContainer.CameraRotation.Get() + ____headRotationVec) * ____rotationOffset;
                __instance.method_19(dt);
                __instance.HandsContainer.CameraTransform.localEulerAngles += __instance.Shootingg.CurrentRecoilEffect.GetCameraRotationRecoil();

                //hud fov
                //this won't apply if doing the realism weapon to camera stuff, camera needs to be able to move to adjust to it
                __instance.HandsContainer.CameraOffset = new Vector3(camXOffset, camYOffset, camZOffset); //no idea if I made up 0.04 or not.

                return false;
            }
            return true;
        }
    }

    public class PwaWeaponParamsPatch : ModulePatch
    {
        private static FieldInfo playerField;
        private static FieldInfo fcField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("method_23", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref EFT.Animations.ProceduralWeaponAnimation __instance)
        {
            FirearmController firearmController = (FirearmController)fcField.GetValue(__instance);
            if (firearmController == null) return;
            Player player = (Player)playerField.GetValue(firearmController);
            if (player != null && player.IsYourPlayer)
            {
                //cloned weapon appears here

                Plugin.FovController.IsPistol = firearmController.Weapon.WeapClass == "pistol";
                Plugin.FovController.WeapId = firearmController.Weapon.Id;
                Plugin.FovController.CurrentWeapon = firearmController.Weapon;
                Plugin.FovController.ChangeMainCamFOV();
            }
        }
    }

    //changes "HUD FOV", or how the player model is rendered
    public class CalculateScaleValueByFovPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("CalculateScaleValueByFov");
        }

        public static void UpdateRibcageScale(float newScale)
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            
            if (player != null)
            {
                player.RibcageScaleCurrentTarget = newScale;
            }
        }

        public static void RestoreScale()
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            
            if (player != null)
            {
                player.CalculateScaleValueByFov(CameraClass.Instance.Fov);
                player.SetCompensationScale(true);
            }
        }

        [PatchPrefix]
        public static bool Prefix(Player __instance, ref float ____ribcageScaleCompensated)
        {
            float scale = Plugin.FovScale.Value;

            if (Plugin.EnableFovScaleFix.Value)
            {
                ____ribcageScaleCompensated = scale;
                UpdateRibcageScale(scale);
            
                return false;
            }

            return true;
        }
    }

    //BSG changed the 3rd person ADS animation. It's enabled for 1st person too for some reason, making ADS camera movement janky.
    public class SetPlayerAimingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "SetAim", new[] { typeof(bool) });
        }

        [PatchPostfix]
        public static void PatchPostfix(Player.FirearmController __instance, bool value)
        {
            if (__instance == null) return; // nre fix

            Player player = __instance.GetComponent<Player>();
            bool isYourPlayer = player.IsYourPlayer;
            ProceduralWeaponAnimation pwa = player.ProceduralWeaponAnimation;
            EPointOfView pov = pwa.PointOfView;

            if (isYourPlayer && pov == EPointOfView.FirstPerson)
            {
                player.MovementContext.PlayerAnimator.SetAiming(false);
            }
        }
    }

}
